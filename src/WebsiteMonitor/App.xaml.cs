using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using WebsiteMonitor.Helpers;
using WebsiteMonitor.Services;
using WebsiteMonitor.Services.Interfaces;
using WebsiteMonitor.ViewModels;
using WebsiteMonitor.Views;
using Application = System.Windows.Application;

namespace WebsiteMonitor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private ServiceProvider? _serviceProvider;
    private System.Drawing.Icon? _generatedIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Create the main window
        _mainWindow = new MainWindow();
        _mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();

        // Get the tray icon and configure it
        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");

        // Generate and set the tray icon
        _generatedIcon = IconGenerator.CreateTrayIcon();
        _trayIcon.Icon = _generatedIcon;

        // Handle tray icon click
        _trayIcon.TrayLeftMouseUp += TrayIcon_TrayLeftMouseUp;
    }

    private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
    {
        if (_mainWindow == null) return;

        if (_mainWindow.IsVisible)
        {
            _mainWindow.Hide();
        }
        else
        {
            _mainWindow.ShowAtTray();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IPortDetectionService, PortDetectionService>();
        services.AddSingleton<IProcessMonitorService, ProcessMonitorService>();
        services.AddSingleton<IProcessTerminationService, ProcessTerminationService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mainWindow?.Close();
        _trayIcon?.Dispose();
        _generatedIcon?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
