using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OpenLoopRun;
using ScottPlot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpenLoopGUI
{
	public partial class MainWindow : Window
	{
		readonly OpenLoopScript Script;
		Runner r;
		public MainWindow()
		{
			Script = new OpenLoopScript();
			r = new() {Script=Script };
			InitializeComponent();
			plot.Plot.Style(Style.Blue2);
			plot.Plot.Palette = Palette.OneHalfDark;
		}
		private void OnScriptEdited(object sender, KeyEventArgs e) 
		{
			UpdateScriptFromWorkspace();
			SimProgress.Value = 0;
		}
		void UpdateScriptFromWorkspace()
		{
			Script.Iterations = long.Parse(IterInput.Text);
			if (loopCodeInput.Text is not null)
				Script.LoopCode = Regex.Split(loopCodeInput.Text, "\r\n|\r|\n").ToList();
			if (startCodeInput.Text is not null)
				Script.StartCode = Regex.Split(startCodeInput.Text, "\r\n|\r|\n").ToList();
		}
		private void RunSim_Click(object sender, RoutedEventArgs e)
		{
			UpdateScriptFromWorkspace();
			if (xSelect.SelectedItem is not string oldx) oldx = "";
			if (ySelect.SelectedItem is not string oldy) oldy = "";
			xSelect.Items = null;
			ySelect.Items = null;
			SimProgress.Value = 0;
			r.RunScript();
			try
			{
				
			}
			catch { return; }
			SimProgress.Value = 100;
			xSelect.Items = r.VarHistory[0].Keys;
			ySelect.Items = r.VarHistory[0].Keys;
			if (r.VarHistory[0].Keys.Contains(oldx))
				xSelect.SelectedItem = oldx;
			else
			{
				if (r.VarHistory[0].Keys.Contains("t"))
					xSelect.SelectedItem = "t";
				if (r.VarHistory[0].Keys.Contains("x"))
					xSelect.SelectedItem = "x";
			}
			if (r.VarHistory[0].Keys.Contains(oldy))
				ySelect.SelectedItem = oldy;
			else
			{
				if (r.VarHistory[0].Keys.Contains("y"))
					ySelect.SelectedItem = "y";
			}
		}

		private void OnVarSelecionChanged(object sender, SelectionChangedEventArgs e)
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
			UpdateWorkspaceFromScript(p);
			UpdateScriptFromWorkspace();
			SimProgress.Value = 0;
		}
		void UpdateWorkspaceFromScript(OpenLoopScript s)
		{
			IterInput.Text = s.Iterations.ToString();
			loopCodeInput.Text = s.LoopCode.Aggregate("", (c, l) => c + (l + "\n")).Trim();
			startCodeInput.Text = s.StartCode.Aggregate("", (c, l) => c + (l + "\n")).Trim();
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
		private void Close_Click(object sender, RoutedEventArgs e) => Close();
	}
}