using App.ViewModels;
using App.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace App
{
    public class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        public static Window CurrentWindow { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                XmlDocument log4netConfig = new XmlDocument();
                log4netConfig.Load(File.OpenRead("log4net.config"));
                var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                           typeof(Hierarchy));
                XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                CurrentWindow = desktop.MainWindow;
                AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("Unhandled error occured.", e.ExceptionObject as Exception);
        }
    }
}