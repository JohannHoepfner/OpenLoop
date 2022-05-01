using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenLoopRun;
using ScottPlot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Input;

namespace OpenLoopGUI
{
    /// <inheritdoc />
    public partial class MainWindow : Window
    {
        private readonly OpenLoopScript _script;
        private readonly Runner _r;

        /// <inheritdoc />
        public MainWindow()
        {
            _script = new OpenLoopScript();
            _r = new Runner { Script = _script };
            InitializeComponent();
            
            Plot.Plot.Style(Style.Blue2);
            Plot.Plot.Palette = Palette.OneHalfDark;
        }

        /// <summary>
        /// Updates internal script.
        /// </summary>
        private void OnScriptEdited(object? sender, KeyEventArgs keyEventArgs)
        {
            UpdateScriptFromWorkspace();
            SimProgress.Value = 0;
        }

        /// <summary>
        /// Gets script from Workspace and updates internal loop- and start-scripts.
        /// </summary>
        private void UpdateScriptFromWorkspace()
        {
            _script.Iterations = long.Parse(IterInput.Text);
            
            if (LoopCodeInput.Text is not null)
                _script.LoopCode = Regex.Split(LoopCodeInput.Text, "\r\n|\r|\n").ToList();
            
            if (StartCodeInput.Text is not null)
                _script.StartCode = Regex.Split(StartCodeInput.Text, "\r\n|\r|\n").ToList();
        }

        /// <summary>
        /// Runs the Simulation and updates variable history.
        /// </summary>
        private void RunSim_Click(object? sender, RoutedEventArgs routedEventArgs)
        {
            UpdateScriptFromWorkspace();
            
            if (XSelect.SelectedItem is not string oldX) oldX = "";
            if (YSelect.SelectedItem is not string oldY) oldY = "";
            
            XSelect.Items = null;
            YSelect.Items = null;
            
            SimProgress.Value = 0;
            
            _r.VarHistory.Clear();
            
            try
            {
                _r.RunScript();
            }
            catch
            {
                return;
            }

            SimProgress.Value = 100;
            
            XSelect.Items = _r.VarHistory.ToList()[0].Keys;
            YSelect.Items = _r.VarHistory.ToList()[0].Keys;
            
            if (VarExists(oldX))
                XSelect.SelectedItem = oldX;
            
            else
            {
                if (VarExists("t"))
                    XSelect.SelectedItem = "t";
                if (VarExists("x"))
                    XSelect.SelectedItem = "x";
            }

            if (VarExists(oldY))
                YSelect.SelectedItem = oldY;
            else
            {
                if (VarExists("y"))
                    YSelect.SelectedItem = "y";
            }

            bool VarExists(string var) => _r.VarHistory.ToList()[0].ContainsKey(var);
        }

        /// <summary>
        /// Updates the plot to show selected vars.
        /// </summary>
        private void OnVarSelectionChanged(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            Plot.Plot.Clear();
            
            var plot = Plot.Plot;
            if ( XSelect.SelectedItem is not string xSelection
                || YSelect.SelectedItem is not string ySelection
            ) return;
            
            var dataX = _r.VarHistoryT[xSelection].ToArray();
            var dataY = _r.VarHistoryT[ySelection].ToArray();
            
            var scatterPlot = plot.AddScatter(dataX, dataY);
            
            plot.XAxis.Label(xSelection);
            plot.YAxis.Label(ySelection);
            
            scatterPlot.MarkerSize = 2;
            
            Plot.Refresh();
            SimProgress.Value = 0;
        }

        /// <summary>
        /// Writes the olc-code to file.
        /// </summary>
        private async void Save_Button_Click(object? sender, RoutedEventArgs routedEventArgs)
        {
            var fileText = JsonSerializer.Serialize(
                _script,
                new JsonSerializerOptions { WriteIndented = true }
            );
            
            var filePick = new SaveFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Extensions = new List<string> { "olc" },
                        Name = "OpenLoop code files"
                    },
                    new()
                    {
                        Extensions = new List<string> { "*" },
                        Name = "All files"
                    }
                }
            };
            
            var file = await filePick.ShowAsync(this);
            if (file is null or "") return;

            await File.WriteAllTextAsync(file, fileText);
        }

        /// <summary>
        /// Updates the olc-code to match file
        /// </summary>
        private void Load_Button_Click(object? sender, RoutedEventArgs routedEventArgs)
        {
            var filePick = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Extensions = new List<string> { "olc" },
                        Name = "OpenLoop code files"
                    },
                    new()
                    {
                        Extensions = new List<string> { "*" },
                        Name = "All files"
                    }
                }
            };

            var res = filePick.ShowAsync(this).Result;
            if (res is null) return;

            var file = res[0];
            var fileText = File.ReadAllText(file);

            var script = JsonSerializer.Deserialize<OpenLoopScript>(fileText);
            if (script is null) return;

            UpdateWorkspaceFromScript(script);
            UpdateScriptFromWorkspace();

            SimProgress.Value = 0;
        }

        /// <summary>
        /// Updates internal scripts to match supplied script
        /// </summary>
        /// <param name="script"></param>
        private void UpdateWorkspaceFromScript(OpenLoopScript script)
        {
            IterInput.Text = script.Iterations.ToString();

            LoopCodeInput.Text = script.LoopCode
                .Aggregate("", (current, last) => current + last + "\n")
                .Trim();

            StartCodeInput.Text = script.StartCode
                .Aggregate("", (current, last) => current + last + "\n")
                .Trim();
        }

        /// <summary>
        /// Exports calculated data to csv.
        /// </summary>
        private async void ExportSimData_Button_Click(object sender, RoutedEventArgs e)
        {
            var data = _r.VarHistory;

            if (data.Count is 0) return;

            var keys = data.ToList()[0].Keys;

            var csv = keys.Aggregate(
                "", (current, k) => current + k + "\t"
            ).Trim();

            csv = data
                .Aggregate(csv, (current1, i)
                    => keys.Aggregate(current1 + "\n", (current, v) => current + (i[v] + "\t"))
                        .Trim());

            var filePick = new SaveFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Extensions = new List<string> { "csv" },
                        Name = "csv files"
                    },
                    new()
                    {
                        Extensions = new List<string> { "*" },
                        Name = "All files"
                    }
                }
            };
            var file = await filePick.ShowAsync(this);
            if (file is null or "")
            {
                return;
            }

            await File.WriteAllTextAsync(file, csv);
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        private void Close_Click(object? sender, RoutedEventArgs routedEventArgs) => Close();
    }
}