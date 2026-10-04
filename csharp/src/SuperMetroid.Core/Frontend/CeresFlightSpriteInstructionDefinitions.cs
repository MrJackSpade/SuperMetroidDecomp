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
    /// <summary>$8C:9150, Ceres under-attack spritemap.</summary>
    private const ushort UnderAttackFrame = 0x9150;
    /// <summary>$8C:90FE, Ceres small-asteroid spritemap.</summary>
    private const ushort SmallAsteroidFrame = 0x90fe;
    /// <summary>$8C:8FE7, Ceres purple vortex first frame.</summary>
    private const ushort VortexFrame1 = 0x8fe7;
    /// <summary>$8C:93D1, Ceres purple vortex second frame.</summary>
    private const ushort VortexFrame2 = 0x93d1;
    /// <summary>$8C:9478, shared front/rear Ceres star field.</summary>
    private const ushort StarsFrame = 0x9478;
    /// <summary>$8C:94F7, Ceres explosion large asteroids.</summary>
    private const ushort LargeAsteroidFrame = 0x94f7;

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

    private static byte VortexByte(int offset)
    {
        ushort word = (offset / 2) switch
        {
            0 or 2 => 1,
            1 => VortexFrame1,
            3 => VortexFrame2,
            4 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            5 => VortexStart,
            _ => throw new InvalidDataException("Ceres vortex cursor is invalid."),
        };
        return (byte)(word >> (8 * (offset & 1)));
    }
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is >= RearClusterStart and < RearClusterEnd)
            return pointer < SmallAsteroidStart
                ? LoopByte(pointer - RearClusterStart, RearClusterStart, UnderAttackFrame)
                : pointer < VortexStart
                    ? LoopByte(pointer - SmallAsteroidStart, SmallAsteroidStart, SmallAsteroidFrame)
                    : VortexByte(pointer - VortexStart);
        if (pointer is >= StarsStart and < StarsEnd)
            return LoopByte(pointer - StarsStart, StarsStart, StarsFrame);
        if (pointer is >= LargeAsteroidStart and < LargeAsteroidEnd)
            return LoopByte(pointer - LargeAsteroidStart, LargeAsteroidStart, LargeAsteroidFrame);
        throw new InvalidDataException(
            $"Ceres flight instruction $8B:{pointer:X4} leaves its compiled lists.");
    }

    internal static ushort ReadWord(ushort pointer)
    {
        ushort next = unchecked((ushort)(pointer + 1));
        if (next is RearClusterEnd or StarsEnd or LargeAsteroidEnd)
            throw new InvalidDataException(
                $"Ceres flight word $8B:{pointer:X4} crosses a compiled-list boundary.");
        return (ushort)(ReadByte(pointer) | ReadByte(next) << 8);
    }
}
