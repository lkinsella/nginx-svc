using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;

namespace Codeology.NGINX
{

    partial class Service : ServiceBase
    {

        public Service()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
        }

        protected override void OnStart(string[] args)
        {
            // Start background worker
            backgroundWorker.RunWorkerAsync();
        }

        protected override void OnStop()
        {
            // Get NGINX path from configuration
            string path;

            try {
                path = ConfigurationManager.AppSettings["nginxPath"];

                if (String.IsNullOrEmpty(path)) throw new Exception();
            } catch {
                // Write error to event log
                EventLog.WriteEntry("Could not get NGINX installation path, service will stop.",EventLogEntryType.Error);

                // Return
                return;
            }

            // Combine NGINX path and filename
            string filename = Path.Combine(path,"nginx.exe");

            // Stop the NGINX process by using another process to signal stop
            Process process = new Process();

            process.StartInfo.FileName = filename;
            process.StartInfo.WorkingDirectory = path;
            process.StartInfo.Arguments = "-s stop";
            process.Start();

            try {
                // Wait for process to finish
                process.WaitForExit();
            } finally {
                // Close process
                process.Close();
            }
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Check for exception object
            if (e.ExceptionObject == null) return;

            // Get exception object
            Exception ex = (Exception)e.ExceptionObject;

            // Write error to event log
            EventLog.WriteEntry(ex.Message,EventLogEntryType.Error);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get NGINX path from configuration
            string path;

            try {
                path = ConfigurationManager.AppSettings["nginxPath"];

                if (String.IsNullOrEmpty(path)) throw new Exception();
            } catch {
                // Write error to event log
                EventLog.WriteEntry("Could not get NGINX installation path, service will stop.",EventLogEntryType.Error);

                // Stop service
                Stop();

                // Return
                return;
            }

            // Combine NGINX path and filename
            string filename = Path.Combine(path,"nginx.exe");

            // Start the NGINX process
            Process process = new Process();

            process.StartInfo.FileName = filename;
            process.StartInfo.WorkingDirectory = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            try {
                // Get process output
                string output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            } finally {
                // Close process
                process.Close();
            }
        }

    }

}
