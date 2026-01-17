using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace WebsiteMonitor.Helpers;

/// <summary>
/// Helper class to generate the tray icon dynamically.
/// </summary>
public static class IconGenerator
{
    /// <summary>
    /// Creates a simple server monitor icon.
    /// </summary>
    public static Icon CreateTrayIcon()
    {
        const int size = 32;

        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        // Draw a simple server/monitor shape
        using var fillBrush = new SolidBrush(Color.FromArgb(59, 130, 246)); // Blue
        using var outlinePen = new Pen(Color.FromArgb(30, 64, 175), 2);

        // Main circle
        graphics.FillEllipse(fillBrush, 2, 2, size - 4, size - 4);
        graphics.DrawEllipse(outlinePen, 2, 2, size - 4, size - 4);

        // Inner signal bars
        using var whiteBrush = new SolidBrush(Color.White);

        // Three signal bars
        graphics.FillRectangle(whiteBrush, 9, 18, 3, 6);
        graphics.FillRectangle(whiteBrush, 14, 14, 3, 10);
        graphics.FillRectangle(whiteBrush, 19, 10, 3, 14);

        // Convert bitmap to icon
        IntPtr hIcon = bitmap.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    /// <summary>
    /// Ensures the icon file exists in the resources folder.
    /// </summary>
    public static void EnsureIconExists(string iconPath)
    {
        if (File.Exists(iconPath))
            return;

        var directory = Path.GetDirectoryName(iconPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var icon = CreateTrayIcon();
        using var stream = new FileStream(iconPath, FileMode.Create);
        icon.Save(stream);
    }
}
