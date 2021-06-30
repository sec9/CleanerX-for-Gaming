using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Windows.Forms;

namespace CleanerX
{
    public partial class Main : MetroFramework.Forms.MetroForm
    {
        [DllImport("psapi")]
        public static extern int EmptyWorkingSet(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string methodName);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [SecurityCritical]
        internal static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool IsWow64Process([In] IntPtr hSourceProcessHandle, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);

        [SecuritySafeCritical]
        public static bool get_Is64BitOperatingSystem()
        {
            bool flag;
            return (IntPtr.Size == 8) ||
                ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                IsWow64Process(GetCurrentProcess(), out flag)) && flag);
        }
        public Main()
        {
            InitializeComponent();
            if (Process.GetProcessesByName("CleanerX").Length > 1) Environment.Exit(0);
            RegistryKey key;
            ServiceController controller;
            key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\CleanerXGaming", true);
            if (key == null)
            {
                Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\CleanerXGaming");
                key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\CleanerXGaming", true);
                key.SetValue("RAM", "60");
                Globals.RAMPercent = 59;
                key.SetValue("timer", "60");
                Globals.timerinterval = 60000;
            }
            else
            {
                Globals.RAMPercent = Convert.ToInt32(key.GetValue("RAM")) - 1;
                Globals.timerinterval = Convert.ToInt32(key.GetValue("timer")) * 1000;
            }
            timer1.Interval = Globals.timerinterval;
            timer1.Start();
            key.Close();
            if (get_Is64BitOperatingSystem())
            {
                try
                {
                    key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    key.SetValue("CleanerX", "\"" + Application.ExecutablePath + "\"");
                    key.Close();
                }
                catch { }
            }
            else
            {
                try
                {
                    key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    key.SetValue("CleanerX", "\"" + Application.ExecutablePath + "\"");
                    key.Close();
                }
                catch { }
            }
            try
            {
                controller = new ServiceController("XblAuthManager");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("XblGameSave");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("XboxGip");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("XboxGipSvc");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("XboxNetApiSvc");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("edgeupdate");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("edgeupdatem");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("wuauserv");
                controller.Stop();
            }
            catch { }
            try
            {
                controller = new ServiceController("WaaSMedicSvc");
                controller.Stop();
            }
            catch { }
            DirectoryInfo directory = new DirectoryInfo(Path.GetTempPath());
            foreach (FileInfo file in directory.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch { }
            }
            directory = new DirectoryInfo(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine));
            foreach (FileInfo file in directory.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch { }
            }
            if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Int64 phav = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            Int64 tot = PerformanceInfo.GetTotalMemoryInMiB();
            decimal percentFree = ((decimal)phav / (decimal)tot) * 100;
            decimal percentOccupied = 100 - percentFree;
            if (Math.Round(percentOccupied) > Globals.RAMPercent)
            {
                Process[] process = Process.GetProcesses();
                foreach (Process p in process) try { EmptyWorkingSet(p.Handle); } catch { }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\CleanerXGaming", true);
            metroComboBox1.SelectedIndex = metroComboBox1.FindStringExact(key.GetValue("RAM") + "%");
            metroComboBox2.SelectedIndex = metroComboBox2.FindStringExact(key.GetValue("timer") + "");
            key.Close();
            GraphUpdate();
            this.WindowState = FormWindowState.Normal;
            this.Show();
            notifyIcon1.ShowBalloonTip(3000);
            notifyIcon1.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon1.ContextMenuStrip.Items.Add("Show", null, this.show_Click);
            notifyIcon1.ContextMenuStrip.Items.Add("Exit", null, this.exit_Click);
        }

        void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        void show_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\CleanerXGaming", true);
            switch (metroComboBox1.SelectedIndex)
            {
                case 0:
                    key.SetValue("RAM", "20");
                    break;
                case 1:
                    key.SetValue("RAM", "25");
                    break;
                case 2:
                    key.SetValue("RAM", "30");
                    break;
                case 3:
                    key.SetValue("RAM", "35");
                    break;
                case 4:
                    key.SetValue("RAM", "40");
                    break;
                case 5:
                    key.SetValue("RAM", "45");
                    break;
                case 6:
                    key.SetValue("RAM", "50");
                    break;
                case 7:
                    key.SetValue("RAM", "55");
                    break;
                case 8:
                    key.SetValue("RAM", "60");
                    break;
                case 9:
                    key.SetValue("RAM", "65");
                    break;
                case 10:
                    key.SetValue("RAM", "70");
                    break;
                case 11:
                    key.SetValue("RAM", "75");
                    break;
                case 12:
                    key.SetValue("RAM", "80");
                    break;
                case 13:
                    key.SetValue("RAM", "85");
                    break;
                case 14:
                    key.SetValue("RAM", "90");
                    break;
            }
            Globals.RAMPercent = Convert.ToInt32(key.GetValue("RAM")) - 1;
            key.Close();
        }

        private void metroComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\CleanerXGaming", true);
            switch (metroComboBox2.SelectedIndex)
            {
                case 0:
                    key.SetValue("timer", "15");
                    break;
                case 1:
                    key.SetValue("timer", "30");
                    break;
                case 2:
                    key.SetValue("timer", "45");
                    break;
                case 3:
                    key.SetValue("timer", "60");
                    break;
                case 4:
                    key.SetValue("timer", "75");
                    break;
                case 5:
                    key.SetValue("timer", "90");
                    break;
                case 6:
                    key.SetValue("timer", "120");
                    break;
                case 7:
                    key.SetValue("timer", "180");
                    break;
                case 8:
                    key.SetValue("timer", "300");
                    break;
                case 9:
                    key.SetValue("timer", "600");
                    break;
            }
            Globals.timerinterval = Convert.ToInt32(key.GetValue("timer")) * 1000;
            timer1.Interval = Globals.timerinterval;
            key.Close();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            GraphUpdate();
            if (metroButton1.Enabled == false) metroButton1.Enabled = true;
        }

        private void GraphUpdate()
        {
            Int64 phav = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            Int64 tot = PerformanceInfo.GetTotalMemoryInMiB();
            decimal percentFree = ((decimal)phav / (decimal)tot) * 100;
            decimal percentOccupied = 100 - percentFree;
            metroProgressBar1.Value = Convert.ToInt32(Math.Round(percentOccupied));
            metroLabel3.Text = "RAM usage (" + Convert.ToInt32(Math.Round(percentOccupied)) + "%)";
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
                DirectoryInfo directory = new DirectoryInfo(Path.GetTempPath());
                foreach (FileInfo file in directory.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch { }
                }
                directory = new DirectoryInfo(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine));
                foreach (FileInfo file in directory.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch { }
                }
            }
            metroButton1.Enabled = false;
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Minimized)
            {
                timer2.Stop();
                this.Hide();
                this.ShowInTaskbar = false;
                notifyIcon1.BalloonTipText = "Minimized";
                notifyIcon1.ShowBalloonTip(3000);
            }
            else
            {
                this.ShowInTaskbar = true;
                GraphUpdate();
                timer2.Start();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
    public static class PerformanceInfo
    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }
        }

        public static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }
        }
    }
}
