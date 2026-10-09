using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B animation lists for the approach-to-Ceres star field and rear-view actors.
/// Actor placement, motion, and visual spritemaps are separate owners.
/// </summary>
internal static class CeresFlightSpriteInstructionDefinitions
{
    /// <summary>$8B:CC47, first byte of the under-attack, small-asteroid, and vortex lists.</summary>
    internal const ushort RearClusterStart = 0xcc47;
    /// <summary>$8B:CC63, exclusive end before the next cinematic actor list.</summary>
    internal const ushort RearClusterEnd = 0xcc63;
    /// <summary>$8B:CDA3, first byte of the shared front/rear star list.</summary>
    internal const ushort StarsStart = 0xcda3;
    /// <summary>$8B:CDAB, exclusive end before the small-explosion list.</summary>
    internal const ushort StarsEnd = 0xcdab;
    /// <summary>$8B:CE4B, first byte of the large-asteroid list.</summary>
    internal const ushort LargeAsteroidStart = 0xce4b;
    /// <summary>$8B:CE53, exclusive end before the shared deletion opcode.</summary>
    internal const ushort LargeAsteroidEnd = 0xce53;

    /// <summary>$8B:CC4F, small-asteroid loop following the under-attack loop.</summary>
    private const ushort SmallAsteroidStart = 0xcc4f;
    /// <summary>$8B:CC57, two-frame purple-vortex loop.</summary>
    private const ushort VortexStart = 0xcc57;

    /// <summary>Returns one byte of a looping sprite instruction that waits ten ticks, selects a frame, then jumps to its list start.</summary>
    /// <param name="offset">Byte offset within the encoded loop instruction sequence.</param>
    /// <param name="start">Bank-$8B address to which the loop jumps after displaying its frame.</param>
    /// <param name="frame">Compiled sprite frame pointer emitted by the loop.</param>
    /// <returns>The requested byte of the instruction sequence, in little-endian word order.</returns>
    /// <exception cref="InvalidDataException">The byte offset is outside the encoded loop.</exception>
    private static byte LoopByte(int offset, ushort start, ushort frame)
    {
        ushort word = (offset / 2) switch
        {
            0 => 10,
            1 => frame,
            2 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            3 => start,
            _ => throw new InvalidDataException("Ceres flight loop cursor is invalid."),
        };
        return (byte)(word >> (8 * (offset & 1)));
    }

    /// <summary>Returns one byte of the vortex's alternating two-frame animation loop.</summary>
    /// <param name="offset">Byte offset within the six-word encoded loop.</param>
    /// <returns>The requested instruction byte in little-endian word order.</returns>
    /// <exception cref="InvalidDataException">The byte offset is outside the encoded vortex loop.</exception>
    private static byte VortexByte(int offset)
    {
        ushort word = (offset / 2) switch
        {
            0 or 2 => 1,
            1 => CeresFlightSpriteDefinitions.VortexEven,
            3 => CeresFlightSpriteDefinitions.VortexOdd,
            4 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            5 => VortexStart,
            _ => throw new InvalidDataException("Ceres vortex cursor is invalid."),
        };
        return (byte)(word >> (8 * (offset & 1)));
    }

    /// <summary>Reads one encoded instruction byte from the compiled rear-cluster, star, or large-asteroid lists.</summary>
    /// <param name="pointer">Bank-$8B address within one of the ranges bounded by this catalog.</param>
    /// <returns>The byte stored at that instruction address.</returns>
    /// <exception cref="InvalidDataException">The address is outside all supported compiled lists.</exception>
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is >= RearClusterStart and < RearClusterEnd)
            return pointer < SmallAsteroidStart
                ? LoopByte(pointer - RearClusterStart, RearClusterStart, CeresFlightSpriteDefinitions.StationUnderAttack)
                : pointer < VortexStart
                    ? LoopByte(pointer - SmallAsteroidStart, SmallAsteroidStart, CeresFlightSpriteDefinitions.SmallAsteroids)
                    : VortexByte(pointer - VortexStart);
        if (pointer is >= StarsStart and < StarsEnd)
            return LoopByte(pointer - StarsStart, StarsStart, CeresFlightSpriteDefinitions.Stars);
        if (pointer is >= LargeAsteroidStart and < LargeAsteroidEnd)
            return LoopByte(pointer - LargeAsteroidStart, LargeAsteroidStart, CeresFlightSpriteDefinitions.LargeAsteroids);
        throw new InvalidDataException(
            $"Ceres flight instruction $8B:{pointer:X4} leaves its compiled lists.");
    }

    /// <summary>Reads one little-endian instruction word while keeping both bytes inside the same compiled list.</summary>
    /// <param name="pointer">Bank-$8B address of the word's low byte.</param>
    /// <returns>The instruction word formed from the addressed byte and its successor.</returns>
    /// <exception cref="InvalidDataException">The word crosses a compiled-list boundary or either byte is outside a supported list.</exception>
    internal static ushort ReadWord(ushort pointer)
    {
        ushort next = unchecked((ushort)(pointer + 1));
        if (next is RearClusterEnd or StarsEnd or LargeAsteroidEnd)
            throw new InvalidDataException(
                $"Ceres flight word $8B:{pointer:X4} crosses a compiled-list boundary.");
        return (ushort)(ReadByte(pointer) | ReadByte(next) << 8);
    }
}
