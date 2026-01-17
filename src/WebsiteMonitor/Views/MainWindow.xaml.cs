using System.Windows;
using System.Windows.Input;

namespace WebsiteMonitor.Views;

/// <summary>
/// Main popup window for displaying servers.
/// </summary>
public partial class MainWindow : Window
{
    private bool _stayOpen;
    private bool _isPinned;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void ShowAtTray()
    {
        if (!_isPinned)
        {
            // Position in bottom-right corner near tray
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 10;
            Top = workArea.Bottom - Height - 10;
        }

        Show();
        Activate();
    }

    public void KeepOpen()
    {
        _stayOpen = true;
    }

    public void AllowClose()
    {
        _stayOpen = false;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (_stayOpen || _isPinned)
        {
            return;
        }

        // Check if the newly focused window is one of our owned windows (like a dialog)
        foreach (Window ownedWindow in OwnedWindows)
        {
            if (ownedWindow.IsActive)
            {
                return;
            }
        }

        Hide();
    }

    private void PinButton_Click(object sender, RoutedEventArgs e)
    {
        _isPinned = !_isPinned;

        if (_isPinned)
        {
            // Pinned mode - window stays open and is draggable
            PinButton.Content = "📍";
            PinButton.ToolTip = "Unpin window (return to popup mode)";
            Topmost = true;
        }
        else
        {
            // Popup mode - window hides when clicking outside
            PinButton.Content = "📌";
            PinButton.ToolTip = "Pin window (keep open)";
            Topmost = true; // Keep topmost but will hide on deactivate

            // Reposition to tray location
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 10;
            Top = workArea.Bottom - Height - 10;
        }
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Allow dragging when pinned, or always allow dragging by header
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
