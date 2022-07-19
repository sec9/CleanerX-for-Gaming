using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Text;

namespace CleanerX
{
    class Functions
    {
        public static void killProcesses()
        {
            ServiceController controller;
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
        }
    }
}
