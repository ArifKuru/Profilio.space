using CvBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    [Authorize] // Giriş yapmayan mesajlarını göremez
    public class InboxController : Controller
    {
        // GET: Inbox
        ContactsRepository repo = new ContactsRepository();

        public ActionResult Index()
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SADECE giriş yapan kullanıcıya (user_id) gönderilen mesajları listele
            // Eğer bunu yapmazsan herkes herkesin mesajını okur!
            var contact_messages = repo.List(x => x.user_id == userId);

            return View(contact_messages);
        }

        public ActionResult Delete(int id)
        {
            if (Session["user_id"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["user_id"];

            // SİLME GÜVENLİĞİ:
            // Silinmek istenen mesaj gerçekten bana mı gelmiş?
            // Başkasına gelen mesajı ID deneyerek silememeliyim.
            var contact_message = repo.Find(x => x.id == id && x.user_id == userId);

            if (contact_message != null)
            {
                repo.TDelete(contact_message);
            }
            // Mesaj null ise (başkasına aitse) silmeden listeye dön.

            return RedirectToAction("Index");
        }
    }
}