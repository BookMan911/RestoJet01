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
                // Filter to only show orders with Preparing or Preparing_Done status
                Orders = (ordersResult.Item2 ?? new List<JOrder>())
                    .Where(o => o.OrderStatus == JOrderStatus.Preparing || o.OrderStatus == JOrderStatus.Preparing_Done)
                    .ToList();
            }
        }

        public IActionResult OnPostMarkDelivered(string orderGuid)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var result = _restaurantService.UpdateOrderStatus(testAdmin, orderGuid, JOrderStatus.Delivered_Done);
            if (result.Item1 != null)
            {
                Error = result.Item1;
                return new JsonResult(new { success = false, error = result.Item1.Message });
            }

            // Update the local Orders list with the updated order
            var updatedOrder = result.Item2;
            var index = Orders.FindIndex(o => o.Guid == orderGuid);
            if (index >= 0)
            {
                Orders[index] = updatedOrder;
            }

            return new JsonResult(new { success = true });
        }
    }
}
