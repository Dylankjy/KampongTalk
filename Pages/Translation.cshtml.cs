using KampongTalk.Tools;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages
{
    public class TranslationModel : PageModel
    {
        private readonly ITranslator _translator;

        public TranslationModel(ITranslator translator)
        {
            _translator = translator;
        }

        public string userPrefLang { get; set; } = "zh";

        public void OnGet()
        {
        }
    }
}