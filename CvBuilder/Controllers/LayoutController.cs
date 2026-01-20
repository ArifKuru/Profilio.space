using CvBuilder.Models; // ViewModel'in olduğu namespace
using CvBuilder.Models.Entity; // Entity'lerin olduğu namespace
using System.Linq;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    public class LayoutController : Controller
    {
        // Bu Action, direkt sayfa olarak açılmaz, Layout içinde çağrılır
        public PartialViewResult HeaderUserProfile()
        {
            // Eğer giriş yapılmamışsa boş model döndür (Hata vermesin)
            if (Session["user_id"] == null)
            {
                return PartialView("_HeaderUserProfile", new LayoutProfileViewModel
                {
                    FullName = "Misafir",
                    ProfileImage = "/Content/images/profiles/default.png",
                    Email = ""
                });
            }

            int id = (int)Session["user_id"];

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                // Kullanıcı tablosundan ve bilgi tablosundan verileri çek
                var user = db.users.Find(id);
                var userInfo = db.user_informations.FirstOrDefault(x => x.user_id == id);

                LayoutProfileViewModel model = new LayoutProfileViewModel();

                // Email users tablosunda
                model.Email = user.email;
                model.IsPublished = user.isPublished;

                if (userInfo != null)
                {
                    model.FullName = userInfo.name + " " + userInfo.surname;
                    model.ProfileImage = userInfo.profile_image;
                }
                else
                {
                    model.FullName = user.username; // Detay yoksa kullanıcı adını göster
                    model.ProfileImage = "/Content/images/profiles/default.png";
                }

                return PartialView("_HeaderUserProfile", model);
            }
        }

        [HttpPost]
        public JsonResult TogglePublishStatus()
        {
            // Session kontrolü
            if (Session["user_id"] == null)
            {
                return Json(new { success = false, message = "Session not found" });
            }

            int userId = (int)Session["user_id"];

            try
            {
                using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
                {
                    var user = db.users.Find(userId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" });
                    }

                    // Toggle işlemi
                    user.isPublished = !user.isPublished;
                    db.SaveChanges();

                    return Json(new { success = true, isPublished = user.isPublished });
                }
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}