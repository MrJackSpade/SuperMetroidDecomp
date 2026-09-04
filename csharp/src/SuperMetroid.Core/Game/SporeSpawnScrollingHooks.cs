namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge definitions owned by Spore Spawn's bank-$90 scrolling-finished hook.
/// </summary>
public static class SporeSpawnScrollingHooks
{
    /// <summary>
    /// <c>ScrollingFinishedHook_SporeSpawnFight</c> at <c>$90:9589</c>. The routine
    /// prevents layer one from moving above Y <c>$01D0</c> while the live encounter owns
    /// the global scrolling-finished callback.
    /// </summary>
    public const ushort FightMinimumLayerOneY = 0x01d0;
}
