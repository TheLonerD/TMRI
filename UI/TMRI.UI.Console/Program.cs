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
using TMRI.Primitives.Enums;
using TMRI.UI.Console.Menu;

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
                System.Console.WriteLine("No BGM definitions found.\nExiting...");

                return;
            }

            System.Console.WriteLine($"Got {products.Count} BGM definitions: ");
            foreach (var md in products)
            {
                System.Console.WriteLine($"  -{md.Key}:\t{md.Product.Name[Language.EN]}");
            }

            var menu = new Menu.Menu();
            var games = await infoLoader.GetGamesListAsync();
            if (games.Any())
            {
                System.Console.WriteLine($"\nGot {games.Count} games: ");
                foreach (var (key, path) in games)
                {
                    menu = menu.Add($"{key}:\t{path}", async () =>
                    {
                        System.Console.WriteLine($"Selecting {key}..");

                        IPacker packer = new TSAPacker();
                        var gamePath = games[key];
                        var game = products.FirstOrDefault(p => p.Key == key);

                        if (game == null)
                        {
                            System.Console.WriteLine($"No BGM definition found for {key}.");
                            
                            return false;
                        }

                        System.Console.WriteLine("Validating BGM file...");
                        var bgmPath = Path.Combine(gamePath, game.Product.PackInfo.BGMDir ?? "", game.Product.PackInfo.BGMFile);

                        if (!File.Exists(bgmPath))
                        {
                            System.Console.WriteLine($"BGM file \"{bgmPath}\" is not found.");

                            return false;
                        }

                        if (!await packer.ValidateFileAsync(game, bgmPath))
                        {
                            throw new TMRIException($"BGM file \"{bgmPath}\" is not valid.");
                        }

                        System.Console.WriteLine("BGM file is valid.");

                        System.Console.WriteLine("\nPlaylist:");
                        var playlistMenu = new Menu.Menu();
                        foreach (var trackInfo in game.Playlist)
                        {
                            var name = trackInfo.Name[Language.EN];
                            playlistMenu = playlistMenu.Add(name, async () =>
                            {
                                System.Console.WriteLine($"Playing song \"{name}\"...");

                                // TEST
                                await using var fs = new FileStream(bgmPath, FileMode.Open);
                                await using var ms = await packer.ExtractSongAsync(trackInfo, fs);
                                var playInfo = packer.GetPlayInfo(trackInfo);
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
                                        System.Console.WriteLine(" (stop playing)");
                                        d.Stop();
                                    }
                                    else
                                    {
                                        ClearCurrentConsoleLine();
                                        var time = d.GetPositionTimeSpan();
                                        System.Console.Write($"{time.Minutes:D2}:{time.Seconds:D2}");
                                        Thread.Sleep(TimeSpan.FromSeconds(.5));
                                    }
                                }

                                GC.SuppressFinalize(ms);

                                return true;
                            });
                        }

                        playlistMenu.AddInput(() => Input.ReadInt(1, game.Playlist.Count));

                        var drawPlaylist = true;
                        while (drawPlaylist)
                        {
                            drawPlaylist = await playlistMenu.Draw();
                        }
                        
                        return false;
                    });
                }

                menu = menu.AddInput(() => Input.ReadInt(1, games.Count));
            }
            else
            {
                System.Console.WriteLine("No games found. Please add game paths to the file games.json.\nExiting...");

                return;
            }

            var draw = true;
            while (draw)
            {
                draw = await menu.Draw();
            }
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
