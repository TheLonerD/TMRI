using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMRI.Infrastructure;
using TMRI.Infrastructure.Implementations;
using TMRI.Infrastructure.Interfaces;
using TMRI.Primitives;
using TMRI.Primitives.Config;
using TMRI.Primitives.Definitions;

namespace TMRI.Console
{
    class Program
    {
        public static TMRIStorage Storage { get; } = new TMRIStorage
        {
            Config = new AppConfig(),
            GamesDir = new GameDirectories(),
            Products = new List<MusicDefinition>()
        };
        
        static async Task Main(string[] args)
        {
            IInfoLoader infoLoader = new TMRIInfoLoader();
            IInfoWriter infoWriter = new TMRIInfoWriter();

            await infoLoader.PrepareLoaderAsync();
            await infoWriter.PrepareWriteAsync();
            
            var games = await infoLoader.GetGamesListAsync();

            await System.Console.Out.WriteLineAsync($"Got {games.Count} games: ");
            foreach (var gameDirectory in games)
            {
                await System.Console.Out.WriteLineAsync($"  - {gameDirectory.Key}: {gameDirectory.Value}");
            }

            var products = await infoLoader.GetMusicListAsync();

            if (products.Count == 0)
            {
                await System.Console.Out.WriteLineAsync("No BGM definitions are loaded. Exiting...");
            }

            await System.Console.Out.WriteLineAsync($"Got {products.Count} BGM definitions: ");

            // Testing with TH15..
            await System.Console.Out.WriteLineAsync("Opening th15.json...");
            var th15 = products.First(p => p.Path.Contains("th15.json"));

            var bgm = string.Empty;
            IPacker packer = new TSAPacker();
            if (games.ContainsKey("th15"))
            {
                var th15Path = games["th15"];
                await System.Console.Out.WriteLineAsync($"Found game path for th15.json! Validating BGM file...");
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

                await System.Console.Out.WriteLineAsync("BGM file is valid.");
            }

            if (string.IsNullOrWhiteSpace(bgm))
            {
                await System.Console.Out.WriteLineAsync("Path to game is not set in settings. Quitting...");
            }

            await System.Console.Out.WriteLineAsync($"Got {th15.Playlist.Count} songs:");
            foreach (var trackInfo in th15.Playlist)
            {
                await System.Console.Out.WriteLineAsync($"  {trackInfo.Number}. {trackInfo.Name[Language.EN]}");
            }
            await System.Console.Out.WriteLineAsync($"\nType \"list\" to show playlist, \"exit\" or \"quit\" to exit from this program.");

            bool showMenu = true;
            while (showMenu)
            {
                showMenu = await DrawInput(th15, bgm, packer);
            }
        }

        public static async Task<bool> DrawInput(MusicDefinition md, string bgm, IPacker packer)
        {
            await System.Console.Out.WriteAsync($"\nWhat song to play: ");
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
                    await System.Console.Out.WriteLineAsync($"  {trackInfo.Number}. {trackInfo.Name[Language.EN]}");
                }

                return true;
            }

            if (int.TryParse(input, out var songNum))
            {
                if (songNum < 0 || songNum > md.Product.Tracks)
                {
                    await System.Console.Out.WriteLineAsync($"\nNumber should between 1 and {md.Product.Tracks}. Please try again.");
                    return true;
                }

                var song = md.Playlist[songNum - 1];
                await System.Console.Out.WriteLineAsync($"Playing song \"{song.Name[Language.EN]}\"...");

                // TEST
                await using var fs = new FileStream(bgm, FileMode.Open);
                await using var ms = await packer.ExtractSongAsync(song, fs);

                await System.Console.Out.WriteLineAsync($"Zzzzzzzz...");
                Thread.Sleep(TimeSpan.FromSeconds(10));

                return true;
            }

            await System.Console.Out.WriteLineAsync($"\nIncorrect number format. Please try again.");
            return true;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = System.Console.CursorTop;
            System.Console.SetCursorPosition(0, System.Console.CursorTop);
            System.Console.Write(new string(' ', System.Console.WindowWidth));
            System.Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
