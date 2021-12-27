using System.IO;
using Google.Cloud.TextToSpeech.V1;
using IdGen;

namespace KampongTalk.Tools
{
    public interface ISpeech
    {
        string SynthesizeSpeech(string text, string language, string gender);
        string DeleteSpeechFile(string filename);
    }

    public class SpeechSythesizer : ISpeech
    {
        private readonly TextToSpeechClient client;

        public SpeechSythesizer()
        {
            client = TextToSpeechClient.Create();
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