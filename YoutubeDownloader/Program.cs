using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

class Program
{

    static async Task Main(string[] args)
    {

        while (true)
        {
            Console.WriteLine("Enter the download path:");
            string downloadDir = Console.ReadLine();

            Console.WriteLine("Enter the YouTube playlist URL:");
            string playlistUrl = Console.ReadLine();

            var youtube = new YoutubeClient();

            try
            {
                // Get playlist metadata
                var playlist = await youtube.Playlists.GetAsync(playlistUrl);
                Console.WriteLine($"Playlist Title: {playlist.Title}");

                // Get all videos in the playlist
                var videos = await youtube.Playlists.GetVideosAsync(playlist.Id);
                Console.WriteLine($"Number of videos in playlist: {videos.Count}");

                Console.WriteLine("1-Choose The Quailty 2- Highest Quailty 3- medium Quilty");
                int selectedQualityWay = int.Parse(Console.ReadLine());
                foreach (var video in videos)
                {
                    IAudioStreamInfo selectedAudioStream = null;
                    IVideoStreamInfo selectedVideoStream = null;
                    Console.WriteLine($"\nDownloading: {video.Title}");

                    // Sanitize the video title for use as a file name
                    string sanitizedTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                    StreamManifest streamManifest;
                    try
                    {
                        // Get stream manifest
                         streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                    }
                    catch (YoutubeExplode.Exceptions.VideoUnplayableException ex)
                    {
                        Console.WriteLine($"Skipping unplayable video: {video.Title} ({ex.Message})");
                        continue; 
                    }

                    // Get available video streams
                    var videoStreams = streamManifest.GetVideoStreams().OrderByDescending(s => s.VideoQuality).ToList();
                    // Get available audio streams
                    var audioStreams = streamManifest.GetAudioStreams().OrderByDescending(s => s.Bitrate).ToList();

                    if (selectedQualityWay == 1)
                    {
                        // Display available video quality options to the user
                        Console.WriteLine("Available video quality options:");
                        for (int i = 0; i < videoStreams.Count; i++)
                        {
                            var stream = videoStreams[i];
                            double bitrateMbps = stream.Bitrate.KiloBitsPerSecond / 1000.0;
                            Console.WriteLine($"{i + 1}. {stream.VideoQuality.Label} - {bitrateMbps} Mbps");
                        }

                        // Get the user's video choice
                        Console.WriteLine("Enter the number corresponding to your desired video quality:");
                        int videoChoice = int.Parse(Console.ReadLine()) - 1;

                        if (videoChoice < 0 || videoChoice >= videoStreams.Count)
                        {
                            Console.WriteLine("Invalid choice. Skipping download.");
                            continue;
                        }

                        selectedVideoStream = videoStreams[videoChoice];

                        // Display available audio quality options to the user
                        Console.WriteLine("Available audio quality options:");
                        for (int i = 0; i < audioStreams.Count; i++)
                        {
                            var stream = audioStreams[i];
                            double bitrateKbps = stream.Bitrate.KiloBitsPerSecond;
                            Console.WriteLine($"{i + 1}. {bitrateKbps} Kbps");
                        }

                        // Get the user's audio choice
                        Console.WriteLine("Enter the number corresponding to your desired audio quality:");
                        int audioChoice = int.Parse(Console.ReadLine()) - 1;

                        if (audioChoice < 0 || audioChoice >= audioStreams.Count)
                        {
                            Console.WriteLine("Invalid choice. Skipping download.");
                            continue;
                        }

                        selectedAudioStream = audioStreams[audioChoice];
                    }
                    else if (selectedQualityWay == 2)
                    {

                        selectedVideoStream = videoStreams.FirstOrDefault();
                        selectedAudioStream = audioStreams.FirstOrDefault();
                    }
                    else if(selectedQualityWay == 3)
                    {
                        var videoStreamsCount = videoStreams.Count();
                  
                        if (videoStreamsCount >= 2)
                        {
                            selectedVideoStream = videoStreams.ElementAt(1);
                        }
                        else
                        {
                            selectedVideoStream = videoStreams[videoStreamsCount];
                        }
                        selectedAudioStream = audioStreams.FirstOrDefault();
                        
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Skipping download.");

                    }
                    if (selectedVideoStream != null && selectedAudioStream != null)
                    {
                        // Define file paths for the video and audio files
                        var videoFilePath = Path.Combine(downloadDir, $"{sanitizedTitle}_video.{selectedVideoStream.Container.Name}");
                        var audioFilePath = Path.Combine(downloadDir, $"{sanitizedTitle}_audio.{selectedAudioStream.Container.Name}");
                        var finalFilePath = Path.Combine(downloadDir, $"{sanitizedTitle}.mp4");

                        // Ensure the file names are unique
                        videoFilePath = GetUniqueFilePath(videoFilePath);
                        audioFilePath = GetUniqueFilePath(audioFilePath);
                        finalFilePath = GetUniqueFilePath(finalFilePath);

                        // Download the video and audio streams separately
                        await youtube.Videos.Streams.DownloadAsync(selectedVideoStream, videoFilePath);
                        await youtube.Videos.Streams.DownloadAsync(selectedAudioStream, audioFilePath);

                        // Merge video and audio using FFmpeg
                        MergeVideoAndAudio(videoFilePath, audioFilePath, finalFilePath);

                        Console.WriteLine($"Downloaded and merged: {video.Title}");
                    }
                    else
                    {
                        Console.WriteLine($"No suitable video or audio stream found for {video.Title}");
                    }


                }

                Console.WriteLine("All videos have been downloaded.");
            }
        

            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
    static string GetUniqueFilePath(string filePath)
    {
        int count = 1;
        string fileNameOnly = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        string directory = Path.GetDirectoryName(filePath);
        string newFullPath = filePath;

        while (File.Exists(newFullPath))
        {
            string tempFileName = $"{fileNameOnly}({count++})";
            newFullPath = Path.Combine(directory, tempFileName + extension);
        }

        return newFullPath;
    }

    static void MergeVideoAndAudio(string videoPath, string audioPath, string outputPath)
    {
        string ffmpegPath = @"C:\ffmpeg\ffmpeg.exe";

        var processInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $"-i \"{videoPath}\" -i \"{audioPath}\" -c copy \"{outputPath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process())
        {
            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;

            //process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            //process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit(); // Ensure the process has fully exited before continuing

            // Check if the process exited successfully
            if (process.ExitCode == 0)
            {
                Console.WriteLine("Merging completed successfully.");
                try
                {
                    // Delete the original separate video and audio files
                    File.Delete(videoPath);
                    File.Delete(audioPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting files: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Merging failed.");
            }
        }
    }

}


