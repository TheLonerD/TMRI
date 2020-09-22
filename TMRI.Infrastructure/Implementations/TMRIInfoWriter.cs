using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TMRI.Infrastructure.Interfaces;
using TMRI.Primitives.Config;

namespace TMRI.Infrastructure.Implementations
{
    public class TMRIInfoWriter : IInfoWriter
    {
        private readonly string _currentDir;

        public TMRIInfoWriter()
        {
            var fullPath = typeof(TMRIInfoLoader).Assembly.Location;
            _currentDir = Path.GetDirectoryName(fullPath);
        }

        public async Task PrepareWriteAsync()
        {
            // Nothing to do
        }

        public async Task SaveConfigFileAsync(AppConfig config) => throw new NotImplementedException();

        public async Task SaveGameListAsync(Dictionary<string, string> games)
        {
            var file = Path.Combine(_currentDir, AppConfig.GAMESLIST_FILE);
            await using var fs = new FileStream(file, FileMode.Open);
            await JsonSerializer.SerializeAsync(fs, games, new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true
            });
        }
    }
}
