using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The bank-$85 routine words a message definition's setup and draw entries name (#1416).</summary>
[CpuBusBoundary("catalog: message-box routine words")]
internal static class GameplayMessageRoutineWords
{
    /// <summary>The routine word naming each <see cref="GameplayMessageRoutine"/>.</summary>
    internal static readonly NativeWordIdentities<GameplayMessageRoutine> Routines = new(new Dictionary<GameplayMessageRoutine, ushort>
    {
        [GameplayMessageRoutine.DrawLargeTilemap] = 0x825a,
        [GameplayMessageRoutine.DrawSmallTilemap] = 0x8289,
        [GameplayMessageRoutine.PatchShootButton] = 0x83c5,
        [GameplayMessageRoutine.PatchRunButton] = 0x83cc,
        [GameplayMessageRoutine.SetupSmall] = 0x8436,
        [GameplayMessageRoutine.SetupLarge] = 0x8441,
    });
}
