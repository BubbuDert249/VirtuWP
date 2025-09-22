using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace VirtuWP
{
    public partial class App : Application
    {
        public PhoneApplicationFrame RootFrame { get; private set; }

        public App()
        {
            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Global exception handler
            UnhandledException += Application_UnhandledException;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Start the main page
            RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void Application_Exit(object sender, EventArgs e)
        {
            // Handle app exit logic if needed
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // Display message box for debugging
            MessageBox.Show("Unhandled exception: " + e.ExceptionObject.Message);
            e.Handled = true;
        }

        #region Phone application initialization

        private bool phoneApplicationInitialized = false;

        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            phoneApplicationInitialized = true;
        }

        private void CompleteInitializePhoneApplication(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            RootVisual = RootFrame;
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}
