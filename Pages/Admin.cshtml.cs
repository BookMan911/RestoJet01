using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestoJett.Core;

namespace RestoJett.Pages
{
    public class AdminModel : PageModel
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IWebHostEnvironment _hostingEnvironment;
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
        public string ImageSuccess { get; set; }

        [BindProperty]
        public JUser LoggedUser { get; set; }

        [BindProperty]
        public List<JOrderItem> OrderItems { get; set; } = new List<JOrderItem>();

        public AdminModel(IRestaurantService restaurantService, LanguageService langService, IWebHostEnvironment hostingEnvironment)
        {
            _restaurantService = restaurantService;
            LangService = langService;
            _hostingEnvironment = hostingEnvironment;
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
            var imagesPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");
            if (Directory.Exists(imagesPath))
            {
                var imageFiles = Directory.GetFiles(imagesPath, "*.*")
                    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
                
                MealImages = imageFiles.Select(f => "/images/" + Path.GetFileName(f)).ToList();
            }
            else
            {
                MealImages = new List<string>();
            }
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
                    var imagesPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        imageFile.CopyTo(memoryStream);
                        var fileBytes = memoryStream.ToArray();
                        
                        using (var md5 = MD5.Create())
                        {
                            var hashBytes = md5.ComputeHash(fileBytes);
                            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                            
                            var extension = Path.GetExtension(imageFile.FileName);
                            var fileName = hashString + extension;
                            
                            var filePath = Path.Combine(imagesPath, fileName);
                            
                            System.IO.File.WriteAllBytes(filePath, fileBytes);
                            
                            ImageSuccess = "Image uploaded successfully!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ImageError = new Exception("Failed to upload image: " + ex.Message);
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

            try
            {
                if (!string.IsNullOrEmpty(imageName))
                {
                    var imagesPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    var filePath = Path.Combine(imagesPath, imageName);
                    
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        ImageSuccess = "Image deleted successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                ImageError = new Exception("Failed to delete image: " + ex.Message);
            }

            LoadData(testAdmin);
            LoadMealImages();
            LoggedUser = testAdmin;
            return Page();
        }

        public IActionResult OnPostSetMealImage()
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
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = reader.ReadToEndAsync().Result;
                    var data = System.Text.Json.JsonSerializer.Deserialize<SetMealImageRequest>(body);
                    
                    if (!string.IsNullOrEmpty(data.mealGuid) && !string.IsNullOrEmpty(data.imageName))
                    {
                        var meal = Meals.FirstOrDefault(m => m.Guid == data.mealGuid);
                        if (meal != null)
                        {
                            // Extract just the filename from the path if it contains path separators
                            var imageHash = Path.GetFileName(data.imageName);
                            // Remove extension to get just the hash
                            meal.ImageHash = Path.GetFileNameWithoutExtension(imageHash);
                            
                            _restaurantService.UpdateMeal(testAdmin, data.mealGuid, meal);
                            return Ok();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return BadRequest(new { message = "Failed to set image" });
        }
    }

    public class SetMealImageRequest
    {
        public string mealGuid { get; set; }
        public string imageName { get; set; }
    }
}
