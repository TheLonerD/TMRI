using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMRI.Primitives;
using TMRI.Primitives.Definitions;
using TMRI.Primitives.Enums;

namespace TMRI.Infrastructure.Utils
{
    public static class FileUtils
    {
        private static readonly string[] SizeSuffixes =
            {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        private static readonly Dictionary<string, (string, Type)> pKeyTable = new Dictionary<string, (string, Type)>
        {
            {"name", (nameof(ProductInfo.Name), typeof(LocalizedString))},
            {"artist", (nameof(ProductInfo.Artist), typeof(string))},
            {"circle", (nameof(ProductInfo.Circle), typeof(LocalizedString))},
            {"year", (nameof(ProductInfo.Year), typeof(int))},
            {"tracks", (nameof(ProductInfo.Tracks), typeof(int))},
            {"packmethod", (nameof(ProductInfo.PackInfo), typeof(PackInfo))},
            {"bgmfile", (nameof(ProductInfo.PackInfo), typeof(PackInfo))},
            {"bgmdir", (nameof(ProductInfo.PackInfo), typeof(PackInfo))},
            {"zwavid", (nameof(ProductInfo.PackInfo), typeof(PackInfo))},
            {"wikipage", (nameof(UpdateInfo.MetaInfo), typeof(MetaInfo))},
            {"wikirev", (nameof(UpdateInfo.MetaInfo), typeof(MetaInfo))},
            {"comment", (nameof(TrackInfo.Comments), typeof(List<LocalizedString>))},
            {"position", (nameof(TrackInfo.MetaInfo), typeof(MetaInfo))},
            {"frequency", (nameof(TrackInfo.MetaInfo), typeof(MetaInfo))},
            {"filename", (nameof(TrackInfo.MetaInfo), typeof(MetaInfo))},
            {"rel_", (nameof(TrackInfo.MetaInfo), typeof(MetaInfo))},
            {"cmp", (nameof(MusicDefinition.Composer), typeof(List<LocalizedString>))},
            {"composer", (nameof(TrackInfo.Composer), typeof(long?))}
        };

        private static readonly Dictionary<string, (string, Type)> sKeyTable = new Dictionary<string, (string, Type)>
        {
            {"packmethod", (nameof(PackInfo.PackMethod), typeof(PackMethod))},
            {"bgmfile", (nameof(PackInfo.BGMFile), typeof(string))},
            {"bgmdir", (nameof(PackInfo.BGMDir), typeof(string))},
            {"zwavid", (nameof(PackInfo.MetaInfo), typeof(MetaInfo))}
        };

        public static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            // https://stackoverflow.com/a/14488941

            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            }

            if (value < 0)
            {
                return "-" + SizeSuffix(-value);
            }

            if (value == 0)
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            var mag = (int) Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            var adjustedSize = (decimal) value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        public static async Task<MusicDefinition> ConvertOldBGMFileAsync(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!File.Exists(file))
            {
                throw new TMRIException($"File \"{file}\" is not exist.");
            }

            var lines = await File.ReadAllLinesAsync(file);
            string currentSection = null;
            var trackNum = 0L;
            var lineNum = 0;
            ProductInfo pi = null;
            UpdateInfo ui = null;
            List<TrackInfo> tiList = null;
            var result = new MusicDefinition();
            foreach (var str in lines)
            {
                lineNum++;

                if (string.IsNullOrWhiteSpace(str))
                {
                    continue;
                }

                var line = str.Trim();

                if (line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    var section = line.TrimStart('[').TrimEnd(']').Trim();
                    var isNumber = long.TryParse(section, out trackNum);

                    if (isNumber)
                    {
                        section = "number";
                    }

                    currentSection = section.Trim();

                    continue;
                }

                switch (currentSection)
                {
                    case "game":
                        SetProductInfo(line, ref pi, file, lineNum);
                        break;
                    case "update":
                        SetUpdateInfo(line, ref ui, file, lineNum);
                        break;
                    case "composer":
                        SetComposerInfo(line, ref result, file, lineNum);
                        break;
                    case "number":
                        tiList ??= new List<TrackInfo>();
                        var ti = tiList.FirstOrDefault(t => t.Number == trackNum);
                        var addTrack = ti == null;

                        SetTrackInfo(line, ref ti, file, lineNum);

                        if (addTrack)
                        {
                            ti.Number = trackNum;
                            tiList.Add(ti);
                        }

                        break;
                }
            }

            result.Product = pi;
            result.Update = ui;
            result.Playlist = tiList?.OrderBy(t => t.Number).ToList();

            return result;
        }

        private static void SetProductInfo(string line, ref ProductInfo productInfo, string file, int lineNum)
        {
            var (key, value) = GetKeyValue(line, file, lineNum);

            productInfo ??= new ProductInfo();

            SetField(productInfo, key, value, pKeyTable, typeof(ProductInfo));
        }

        private static void SetUpdateInfo(string line, ref UpdateInfo updateInfo, string file, int lineNum)
        {
            var (key, value) = GetKeyValue(line, file, lineNum);

            updateInfo ??= new UpdateInfo
            {
                Source = UpdateSource.TouhouWiki
            };

            SetField(updateInfo, key, value, pKeyTable, typeof(UpdateInfo));
        }

        private static void SetComposerInfo(string line, ref MusicDefinition md, string file, int lineNum)
        {
            var (key, value) = GetKeyValue(line, file, lineNum);

            md ??= new MusicDefinition();

            SetField(md, key, value, pKeyTable, typeof(MusicDefinition));
        }

        private static void SetTrackInfo(string line, ref TrackInfo trackInfo, string file, int lineNum)
        {
            var (key, value) = GetKeyValue(line, file, lineNum);

            trackInfo ??= new TrackInfo();

            SetField(trackInfo, key, value, pKeyTable, typeof(TrackInfo));
        }

        private static (string, string) GetKeyValue(string line, string file, int lineNum)
        {
            var assignment = line.IndexOf('=');

            if (assignment == -1)
            {
                throw new TMRIException($"Incorrect format of line in \"{file}\", line {lineNum}.");
            }

            var key = line.Substring(0, assignment).Trim();
            var value = line.Substring(assignment + 1).Trim();

            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
            }

            return (key, value);
        }

        private static void SetField(object obj, string key, string value, Dictionary<string, (string, Type)> keyTable,
            Type parent)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var props = obj.GetType().GetProperties();

            var tableKey = keyTable.Keys.FirstOrDefault(key.StartsWith);
            if (string.IsNullOrWhiteSpace(tableKey))
            {
                return;
            }

            var (propName, type) = keyTable[tableKey];
            var prop = props.FirstOrDefault(p => p.Name.Equals(propName));

            if (prop == null)
            {
                return;
            }

            if (type == typeof(LocalizedString))
            {
                var ls = (LocalizedString) prop.GetValue(obj) ?? new LocalizedString();

                var lang = Language.JP;
                if (key.EndsWith("_en"))
                {
                    lang = Language.EN;
                }

                ls[lang] = value;

                prop.SetValue(obj, ls);
            }
            else if (type == typeof(string))
            {
                prop.SetValue(obj, value);
            }
            else if (type == typeof(int))
            {
                prop.SetValue(obj, Convert.ToInt32(value));
            }
            else if (type == typeof(PackInfo))
            {
                var pi = (PackInfo) prop.GetValue(obj) ?? new PackInfo();

                SetField(pi, key, value, sKeyTable, typeof(PackInfo));

                prop.SetValue(obj, pi);
            }
            else if (type == typeof(PackMethod))
            {
                var pm = PackMethod.None;

                if (value == "2")
                {
                    pm = PackMethod.TSA;
                }

                prop.SetValue(obj, pm);
            }
            else if (type == typeof(MetaInfo))
            {
                var mi = (MetaInfo) prop.GetValue(obj) ?? new MetaInfo();

                if (parent == typeof(PackInfo))
                {
                    if (value.StartsWith("0x"))
                    {
                        var num = value.Replace("0x", "");
                        mi.Add(key.ToUpper(), int.Parse(num, NumberStyles.HexNumber));
                    }
                    else
                    {
                        mi.Add(key, value);
                    }
                }
                else if (parent == typeof(UpdateInfo))
                {
                    mi.Add(key, value);
                }
                else if (parent == typeof(TrackInfo))
                {
                    switch (key)
                    {
                        case "position":
                            var values = value
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(v => long.Parse(v.Trim().Replace("0x", ""), NumberStyles.HexNumber))
                                .ToList();

                            if (values.Count != 3)
                            {
                                throw new TMRIException("Incorrect Position format");
                            }

                            mi.Add(key, values);
                            break;
                        case "frequency":
                            if (!int.TryParse(value, out var freq))
                            {
                                throw new TMRIException($"Incorrect Frequency number format: {value}.");
                            }

                            mi.Add(key, freq);
                            break;
                        case "rel_loop":
                        case "rel_end":
                            if (!long.TryParse(value.Trim().Replace("0x", ""),
                                NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var num))
                            {
                                throw new TMRIException($"Incorrect {key} number format: {value}.");
                            }

                            mi.Add(key, num);
                            break;
                        case "filename":
                            mi.Add(key, value);
                            break;
                    }
                }

                prop.SetValue(obj, mi);
            }
            else if (type == typeof(List<LocalizedString>))
            {
                var lsList = (List<LocalizedString>) prop.GetValue(obj) ?? new List<LocalizedString>();
                var hasNumber = key.Any(char.IsNumber);
                var lang = Language.JP;

                if (key.EndsWith("_en"))
                {
                    lang = Language.EN;
                }

                if (hasNumber)
                {
                    var num = Convert.ToInt32(new string(key.Where(char.IsNumber).ToArray()));
                    var ls = lsList.ElementAtOrDefault(num - 1);

                    if (ls == null)
                    {
                        ls = new LocalizedString {[lang] = Regex.Unescape(value)};
                        try
                        {
                            lsList.Insert(num - 1, ls);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            for (var i = 0; i < num - 1; i++)
                            {
                                if (lsList.ElementAtOrDefault(i) == null)
                                {
                                    lsList.Insert(i, new LocalizedString());
                                }
                            }

                            lsList.Insert(num - 1, ls);
                        }
                    }
                    else
                    {
                        ls[lang] = Regex.Unescape(value);
                    }
                }
                else
                {
                    var item = lsList.FirstOrDefault();

                    if (item == null)
                    {
                        item = new LocalizedString {[lang] = Regex.Unescape(value)};
                        lsList.Add(item);
                    }
                    else
                    {
                        item[lang] = Regex.Unescape(value);
                    }
                }

                prop.SetValue(obj, lsList);
            }
            else if (type == typeof(long?))
            {
                prop.SetValue(obj, Convert.ToInt64(value));
            }
            else
            {
                throw new TMRIException($"Unknown type \"{type.Name}\" to deserialize.");
            }
        }
    }
}
