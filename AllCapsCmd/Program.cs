using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace AllCaps
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var capture = new WasapiLoopbackCapture())
            using (var stream = new WaveCaptureStream(capture))
            using (var resample = new Wave32To16CaptureStream(stream))
            using (var eng = new SpeechRecognitionEngine())
            {
                eng.LoadGrammar(new DictationGrammar());
                eng.SpeechRecognized += (snd, evt) =>
                {
                    string text = evt.Result.Text;
                    Console.WriteLine($"{text}");
                };

                eng.AudioSignalProblemOccurred += (snd, evt) =>
                {
                    Debug.WriteLine(evt.AudioSignalProblem);
                };

                capture.StartRecording();
                eng.SetInputToAudioStream(resample, new SpeechAudioFormatInfo(resample.WaveFormat.SampleRate, (AudioBitsPerSample)resample.WaveFormat.BitsPerSample, (AudioChannel)resample.WaveFormat.Channels));
                eng.RecognizeAsync(RecognizeMode.Multiple);
                while (true)
                {
                    Console.ReadLine();
                    break;
                }

                eng.RecognizeAsyncStop();

                capture.StopRecording();
            }
        }
    }
}
