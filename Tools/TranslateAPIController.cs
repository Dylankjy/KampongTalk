using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Tools
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslateAPIController : ControllerBase
    {
        private readonly ITranslator _translator;
        public TranslateAPIController(ITranslator translator)
        {
            _translator = translator;
        }

        [HttpGet]
        [Route("Translate")]
        // Called using https://localhost:44351/api/TranslateAPI/Translate?text=No longer wok&language=zh
        public ActionResult Translate(string text, string language)
        {
            var translatedText = _translator.TranslateText(text, language);
            return Ok(translatedText);
        }

        [HttpGet]
        [Route("TranslateHtml")]
        public ActionResult TranslateHtml(string html, string language)
        {
            var translatedText = _translator.TranslateHtml(html, language);
            return Ok(translatedText);
        }
    }
}
