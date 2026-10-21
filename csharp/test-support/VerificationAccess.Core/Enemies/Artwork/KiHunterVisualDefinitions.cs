using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed body and wing compositions selected by KiHunter programs.</summary>
internal static class KiHunterVisualDefinitions
{
    /// <summary>$A8, the bank containing KiHunter's animation and OAM data.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>
    /// The 41 distinct body/wing spritemaps selected by 59 presentation operands
    /// in KiHunter's thirteen native programs. Addresses are stable frame identities;
    /// timing, callbacks and collision continue to use compiled mechanics.
    /// </summary>
    internal static NativeEnemySpritemapDefinition[] Frames() =>
        [.. new SortedSet<ushort>(KiHunterAnimationsConstants.Spritemaps(Enum.GetValues<KiHunterProgram>()))
            .Select(pointer => new NativeEnemySpritemapDefinition(Bank, pointer, $"ki_hunter_a8_{pointer:x4}"))];
}
