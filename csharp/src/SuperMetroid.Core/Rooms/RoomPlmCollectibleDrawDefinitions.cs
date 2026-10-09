namespace SuperMetroid.Core.Rooms;

/// <summary>One native one-block collectible draw list and its physical level word.</summary>
/// <param name="Pointer">Bank-$84 address of the native six-byte draw record.</param>
/// <param name="LevelWord">Complete physical level word written by the draw list.</param>
/// <param name="Id">Stable authoring identifier used to associate editable appearance data with this draw.</param>
internal readonly record struct RoomPlmCollectibleDrawFrame(
    ushort Pointer, ushort LevelWord, string Id);

/// <summary>
/// Compiled bank-$84 collectible draw geometry and level words. Only the low visual
/// twelve bits may be overridden; pickup, collision, and persistence stay native.
/// </summary>
internal static class RoomPlmCollectibleDrawDefinitions
{
    /// <summary>Empty-item draw list at $84:A2B5.</summary>
    internal const ushort Empty = 0xa2b5;
    /// <summary>Chozo orb first frame at $84:A2C7.</summary>
    internal const ushort OrbFirst = 0xa2c7;
    /// <summary>Chozo orb burst frame at $84:A2D9.</summary>
    internal const ushort OrbBurst = 0xa2d9;
    /// <summary>First exposed tank frame at $84:A2DF.</summary>
    internal const ushort TankFirst = 0xa2df;
    /// <summary>First dynamically assigned item frame at $84:A30F.</summary>
    internal const ushort DynamicFirst = 0xa30f;
    /// <summary>Shot-block reveal first frame at $84:A3DD.</summary>
    internal const ushort ShotRevealFirst = 0xa3dd;
    /// <summary>Dynamic-item first-frame selector table at $84:E05F.</summary>
    internal const ushort DynamicFrame0Table = 0xe05f;
    /// <summary>Dynamic-item second-frame selector table at $84:E077.</summary>
    internal const ushort DynamicFrame1Table = 0xe077;

    /// <summary>Enumerates every supported collectible draw frame, including orb, tank, dynamic-slot, and reveal layouts.</summary>
    internal static IEnumerable<RoomPlmCollectibleDrawFrame> All
    {
        get
        {
            TryGet(Empty, out var empty);
            yield return empty;
            for (int frame = 0; frame < 20; frame++)
            {
                TryGet((ushort)(OrbFirst + frame * 6), out var value);
                yield return value;
            }
            for (int frame = 0; frame < 3; frame++)
            {
                TryGet((ushort)(ShotRevealFirst + frame * 6), out var value);
                yield return value;
            }
        }
    }

    /// <summary>
    /// Twenty-four one-cell lists: air, three shootable orb frames and solid burst,
    /// eight trigger tank frames, eight trigger dynamic-slot frames, and three
    /// solid reveal frames. Six-byte native records advance one tile within each
    /// group. Preserve holes between groups and reject incomplete record starts.
    /// </summary>
    internal static bool TryGet(ushort pointer, out RoomPlmCollectibleDrawFrame frame) => TryResolve(pointer, true, out frame);

    /// <summary>Physical runtime projection without constructing exported artwork names.</summary>
    internal static bool TryGetWord(ushort pointer, out ushort word)
    {
        bool found = TryResolve(pointer, false, out var frame);
        word = frame.LevelWord;
        return found;
    }

    /// <summary>
    /// Resolves a recognized native draw-record start to its physical level word and optionally
    /// its stable authoring identifier.
    /// </summary>
    /// <param name="pointer">Bank-$84 address to resolve; it must point to a complete record start.</param>
    /// <param name="includeId">Whether to construct the asset identifier when the caller needs exported artwork metadata.</param>
    /// <param name="frame">Receives the resolved draw, or the default value when the address is not recognized.</param>
    /// <returns><see langword="true"/> when the pointer identifies a supported draw record.</returns>
    private static bool TryResolve(ushort pointer, bool includeId, out RoomPlmCollectibleDrawFrame frame)
    {
        int index;
        ushort word;
        string id;
        if (pointer == Empty) { word = 0x00ff; id = "empty"; }
        else if (TryIndex(pointer, OrbFirst, 3, out index))
        { word = (ushort)(0xc072 + index); id = includeId ? $"chozo-orb-{index}" : string.Empty; }
        else if (pointer == OrbBurst) { word = 0x8075; id = "chozo-orb-burst"; }
        else if (TryIndex(pointer, TankFirst, 8, out index))
        {
            word = (ushort)(0xb04a + index);
            string kind = (index / 2) switch
            {
                0 => "energy", 1 => "missile", 2 => "super-missile", _ => "power-bomb",
            };
            id = includeId ? $"{kind}-tank-{index % 2}" : string.Empty;
        }
        else if (TryIndex(pointer, DynamicFirst, 8, out index))
        { word = (ushort)(0xb08e + index); id = includeId ? $"dynamic-slot-{index / 2}-frame-{index % 2}" : string.Empty; }
        else if (TryIndex(pointer, ShotRevealFirst, 3, out index))
        { word = (ushort)(0x8053 + index); id = includeId ? $"shot-reveal-{index}" : string.Empty; }
        else { frame = default; return false; }
        frame = new(pointer, word, id);
        return true;
    }
    /// <summary>Maps a six-byte record address to its zero-based position within a contiguous native draw group.</summary>
    /// <param name="pointer">Address being checked.</param>
    /// <param name="first">Address of the group's first record.</param>
    /// <param name="count">Number of records in the group.</param>
    /// <param name="index">Receives the record position calculated from the address offset.</param>
    /// <returns><see langword="true"/> only for aligned record starts within the group's range.</returns>
    private static bool TryIndex(ushort pointer, ushort first, int count, out int index)
    {
        int offset = pointer - first;
        index = offset / 6;
        return offset >= 0 && offset % 6 == 0 && index < count;
    }
    /// <summary>Finds a supported collectible draw by its exact, case-sensitive authoring identifier.</summary>
    /// <param name="id">Stable identifier assigned to a draw frame.</param>
    /// <param name="frame">Receives the matching draw, or the default value when no identifier matches.</param>
    /// <returns><see langword="true"/> when the identifier belongs to one of the catalog's supported draw frames.</returns>
    internal static bool TryGetById(string id, out RoomPlmCollectibleDrawFrame frame)
    {
        foreach (var candidate in All)
        {
            if (!string.Equals(candidate.Id, id, StringComparison.Ordinal)) continue;
            frame = candidate;
            return true;
        }
        frame = default;
        return false;
    }

    /// <summary>$84:DFB3 cycles forward through three orb draws and back through the middle draw.</summary>
    internal static ushort OrbFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 4)
            throw new InvalidDataException(
                $"Chozo orb animation index {animationIndex} is outside four authored phases.");
        return checked((ushort)(OrbFirst + (2 - Math.Abs(animationIndex - 2)) * 6));
    }

    /// <summary>$84:E012 reveals three consecutive draws; $84:E022 consumes them in reverse.</summary>
    internal static ushort ShotRevealFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 3)
            throw new InvalidDataException(
                $"Shot-block reveal index {animationIndex} is outside three authored frames.");
        return checked((ushort)(ShotRevealFirst + animationIndex * 6));
    }

    /// <summary>Two six-byte draws per tank kind or allocated dynamic graphics slot.</summary>
    internal static ushort VisibleFrame(InWorldCollectibleKind kind,
        int animationIndex, int graphicsSlot)
    {
        if ((uint)animationIndex >= 2)
            throw new InvalidDataException(
                $"Collectible animation index {animationIndex} is outside two authored frames.");
        if (kind <= InWorldCollectibleKind.PowerBombTank)
            return checked((ushort)(TankFirst + (int)kind * 12 + animationIndex * 6));
        if ((uint)graphicsSlot >= 4)
            throw new InvalidDataException(
                $"Dynamic collectible graphics slot {graphicsSlot} is outside four allocations.");
        return checked((ushort)(DynamicFirst + graphicsSlot * 12 + animationIndex * 6));
    }
}
