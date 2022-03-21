using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenLoopGUI
{
	public partial class CodeWindow : Window
	{
		public MainWindow MW { get; set; }
		public CodeWindow()
		{
			InitializeComponent();
		}
		private void SaveClose_Click(object sender, RoutedEventArgs e)
		{
			MW.Program = new OpenLoopRun.OpenLoopProgram()
			{
				Iterations = long.Parse(this.FindControl<TextBox>("IterInput").Text),
				StartCode = Regex.Split(this.FindControl<TextBox>("startCodeInput").Text, "\r\n|\r|\n").ToList(),
				LoopCode = Regex.Split(this.FindControl<TextBox>("loopCodeInput").Text, "\r\n|\r|\n").ToList()
			};
			MW.SimProgress.Value = 0;
			Close();
		}
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
