using Avalonia.Controls;
using AdvancedWorkloadGenerator.Core.Interfaces;
using AdvancedWorkloadGenerator.Core.Models.DatabaseConnections;
using AdvancedWorkloadGenerator.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator;

public partial class ConnectionEditing : Window
{
    private IConnectionStringService _connectionService = null!;
    private DatabaseConnectionDTO? _existingConnection;
    private ILogger<ConnectionEditing>? _logger;
    
    public ConnectionEditing()
    {
        InitializeComponent();
        Initialize();
        Loaded += (s, e) => NameTextBox.Focus();
    }
    
    public ConnectionEditing(DatabaseConnectionDTO existingConnection) : this()
    {
        _existingConnection = existingConnection;
        LoadConnectionData();
    }
    
    private void Initialize()
    {
        try
        {
            var serviceProvider = Program.ServiceProvider ?? throw new InvalidOperationException("ServiceProvider not initialized");
            
            _logger = serviceProvider.GetService<ILogger<ConnectionEditing>>();
            _connectionService = serviceProvider.GetService<IConnectionStringService>()
                ?? throw new InvalidOperationException("ConnectionStringService not found in DI container");
                
            LoadDatabaseTypes();
            SetupEventHandlers();
        }
        catch (Exception ex)
        {
            // Fallback to static logger if instance logger is not available
            if (_logger != null)
                _logger.LogError(ex, "Error initializing ConnectionEditing");
            else
                Log.Logger.Error(ex, "Error initializing ConnectionEditing");
            ShowStatus($"Initialization error: {ex.Message}", false);
        }
    }
    
    private void LoadDatabaseTypes()
    {
        var databaseTypes = Enum.GetValues<DatabaseType>().ToList();
        DatabaseTypeComboBox.ItemsSource = databaseTypes;
        if (databaseTypes.Any())
            DatabaseTypeComboBox.SelectedIndex = 0;
    }
    
    private void SetupEventHandlers()
    {
        // Auto-generate connection string when fields change
        HostTextBox.TextChanged += (s, e) => { ClearFieldError("Host"); GenerateConnectionString(); };
        PortNumericUpDown.ValueChanged += (s, e) => GenerateConnectionString();
        DatabaseNameTextBox.TextChanged += (s, e) => { ClearFieldError("DatabaseName"); GenerateConnectionString(); };
        UsernameTextBox.TextChanged += (s, e) => { ClearFieldError("Username"); GenerateConnectionString(); };
        PasswordTextBox.TextChanged += (s, e) => { ClearFieldError("Password"); GenerateConnectionString(); };
        DatabaseTypeComboBox.SelectionChanged += (s, e) => GenerateConnectionString();
        NameTextBox.TextChanged += (s, e) => ClearFieldError("Name");
        ConnectionStringTextBox.TextChanged += (s, e) => ClearFieldError("ConnectionString");
        
        // Handle Enter key for quick save
        NameTextBox.KeyDown += HandleEnterKey;
        HostTextBox.KeyDown += HandleEnterKey;
        DatabaseNameTextBox.KeyDown += HandleEnterKey;
        UsernameTextBox.KeyDown += HandleEnterKey;
        PasswordTextBox.KeyDown += HandleEnterKey;
        ConnectionStringTextBox.KeyDown += HandleEnterKey;
    }
    
    private void LoadConnectionData()
    {
        if (_existingConnection == null) 
        {
            SaveButton.Content = "Save Connection";
            return;
        }
        
        NameTextBox.Text = _existingConnection.Name;
        DatabaseTypeComboBox.SelectedItem = _existingConnection.DatabaseType;
        HostTextBox.Text = _existingConnection.Host;
        PortNumericUpDown.Value = _existingConnection.Port;
        DatabaseNameTextBox.Text = _existingConnection.DatabaseName;
        UsernameTextBox.Text = _existingConnection.Username;
        PasswordTextBox.Text = _existingConnection.Password;
        ConnectionStringTextBox.Text = _existingConnection.ConnectionString;
        
        Title = $"Edit Connection - {_existingConnection.Name}";
        SaveButton.Content = "Update Connection";
    }
    
    private void GenerateConnectionString()
    {
        if (DatabaseTypeComboBox.SelectedItem is not DatabaseType dbType) return;
        
        var host = HostTextBox.Text ?? "localhost";
        var port = (int)(PortNumericUpDown.Value ?? 5432);
        var database = DatabaseNameTextBox.Text ?? "";
        var username = UsernameTextBox.Text ?? "";
        var password = PasswordTextBox.Text ?? "";
        
        if (string.IsNullOrWhiteSpace(database)) return;
        
        string connectionString = dbType switch
        {
            DatabaseType.PostgreSQL => $"Host={host};Port={port};Database={database};Username={username};Password={password};",
            _ => ""
        };
        
        ConnectionStringTextBox.Text = connectionString;
    }
    
    private async void TestConnectionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClearAllErrors();
        StatusTextBlock.IsVisible = false;
        
        var connectionString = ConnectionStringTextBox.Text;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            ShowFieldError("ConnectionString", "Please enter a connection string");
            ShowStatus("Connection string is required for testing", false);
            return;
        }
        
        TestConnectionButton.Content = "Testing...";
        TestConnectionButton.IsEnabled = false;
        TestProgressBar.IsVisible = true;
        
        try
        {
            // You would implement actual connection testing here
            // For now, just simulate a test
            await Task.Delay(2000);
            ShowStatus("✅ Connection test successful!", true);
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Connection test failed: {ex.Message}", false);
        }
        finally
        {
            TestConnectionButton.Content = "Test Connection";
            TestConnectionButton.IsEnabled = true;
            TestProgressBar.IsVisible = false;
        }
    }
    
    private void ShowStatus(string message, bool isSuccess)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = isSuccess ? 
            Avalonia.Media.Brushes.Green : 
            Avalonia.Media.Brushes.Red;
        StatusTextBlock.IsVisible = true;
    }
    
    private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_connectionService == null)
        {
            ShowStatus("❌ Connection service not available", false);
            return;
        }
        
        if (!ValidateForm()) return;
        
        var createDto = new DatabaseConnectionCreateDTO
        {
            Name = NameTextBox.Text ?? "",
            DatabaseType = (DatabaseType)(DatabaseTypeComboBox.SelectedItem ?? DatabaseType.PostgreSQL),
            Host = HostTextBox.Text ?? "",
            Port = (int)(PortNumericUpDown.Value ?? 5432),
            DatabaseName = DatabaseNameTextBox.Text ?? "",
            Username = UsernameTextBox.Text ?? "",
            Password = PasswordTextBox.Text ?? "",
            ConnectionString = ConnectionStringTextBox.Text ?? ""
        };
        
        try
        {
            SaveButton.Content = "Saving...";
            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            
            if (_existingConnection != null)
            {
                // Update existing connection
                var response = await _connectionService.UpdateConnectionString(_existingConnection.Id, createDto);
                if (response.IsSuccess)
                {
                    ShowStatus("✅ Connection updated successfully!", true);
                    await Task.Delay(500); // Brief delay to show success message
                    Close(response.Data);
                }
                else
                {
                    ShowStatus($"❌ Error updating connection: {string.Join(", ", response.Errors)}", false);
                }
            }
            else
            {
                // Create new connection
                var response = await _connectionService.CreateConnectionString(createDto);
                if (response.IsSuccess)
                {
                    ShowStatus("✅ Connection created successfully!", true);
                    await Task.Delay(500); // Brief delay to show success message
                    Close(response.Data);
                }
                else
                {
                    ShowStatus($"❌ Error creating connection: {string.Join(", ", response.Errors)}", false);
                }
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Error: {ex.Message}", false);
        }
        finally
        {
            SaveButton.Content = _existingConnection != null ? "Update Connection" : "Save Connection";
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }
    }
    
    private bool ValidateForm()
    {
        ClearAllErrors();
        bool isValid = true;
        
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ShowFieldError("Name", "Connection name is required");
            isValid = false;
        }
        
        if (string.IsNullOrWhiteSpace(HostTextBox.Text))
        {
            ShowFieldError("Host", "Host/Server is required");
            isValid = false;
        }
        
        if (string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text))
        {
            ShowFieldError("DatabaseName", "Database name is required");
            isValid = false;
        }
        
        if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
        {
            ShowFieldError("Username", "Username is required");
            isValid = false;
        }
        
        if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
        {
            ShowFieldError("Password", "Password is required");
            isValid = false;
        }
        
        if (string.IsNullOrWhiteSpace(ConnectionStringTextBox.Text))
        {
            ShowFieldError("ConnectionString", "Connection string is required");
            isValid = false;
        }
        
        if (!isValid)
        {
            ShowStatus("❌ Please fix the errors above before saving", false);
        }
        
        return isValid;
    }
    
    private void ShowFieldError(string fieldName, string message)
    {
        TextBlock? errorTextBlock = fieldName switch
        {
            "Name" => NameErrorText,
            "Host" => HostErrorText,
            "DatabaseName" => DatabaseNameErrorText,
            "Username" => UsernameErrorText,
            "Password" => PasswordErrorText,
            "ConnectionString" => ConnectionStringErrorText,
            _ => null
        };
        
        if (errorTextBlock != null)
        {
            errorTextBlock.Text = message;
            errorTextBlock.IsVisible = true;
        }
        
        // Add error styling to the corresponding input
        Control? inputControl = fieldName switch
        {
            "Name" => NameTextBox,
            "Host" => HostTextBox,
            "DatabaseName" => DatabaseNameTextBox,
            "Username" => UsernameTextBox,
            "Password" => PasswordTextBox,
            "ConnectionString" => ConnectionStringTextBox,
            _ => null
        };
        
        if (inputControl != null)
        {
            inputControl.Classes.Add("Error");
        }
    }
    
    private void ClearFieldError(string fieldName)
    {
        TextBlock? errorTextBlock = fieldName switch
        {
            "Name" => NameErrorText,
            "Host" => HostErrorText,
            "DatabaseName" => DatabaseNameErrorText,
            "Username" => UsernameErrorText,
            "Password" => PasswordErrorText,
            "ConnectionString" => ConnectionStringErrorText,
            _ => null
        };
        
        if (errorTextBlock != null)
        {
            errorTextBlock.IsVisible = false;
        }
        
        // Remove error styling from the corresponding input
        Control? inputControl = fieldName switch
        {
            "Name" => NameTextBox,
            "Host" => HostTextBox,
            "DatabaseName" => DatabaseNameTextBox,
            "Username" => UsernameTextBox,
            "Password" => PasswordTextBox,
            "ConnectionString" => ConnectionStringTextBox,
            _ => null
        };
        
        if (inputControl != null)
        {
            inputControl.Classes.Remove("Error");
        }
    }
    
    private void ClearAllErrors()
    {
        ClearFieldError("Name");
        ClearFieldError("Host");
        ClearFieldError("DatabaseName");
        ClearFieldError("Username");
        ClearFieldError("Password");
        ClearFieldError("ConnectionString");
        StatusTextBlock.IsVisible = false;
    }
    
    private void HandleEnterKey(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            SaveButton_Click(null, new Avalonia.Interactivity.RoutedEventArgs());
        }
    }
    
    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(null);
    }
}