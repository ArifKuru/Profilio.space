using CvBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace CvBuilder.Controllers
{
    [Authorize] // Giriş yapmayan mesajlarını göremez
    public class InboxController : Controller
    {
        ContactsRepository repo = new ContactsRepository();

        // GET: Inbox - View sayfası
        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");
            return View();
        }

        // API: Mesajları sayfalama ile getir
        [HttpGet]
        public JsonResult GetMessages(int page = 1, int limit = 10)
        {
            if (Session["user_id"] == null)
            {
                return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            int userId = (int)Session["user_id"];

            try
            {
                // Mesajları çek, sırala ve sayfalama yap (tek seferde)
                var allMessages = repo.List(x => x.user_id == userId);
                
                // Toplam sayıyı hesapla
                int totalCount = allMessages.Count;
                int totalPages = (int)Math.Ceiling((double)totalCount / limit);

                // Sıralama ve sayfalama (tek seferde)
                var messages = allMessages
                    .OrderByDescending(x => x.created_at)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(x => new
                    {
                        id = x.id,
                        full_name = x.full_name,
                        email = x.email,
                        message = x.message,
                        created_at = x.created_at.HasValue ? new
                        {
                            date = x.created_at.Value.ToString("dd MMM yyyy", new CultureInfo("en-US")),
                            time = x.created_at.Value.ToString("HH:mm"),
                            full = x.created_at.Value.ToString("dd MMM yyyy HH:mm", new CultureInfo("en-US"))
                        } : null
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = messages,
                    pagination = new
                    {
                        currentPage = page,
                        totalPages = totalPages,
                        totalCount = totalCount,
                        limit = limit,
                        hasNextPage = page < totalPages,
                        hasPrevPage = page > 1
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // API: Mesaj silme
        [HttpPost]
        public JsonResult DeleteMessage()
        {
            if (Session["user_id"] == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            int userId = (int)Session["user_id"];
            int id = 0;

            // Request'ten id'yi al (form data veya JSON)
            if (Request.Form["id"] != null)
            {
                int.TryParse(Request.Form["id"], out id);
            }
            else if (Request["id"] != null)
            {
                int.TryParse(Request["id"].ToString(), out id);
            }

            if (id == 0)
            {
                return Json(new { success = false, message = "Invalid message ID" });
            }

            try
            {
                // GÜVENLİK KONTROLÜ: Mesaj gerçekten bana mı gelmiş?
                var contact_message = repo.Find(x => x.id == id && x.user_id == userId);

                if (contact_message != null)
                {
                    repo.TDelete(contact_message);
                    return Json(new { success = true, message = "Message deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Message not found or unauthorized" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Eski Delete action (redirect için - geriye uyumluluk)
        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");
            return RedirectToAction("Index");
        }
    }
}