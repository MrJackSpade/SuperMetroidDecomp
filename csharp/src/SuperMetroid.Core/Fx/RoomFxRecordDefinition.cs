using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>One immutable bank-$83 room-FX record, with its native field identities preserved.</summary>
/// <remarks><c>Door</c> is the door the record applies to, or null for the list's default record or its terminator;
/// <c>Terminates</c> ends the list without selecting FX.</remarks>
public sealed record RoomFxRecordDefinition(
    CartridgeDoorHeader? Door,
    bool Terminates,
    ushort BaseYPosition,
    ushort TargetYPosition,
    ushort PackedYVelocity,
    byte Timer,
    byte Type,
    byte DefaultLayerBlend,
    byte Layer3LayerBlend,
    byte LiquidOptions,
    byte PaletteFxBitset,
    byte AnimatedTileBitset,
    byte PaletteBlend)
{
}
