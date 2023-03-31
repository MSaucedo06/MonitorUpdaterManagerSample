namespace Sample
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using log4net;
    using log4net.Repository.Hierarchy;

    public static class MonitorUpdaterManagerSample
    {
        #region Constantes
        private const string MonitorServiceNameKey = "monitorsk";
        public const string UpdaterMonitorInstallationFolder = "monSelfUpdater";
        const string MonitorUpdatesPath = "/tmp";
        public const string UpdaterMonitorFolder = "actualizaciones";
        public static string InstalledRollbackFilesPath = "/tmp";
        public static string monitorFilesLocation = System.IO.Directory.GetCurrentDirectory() + "\\files\\";
        public static string installationFolder = System.IO.Directory.GetCurrentDirectory() + "\\installation\\";
        public static string version = "1.0";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        #endregion

        #region Metodos

        static void Main() 
        {
            UpdateMonitor(monitorFilesLocation, installationFolder, version);
        }
        public static void UpdateMonitor(string monitorFilesLocation, string installationFolder, string version)
        {
            try
            {
                var winServiceManager = new WindowsServiceManager();

                Log.Info("Iniciando las actualizaciones al monitor...");
                Console.WriteLine("Iniciando las actualizaciones al monitor...");

                try
                {
                    Process[] processes = Process.GetProcessesByName("opera");

                    if (processes.Any())
                    {
                        Log.Info("Cerrando el monitor de actualizaciones...");
                        Console.WriteLine("Cerrando el monitor de actualizaciones...");
                        foreach (var proc in processes)
                        {
                            proc.Kill();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Ocurrió un error al intentar terminar el proceso del monitor de actualizaciones.", ex);
                    Console.WriteLine("Ocurrió un error al intentar terminar el proceso del monitor de actualizaciones.");
                }

                var backupPath = System.IO.Path.Combine(MonitorUpdatesPath, "Backup", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6));
                if (!System.IO.Directory.Exists(backupPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(backupPath);
                    }
                    catch
                    {
                        backupPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), UpdaterMonitorFolder, "Backup", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6));
                        if (!System.IO.Directory.Exists(backupPath))
                        {
                            System.IO.Directory.CreateDirectory(backupPath);
                        }
                    }
                }

                var fileManager = new FileManagerSample();

                var result = fileManager.UpdateFiles(monitorFilesLocation.Trim(new char[] { '"' }), installationFolder.Trim(new char[] { '"' }), backupPath);
                bool updateError = false;

                if (!string.IsNullOrEmpty(result))
                {
                    updateError = true;
                    Log.Error(result);

                    result = null;
                }
                if (updateError)
                {
                    Log.Info("Realizando rollback de las actualizaciones al monitor...");
                    Console.WriteLine("Realizando rollback de las actualizaciones al monitor...");

                    result = fileManager.UpdateFiles(backupPath, installationFolder);
                    fileManager.RemoveDirectoryContents(backupPath);

                    if (!string.IsNullOrEmpty(result))
                    {
                        Exception e = new Exception(result);
                        Log.Info("MonitorUpdater", e);
                        Console.WriteLine("MonitorUpdater");
                    }
                    else
                    {
                        Log.Info("Terminado rollback de las actualizaciones al monitor...");
                        Console.WriteLine("Terminado rollback de las actualizaciones al monitor...");
                    }
                    return;
                }

                fileManager.RemoveDirectoryContents(backupPath);
                fileManager.RemoveDirectoryContents(monitorFilesLocation.Trim(new char[] { '"' }));
                System.IO.Directory.Delete(backupPath, true);
                System.IO.Directory.Delete(monitorFilesLocation.Trim(new char[] { '"' }), true);

                ReleaseUpdateMonitorTask();
                Log.Info("Actualizaciones al monitor terminadas...");
                Console.WriteLine("Actualizaciones al monitor terminadas...");
            }
            catch (Exception ex)
            {
                Log.Error("Ocurrió un error durante el proceso de actualización del Monitor.", ex);
                Console.WriteLine("Ocurrió un error durante el proceso de actualización del Monitor.");
            }
        }

        private static void ReleaseUpdateMonitorTask()
        {
            // elimina la tarea
        }
        #endregion
    }
}
