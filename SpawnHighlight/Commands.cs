using System;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SpawnHighlight
{
    internal partial class Commands
    {
        Config config;
        ICoreClientAPI api;

        public Commands(ICoreClientAPI api, Config config, Action TriggerHighlights)
        {
            this.api = api;
            this.config = config;

            var p = api.ChatCommands.Parsers;

            var command = api.ChatCommands.Create("sphi")
                .WithDescription("Trigger highlights for hostile mob spawn locations")
                .WithAlias(config.ModId)
                .HandleWith(args =>
                {
                    TriggerHighlights();
                    return TextCommandResult.Success();
                })

                .BeginSubCommand("radius")
                    .WithAlias("r")
                    .WithDescription("Set radius for highlighted area")
                    .WithArgs(p.OptionalInt("radius"))
                    .HandleWith(RadiusCommand)
                .EndSubCommand()
                
                .BeginSubCommand("litcolor")
                    .WithAlias("l")
                    .WithDescription("Set color for sufficiently lit zones")
                    .WithArgs(p.Word("color"))
                    .HandleWith(LitColorCommand)
                .EndSubCommand()

                .BeginSubCommand("spawncolor")
                    .WithAlias("s")
                    .WithDescription("Set color for zones where mobs can spawn")
                    .WithArgs(p.Word("color"))
                    .HandleWith(SpawnColorCommand)
                .EndSubCommand()

                .BeginSubCommand("riftcolor")
                    .WithAlias("rc")
                    .WithDescription("Set color for dark zones under open sky, where mobs only spawn near rifts")
                    .WithArgs(p.Word("color"))
                    .HandleWith(RiftColorCommand)
                .EndSubCommand();
        }

        private TextCommandResult RadiusCommand(TextCommandCallingArgs args)
        {
            var radius = (int)args[0];
            if (radius == 0) return TextCommandResult.Success($"Current radius: {config.Radius}");

            config.Radius = radius;
            config.Save(api);

            return TextCommandResult.Success();
        }

        private partial class Reg
        {
            public static Regex colorRegex = ColorRegex();

            [GeneratedRegex(@"^\s*#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?\s*$")]
            private static partial Regex ColorRegex();
        }

        private TextCommandResult LitColorCommand(TextCommandCallingArgs args)
        {
            var color = (string)args[0];
            if (!Reg.colorRegex.IsMatch(color))
            {
                return TextCommandResult.Error($"{color} is not a valid hex color. Write color in format #RRGGBB or #RRGGBBAA");
            } 

            config.SetSafeColor(color);
            config.Save(api);

            return TextCommandResult.Success();
        }

        private TextCommandResult SpawnColorCommand(TextCommandCallingArgs args)
        {
            var color = (string)args[0];
            if (!Reg.colorRegex.IsMatch(color))
            {
                return TextCommandResult.Error($"{color} is not a valid hex color. Write color in format #RRGGBB or #RRGGBBAA");
            }

            config.SetSpawnableColor(color);
            config.Save(api);

            return TextCommandResult.Success();
        }

        private TextCommandResult RiftColorCommand(TextCommandCallingArgs args)
        {
            var color = (string)args[0];
            if (!Reg.colorRegex.IsMatch(color))
            {
                return TextCommandResult.Error($"{color} is not a valid hex color. Write color in format #RRGGBB or #RRGGBBAA");
            }

            config.SetRiftColor(color);
            config.Save(api);

            return TextCommandResult.Success();
        }

    }
}
