using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>Encodes typed room-FX record fields back to their cartridge bytes, for checks that seed synthetic records
/// and compare the import's decode with the cartridge (#1415).</summary>
internal static class RoomFxRecordEncoding
{
    /// <summary>The record byte naming <paramref name="type"/>.</summary>
    internal static byte TypeByte(RoomFxType type) => (byte)RoomFxRecordWords.Types.Word(type);

    /// <summary>The record byte naming <paramref name="configuration"/>.</summary>
    internal static byte LayerBlendByte(LayerBlendingConfiguration configuration) => (byte)RoomFxRecordCodec.LayerBlends.Word(configuration);

    /// <summary>The record byte naming <paramref name="blend"/>.</summary>
    internal static byte PaletteBlendByte(RoomFxPaletteBlend blend) => (byte)RoomFxRecordWords.PaletteBlends.Word(blend);

    /// <summary>The record byte <paramref name="options"/> encode to.</summary>
    internal static byte LiquidOptionsByte(RoomLiquidOptions options) => (byte)options;

    /// <summary>The record byte <paramref name="slots"/> encode to.</summary>
    internal static byte SlotsByte(RoomFxEffectSlots slots) => (byte)slots;
}
