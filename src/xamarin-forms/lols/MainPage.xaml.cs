using System;

using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

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

        MainThread.BeginInvokeOnMainThread(() => UpdateText(text));
    }

    void UpdateText(string text) => lols.Text = text;

	void RunTest()
	{
		var random = new Random();

		while (count < 5000)
		{
			var label = new Label
			{
				Text = "lol?",
				TextColor = new Color(random.NextDouble(), random.NextDouble(), random.NextDouble()),
				Rotation = random.NextDouble() * 360
			};
			AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds(label, new Rect(random.NextDouble(), random.NextDouble(), 80, 24));
            MainThread.BeginInvokeOnMainThread(
                () =>
                    {
                        if (absolute.Children.Count >= Max)
                            absolute.Children.RemoveAt(0);
                        absolute.Children.Add(label);
                        count++;
                    });

            //NOTE: plain Android we could put 1
			Thread.Sleep(2);
		}

		stopwatch.Stop();
		timer.Stop();
	}
}

