using AllCaps.Recognition;
using System;
using System.Threading.Tasks;

namespace AllCaps
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Please put in \"key,region\" for Azure Cognitive Services [leave blank for local]:");
            var config = Console.ReadLine().Trim();
            using (var recognizer = CreateRecognizer(config))
            {
                ISpeechRecognizer inst = recognizer;
                inst.SpeechRecognized += (snd, evt) =>
                {
                    Console.Write($"\r[{evt.Result.Offset:hh\\:mm\\:ss}]: {evt.Result.Text}\r\n");
                };

                inst.SpeechPredicted += (snd, evt) =>
                {
                    Console.Write($"\r[{evt.Result.Offset:hh\\:mm\\:ss}]: {evt.Result.Text}");
                };

                await inst.StartAsync();

                while (true)
                {
                    Console.ReadLine();
                    break;
                }

                await inst.StopAsync();
            }
        }

        static ISpeechRecognizer CreateRecognizer(string config)
        {
            if (!string.IsNullOrEmpty(config))
            {
                var parts = config.Split(',');
                return new LoopbackRecognizer(parts[0], parts[1]);
            }

            return new LoopbackRecognizer();
        }
    }
}
