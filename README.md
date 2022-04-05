# Synth Riders Performance Meter
Synth Riders Performance Meter mod inspired by [MCJack123's PerformanceMeter BeatSaber mod](https://github.com/MCJack123/PerformanceMeter).

Currently uses [SynthRiders-Websocket-Mod](https://github.com/KK964/SynthRiders-Websockets-Mod) to get most of the data.

Adds an additional panel to the end game screen showing various statistics from your last song run.
- Colorful line is life percentage
- White horizontal line is average life percentage
- Rising white line is high total score (saved locally)
- Rising yellow line is total score from current run

More features coming soon!

![End Game Screen](/end_game_example.jpeg "End Game Screen")


## Installing
1. Prepare Synth Riders for modding by following the [MelonLoader](https://melonwiki.xyz/#/README) wiki guide
2. Download the newest version of SRPerformanceMeter from the Releases section. Your web browser may warn you about the .zip file because it contains a .dll - this is normal, and is how mods are built.
3. Extract the contents of the .zip file to  `<path-to-synth-riders>\Mods` (create a new directory if it doesn't exist). On Windows, this will be at `C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods`
4. Run Synth Riders and enjoy!

## Configuration
After running Synth Riders at least once with this mod, a configuration file will be created at `SynthRiders\UserData\PerformanceMeter\PerformanceMeter.cfg` that can be edited with any simple text editor. Settings are loaded at boot and are logged out in MelonLoader.

### `isEnabled`
Default: `true`
Values: `true` or `false`
Description: Toggles the entire mod.

### `showAverageLine`
Default: `true`
Values: `true` or `false`
Description: Toggles the horizontal line indicating your average life percentage.

### `showLifePercentGraph`
Default: `true`
Values: `true` or `false`
Description: Toggles the graph showing life percent over the course of the song.

### `showTotalScoreComparisonGraph`
Default: `true`
Values: `true` or `false`
Description: Toggles the graph showing your current score (yellow) and last high score (white).

### `markerPeriodMs`
Default: 30000
Values: 1000 to 300000 (1 sec to 5 min)
Description: Controls the time interval that the vertical markers at the bottom of the graph represent.

## Technical Detail

- Less than 50% life is red, 50-74.99% is orange, 75-89.99% is yellow-green, and 90%+ is green
- Local data is stored in SynthRiders/UserData/PerformanceMeter/PerformanceMeter.db, a LiteDB database.

---

## Disclaimer
This mod is not affiliated with Synth Riders or Kluge Interactive. Use at your own risk.

