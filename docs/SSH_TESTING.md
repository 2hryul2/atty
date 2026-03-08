# SSH Usage and Test Guide

## Purpose

This document explains how to use the SSH feature in `Atty.Wpf` and how to test connection problems step by step.

## App Run

```powershell
cd D:\source\Atty
dotnet run --project .\Atty.Wpf\Atty.Wpf.csproj
```

## Basic SSH Usage

1. Launch the app.
2. In the configuration screen, enter:
   - `Host`
   - `Port`
   - `Username`
   - `Password`
3. Click `Open SSH` or `Open AI Terminal`.
4. Wait for the connection state to change to `Connected`.
5. In the terminal input area, type a command and press `Enter` or click `Send`.
6. Use `Disconnect` to close the session.

## Current Default Test Values

- Host: `127.0.0.1`
- Port: `22`
- Username: `sds`
- Password: enter manually in the app

## Recommended Test Commands

After connection, try these commands:

```text
whoami
pwd
hostname
ls -l
```

For Windows OpenSSH targets, try:

```text
whoami
hostname
pwd
dir
```

## Step-by-Step Connection Test

### 1. Check SSH server service

Windows:

```powershell
Get-Service sshd
```

Expected:
- service status should be `Running`

### 2. Check local SSH port

```powershell
Test-NetConnection localhost -Port 22
Test-NetConnection 127.0.0.1 -Port 22
```

Expected:
- `TcpTestSucceeded : True`

### 3. Check target host IP reachability

```powershell
Test-NetConnection <host> -Port 22
```

Examples:

```powershell
Test-NetConnection 127.0.0.1 -Port 22
Test-NetConnection 192.168.0.1 -Port 22
```

Interpretation:
- `True`: TCP path to SSH port is open
- `False`: network, firewall, address, or service binding issue exists before authentication

### 4. Test login in the app

If port `22` is reachable, test in the app with the same values.

Expected:
- status changes to `Connected`
- dashboard view opens
- terminal output appears

### 5. Test command execution

Run a simple command:

```text
whoami
```

Expected:
- remote shell returns the logged-in user name

## Failure Cases and Meaning

### `Connection failed to establish within 20000 milliseconds`

Meaning:
- server did not respond in time
- host may be wrong
- port `22` may be blocked
- firewall may be blocking traffic
- SSH service may not be reachable on that interface

### `TcpTestSucceeded : False`

Meaning:
- problem exists before username/password validation
- likely host, routing, firewall, or SSH port exposure issue

### Login fails even though port 22 is open

Meaning:
- SSH service is reachable
- issue is likely one of:
  - wrong username
  - wrong password
  - password login disabled on server
  - account login restrictions

## Troubleshooting Checklist

1. Confirm the host IP is correct.
2. Confirm the SSH service is running.
3. Confirm `Test-NetConnection <host> -Port 22` succeeds.
4. Confirm the username is correct.
5. Confirm the password is correct.
6. Confirm the server allows password-based SSH login.
7. Confirm local or server firewall rules are not blocking `22/tcp`.

## Notes

- The current app supports password-based SSH login only.
- Private key authentication is not implemented yet.
- The AI Assistant pane is a visual companion panel and does not yet execute real AI actions.
