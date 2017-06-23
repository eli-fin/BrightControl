using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BrightControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BrightnessSlider : Window
    {
        // Interop declerations
        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion

        // Contansts
        const int BRIGHTNESS_UPDATE_INTERVAL = 1000 / 3;
        const int HIDE_ANIMATION_LENGTH = 1;

        // Variables
        int sliderVal = 0;
        int sliderValOld;
        Timer updateBoxTimer;
        DoubleAnimation hidingAnimation;

        public BrightnessSlider()
        {
            InitializeComponent();
        }

        public void Show_Slider(int bottomLeftX, int bottomLeftY)
        {
            Top = bottomLeftY - Height;
            Left = bottomLeftX - Width;

            if (hidingAnimation != null)
                BeginAnimation(OpacityProperty, null);

            Opacity = 1;
            
            // Set initial slider value
            percentageSlider.Value = Brightness.GetBrightnes();
            sliderVal = sliderValOld = (int)percentageSlider.Value;

            // Use a time for the updater to avoid repeptive needless updates
            updateBoxTimer = new Timer(UpdateBrightness, null, BRIGHTNESS_UPDATE_INTERVAL, BRIGHTNESS_UPDATE_INTERVAL);

            Activate();

            Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // This is to hide the window from the task switcher

            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sliderVal = (int)e.NewValue;
        }

        private void UpdateBrightness(object o)
        {
            // Check for a change
            if (sliderVal == sliderValOld)
                return;
            sliderValOld = sliderVal;

            // Update brightness
            Brightness.SetBrightness((byte)sliderVal);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Hide window if escape was pressed
            if (e.Key.ToString() == "Escape")
            {
                updateBoxTimer.Dispose();
                Hide();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            updateBoxTimer.Dispose();

            // Hide with an animation
            hidingAnimation = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(HIDE_ANIMATION_LENGTH));
            hidingAnimation.Completed += (s, _) => HideIfTransparent();
            BeginAnimation(Control.OpacityProperty, hidingAnimation);
        }

        // This function is needed if the tray is clicked multiple times and the previous animations
        // will keep hiding the window even if it was clicked again
        private void HideIfTransparent()
        {
            if (Opacity == 0)
                Hide();
        }
    }
}
