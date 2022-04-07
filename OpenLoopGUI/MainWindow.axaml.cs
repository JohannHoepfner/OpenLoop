using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenLoopRun;
using ScottPlot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OpenLoopGUI
{
	public partial class MainWindow : Window
	{
		public OpenLoopScript Script { get; set; }
		Runner r;
		public MainWindow()
		{
			r = new();
			Script = new OpenLoopScript();
			InitializeComponent();
			plot.Plot.Style(Style.Blue2);
			plot.Plot.Palette = Palette.OneHalfDark;
		}
		void OpenCodeWindow_Click(object sender, RoutedEventArgs e)
		{
			CodeWindow c = new()
			{
				MW = this
			};
			c.FindControl<TextBox>("IterInput").Text = Script.Iterations.ToString();
			var loopCode =
				Script.LoopCode.Aggregate(
					"", (current, line) => current + (line + "\n")
					).Trim();
			var startCode =
				Script.StartCode.Aggregate(
					"", (current, line) => current + (line + "\n")
					).Trim();
			c.FindControl<TextBox>("loopCodeInput").Text = loopCode;
			c.FindControl<TextBox>("startCodeInput").Text = startCode;
			c.ShowDialog(this);
		}
		private void RunSim_Click(object sender, RoutedEventArgs e)
		{
			xSelect.Items = null;
			ySelect.Items = null;
			SimProgress.Value = 0;
			r = new() { Script = this.Script };
			try
			{
				r.RunScript();
			}
			catch { return; }
			SimProgress.Value = 100;
			xSelect.Items = r.VarHistory[0].Keys;
			ySelect.Items = r.VarHistory[0].Keys;
		}

		private void UpdatePlotButton_Click(object sender, RoutedEventArgs e)
		{
			plot.Plot.Clear();
			var p = plot.Plot;
			try
			{
				var xSelection = xSelect.SelectedItem as string;
				var ySelection = ySelect.SelectedItem as string;
				var dataX = r.VarHistoryT[xSelection].ToArray();
				var dataY = r.VarHistoryT[ySelection].ToArray();
				var s = p.AddScatter(dataX, dataY);
				p.XAxis.Label(xSelection);
				p.YAxis.Label(ySelection);
				s.MarkerSize = 2;
			}
			catch { }
			plot.Refresh();
			SimProgress.Value = 0;
		}

		private async void Save_Button_Click(object sender, RoutedEventArgs e)
		{
			var fileText = JsonSerializer.Serialize<OpenLoopScript>(
				value: Script,
				options: new JsonSerializerOptions() { WriteIndented = true }
				);
			var filePick = new SaveFileDialog
			{
				Filters = new List<FileDialogFilter> {
					new FileDialogFilter()
					{
						Extensions = new List<string> { "olc"},
						Name = "OpenLoop code files"
					},
					new FileDialogFilter()
					{
						Extensions = new List<string> { "*"},
						Name = "All files"
					}
				}
			};
			var file = await filePick.ShowAsync(this);
			if (file is null || file == "") { return; }
			File.WriteAllText(path: file, contents: fileText);
		}

		private void Load_Button_Click(object sender, RoutedEventArgs e)
		{
			var filePick = new OpenFileDialog
			{
				Filters = new List<FileDialogFilter> {
					new FileDialogFilter()
					{
						Extensions = new List<string> { "olc"},
						Name = "OpenLoop code files"
					},
					new FileDialogFilter()
					{
						Extensions = new List<string> { "*"},
						Name = "All files"
					}
				}
			};
			var res = filePick.ShowAsync(this).Result;
			if (res is null) { return; }
			var file = res[0];
			var fileText = File.ReadAllText(path: file);
			var p = JsonSerializer.Deserialize<OpenLoopScript>(fileText);
			if (p is null) { return; }
			Script = p;
			SimProgress.Value = 0;
		}
		private async void ExportSimData_Button_Click(object sender, RoutedEventArgs e)
		{

			var data = r.VarHistory;
			if (data is null || data.Count is 0) { return; }
			string csv;
			var keys = data[0].Keys;
			csv = keys.Aggregate(
				"", (current, k) => current + (k + "\t")
				);
			csv = csv.Trim();
			foreach (var i in data)
			{
				csv += "\n";
				csv = keys.Aggregate(
					csv, (current, v) => current + (i[v] + "\t")
					);
				csv = csv.Trim();
			}
			var filePick = new SaveFileDialog
			{
				Filters = new List<FileDialogFilter> {
					new FileDialogFilter()
					{
						Extensions = new List<string> { "csv"},
						Name = "csv files"
					},
					new FileDialogFilter()
					{
						Extensions = new List<string> { "*"},
						Name = "All files"
					}
				}
			};
			var file = await filePick.ShowAsync(this);
			if (file is null or "") { return; }
			await File.WriteAllTextAsync(path: file, contents: csv);
		}

		private void CloseWindow_Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}