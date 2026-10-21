using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads enemy frames out of the cartridge by where <see cref="EnemyFrameCartridgeLocationDefinitions"/> says it stores them.</summary>
internal static class EnemyFrameCartridgeLookup
{
    /// <summary>The bank and bank-local word of <paramref name="frame"/>'s record.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cartridge stores no composition for <paramref name="frame"/>.</exception>
    internal static SnesAddress Of(EnemyFrameId frame) => EnemyFrameCartridgeLocationDefinitions.Locations.TryGetValue(frame, out SnesAddress location)
        ? location
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "The cartridge stores no composition for this enemy frame.");

    /// <summary>
    /// Rejects a display binding that selects a frame from another enemy bank: the cartridge draws an enemy's frames from
    /// its own bank, so an ordinary composition may only stand in for one stored alongside it.
    /// </summary>
    /// <exception cref="InvalidDataException">A binding selects a frame stored in another bank.</exception>
    internal static EnemySpritemapCatalog RequireSameBankDisplayBindings(EnemySpritemapCatalog catalog)
    {
        foreach ((EnemyFrameId shown, EnemyFrameId selected) in catalog.displayFrames)
            if (Of(shown).Bank != Of(selected).Bank)
                throw new InvalidDataException($"Enemy display binding {shown} must select a named frame in the same bank.");
        return catalog;
    }
}
