using System.Globalization;
using Helper = Com.Microsoft.Lols.Helper;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace lols;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
	int count = 0;
	readonly System.Timers.Timer timer = new System.Timers.Timer(500);
	readonly Stopwatch stopwatch = new Stopwatch();
	TextView lols;
	RelativeLayout layout;
	LolsView lolsView;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		SetContentView(Resource.Layout.activity_main);
		lols = RequireViewById<TextView>(Resource.Id.lols);
		layout = RequireViewById<RelativeLayout>(Resource.Id.layout);
		layout.AddView(lolsView = new LolsView (this), 0);

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

	void RunTest()
	{
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
			lolsView.AddLol(width, height);
			count++;
			Thread.Sleep(1);
		}

		stopwatch.Stop();
		timer.Stop();
	}
}