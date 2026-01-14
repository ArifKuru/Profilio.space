using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CvBuilder.Models.Entity;
using CvBuilder.Repositories;

namespace CvBuilder.Controllers
{
    public class WorkflowsController : Controller
    {
        WorkflowsRepository repo = new WorkflowsRepository();

        // GET: Workflows
        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SADECE giriş yapan kullanıcının verilerini getir
            var workflows = repo.List(x => x.user_id == userId);

            return View(workflows);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(workflows workflow)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            // Formdan user_id beklemiyoruz, Session'dan atıyoruz
            workflow.user_id = (int)Session["user_id"];

            repo.TAdd(workflow);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SİLME GÜVENLİĞİ: 
            // Silinecek ID bu kullanıcıya mı ait kontrolü (x.user_id == userId)
            workflows workflow = repo.Find(x => x.id == id && x.user_id == userId);

            if (workflow != null)
            {
                repo.TDelete(workflow);
            }
            // workflow null ise başkasının verisidir, işlem yapmadan döner.

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // DÜZENLEME GÜVENLİĞİ:
            // Sadece kendi verisini düzenleme sayfasına girebilir
            workflows w = repo.Find(x => x.id == id && x.user_id == userId);

            if (w == null)
            {
                return RedirectToAction("Index");
            }

            return View(w);
        }

        [HttpPost]
        public ActionResult Update(workflows workflow)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // POST GÜVENLİĞİ:
            // Güncellenen veri gerçekten bu kullanıcıya mı ait?
            workflows w = repo.Find(x => x.id == workflow.id && x.user_id == userId);

            if (w != null)
            {
                w.description = workflow.description;
                // w.user_id = userId; // Gerek yok, zaten değişmedi.

                repo.TUpdate(w);
            }

            return RedirectToAction("Index");
        }
    }
}