# YouTube Playlist Downloader

This project is a console application that allows users to download videos from a YouTube playlist and merge video and audio streams into a single MP4 file using FFmpeg. It also provides options for selecting different video qualities.

## Features

- Downloads videos and audio separately from a YouTube playlist.
- Allows the user to select video quality:
  - Custom quality selection.
  - Highest quality.
  - Medium quality.
- Merges video and audio streams into a single MP4 file using FFmpeg.
- Supports multiple downloads in a loop.

## Requirements

1. **.NET 6 SDK** - Install from [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
2. **FFmpeg** - You need to have FFmpeg installed and its path set correctly in the code:
   - You can download FFmpeg from [here](https://ffmpeg.org/download.html).
   - Set the `ffmpegPath` variable in the code to the path where `ffmpeg.exe` is installed on your machine.

3. **YoutubeExplode** - This project uses the `YoutubeExplode` library to access and download video streams from YouTube.

   To install YoutubeExplode, run the following command:
   ```bash
   dotnet add package YoutubeExplode --version 6.0.0
