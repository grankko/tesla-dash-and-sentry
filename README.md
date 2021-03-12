# Tesla Dashcam and Sentry video viewer
## What is it?
Universal Windows Platform app to view video footage from a Tesla Dashcam / Sentry drive. Plots all events found on a map and enables user to view footage from all four cameras at once.

[Early video and discussion on Reddit](https://www.reddit.com/r/teslamotors/comments/lxzhv4/i_put_together_a_windows_tesla_cam_viewer_app/)

[2021-03-12 demo on YouTube](https://www.youtube.com/watch?v=TEjPuF42Aq8&t=4s)

## Development setup
Requires a Bing Map API key. The solution expects the key in a file named `bing_key` in `/src/TeslaCamMap.UwpClient` with build action set to `Content`. This file is excluded from the Git repo and you would need to register for your own key (Basic / Windows App) [here](https://www.microsoft.com/en-us/maps/create-a-bing-maps-key).

## External packages
Uses [FFMpegInteropX](https://github.com/ffmpeginteropx/FFmpegInteropX) for better video playback.

## Remarks
* Only tested on video from one 2020 Model 3 SR+.

## Screenshots
![Map view](https://github.com/grankko/TeslaCamMap/raw/main/screenshot_map.png)
![Video view](https://github.com/grankko/TeslaCamMap/raw/main/screenshot_video.png)
![Full video view](https://github.com/grankko/TeslaCamMap/raw/main/screenshot_video_full.png)
