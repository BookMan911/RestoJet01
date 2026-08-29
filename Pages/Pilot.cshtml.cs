using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestoJett.Core;

namespace RestoJett.Web.Pages
{
    public class PilotModel : PageModel
    {
        private readonly IRestaurantService _restaurantService;

        public PilotModel(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        public readonly LanguageService LangService;

        public PilotModel(IRestaurantService restaurantService, LanguageService langService)
        {
            _restaurantService = restaurantService;
            LangService = langService;
        }

        [BindProperty(SupportsGet = true)]
        public string PilotGuid { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PilotName { get; set; }

        public List<JOrder> Orders { get; set; } = new List<JOrder>();
        public Exception Error { get; set; }

        public void OnGet()
        {
            LangService.For("en");

            if (string.IsNullOrEmpty(PilotGuid))
            {
                // Try to get from query parameter 'pilot' as fallback
                PilotGuid = Request.Query["pilot"].ToString();
                if (!string.IsNullOrEmpty(Request.Query["name"]))
                {
                    PilotName = Request.Query["name"].ToString();
                }
            }

            if (string.IsNullOrEmpty(PilotGuid))
            {
                return;
            }

            var result = _restaurantService.GetOrdersByPilot(PilotGuid);
            if (result.Item1 != null)
            {
                Error = result.Item1;
            }
            else
            {
                Orders = result.Item2 ?? new List<JOrder>();
            }
        }
    }
}
