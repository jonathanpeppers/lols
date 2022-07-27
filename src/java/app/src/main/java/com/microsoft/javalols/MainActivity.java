package com.microsoft.javalols;

import androidx.appcompat.app.AppCompatActivity;

import android.graphics.Color;
import android.os.Bundle;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import android.widget.TextView;

import java.text.DecimalFormat;
import java.time.Duration;
import java.time.Instant;
import java.util.Random;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.TimeUnit;

public class MainActivity extends AppCompatActivity {

	final int Max = 500;
	final DecimalFormat format = new DecimalFormat("0.00");
	int count = 0;
	TextView lols;
	RelativeLayout layout;
	Timer timer;
	long start;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		lols = requireViewById(R.id.lols);
		layout = requireViewById(R.id.layout);

		timer = new Timer();
		timer.schedule(new TimerTask() {
			@Override
			public void run() {
				onTimer();
			}
		}, 0, 500);

		start = System.nanoTime();

		Thread thread = new Thread(() -> runTest());
		thread.start();
	}

	void onTimer() {
		double duration = (double)(System.nanoTime() - start) / 1_000_000_000;
		double avg = (double)count / duration;
		String text = "LOL/s: " + format.format(avg);
		runOnUiThread(() -> lols.setText(text));
	}

	void runTest() {
		Random random = new Random();
		int width = layout.getWidth();
		int height = layout.getHeight();

		//TODO: something better?
		while (width == 0 || height == 0) {
			width = layout.getWidth();
			height = layout.getHeight();
		}

		while (count < 5000) {
			TextView textView = new TextView(this);
			textView.setText("lol?");
			textView.setTextColor(Color.valueOf(random.nextFloat(), random.nextFloat(), random.nextFloat()).toArgb());
			textView.setRotation(random.nextFloat() * 360);

			RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT);
			layoutParams.leftMargin = random.nextInt(width);
			layoutParams.topMargin = random.nextInt(height);
			textView.setLayoutParams(layoutParams);

			runOnUiThread(() -> {
				int childCount = layout.getChildCount();
				if (childCount > 500)
					layout.removeViewAt(childCount - 2);
				layout.addView(textView, 0);
				count++;
			});

			try {
				Thread.sleep(1);
			} catch (InterruptedException e) {
				throw new RuntimeException(e);
			}
		}

		timer.cancel();
	}
}