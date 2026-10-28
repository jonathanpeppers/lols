// LOLs per second in Flutter, with the same two modes, pacing, and counting as the Slint for .NET port
// (examples/lols in https://github.com/yurkinh/slint, branch dotnet_bindings).
// Pick the mode at build time: flutter build apk --dart-define=MODE=elements

import 'dart:collection';
import 'dart:io' show sleep;
import 'dart:isolate';
import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';

const mode = String.fromEnvironment('MODE', defaultValue: 'canvas');
const max = 500;
// canvas queues 100000 lols and sleeps every 256; elements adds 5000 and sleeps after each.
const total = mode == 'canvas' ? 100000 : 5000;

class Lol {
  Lol(this.x, this.y, this.color, this.rotation);
  final double x, y, rotation;
  final Color color;
}

// The background thread of the other versions: it never touches the UI, and posts lols to it.
void produce(SendPort port) {
  final random = Random();
  var batch = <List<num>>[];
  for (var produced = 1; produced <= total; produced++) {
    batch.add([random.nextDouble(), random.nextDouble(), 0xFF000000 | random.nextInt(0xFFFFFF), random.nextDouble() * 360]);
    if (mode == 'elements' || produced % 256 == 0) {
      port.send(batch);
      batch = [];
      sleep(const Duration(milliseconds: 1));
    }
  }
  if (batch.isNotEmpty) port.send(batch);
}

void main() => runApp(const MaterialApp(debugShowCheckedModeBanner: false, home: LolsPage()));

class LolsPage extends StatefulWidget {
  const LolsPage({super.key});
  @override
  State<LolsPage> createState() => _LolsPageState();
}

class _LolsPageState extends State<LolsPage> with SingleTickerProviderStateMixin {
  final queue = Queue<Lol>();
  final lols = <Lol>[];
  final stopwatch = Stopwatch()..start();
  late final Ticker ticker;
  var count = 0;
  var next = 0;
  var rate = 'LOL/s:';
  var reported = false;
  var lastRate = Duration.zero;

  @override
  void initState() {
    super.initState();
    final port = ReceivePort();
    port.listen((batch) {
      for (final l in batch as List) {
        queue.add(Lol(l[0], l[1], Color(l[2]), l[3]));
      }
    });
    Isolate.spawn(produce, port.sendPort);
    // Applies the queued lols once per frame.
    ticker = createTicker(drain)..start();
  }

  void drain(Duration elapsed) {
    var changed = false;
    while (count < total && queue.isNotEmpty) {
      final lol = queue.removeFirst();
      if (mode == 'elements') {
        if (lols.length >= max) lols.removeAt(0);
        lols.add(lol);
      } else if (lols.length < max) {
        lols.add(lol);
      } else {
        lols[next] = lol;
        next = (next + 1) % max;
      }
      count++;
      changed = true;
    }
    if (count == total && !reported) {
      stopwatch.stop();
      reported = true;
      debugPrint('$mode: ${rateText()} after $count lols in ${(stopwatch.elapsedMilliseconds / 1000).toStringAsFixed(2)} s');
    }
    if (elapsed - lastRate >= const Duration(milliseconds: 500)) {
      lastRate = elapsed;
      rate = rateText();
      changed = true;
    }
    if (changed) setState(() {});
  }

  String rateText() => 'LOL/s: ${(count / (stopwatch.elapsedMicroseconds / 1e6)).toStringAsFixed(2)}';

  @override
  void dispose() {
    ticker.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final top = MediaQuery.paddingOf(context).top;
    return Scaffold(
      backgroundColor: Colors.white,
      body: LayoutBuilder(builder: (context, size) {
        return Stack(children: [
          if (mode == 'canvas')
            Positioned.fill(child: CustomPaint(painter: LolsPainter(List.of(lols))))
          else
            for (var i = 0; i < lols.length; i++)
              Positioned(
                // Keyed by the lol, so Flutter creates a widget for each new one and drops the oldest.
                key: ObjectKey(lols[i]),
                left: lols[i].x * size.maxWidth,
                top: lols[i].y * size.maxHeight,
                child: Transform.rotate(
                  angle: lols[i].rotation * pi / 180,
                  child: Text('lol?', style: TextStyle(color: lols[i].color, fontSize: 14)),
                ),
              ),
          Positioned(
            top: top + 20,
            left: 0,
            right: 0,
            child: Center(
              child: Container(
                color: Colors.red,
                padding: const EdgeInsets.all(7),
                child: Text(rate, style: const TextStyle(color: Colors.white, fontSize: 14)),
              ),
            ),
          ),
        ]);
      }),
    );
  }
}

// One painter for all lols, like LolsView on the canvas branch of the original.
class LolsPainter extends CustomPainter {
  LolsPainter(this.lols);
  final List<Lol> lols;

  @override
  void paint(Canvas canvas, Size size) {
    for (final lol in lols) {
      final text = TextPainter(
        text: TextSpan(text: 'lol?', style: TextStyle(color: lol.color, fontSize: 14)),
        textDirection: TextDirection.ltr,
      )..layout();
      canvas.save();
      canvas.translate(lol.x * size.width, lol.y * size.height);
      canvas.rotate(lol.rotation * pi / 180);
      text.paint(canvas, Offset.zero);
      canvas.restore();
    }
  }

  @override
  bool shouldRepaint(LolsPainter old) => true;
}
