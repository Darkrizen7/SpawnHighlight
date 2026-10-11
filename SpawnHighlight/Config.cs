using System;
using System.Globalization;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace SpawnHighlight
{
    internal class Config
    {
        public Config(ICoreClientAPI api, Mod mod)
        {
            ModId = mod.Info.ModID;

            HotkeyCode = $"toggle{ModId}";
            ThreadName = $"{ModId}Worker";
            TaskCode = $"{ModId}Task";
            configFile = $"{ModId}-config.json";

            data = ReadConfig(api);
            Save(api);
        }
        public string ModId { get; private set; }
        public string HotkeyCode { get; private set; }
        public string ThreadName { get; private set; }
        public string TaskCode { get; private set; }

        public int HighlighSlot { get; } = 5229;

        public int Radius
        {
            get => data.Radius;
            set => data.Radius = value;
        }

        public int SafeColor { get; private set; }

        public void SetSafeColor(int color)
        {
            SafeColor = color;
            data.SafeColor = SerializeColor(color);
        }

        public void SetSafeColor(string color)
        {
            SafeColor = ParseColor(color);
            data.SafeColor = color;
        }


        public int SpawnableColor { get; private set; }
        public void SetSpawnableColor(int color)
        {
            SpawnableColor = color;
            data.SpawnableColor = SerializeColor(color);
        }

        public void SetSpawnableColor(string color)
        {
            SpawnableColor = ParseColor(color);
            data.SpawnableColor = color;
        }

        public int RiftColor { get; private set; }

        public void SetRiftColor(string color)
        {
            RiftColor = ParseColor(color);
            data.RiftColor = color;
        }

        public string HotkeyDescriptionString => Lang.Get($"{ModId}:hotkeyDescription");
        public string EnabledString(bool enabled) => Lang.Get(enabled ? $"{ModId}:disable" : $"{ModId}:enable");

        private readonly string configFile;
        private readonly ConfigData data;

        class ConfigData
        {
            public int Radius;
            public string SafeColor;
            public string SpawnableColor;
            public string RiftColor;
        }

        public void Save(ICoreClientAPI api)
        {
            api.StoreModConfig(data, configFile);
        }

        private ConfigData ReadConfig(ICoreClientAPI api)
        {
            ConfigData config = null;
            try
            {
                config = api.LoadModConfig<ConfigData>(configFile);
            }
            catch (Exception e)
            {
                api.Logger.Error(e);
            }
            config ??= new ConfigData
            {
                Radius = 20,
                SafeColor = "#00FF0020",
                SpawnableColor = "#FF000020",
            };
            // Configs written by older versions have no rift color
            config.RiftColor ??= "#FFFF0020";

            SafeColor = ParseColor(config.SafeColor);
            SpawnableColor = ParseColor(config.SpawnableColor);
            RiftColor = ParseColor(config.RiftColor);

            return config;
        }

        private static int ParseColor(string color)
        {
            int r = int.Parse(color.Substring(1, 2), NumberStyles.HexNumber);
            int g = int.Parse(color.Substring(3, 2), NumberStyles.HexNumber);
            int b = int.Parse(color.Substring(5, 2), NumberStyles.HexNumber);
            int a = ((color.Length < 8) ? 255 : int.Parse(color.Substring(7, 2), NumberStyles.HexNumber));

            return ColorUtil.ReverseColorBytes(ColorUtil.ToRgba(a, r, g, b));
        }
        private static string SerializeColor(int color) => ColorUtil.Int2HexRgba(ColorUtil.ReverseColorBytes(color));


    }
}
