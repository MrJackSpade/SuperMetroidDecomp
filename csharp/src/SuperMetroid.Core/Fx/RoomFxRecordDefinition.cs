namespace SuperMetroid.Core.Fx;

/// <summary>One immutable bank-$83 room-FX record, with its native field identities preserved.</summary>
public sealed record RoomFxRecordDefinition(
    ushort Pointer,
    ushort DoorPointer,
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
