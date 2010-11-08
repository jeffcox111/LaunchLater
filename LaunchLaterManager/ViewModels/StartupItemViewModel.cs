using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using LaunchLaterOM;


namespace LaunchLaterManager.ViewModels
{
    public class StartupItemViewModel
    {
        public StartupItem StartupApp { get; set; }

        public BitmapImage AppIcon
        {
            get
            {
                if (StartupApp.FullPath.EndsWith(".exe"))
                {

                    MemoryStream ms = new MemoryStream();
                    Icon.ExtractAssociatedIcon(StartupApp.FullPath).ToBitmap().Save(ms, ImageFormat.Png);

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

            private set { }

        }
    }
}
