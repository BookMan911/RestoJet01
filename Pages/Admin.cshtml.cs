using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestoJett.Core;

namespace RestoJett.Pages
{
    public class AdminModel : PageModel
    {
        private readonly IRestaurantService _restaurantService;
        public readonly LanguageService LangService;

        public List<JMeal> Meals { get; set; } = new List<JMeal>();
        public List<JUser> Users { get; set; } = new List<JUser>();
        public List<JCustomer> Customers { get; set; } = new List<JCustomer>();
        public List<JPilot> Pilots { get; set; } = new List<JPilot>();
        public List<JOrder> Orders { get; set; } = new List<JOrder>();
        public List<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public Exception MealError { get; set; }
        public Exception UserError { get; set; }
        public Exception CustomerError { get; set; }
        public Exception PilotError { get; set; }
        public Exception OrderError { get; set; }

        [BindProperty]
        public JUser LoggedUser { get; set; }

        [BindProperty]
        public List<JOrderItem> OrderItems { get; set; } = new List<JOrderItem>();

        public AdminModel(IRestaurantService restaurantService, LanguageService langService)
        {
            _restaurantService = restaurantService;
            LangService = langService;
        }

        public IActionResult OnGet()
        {
            // Initialize language (could be from URL or session)
            LangService.For("en");

            // For demo purposes, create a test admin user if none exists
            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            // Load all data
            LoadData(testAdmin);

            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostAddMeal([Bind(Prefix = "meal")] JMeal meal)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var result = _restaurantService.AddMeal(testAdmin, meal);
            if (result.Item1 != null)
            {
                MealError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostAddUser([Bind(Prefix = "user")] JUser user)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var result = _restaurantService.AddUser(testAdmin, user);
            if (result.Item1 != null)
            {
                UserError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostAddCustomer([Bind(Prefix = "customer")] JCustomer customer)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var result = _restaurantService.AddCustomer(testAdmin, customer);
            if (result.Item1 != null)
            {
                CustomerError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostAddOrder([Bind(Prefix = "order")] JOrder order, [Bind(Prefix = "orderItems")] List<JOrderItem> orderItems)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            // Populate order items with meal details and add to order dictionary
            if (orderItems != null && orderItems.Count > 0)
            {
                foreach (var item in orderItems)
                {
                    var meal = Meals.FirstOrDefault(m => m.Guid == item.MealGuid);
                    if (meal != null)
                    {
                        item.MealName = meal.Name;
                        item.Price = meal.Price;
                        order.Items[item.MealGuid] = item;
                    }
                }
            }

            var result = _restaurantService.AddOrder(testAdmin, order);
            if (result.Item1 != null)
            {
                OrderError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostAddPilot([Bind(Prefix = "pilot")] JPilot pilot)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var result = _restaurantService.AddPilot(testAdmin, pilot);
            if (result.Item1 != null)
            {
                PilotError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        private void LoadData(JUser user)
        {
            // Load meals
            var mealsResult = _restaurantService.GetMeals(user);
            if (mealsResult.Item1 == null)
            {
                Meals = mealsResult.Item2;
            }
            else
            {
                MealError = mealsResult.Item1;
            }

            // Load users
            var usersResult = _restaurantService.GetUsers(user);
            if (usersResult.Item1 == null)
            {
                Users = usersResult.Item2;
            }
            else
            {
                UserError = usersResult.Item1;
            }

            // Load customers
            var customersResult = _restaurantService.GetCustomers(user);
            if (customersResult.Item1 == null)
            {
                Customers = customersResult.Item2;
            }
            else
            {
                CustomerError = customersResult.Item1;
            }

            // Load pilots
            var pilotsResult = _restaurantService.GetPilots(user);
            if (pilotsResult.Item1 == null)
            {
                Pilots = pilotsResult.Item2;
            }
            else
            {
                PilotError = pilotsResult.Item1;
            }

            // Load orders
            var ordersResult = _restaurantService.GetOrders(user);
            if (ordersResult.Item1 == null)
            {
                Orders = ordersResult.Item2;
            }
            else
            {
                OrderError = ordersResult.Item1;
            }

            // Load audit logs
            var auditResult = _restaurantService.GetAuditLogs(user);
            if (auditResult.Item1 == null)
            {
                AuditLogs = auditResult.Item2;
            }
        }
    }
}
