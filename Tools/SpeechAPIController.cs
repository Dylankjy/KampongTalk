using Microsoft.AspNetCore.Mvc;

namespace KampongTalk.Tools
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechAPIController : ControllerBase
    {
        private readonly ISpeech _speech;

        public SpeechAPIController(ISpeech speech)
        {
            _speech = speech;
        }

        [HttpGet]
        [Route("Synthesize")]
        public ActionResult Synthesize(string text, string language, string gender)
        {
            var filename = _speech.SynthesizeSpeech(text, language, gender);
            return Ok(filename);
        }
        
        [HttpGet]
        [Route("AutoSynthesize")]
        public ActionResult AutoSynthesize(string text, string gender)
        {
            var res = _speech.AutoSynthesizeSpeech(text, gender);
            return Ok(res);
        }

        [HttpGet]
        [Route("Delete")]
        public ActionResult Delete(string filename)
        {
            var resp = _speech.DeleteSpeechFile(filename);
            return Ok(resp);
        }
    }
}