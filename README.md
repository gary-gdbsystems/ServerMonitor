# ServerMonitor

A Windows system tray application for monitoring and managing local development servers.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

- **Auto-detection** - Automatically discovers running development servers (node.exe, dotnet.exe, and custom processes) and their listening ports
- **Graceful shutdown** - Sends Ctrl+C signal before force killing, allowing servers to clean up properly
- **Start/Stop/Remove** - Full control over server lifecycle with persistent state
- **Server grouping** - Organize servers into named groups for better organization
- **Pin mode** - Keep the popup window open and drag it anywhere on screen
- **Dark theme** - Modern "Terminal Luxe" dark UI with glowing status indicators

## Screenshots

The application runs in the system tray and shows a popup when clicked:

- Green indicator = server running
- Gray indicator = server stopped
- Group labels displayed as colored badges
- STOP button for running servers
- START/REMOVE buttons for stopped servers

## Requirements

- Windows 10/11
- .NET 8.0 Runtime

## Installation

### Build from source

```bash
git clone https://github.com/gary-gdbsystems/ServerMonitor.git
cd ServerMonitor
dotnet build
dotnet run --project src/WebsiteMonitor
```

### Run the built executable

After building, run:
```
src\WebsiteMonitor\bin\Debug\net8.0-windows\WebsiteMonitor.exe
```

## Usage

1. **Launch** - The app starts minimized to the system tray
2. **Click tray icon** - Opens the server list popup
3. **Stop a server** - Click the STOP button to gracefully terminate
4. **Start a server** - Click START to restart a previously stopped server
5. **Remove a server** - Click REMOVE to remove a stopped server from the list
6. **Group servers** - Right-click a server → "Assign to Group" or "New Group..."
7. **Pin the window** - Click the 📌 button to keep the window open and draggable

## Configuration

Configuration is stored in `%APPDATA%\WebsiteMonitor\config.json`:

```json
{
  "groups": [...],
  "assignments": [...],
  "monitoredProcessNames": ["node", "dotnet", "Ripples.Api"],
  "rememberedServers": [...]
}
```

### Adding custom process names

To monitor additional processes, add their names (without .exe) to the `monitoredProcessNames` array in the config file.

## Architecture

- **Framework**: .NET 8.0 WPF
- **MVVM**: CommunityToolkit.Mvvm
- **System Tray**: Hardcodet.NotifyIcon.Wpf
- **DI**: Microsoft.Extensions.DependencyInjection

### Key Components

| Component | Description |
|-----------|-------------|
| `ProcessMonitorService` | Scans for running processes every 2 seconds |
| `PortDetectionService` | Uses Windows API (GetExtendedTcpTable) to detect listening ports |
| `ProcessTerminationService` | Handles graceful shutdown with Ctrl+C signal |
| `ConfigurationService` | Persists groups and server state to JSON |

## License

MIT License - feel free to use and modify as needed.
