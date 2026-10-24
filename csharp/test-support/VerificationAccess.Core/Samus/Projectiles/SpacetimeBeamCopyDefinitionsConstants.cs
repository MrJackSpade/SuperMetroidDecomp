
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where the SpaceTime copy source ends on the cartridge, which only verification reads.</summary>
internal static class SpacetimeBeamCopyDefinitionsConstants
{
    /// <summary>$80:FFFF ($00:FFFF) ends the immutable source before the long word carries into low WRAM.</summary>
    public const int LastSourceAddress = 0x80ffff;
}
