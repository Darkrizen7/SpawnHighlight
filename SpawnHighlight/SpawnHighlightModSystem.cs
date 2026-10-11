using System;
using System.Collections.Generic;
using System.Threading;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace SpawnHighlight
{
    public class SpawnHighlightModSystem : ModSystem
    {
        private ICoreClientAPI api;
        private Config config;
        private Thread thread;
        private Commands commands;
        private IBlockAccessor blockAccessor;
        private bool enabled = false;

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.api = api;
            config = new Config(api, Mod);
            commands = new Commands(api, config, ToggleRun);

            RegisterHotkey();
        }

        private void RegisterHotkey()
        {
            api.Input.RegisterHotKey(config.HotkeyCode, config.HotkeyDescriptionString, GlKeys.L, type: HotkeyType.HelpAndOverlays, ctrlPressed: true);
            api.Input.SetHotKeyHandler(config.HotkeyCode, OnHotkey);
        }

        private bool OnHotkey(KeyCombination _)
        {
            ToggleRun();
            return true;
        }

        private void ToggleRun()
        {
            // Prevent starting new thread before old one has ended
            if (!enabled && thread?.IsAlive == true) return;

            api.ShowChatMessage(config.EnabledString(enabled));

            enabled = !enabled;
            if (!enabled) return;

            blockAccessor = api.World.GetLockFreeBlockAccessor();

            thread = new Thread(RunThread)
            {
                IsBackground = true,
                Name = config.ThreadName
            };
            thread.Start();
        }

        private void RunThread()
        {
            while (enabled)
            {
                try
                {
                    RenderHighlights();
                }
                catch (Exception ex)
                {
                    api.Logger.Error(ex);
                }

                Thread.Sleep(250);
            }

            ClearHighlights();
        }

        private void RenderHighlights()
        {
            var position = api.World.Player.Entity.Pos.AsBlockPos;

            List<BlockPos> positions = new();
            List<int> colors = new();

            var r = config.Radius;
            var px = position.X;
            var py = position.Y;
            var pz = position.Z;

            blockAccessor.WalkBlocks(
                new(px - r, py - r, pz - r),
                new(px + r, py + r, pz + r),
                (block, x, y, z) =>
                {
                    if (block.Id == 0) return;
                    if (!CanSpawnOn(block)) return;

                    var blockPosition = new BlockPos(x, y, z);
                    var spawnPosition = blockPosition.UpCopy();

                    var insideBlock = blockAccessor.GetBlock(spawnPosition);
                    if (!CanSpawnInside(insideBlock)) return;
                    if (!HasRoomForMob(insideBlock, spawnPosition)) return;

                    // Same checks as SystemTemporalStability.CanSpawnNearby: light is measured where the mob stands
                    var blockLight = blockAccessor.GetLightLevel(spawnPosition, EnumLightLevelType.OnlyBlockLight);
                    var sunLight = blockAccessor.GetLightLevel(spawnPosition, EnumLightLevelType.OnlySunLight);

                    positions.Add(blockPosition);
                    if (blockLight > MaxSpawnLightLevel) colors.Add(config.SafeColor);
                    else if (sunLight >= OpenSkySunLightLevel) colors.Add(config.RiftColor);
                    else colors.Add(config.SpawnableColor);
                });

            ShowHighlights(positions, colors);
        }

        // Runtime spawn conditions of drifters, shivers and bowtorns (assets/survival/entities/lore/*.json)
        private const int MaxSpawnLightLevel = 7;
        private const int OpenSkySunLightLevel = 16;
        // Drifter hitbox height (1.3) plus the 0.1 margin the spawner adds around SpawnCollisionBox
        private const float MobHeight = 1.4f;
        private const float MobHalfWidth = 0.4f;

        // Block.CanCreatureSpawnOn with RequireSolidGround and the "hostile" group
        private static bool CanSpawnOn(Block block)
        {
            if (!block.SideSolid[BlockFacing.UP.Index]) return false;
            if (block.AllCreaturesAllowed) return true;

            var groups = block.AllowSpawnCreatureGroups;
            return groups != null && (Array.IndexOf(groups, "*") >= 0 || Array.IndexOf(groups, "hostile") >= 0);
        }

        // insideBlockCodes: ["air", "tallgrass-*", "loosestones-*"]
        private static bool CanSpawnInside(Block block)
        {
            var path = block.Code?.Path;
            if (path == null) return false;

            return path == "air" || path.StartsWith("tallgrass-") || path.StartsWith("loosestones-");
        }

        // Mirrors the spawner's CollisionTester check on the mob's spawn box
        private bool HasRoomForMob(Block insideBlock, BlockPos spawnPosition)
        {
            float bottom = 1e-7f;
            var insideBoxes = insideBlock.GetCollisionBoxes(blockAccessor, spawnPosition);
            if (insideBoxes != null && insideBoxes.Length > 0) bottom += insideBoxes[0].MaxY % 1f;

            float top = bottom + MobHeight;
            for (int dy = 0; dy < top; dy++)
            {
                var position = spawnPosition.UpCopy(dy);
                var block = dy == 0 ? insideBlock : blockAccessor.GetBlock(position);
                var boxes = block.GetCollisionBoxes(blockAccessor, position);
                if (boxes == null) continue;

                foreach (var box in boxes)
                {
                    if (box.Y1 + dy < top && box.Y2 + dy > bottom
                        && box.X1 < 0.5f + MobHalfWidth && box.X2 > 0.5f - MobHalfWidth
                        && box.Z1 < 0.5f + MobHalfWidth && box.Z2 > 0.5f - MobHalfWidth)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ShowHighlights(List<BlockPos> positions, List<int> colors)
        {
            api.Event.EnqueueMainThreadTask(() => api.World.HighlightBlocks(api.World.Player, config.HighlighSlot, positions, colors), config.TaskCode);
        }
        private void ClearHighlights()
        {
            api.Event.EnqueueMainThreadTask(() => api.World.HighlightBlocks(api.World.Player, config.HighlighSlot, new List<BlockPos>()), config.TaskCode);
        }
    }
}
