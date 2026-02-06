using HidSharp;

namespace DSBar
{
    internal class DSBarApp : ApplicationContext
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new DSBarApp());
        }

        private System.Windows.Forms.Timer rescanTimer;
        private const int SCAN_INTERVAL = 2000;

        private Dictionary<string, BaseDevice> devices = new();
        private readonly NotifyIcon mainIcon;
        private ContextMenuStrip mainMenu;

        private Thread worker;
        private static AutoResetEvent workerEvent = new(true);

        public DSBarApp()
        {
            mainMenu = new ContextMenuStrip();
            mainMenu.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication()));

            mainIcon = new()
            {
                Icon = Resources.GetIcon("DSBar.Resources.icon.ico"),
                ContextMenuStrip = mainMenu,
                Text = "DSBar",
                Visible = true
            };

            rescanTimer = new System.Windows.Forms.Timer();
            rescanTimer.Interval = SCAN_INTERVAL;
            rescanTimer.Tick += (s, e) => workerEvent.Set();
            rescanTimer.Start();

            worker = new Thread(DeviceLookup);
            worker.Start();
        }

        private void DeviceLookup()
        {
            try
            {
                while (true)
                {
                    workerEvent.WaitOne();

                    var dualsenses = DualSenseDevice.ScanForDevices(DeviceList.Local);
                    var toRemove = new HashSet<string>();

                    foreach (var device in devices)
                    {
                        toRemove.Add(device.Key);
                    }

                    foreach (var dualsense in dualsenses)
                    {
                        var sn = dualsense.GetSerialNumber();
                        if (devices.ContainsKey(sn))
                        {
                            toRemove.Remove(sn);
                        }
                        else
                        {
                            devices[sn] = new DualSenseDevice(dualsense);
                        }
                    }

                    foreach (var removedKey in toRemove)
                    {
                        devices[removedKey].Dispose();
                        devices.Remove(removedKey);
                    }

                    foreach (var device in devices)
                    {
                        device.Value.Update();
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
        }

        private void ExitApplication()
        {
            mainIcon.Dispose();
            worker.Interrupt();
            worker.Join();
            Application.Exit();
        }
    }
}
