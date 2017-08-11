using System;

namespace BrightControl
{
    static class Brightness
    {
        // Returns brightness of first screen
        public static int GetBrightnes()
        {
            int curBrightness = 0;

            // Define scope (namespace)
            System.Management.ManagementScope scope = new System.Management.ManagementScope("root\\WMI");
            
            // Define query
            System.Management.SelectQuery query = new System.Management.SelectQuery("WmiMonitorBrightness");

            // Get current brightness
            using (System.Management.ManagementObjectSearcher objectSearcher = new System.Management.ManagementObjectSearcher(scope, query))
            using (System.Management.ManagementObjectCollection objectCollection = objectSearcher.Get())
            {
                // Store result
                foreach (System.Management.ManagementObject o in objectCollection)
                {
                    curBrightness = (byte)o.GetPropertyValue("CurrentBrightness");
                    break; // Return brightness of first object
                }
            }

            return (int)curBrightness;
        }

        // Set's brightness of all screens
        public static void SetBrightness(byte percentage)
        {
            // Define scope (namespace)
            System.Management.ManagementScope scope = new System.Management.ManagementScope("root\\WMI");

            // Define query
            System.Management.SelectQuery query = new System.Management.SelectQuery("WmiMonitorBrightnessMethods");

            // Get brightness methods
            using (System.Management.ManagementObjectSearcher objectSearcher = new System.Management.ManagementObjectSearcher(scope, query))
            using (System.Management.ManagementObjectCollection objectCollection = objectSearcher.Get())
            {
                // Store result
                foreach (System.Management.ManagementObject o in objectCollection)
                {
                    // Object is made out of (delayTime, newPercentage)
                    o.InvokeMethod("WmiSetBrightness", new Object[] { 0, percentage });
                    //break; // Only act on first object
                }
            }
        }
    }
}
