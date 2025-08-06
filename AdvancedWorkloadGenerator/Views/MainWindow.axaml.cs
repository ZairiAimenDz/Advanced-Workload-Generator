using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AdvancedWorkloadGenerator.Core.Interfaces;
using AdvancedWorkloadGenerator.Core.Models.DatabaseConnections;
using AdvancedWorkloadGenerator.Core.Models.DatabaseTables;
using AdvancedWorkloadGenerator.Core.Enums;
using AdvancedWorkloadGenerator.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator
{
    public class StatusToColorConverter : IValueConverter
    {
        public static StatusToColorConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string statusText)
            {
                if (statusText.Contains("✓ Analyzed"))
                    return Color.FromRgb(78, 201, 176); // Green for analyzed
                else if (statusText.Contains("○ Not Analyzed"))
                    return Color.FromRgb(255, 206, 84); // Yellow for not analyzed
            }
            return Color.FromRgb(128, 128, 128); // Gray for unknown status
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionDisplayItem
    {
        public DatabaseConnectionDTO Connection { get; set; } = null!;
        public string DisplayText { get; set; } = string.Empty;
        public string SubText { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        
        public override string ToString() => DisplayText;
    }

    public partial class MainWindow : Window
    {
        private IConnectionStringService? _connectionService;
        private IDatabaseAnalysisService? _analysisService;
        private List<DatabaseConnectionDTO> _connections = new();
        private DatabaseDiagramControl _diagramControl;
        private ConnectionDisplayItem? _currentlySelectedConnection;
        private readonly ILogger<MainWindow>? _logger;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize diagram control
            _diagramControl = new DatabaseDiagramControl();
            DiagramContainer.Content = _diagramControl;
            
            // Get the logger and services from DI container
            try
            {
                _logger = Program.ServiceProvider?.GetService<ILogger<MainWindow>>();
                    
                _connectionService = Program.ServiceProvider?.GetService<IConnectionStringService>()
                    ?? throw new InvalidOperationException("ConnectionStringService not found in DI container");
                
                _analysisService = Program.ServiceProvider?.GetService<IDatabaseAnalysisService>()
                    ?? throw new InvalidOperationException("DatabaseAnalysisService not found in DI container");
                
                _ = LoadConnectionsAsync();
            }
            catch (Exception ex)
            {
                // Fallback to static logger if instance logger is not available
                Serilog.Log.Error(ex, "Error initializing MainWindow");
                // Show error in UI
                ShowInitializationError(ex.Message);
            }
        }
        
        private void ShowInitializationError(string message)
        {
            // Add error text to the first tab
            if (ConnectionsListBox != null)
            {
                var errorItems = new List<string> { $"Initialization Error: {message}" };
                ConnectionsListBox.ItemsSource = errorItems;
            }
        }



        private async Task LoadConnectionsAsync()
        {
            if (_connectionService == null) return;
            
            try
            {
                var response = await _connectionService.GetAllConnectionStrings();
                if (response.IsSuccess && response.Data != null)
                {
                    _connections = response.Data;
                    
                    // Create a more detailed display for connections
                    var connectionDisplayItems = _connections.Select(c => new ConnectionDisplayItem
                    {
                        Connection = c,
                        DisplayText = $"{c.Name}",
                        SubText = $"{c.DatabaseType} - {c.Host}:{c.Port}/{c.DatabaseName}",
                        StatusText = c.IsAnalyzed ? "✓ Analyzed" : "○ Not Analyzed"
                    }).ToList();
                    
                    ConnectionsListBox.ItemsSource = connectionDisplayItems;
                    
                    // Update status
                    if (_connections.Any())
                    {
                        UpdateStatusText($"Loaded {_connections.Count} connection(s)");
                    }
                    else
                    {
                        UpdateStatusText("No connections found");
                    }
                }
                else if (!response.IsSuccess)
                {
                    ShowInitializationError($"Failed to load connections: {string.Join(", ", response.Errors)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading connections");
                ShowInitializationError($"Error loading connections: {ex.Message}");
            }
        }

        private async void AddConnectionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_connectionService == null) return;
            
            ShowProgress("Opening connection editor...");
            
            try
            {
                var editWindow = new ConnectionEditing();
                var result = await editWindow.ShowDialog<DatabaseConnectionDTO?>(this);
                
                if (result != null)
                {
                    ShowProgress("Refreshing connections...");
                    await LoadConnectionsAsync();
                    HideProgress($"Connection '{result.Name}' added successfully");
                }
                else
                {
                    HideProgress("Ready");
                }
            }
            catch (Exception ex)
            {
                HideProgress($"Error: {ex.Message}");
            }
        }

        private async void EditConnectionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_connectionService == null) return;
            
            if (ConnectionsListBox.SelectedItem is ConnectionDisplayItem selectedItem)
            {
                ShowProgress($"Editing connection '{selectedItem.Connection.Name}'...");
                
                try
                {
                    var editWindow = new ConnectionEditing(selectedItem.Connection);
                    var result = await editWindow.ShowDialog<DatabaseConnectionDTO?>(this);
                    
                    if (result != null)
                    {
                        ShowProgress("Refreshing connections...");
                        await LoadConnectionsAsync();
                        HideProgress($"Connection '{result.Name}' updated successfully");
                    }
                    else
                    {
                        HideProgress("Ready");
                    }
                }
                catch (Exception ex)
                {
                    HideProgress($"Error: {ex.Message}");
                }
            }
        }

        private async void DeleteConnectionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_connectionService == null) return;
            
            if (ConnectionsListBox.SelectedItem is ConnectionDisplayItem selectedItem)
            {
                var result = await MessageBox.Show(this, 
                    $"Are you sure you want to delete the connection '{selectedItem.Connection.Name}'?", 
                    "Confirm Delete", 
                    MessageBox.MessageBoxButtons.YesNo);
                
                if (result == MessageBox.MessageBoxResult.Yes)
                {
                    ShowProgress($"Deleting connection '{selectedItem.Connection.Name}'...");
                    
                    try
                    {
                        var deleteResponse = await _connectionService.DeleteConnectionString(selectedItem.Connection.Id);
                        if (deleteResponse.IsSuccess)
                        {
                            ShowProgress("Refreshing connections...");
                            await LoadConnectionsAsync();
                            HideProgress($"Connection '{selectedItem.Connection.Name}' deleted successfully");
                        }
                        else
                        {
                            HideProgress("Delete failed");
                            await MessageBox.Show(this,
                                $"Failed to delete connection: {string.Join(", ", deleteResponse.Errors)}",
                                "Delete Error",
                                MessageBox.MessageBoxButtons.Ok);
                        }
                    }
                    catch (Exception ex)
                    {
                        HideProgress($"Error: {ex.Message}");
                        _logger.LogError(ex, "Error deleting connection");
                        await MessageBox.Show(this,
                            $"Error deleting connection: {ex.Message}",
                            "Delete Error", 
                            MessageBox.MessageBoxButtons.Ok);
                    }
                }
                else
                {
                    UpdateStatusText("Ready");
                }
            }
        }

        private async void ResetAnalysisButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_connectionService == null || _analysisService == null) return;
            
            if (ConnectionsListBox.SelectedItem is ConnectionDisplayItem selectedItem)
            {
                var result = await MessageBox.Show(this,
                    $"This will reset and re-analyze the database schema for '{selectedItem.Connection.Name}'.\n\n" +
                    "All existing analysis data will be cleared and the database will be re-analyzed. Continue?",
                    "Reset and Re-analyze Database",
                    MessageBox.MessageBoxButtons.YesNo);
                
                if (result == MessageBox.MessageBoxResult.Yes)
                {
                    try
                    {
                        await ResetAndAnalyzeDatabaseAsync(selectedItem);
                    }
                    catch (Exception ex)
                    {
                        HideProgress($"Error: {ex.Message}");
                        _logger.LogError(ex, "Error resetting analysis");
                        await MessageBox.Show(this,
                            $"Error resetting database analysis: {ex.Message}",
                            "Reset Error", 
                            MessageBox.MessageBoxButtons.Ok);
                    }
                }
                else
                {
                    UpdateStatusText("Ready");
                }
            }
        }

        private async void ConnectionsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ConnectionsListBox.SelectedItem is ConnectionDisplayItem selectedItem)
            {
                EditConnectionButton.IsEnabled = true;
                DeleteConnectionButton.IsEnabled = true;
                ResetAnalysisButton.IsEnabled = true;
                
                // Check if we need to show confirmation dialog (only if it's a different connection)
                if (_currentlySelectedConnection != null && 
                    _currentlySelectedConnection.Connection.Id != selectedItem.Connection.Id)
                {
                    // Check if the current connection is already analyzed
                    if (selectedItem.Connection.IsAnalyzed)
                    {
                        _logger.LogInformation("Switching to already analyzed connection: {ConnectionName}", selectedItem.Connection.Name);
                        
                        // Just show the existing diagram without confirmation
                        _currentlySelectedConnection = selectedItem;
                        UpdateConnectionStatus(selectedItem.Connection.Name, true);
                        UpdateStatusText($"Selected: {selectedItem.Connection.Name}");
                        
                        // Load and display existing diagram
                        _ = LoadExistingDiagramAsync(selectedItem);
                        return;
                    }
                    
                    var result = await MessageBox.Show(this,
                        $"You have already selected '{_currentlySelectedConnection.Connection.Name}'. " +
                        $"Do you want to switch to '{selectedItem.Connection.Name}' and analyze it?",
                        "Switch Database Connection",
                        MessageBox.MessageBoxButtons.YesNo);
                    
                    if (result == MessageBox.MessageBoxResult.No)
                    {
                        // Revert selection
                        ConnectionsListBox.SelectedItem = _currentlySelectedConnection;
                        return;
                    }
                }
                
                _currentlySelectedConnection = selectedItem;
                
                // Update status bar
                UpdateConnectionStatus(selectedItem.Connection.Name, true);
                UpdateStatusText($"Selected: {selectedItem.Connection.Name}");
                
                // Start database analysis or load existing diagram
                if (selectedItem.Connection.IsAnalyzed)
                {
                    _ = LoadExistingDiagramAsync(selectedItem);
                }
                else
                {
                    _ = AnalyzeDatabaseAsync(selectedItem);
                }
            }
            else
            {
                EditConnectionButton.IsEnabled = false;
                DeleteConnectionButton.IsEnabled = false;
                ResetAnalysisButton.IsEnabled = false;
                _currentlySelectedConnection = null;
                
                UpdateConnectionStatus("No Connection", false);
                UpdateStatusText("Ready");
                
                // Clear diagram
                _diagramControl.ClearDiagram();
            }
        }
        
        private void UpdateStatusText(string message)
        {
            StatusText.Text = message;
        }
        
        private void UpdateConnectionStatus(string connectionName, bool isConnected)
        {
            ConnectionStatusText.Text = connectionName;
            ConnectionStatusIndicator.Fill = isConnected 
                ? Avalonia.Media.Brushes.LimeGreen 
                : Avalonia.Media.Brushes.Gray;
        }
        
        public void ShowProgress(string message, bool isIndeterminate = true)
        {
            UpdateStatusText(message);
            ProgressBar.IsVisible = true;
            ProgressBar.IsIndeterminate = isIndeterminate;
        }
        
        public void UpdateProgress(double value, string? message = null)
        {
            if (message != null)
                UpdateStatusText(message);
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = value;
        }
        
        public void HideProgress(string? message = null)
        {
            ProgressBar.IsVisible = false;
            if (message != null)
                UpdateStatusText(message);
        }

        private async Task AnalyzeDatabaseAsync(ConnectionDisplayItem connectionItem)
        {
            if (_analysisService == null) return;

            try
            {
                // Show progress on diagram
                await _diagramControl.ShowAnalysisProgressAsync();
                ShowProgress($"Analyzing database '{connectionItem.Connection.Name}'...");

                // Update progress steps
                await _diagramControl.UpdateAnalysisProgressAsync("Connecting to database...");
                await Task.Delay(500); // Small delay for visual feedback

                await _diagramControl.UpdateAnalysisProgressAsync("Reading schema information...");
                await Task.Delay(500);

                await _diagramControl.UpdateAnalysisProgressAsync("Analyzing tables and relationships...");
                
                // Perform actual analysis
                var analysisResult = await _analysisService.AnalyseDatabase(connectionItem.Connection.Id);
                
                if (analysisResult.IsSuccess)
                {
                    await _diagramControl.UpdateAnalysisProgressAsync("Loading table structures...");
                    
                    // Get the analyzed tables for display
                    var tables = await GetAnalyzedTablesAsync(connectionItem.Connection.Id);
                    
                    if (tables.Any())
                    {
                        await _diagramControl.UpdateAnalysisProgressAsync("Creating diagram...");
                        await Task.Delay(500);
                        
                        // Display the schema diagram
                        await _diagramControl.ShowDatabaseSchemaAsync(connectionItem.Connection, tables);
                        
                        HideProgress($"Database '{connectionItem.Connection.Name}' analyzed successfully - {tables.Count} tables found");
                    }
                    else
                    {
                        await _diagramControl.ShowErrorAsync("No tables found in the database schema.");
                        HideProgress($"Analysis completed but no tables found in '{connectionItem.Connection.Name}'");
                    }
                    
                    // Update connection status
                    connectionItem.StatusText = "✓ Analyzed";
                    
                    // Refresh the connections list to show updated status, but preserve current selection
                    var currentSelectionId = connectionItem.Connection.Id;
                    await LoadConnectionsAsync();
                    
                    // Restore selection to prevent diagram from being cleared
                    RestoreSelection(currentSelectionId);
                }
                else
                {
                    await _diagramControl.ShowErrorAsync($"Analysis failed: {string.Join(", ", analysisResult.Errors)}");
                    HideProgress($"Analysis failed for '{connectionItem.Connection.Name}'");
                }
            }
            catch (Exception ex)
            {
                await _diagramControl.ShowErrorAsync($"Error during analysis: {ex.Message}");
                HideProgress($"Error analyzing '{connectionItem.Connection.Name}': {ex.Message}");
                _logger.LogError(ex, "Database analysis error");
            }
        }

        private async Task<List<DatabaseTableDTO>> GetAnalyzedTablesAsync(Guid connectionId)
        {
            if (_connectionService == null) 
            {
                _logger.LogWarning("ConnectionService is null");
                return new List<DatabaseTableDTO>();
            }

            try
            {
                _logger.LogDebug("Getting analyzed tables for connection ID: {ConnectionId}", connectionId);
                var connectionResponse = await _connectionService.GetConnectionStringById(connectionId);
                
                _logger.LogDebug("Connection response - Success: {IsSuccess}", connectionResponse.IsSuccess);
                
                if (connectionResponse.IsSuccess && connectionResponse.Data != null)
                {
                    _logger.LogDebug("Connection data retrieved: {ConnectionName}", connectionResponse.Data.Name);
                    _logger.LogDebug("IsAnalyzed: {IsAnalyzed}", connectionResponse.Data.IsAnalyzed);
                    _logger.LogDebug("DatabaseTables count: {TableCount}", connectionResponse.Data.DatabaseTables?.Count ?? 0);
                    
                    if (connectionResponse.Data.DatabaseTables != null)
                    {
                        foreach (var table in connectionResponse.Data.DatabaseTables)
                        {
                            _logger.LogDebug("Table: {TableName} - Columns: {ColumnCount}", table.TableName, table.Columns?.Count ?? 0);
                        }
                        return connectionResponse.Data.DatabaseTables;
                    }
                    else
                    {
                        _logger.LogDebug("DatabaseTables is null");
                    }
                }
                else
                {
                    _logger.LogError("Failed to get analyzed tables. Errors: {Errors}", string.Join(", ", connectionResponse.Errors));
                    if (connectionResponse.Data == null)
                    {
                        _logger.LogDebug("Connection data is null");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception getting analyzed tables");
            }

            return new List<DatabaseTableDTO>();
        }

        private async Task LoadExistingDiagramAsync(ConnectionDisplayItem connectionItem)
        {
            try
            {
                _logger.LogInformation("Loading existing diagram for connection: {ConnectionName}", connectionItem.Connection.Name);
                
                ShowProgress($"Loading diagram for '{connectionItem.Connection.Name}'...");
                
                // Get the analyzed tables for display
                var tables = await GetAnalyzedTablesAsync(connectionItem.Connection.Id);
                
                _logger.LogDebug("LoadExistingDiagramAsync: Retrieved {TableCount} tables for connection {ConnectionName}", tables.Count, connectionItem.Connection.Name);
                
                if (tables.Any())
                {
                    _logger.LogDebug("Displaying diagram with {TableCount} tables", tables.Count);
                    // Display the schema diagram
                    await _diagramControl.ShowDatabaseSchemaAsync(connectionItem.Connection, tables);
                    HideProgress($"Diagram loaded for '{connectionItem.Connection.Name}' - {tables.Count} tables displayed");
                }
                else
                {
                    _logger.LogDebug("No tables found, showing error message");
                    await _diagramControl.ShowErrorAsync("No tables found for this analyzed connection.");
                    HideProgress($"No tables found for '{connectionItem.Connection.Name}'");
                }
            }
            catch (Exception ex)
            {
                await _diagramControl.ShowErrorAsync($"Error loading diagram: {ex.Message}");
                HideProgress($"Error loading diagram for '{connectionItem.Connection.Name}': {ex.Message}");
                _logger.LogError(ex, "Error loading existing diagram");
            }
        }

        private void RestoreSelection(Guid connectionId)
        {
            try
            {
                if (ConnectionsListBox.ItemsSource is IEnumerable<ConnectionDisplayItem> items)
                {
                    var itemToSelect = items.FirstOrDefault(item => item.Connection.Id == connectionId);
                    if (itemToSelect != null)
                    {
                        // Temporarily disable the selection changed event to prevent recursive calls
                        ConnectionsListBox.SelectionChanged -= ConnectionsListBox_SelectionChanged;
                        ConnectionsListBox.SelectedItem = itemToSelect;
                        ConnectionsListBox.SelectionChanged += ConnectionsListBox_SelectionChanged;
                        
                        _currentlySelectedConnection = itemToSelect;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring selection");
            }
        }

        private async Task ResetAndAnalyzeDatabaseAsync(ConnectionDisplayItem connectionItem)
        {
            if (_analysisService == null || _connectionService == null) return;

            try
            {
                _logger.LogInformation("Starting reset and re-analysis for connection: {ConnectionName}", connectionItem.Connection.Name);
                
                // Clear the diagram first
                _diagramControl.ClearDiagram();
                
                // Show progress on diagram
                await _diagramControl.ShowAnalysisProgressAsync();
                ShowProgress($"Resetting analysis for '{connectionItem.Connection.Name}'...");

                // Step 1: Reset the analysis (this will clear existing data and mark as not analyzed)
                await _diagramControl.UpdateAnalysisProgressAsync("Clearing previous analysis data...");
                await Task.Delay(500); // Small delay for visual feedback

                // The AnalyseDatabase method in DatabaseAnalysisService already clears existing data
                // So we can directly call it - it will reset and re-analyze
                await _diagramControl.UpdateAnalysisProgressAsync("Connecting to database...");
                await Task.Delay(500);

                await _diagramControl.UpdateAnalysisProgressAsync("Reading schema information...");
                await Task.Delay(500);

                await _diagramControl.UpdateAnalysisProgressAsync("Analyzing tables and relationships...");
                
                // Perform the analysis (this will clear old data and create new analysis)
                var analysisResult = await _analysisService.AnalyseDatabase(connectionItem.Connection.Id);
                
                if (analysisResult.IsSuccess)
                {
                    await _diagramControl.UpdateAnalysisProgressAsync("Loading table structures...");
                    
                    // Get the newly analyzed tables for display
                    var tables = await GetAnalyzedTablesAsync(connectionItem.Connection.Id);
                    
                    if (tables.Any())
                    {
                        await _diagramControl.UpdateAnalysisProgressAsync("Creating diagram...");
                        await Task.Delay(500);
                        
                        // Display the schema diagram
                        await _diagramControl.ShowDatabaseSchemaAsync(connectionItem.Connection, tables);
                        
                        HideProgress($"Database '{connectionItem.Connection.Name}' re-analyzed successfully - {tables.Count} tables found");
                    }
                    else
                    {
                        await _diagramControl.ShowErrorAsync("No tables found in the database schema after re-analysis.");
                        HideProgress($"Re-analysis completed but no tables found in '{connectionItem.Connection.Name}'");
                    }
                    
                    // Update connection status
                    connectionItem.StatusText = "✓ Analyzed";
                    
                    // Refresh the connections list to show updated status, but preserve current selection
                    var currentSelectionId = connectionItem.Connection.Id;
                    await LoadConnectionsAsync();
                    
                    // Restore selection to prevent diagram from being cleared
                    RestoreSelection(currentSelectionId);
                    
                    // Show success message
                    await MessageBox.Show(this,
                        $"Database '{connectionItem.Connection.Name}' has been successfully re-analyzed.\n\n" +
                        $"Found {tables.Count} table(s) in the schema.",
                        "Re-analysis Complete",
                        MessageBox.MessageBoxButtons.Ok);
                }
                else
                {
                    await _diagramControl.ShowErrorAsync($"Re-analysis failed: {string.Join(", ", analysisResult.Errors)}");
                    HideProgress($"Re-analysis failed for '{connectionItem.Connection.Name}'");
                    
                    await MessageBox.Show(this,
                        $"Failed to re-analyze database '{connectionItem.Connection.Name}':\n\n" +
                        $"{string.Join("\n", analysisResult.Errors)}",
                        "Re-analysis Failed",
                        MessageBox.MessageBoxButtons.Ok);
                }
            }
            catch (Exception ex)
            {
                await _diagramControl.ShowErrorAsync($"Error during re-analysis: {ex.Message}");
                HideProgress($"Error re-analyzing '{connectionItem.Connection.Name}': {ex.Message}");
                _logger.LogError(ex, "Database re-analysis error");
                
                await MessageBox.Show(this,
                    $"An error occurred while re-analyzing the database:\n\n{ex.Message}",
                    "Re-analysis Error",
                    MessageBox.MessageBoxButtons.Ok);
            }
        }
    }

    // Simple MessageBox implementation for Avalonia
    public static class MessageBox
    {
        public enum MessageBoxButtons
        {
            Ok,
            YesNo
        }

        public enum MessageBoxResult
        {
            Ok,
            Yes,
            No
        }

        public static async Task<MessageBoxResult> Show(Window parent, string message, string title, MessageBoxButtons buttons)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            panel.Children.Add(new TextBlock { Text = message, Margin = new Avalonia.Thickness(0, 0, 0, 20) });

            var buttonPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };

            MessageBoxResult result = MessageBoxResult.Ok;

            if (buttons == MessageBoxButtons.YesNo)
            {
                var yesButton = new Button { Content = "Yes", Margin = new Avalonia.Thickness(0, 0, 10, 0) };
                var noButton = new Button { Content = "No" };

                yesButton.Click += (s, e) => { result = MessageBoxResult.Yes; dialog.Close(); };
                noButton.Click += (s, e) => { result = MessageBoxResult.No; dialog.Close(); };

                buttonPanel.Children.Add(yesButton);
                buttonPanel.Children.Add(noButton);
            }
            else
            {
                var okButton = new Button { Content = "OK" };
                okButton.Click += (s, e) => { result = MessageBoxResult.Ok; dialog.Close(); };
                buttonPanel.Children.Add(okButton);
            }

            panel.Children.Add(buttonPanel);
            dialog.Content = panel;

            await dialog.ShowDialog(parent);
            return result;
        }
    }
}