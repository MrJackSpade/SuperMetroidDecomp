using SuperMetroid.Core;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The installed key of each <see cref="RoomEffectTint"/> (#1740, #2548): the key the installed room-FX blend document
/// names each blend's colours by, as the blend's identity names it, never the cartridge's colour-table offset. The untinted
/// look's key names no blend: the document holds none for it.
/// </summary>
internal static partial class RoomEffectTintKeys
{
    /// <summary>Each RoomEffectTint member's installed key.</summary>
    internal static InstalledKeys<RoomEffectTint> Installed { get; } = new(new Dictionary<RoomEffectTint, string>
    {
        [RoomEffectTint.None] = "none",
        [RoomEffectTint.Lava] = "lava",
        [RoomEffectTint.LandingSiteRain] = "landing-site-rain",
        [RoomEffectTint.IndigoMaridiaWater] = "indigo-maridia-water",
        [RoomEffectTint.WaterAndAcid] = "water-and-acid",
        [RoomEffectTint.Fog] = "fog",
        [RoomEffectTint.TealMaridiaWater] = "teal-maridia-water",
        [RoomEffectTint.GreenMaridiaWater] = "green-maridia-water",
        [RoomEffectTint.PurpleMaridiaWater] = "purple-maridia-water",
    });
}
