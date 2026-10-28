# LOLs per Second in Flutter

A Flutter version of the test, written to compare with the
[Slint for .NET port](https://github.com/yurkinh/slint/tree/dotnet_bindings/examples/lols), with the same two modes, pacing, and counting:

- `canvas`: a background isolate queues 100,000 lols and sleeps 1 ms after every 256,
  and one `CustomPainter` draws the latest 500 each frame.
- `elements`: a background isolate queues 5,000 lols and sleeps 1 ms after each,
  and each lol is a keyed, rotated `Text` widget; the oldest goes at 500.

In both modes, a ticker applies the queued lols once per frame, and the count goes up as each lol reaches the UI.
The app prints the final rate to the log.

```sh
flutter build apk --release --dart-define=MODE=elements
adb install -r build/app/outputs/flutter-apk/app-release.apk
adb logcat -s flutter
```
