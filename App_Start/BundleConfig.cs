using System.Web.Optimization;

namespace PandoraWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/bundles/public_css").Include(
          "~/assets/css/global.css",
          "~/assets/css/components/buttons.css",
          "~/assets/css/components/navbar.css",
          "~/assets/css/components/product-card.css",
          "~/assets/css/components/footer.css",
          "~/assets/css/components/forms.css",
          "~/assets/css/components/quantity-input.css",
          "~/assets/css/components/section-title.css",
          "~/assets/css/components/breadcrumb.css",
          "~/assets/css/index.css",
          "~/assets/css/profile.css"));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}

