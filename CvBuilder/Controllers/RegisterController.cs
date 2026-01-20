using CvBuilder.Models.Entity;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

namespace CvBuilder.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(users p, string name, string surname, string plain_password)
        {
            // --- 1. REZERVE KELİME KONTROLÜ (YENİ EKLENDİ) ---
            // Bu kullanıcı adları sistem tarafından kullanıldığı için alınamaz.
            string[] reservedNames = { "admin", "home", "login", "register" ,"profile","gdpr","privacy","terms","disclaimer"};

            // Kullanıcı adı boş değilse ve listede varsa (küçük harfe çevirip kontrol et)
            if (!string.IsNullOrEmpty(p.username) && reservedNames.Contains(p.username.ToLower()))
            {
                ViewBag.Error = "This username is reserved and cannot be used.";
                return View(p);
            }

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                // 2. KONTROL: Bu email veya kullanıcı adı VERİTABANINDA var mı?
                var checkUser = db.users.FirstOrDefault(x => x.email == p.email || x.username == p.username);

                if (checkUser != null)
                {
                    ViewBag.Error = "Email or Username already exists!";
                    return View(p);
                }

                // 3. ADIM: Users Tablosuna Kayıt
                p.password_hash = SHA256Hash(plain_password);

                db.users.Add(p);
                db.SaveChanges();

                // 4. ADIM: UserInformations Tablosuna Kayıt
                user_informations info = new user_informations();
                info.user_id = p.id;
                info.name = name;
                info.surname = surname;
                info.profile_image = "/Content/images/profiles/default.png";
                info.address = "Address not entered";

                db.user_informations.Add(info);
                db.SaveChanges();

                // 5. ADIM: Otomatik Giriş Yap ve Yönlendir
                FormsAuthentication.SetAuthCookie(p.username, false);
                Session["user_id"] = p.id;
                Session["username"] = p.username;

                return RedirectToAction("Index", "Dashboard");
            }
        }

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