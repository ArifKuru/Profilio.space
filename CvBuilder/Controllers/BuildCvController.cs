using CvBuilder.Models;
using CvBuilder.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
// To fix CS0246, ensure the Rotativa NuGet package is installed in your project.
// In Visual Studio, use: Tools > NuGet Package Manager > Package Manager Console
// Then run:
// Install-Package Rotativa

// Or, if using .NET CLI:
// dotnet add package Rotativa

// After installing, the following using directive will work:
using Rotativa;

namespace CvBuilder.Controllers
{
    public class BuildCvController : Controller
    {
        arifkuru_cvEntities1 db = new arifkuru_cvEntities1();

        // GET: BuildCv
        public ActionResult Index()
        {
            // Session check
            if (Session["user_id"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int userId = (int)Session["user_id"];

            // Get all CV data
            var cvData = GetCvData(userId);

            return View(cvData);
        }

        // CV view page (for PDF) - Public access
        [AllowAnonymous]
        public ActionResult ViewCv(int? userId = null)
        {
            int actualUserId;
            
            // If userId is provided, use it; otherwise try to get from session
            if (userId.HasValue)
            {
                actualUserId = userId.Value;
            }
            else
            {
                // Try to get from session (for preview)
                if (Session["user_id"] == null)
                {
                    return HttpNotFound("User ID is required");
                }
                actualUserId = (int)Session["user_id"];
            }
            
            var cvData = GetCvData(actualUserId);
            return View(cvData);
        }

        // Download as PDF - Public access (for profile pages)
        [AllowAnonymous]
        public ActionResult DownloadPdf(int userId)
        {
            var user = db.users.Find(userId);
            if (user == null)
            {
                return HttpNotFound("User not found");
            }

            string fileName = $"{user.username}_CV.pdf";

            // Pass userId as parameter to ViewCv
            var pdfResult = new ActionAsPdf("ViewCv", new { userId = userId })
            {
                FileName = fileName,
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10),
                CustomSwitches = "--disable-smart-shrinking " +
                     "--viewport-size 1024x768 " +
                     "--print-media-type " +
                     "--no-stop-slow-scripts " +
                     "--enable-local-file-access"   
            };

            return pdfResult;
        }

        // Download as PDF - For authenticated users (admin panel)
        public ActionResult DownloadPdfAuth()
        {
            if (Session["user_id"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int userId = (int)Session["user_id"];
            return DownloadPdf(userId);
        }

        // Collect all CV data
        private CvViewModel GetCvData(int userId)
        {
            var user = db.users.Find(userId);
            var userInformation = db.user_informations.FirstOrDefault(x => x.user_id == userId);
            var experiences = db.experiences.Where(x => x.user_id == userId).OrderByDescending(x => x.start_date).ToList();
            var educations = db.educations.Where(x => x.user_id == userId).OrderByDescending(x => x.start_date).ToList();
            var workflows = db.workflows.Where(x => x.user_id == userId).ToList();
            var interests = db.interests.Where(x => x.user_id == userId).ToList();
            var achievements = db.achievements.Where(x => x.user_id == userId).ToList();
            var socialLinks = db.social_links.Where(x => x.user_id == userId).ToList();

            return new CvViewModel
            {
                User = user,
                UserInformation = userInformation ?? new user_informations(),
                Experiences = experiences,
                Educations = educations,
                Workflows = workflows,
                Interests = interests,
                Achievements = achievements,
                SocialLinks = socialLinks
            };
        }
    }
}