using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.FFmpegRender.UI.ViewModels
{
    class RenderWVM : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        private void NotifyPropertyChange([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion



        double _ProgressValue = 0;
        public double ProgressValue
        {
            get { return _ProgressValue; }
            set { _ProgressValue = value; NotifyPropertyChange(); }
        }
        double _ProgressMax = 1;
        public double ProgressMax
        {
            get { return _ProgressMax; }
            set { _ProgressMax = value; NotifyPropertyChange(); }
        }

        double _Percent = 0;
        public double Percent
        {
            get { return _Percent; }
            set { _Percent = value; NotifyPropertyChange(); }
        }

        string _StepInfo = string.Empty;
        public string StepInfo
        {
            get { return _StepInfo; }
            set { _StepInfo = value; NotifyPropertyChange(); }
        }

    }
}
