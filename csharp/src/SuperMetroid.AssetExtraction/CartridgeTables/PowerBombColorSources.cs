using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps each of the Power Bomb's authored RGB5 sequences (#1411).</summary>
internal static class PowerBombColorSources
{
    /// <summary>The bank-$88 address <paramref name="sequence"/>'s colours start at.</summary>
    internal static int Start(PowerBombFixedColorSequence sequence) => sequence switch
    {
        PowerBombFixedColorSequence.PreExplosion => SamusSourceDefinitions.PowerBombPreExplosionColors.Value,
        PowerBombFixedColorSequence.Explosion => SamusSourceDefinitions.PowerBombExplosionColors.Value,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
    };
}
