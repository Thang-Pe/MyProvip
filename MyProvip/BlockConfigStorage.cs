using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyProvip
{
    internal class BlockConfigStorage
    {
        private static readonly string configPath = "blocks_config.json";

        public static void Save(Dictionary<string, string> blockTypes)
        {
            string json = JsonSerializer.Serialize(blockTypes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }

        public static Dictionary<string, string> Load()
        {
            if (!File.Exists(configPath)) return new Dictionary<string, string>();

            string json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        public static (string Part1, string Part2) SplitEquipment(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (string.Empty, string.Empty);

            // Thay &#xA; thành xuống dòng thực
            text = text.Replace("&#xA;", "\n");

            // Tách theo xuống dòng
            string[] parts = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                return (parts[0].Trim(), parts[1].Trim());
            }
            else if (parts.Length == 1)
            {
                return (string.Empty, parts[0].Trim());
            }
            else
            {
                return (string.Empty, string.Empty);
            }
        }


    }
}
