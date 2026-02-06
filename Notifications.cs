using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSBar
{
    internal class Notifications
    {
        public static void DeviceAdded(BaseDevice device)
        {
            new ToastContentBuilder()
                .AddText("Device added")
                .AddText($"{device.GetName()} has been connected. It is at {device.GetBatteryLevel()}.")
                .Show();
        }

        public static void DeviceRemoved(BaseDevice device)
        {
            new ToastContentBuilder()
                .AddText("Device removed")
                .AddText($"{device.GetName()} has been disconnected.")
                .Show();
        }

        public static void DeviceFullyCharged(BaseDevice device)
        {
            new ToastContentBuilder()
                .AddText("Device fully charged")
                .AddText($"{device.GetName()} is fully charged.")
                .Show();
        }
    }
}
