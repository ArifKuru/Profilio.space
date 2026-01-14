using CvBuilder.Models.Entity;
using CvBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    // Giriş yapmayanlar bu Controller'a erişemesin
    public class AboutController : Controller
    {
        UserInformationsRepository repo = new UserInformationsRepository();

        // GET: About
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["user_id"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = (int)Session["user_id"];
            var user_information = repo.Find(x => x.user_id == userId);

            if (user_information == null)
            {
                user_information = new user_informations();
            }

            return View(user_information);
        }

        [HttpPost]
        public ActionResult Update(user_informations p, HttpPostedFileBase profileImageFile, bool isImageRemoved = false)
        {
            // Session kontrolü
            if (Session["user_id"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = (int)Session["user_id"];

            // Mevcut kullanıcıyı bul
            var u = repo.Find(x => x.user_id == userId);

            if (u != null)
            {
                // Standart alanlar
                u.name = p.name;
                u.surname = p.surname;
                u.address = p.address;
                u.phone = p.phone;
                u.description = p.description;

                // --- YENİ EKLENEN KISIM BAŞLANGIÇ ---
                // Formdan gelen 'character_type' verisini veritabanı nesnesine aktarıyoruz.
                // Eğer kullanıcı seçim yapmadıysa null gelebilir, sorun yok (DB nullable).
                u.character_type = p.character_type;
                // --- YENİ EKLENEN KISIM BİTİŞ ---

                // Resim İşlemleri
                if (profileImageFile != null && profileImageFile.ContentLength > 0)
                {
                    var uploads = Server.MapPath("~/Content/images/profiles");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(profileImageFile.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);

                    profileImageFile.SaveAs(filePath);
                    u.profile_image = "/Content/images/profiles/" + fileName;
                }
                else if (isImageRemoved)
                {
                    u.profile_image = "/Content/images/profiles/default.png";
                }

                repo.TUpdate(u);
            }

            return RedirectToAction("Index");
        }
    }
}