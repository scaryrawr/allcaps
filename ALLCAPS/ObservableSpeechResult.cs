using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ALLCAPS
{
    public class ObservableSpeechResult : INotifyPropertyChanged
    {
        private string text;
        private TimeSpan offset;
        private TimeSpan duration;

        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Text)));
            }
        }

        public TimeSpan Offset
        {
            get => this.offset;
            set
            {
                this.offset = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Offset)));
            }
        }

        public TimeSpan Duration
        {
            get => this.duration;
            set
            {
                this.duration = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Duration)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
