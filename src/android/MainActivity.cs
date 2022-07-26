using Android.Graphics;
using System.Globalization;
using Helper = Com.Microsoft.Lols.Helper;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace lols;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
	const int Max = 500;
	int count = 0;
	readonly System.Timers.Timer timer = new System.Timers.Timer(500);
	readonly Stopwatch stopwatch = new Stopwatch();
	TextView lols;
	RelativeLayout layout;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		SetContentView(Resource.Layout.activity_main);
		lols = RequireViewById<TextView>(Resource.Id.lols);
		layout = RequireViewById<RelativeLayout>(Resource.Id.layout);

		timer.Elapsed += OnTimer;

		stopwatch.Start();
		timer.Start();
		_ = Task.Factory.StartNew(RunTest, TaskCreationOptions.LongRunning);
	}

	void OnTimer(object? sender, System.Timers.ElapsedEventArgs e)
	{
		double avg = count / stopwatch.Elapsed.TotalSeconds;
		string text = "LOL/s: " + avg.ToString("0.00", CultureInfo.InvariantCulture);
		RunOnUiThread(() => UpdateText(text));
	}

	void UpdateText(string text) => lols!.Text = text;

	async void RunTest()
	{
		var random = Random.Shared;
		int width = layout.Width;
		int height = layout.Height;

		//TODO: something better?
		while (width == 0 || height == 0)
		{
			width = layout.Width;
			height = layout.Height;
		}

		while (count < 5000)
		{
			var color = new Color(random.Next(byte.MaxValue), random.Next(byte.MaxValue), random.Next(byte.MaxValue));
			var label = Helper.CreateTextView(this, color, random.NextSingle() * 360, random.Next(width), random.Next(height));
			RunOnUiThread(() =>
			{
				Helper.Add(layout, label);
				count++;
			});
			await Task.Delay(1);
		}

		stopwatch.Stop();
		timer.Stop();
	}
}