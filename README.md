# LOLs per second

A .NET MAUI performance test similar to:

* https://github.com/maxim-saplin/dopetest_xamarin
* https://github.com/maxim-saplin/dopetest_flutter
* https://github.com/unoplatform/performance/tree/master/src/dopes/DopeTestMaui

This puts a number of labels on the screen with a random color and
random rotation -- measuring the "LOLs per second".

In many ways, this test is a bit dubious, but it is worthwhile
comparing three types of apps that use the same underlying Android
stack and `Android.Widget.TextView`:

1. Xamarin.Forms 5 app - the `Label` class is a cross-platform layer over `TextView`, using an older Mono runtime & BCL
2. .NET MAUI app - the `Label` class is a cross-platform layer over `TextView`, using the latest runtime & BCL
3. .NET 6 Android app - use `TextView` directly, but with the interop overhead of calling from C# into Java
4. Java Android app - use `TextView` directly, without any C# to Java interop

Using these three apps, I think we have a reasonable estimate of what
the overhead of using .NET (and .NET MAUI) comparing against the Java app.

Profiling the .NET MAUI app is also quite useful, I've found several
things to fix when doing this:

* https://github.com/dotnet/maui/pull/7996
* https://github.com/dotnet/maui/pull/8001
* https://github.com/dotnet/maui/pull/8033
* https://github.com/dotnet/maui/pull/8034
* https://github.com/dotnet/maui/pull/8250

## Xamarin.Forms 5 Pixel 5

~179 per second in Xamarin.Forms 5:

<img src="docs/xamarin-forms.png" width="200" />

Thanks [@roubachof](https://github.com/roubachof) for this sample!

## .NET MAUI Android Pixel 5

~327 per second in .NET 6 MAUI:

<img src="docs/maui.png" width="200" />

Note that I was getting around 397 already in .NET 7 MAUI.

## .NET 6 Android Pixel 5

~594 per second in .NET 6:

<img src="docs/android.png" width="200" />

## Java Android Pixel 5

~682 per second using straight Java:

<img src="docs/java.png" width="200" />

## Comparing on Your Own Phone

This branch has every version of the test, updated to build with today's tools, so you can run the same comparison on your own hardware:

| Directory | Version | Mode |
| --- | --- | --- |
| [`src/java`](src/java) | Java, a `TextView` per lol | one label per lol |
| [`src/android`](src/android) | .NET for Android, a `TextView` per lol | one label per lol |
| [`src/maui`](src/maui) | .NET MAUI, a `Label` per lol | one label per lol |
| [`src/android-canvas`](src/android-canvas) | .NET for Android, one `View` that draws on a `Canvas`, from the `canvas` branch | canvas |
| [`src/flutter`](src/flutter) | Flutter, a `Text` widget per lol, or one `CustomPainter` | both, picked at build time |
| [`src/slint`](src/slint) | Slint for .NET, a `Text` element per lol, or 500 reused ones, on Android and the desktop | both, picked at launch |

The .NET versions target .NET 10, and the Java version signs its Release build with your debug key.
The two .NET for Android versions build their Java helper from [`src/android-native`](src/android-native)
with an `AndroidGradleProject` item, instead of a prebuilt `lols.aar`.
`src/xamarin-forms` is left as it was, since Xamarin is out of support.

To build every version and run each on the connected device, with the screen unlocked:

```sh
scripts/compare-android.sh
```

The script needs the .NET 10 SDK with the `android` and `maui` workloads, Flutter, JDK 17, and an arm64 device with USB debugging.
The script keeps the screen on while it runs, prints each rate, and saves screenshots in `results/`.

Results on a Motorola edge 60 neo with Android 16, Release builds:

| Version | One label per lol, 5,000 | Canvas, 100,000 |
| --- | --- | --- |
| Java | 625 LOL/s | |
| .NET for Android | 587 LOL/s | 161,656 LOL/s, counted when queued |
| .NET MAUI | 271 LOL/s, with 2 ms sleeps | |
| Flutter | 880 LOL/s | 158,000 to 170,000 LOL/s |
| Slint for .NET | 821 LOL/s | 170,000 to 204,000 LOL/s |

Read them with the notes below in mind:

- The 1 ms sleep after each label caps every app in the first column.
  The Flutter and Slint versions apply a frame's worth of labels at once, while the others post each label to the UI thread.
- In the canvas column, the Flutter and Slint versions count a lol when the UI thread has applied it,
  and the .NET for Android one when the background thread queues it.
- Runs vary by a few percent; run each a few times.

## Notes on `Thead.Sleep()` calls

Each sample has a `Thread.Sleep()` such as:

```csharp
while (count < 5000)
{
   // Do stuff
   Thread.Sleep(1);
}
```

I chose a number that the UI thread moved reasonably well visually on
a Pixel 5 device. Some samples I had to put 2, 1, or even an `if
(count % N == 0)` check.

If the number is too low, you won't see the labels "move" and the UI
will appear to freeze/lock up. If it looks smooth the number is
probably OK.

If the number is too high, you'll simply get a lower LOLs per second
than what is possible.

## How does Flutter compare?

A reported number is 12,255 per second:

<img src="https://raw.githubusercontent.com/maxim-saplin/dopetest_flutter/master/Screenshot_20200609-184950.jpg" width="200" />

Note that the test may not be exactly the same -- or running on the
same device.

The problem with this test in Flutter, is it has a different UI
paradigm than `Android.Widget.TextView`. The main bottleneck in the
Java sample is the use of `runOnUiThread`, you can't add a `TextView`
to the screen without this call.

Basically what Flutter is doing:

1. Create a "Label" that is an object that represents text

2. A render thread (running at either 60hz or 120hz) loops over the
   labels and renders them.

If I were to reproduce this same logic in C#:

1. Create a POCO (plain old C# object) with all the values.

2. Push the objects into a `ConcurrentQueue`

3. Call `Invalidate()`

4. The UI thread overrides `OnDraw()`, loop over the `ConcurrentQueue`
   and draw them.

This results in a *completely ridiculous* ~152,661 per second:

<img src="https://github.com/jonathanpeppers/lols/raw/canvas/docs/canvas.png" width="200" />

Thus, I think measuring the LOLs per second with this UI paradigm
isn't actually useful or measuring anything. Measuring how many times
per second I can create a POCO and place it in a queue? This is
effectively what Flutter is doing, and we can also achieve it in C#.
Both are basically 60/120 per second depending on how fast the UI
thread is refreshing the screen.

However! It is worth considering if *something* in .NET could be
implemented the same way as Flutter. Perhaps measuring battery or CPU
usage with 60/120 per second would be a more useful test.

You can find the code for this on the [canvas branch](https://github.com/jonathanpeppers/lols/blob/canvas/src/android/LolsView.cs).
