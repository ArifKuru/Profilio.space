using CvBuilder.Models.Entity;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // GET: Profile
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int id = (int)Session["user_id"];

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                // 1. Kullanıcı giriş bilgilerini çek (users tablosu)
                var user = db.users.Find(id);

                // 2. Kullanıcı detay bilgilerini çek (user_informations tablosu)
                var userInfo = db.user_informations.FirstOrDefault(x => x.user_id == id);

                // 3. Bilgileri ViewBag'e at (View'da kullanmak için)
                if (userInfo != null)
                {
                    ViewBag.FullName = userInfo.name + " " + userInfo.surname;
                    ViewBag.ProfileImage = userInfo.profile_image; // Veritabanındaki resim yolu
                    ViewBag.character_type = userInfo.character_type;
                    // Session'ı da güncelleyelim ki Layout'taki resim de değişsin
                    Session["FullName"] = userInfo.name + " " + userInfo.surname;
                    Session["ProfileImage"] = userInfo.profile_image;
                }
                else
                {
                    // Eğer detay yoksa varsayılan değerler
                    ViewBag.FullName = "User";
                    ViewBag.ProfileImage = "/Content/images/profiles/default.png";
                }

                return View(user);
            }
        }

        // Şifre Değiştirme İşlemi
        [HttpPost]
        public ActionResult ChangePassword(string current_password, string new_password, string confirm_password)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int id = (int)Session["user_id"];

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                var user = db.users.Find(id);

                // --- SAYFA YENİLENDİĞİNDE RESİM GİTMESİN DİYE TEKRAR ÇEKİYORUZ ---
                var userInfo = db.user_informations.FirstOrDefault(x => x.user_id == id);
                if (userInfo != null)
                {
                    ViewBag.FullName = userInfo.name + " " + userInfo.surname;
                    ViewBag.ProfileImage = userInfo.profile_image;
                }
                else
                {
                    ViewBag.FullName = "User";
                    ViewBag.ProfileImage = "/Content/images/profiles/default.png";
                }
                // ---------------------------------------------------------------

                // 1. Yeni şifreler eşleşiyor mu?
                if (new_password != confirm_password)
                {
                    ViewBag.Error = "New passwords do not match!";
                    return View("Index", user);
                }

                // 2. Eski şifre doğru mu?
                string currentHash = SHA256Hash(current_password);
                if (user.password_hash != currentHash)
                {
                    ViewBag.Error = "Current password is wrong!";
                    return View("Index", user);
                }

                // 3. Her şey tamamsa kaydet
                user.password_hash = SHA256Hash(new_password);
                db.SaveChanges();

                ViewBag.Success = "Password changed successfully!";
                return View("Index", user);
            }
        }

        // Hash Yardımcı Metodu
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