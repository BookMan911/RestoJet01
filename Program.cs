using RestoJett.Core;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register the RestaurantService as a singleton (implements IRestaurantService)
builder.Services.AddSingleton<IRestaurantService, RestaurantService>();

// Register LanguageService as a singleton
builder.Services.AddSingleton<LanguageService>();

// Register IHostingEnvironment for accessing web root path
builder.Services.AddSingleton<IHostingEnvironment>(builder.Environment);

var app = builder.Build();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();