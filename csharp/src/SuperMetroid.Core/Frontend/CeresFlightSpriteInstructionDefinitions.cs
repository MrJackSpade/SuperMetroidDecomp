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
    private static byte LoopByte(int offset, ushort start, ushort frame)
    {
        ushort word = (offset / 2) switch
        {
            0 => 10,
            1 => frame,
            2 => (ushort)CinematicSpriteInstruction.Goto,
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
            1 => CeresFlightSpriteDefinitions.VortexEven,
            3 => CeresFlightSpriteDefinitions.VortexOdd,
            4 => (ushort)CinematicSpriteInstruction.Goto,
            5 => VortexStart,
            _ => throw new InvalidDataException("Ceres vortex cursor is invalid."),
        };
        return (byte)(word >> (8 * (offset & 1)));
    }
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

    internal static ushort ReadWord(ushort pointer)
    {
        ushort next = unchecked((ushort)(pointer + 1));
        if (next >= EndOf(ListContaining(pointer)))
            throw new InvalidDataException(
                $"Ceres flight word $8B:{pointer:X4} crosses a compiled-list boundary.");
        return (ushort)(ReadByte(pointer) | ReadByte(next) << 8);
    }

    /// <summary>The three separately compiled bank-$8B lists this owner serves.</summary>
    private enum CompiledList
    {
        /// <summary>Under-attack, small-asteroid and vortex lists at <c>$8B:CC47-CC62</c>.</summary>
        RearCluster,
        /// <summary>Shared front/rear star list at <c>$8B:CDA3-CDAA</c>.</summary>
        Stars,
        /// <summary>Large-asteroid list at <c>$8B:CE4B-CE52</c>.</summary>
        LargeAsteroid,
    }

    private static ushort StartOf(CompiledList list) => list switch
    {
        CompiledList.RearCluster => RearClusterStart,
        CompiledList.Stars => StarsStart,
        CompiledList.LargeAsteroid => LargeAsteroidStart,
        _ => throw new InvalidOperationException($"Undefined {nameof(CompiledList)} {list}."),
    };

    private static ushort EndOf(CompiledList list) => list switch
    {
        CompiledList.RearCluster => RearClusterEnd,
        CompiledList.Stars => StarsEnd,
        CompiledList.LargeAsteroid => LargeAsteroidEnd,
        _ => throw new InvalidOperationException($"Undefined {nameof(CompiledList)} {list}."),
    };

    private static CompiledList ListContaining(ushort pointer)
    {
        foreach (CompiledList list in Enum.GetValues<CompiledList>())
        {
            if (pointer >= StartOf(list) && pointer < EndOf(list))
                return list;
        }
        throw new InvalidDataException(
            $"Ceres flight instruction $8B:{pointer:X4} leaves its compiled lists.");
    }
}
