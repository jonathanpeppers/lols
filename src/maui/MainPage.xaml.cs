using Microsoft.Maui.Layouts;
using System.Diagnostics;
using System.Globalization;

namespace lols;

public partial class MainPage : ContentPage
{
	const int Max = 500;
	int count = 0;
	readonly System.Timers.Timer timer = new System.Timers.Timer(500);
	readonly Stopwatch stopwatch = new Stopwatch();

	public MainPage()
	{
		InitializeComponent();

		timer.Elapsed += OnTimer;

		stopwatch.Start();
		timer.Start();
		_ = Task.Factory.StartNew(RunTest, TaskCreationOptions.LongRunning);
	}

	void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
	{
		double avg = count / stopwatch.Elapsed.TotalSeconds;
		string text = "LOL/s: " + avg.ToString("0.00", CultureInfo.InvariantCulture);
		Dispatcher.Dispatch(() => UpdateText(text));
	}

	void UpdateText(string text) => lols.Text = text;

	async void RunTest()
	{
		var random = Random.Shared;

		while (count < 5000)
		{
			var label = new Label
			{
				Text = "lol?",
				TextColor = new Color(random.NextSingle(), random.NextSingle(), random.NextSingle()),
				Rotation = random.NextDouble() * 360
			};
			AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds(label, new Rect(random.NextDouble(), random.NextDouble(), 80, 24));
			Dispatcher.Dispatch(() =>
			{
				if (absolute.Children.Count >= Max)
					absolute.Children.RemoveAt(0);
				absolute.Children.Add(label);
				count++;
			});
			await Task.Delay(5);
		}

		stopwatch.Stop();
		timer.Stop();
	}
}

