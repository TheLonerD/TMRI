using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TMRI.Infrastructure.Interfaces;
using TMRI.Infrastructure.Utils;
using TMRI.Primitives;
using TMRI.Primitives.Config;
using TMRI.Primitives.Definitions;

namespace TMRI.Infrastructure.Implementations
{
    public class TMRIInfoLoader : IInfoLoader
    {
        private readonly string _currentDir;

        public TMRIInfoLoader()
        {
            var fullPath = typeof(TMRIInfoLoader).Assembly.Location;
            _currentDir = Path.GetDirectoryName(fullPath);
        }

        public async Task PrepareLoaderAsync()
        {
            await EnsureFilesCreatedAsync();
        }

        public async Task<AppConfig> GetConfigAsync()
        {
            var file = Path.Combine(_currentDir, AppConfig.APPCONFIG_FILE);

            if (!File.Exists(file))
            {
                return new AppConfig();
            }

            try
            {
                await using var fileStream = new FileStream(file, FileMode.Open);
                var result = await JsonSerializer.DeserializeAsync<AppConfig>(fileStream);

                return result;
            }
            catch (JsonException e)
            {
                throw new TMRIException("Cannot parse AppConfig file \"{file}\"", e);
            }
            catch (Exception e)
            {
                throw new TMRIException($"Cannot open AppConfig file \"{file}\"", e);
            }
        }

        public async Task<GameDirectories> GetGamesListAsync()
        {
            var file = Path.Combine(_currentDir, AppConfig.GAMESLIST_FILE);

            try
            {
                await using var fileStream = new FileStream(file, FileMode.Open);
                var result = await JsonSerializer.DeserializeAsync<GameDirectories>(fileStream);

                return result;
            }
            catch (JsonException e)
            {
                throw new TMRIException("Cannot parse Games file \"{file}\"", e);
            }
            catch (Exception e)
            {
                throw new TMRIException($"Cannot open Games file \"{file}\"", e);
            }
        }

        public async Task<List<MusicDefinition>> GetMusicListAsync()
        {
            var path = Path.Combine(_currentDir, AppConfig.BGFINFO_FOLDER);
            var files = Directory.GetFiles(path, AppConfig.BGMINFO_SEARCH_PATTERN, new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive,
                ReturnSpecialDirectories = false,
                MatchType = MatchType.Simple
            });

            var result = new List<MusicDefinition>();
            foreach (var file in files)
            {
                await using var fs = new FileStream(file, FileMode.Open);
                var pi = await JsonUtils.DeserializeAsync<MusicDefinition>(fs);
                pi.Path = file;
                result.Add(pi);
            }

            return result;
        }

        private async Task EnsureFilesCreatedAsync()
        {
            var configFile = Path.Combine(_currentDir, AppConfig.APPCONFIG_FILE);
            var gamesListFile = Path.Combine(_currentDir, AppConfig.GAMESLIST_FILE);
            var bgmInfoFolder = Path.Combine(_currentDir, AppConfig.BGFINFO_FOLDER);

            if (!File.Exists(configFile))
            {
                var config = new AppConfig(); // TODO: Write current config
                await using var fs = new FileStream(configFile, FileMode.CreateNew);
                await JsonUtils.SerializeAsync(fs, configFile);
            }

            if (!File.Exists(gamesListFile))
            {
                var games = new GameDirectories();
                await using var fs = new FileStream(gamesListFile, FileMode.CreateNew);
                await JsonUtils.SerializeAsync(fs, games);
            }

            if (!Directory.Exists(bgmInfoFolder))
            {
                Directory.CreateDirectory(bgmInfoFolder);
            }
        }
    }
}
