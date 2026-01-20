using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CvBuilder.Models.Entity;

namespace CvBuilder.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        arifkuru_cvEntities1 db = new arifkuru_cvEntities1();

        // 1. Index artık "username" alıyor
        public ActionResult Index(string username)
        {
            // Eğer username boş gelirse (örn: site.com/) varsayılan bir kullanıcıya ya da hataya yönlendir
            if (string.IsNullOrEmpty(username))
            {
                // İstersen burada kendi kullanıcı adını default yapabilirsin
                return RedirectToAction("Index", "Register");
            }

            // Users tablosundan kullanıcı adını bul (Tablo adın "users" veya "tbl_users" olabilir, kontrol et)
            // Örnek: var user = db.users.FirstOrDefault(x => x.username == username);

            // Simülasyon: Kullanıcı tablosunu bilmediğim için user_informations içinde username var mı diye bakıyorum. 
            // Eğer ayrı bir Users tablon varsa oradan sorgula.
            // ÖRNEK SENARYO:
            // Sadece isPublished=1 olan kullanıcıları ara
            var foundUser = db.users.FirstOrDefault(x => x.username == username && x.isPublished == true);

            if (foundUser == null)
            {
                ViewBag.Username = username;
                Response.StatusCode = 404;
                return View("NotFound");
            }

            // ID'yi bulduk
            int targetUserId = foundUser.id;
            string targetUserEmail = foundUser.email; // <--- YENİ EKLENDİ

            // Ziyaret kaydı oluştur (profile_visits tablosuna)
            try
            {
                var visit = new profile_visits
                {
                    user_id = targetUserId,
                    visited_at = DateTime.Now,
                    ip_address = Request.UserHostAddress,
                    user_agent = Request.UserAgent,
                    referrer = Request.UrlReferrer?.ToString()
                };
                db.profile_visits.Add(visit);
                db.SaveChanges();
            }
            catch
            {
                // Tablo henüz oluşturulmamışsa sessizce devam et
            }

            // View'a gönderilecek veriyi çek
            var user_informations = db.user_informations.Where(x => x.user_id == targetUserId).ToList();

            // ÖNEMLİ: Partial View'ların bu ID'yi bilmesi için ViewBag'e atıyoruz
            ViewBag.TargetUserId = targetUserId;
            ViewBag.TargetUserEmail = targetUserEmail; // <--- ARTIK VIEW'DA KULLANILABİLİR
            return View(user_informations);
        }

        // 2. Tüm Partial Metotlar artık userId parametresi almalı
        public PartialViewResult Experiences(int id)
        {
            var experiences = db.experiences.Where(x => x.user_id == id).ToList();
            return PartialView(experiences);
        }

        public PartialViewResult Educations(int id)
        {
            var educations = db.educations.Where(x => x.user_id == id).ToList();
            return PartialView(educations);
        }

        public PartialViewResult Workflows(int id)
        {
            var workflows = db.workflows.Where(x => x.user_id == id).ToList();
            return PartialView(workflows);
        }

        public PartialViewResult Interests(int id)
        {
            var interests = db.interests.Where(x => x.user_id == id).ToList();
            return PartialView(interests);
        }

        public PartialViewResult Social_links(int id)
        {
            var social_links = db.social_links.Where(x => x.user_id == id).ToList();
            return PartialView(social_links);
        }

        public PartialViewResult Achievements(int id)
        {
            var achievements = db.achievements.Where(x => x.user_id == id).ToList();
            return PartialView(achievements);
        }

        [HttpGet]
        public PartialViewResult Contact(int id, string email)
        {
            // İletişim formunda kime mesaj atılacağını bilmek için ID'yi modele veya viewbag'e koymalısın
            ViewBag.TargetUserId = id;
            ViewBag.TargetUserEmail = email; // <-- Artık elimizde var
            return PartialView();
        }
        [HttpPost]
        public JsonResult Contact(contact_messages form_data)
        {
            try
            {
                // NOT: View tarafında <input type="hidden" name="user_id" ... /> olduğundan emin olun.

                form_data.created_at = DateTime.Now;
                db.contact_messages.Add(form_data);
                db.SaveChanges();

                // Başarılı yanıt (JSON)
                return Json(new { success = true, message = "Message sent successfully!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Hata yanıtı (JSON)
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}