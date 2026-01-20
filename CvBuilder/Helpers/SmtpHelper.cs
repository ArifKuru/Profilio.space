using CvBuilder.Models.Entity;
using System;
using System.Net;
using System.Net.Mail;
using System.Linq;

namespace CvBuilder.Helpers
{
    public class SmtpHelper
    {
        /// <summary>
        /// Gets active SMTP settings from database
        /// </summary>
        public static smtp_settings GetActiveSmtpSettings()
        {
            using (arifkuru_cvEntities1 db = new arifkuru_cvEntities1())
            {
                return db.smtp_settings.FirstOrDefault(s => s.is_active == true);
            }
        }

        /// <summary>
        /// Sends an email using SMTP settings from database
        /// </summary>
        public static bool SendEmail(string to, string subject, string body, bool isBodyHtml = true)
        {
            try
            {
                var smtpSettings = GetActiveSmtpSettings();
                
                if (smtpSettings == null)
                {
                    throw new Exception("No active SMTP settings found in database.");
                }

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(smtpSettings.from_email, smtpSettings.from_name ?? "CvBuilder System");
                    mail.To.Add(to);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = isBodyHtml;

                    using (SmtpClient smtp = new SmtpClient(smtpSettings.smtp_server, smtpSettings.smtp_port))
                    {
                        // SMTP Authentication - Credentials set edildiğinde otomatik aktif olur
                        smtp.Credentials = new NetworkCredential(smtpSettings.smtp_username, smtpSettings.smtp_password);
                        smtp.EnableSsl = smtpSettings.enable_ssl;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log error if needed
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tests SMTP connection by sending a test email
        /// </summary>
        public static bool TestSmtpConnection()
        {
            try
            {
                var smtpSettings = GetActiveSmtpSettings();
                
                if (smtpSettings == null)
                {
                    throw new Exception("No active SMTP settings found in database.");
                }

                // Send a test email to the from_email address
                return SendEmail(
                    smtpSettings.from_email,
                    "SMTP Test Email - CvBuilder",
                    "<p>This is a test email to verify your SMTP configuration.</p><p>If you received this email, your SMTP settings are working correctly!</p>",
                    true
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"SMTP connection test failed: {ex.Message}", ex);
            }
        }
    }
}

