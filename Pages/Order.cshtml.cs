using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestoJett.Core;

namespace RestoJett.Pages
{
    public class OrderModel : PageModel
    {
        private readonly IRestaurantService _restaurantService;
        public readonly LanguageService LangService;

        public List<JMeal> Meals { get; set; } = new List<JMeal>();
        public string CustomerName { get; set; }
        public JCustomer CurrentCustomer { get; set; }
        public List<OrderItemViewModel> CartItems { get; set; } = new List<OrderItemViewModel>();
        public Exception OrderError { get; set; }
        public bool OrderSubmitted { get; set; }
        public string OrderConfirmationGuid { get; set; }
        public string CurrentLanguage { get; set; } = "en";
        public List<CustomerOrderViewModel> CustomerOrders { get; set; } = new List<CustomerOrderViewModel>();

        [BindProperty]
        public JUser LoggedUser { get; set; }

        public OrderModel(IRestaurantService restaurantService, LanguageService langService)
        {
            _restaurantService = restaurantService;
            LangService = langService;
        }

        public IActionResult OnGet(string customerName, string lang = "en", string urlRes = "")
        {
            CurrentLanguage = lang;
            
            // Load language files if not already loaded
            var langPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lang", $"{lang}.json");
            if (System.IO.File.Exists(langPath))
            {
                LangService.loadFromJson(langPath);
            }
            
            LangService.For(lang);

            // Create or retrieve customer based on name from URL
            if (string.IsNullOrEmpty(customerName))
            {
                customerName = "Guest";
            }

            CustomerName = customerName;

            // Create a test admin user for accessing the service
            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            LoggedUser = testAdmin;

            // Load meals from the service
            var mealsResult = _restaurantService.GetMeals(testAdmin);
            if (mealsResult.Item1 == null)
            {
                Meals = mealsResult.Item2;
            }

            // If urlRes is provided, validate it exists using the hashtable lookup
            if (!string.IsNullOrEmpty(urlRes))
            {
                var customerResult = _restaurantService.GetCustomerByUrlRes(urlRes);
                
                if (customerResult.Item1 != null)
                {
                    // Invalid urlRes - redirect to error page
                    return RedirectToPage("/InvalidUrl", new { lang = lang });
                }
                
                CurrentCustomer = customerResult.Item2;
            }

            // If urlRes was provided and validated, CurrentCustomer is already set
            // Otherwise, try to find existing customer by name
            if (CurrentCustomer == null)
            {
                var customersResult2 = _restaurantService.GetCustomers(testAdmin);
                if (customersResult2.Item1 == null)
                {
                    // Try to find by name
                    CurrentCustomer = customersResult2.Item2.FirstOrDefault(c => c.Name == customerName);
                    
                    if (CurrentCustomer == null)
                    {
                        // Create new customer
                        var newCustomer = new JCustomer
                        {
                            Name = customerName,
                            CurrentUrlRes = !string.IsNullOrEmpty(urlRes) ? urlRes : Guid.NewGuid().ToString()
                        };
                        var addResult = _restaurantService.AddCustomer(testAdmin, newCustomer);
                        if (addResult.Item1 == null)
                        {
                            CurrentCustomer = addResult.Item2;
                        }
                    }
                }
            }

            // Load customer orders
            var allOrdersResult = _restaurantService.GetOrders(testAdmin);
            if (allOrdersResult.Item1 == null && CurrentCustomer != null)
            {
                CustomerOrders = allOrdersResult.Item2
                    .Where(o => o.CustomerGuid == CurrentCustomer.Guid)
                    .Select(o => new CustomerOrderViewModel
                    {
                        OrderGuid = o.Guid,
                        OrderName = o.Name,
                        OrderDate = o.Date,
                        OrderStatus = o.OrderStatus.ToString(),
                        DeliveryStatus = o.DeliveryStatus.ToString(),
                        PaymentType = o.PaymentType.ToString(),
                        Items = o.Items.Values.Select(i => new OrderItemViewModel
                        {
                            MealGuid = i.MealGuid,
                            MealName = i.MealName,
                            Count = i.Count,
                            Price = i.Price
                        }).ToList(),
                        TotalAmount = o.Items.Values.Sum(i => i.Price * i.Count)
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
            }

            return Page();
        }

        public IActionResult OnPostAddToCart(string mealGuid, string mealName, decimal price, int count, string lang = "en")
        {
            CurrentLanguage = lang;
            
            // Load language files if not already loaded
            var langPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lang", $"{lang}.json");
            if (System.IO.File.Exists(langPath))
            {
                LangService.loadFromJson(langPath);
            }
            
            LangService.For(lang);

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            LoggedUser = testAdmin;

            // Reload meals
            var mealsResult = _restaurantService.GetMeals(testAdmin);
            if (mealsResult.Item1 == null)
            {
                Meals = mealsResult.Item2;
            }

            // Get customer name from form
            CustomerName = Request.Form["CustomerName"].ToString();
            var urlRes = Request.Form["CurrentUrlRes"].ToString();

            // Find or create customer - use hashtable lookup if urlRes is provided
            if (!string.IsNullOrEmpty(urlRes))
            {
                var customerResult = _restaurantService.GetCustomerByUrlRes(urlRes);
                if (customerResult.Item1 == null)
                {
                    CurrentCustomer = customerResult.Item2;
                }
            }
            
            // If not found by urlRes, try by name
            if (CurrentCustomer == null)
            {
                var customersResult = _restaurantService.GetCustomers(testAdmin);
                if (customersResult.Item1 == null)
                {
                    CurrentCustomer = customersResult.Item2.FirstOrDefault(c => c.Name == CustomerName);
                }
            }
            
            // Create new customer if still not found
            if (CurrentCustomer == null)
            {
                var newCustomer = new JCustomer 
                { 
                    Name = CustomerName, 
                    CurrentUrlRes = !string.IsNullOrEmpty(urlRes) ? urlRes : Guid.NewGuid().ToString() 
                };
                var addResult = _restaurantService.AddCustomer(testAdmin, newCustomer);
                if (addResult.Item1 == null)
                {
                    CurrentCustomer = addResult.Item2;
                }
            }

            // Add item to cart (stored in Session for session-like behavior)
            var cartKey = $"cart_{CustomerName}";
            var existingCart = HttpContext.Session.GetString(cartKey);

            var cartItems = new List<OrderItemViewModel>();
            if (!string.IsNullOrEmpty(existingCart))
            {
                cartItems = System.Text.Json.JsonSerializer.Deserialize<List<OrderItemViewModel>>(existingCart);
            }

            var existingItem = cartItems.FirstOrDefault(i => i.MealGuid == mealGuid);
            if (existingItem != null)
            {
                existingItem.Count += count;
            }
            else
            {
                cartItems.Add(new OrderItemViewModel
                {
                    MealGuid = mealGuid,
                    MealName = mealName,
                    Price = price,
                    Count = count
                });
            }

            HttpContext.Session.SetString(cartKey, System.Text.Json.JsonSerializer.Serialize(cartItems));

            return RedirectToPage(new { customerName = CustomerName, urlRes = !string.IsNullOrEmpty(urlRes) ? urlRes : CurrentCustomer?.CurrentUrlRes });
        }

        public IActionResult OnPostSubmitOrder(string lang = "en")
        {
            CurrentLanguage = lang;
            
            // Load language files if not already loaded
            var langPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lang", $"{lang}.json");
            if (System.IO.File.Exists(langPath))
            {
                LangService.loadFromJson(langPath);
            }
            
            LangService.For(lang);

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            LoggedUser = testAdmin;

            // Get customer name from form
            CustomerName = Request.Form["CustomerName"].ToString();
            var customerGuid = Request.Form["CustomerGuid"].ToString();
            var urlRes = Request.Form["CurrentUrlRes"].ToString();
            var selectedAddress = Request.Form["SelectedAddress"].ToString();
            var selectedPaymentTypeStr = Request.Form["SelectedPaymentType"].ToString();

            // Parse payment type
            JPaymentType selectedPaymentType = JPaymentType.None;
            if (!string.IsNullOrEmpty(selectedPaymentTypeStr))
            {
                Enum.TryParse(selectedPaymentTypeStr, out selectedPaymentType);
            }

            // Get cart items from Session
            var cartKey = $"cart_{CustomerName}";
            var existingCart = HttpContext.Session.GetString(cartKey);

            var cartItems = new List<OrderItemViewModel>();
            if (!string.IsNullOrEmpty(existingCart))
            {
                cartItems = System.Text.Json.JsonSerializer.Deserialize<List<OrderItemViewModel>>(existingCart);
            }

            if (cartItems.Count == 0)
            {
                OrderError = new Exception("Cannot submit empty order.");
                
                // Reload meals and customer info
                var mealsResult = _restaurantService.GetMeals(testAdmin);
                if (mealsResult.Item1 == null)
                {
                    Meals = mealsResult.Item2;
                }

                CurrentCustomer = new JCustomer { Name = CustomerName, Guid = customerGuid, CurrentUrlRes = urlRes };
                return Page();
            }

            // Create order
            var order = new JOrder
            {
                CustomerGuid = customerGuid,
                Name = $"Order for {CustomerName}",
                Items = new Dictionary<string, JOrderItem>(),
                OrderStatus = JOrderStatus.Unpaid,
                DeliveryStatus = JDeliveryStatus.Pending,
                PaymentType = selectedPaymentType,
                AddressInfo = selectedAddress,
                Confirmed = false
            };

            // Add items to order
            foreach (var item in cartItems)
            {
                order.Items[item.MealGuid] = new JOrderItem
                {
                    MealGuid = item.MealGuid,
                    MealName = item.MealName,
                    Count = item.Count,
                    Price = item.Price
                };
            }

            // Submit order to service
            var result = _restaurantService.AddOrder(testAdmin, order);
            if (result.Item1 != null)
            {
                OrderError = result.Item1;
                
                // Reload meals
                var mealsResult = _restaurantService.GetMeals(testAdmin);
                if (mealsResult.Item1 == null)
                {
                    Meals = mealsResult.Item2;
                }

                CurrentCustomer = new JCustomer { Name = CustomerName, Guid = customerGuid, CurrentUrlRes = urlRes };
                return Page();
            }

            // Clear cart from Session
            HttpContext.Session.Remove(cartKey);

            OrderSubmitted = true;
            OrderConfirmationGuid = result.Item2.Guid;

            // Reload meals for display
            var mealsResult2 = _restaurantService.GetMeals(testAdmin);
            if (mealsResult2.Item1 == null)
            {
                Meals = mealsResult2.Item2;
            }

            CurrentCustomer = new JCustomer { Name = CustomerName, Guid = customerGuid, CurrentUrlRes = urlRes };
            return Page();
        }

        public IActionResult OnGetCartItems(string customerName, string lang = "en", string urlRes = "")
        {
            CurrentLanguage = lang;
            
            // Load language files if not already loaded
            var langPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lang", $"{lang}.json");
            if (System.IO.File.Exists(langPath))
            {
                LangService.loadFromJson(langPath);
            }
            
            LangService.For(lang);
            
            var cartKey = $"cart_{customerName}";
            var existingCart = HttpContext.Session.GetString(cartKey);

            if (string.IsNullOrEmpty(existingCart))
            {
                return new JsonResult(new List<OrderItemViewModel>());
            }

            var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<OrderItemViewModel>>(existingCart);
            return new JsonResult(cartItems);
        }

        public class OrderItemViewModel
        {
            public string MealGuid { get; set; }
            public string MealName { get; set; }
            public decimal Price { get; set; }
            public int Count { get; set; }
        }

        public class CustomerOrderViewModel
        {
            public string OrderGuid { get; set; }
            public string OrderName { get; set; }
            public string OrderDate { get; set; }
            public string OrderStatus { get; set; }
            public string DeliveryStatus { get; set; }
            public string PaymentType { get; set; }
            public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
            public decimal TotalAmount { get; set; }
        }
    }
}
