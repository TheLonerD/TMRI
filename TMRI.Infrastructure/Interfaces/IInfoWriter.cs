using System.Collections.Generic;
using System.Threading.Tasks;
using TMRI.Primitives.Config;

namespace TMRI.Infrastructure.Interfaces
{
    public interface IInfoWriter
    {
        public Task PrepareWriteAsync();
        public Task SaveConfigFileAsync(AppConfig config);
        public Task SaveGameListAsync(Dictionary<string, string> games);
    }
}
