using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenLoopRun;
using ScottPlot;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenLoopGUI
{
	public partial class MainWindow : Window
	{
		public OpenLoopProgram Program { get; set; }
		Runner r;
		public MainWindow()
		{
			r = new();
			Program = new OpenLoopProgram();
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
			c.FindControl<TextBox>("IterInput").Text = Program.Iterations.ToString();
			string loopCode = "";
			foreach (var line in Program.LoopCode)
			{
				loopCode += line + "\n";
			}
			string startCode = "";
			foreach (var line in Program.StartCode)
			{
				startCode += line + "\n";
			}
			startCode = startCode.Trim();
			loopCode = loopCode.Trim();
			c.FindControl<TextBox>("loopCodeInput").Text = loopCode;
			c.FindControl<TextBox>("startCodeInput").Text = startCode;
			c.ShowDialog(this);
		}
		private void RunSim_Click(object sender, RoutedEventArgs e)
		{
			xSelect.Items = null;
			ySelect.Items = null;
			SimProgress.Value = 0;
			r = new() { Program = this.Program };
			try
			{
				r.Start();
				for (int i = 0; i < Program.Iterations; i++)
				{
					r.Step();
					// SimProgress.Value = 100*i/(double)Program.Iterations;
				}
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
				var xselection = xSelect.SelectedItem as string;
				var yselection = ySelect.SelectedItem as string;
				var dataX = r.VarHistoryT()[xselection].ToArray();
				var dataY = r.VarHistoryT()[yselection].ToArray();
				var s = p.AddScatter(dataX, dataY);
				p.XAxis.Label(xselection);
				p.YAxis.Label(yselection);
				s.MarkerSize = 2;
			}
			catch { }
			plot.Refresh();
			SimProgress.Value = 0;
		}

		private async void Save_Button_Click(object sender, RoutedEventArgs e)
		{
			var fileText = JsonSerializer.Serialize<OpenLoopProgram>(
				value: Program,
				options: new JsonSerializerOptions() { WriteIndented = true }
				);
			var filePick = new SaveFileDialog();
			filePick.Filters = new List<FileDialogFilter> {
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
			};
			var res = await filePick.ShowAsync(this);
			if (res is null || res == "") { return; }
			var file = res;
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
			var p = JsonSerializer.Deserialize<OpenLoopProgram>(fileText);
			if (p is null) { return; }
			Program = p;
			SimProgress.Value = 0;
		}
		private void CloseWindow_Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
