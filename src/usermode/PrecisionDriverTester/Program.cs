using System;
using System.Windows.Forms;

namespace PrecisionDriverTester
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles for better appearance
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if running as administrator
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show(
                    "Questa applicazione deve essere eseguita come Amministratore per comunicare con il driver.\n\n" +
                    "Riavvia l'applicazione con privilegi di amministratore.",
                    "Privilegi Amministratore Richiesti",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                // You could also try to restart as admin here
                // RestartAsAdministrator();
                return;
            }

            Application.Run(new Form1());
        }

        private static bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private static void RestartAsAdministrator()
        {
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                    Verb = "runas"
                };

                System.Diagnostics.Process.Start(processInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossibile riavviare come amministratore: {ex.Message}",
                              "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}