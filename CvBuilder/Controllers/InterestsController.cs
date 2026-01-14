using CvBuilder.Models.Entity;
using CvBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    [Authorize] // Giriş yapmayan erişemesin
    public class InterestsController : Controller
    {
        InterestsRepository repo = new InterestsRepository();

        // GET: Interests
        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SADECE giriş yapan kullanıcının hobilerini getir
            var interests = repo.List(x => x.user_id == userId);

            return View(interests);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(interests interest)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            // Kullanıcıdan user_id bekleme, Session'dan alıp ata
            interest.user_id = (int)Session["user_id"];

            repo.TAdd(interest);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SİLME GÜVENLİĞİ:
            // Silinecek ID bu kullanıcıya mı ait?
            var interest = repo.Find(x => x.id == id && x.user_id == userId);

            if (interest != null)
            {
                repo.TDelete(interest);
            }
            // Null gelirse başkasının verisidir, silinmez.

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // DÜZENLEME GÜVENLİĞİ:
            // Sadece kendi verisini düzenleme sayfasına girebilir
            var interest = repo.Find(x => x.id == id && x.user_id == userId);

            if (interest == null)
            {
                return RedirectToAction("Index");
            }

            return View(interest);
        }

        [HttpPost]
        public ActionResult Update(interests interest)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // GÜNCELLEME GÜVENLİĞİ:
            // Post edilen veri gerçekten bu kullanıcıya mı ait?
            interests a = repo.Find(x => x.id == interest.id && x.user_id == userId);

            if (a != null)
            {
                a.description = interest.description;
                // a.user_id = userId; // Gerek yok, zaten değişmedi.

                repo.TUpdate(a);
            }

            return RedirectToAction("Index");
        }
    }
}