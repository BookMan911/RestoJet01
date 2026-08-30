using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestoJett.Core;

namespace RestoJett.Web.Pages
{
    public class PilotModel : PageModel
    {
        private readonly IRestaurantService _restaurantService;

        public readonly LanguageService LangService;

        public PilotModel(IRestaurantService restaurantService, LanguageService langService)
        {
            _restaurantService = restaurantService;
            LangService = langService;
        }

        [BindProperty(SupportsGet = true)]
        public string ResUrl { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PilotName { get; set; }

        public List<JOrder> Orders { get; set; } = new List<JOrder>();
        public Exception Error { get; set; }

        public void OnGet()
        {
            LangService.For("en");

            if (string.IsNullOrEmpty(ResUrl))
            {
                return;
            }

            var result = _restaurantService.GetPilotByResUrl(ResUrl);
            if (result.Item1 != null)
            {
                // Invalid ResUrl - redirect to InvalidUrl page
                HttpContext.Response.Redirect("/InvalidUrl");
                return;
            }

            var pilot = result.Item2;
            if (pilot == null)
            {
                HttpContext.Response.Redirect("/InvalidUrl");
                return;
            }

            // Get orders for this pilot using their Guid
            var ordersResult = _restaurantService.GetOrdersByPilot(pilot.Guid);
            if (ordersResult.Item1 != null)
            {
                Error = ordersResult.Item1;
            }
            else
            {
                Orders = ordersResult.Item2 ?? new List<JOrder>();
            }
        }
    }
}
