using CvBuilder.Models.Entity;
using CvBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    [Authorize] // 1. Adım: Giriş yapmayan kimse erişemesin
    public class AchievementsController : Controller
    {
        // Not: Eğer özel bir 'AchievementsRepository' oluşturmadıysan GenericRepository kullanmaya devam edebilirsin.
        GenericRepository<achievements> repo = new GenericRepository<achievements>();

        // GET: Achievements
        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // 2. Adım: Sadece oturum açan kişiye ait başarıları listele
            var achievements = repo.List(x => x.user_id == userId);

            return View(achievements);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(achievements achievement)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            // 3. Adım: Formdan user_id bekleme, Session'dan alıp ata (Güvenlik)
            achievement.user_id = (int)Session["user_id"];

            repo.TAdd(achievement);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // 4. Adım: Başkasının verisini silmeyi engelle (IDOR Koruması)
            var achievement = repo.Find(x => x.id == id && x.user_id == userId);

            if (achievement != null)
            {
                repo.TDelete(achievement);
            }
            // Kayıt yoksa veya kullanıcıya ait değilse işlem yapmadan döner.

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // 5. Adım: Başkasının verisini düzenleme ekranını açamasın
            var achievement = repo.Find(x => x.id == id && x.user_id == userId);

            if (achievement == null)
            {
                return RedirectToAction("Index");
            }

            return View(achievement);
        }

        [HttpPost]
        public ActionResult Update(achievements achievement)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // 6. Adım: Güncelleme isteği gerçekten verinin sahibinden mi geliyor?
            var a = repo.Find(x => x.id == achievement.id && x.user_id == userId);

            if (a != null)
            {
                a.description = achievement.description;
                // a.user_id = userId; // Zaten değişmediği için tekrar atamaya gerek yok

                repo.TUpdate(a);
            }

            return RedirectToAction("Index");
        }
    }
}