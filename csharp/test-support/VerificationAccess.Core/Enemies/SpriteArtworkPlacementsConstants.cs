using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="SpriteArtworkPlacements"/> that only verification reads.</summary>
internal static class SpriteArtworkPlacementsConstants
{
    /// <summary>
    /// The placements <paramref name="looks"/> leave: each look shown, in order, where the cartridge shows it. Replaying
    /// the looks in the order last shown leaves the placements showing every look in the order each was ever shown would,
    /// as a look shown again covers its own earlier showing.
    /// </summary>
    /// <exception cref="InvalidDataException">The cartridge shows one of the looks nowhere among the sprite characters.</exception>
    public static SpriteArtworkPlacements Of(ShownSpriteArtwork looks)
    {
        var placements = new SpriteArtworkPlacements();
        foreach (SpriteArtworkSource look in Ensure.NotNull(looks).InOrder)
            placements.Show(EncounterCharacterPlacements.Placed(look));
        return placements;
    }
}
