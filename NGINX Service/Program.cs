using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace Codeology.NGINX
{

    public enum InstallCommand
    {
        None,
        Install,
        Uninstall
    }

    static class Program
    {

        static InstallCommand command;
        static bool silent;

        static void Main(string[] args)
        {
            command = InstallCommand.None;
            silent = false;

            foreach(string arg in args) {
                if (arg.ToLower() == "/silent") {
                    silent = true;
                } else if (arg.ToLower() == "/install") {
                    command = InstallCommand.Install;
                } else if (arg.ToLower() == "/uninstall") {
                    command = InstallCommand.Uninstall;
                }
            }

            if (!Environment.UserInteractive) silent = true;

            switch (command) {
                case InstallCommand.Install:    Install();
                                                break;
                case InstallCommand.Uninstall:  Uninstall();
                                                break;
                default:                        ServiceBase.Run(new ServiceBase[] {new Service()});
                                                break;
            }
        }

        static void Install()
        {
            try {
                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });

                if (!silent) MessageBox.Show("Service was installed successfully.","Service Installed",MessageBoxButtons.OK,MessageBoxIcon.Information);
            } catch (Exception e) {
                if (!silent) MessageBox.Show("There was an exception installing the service:\r\n\r\n" + e.Message,"Service Install Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        static void Uninstall()
        {
            try {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });

                if (!silent) MessageBox.Show("Service was uninstalled successfully.","Service Uninstalled",MessageBoxButtons.OK,MessageBoxIcon.Information);
            } catch (Exception e) {
                if (!silent) MessageBox.Show("There was an exception uninstalling the service:\r\n\r\n" + e.Message,"Service Uninstall Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

    }

}
