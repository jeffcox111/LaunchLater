using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LaunchLaterOM;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Diagnostics;

namespace LaunchLaterManager.ViewModels
{
    public class LLApplicationViewModel : INotifyPropertyChanged
    {

        private LLApplication app;
        public LLApplication App { get { return app; } set { app = value; NotifyPropertyChanged("App"); } }


        public string Name
        {
            get
            {
                return App.Name;
            }
            set { App.Name = value; NotifyPropertyChanged("Name");
                 
            }
        }

       
        public string FullPath
        {
            get
            {
                return App.FullPath;
            }
            set
            {
                App.FullPath = value;
                NotifyPropertyChanged("FullPath");
                NotifyPropertyChanged("AppIcon");
            }
        }

        public string Arguments
        {
            get
            {
                return app.Arguments;
            }
            set
            {
                app.Arguments = value;
                NotifyPropertyChanged("Arguments");
            }
        }
        public string DelaySeconds
        {
            get
            {
                return App.DelaySeconds.ToString();
            }
            set {
                int seconds;
                bool parseResult = int.TryParse(value, out seconds);
                App.DelaySeconds = parseResult ? seconds : 0;
                NotifyPropertyChanged("DelaySeconds");
            }
        }

       
        public BitmapImage AppIcon
        {
            get
            {
                if (App.FullPath.EndsWith(".exe"))
                {
                   
                    MemoryStream ms = new MemoryStream();
                    Icon.ExtractAssociatedIcon(App.FullPath).ToBitmap().Save(ms, ImageFormat.Png);

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();

                    return bi;
                }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    Icon i = new Icon("settings.ico");
                    i.ToBitmap().Save(ms, ImageFormat.Png);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();

                    return bi;
                   
                }

                   
            }
           
            private set {}

        }



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
