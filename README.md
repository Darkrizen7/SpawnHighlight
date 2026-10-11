# Spawn Highlight

Allows to highlight blocks where drifters can spawn.  
Ever tired of drifter-proofing your base? This mod is for you!

Press `Ctrl+L` to show/hide highlights. Keybinding can be changed in the settings.

Default colors:  
Red - drifters can spawn on this block  
Yellow - dark but under open sky: drifters only spawn here near a temporal rift (or during a storm / low temporal stability)  
Green - drifters cannot spawn on this block because it is sufficiently lit  
Not highlighted - drifter will never spawn on this block. Examples are lower slabs, stones and chiseled blocks

Remember that drifters **will** spawn anywhere they want while temporal storm is in progress, and lit areas become unsafe when your temporal stability is low.

To be more specific, the mod mirrors the game's runtime spawn rules (Vintage Story 1.22) for drifters, shivers and bowtorns:

1. Block has solid upper side and allows hostile spawns
2. Block above is air, tall grass or loose stones
3. There is enough room above for a drifter (about 1.4 blocks)
4. Block light (torches, lanterns...) at the spawn spot is 7 or less. 8 and more is safe
5. Spots under open sky are only used near temporal rifts

This mod is quite similar to [Easy Light Levels](https://mods.vintagestory.at/show/mod/2414). You may check it too if you want wider configuration options.

## Building

Requires the .NET 10 SDK and the `VINTAGE_STORY` environment variable pointing to the game folder. Run `./build.sh` (or `./build.ps1`); the mod zip ends up in `Releases/`.

## Commands

`.sphi` - Show/Hide highlights. Does exactly the same as `Ctrl+L`  
`.sphi radius <number>` - Change size of cube around you, where mod will scan and highlight blocks. Default is `20`  
`.sphi litcolor #RRGGBBAA` - Set color for sufficiently lit zones. Default is `#00FF0020`  
`.sphi spawncolor #RRGGBBAA` - Set color for zones where mobs can spawn. Default is `#FF000020`  
`.sphi riftcolor #RRGGBBAA` - Set color for dark zones under open sky. Default is `#FFFF0020`

## Config

If you prefer json instead of commands, you can change the values in the `spawnhighlight-config.json` which can be found in `VintagestoryData/ModConfig` folder

```json
{
  "Radius": 20,
  "SafeColor": "#00FF0020",
  "SpawnableColor": "#FF000020",
  "RiftColor": "#FFFF0020"
}
```
