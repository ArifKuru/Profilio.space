using CvBuilder.Models.Entity;
using CvBuilder.Models;
using CvBuilder.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    [AllowAnonymous]
    public class PsAdminController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            // Eğer zaten admin girişi yapılmışsa panel sayfasına yönlendir
            if (Session["admin_logged_in"] != null && (bool)Session["admin_logged_in"])
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                // Admin kullanıcıyı bul
                var admin = db.admin_users.FirstOrDefault(x => x.username == username);

                if (admin == null)
                {
                    ViewBag.Error = "Invalid username or password!";
                    return View();
                }

                // Direkt şifre kontrolü (hash yok)
                if (admin.password_hash != password)
                {
                    ViewBag.Error = "Invalid username or password!";
                    return View();
                }

                // Giriş başarılı
                Session["admin_logged_in"] = true;
                Session["admin_username"] = admin.username;
                Session["admin_id"] = admin.id;

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            // Admin giriş kontrolü
            if (Session["admin_logged_in"] == null || !(bool)Session["admin_logged_in"])
            {
                return RedirectToAction("Login");
            }

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                // Tüm kullanıcıları ve bilgilerini çek
                var userList = db.users.ToList();
                var userInfoList = db.user_informations.ToList();

                var users = userList.Select(u =>
                {
                    var userInfo = userInfoList.FirstOrDefault(ui => ui.user_id == u.id);
                    return new AdminUserViewModel
                    {
                        Id = u.id,
                        Username = u.username,
                        Email = u.email,
                        FullName = userInfo != null ? (userInfo.name + " " + userInfo.surname) : u.username,
                        Name = userInfo != null ? userInfo.name : "",
                        Surname = userInfo != null ? userInfo.surname : ""
                    };
                }).ToList();

                ViewData["Title"] = "Admin Panel - Users";
                return View(users);
            }
        }

        public ActionResult Logout()
        {
            Session["admin_logged_in"] = false;
            Session["admin_username"] = null;
            Session["admin_id"] = null;
            return RedirectToAction("Login");
        }

        // Helper method to check admin authentication
        private bool IsAdminAuthenticated()
        {
            return Session["admin_logged_in"] != null && (bool)Session["admin_logged_in"];
        }

        [HttpGet]
        [ActionName("smtp-settings")]
        public ActionResult SmtpSettings()
        {
            // Admin giriş kontrolü
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Login");
            }

            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                var smtpSettings = db.smtp_settings.FirstOrDefault();
                
                SmtpSettingsViewModel model;
                if (smtpSettings != null)
                {
                    model = new SmtpSettingsViewModel
                    {
                        Id = smtpSettings.id,
                        SmtpServer = smtpSettings.smtp_server,
                        SmtpPort = smtpSettings.smtp_port,
                        SmtpUsername = smtpSettings.smtp_username,
                        SmtpPassword = smtpSettings.smtp_password,
                        FromEmail = smtpSettings.from_email,
                        FromName = smtpSettings.from_name,
                        EnableSsl = smtpSettings.enable_ssl,
                        IsActive = smtpSettings.is_active
                    };
                }
                else
                {
                    model = new SmtpSettingsViewModel
                    {
                        SmtpPort = 587,
                        EnableSsl = true,
                        IsActive = true
                    };
                }

                ViewData["Title"] = "Admin Panel - SMTP Settings";
                return View("SmtpSettings", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("smtp-settings")]
        public ActionResult SmtpSettings(SmtpSettingsViewModel model)
        {
            // Admin giriş kontrolü
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Admin Panel - SMTP Settings";
                return View("SmtpSettings", model);
            }

            try
            {
                using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
                {
                    smtp_settings smtpSettings;
                    
                    if (model.Id > 0)
                    {
                        // Update existing
                        smtpSettings = db.smtp_settings.Find(model.Id);
                        if (smtpSettings == null)
                        {
                            ViewBag.Error = "SMTP settings not found.";
                            ViewData["Title"] = "Admin Panel - SMTP Settings";
                            return View("SmtpSettings", model);
                        }
                    }
                    else
                    {
                        // Create new
                        smtpSettings = new smtp_settings();
                        smtpSettings.created_at = DateTime.Now;
                        db.smtp_settings.Add(smtpSettings);
                    }

                    smtpSettings.smtp_server = model.SmtpServer;
                    smtpSettings.smtp_port = model.SmtpPort;
                    smtpSettings.smtp_username = model.SmtpUsername;
                    smtpSettings.smtp_password = model.SmtpPassword;
                    smtpSettings.from_email = model.FromEmail;
                    smtpSettings.from_name = model.FromName;
                    smtpSettings.enable_ssl = model.EnableSsl;
                    smtpSettings.is_active = model.IsActive;
                    smtpSettings.updated_at = DateTime.Now;

                    db.SaveChanges();

                    ViewBag.Success = "SMTP settings saved successfully!";
                    ViewData["Title"] = "Admin Panel - SMTP Settings";
                    return View("SmtpSettings", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while saving SMTP settings: " + ex.Message;
                ViewData["Title"] = "Admin Panel - SMTP Settings";
                return View("SmtpSettings", model);
            }
        }

        [HttpPost]
        public JsonResult TestSmtpConnection()
        {
            // Admin giriş kontrolü
            if (!IsAdminAuthenticated())
            {
                return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Get SMTP settings
                var smtpSettings = SmtpHelper.GetActiveSmtpSettings();
                
                if (smtpSettings == null)
                {
                    return Json(new { success = false, message = "No active SMTP settings found." }, JsonRequestBehavior.AllowGet);
                }

                // Try to send a test email to the from_email address
                bool emailSent = SmtpHelper.SendEmail(
                    smtpSettings.from_email,
                    "SMTP Test Email - CvBuilder",
                    "<html><body><h2>SMTP Configuration Test</h2><p>This is a test email to verify your SMTP configuration.</p><p>If you received this email, your SMTP settings are working correctly!</p><p><strong>Test Time:</strong> " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</p></body></html>",
                    true
                );

                if (emailSent)
                {
                    return Json(new { 
                        success = true, 
                        message = "SMTP connection test successful! Test email sent to: " + smtpSettings.from_email + ". Please check your inbox (and spam folder)." 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Email sending returned false. Please check your SMTP settings." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Get inner exception details for more information
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner Exception: " + ex.InnerException.Message;
                }
                
                return Json(new { 
                    success = false, 
                    message = "SMTP test failed: " + errorMessage + ". Please verify your SMTP credentials and settings." 
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

