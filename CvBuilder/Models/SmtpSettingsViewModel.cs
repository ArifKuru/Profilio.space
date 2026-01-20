using System;
using System.ComponentModel.DataAnnotations;

namespace CvBuilder.Models
{
    public class SmtpSettingsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "SMTP Server is required")]
        [Display(Name = "SMTP Server")]
        public string SmtpServer { get; set; }

        [Required(ErrorMessage = "SMTP Port is required")]
        [Display(Name = "SMTP Port")]
        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
        public int SmtpPort { get; set; }

        [Required(ErrorMessage = "SMTP Username is required")]
        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; }

        [Required(ErrorMessage = "SMTP Password is required")]
        [Display(Name = "SMTP Password")]
        [DataType(DataType.Password)]
        public string SmtpPassword { get; set; }

        [Required(ErrorMessage = "From Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "From Email")]
        public string FromEmail { get; set; }

        [Display(Name = "From Name")]
        public string FromName { get; set; }

        [Display(Name = "Enable SSL")]
        public bool EnableSsl { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}

