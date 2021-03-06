# Tesla Cam Map
## What is it?
Universal Windows Platform app to view video footage from a Tesla Dashcam / Sentry drive. Plots all events found on a map and enables user to view footage from all four cameras at once.

[Video and discussion here](https://www.reddit.com/r/teslamotors/comments/lxzhv4/i_put_together_a_windows_tesla_cam_viewer_app/)

## Development setup
Requires a Bing Map API key. The solution expects the key in a file named `bing_key` in `/src/TeslaCamMap.UwpClient` with build action set to `Content`. This file is excluded from the Git repo and you would need to register for your own key (Basic / Windows App) [here](https://www.microsoft.com/en-us/maps/create-a-bing-maps-key).

## External packages
Uses [FFMpegInteropX](https://github.com/ffmpeginteropx/FFmpegInteropX) for better video playback.

## Known issues
* To change position in a video you need to drag the thumb of the slider, "just" clicking a position in the slider does not work.
* Jumping between video segments of an event too fast can cause a crash.
* If Tesla changes anything in how these files are produced it will break. Only tested on video from a 2020 Model 3 SR+.

## Screenshots
![how to start game](https://github.com/grankko/TeslaCamMap/raw/main/screenshot_map.png)
![how to start game](https://github.com/grankko/TeslaCamMap/raw/main/screenshot_video.png)
