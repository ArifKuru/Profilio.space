using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CvBuilder
{
    public class RouteConfig
    {

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
    name: "Root",
    url: "",
    defaults: new { controller = "Landing", action = "Index" }
);
            // Legal sayfaları için route'lar (username rotasından önce olmalı)

            routes.MapRoute(
               name: "LandingGdpr",
               url: "gdpr",
               defaults: new { controller = "Landing", action = "Gdpr" }
           );
            routes.MapRoute(
                name: "LandingPrivacy",
                url: "privacy",
                defaults: new { controller = "Landing", action = "Privacy" }
            );
            routes.MapRoute(
                name: "LandingTerms",
                url: "terms",
                defaults: new { controller = "Landing", action = "Terms" }
            );
            routes.MapRoute(
                name: "LandingDisclaimer",
                url: "disclaimer",
                defaults: new { controller = "Landing", action = "Disclaimer" }
            );


            routes.MapRoute(
             name: "AdminDashboard",
             url: "admin/dashboard/{action}/{id}",
             defaults: new { controller = "dashboard", action = "Index", id = UrlParameter.Optional }
         );

            routes.MapRoute(
             name: "AdminAbout",
             url: "admin/about/{action}/{id}",
             defaults: new { controller = "about", action = "Index", id = UrlParameter.Optional }
         );

            routes.MapRoute(
                 name: "AdminBuildCv",
                 url: "admin/buildcv/{action}/{id}",
                 defaults: new { controller = "BuildCv", action = "Index", id = UrlParameter.Optional }
             );

                        routes.MapRoute(
               name: "AdminLinks",
               url: "admin/links/{action}/{id}",
               defaults: new { controller = "Links", action = "Index", id = UrlParameter.Optional }
            );

                            routes.MapRoute(
                  name: "AdminExperiences",
                  url: "admin/experiences/{action}/{id}",
                  defaults: new { controller = "Experiences", action = "Index", id = UrlParameter.Optional }
                );

                        routes.MapRoute(
            name: "AdminEducations",
            url: "admin/educations/{action}/{id}",
            defaults: new { controller = "Educations", action = "Index", id = UrlParameter.Optional }
            );

                        routes.MapRoute(
            name: "AdminWorkflows",
            url: "admin/workflows/{action}/{id}",
            defaults: new { controller = "workflows", action = "Index", id = UrlParameter.Optional }
            );


            routes.MapRoute(
name: "AdminProfile",
url: "admin/profile/{action}/{id}",
defaults: new { controller = "profile", action = "Index", id = UrlParameter.Optional }
);

            routes.MapRoute(
                name: "AdminInterests",
                url: "admin/interests/{action}/{id}",
                defaults: new { controller = "interests", action = "Index", id = UrlParameter.Optional }
                );

            routes.MapRoute(
  name: "AdminAchievements",
  url: "admin/achievements/{action}/{id}",
  defaults: new { controller = "achievements", action = "Index", id = UrlParameter.Optional }
  );
            routes.MapRoute(
name: "AdminInbox",
url: "admin/inbox/{action}/{id}",
defaults: new { controller = "inbox", action = "Index", id = UrlParameter.Optional }
);

            routes.MapRoute(
                name: "PsAdmin",
                url: "ps-admin/{action}/{id}",
                defaults: new { controller = "PsAdmin", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
        name: "LoginRoute",
        url: "login",
        defaults: new { controller = "Login", action = "Index" } // Controller ve Action adını projene göre düzenle
    );
            routes.MapRoute(
        name: "RegisterRoute",
        url: "register",
        defaults: new { controller = "Register", action = "Index" } // Controller ve Action adını projene göre düzenle
    );
            // ... Login ve Register rotalarının hemen altına ekle ...

           
            // "/admin" yazıldığında About sayfasına gitsin (Geçici)
            routes.MapRoute(
                name: "AdminTemp",
                url: "admin",
                defaults: new { controller = "dashboard", action = "Index" }
            );

            routes.MapRoute(
   name: "UserProfileAdmin",
   url: "profile", // Örnek: /arifkuru
   defaults: new { controller = "Profile", action = "Index" }
);
            // ... {username} UserProfile rotası bunun altında kalmalı ...
            // YENİ EKLENECEK KISIM: Kullanıcı Profili Rotası
            routes.MapRoute(
                name: "UserProfile",
                url: "{username}", // Örnek: /arifkuru
                defaults: new { controller = "Home", action = "Index" }
            );

   

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );


        }
    }
}
