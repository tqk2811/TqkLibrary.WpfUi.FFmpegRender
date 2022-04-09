using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TqkLibrary.WpfUi.FFmpegRender.UI;

namespace TqkLibrary.WpfUi.FFmpegRender
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RenderWindow renderWindow = new RenderWindow(e.Args);
            renderWindow.Show();
        }
    }
}
