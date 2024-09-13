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

1. **.NET 7 SDK** - Install from [here](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).
2. **FFmpeg** - You need to have FFmpeg installed and set its path in the code:
   - Download FFmpeg from [here](https://ffmpeg.org/download.html).
   - Extract FFmpeg and update the path in the code as follows:
     - Open the `Program.cs` file.
     - Find the line:
       ```csharp
       string ffmpegPath = @"C:\ffmpeg\ffmpeg.exe";
       ```
     - Replace `C:\ffmpeg\ffmpeg.exe` with the actual path where `ffmpeg.exe` is located on your system.

3. **YoutubeExplode** - This project uses the `YoutubeExplode` library to access and download video streams from YouTube.

   To install YoutubeExplode, run the following command:
   ```bash
   dotnet add package YoutubeExplode --version 6.4.0
