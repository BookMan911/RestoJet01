using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Antiforgery;
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
        public List<string> MealImages { get; set; } = new List<string>();

        public Exception MealError { get; set; }
        public Exception UserError { get; set; }
        public Exception CustomerError { get; set; }
        public Exception PilotError { get; set; }
        public Exception OrderError { get; set; }
        public Exception ImageError { get; set; }
        public string ImageMessage { get; set; }

        [BindProperty]
        public JUser LoggedUser { get; set; }

        [BindProperty]
        public List<JOrderItem> OrderItems { get; set; } = new List<JOrderItem>();

        public class UpdateMealImageRequest
        {
            public string MealGuid { get; set; }
            public string ImageHash { get; set; }
        }

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
            LoadMealImages();

            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostUploadMealImage(IFormFile imageFile)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mealimages");
                    Directory.CreateDirectory(uploadsFolder);

                    using (var md5 = MD5.Create())
                    {
                        using (var stream = imageFile.OpenReadStream())
                        {
                            var hashBytes = md5.ComputeHash(stream);
                            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                            
                            var extension = Path.GetExtension(imageFile.FileName);
                            var fileName = $"{hashString}{extension}";
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            stream.Position = 0;
                            using (var fileStream = System.IO.File.Create(filePath))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }
                    }

                    ImageMessage = "Image uploaded successfully!";
                }
            }
            catch (Exception ex)
            {
                ImageError = ex;
            }

            LoadData(testAdmin);
            LoadMealImages();
            LoggedUser = testAdmin;
            return Page();
        }

        private void LoadMealImages()
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mealimages");
            if (Directory.Exists(uploadsFolder))
            {
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                MealImages = Directory.GetFiles(uploadsFolder)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .Select(f => $"/mealimages/{Path.GetFileName(f)}")
                    .ToList();
            }
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

            // Generate a random UUID for CurrentUrlRes if not provided
            if (string.IsNullOrEmpty(customer.CurrentUrlRes))
            {
                customer.CurrentUrlRes = Guid.NewGuid().ToString();
            }

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

            // Generate a random UUID for CurrentResUrl if not provided
            if (string.IsNullOrEmpty(pilot.CurrentResUrl))
            {
                pilot.CurrentResUrl = Guid.NewGuid().ToString();
            }

            var result = _restaurantService.AddPilot(testAdmin, pilot);
            if (result.Item1 != null)
            {
                PilotError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostRenewPilotResUrl(string pilotGuid)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var result = _restaurantService.RenewPilotResUrl(testAdmin, pilotGuid);
            if (result.Item1 != null)
            {
                PilotError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostRenewCustomerUrlRes(string customerGuid)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var result = _restaurantService.RenewCustomerUrlRes(testAdmin, customerGuid);
            if (result.Item1 != null)
            {
                CustomerError = result.Item1;
            }

            LoadData(testAdmin);
            LoggedUser = testAdmin;
            return Page();
        }

        [IgnoreAntiforgeryToken]
        public IActionResult OnPostUpdateMealImage([FromBody] UpdateMealImageRequest request)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            try
            {
                // Reload meals data since it's not populated on POST
                LoadData(testAdmin);
                
                var meal = Meals.FirstOrDefault(m => m.Guid == request.MealGuid);
                if (meal != null)
                {
                    meal.ImageHash = request.ImageHash;
                    var result = _restaurantService.UpdateMeal(testAdmin, request.MealGuid, meal);
                    if (result.Item1 != null)
                    {
                        MealError = result.Item1;
                        return new JsonResult(new { success = false, error = result.Item1.Message });
                    }
                    
                    return new JsonResult(new { success = true, imageHash = request.ImageHash });
                }
                
                return new JsonResult(new { success = false, error = "Meal not found or update failed" });
            }
            catch (Exception ex)
            {
                MealError = ex;
                return new JsonResult(new { success = false, error = ex.Message });
            }
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
