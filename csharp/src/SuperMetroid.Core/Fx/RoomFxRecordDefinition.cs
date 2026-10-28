using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One room-FX record, its fields decoded at import into the effect, blends, liquid options, effect slots and palette
/// blend they select (#1415).
/// </summary>
/// <remarks><c>Door</c> is the door the record applies to, or null for the list's default record or its terminator;
/// <c>Terminates</c> ends the list without selecting FX. <c>BaseYPosition</c> is the liquid surface's starting height,
/// or null when the record names none and the surface sits below every room; <c>TargetYPosition</c> is the height the
/// surface moves to, or null when the record names none and the surface never moves; <c>YVelocity</c> is the signed
/// speed it moves at.</remarks>
public sealed record RoomFxRecordDefinition(
    DoorDefinition? Door,
    bool Terminates,
    int? BaseYPosition,
    int? TargetYPosition,
    SignedEightEight YVelocity,
    int Timer,
    RoomFxType Type,
    LayerBlendingConfiguration DefaultLayerBlend,
    LayerBlendingConfiguration Layer3LayerBlend,
    RoomLiquidOptions LiquidOptions,
    RoomFxEffectSlots PaletteFxSlots,
    RoomFxEffectSlots AnimatedTileSlots,
    RoomFxPaletteBlend PaletteBlend)
{
}
