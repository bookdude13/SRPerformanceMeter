# Synth Riders Performance Meter
Synth Riders Performance Meter mod inspired by [MCJack123's PerformanceMeter BeatSaber mod](https://github.com/MCJack123/PerformanceMeter).

Adds an additional panel to the end game screen showing the average percentage of your life bar and a graph of the life bar over the course of the song.

More features coming soon!

![End Game Screen](/end_game_example.jpeg "End Game Screen")


## Setup
1. Prepare Synth Riders for modding by following the [MelonLoader](https://melonwiki.xyz/#/README) wiki guide
2. Download the newest version of PerformanceMeter.dll from the Releases section
3. Copy PerformanceMeter.dll to `<path-to-game>\Mods` (create new directory if it doesn't exist). On Windows this will be at `C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods`
4. Run Synth Riders and enjoy!

## Configuration
After running Synth Riders at least once with this mod, a configuration file will be created at `SynthRiders\UserData\PerformanceMeter\PerformanceMeter.cfg` that can be edited with any simple text editor. Settings are loaded at boot and are logged out in MelonLoader.

### `showAverageLine`
Default: `true`
Values: `true` or `false`
Description: Toggles the horizontal line indicating your average of the currently tracked statistic.

### `markerPeriodMs`
Default: 30000
Values: 1000 to 300000 (1 sec to 5 min)
Description: Controls the time interval that the vertical markers at the bottom of the graph represent.

### `lifeCheckPeriodMs`
Default: 100
Values: 50 to 5000 (50 ms to 5 sec)
Description: Controls how frequently life percentage data is retrieved. Increasing this value may help with performance at the cost of a less accurate graph.

## Technical Detail
- During gameplay the life bar percentage is polled occasionally

- Less than 50% life is red, 50-74.99% is orange, 75-89.99% is yellow-green, and 90%+ is green

---

## Disclaimer
This mod is not affiliated with Synth Riders or Kluge Interactive. Use at your own risk.

