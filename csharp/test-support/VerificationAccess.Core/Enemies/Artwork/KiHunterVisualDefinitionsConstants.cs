using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="KiHunterVisualDefinitions"/> that only verification reads.</summary>
internal static class KiHunterVisualDefinitionsConstants
{
    /// <summary>The pose the left-facing flight opens on, the idle Ki-Hunter's first frame.</summary>
    internal static ushort IdleFrame => KiHunterAnimations.Spritemaps(KiHunterProgram.FlyingLeft).First();
}
