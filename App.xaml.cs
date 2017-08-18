using System;
using System.IO;
using System.Windows;

namespace BrightControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static System.Windows.Forms.NotifyIcon notifyIcon;
        static BrightnessSlider slider = new BrightnessSlider();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Test for wmi brightness control system support
            // Check if wmi brightness functions are supported
            try
            {
                Brightness.GetBrightnes();
            }
            catch (System.Management.ManagementException ex)
            {
                if (ex.Message.Trim() == "Not supported")
                {
                    MessageBox.Show(
                        "It seems like your system doesn't support WMI Brightness control\n" +
                        "(Usually only supported by laptops and tablets)\n\n" +
                        "Bye!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(1);
                }
            }

            // Set system tray icon
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(NotifyIcon_Click);
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIcon.ContextMenu.MenuItems.Add("About", new EventHandler(About));
            notifyIcon.ContextMenu.MenuItems.Add("Exit", new EventHandler(ShutDown));
            Stream iconStream = GetResourceStream(new Uri("pack://application:,,,/Resources/trayIcon.ico")).Stream;
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            notifyIcon.Text = "BrightControl";
            notifyIcon.Visible = true;
        }
        
        static private void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                // This must be cached separately
                // (I guess because it's an event, it doesn't bubble up to the application unhandledException handler even if app.dispacher is used)
                try
                {
                    slider.Show_Slider(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured (" + ex.GetType() + ")\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(1);
                }
            }
        }

        static private void ShutDown(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Environment.Exit(1);
        }

        static private void About(object sender, EventArgs e)
        {
            MessageBox.Show("This program provides an easy interface to ajust screen brightness through WMI", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An error occured (" + e.Exception.GetType() + ")\n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            Environment.Exit(1);
        }
    }
}
