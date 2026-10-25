using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One room-FX record, its fields decoded at import into the effect, blends, liquid options, effect slots and palette
/// blend they select (#1415).
/// </summary>
/// <remarks><c>Door</c> is the door the record applies to, or null for the list's default record or its terminator;
/// <c>Terminates</c> ends the list without selecting FX.</remarks>
public sealed record RoomFxRecordDefinition(
    DoorDefinition? Door,
    bool Terminates,
    ushort BaseYPosition,
    ushort TargetYPosition,
    ushort PackedYVelocity,
    byte Timer,
    RoomFxType Type,
    LayerBlendingConfiguration DefaultLayerBlend,
    LayerBlendingConfiguration Layer3LayerBlend,
    RoomLiquidOptions LiquidOptions,
    RoomFxEffectSlots PaletteFxSlots,
    RoomFxEffectSlots AnimatedTileSlots,
    RoomFxPaletteBlend PaletteBlend)
{
}
