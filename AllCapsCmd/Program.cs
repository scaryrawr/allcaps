using AllCaps.Recognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllCaps
{
    class Location
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
    class ConsoleText
    {
        public string Text { get; set; }

        public Location Location { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Please put in \"key,region\" for Azure Cognitive Services [leave blank for local]:");
            var config = Console.ReadLine().Trim();
            using (var recognizer = CreateRecognizer(config))
            {
                var recognitions = new Dictionary<string, ConsoleText>();
                ISpeechRecognizer inst = recognizer;
                inst.SpeechRecognized += (snd, evt) =>
                {
                    var tmp = $"\r[{evt.Result.Offset:hh\\:mm\\:ss}]: {evt.Result.Text}\r\n";
                    if (recognitions.TryGetValue(evt.ResultId, out var text))
                    {
                        UpdateDisplay(tmp, text);
                        recognitions.Remove(evt.ResultId);
                    }
                };

                inst.SpeechPredicted += (snd, evt) =>
                {
                    string tmp = $"\r[{evt.Result.Offset:hh\\:mm\\:ss}]: {evt.Result.Text}\r\n";
                    if (!recognitions.TryGetValue(evt.ResultId, out var text))
                    {
                        text = new ConsoleText
                        {
                            Text = tmp,
                            Location = new Location
                            {
                                Column = Console.CursorLeft,
                                Row = Console.CursorTop
                            }
                        };
                    }

                    UpdateDisplay(tmp, text);

                    recognitions[evt.ResultId] = text;
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

        private static void UpdateDisplay(string tmp, ConsoleText text)
        {
            // Clear?
            Console.CursorLeft = text.Location.Column;
            Console.CursorTop = text.Location.Row;
            Console.Write(Enumerable.Repeat(' ', text.Text.Length).ToArray());

            Console.CursorLeft = text.Location.Column;
            Console.CursorTop = text.Location.Row;
            Console.Write(tmp);
            text.Text = tmp;
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
