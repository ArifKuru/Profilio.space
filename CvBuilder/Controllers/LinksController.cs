using CvBuilder.Models.Entity;
using CvBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    public class LinksController : Controller
    {
        LinkRepository repo = new LinkRepository();

        // GET: Links
        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SADECE giriş yapan kullanıcının linklerini getir
            var links = repo.List(x => x.user_id == userId);

            return View(links);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(social_links model)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            // Modeli kaydederken user_id'yi Session'dan alıp atıyoruz.
            // Böylece formda hidden input olarak tutmaya gerek kalmıyor (daha güvenli).
            model.user_id = (int)Session["user_id"];

            repo.TAdd(model);

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // GÜVENLİK KONTROLÜ:
            // Silinecek id'yi ararken user_id'si de eşleşiyor mu diye bakıyoruz.
            // Eğer eşleşmiyorsa (başkasının verisiyse) 'link' null gelecektir.
            var link = repo.Find(x => x.id == id && x.user_id == userId);

            if (link != null)
            {
                repo.TDelete(link);
            }
            // Link null ise (başkasının linkini silmeye çalışıyorsa) hiçbir şey yapmadan listeye dön.

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // Başkasının linkini düzenleme sayfasına giremesin
            var link = repo.Find(x => x.id == id && x.user_id == userId);

            if (link == null)
            {
                return RedirectToAction("Index");
            }

            return View(link);
        }

        [HttpPost]
        public ActionResult Update(social_links model)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // GÜVENLİK KONTROLÜ (Kritik):
            // Formdan gelen ID'yi direkt güncellemek yerine, önce DB'den bu ID ve USER_ID eşleşiyor mu diye kontrol et.
            var l = repo.Find(x => x.id == model.id && x.user_id == userId);

            if (l != null)
            {
                l.link = model.link;
                l.type = model.type;
                // l.user_id = userId; // Zaten kendi verisi, değiştirmeye gerek yok ama güvenli kalır.

                repo.TUpdate(l);
            }

            return RedirectToAction("Index");
        }
    }
}