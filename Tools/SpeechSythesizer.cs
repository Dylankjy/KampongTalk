using System.IO;
using Google.Cloud.TextToSpeech.V1;
using Google.Cloud.Translation.V2;
using IdGen;

namespace KampongTalk.Tools
{
    public interface ISpeech
    {
        string SynthesizeSpeech(string text, string language, string gender);
        string AutoSynthesizeSpeech(string text, string gender);
        string DeleteSpeechFile(string filename);
    }

    public class SpeechSythesizer : ISpeech
    {
        private readonly TextToSpeechClient client;
        private readonly TranslationClient translateClient;

        public SpeechSythesizer()
        {
            client = TextToSpeechClient.Create();
            translateClient = TranslationClient.Create();
        }

        public string AutoSynthesizeSpeech(string text, string gender)
        {
            var filename = new IdGenerator(2).CreateId().ToString();
            filename += ".mp3";

            var input = new SynthesisInput
            {
                Text = text
            };

            var voiceGender = SsmlVoiceGender.Male;

            if (gender == "Female") voiceGender = SsmlVoiceGender.Female;

            var detectedLangCode = translateClient.DetectLanguage(text).Language;
            string language;

            switch (detectedLangCode)
            {
                case "en":
                    language = "en-US";
                    break;
                case "zh-CN":
                    language = "cmn-CN";
                    break;
                case "ms":
                    language = "ms";
                    break;
                case "ta":
                    language = "ta";
                    break;
                default:
                    language = "en-US";
                    break;
            }

            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = language,
                SsmlGender = voiceGender
            };

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            using (var output = File.Create($"wwwroot/speech/{filename}"))
            {
                response.AudioContent.WriteTo(output);
            }

            return filename;
        }

        public string SynthesizeSpeech(string text, string language, string gender)
        {
            var filename = new IdGenerator(2).CreateId().ToString();
            filename += ".mp3";

            var input = new SynthesisInput
            {
                Text = text
            };

            var voiceGender = SsmlVoiceGender.Male;

            if (gender == "Female") voiceGender = SsmlVoiceGender.Female;

            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = language,
                SsmlGender = voiceGender
            };

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            using (var output = File.Create($"wwwroot/speech/{filename}"))
            {
                response.AudioContent.WriteTo(output);
            }

            return filename;
        }

        public string DeleteSpeechFile(string filename)
        {
            File.Delete($"wwwroot/{filename}");
            return "Deleted";
        }
    }
}