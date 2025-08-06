using Avalonia.Controls;
using Avalonia.Threading;
using AdvancedWorkloadGenerator.Core.Models.DatabaseConnections;
using AdvancedWorkloadGenerator.Core.Models.DatabaseTables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Controls
{
    public partial class DatabaseDiagramControl : UserControl
    {
        private DatabaseConnectionDTO? _currentConnection;
        private readonly ILogger<DatabaseDiagramControl>? _logger;

        public DatabaseDiagramControl()
        {
            InitializeComponent();
            
            try
            {
                _logger = Program.ServiceProvider?.GetService<ILogger<DatabaseDiagramControl>>();
            }
            catch (Exception ex)
            {
                // Fallback to static logger if DI is not available
                Log.Logger.Error(ex, "Error getting logger for DatabaseDiagramControl");
            }
            
            InitializeDiagram();
        }

        private void InitializeDiagram()
        {
            // For now, we'll use a simple implementation
            // The actual GoDiagram implementation can be added later
        }

        public async Task ShowAnalysisProgressAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                EmptyState.IsVisible = false;
                DiagramScrollViewer.IsVisible = false;
                ProgressOverlay.IsVisible = true;
                ProgressText.Text = "Analyzing database schema...";
            });
        }

        public async Task UpdateAnalysisProgressAsync(string message)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText.Text = message;
            });
        }

        public async Task ShowDatabaseSchemaAsync(DatabaseConnectionDTO connection, List<DatabaseTableDTO> tables)
        {
            _currentConnection = connection;
            
            _logger?.LogDebug("ShowDatabaseSchemaAsync called with {TableCount} tables for connection: {ConnectionName}", tables.Count, connection.Name);
            
            // Log table details
            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i];
                _logger?.LogDebug("Table {TableIndex}: {TableName} (Schema: {SchemaName}) - Columns: {ColumnCount}", i + 1, table.TableName, table.SchemaName, table.Columns?.Count ?? 0);
                if (table.Columns != null)
                {
                    foreach (var column in table.Columns.Take(3)) // Log first 3 columns
                    {
                        _logger?.LogDebug($"  Column: {column.ColumnName} ({column.DataType}) - PK: {column.IsPrimaryKey}");
                    }
                    if (table.Columns.Count > 3)
                    {
                        _logger?.LogDebug($"  ... and {table.Columns.Count - 3} more columns");
                    }
                }
            }
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    ProgressOverlay.IsVisible = false;
                    EmptyState.IsVisible = false;
                    DiagramScrollViewer.IsVisible = true;
                    
                    _logger?.LogDebug($"UI visibility updated - Progress: {ProgressOverlay.IsVisible}, Empty: {EmptyState.IsVisible}, Diagram: {DiagramScrollViewer.IsVisible}");
                    
                    // If no tables, create a test diagram to verify rendering works
                    if (tables.Count == 0)
                    {
                        _logger?.LogDebug("No tables found, creating test diagram");
                        CreateTestDiagram();
                    }
                    else
                    {
                        // Create a simple table visualization using Avalonia controls
                        CreateSimpleDiagram(tables);
                    }
                    
                    _logger?.LogDebug("Diagram creation completed successfully");
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug($"Error in ShowDatabaseSchemaAsync UI thread: {ex.Message}");
                    throw;
                }
            });
        }

        private void CreateSimpleDiagram(List<DatabaseTableDTO> tables)
        {
            _logger?.LogDebug($"CreateSimpleDiagram called with {tables.Count} tables");
            
            DiagramPanel.Child = null;
            
            if (tables.Count == 0)
            {
                _logger?.LogDebug("No tables to display, setting empty message");
                var emptyMessage = new TextBlock
                {
                    Text = "No tables found in the database",
                    Foreground = Avalonia.Media.Brushes.LightGray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontSize = 16,
                    Margin = new Avalonia.Thickness(20)
                };
                DiagramPanel.Child = emptyMessage;
                return;
            }
            
            // Calculate canvas size based on number of tables (3 tables per row)
            int tablesPerRow = 3;
            int rows = (int)Math.Ceiling((double)tables.Count / tablesPerRow);
            int canvasWidth = Math.Max(1200, tablesPerRow * 300 + 40); // Extra width for padding
            int canvasHeight = Math.Max(800, rows * 280 + 40); // Extra height for padding
            
            var canvas = new Canvas
            {
                Width = canvasWidth,
                Height = canvasHeight,
                Background = Avalonia.Media.Brushes.Transparent
            };
            
            _logger?.LogDebug($"Created canvas with size: {canvas.Width} x {canvas.Height} for {tables.Count} tables in {rows} rows");

            // Store table positions for relationship drawing
            var tablePositions = new Dictionary<string, (double X, double Y, Border Control)>();

            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i];
                _logger?.LogDebug($"Creating control for table {i + 1}/{tables.Count}: {table.TableName} with {table.Columns?.Count ?? 0} columns");
                
                var tableControl = CreateTableControl(table);
                
                // Position tables in a grid layout (3 per row)
                var x = (i % tablesPerRow) * 300 + 20;
                var y = (i / tablesPerRow) * 280 + 20;
                
                Canvas.SetLeft(tableControl, x);
                Canvas.SetTop(tableControl, y);
                
                canvas.Children.Add(tableControl);
                tablePositions[table.TableName] = (x, y, tableControl);
                _logger?.LogDebug($"Added table {table.TableName} at position ({x}, {y})");
            }
            
            // Draw relationships
            foreach (var table in tables)
            {
                if (table.Columns != null)
                {
                    foreach (var column in table.Columns.Where(c => c.IsForeignKey && !string.IsNullOrEmpty(c.ReferencedTable)))
                    {
                        if (tablePositions.ContainsKey(table.TableName) && 
                            tablePositions.ContainsKey(column.ReferencedTable!))
                        {
                            var fromPos = tablePositions[table.TableName];
                            var toPos = tablePositions[column.ReferencedTable!];
                            
                            DrawRelationshipLine(canvas, fromPos, toPos, table.TableName, column.ReferencedTable!);
                        }
                    }
                }
            }
            
            DiagramPanel.Child = canvas;
            _logger?.LogDebug($"Diagram creation completed. Canvas has {canvas.Children.Count} child controls");
            
            // Force a layout update
            canvas.InvalidateVisual();
            DiagramPanel.InvalidateVisual();
        }

        private void CreateTestDiagram()
        {
            _logger?.LogDebug("Creating test diagram to verify rendering works");
            
            var canvas = new Canvas
            {
                Width = 1200,
                Height = 800,
                Background = Avalonia.Media.Brushes.Transparent
            };
            
            // Create a test table visualization
            var testBorder = new Border
            {
                Background = Avalonia.Media.Brushes.DarkSlateGray,
                BorderBrush = Avalonia.Media.Brushes.Gray,
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(5),
                Width = 250,
                Margin = new Avalonia.Thickness(5)
            };

            var stackPanel = new StackPanel();

            // Test table header
            var headerBorder = new Border
            {
                Background = Avalonia.Media.Brushes.SteelBlue,
                Padding = new Avalonia.Thickness(10, 8)
            };

            var headerText = new TextBlock
            {
                Text = "TEST TABLE (No Data Found)",
                Foreground = Avalonia.Media.Brushes.White,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            headerBorder.Child = headerText;
            stackPanel.Children.Add(headerBorder);

            // Add some test columns
            var testColumns = new[] { "id (INTEGER)", "name (VARCHAR)", "created_at (TIMESTAMP)" };
            foreach (var column in testColumns)
            {
                var columnPanel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Margin = new Avalonia.Thickness(10, 3)
                };

                var keyIndicator = new Border
                {
                    Width = 8,
                    Height = 8,
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Background = column.Contains("id") ? Avalonia.Media.Brushes.Gold : Avalonia.Media.Brushes.LightGray,
                    Margin = new Avalonia.Thickness(0, 0, 5, 0),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                var columnText = new TextBlock
                {
                    Text = column,
                    Foreground = Avalonia.Media.Brushes.White,
                    FontSize = 11,
                    FontFamily = new Avalonia.Media.FontFamily("Consolas,monospace")
                };

                columnPanel.Children.Add(keyIndicator);
                columnPanel.Children.Add(columnText);
                stackPanel.Children.Add(columnPanel);
            }

            testBorder.Child = stackPanel;
            
            Canvas.SetLeft(testBorder, 50);
            Canvas.SetTop(testBorder, 50);
            canvas.Children.Add(testBorder);
            
            // Add a message
            var messageText = new TextBlock
            {
                Text = "This is a test diagram.\nThe database analysis may not have returned any tables,\nor there might be an issue with the data retrieval.",
                Foreground = Avalonia.Media.Brushes.Orange,
                FontSize = 12,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Width = 300
            };
            
            Canvas.SetLeft(messageText, 350);
            Canvas.SetTop(messageText, 50);
            canvas.Children.Add(messageText);
            
            DiagramPanel.Child = canvas;
            _logger?.LogDebug("Test diagram created successfully");
        }

        private Border CreateTableControl(DatabaseTableDTO table)
        {
            var border = new Border
            {
                Background = Avalonia.Media.Brushes.DarkSlateGray,
                BorderBrush = Avalonia.Media.Brushes.Gray,
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(5),
                Width = 250,
                Margin = new Avalonia.Thickness(5)
            };

            var stackPanel = new StackPanel();

            // Table header
            var headerBorder = new Border
            {
                Background = Avalonia.Media.Brushes.SteelBlue,
                Padding = new Avalonia.Thickness(10, 8)
            };

            var headerText = new TextBlock
            {
                Text = table.TableName,
                Foreground = Avalonia.Media.Brushes.White,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            headerBorder.Child = headerText;
            stackPanel.Children.Add(headerBorder);

            // Table columns
            if (table.Columns != null)
            {
                foreach (var column in table.Columns.Take(10)) // Limit to 10 columns for display
                {
                    var columnPanel = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Margin = new Avalonia.Thickness(10, 3)
                    };

                    // Primary key indicator
                    var keyIndicator = new Border
                    {
                        Width = 8,
                        Height = 8,
                        CornerRadius = new Avalonia.CornerRadius(4),
                        Background = column.IsPrimaryKey ? Avalonia.Media.Brushes.Gold : Avalonia.Media.Brushes.LightGray,
                        Margin = new Avalonia.Thickness(0, 0, 5, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };

                    // Index indicator (blue dot)
                    var indexIndicator = new Border
                    {
                        Width = 6,
                        Height = 6,
                        CornerRadius = new Avalonia.CornerRadius(3),
                        Background = column.HasIndex ? Avalonia.Media.Brushes.DodgerBlue : Avalonia.Media.Brushes.Transparent,
                        Margin = new Avalonia.Thickness(0, 0, 3, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        IsVisible = column.HasIndex
                    };

                    // Foreign key indicator
                    var fkIndicator = new TextBlock
                    {
                        Text = column.IsForeignKey ? "🔗" : "",
                        Foreground = Avalonia.Media.Brushes.Orange,
                        FontSize = 10,
                        Margin = new Avalonia.Thickness(0, 0, 3, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };

                    var columnText = new TextBlock
                    {
                        Text = CreateColumnDisplayText(column),
                        Foreground = Avalonia.Media.Brushes.White,
                        FontSize = 11,
                        FontFamily = new Avalonia.Media.FontFamily("Consolas,monospace")
                    };

                    columnPanel.Children.Add(keyIndicator);
                    if (column.HasIndex)
                        columnPanel.Children.Add(indexIndicator);
                    if (column.IsForeignKey)
                        columnPanel.Children.Add(fkIndicator);
                    columnPanel.Children.Add(columnText);
                    stackPanel.Children.Add(columnPanel);
                }

                if (table.Columns.Count > 10)
                {
                    var moreText = new TextBlock
                    {
                        Text = $"... and {table.Columns.Count - 10} more columns",
                        Foreground = Avalonia.Media.Brushes.LightGray,
                        FontSize = 10,
                        FontStyle = Avalonia.Media.FontStyle.Italic,
                        Margin = new Avalonia.Thickness(10, 3),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    };
                    stackPanel.Children.Add(moreText);
                }
            }

            border.Child = stackPanel;
            return border;
        }

        public async Task ShowErrorAsync(string errorMessage)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressOverlay.IsVisible = false;
                DiagramScrollViewer.IsVisible = false;
                EmptyState.IsVisible = true;
                
                // Update empty state to show error
                var errorPanel = EmptyState.Children.OfType<StackPanel>().FirstOrDefault();
                if (errorPanel != null)
                {
                    var textBlocks = errorPanel.Children.OfType<TextBlock>().ToList();
                    if (textBlocks.Count >= 2)
                    {
                        textBlocks[0].Text = "Analysis Failed";
                        textBlocks[1].Text = errorMessage;
                        textBlocks[1].Foreground = Avalonia.Media.Brushes.OrangeRed;
                    }
                }
            });
        }

        public void ClearDiagram()
        {
            _currentConnection = null;
            Dispatcher.UIThread.Post(() =>
            {
                ProgressOverlay.IsVisible = false;
                DiagramScrollViewer.IsVisible = false;
                EmptyState.IsVisible = true;
                
                // Reset empty state text
                var errorPanel = EmptyState.Children.OfType<StackPanel>().FirstOrDefault();
                if (errorPanel != null)
                {
                    var textBlocks = errorPanel.Children.OfType<TextBlock>().ToList();
                    if (textBlocks.Count >= 2)
                    {
                        textBlocks[0].Text = "Database Schema Diagram";
                        textBlocks[1].Text = "Select a database connection to visualize the schema";
                        textBlocks[1].Foreground = Avalonia.Media.Brushes.Gray;
                    }
                }
            });
        }

        public DatabaseConnectionDTO? CurrentConnection => _currentConnection;

        private void DrawRelationshipLine(Canvas canvas, (double X, double Y, Border Control) fromPos, (double X, double Y, Border Control) toPos, string fromTable, string toTable)
        {
            try
            {
                var fromX = fromPos.X + 140; // Center of table
                var fromY = fromPos.Y + 100; // Middle of table
                var toX = toPos.X + 140;
                var toY = toPos.Y + 100;

                // Create a simple line
                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Avalonia.Point(fromX, fromY),
                    EndPoint = new Avalonia.Point(toX, toY),
                    Stroke = Avalonia.Media.Brushes.LightBlue,
                    StrokeThickness = 2,
                    StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 5, 3 }
                };

                canvas.Children.Insert(0, line); // Insert at beginning so lines are behind tables
                
                _logger?.LogDebug($"Drew relationship line from {fromTable} to {toTable}");
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"Error drawing relationship line: {ex.Message}");
            }
        }

        private string CreateColumnDisplayText(DatabaseColumnDTO column)
        {
            var text = $"{column.ColumnName} : {column.DataType}";
            
            if (!column.IsNullable)
                text += " NOT NULL";
                
            if (column.IsForeignKey && !string.IsNullOrEmpty(column.ReferencedTable))
                text += $" → {column.ReferencedTable}";
                
            return text;
        }
    }
}