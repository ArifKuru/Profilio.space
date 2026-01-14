using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CvBuilder.Models.Entity;
using CvBuilder.Repositories;

namespace CvBuilder.Controllers
{
    [Authorize] // Giriş yapmayan erişemesin
    public class EducationsController : Controller
    {
        EducationsRepository repo = new EducationsRepository();

        // GET: Educations
        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SADECE giriş yapan kullanıcının eğitim bilgilerini getir
            var educations = repo.List(x => x.user_id == userId);

            return View(educations);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(educations education)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            // Formdan user_id beklemiyoruz, Session'dan atıyoruz (Güvenlik)
            education.user_id = (int)Session["user_id"];

            repo.TAdd(education);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SİLME GÜVENLİĞİ:
            // Silinecek ID ile Session'daki UserID eşleşiyor mu?
            var education = repo.Find(x => x.id == id && x.user_id == userId);

            if (education != null)
            {
                repo.TDelete(education);
            }
            // education null ise başkasının verisidir, silinmez.

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // DÜZENLEME SAYFASI GÜVENLİĞİ:
            // Başkasının eğitim bilgisini düzenleme sayfasına giremesin
            var education = repo.Find(x => x.id == id && x.user_id == userId);

            if (education == null)
            {
                return RedirectToAction("Index");
            }

            return View(education);
        }

        [HttpPost]
        public ActionResult Update(educations education)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // GÜNCELLEME İŞLEMİ GÜVENLİĞİ:
            // Post edilen veri gerçekten bu kullanıcıya mı ait?
            educations e = repo.Find(x => x.id == education.id && x.user_id == userId);

            if (e != null)
            {
                e.school = education.school;
                e.type = education.type;
                e.description = education.description;
                e.gpa_score = education.gpa_score;
                e.start_date = education.start_date;
                e.end_date = education.end_date;

                // e.user_id = userId; // Gerek yok ama yazılabilir

                repo.TUpdate(e);
            }

            return RedirectToAction("Index");
        }
    }
}