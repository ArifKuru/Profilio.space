using CvBuilder.Models.Entity;
using CvBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    public class ExperiencesController : Controller
    {
        ExperiencesRepository repo = new ExperiencesRepository();

        // GET: Experiences
        public ActionResult Index()
        {
            // Session Kontrolü
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SADECE kullanıcının kendi deneyimlerini listele (Filtreleme)
            var experiences = repo.List(x => x.user_id == userId);

            return View(experiences);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(experiences experience)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            // Kullanıcı ID'sini Session'dan alıp modele gömüyoruz.
            experience.user_id = (int)Session["user_id"];

            repo.TAdd(experience);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // GÜVENLİK: Silinmek istenen ID, bu kullanıcıya mı ait?
            var experience = repo.Find(x => x.id == id && x.user_id == userId);

            if (experience != null)
            {
                repo.TDelete(experience);
            }
            // experience null ise (başkasının verisi), silme yapmadan listeye döner.

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // GÜVENLİK: Sadece kendi verisini düzenleme sayfasına gidebilir
            var experience = repo.Find(x => x.id == id && x.user_id == userId);

            if (experience == null)
            {
                return RedirectToAction("Index");
            }

            return View(experience);
        }

        [HttpPost]
        public ActionResult Update(experiences experience)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // GÜVENLİK: Güncellenecek veri gerçekten bu kullanıcıya mı ait?
            var e = repo.Find(x => x.id == experience.id && x.user_id == userId);

            if (e != null)
            {
                e.company = experience.company;
                e.title = experience.title;
                e.start_date = experience.start_date;
                e.end_date = experience.end_date;
                e.description = experience.description;
                // e.user_id = userId; // Değişmesine gerek yok ama güvenlik için

                repo.TUpdate(e);
            }

            return RedirectToAction("Index");
        }
    }
}