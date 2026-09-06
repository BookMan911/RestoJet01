using RestoJett.Core;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register the RestaurantService as a singleton (implements IRestaurantService)
builder.Services.AddSingleton<IRestaurantService, RestaurantService>();

// Register LanguageService as a singleton
builder.Services.AddSingleton<LanguageService>();

// Register IWebHostEnvironment for accessing web root path
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

// 1. Build a temporary ServiceProvider
using var serviceProvider = builder.Services.BuildServiceProvider();

// 2. Resolve the service





var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var languageService = scope.ServiceProvider.GetRequiredService<LanguageService>();

    var restaurantService = scope.ServiceProvider.GetRequiredService<IRestaurantService>();
    var user = new JUser();
    user.Name = "Admin";
    user.Guid = "888-22-33-11";
    user.UserType = JUserType.Admin;
    restaurantService.AddUser(user, user);

    var customer = new JCustomer();
    customer.Name = "Ahmed";
    customer.Guid = Guid.NewGuid().ToString();
    restaurantService.AddCustomer(user, customer);


    var meal = new JMeal();
    meal.Name = "Pizza";
    meal.Price = 10.99m;
    meal.Guid = Guid.NewGuid().ToString();
    var addMealResult = restaurantService.AddMeal(user, meal);
    var meal2 = new JMeal();
    meal2.Name = "Burger";
    meal2.Price = 8.99m;
    meal2.Guid = Guid.NewGuid().ToString();
    var addMealResult2 = restaurantService.AddMeal(user, meal2);
    var meal3 = new JMeal();
    meal3.Name = "Pasta";
    meal3.Price = 12.99m;
    meal3.Guid = Guid.NewGuid().ToString();
    var addMealResult3 = restaurantService.AddMeal(user, meal3);
    var meal4 = new JMeal();
    meal4.Name = "Salad";
    meal4.Price = 6.99m;
    meal4.Guid = Guid.NewGuid().ToString();
    var addMealResult4 = restaurantService.AddMeal(user, meal4);



    var pilot = new JPilot();
    pilot.Name = "John";
    pilot.Guid = Guid.NewGuid().ToString();
    pilot.CurrentResUrl = Guid.NewGuid().ToString();
    var addPilotResult = restaurantService.AddPilot(user, pilot);



    // Use your service here
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();