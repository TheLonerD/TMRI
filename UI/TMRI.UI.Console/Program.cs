using System;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Utils;
using NAudio.Wave;
using TMRI.Infrastructure.Implementations;
using TMRI.Infrastructure.Implementations.Packers;
using TMRI.Infrastructure.Interfaces;
using TMRI.Primitives;
using TMRI.Primitives.Definitions;
using TMRI.Primitives.Enums;

namespace TMRI.UI.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            IInfoLoader infoLoader = new TMRIInfoLoader();
            await infoLoader.PrepareLoaderAsync();

            var products = await infoLoader.GetMusicListAsync();

            if (products.Count == 0)
            {
                System.Console.WriteLine("No BGM definitions found. Exiting...");

                return;
            }

            System.Console.WriteLine($"Got {products.Count} BGM definitions: ");
            foreach (var md in products)
            {
                System.Console.WriteLine($"  -{md.Key}:\t{md.Product.Name[Language.EN]}");
            }

            var games = await infoLoader.GetGamesListAsync();
            if (games.Any())
            {
                var valid = games.Where(g => products.Any(p => p.Key.Equals(g.Key)))
                    .ToDictionary(g => g.Key, g => g.Value);

                System.Console.WriteLine($"Got {valid.Count} games: ");
                foreach (var (key, path) in valid)
                {
                    System.Console.WriteLine($"  - {key}:\t{path}");
                }
            }
            else
            {
                System.Console.WriteLine("No games found. Exiting...");

                return;
            }

            // Testing with TH15..
            System.Console.WriteLine("Opening th15.json...");
            var th15 = products.First(p => p.Path.Contains("th15.json"));

            var bgm = string.Empty;
            IPacker packer = new TSAPacker();
            if (games.ContainsKey("th15"))
            {
                var th15Path = games["th15"];
                System.Console.WriteLine("Found game path for th15.json! Validating BGM file...");
                var bgmPath = Path.Combine(th15Path, th15.Product.PackInfo.BGMDir ?? "", th15.Product.PackInfo.BGMFile);

                if (!File.Exists(bgmPath))
                {
                    throw new TMRIException($"BGM file \"{bgmPath}\" is not found.");
                }

                bgm = bgmPath;

                if (!await packer.ValidateFileAsync(th15, bgm))
                {
                    throw new TMRIException($"BGM file \"{bgm}\" is not valid.");
                }

                System.Console.WriteLine("BGM file is valid.");
            }

            if (string.IsNullOrWhiteSpace(bgm))
            {
                System.Console.WriteLine("Path to game is not set in games list. Quitting...");

                return;
            }

            System.Console.WriteLine($"Got {th15.Playlist.Count} songs:");
            foreach (var trackInfo in th15.Playlist)
            {
                System.Console.WriteLine($"  {trackInfo.Number}. {trackInfo.Name[Language.EN]}");
            }

            System.Console.WriteLine("\nType \"list\" to show playlist, " +
                                                    "\"exit\" or \"quit\" to exit from this program.");

            var showMenu = true;
            while (showMenu)
            {
                showMenu = await DrawInput(th15, bgm, packer);
            }
        }

        public static async Task<bool> DrawInput(MusicDefinition md, string bgm, IPacker packer)
        {
            System.Console.Write("\nWhat song to play: ");
            var input = await System.Console.In.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)
                || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (input.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var trackInfo in md.Playlist)
                {
                    System.Console.WriteLine($"  {trackInfo.Number}. {trackInfo.Name[Language.EN]}");
                }

                return true;
            }

            if (int.TryParse(input, out var songNum))
            {
                if (songNum < 0 || songNum > md.Product.Tracks)
                {
                    System.Console.WriteLine($"\nNumber should between 1 and {md.Product.Tracks}. " +
                                                            "Please try again.");
                    return true;
                }

                var song = md.Playlist[songNum - 1];
                System.Console.WriteLine($"Playing song \"{song.Name[Language.EN]}\"...");

                // TEST
                await using var fs = new FileStream(bgm, FileMode.Open);
                await using var ms = await packer.ExtractSongAsync(song, fs);
                var playInfo = packer.GetPlayInfo(song);
                await using var wav = new RawSourceWaveStream(ms, new WaveFormat());
                await using var provider = new LoopStream(wav)
                {
                    LoopPosition = playInfo.Loop - playInfo.Start,
                    LoopCount = 1
                };
                provider.Seek(0, SeekOrigin.Begin);

                using var d = new WaveOutEvent();
                d.Init(provider);
                d.Play();

                while (d.PlaybackState == PlaybackState.Playing)
                {
                    if (System.Console.KeyAvailable)
                    {
                        System.Console.Write(" (stop playing)");
                        d.Stop();
                    }
                    else
                    {
                        ClearCurrentConsoleLine();
                        var time = d.GetPositionTimeSpan();
                        System.Console.Write($"{time.Minutes:D2}:{time.Seconds:D2}");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }

                GC.SuppressFinalize(ms);

                return true;
            }

            System.Console.WriteLine("\nIncorrect input. " +
                                                    $"Expected: [1-{md.Product.Tracks}], \"list\", \"exit\", \"quit\"");

            return true;
        }

        public static void ClearCurrentConsoleLine()
        {
            var currentLineCursor = System.Console.CursorTop;
            System.Console.SetCursorPosition(0, System.Console.CursorTop);
            System.Console.Write(new string(' ', System.Console.WindowWidth));
            System.Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
