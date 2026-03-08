using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Atty.Wpf;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private SshClient? _sshClient;
    private ShellStream? _shellStream;
    private CancellationTokenSource? _readLoopCts;
    private string _selectedCategory = "Session";
    private string _sessionName = "dev-server";
    private string _host = "127.0.0.1";
    private string _port = "22";
    private string _username = "sds";
    private string _password = string.Empty;
    private string _workspacePath = "/home/user/project";
    private string _terminalInput = string.Empty;
    private string _currentPromptText = "sds@127.0.0.1:~$";
    private string _connectionStatusText = "Disconnected";
    private Brush _connectionStatusBrush = Brushes.IndianRed;
    private string _serverDisplay = "Not connected";
    private string _userDisplay = "-";
    private bool _isSshMode;
    private bool _isAiCliMode;
    private bool _isHybridMode = true;
    private bool _showDashboard;
    private bool _isConnected;
    private bool _isBusy;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        Categories = new ObservableCollection<CategoryNode>
        {
            new("Session", true, new ObservableCollection<CategoryNode> { new("Logging") }),
            new("Terminal", true, new ObservableCollection<CategoryNode> { new("Keyboard"), new("Bell"), new("Features") }),
            new("Window", true, new ObservableCollection<CategoryNode> { new("Appearance"), new("Behavior"), new("Translation"), new("Selection"), new("Colours") }),
            new("Connection", true, new ObservableCollection<CategoryNode> { new("Data"), new("Proxy"), new("SSH", true, new ObservableCollection<CategoryNode> { new("Auth"), new("X11") }), new("Serial") }),
            new("AI CLI", true, new ObservableCollection<CategoryNode> { new("Provider"), new("Model"), new("Workspace"), new("Prompt"), new("Tools") })
        };

        SavedSessions = new ObservableCollection<string> { "Default", "Local AI", "Pair Code" };
        TerminalLines = new ObservableCollection<TerminalLineViewModel>();
        Closed += OnWindowClosed;
    }

    public ObservableCollection<CategoryNode> Categories { get; }
    public ObservableCollection<string> SavedSessions { get; }
    public ObservableCollection<TerminalLineViewModel> TerminalLines { get; }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetField(ref _selectedCategory, value))
            {
                OnPropertyChanged(nameof(IsSessionCategory));
                OnPropertyChanged(nameof(IsNotSessionCategory));
                OnPropertyChanged(nameof(CategoryTitle));
                OnPropertyChanged(nameof(GenericCategoryLines));
            }
        }
    }

    public string SessionName
    {
        get => _sessionName;
        set => SetField(ref _sessionName, value);
    }

    public string Host
    {
        get => _host;
        set
        {
            if (SetField(ref _host, value) && !IsConnected)
            {
                CurrentPromptText = $"{Username}@{Host}:~$";
            }
        }
    }

    public string Port
    {
        get => _port;
        set => SetField(ref _port, value);
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetField(ref _username, value) && !IsConnected)
            {
                CurrentPromptText = $"{Username}@{Host}:~$";
            }
        }
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public string WorkspacePath
    {
        get => _workspacePath;
        set => SetField(ref _workspacePath, value);
    }

    public bool IsSshMode
    {
        get => _isSshMode;
        set => SetField(ref _isSshMode, value);
    }

    public bool IsAiCliMode
    {
        get => _isAiCliMode;
        set => SetField(ref _isAiCliMode, value);
    }

    public bool IsHybridMode
    {
        get => _isHybridMode;
        set => SetField(ref _isHybridMode, value);
    }

    public bool ShowDashboard
    {
        get => _showDashboard;
        set
        {
            if (SetField(ref _showDashboard, value))
            {
                OnPropertyChanged(nameof(ShowConfigView));
            }
        }
    }

    public bool ShowConfigView => !ShowDashboard;
    public bool IsSessionCategory => SelectedCategory == "Session";
    public bool IsNotSessionCategory => !IsSessionCategory;
    public string CategoryTitle => SelectedCategory == "Session" ? "Basic options" : SelectedCategory;

    public bool IsConnected
    {
        get => _isConnected;
        set => SetField(ref _isConnected, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public string TerminalInput
    {
        get => _terminalInput;
        set => SetField(ref _terminalInput, value);
    }

    public string CurrentPromptText
    {
        get => _currentPromptText;
        set => SetField(ref _currentPromptText, value);
    }

    public string ConnectionStatusText
    {
        get => _connectionStatusText;
        set => SetField(ref _connectionStatusText, value);
    }

    public Brush ConnectionStatusBrush
    {
        get => _connectionStatusBrush;
        set => SetField(ref _connectionStatusBrush, value);
    }

    public string ServerDisplay
    {
        get => _serverDisplay;
        set => SetField(ref _serverDisplay, value);
    }

    public string UserDisplay
    {
        get => _userDisplay;
        set => SetField(ref _userDisplay, value);
    }

    public ObservableCollection<string> GenericCategoryLines => SelectedCategory switch
    {
        "Logging" => new ObservableCollection<string> { "Session log enabled", "SSH packet logging disabled", "Log file path configured" },
        "Terminal" => new ObservableCollection<string> { "Terminal-type string: xterm", "Local echo: Auto", "Local line editing: Auto" },
        "Keyboard" => new ObservableCollection<string> { "Backspace key sends Control-H", "Home/End standard handling", "Function keys: Xterm R6" },
        "Bell" => new ObservableCollection<string> { "Default alert sound", "Visual bell disabled", "Taskbar flash on bell" },
        "Features" => new ObservableCollection<string> { "Disable app keypad mode", "Background colour erase", "No remote title override" },
        "Appearance" => new ObservableCollection<string> { "Consolas 10", "Cursor: Block", "Window title inherits session" },
        "Behavior" => new ObservableCollection<string> { "Warn before closing", "Reset scrollback on keypress", "Resize terminal on drag" },
        "Translation" => new ObservableCollection<string> { "Remote charset UTF-8", "CJK ambiguous width enabled", "VT100 line drawing copy" },
        "Selection" => new ObservableCollection<string> { "Shift overrides mouse", "Ctrl+Alt replaces AltGr", "Clipboard plain text mode" },
        "Colours" => new ObservableCollection<string> { "ANSI colours enabled", "256-colour mode on", "Custom palette active" },
        "Data" => new ObservableCollection<string> { "Auto-login username set", "Terminal details string configured" },
        "Proxy" => new ObservableCollection<string> { "Proxy type none", "Hostname saved", "Port and credentials available" },
        "SSH" => new ObservableCollection<string> { "Compression enabled", "Pageant auth enabled", "Protocol version 2 only" },
        "Auth" => new ObservableCollection<string> { "Private key file configured", "Agent forwarding enabled" },
        "X11" => new ObservableCollection<string> { "X11 forwarding optional", "Display location configured" },
        "Serial" => new ObservableCollection<string> { "Serial line COM3", "Speed 9600", "Flow control XON/XOFF" },
        "AI CLI" => new ObservableCollection<string> { "AI CLI root settings", "Provider, model and workspace available" },
        "Provider" => new ObservableCollection<string> { "Provider: Local AI Engine", "Provider endpoint healthy" },
        "Model" => new ObservableCollection<string> { "Selected model: coder-large", "Fallback model available" },
        "Workspace" => new ObservableCollection<string> { "Workspace path mapped", "Browse integration available" },
        "Prompt" => new ObservableCollection<string> { "System prompt template configured", "Instruction injection rules available" },
        "Tools" => new ObservableCollection<string> { "Shell tool enabled", "Edit tool enabled", "Explain tool enabled" },
        _ => new ObservableCollection<string>()
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnCategorySelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is CategoryNode node)
        {
            SelectedCategory = node.Title;
        }
    }

    private async void OpenDashboard(object sender, RoutedEventArgs e)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ConnectionStatusText = "Connecting...";
        ConnectionStatusBrush = Brushes.DarkGoldenrod;

        try
        {
            await ConnectAsync();
            ShowDashboard = true;
        }
        catch (Exception ex)
        {
            CleanupConnection();
            ConnectionStatusText = "Connection failed";
            ConnectionStatusBrush = Brushes.IndianRed;
            var message = ex is SshOperationTimeoutException
                ? $"Host {Host}:{Port} did not respond within 20 seconds. Check server reachability, firewall rules, and credentials."
                : ex.Message;
            MessageBox.Show(this, message, "SSH connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowConfig(object sender, RoutedEventArgs e)
    {
        ShowDashboard = false;
        CleanupConnection();
    }

    private async Task ConnectAsync()
    {
        CleanupConnection();

        if (!int.TryParse(Port, out var port))
        {
            throw new InvalidOperationException("Port must be a valid number.");
        }

        if (string.IsNullOrWhiteSpace(Host) || string.IsNullOrWhiteSpace(Username))
        {
            throw new InvalidOperationException("Host and username are required.");
        }

        if (string.IsNullOrEmpty(Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        var connectionInfo = new PasswordConnectionInfo(Host, port, Username, Password)
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        _sshClient = new SshClient(connectionInfo);
        await Task.Run(() => _sshClient.Connect());

        if (!_sshClient.IsConnected)
        {
            throw new InvalidOperationException("SSH connection was not established.");
        }

        _shellStream = _sshClient.CreateShellStream("xterm", 120, 40, 1280, 720, 4096);
        _readLoopCts = new CancellationTokenSource();

        TerminalLines.Clear();
        ServerDisplay = Host;
        UserDisplay = Username;
        CurrentPromptText = $"{Username}@{Host}:~$";
        ConnectionStatusText = "Connected";
        ConnectionStatusBrush = Brushes.ForestGreen;
        IsConnected = true;

        _ = Task.Run(() => ReadShellLoopAsync(_readLoopCts.Token));
        await Task.Delay(200);
        _shellStream.WriteLine(string.Empty);
    }

    private async Task ReadShellLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new StringBuilder();

        while (!cancellationToken.IsCancellationRequested && _shellStream is not null && _sshClient?.IsConnected == true)
        {
            try
            {
                if (!_shellStream.DataAvailable)
                {
                    await Task.Delay(75, cancellationToken);
                    continue;
                }

                var chunk = _shellStream.Read();
                if (string.IsNullOrEmpty(chunk))
                {
                    await Task.Delay(50, cancellationToken);
                    continue;
                }

                chunk = chunk.Replace("\r\n", "\n").Replace('\r', '\n');
                buffer.Append(chunk);

                while (true)
                {
                    var content = buffer.ToString();
                    var newLineIndex = content.IndexOf('\n');
                    if (newLineIndex < 0)
                    {
                        break;
                    }

                    var line = content[..newLineIndex];
                    buffer.Remove(0, newLineIndex + 1);
                    AppendTerminalLine(line);
                }

                var tail = buffer.ToString();
                if (!string.IsNullOrWhiteSpace(tail) && (tail.Contains(":~$") || tail.EndsWith("$")))
                {
                    await Dispatcher.InvokeAsync(() => CurrentPromptText = tail.TrimEnd());
                    buffer.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SshConnectionException ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    AppendTerminalLine($"Connection lost: {ex.Message}");
                    ConnectionStatusText = "Disconnected";
                    ConnectionStatusBrush = Brushes.IndianRed;
                    IsConnected = false;
                });
                break;
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => AppendTerminalLine($"Read error: {ex.Message}"));
                break;
            }
        }
    }

    private void AppendTerminalLine(string text)
    {
        Dispatcher.Invoke(() =>
        {
            TerminalLines.Add(new TerminalLineViewModel(text));
        });
    }

    private void SendTerminalCommand(object sender, RoutedEventArgs e)
    {
        SendCurrentCommand();
    }

    private void TerminalInputKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SendCurrentCommand();
            e.Handled = true;
        }
    }

    private void SendCurrentCommand()
    {
        if (!IsConnected || _shellStream is null)
        {
            return;
        }

        _shellStream.WriteLine(TerminalInput);
        TerminalInput = string.Empty;
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            Password = passwordBox.Password;
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        CleanupConnection();
    }

    private void CleanupConnection()
    {
        try
        {
            _readLoopCts?.Cancel();
            _shellStream?.Dispose();

            if (_sshClient?.IsConnected == true)
            {
                _sshClient.Disconnect();
            }

            _sshClient?.Dispose();
        }
        catch
        {
        }
        finally
        {
            _readLoopCts = null;
            _shellStream = null;
            _sshClient = null;
            IsConnected = false;
            CurrentPromptText = $"{Username}@{Host}:~$";
            ServerDisplay = "Not connected";
            UserDisplay = "-";
            ConnectionStatusText = "Disconnected";
            ConnectionStatusBrush = Brushes.IndianRed;
        }
    }
}

public class CategoryNode
{
    public CategoryNode(string title, bool isExpanded = false, ObservableCollection<CategoryNode>? children = null)
    {
        Title = title;
        IsExpanded = isExpanded;
        Children = children ?? new ObservableCollection<CategoryNode>();
    }

    public string Title { get; }
    public bool IsExpanded { get; set; }
    public ObservableCollection<CategoryNode> Children { get; }
}

public class TerminalLineViewModel
{
    public TerminalLineViewModel(string text)
    {
        Text = text;
    }

    public string Text { get; }
    public bool IsCommand => Text.Contains(":~$");
    public bool IsSecondary => Text.StartsWith("12:34:56 up");
}
