namespace SuperMetroid.Core.Game;

/// <summary>One immutable bank-$83 room-FX record, with its native field identities preserved.</summary>
/// <param name="Pointer">Bank-relative identity of this 16-byte room-FX record.</param>
/// <param name="DoorPointer">Door selector used by the room-FX list lookup; zero denotes the default entry and FFFF terminates the list.</param>
/// <param name="BaseYPosition">Native base liquid-surface Y word.</param>
/// <param name="TargetYPosition">Native target liquid-surface Y word used by the room effect.</param>
/// <param name="PackedYVelocity">Packed vertical-motion value associated with the liquid surface.</param>
/// <param name="Timer">Native effect timer byte.</param>
/// <param name="Type">Room-FX handler identity selected by this record.</param>
/// <param name="DefaultLayerBlend">Default layer-blend control byte.</param>
/// <param name="Layer3LayerBlend">Layer 3 blend control byte.</param>
/// <param name="LiquidOptions">Liquid behavior options interpreted by Samus's physics routines.</param>
/// <param name="PaletteFxBitset">Enabled palette-effect flags for the room effect.</param>
/// <param name="AnimatedTileBitset">Enabled animated-tile flags for the room effect.</param>
/// <param name="PaletteBlend">Palette-blend control byte retained from the native record.</param>
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

/// <summary>Compiled retail room-FX records selected by all known room states.</summary>
public static partial class RoomFxRecordDefinitions
{
    /// <summary>Directly selects the room-FX configuration for one original record identity.</summary>
    /// <remarks>All 295 selected identities preserve the native four words/eight bytes.
    /// Terminator identities preserve the existing canonical view: door FFFF and
    /// zero payload, rather than exposing adjacent native records. Byte and odd-word
    /// views preserve that same representation. Unknown/interior addresses reject.
    /// Each identity selects a room/entrance configuration: liquid positions and velocity,
    /// timer, effect kind, layer blend controls and enabled palette/animation effects.
    /// These are semantic configuration cases, not samples of a numerical curve.
    /// Field layout and original values are independently verified against supported
    /// NTSC J/U v1.0 bank83 and pinned bank_83.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// No record array or generated dictionary is retained.</remarks>
    public static RoomFxRecordDefinition Get(ushort pointer) => SelectRecord(pointer) ??
        throw new InvalidDataException($"No compiled room-FX record at $83:{pointer:X4}.");

    /// <summary>Selects the first default or matching-door record in a bounded FX suffix.</summary>
    /// <remarks>Native $89:AB99..ABB7 treats door FFFF as an unconditional terminator,
    /// before comparing the requested door. Zero selects a default; other mismatches
    /// advance sixteen bytes with ushort wrap. Null roots return zero; unsupported
    /// record identities reject. All 295 supported suffixes and every ushort door
    /// are independently verified against original bank83 words. No stored selector
    /// mapping is required, and the 256-record safety bound is preserved.</remarks>
    public static ushort Select(ushort fxPointer, ushort doorPointer)
    {
        if (fxPointer == 0) return 0;
        ushort pointer = fxPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort candidate = Get(pointer).DoorPointer;
            if (candidate == RoomFxRomData.Record.TerminatorDoorPointer) return 0;
            if (candidate == 0 || candidate == doorPointer) return pointer;
            pointer = unchecked((ushort)(pointer + RoomFxRomData.Record.ByteCount));
        }
        throw new InvalidDataException(
            $"Compiled room-FX list $83:{fxPointer:X4} did not terminate for door $83:{doorPointer:X4}.");
    }

}
