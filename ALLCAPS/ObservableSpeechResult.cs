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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
