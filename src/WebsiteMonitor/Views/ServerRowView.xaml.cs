using System.Windows;
using WebsiteMonitor.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace WebsiteMonitor.Views;

/// <summary>
/// Interaction logic for ServerRowView.xaml
/// </summary>
public partial class ServerRowView : UserControl
{
    public ServerRowView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Bind the group menu items
        if (DataContext is ServerRowViewModel vm)
        {
            AssignToGroupMenuItem.ItemsSource = vm.AvailableGroups;
        }

        // Keep window open when context menu is open
        if (ContextMenu != null)
        {
            ContextMenu.Opened += ContextMenu_Opened;
            ContextMenu.Closed += ContextMenu_Closed;
        }
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        GetMainWindow()?.KeepOpen();
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        // Don't allow close here - let the menu item handlers decide
        // This prevents the window from closing between menu close and dialog open
    }

    private MainWindow? GetMainWindow()
    {
        return Window.GetWindow(this) as MainWindow;
    }

    private void NewGroupMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ServerRowViewModel vm) return;

        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;

        // Keep the main window open
        mainWindow.KeepOpen();

        var dialog = new NewGroupDialog();
        dialog.Owner = mainWindow;

        // When dialog closes, handle the result and then allow main window to close on deactivate
        var result = dialog.ShowDialog();

        if (result == true && !string.IsNullOrWhiteSpace(dialog.GroupName))
        {
            vm.CreateAndAssignToGroup(dialog.GroupName);
            // Refresh the submenu items
            AssignToGroupMenuItem.ItemsSource = null;
            AssignToGroupMenuItem.ItemsSource = vm.AvailableGroups;
        }

        // Re-activate main window and allow it to close on deactivate again
        mainWindow.Activate();
        mainWindow.AllowClose();
    }
}
