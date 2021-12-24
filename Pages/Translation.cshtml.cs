using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages
{
    public class TranslationModel : PageModel
    {
        private readonly ITranslator _translator;
        public string userPrefLang { get; set; } = "zh";

        public TranslationModel(ITranslator translator)
        {
            _translator = translator;
        }

        public void OnGet()
        {
          
        }
    }
}
