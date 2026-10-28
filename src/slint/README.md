# LOLs per Second in Slint for .NET

A version of the test in [Slint](https://slint.dev), with the UI in [`ui/lols.slint`](ui/lols.slint)
and the logic in C# with the [`yurkinh.slint.net`](https://www.nuget.org/packages/yurkinh.slint.net) package,
a community build of the Slint .NET bindings.
The desktop and Android projects share [`LolsBenchmark.cs`](LolsBenchmark.cs).

It has two modes:

- `canvas`, the default, like `LolsView` on the `canvas` branch:
  a background thread queues 100,000 lols and sleeps 1 ms after every 256.
  The window shows the latest 500 in a fixed set of rows, which tell Slint once per frame which of them changed,
  so Slint updates properties and creates no elements.
- `elements`, like the `TextView` version:
  a background thread queues 5,000 lols and sleeps 1 ms after each.
  Slint adds a `Text` for each lol and removes the oldest one at 500.

In both modes, a timer applies the queued lols about once per frame, and the count goes up as each lol reaches the UI.
The app prints the final rate when all lols are in.

On the desktop:

```sh
dotnet run --project desktop -- elements
```

On Android, with the `android` workload of .NET 10:

```sh
dotnet build android -c Release
adb install -r android/bin/Release/net10.0-android/dev.slint.dotnet.lols-Signed.apk
adb shell am start -n dev.slint.dotnet.lols/lols.MainActivity --es mode elements
adb logcat -s Lols
```
