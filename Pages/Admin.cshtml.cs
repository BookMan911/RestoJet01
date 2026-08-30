using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

        public List<string> MealImages { get; set; } = new List<string>();

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
            LoadMealImages();
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

        private void LoadMealImages()
        {
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "meal-images");
            if (Directory.Exists(imagesPath))
            {
                MealImages = Directory.GetFiles(imagesPath)
                    .Select(Path.GetFileName)
                    .ToList();
            }
        }

        public IActionResult OnPostUploadMealImage()
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                MealError = new Exception("No file uploaded");
                LoadData(testAdmin);
                LoadMealImages();
                LoggedUser = testAdmin;
                return Page();
            }

            // Validate file is an image
            var contentType = file.ContentType;
            if (!contentType.StartsWith("image/"))
            {
                MealError = new Exception("Invalid file type. Please upload an image.");
                LoadData(testAdmin);
                LoadMealImages();
                LoggedUser = testAdmin;
                return Page();
            }

            // Calculate MD5 hash of the file
            string md5Hash;
            using (var md5 = MD5.Create())
            {
                using (var stream = file.OpenReadStream())
                {
                    var hashBytes = md5.ComputeHash(stream);
                    md5Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }

            // Get file extension
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{md5Hash}{extension}";

            // Save file to wwwroot/meal-images
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "meal-images");
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            var filePath = Path.Combine(imagesPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            LoadData(testAdmin);
            LoadMealImages();
            LoggedUser = testAdmin;

            return Page();
        }

        public IActionResult OnPostDeleteMealImage(string imageName)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            // Validate filename to prevent directory traversal attacks
            if (string.IsNullOrEmpty(imageName) || imageName.Contains("..") || imageName.Contains("/"))
            {
                MealError = new Exception("Invalid image name");
                LoadData(testAdmin);
                LoadMealImages();
                LoggedUser = testAdmin;
                return Page();
            }

            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "meal-images");
            var filePath = Path.Combine(imagesPath, imageName);

            if (!System.IO.File.Exists(filePath))
            {
                MealError = new Exception("Image not found");
                LoadData(testAdmin);
                LoadMealImages();
                LoggedUser = testAdmin;
                return Page();
            }

            System.IO.File.Delete(filePath);

            LoadData(testAdmin);
            LoadMealImages();
            LoggedUser = testAdmin;

            return Page();
        }

        public IActionResult OnPostAssignMealImage(string mealGuid, string imageHash)
        {
            LangService.For("en");

            var testAdmin = new JUser
            {
                Name = "admin",
                Password = "admin123",
                Guid = "admin-guid",
                UserType = JUserType.Admin
            };

            if (string.IsNullOrEmpty(mealGuid) || string.IsNullOrEmpty(imageHash))
            {
                return new JsonResult(new { success = false, error = "Invalid parameters" });
            }

            // Find the meal and update its ImageHash
            var mealsResult = _restaurantService.GetMeals(testAdmin);
            if (mealsResult.Item1 != null)
            {
                return new JsonResult(new { success = false, error = mealsResult.Item1.Message });
            }

            var meal = mealsResult.Item2.FirstOrDefault(m => m.Guid == mealGuid);
            if (meal == null)
            {
                return new JsonResult(new { success = false, error = "Meal not found" });
            }

            // Extract just the MD5 hash (without extension) for storage
            var md5Hash = Path.GetFileNameWithoutExtension(imageHash);
            
            // Create a copy of the meal with updated ImageHash
            var updatedMeal = new JMeal
            {
                Guid = meal.Guid,
                Name = meal.Name,
                Price = meal.Price,
                Discount = meal.Discount,
                Version = meal.Version,
                Description = meal.Description,
                ImageHash = md5Hash
            };

            // Update the meal through the service
            var result = _restaurantService.UpdateMeal(testAdmin, mealGuid, updatedMeal);
            if (result.Item1 != null)
            {
                return new JsonResult(new { success = false, error = result.Item1.Message });
            }

            return new JsonResult(new { success = true });
        }
    }
}
