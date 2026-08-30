using Microsoft.AspNetCore.Mvc.RazorPages;
using RestoJett.Core;

namespace RestoJett.Pages
{
    public class InvalidUrlModel : PageModel
    {
        public readonly LanguageService LangService;
        public string CurrentLanguage { get; set; } = "en";

        public InvalidUrlModel(LanguageService langService)
        {
            LangService = langService;
        }

        public void OnGet(string lang = "en")
        {
            CurrentLanguage = lang;
            
            // Load language files if not already loaded
            var langPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lang", $"{lang}.json");
            if (System.IO.File.Exists(langPath))
            {
                LangService.loadFromJson(langPath);
            }
            
            LangService.For(lang);
        }
    }
}
