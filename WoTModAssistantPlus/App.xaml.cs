using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WoTModAssistant
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Subscribe to the UnhandledException event
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Handle the unhandled exception here
            Exception exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                MessageBox.Show($"Unhandled Exception: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("An unknown unhandled exception occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // You might perform additional logging or cleanup here

            // Terminate the application (optional)
            Environment.Exit(1);
        }    
}
}
