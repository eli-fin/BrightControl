using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace BrightControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BrightnessSlider : Window, IDisposable
    {
        // Contansts
        const int BRIGHTNESS_UPDATE_INTERVAL = 1000 / 3;
        const int HIDE_ANIMATION_LENGTH = 1;
        const double SLIDER_OPACITY = 0.8;

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

            Opacity = SLIDER_OPACITY;
            
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

            int exStyle = NativeMethods.GetWindowLong(
                wndHelper.Handle,
                (int)NativeMethods.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLong(
                wndHelper.Handle,
                (int)NativeMethods.GetWindowLongFields.GWL_EXSTYLE,
                (IntPtr)exStyle);
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
            hidingAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(HIDE_ANIMATION_LENGTH));
            hidingAnimation.Completed += (s, _) => HideIfTransparent();
            BeginAnimation(OpacityProperty, hidingAnimation);
        }

        // This function is needed if the tray is clicked multiple times and the previous animations
        // will keep hiding the window even if it was clicked again
        private void HideIfTransparent()
        {
            if (Opacity == 0)
                Hide();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    updateBoxTimer.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    internal static class NativeMethods
    {
        [Flags]
        internal enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        internal enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        internal static extern Int32 GetWindowLong(IntPtr hWnd, int nIndex);

        internal static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = SetWindowLong32(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        internal static extern void SetLastError(int dwErrorCode);
    }
}
