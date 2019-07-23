using AllCaps.Input;
using AllCaps.Recognition;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Threading.Tasks;

namespace AllCaps
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var capture = new WasapiLoopbackCapture())
            using (var stream = new WaveCaptureStream(capture))
            using (ISpeechRecognizer eng = new LocalSpeechRecognizer(stream))
            {
                eng.SpeechRecognized += (snd, evt) =>
                {
                    Console.WriteLine($"{evt.Text}");
                };

                capture.StartRecording();
                await eng.StartAsync();

                while (true)
                {
                    Console.ReadLine();
                    break;
                }

                await eng.StopAsync();
                capture.StopRecording();
            }
        }
    }
}
