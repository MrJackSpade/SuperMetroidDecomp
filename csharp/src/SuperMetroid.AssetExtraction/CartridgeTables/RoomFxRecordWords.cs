using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The cartridge bytes a room-FX record stores for its effect and palette blend (#1415). Each is a byte offset into a
/// cartridge table, the effect type into the bank-$83 dispatcher and page tables and the blend into the $89:AA02
/// colours, so extraction reads those tables through them; the identities' own values are arbitrary.
/// </summary>
[EncodingOwner("room-FX record effect and palette-blend bytes")]
internal static class RoomFxRecordWords
{
    /// <summary>The dispatcher byte naming each <see cref="RoomFxType"/>.</summary>
    internal static readonly NativeWordIdentities<RoomFxType> Types = new(new Dictionary<RoomFxType, ushort>
    {
        [RoomFxType.None] = 0x00,
        [RoomFxType.Lava] = 0x02,
        [RoomFxType.Acid] = 0x04,
        [RoomFxType.Water] = 0x06,
        [RoomFxType.Spores] = 0x08,
        [RoomFxType.Rain] = 0x0a,
        [RoomFxType.Fog] = 0x0c,
        [RoomFxType.ScrollingSky] = 0x20,
        [RoomFxType.Fireflea] = 0x24,
        [RoomFxType.TourianEntranceStatue] = 0x26,
        [RoomFxType.CeresRidley] = 0x28,
        [RoomFxType.CeresElevator] = 0x2a,
        [RoomFxType.CeresHaze] = 0x2c,
    });

    /// <summary>The colour-table byte offset naming each <see cref="RoomEffectTint"/>; zero clears the blend.</summary>
    internal static readonly NativeWordIdentities<RoomEffectTint> PaletteBlends = new(new Dictionary<RoomEffectTint, ushort>
    {
        [RoomEffectTint.None] = 0x00,
        [RoomEffectTint.Lava] = 0x02,
        [RoomEffectTint.LandingSiteRain] = 0x22,
        [RoomEffectTint.IndigoMaridiaWater] = 0x42,
        [RoomEffectTint.WaterAndAcid] = 0x48,
        [RoomEffectTint.Fog] = 0x62,
        [RoomEffectTint.TealMaridiaWater] = 0xe2,
        [RoomEffectTint.GreenMaridiaWater] = 0xe8,
        [RoomEffectTint.PurpleMaridiaWater] = 0xee,
    });
}
