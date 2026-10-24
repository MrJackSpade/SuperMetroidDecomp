using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="SpacetimeBeamGraphicsDefinitions"/> that only verification reads.</summary>
internal static class SpacetimeBeamGraphicsDefinitionsConstants
{
    /// <summary>$9A:C401: $90:C3CD's palette pointer used as a tile pointer by $90:AC8D.</summary>
    internal const int TileSource = 0x9ac401;
}
