using CvBuilder.Models.Entity;
using System.Collections.Generic;

namespace CvBuilder.Models
{
    public class CvViewModel
    {
        public users User { get; set; }
        public user_informations UserInformation { get; set; }
        public List<experiences> Experiences { get; set; }
        public List<educations> Educations { get; set; }
        public List<workflows> Workflows { get; set; }
        public List<interests> Interests { get; set; }
        public List<achievements> Achievements { get; set; }
        public List<social_links> SocialLinks { get; set; }
    }
}

