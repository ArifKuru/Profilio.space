using CvBuilder.Models.Entity;
using CvBuilder.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography; // Şifreleme kütüphanesi
using System.Text; // Text işlemleri
using System.Web.Mvc;
using System.Web.Security;

namespace CvBuilder.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(users user)
        {
            // using bloğu bellek yönetimi için önemlidir
            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                // 1. Adım: Önce sadece E-Mail ile kullanıcıyı bul
                var userInfo = db.users.FirstOrDefault(x => x.email == user.email);

                // Kullanıcı yoksa hata ver
                if (userInfo == null)
                {
                    ViewBag.Error = "User not found";
                    return View(user);
                }

                // 2. Adım: Şifre HASH Kontrolü
                // View'dan gelen 'user.password_hash' içinde aslında kullanıcının girdiği DÜZ şifre var (örn: "123456").
                // Biz bunu alıp şifreliyoruz:
                string incomingPasswordHashed = SHA256Hash(user.password_hash);

                // Şimdi veritabanındaki kayıtlı hash ile bizim hesapladığımız hash aynı mı bakıyoruz
                if (userInfo.password_hash != incomingPasswordHashed)
                {
                    ViewBag.Error = "Wrong password";
                    return View(user);
                }

                // 3. Adım: Giriş Başarılı
                FormsAuthentication.SetAuthCookie(userInfo.username, false);
                Session["user_id"] = userInfo.id;
                Session["username"] = userInfo.username;

                // Veritabanındaki user_informations tablosunda resmi var mı diye bakıp session'a atabiliriz (Opsiyonel)
                // Şimdilik About sayfasına yönlendiriyoruz
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); // Session'ı tamamen öldür
            return RedirectToAction("Index", "Login");
        }

        // --- ŞİFRE SIFIRLAMA İŞLEMLERİ ---

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Email address is required.";
                return View();
            }

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                var user = db.users.FirstOrDefault(x => x.email == email);
                
                // Güvenlik: Kullanıcı var mı yok mu bilgisini vermemek için her zaman başarı mesajı göster
                if (user != null)
                {
                    // Token oluştur
                    string token = GenerateResetToken();
                    DateTime expiry = DateTime.Now.AddHours(24); // 24 saat geçerli

                    // Token'ı veritabanına kaydet
                    user.password_reset_token = token;
                    user.password_reset_expiry = expiry;
                    db.SaveChanges();

                    // Email gönder
                    try
                    {
                        string resetLink = Url.Action("ResetPassword", "Login", new { token = token }, Request.Url.Scheme);
                        string emailBody = GetPasswordResetEmailHtml(user.username, resetLink);
                        
                        SmtpHelper.SendEmail(user.email, "Password Reset - Profilio.space", emailBody, true);
                    }
                    catch (Exception ex)
                    {
                        // Email gönderilemezse token'ı temizle
                        user.password_reset_token = null;
                        user.password_reset_expiry = null;
                        db.SaveChanges();
                        
                        ViewBag.Error = "An error occurred while sending the email. Please try again later.";
                        return View();
                    }
                }

                // Güvenlik: Her durumda aynı mesajı göster
                ViewBag.Success = "If this email address is registered in the system, a password reset link has been sent to your email address.";
                return View();
            }
        }

        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid or missing token.";
                return View();
            }

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                var user = db.users.FirstOrDefault(x => x.password_reset_token == token && 
                                                         x.password_reset_expiry != null && 
                                                         x.password_reset_expiry > DateTime.Now);

                if (user == null)
                {
                    ViewBag.Error = "Token is invalid or has expired. Please create a new password reset request.";
                    return View();
                }

                ViewBag.Token = token;
                return View();
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Error = "All fields are required.";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long.";
                ViewBag.Token = token;
                return View();
            }

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                var user = db.users.FirstOrDefault(x => x.password_reset_token == token && 
                                                         x.password_reset_expiry != null && 
                                                         x.password_reset_expiry > DateTime.Now);

                if (user == null)
                {
                    ViewBag.Error = "Token is invalid or has expired. Please create a new password reset request.";
                    return View();
                }

                // Yeni şifreyi hash'le ve kaydet
                user.password_hash = SHA256Hash(newPassword);
                user.password_reset_token = null;
                user.password_reset_expiry = null;
                db.SaveChanges();

                ViewBag.Success = "Your password has been successfully reset. You can now sign in.";
                return View();
            }
        }

        // --- YARDIMCI METODLAR ---

        // Token oluşturma metodu
        private string GenerateResetToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }

        // Email HTML template
        private string GetPasswordResetEmailHtml(string username, string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title>Password Reset - Profilio.space</title>
    <!--[if mso]>
    <style type='text/css'>
        body, table, td {{font-family: Arial, sans-serif !important;}}
    </style>
    <![endif]-->
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #334155;
            background-color: #f1f5f9;
            padding: 0;
            margin: 0;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
        }}
        .email-wrapper {{
            width: 100%;
            background-color: #f1f5f9;
            padding: 40px 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
        }}
        .header {{
            background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
            padding: 40px 30px;
            text-align: center;
        }}
        .header-logo {{
            color: #ffffff;
            font-size: 28px;
            font-weight: 700;
            letter-spacing: -0.5px;
            margin: 0;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }}
        .header-subtitle {{
            color: #cbd5e1;
            font-size: 14px;
            margin-top: 8px;
            font-weight: 400;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .greeting {{
            font-size: 24px;
            font-weight: 600;
            color: #0f172a;
            margin-bottom: 16px;
            line-height: 1.3;
        }}
        .message {{
            font-size: 15px;
            color: #475569;
            line-height: 1.7;
            margin-bottom: 24px;
        }}
        .button-wrapper {{
            text-align: center;
            margin: 32px 0;
        }}
        .button {{
            display: inline-block;
            padding: 14px 36px;
            background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 15px;
            letter-spacing: 0.3px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
            transition: all 0.3s ease;
        }}
        .button:hover {{
            background: linear-gradient(135deg, #1e293b 0%, #334155 100%);
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
            transform: translateY(-1px);
        }}
        .link-fallback {{
            margin-top: 24px;
            padding: 16px;
            background-color: #f8fafc;
            border-radius: 8px;
            border-left: 3px solid #16A34A;
        }}
        .link-fallback-text {{
            font-size: 12px;
            color: #64748b;
            margin-bottom: 8px;
            font-weight: 500;
        }}
        .link-fallback-url {{
            word-break: break-all;
            color: #16A34A;
            font-size: 12px;
            font-family: 'Courier New', monospace;
            line-height: 1.6;
        }}
        .warning-box {{
            margin-top: 32px;
            padding: 16px 20px;
            background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
            border-left: 4px solid #f59e0b;
            border-radius: 8px;
            box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
        }}
        .warning-icon {{
            display: inline-block;
            font-size: 18px;
            margin-right: 8px;
            vertical-align: middle;
        }}
        .warning-text {{
            color: #92400e;
            font-size: 13px;
            line-height: 1.6;
            margin: 0;
            font-weight: 500;
        }}
        .warning-text strong {{
            font-weight: 700;
        }}
        .divider {{
            height: 1px;
            background: linear-gradient(to right, transparent, #e2e8f0, transparent);
            margin: 32px 0;
        }}
        .footer {{
            background-color: #f8fafc;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e2e8f0;
        }}
        .footer-text {{
            color: #64748b;
            font-size: 12px;
            line-height: 1.6;
            margin: 4px 0;
        }}
        .footer-text.copyright {{
            color: #94a3b8;
            font-size: 11px;
            margin-top: 12px;
        }}
        .icon {{
            display: inline-block;
            width: 20px;
            height: 20px;
            vertical-align: middle;
            margin-right: 6px;
        }}
        @media only screen and (max-width: 600px) {{
            .email-wrapper {{
                padding: 20px 10px;
            }}
            .header {{
                padding: 30px 20px;
            }}
            .content {{
                padding: 30px 20px;
            }}
            .footer {{
                padding: 24px 20px;
            }}
            .greeting {{
                font-size: 22px;
            }}
            .button {{
                padding: 12px 28px;
                font-size: 14px;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='email-container'>
            <!-- Header -->
            <div class='header'>
                <h1 class='header-logo'>Profilio.space</h1>
                <p class='header-subtitle'>Professional Portfolio Platform</p>
            </div>
            
            <!-- Content -->
            <div class='content'>
                <h2 class='greeting'>Hello {username},</h2>
                
                <p class='message'>
                    We received a request to reset your password for your Profilio.space account. 
                    If you made this request, please click the button below to create a new password.
                </p>
                
                <div class='button-wrapper'>
                    <a href='{resetLink}' class='button' style='color: #ffffff; text-decoration: none;'>
                        🔐 Reset My Password
                    </a>
                </div>
                
                <div class='link-fallback'>
                    <p class='link-fallback-text'>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p class='link-fallback-url'>{resetLink}</p>
                </div>
                
                <div class='warning-box'>
                    <p class='warning-text'>
                        <span class='warning-icon'>⚠️</span>
                        <strong>Security Notice:</strong> This password reset link will expire in 24 hours. 
                        If you didn't request a password reset, please ignore this email or contact our support team 
                        if you have concerns about your account security.
                    </p>
                </div>
                
                <div class='divider'></div>
                
                <p class='message' style='margin-bottom: 0; font-size: 14px; color: #64748b;'>
                    For security reasons, we recommend choosing a strong, unique password that you haven't used elsewhere.
                </p>
            </div>
            
            <!-- Footer -->
            <div class='footer'>
                <p class='footer-text'>
                    <strong>Need help?</strong> Contact our support team or visit our help center.
                </p>
                <p class='footer-text copyright'>
                    © {DateTime.Now.Year} Profilio.space. All rights reserved.<br>
                    This is an automated message, please do not reply to this email.
                </p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        // --- YARDIMCI METOD: SHA256 HASH ---
        // RegisterController'daki metodun aynısıdır.
        // İleride bunu "GeneralHelper" gibi ortak bir class'a taşıyıp her yerden çağırabilirsin.
        public static string SHA256Hash(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}