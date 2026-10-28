#!/bin/bash
# Builds every version of the test, runs each on the Android device that adb uses, and prints the rates.
# Screenshots go to results/.
#
# Usage: scripts/compare-android.sh [--no-build]
#
# Needs the .NET 10 SDK with the android and maui workloads, Flutter, JDK 17 for the Java version,
# and an arm64 Android device with USB debugging, unlocked.

set -eu
cd "$(dirname "$0")/.."

build=true
[ "${1:-}" = "--no-build" ] && build=false
out=results
mkdir -p "$out" apks

java_home=${JAVA17_HOME:-$(/usr/libexec/java_home -v 17 2> /dev/null || echo "${JAVA_HOME:-}")}

if $build; then
    (cd src/java && JAVA_HOME=$java_home sh ./gradlew --no-daemon -q assembleRelease)
    cp src/java/app/build/outputs/apk/release/app-release.apk apks/java.apk
    dotnet build src/android/lols.csproj -c Release -v q
    cp src/android/bin/Release/net10.0-android/android-arm64/com.companyname.android-Signed.apk apks/dotnet-android.apk
    dotnet build src/android-canvas/lols.csproj -c Release -v q
    cp src/android-canvas/bin/Release/net10.0-android/android-arm64/com.companyname.android.canvas-Signed.apk apks/dotnet-android-canvas.apk
    dotnet build src/maui/lols.csproj -c Release -f net10.0-android -v q
    cp src/maui/bin/Release/net10.0-android/com.companyname.lols-Signed.apk apks/maui.apk
    for mode in elements canvas; do
        (cd src/flutter && flutter build apk --release --target-platform android-arm64 --dart-define=MODE=$mode)
        cp src/flutter/build/app/outputs/flutter-apk/app-release.apk apks/flutter-$mode.apk
    done
    dotnet build src/slint/android/LolsAndroid.csproj -c Release -v q
    cp src/slint/android/bin/Release/net10.0-android/dev.slint.dotnet.lols-Signed.apk apks/slint.apk
fi

# Keeps the screen on while the tests run, since a locked phone draws nothing.
stayon=$(adb shell settings get global stay_on_while_plugged_in | tr -d '\r')
trap 'adb shell settings put global stay_on_while_plugged_in "$stayon"' EXIT
adb shell svc power stayon usb

# run <name> <apk> <package> <seconds> [mode]
run() {
    local name=$1 apk=$2 package=$3 seconds=$4 mode=${5:-}
    [ -f "$apk" ] || { echo "$name: $apk not found, skipped"; return; }
    adb install -r "$apk" > /dev/null
    adb logcat -c
    local activity
    activity=$(adb shell cmd package resolve-activity --brief "$package" | tail -n 1 | tr -d '\r')
    if [ -n "$mode" ]; then
        adb shell am start -W -n "$activity" --es mode "$mode" > /dev/null
    else
        adb shell am start -W -n "$activity" > /dev/null
    fi
    sleep "$seconds"
    adb exec-out screencap -p > "$out/$name.png"
    # Flutter and Slint print the final rate; the others only show it, so it's read from the view hierarchy.
    local rate
    rate=$(adb logcat -d | grep -o 'LOL/s: [0-9.]* after' | head -n 1 | grep -o '[0-9.]*' || true)
    if [ -z "$rate" ]; then
        adb shell uiautomator dump /sdcard/lols.xml > /dev/null 2>&1 || true
        rate=$(adb shell cat /sdcard/lols.xml 2> /dev/null | grep -o 'LOL/s: [0-9.,]*' | head -n 1 | grep -o '[0-9.,]*' || true)
        adb shell rm -f /sdcard/lols.xml
    fi
    adb shell am force-stop "$package"
    printf '%-24s %s\n' "$name" "${rate:-no rate, see $out/$name.png}"
}

echo "One label per lol, 5,000:"
run java apks/java.apk com.microsoft.javalols 20
run dotnet-android apks/dotnet-android.apk com.companyname.android 20
run maui apks/maui.apk com.companyname.lols 30
run flutter-elements apks/flutter-elements.apk dev.slint.lols_flutter 20
run slint-elements apks/slint.apk dev.slint.dotnet.lols 20 elements
echo "Canvas, 100,000:"
run dotnet-android-canvas apks/dotnet-android-canvas.apk com.companyname.android.canvas 15
run flutter-canvas apks/flutter-canvas.apk dev.slint.lols_flutter 15
run slint-canvas apks/slint.apk dev.slint.dotnet.lols 15 canvas
