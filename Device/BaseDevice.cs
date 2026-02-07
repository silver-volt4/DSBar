using HidSharp;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DSBar
{
    enum DeviceStatus
    {
        New,
        Unknown,
        Discharging,
        Charging,
        FullyCharged
    }

    static class XDeviceStatus
    {
        public static bool IsInitialized(this DeviceStatus status)
        {
            return status != DeviceStatus.New && status != DeviceStatus.Unknown;
        }

        public static bool IsCharging(this DeviceStatus status)
        {
            return status == DeviceStatus.Charging || status == DeviceStatus.FullyCharged;
        }
    }

    internal abstract class BaseDevice
    {
        protected HidDevice device;
        protected NotifyIcon icon;

        protected int batteryLevel = 0;
        protected DeviceStatus status = DeviceStatus.New;

        public BaseDevice(HidDevice device)
        {
            this.device = device;
            icon = new()
            {
                Visible = true
            };
            UpdateTrayIcon();
        }

        public virtual string GetName()
        {
            return "Controller";
        }

        public string GetBatteryLevel()
        {
            if (status == DeviceStatus.Unknown || status == DeviceStatus.New)
            {
                return "unknown battery status";
            }

            return $"{batteryLevel}%";
        }

        public void Update()
        {
            ReadControllerData(out int newLevel, out DeviceStatus newStatus);

            bool sendInitialNotification = false;

            if (status == DeviceStatus.New)
            {
                if (newStatus == DeviceStatus.New || newStatus == DeviceStatus.Unknown)
                {
                    return;
                }
                sendInitialNotification = true;
            }

            bool shouldTrayUpdate = false;

            if ((status != newStatus) || (batteryLevel != newLevel))
            {
                shouldTrayUpdate = true;

                if (newStatus == DeviceStatus.FullyCharged)
                {
                    Notifications.DeviceFullyCharged(this);
                    sendInitialNotification = false;
                }
            }

            status = newStatus;
            batteryLevel = newLevel;

            if (shouldTrayUpdate)
            {
                UpdateTrayIcon();
            }

            if (sendInitialNotification)
            {
                Notifications.DeviceAdded(this);
            }
        }

        private void UpdateTrayIcon()
        {
            string suffix = "";
            if (status == DeviceStatus.Charging)
            {
                suffix = " ⚡";
            }
            else if (status == DeviceStatus.FullyCharged)
            {
                suffix = " ⚡✅";
            }

            icon.Text = $"{GetName()} ({GetBatteryLevel()}){suffix}";
            icon.Icon = GetIconByControllerState();
        }

        protected abstract void ReadControllerData(out int batteryLevel, out DeviceStatus status);
        public abstract Icon GetIconByControllerState();

        public void Dispose()
        {
            Notifications.DeviceRemoved(this);
            icon.Visible = false;
            icon.Dispose();
        }
    }
}
