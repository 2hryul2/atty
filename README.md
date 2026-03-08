# atty

`atty` is a WPF-based Windows SSH client prototype inspired by PuTTY configuration flows and a dual-pane SSH + AI dashboard UI.

## Overview

This repository contains a `.NET 8 WPF` desktop application with:

- a PuTTY-style configuration screen
- a dashboard-style terminal screen
- real SSH password-based connection support via `SSH.NET`
- a terminal command input area that sends commands to the connected SSH shell
- a simulated AI Assistant panel for visual workflow demonstration

## Tech Stack

- `.NET 8`
- `WPF`
- [`SSH.NET`](https://github.com/sshnet/SSH.NET)

## Project Structure

```text
Atty.Wpf/
  App.xaml
  App.xaml.cs
  MainWindow.xaml
  MainWindow.xaml.cs
  Atty.Wpf.csproj
AGENTS.md
.gitignore
```

## Prerequisites

- Windows
- `.NET 8 SDK`
- OpenSSH-accessible target server

## Run

```powershell
cd D:\source\Atty
dotnet run --project .\Atty.Wpf\Atty.Wpf.csproj
```

## Build

Debug build:

```powershell
cd D:\source\Atty
dotnet build .\Atty.Wpf\Atty.Wpf.csproj
```

Important:

- Before any release or deployment build, explicit user confirmation is required.
- Do not run release/distribution packaging without approval.

## SSH Test Defaults

Current default connection values in the app:

- Host: `127.0.0.1`
- Port: `22`
- Username: `sds`

Password is entered manually in the app.

## Usage

1. Launch the app.
2. Enter SSH credentials on the configuration screen.
3. Click `Open SSH` or `Open AI Terminal`.
4. After connection, use the lower terminal input area to send commands.
5. Click `Disconnect` to close the SSH session.

## Documentation

- [SSH Usage and Test Guide](docs/SSH_TESTING.md)
- [SSH 사용 및 테스트 가이드](docs/SSH_TESTING.ko.md)

## Notes

- Current implementation supports password-based SSH login.
- The AI Assistant pane is UI-only and does not execute model calls.
- If `127.0.0.1:22` works but a LAN IP does not, check Windows Firewall and interface binding.

## Future Improvements

- private key authentication
- ANSI color rendering
- SFTP/file browser integration
- real AI assistant integration with terminal output summarization
