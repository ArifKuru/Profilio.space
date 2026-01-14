using CvBuilder.Models.Entity;
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
                return RedirectToAction("Index", "About");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); // Session'ı tamamen öldür
            return RedirectToAction("Index", "Login");
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