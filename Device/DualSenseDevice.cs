using HidSharp;

namespace DSBar
{
    internal class DualSenseDevice : BaseDevice
    {
        private const int USB_REPORT_LENGTH = 64;
        private const int BLUETOOTH_REPORT_LENGTH = 78;
        private const int BLUETOOTH_FULL_REPORT = 0x31;
        private const int BLUETOOTH_MINIMAL_REPORT = 0x01;
        private const int BLUETOOTH_FEATURE_GET_CALIBRATION = 0x05;
        private const int FAILURES_BEFORE_RESET = 3;

        protected int failures = 0;

        public static IEnumerable<HidDevice> ScanForDevices(DeviceList list)
        {
            foreach (var device in list.GetHidDevices(0x054C, 0x0CE6))
            {
                yield return device;
            }
        }

        public DualSenseDevice(HidDevice device) : base(device)
        {
        }

        public override string GetName()
        {
            return "DualSense controller";
        }

        protected override void ReadControllerData(out int batteryLevel, out DeviceStatus status)
        {
            batteryLevel = this.batteryLevel;
            status = this.status;

            try
            {
                using (var stream = device.Open())
                {
                    byte[] buf = new byte[device.GetMaxInputReportLength()];
                    int read = stream.Read(buf, 0, buf.Length);

                    if (read > 0)
                    {
                        ProcessControllerData(buf, read, ref batteryLevel, ref status);
                        failures = 0;
                    }
                }
            }
            catch
            {
                if (failures < FAILURES_BEFORE_RESET)
                {
                    failures += 1;
                    if (failures == FAILURES_BEFORE_RESET)
                    {
                        status = DeviceStatus.Unknown;
                    }
                }
            }
        }

        private void ProcessControllerData(byte[] inputBuffer, int bytesRead, ref int batteryLevel, ref DeviceStatus status)
        {
            bool success = false;
            byte b0 = 0;
            byte b1 = 0;

            if (bytesRead == USB_REPORT_LENGTH)
            {
                b0 = inputBuffer[53];
                b1 = inputBuffer[54];
                success = true;
            }
            else if (bytesRead == BLUETOOTH_REPORT_LENGTH)
            {
                if (inputBuffer[0] == BLUETOOTH_FULL_REPORT)
                {
                    b0 = inputBuffer[54];
                    b1 = inputBuffer[55];
                    success = true;
                }
                else if (inputBuffer[0] == BLUETOOTH_MINIMAL_REPORT)
                {
                    RequestCompleteBluetoothReport();
                }
            }

            if (success)
            {
                batteryLevel = ((b0 & 0x08) * 100) / 8;
                status = DeviceStatus.Discharging;
                if (b1 > 0)
                {
                    status = batteryLevel == 100 ? DeviceStatus.FullyCharged : DeviceStatus.Charging;
                }
            }
        }

        private void RequestCompleteBluetoothReport()
        {
            using (var stream = device.Open())
            {
                byte[] buffer = new byte[device.GetMaxFeatureReportLength()];
                buffer[0] = BLUETOOTH_FEATURE_GET_CALIBRATION;
                stream.GetFeature(buffer);
            }
        }

        public override Icon GetIconByControllerState()
        {
            if (status.IsInitialized())
            {
                if (batteryLevel == 0)
                    return status.IsCharging() ? Resources.Dualsense.Icons.Charging0 : Resources.Dualsense.Icons.Discharging0;
                else if (batteryLevel <= 25)
                    return status.IsCharging() ? Resources.Dualsense.Icons.Charging25 : Resources.Dualsense.Icons.Discharging0;
                else if (batteryLevel <= 50)
                    return status.IsCharging() ? Resources.Dualsense.Icons.Charging50 : Resources.Dualsense.Icons.Discharging50;
                else if (batteryLevel <= 75)
                    return status.IsCharging() ? Resources.Dualsense.Icons.Charging75 : Resources.Dualsense.Icons.Discharging75;
                else
                    return status.IsCharging() ? Resources.Dualsense.Icons.Charging100 : Resources.Dualsense.Icons.Discharging100;
            }
            return Resources.Dualsense.Icons.Base;
        }
    }
}
