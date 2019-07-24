using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllCaps.Recognition
{
    public interface ISpeechRecognizer : IDisposable
    {
        /// <summary>
        /// Event that's triggered when speech is recognized
        /// </summary>
        event EventHandler<RecognitionEventArgs> SpeechRecognized;

        /// <summary>
        /// Event that's triggered when speech is guessed
        /// </summary>
        event EventHandler<RecognitionEventArgs> SpeechPredicted;

        /// <summary>
        /// Starts the recognizer
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops the recognizer
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Gets a string name representing the recognizer
        /// </summary>
        string RecognizerName { get; }
    }
}
