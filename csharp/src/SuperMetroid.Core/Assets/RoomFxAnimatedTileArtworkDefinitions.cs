using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The cartridge's five simple bank-$87 room-FX animated-tile objects use contiguous
/// frame artwork within each object. These addresses identify visual bytes only;
/// duration, loop control, transfer size and VRAM destination remain compiled in
/// <see cref="RoomFxAnimatedTileMechanicsDefinitions"/>.
/// </summary>
public static class RoomFxAnimatedTileArtworkDefinitions
{
    /// <summary>$87:91E4, four ceiling-sand frames selected by $87:8221.</summary>
    public const int MaridiaSandCeilingFirstSource = 0x8791e4;
    /// <summary>$87:9164, four falling-sand frames selected by $87:8235.</summary>
    public const int MaridiaSandFallingFirstSource = 0x879164;
    /// <summary>$87:A564, five lava frames selected by $87:8293.</summary>
    public const int LavaFirstSource = 0x87a564;
    /// <summary>$87:A6A4, five acid frames selected by $87:82B1.</summary>
    public const int AcidFirstSource = 0x87a6a4;
    /// <summary>$87:A874, five rain frames selected by $87:82CF.</summary>
    public const int RainFirstSource = 0x87a874;

    /// <summary>Returns the native source identity for one compiled frame cursor.</summary>
    public static int SourceAddress(RoomFxAnimatedTileObjectDefinition definition,
        ushort instructionPointer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        int first = definition.ObjectPointer switch
        {
            AnimatedTileObjectPointers.MaridiaSandCeiling => MaridiaSandCeilingFirstSource,
            AnimatedTileObjectPointers.MaridiaSandFalling => MaridiaSandFallingFirstSource,
            AnimatedTileObjectPointers.Lava => LavaFirstSource,
            AnimatedTileObjectPointers.Acid => AcidFirstSource,
            AnimatedTileObjectPointers.Rain => RainFirstSource,
            _ => throw new InvalidDataException(
                $"No compiled artwork source for room-FX object $87:{definition.ObjectPointer:X4}."),
        };
        for (int index = 0; index < definition.Frames.Count; index++)
            if (definition.Frames[index].InstructionPointer == instructionPointer)
                return first + index * definition.TransferByteCount;
        throw new InvalidDataException(
            $"Room-FX object $87:{definition.ObjectPointer:X4} has no artwork frame at $87:{instructionPointer:X4}.");
    }
}
