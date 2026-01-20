using CvBuilder.Models.Entity;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        arifkuru_cvEntities1 db = new arifkuru_cvEntities1();

        // GET: Dashboard
        public ActionResult Index()
        {
            // Session check
            if (Session["user_id"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int userId = (int)Session["user_id"];

            try
            {
                // Default values
                var totalVisits = 0;
                var todayVisits = 0;
                var thisWeekVisits = 0;
                var thisMonthVisits = 0;
                var last7DaysVisits = Enumerable.Range(0, 7)
                    .Select(i => new
                    {
                        Date = DateTime.Today.AddDays(-i),
                        Count = 0
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                var visitsByHour = Enumerable.Range(0, 24)
                    .Select(hour => new
                    {
                        Hour = hour,
                        Count = 0
                    })
                    .ToList();

                // Fetch data if table exists (safe access with try-catch)
                try
                {
                    // Try to access profile_visits table
                    totalVisits = db.profile_visits.Count(x => x.user_id == userId);
                    
                    var todayStart = DateTime.Today;
                    var todayEnd = todayStart.AddDays(1);
                    todayVisits = db.profile_visits.Count(x => x.user_id == userId && 
                        x.visited_at >= todayStart && x.visited_at < todayEnd);
                    
                    // This Week: Haftanın başlangıcından (Pazartesi) bugüne kadar, bugün dahil
                    var now = DateTime.Now;
                    var daysSinceMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    var weekStart = now.Date.AddDays(-daysSinceMonday);
                    thisWeekVisits = db.profile_visits.Count(x => x.user_id == userId && 
                        x.visited_at >= weekStart && x.visited_at <= now);
                    
                    // This Month: Ayın başlangıcından bugüne kadar, bugün dahil
                    var monthStart = new DateTime(now.Year, now.Month, 1);
                    thisMonthVisits = db.profile_visits.Count(x => x.user_id == userId && 
                        x.visited_at >= monthStart && x.visited_at <= now);

                    // Last 7 days daily visit counts (for chart)
                    last7DaysVisits = Enumerable.Range(0, 7)
                        .Select(i =>
                        {
                            var targetDate = DateTime.Today.AddDays(-i);
                            var startDate = targetDate.Date;
                            var endDate = startDate.AddDays(1);
                            
                            return new
                            {
                                Date = targetDate,
                                Count = db.profile_visits.Count(x => x.user_id == userId && 
                                    x.visited_at >= startDate && x.visited_at < endDate)
                            };
                        })
                        .OrderBy(x => x.Date)
                        .ToList();

                    // Most visited hours (0-23)
                    visitsByHour = Enumerable.Range(0, 24)
                        .Select(hour => new
                        {
                            Hour = hour,
                            Count = db.profile_visits.Count(x => x.user_id == userId && 
                                x.visited_at.Hour == hour)
                        })
                        .ToList();
                }
                catch (System.Data.Entity.Core.EntityException ex)
                {
                    // Use default values if table doesn't exist or database error occurs
                    System.Diagnostics.Debug.WriteLine("EntityException: " + ex.Message);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    // Use default values if SQL error occurs (like table doesn't exist)
                    System.Diagnostics.Debug.WriteLine("SqlException: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Use default values for other errors
                    System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                }

                // Get CV summary data (always fetch, not dependent on profile_visits table)
                var experiences = db.experiences.Where(x => x.user_id == userId).ToList();
                var educations = db.educations.Where(x => x.user_id == userId).ToList();
                var workflows = db.workflows.Where(x => x.user_id == userId).ToList();
                var achievements = db.achievements.Where(x => x.user_id == userId).ToList();
                var interests = db.interests.Where(x => x.user_id == userId).ToList();
                var socialLinks = db.social_links.Where(x => x.user_id == userId).ToList();

                // Determine recommendations (less than 2 items)
                var recommendations = new System.Collections.Generic.List<string>();
                if (experiences.Count < 2) recommendations.Add("Experiences");
                if (educations.Count < 2) recommendations.Add("Educations");
                if (workflows.Count < 2) recommendations.Add("Projects");
                if (achievements.Count < 2) recommendations.Add("Achievements");
                if (interests.Count < 2) recommendations.Add("Interests");

                // Pass data to ViewBag
                ViewBag.TotalVisits = totalVisits;
                ViewBag.TodayVisits = todayVisits;
                ViewBag.ThisWeekVisits = thisWeekVisits;
                ViewBag.ThisMonthVisits = thisMonthVisits;
                ViewBag.Last7DaysVisits = last7DaysVisits;
                ViewBag.VisitsByHour = visitsByHour;
                ViewBag.ExperiencesCount = experiences.Count;
                ViewBag.EducationsCount = educations.Count;
                ViewBag.WorkflowsCount = workflows.Count;
                ViewBag.AchievementsCount = achievements.Count;
                ViewBag.InterestsCount = interests.Count;
                ViewBag.SocialLinksCount = socialLinks.Count;
                ViewBag.Recommendations = recommendations;

                return View();
            }
            catch (Exception ex)
            {
                // Show with empty data on error
                ViewBag.TotalVisits = 0;
                ViewBag.TodayVisits = 0;
                ViewBag.ThisWeekVisits = 0;
                ViewBag.ThisMonthVisits = 0;
                ViewBag.Last7DaysVisits = Enumerable.Range(0, 7)
                    .Select(i => new { Date = DateTime.Today.AddDays(-i), Count = 0 })
                    .OrderBy(x => x.Date)
                    .ToList();
                ViewBag.VisitsByHour = Enumerable.Range(0, 24)
                    .Select(hour => new { Hour = hour, Count = 0 })
                    .ToList();
                ViewBag.ExperiencesCount = 0;
                ViewBag.EducationsCount = 0;
                ViewBag.WorkflowsCount = 0;
                ViewBag.AchievementsCount = 0;
                ViewBag.InterestsCount = 0;
                ViewBag.SocialLinksCount = 0;
                ViewBag.Recommendations = new System.Collections.Generic.List<string>();
                ViewBag.Error = "Database error: " + ex.Message;
                return View();
            }
        }

        // API: Get Last 7 Days Visits
        [HttpGet]
        public JsonResult GetLast7DaysVisits()
        {
            if (Session["user_id"] == null)
            {
                return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            int userId = (int)Session["user_id"];

            try
            {
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => new
                    {
                        Date = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"),
                        Count = 0
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                try
                {
                    last7Days = Enumerable.Range(0, 7)
                        .Select(i =>
                        {
                            var targetDate = DateTime.Today.AddDays(-i);
                            var startDate = targetDate.Date;
                            var endDate = startDate.AddDays(1);
                            
                            return new
                            {
                                Date = targetDate.ToString("yyyy-MM-dd"),
                                Count = db.profile_visits.Count(x => x.user_id == userId &&
                                    x.visited_at >= startDate && x.visited_at < endDate)
                            };
                        })
                        .OrderBy(x => x.Date)
                        .ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error in GetLast7DaysVisits: " + ex.Message);
                }

                return Json(new { success = true, data = last7Days }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error loading data" }, JsonRequestBehavior.AllowGet);
            }
        }

        // API: Get Hourly Visits
        [HttpGet]
        public JsonResult GetHourlyVisits()
        {
            if (Session["user_id"] == null)
            {
                return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            int userId = (int)Session["user_id"];

            try
            {
                var visitsByHour = Enumerable.Range(0, 24)
                    .Select(hour => new
                    {
                        Hour = hour,
                        Count = 0
                    })
                    .ToList();

                try
                {
                    visitsByHour = Enumerable.Range(0, 24)
                        .Select(hour => new
                        {
                            Hour = hour,
                            Count = db.profile_visits.Count(x => x.user_id == userId &&
                                x.visited_at.Hour == hour)
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error in GetHourlyVisits: " + ex.Message);
                }

                return Json(new { success = true, data = visitsByHour }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error loading data" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

