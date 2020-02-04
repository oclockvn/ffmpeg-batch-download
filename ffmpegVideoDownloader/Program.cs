using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ffmpegVideoDownloader
{
    class Program
    {
        static void LogAndExit(string msg)
        {
            Console.Write(msg);
            Console.Read();
        }

        static async Task Main(string[] args)
        {
            var delay = 2;
            var outputDir = "Videos";

            if (args?.Length > 0)
            {
                // first arg is delay time
                if (int.TryParse(args[0], out var delayConfig))
                {
                    delay = delayConfig;
                }

                // second arg is output dir
                if (args.Length > 1)
                {
                    outputDir = args[1];
                }
            }

            const string source = "input.txt";
            if (!File.Exists(source))
            {
                LogAndExit($"'{source}' not found. Press any keys to exit...");
                return;
            }

            var videos = await File.ReadAllLinesAsync(source);
            var urls = videos?.Where(x => !string.IsNullOrWhiteSpace(x) && x.StartsWith("ffmpeg"))?.ToList();

            if (urls == null || urls.Count == 0)
            {
                LogAndExit("No links found. Press any keys to exit...");
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            Console.WriteLine($"Start downloading {urls.Count} videos, delay time = {delay} minutes, output = {outputDir}");

            foreach (var link in urls)
            {
                Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")} > [{link}]");
                var p = Process.Start(new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = true,
                    FileName = "cmd.exe",
                    Arguments = $"/C {link}"
                });
                p.EnableRaisingEvents = true;
                p.Exited += (s, e) =>
                {
                    Console.WriteLine("File downloaded, copying...");
                    // Scan for all latest video then copy to output dir
                    var downloaded = Directory.GetFiles(".", "**.mp4", SearchOption.TopDirectoryOnly);
                    if (downloaded?.Length > 0)
                    {
                        foreach (var file in downloaded)
                        {
                            File.Move(file, Path.Combine(outputDir, file), false);
                        }
                    }
                };

                await Task.Delay(delay * 60 * 1000);
            }

            LogAndExit("Done! Press any keys to exit...");
        }
    }
}
