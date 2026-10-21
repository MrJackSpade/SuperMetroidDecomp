using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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

    /// <summary>Enumerates all 24 collectible one-cell draw lists in native address order.</summary>
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
    internal static bool TryGet(ushort pointer, out RoomPlmCollectibleDrawFrame frame)
    {
        bool found = TryLocate(pointer, out DrawGroup group, out int index);
        frame = found ? new(pointer, new RoomLevelWord(Word(group, index)), VisualId(group, index)) : default;
        return found;
    }

    /// <summary>Physical runtime projection without constructing exported artwork names.</summary>
    internal static bool TryGetWord(ushort pointer, out ushort word)
    {
        bool found = TryLocate(pointer, out DrawGroup group, out int index);
        word = found ? Word(group, index) : (ushort)0;
        return found;
    }

    /// <summary>The six draw-list groups in address order; holes between them are not draw lists.</summary>
    private enum DrawGroup
    {
        /// <summary>Air tile after an item is removed.</summary>
        Empty,
        /// <summary>Three shootable Chozo orb animation frames.</summary>
        Orb,
        /// <summary>Solid burst frame when a Chozo orb opens.</summary>
        OrbBurst,
        /// <summary>Two visible frames for each of four tank pickup kinds.</summary>
        Tank,
        /// <summary>Two frames for each dynamically allocated item graphics slot.</summary>
        Dynamic,
        /// <summary>Three solid frames of a shot-block item reveal.</summary>
        ShotReveal,
    }

    /// <summary>Stable order of the six disjoint collectible draw address groups.</summary>
    private static readonly DrawGroup[] DrawGroups = Enum.GetValues<DrawGroup>();

    /// <summary>Gets a group's first native draw pointer and number of six-byte records.</summary>
    private static CollectibleDrawSpan SpanOf(DrawGroup group) => group switch
    {
        DrawGroup.Empty => new CollectibleDrawSpan(Empty, 1),
        DrawGroup.Orb => new CollectibleDrawSpan(OrbFirst, 3),
        DrawGroup.OrbBurst => new CollectibleDrawSpan(OrbBurst, 1),
        DrawGroup.Tank => new CollectibleDrawSpan(TankFirst, 8),
        DrawGroup.Dynamic => new CollectibleDrawSpan(DynamicFirst, 8),
        DrawGroup.ShotReveal => new CollectibleDrawSpan(ShotRevealFirst, 3),
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {(int)group}."),
    };

    /// <summary>Finds the group a draw pointer belongs to and its record index within the group.</summary>
    private static bool TryLocate(ushort pointer, out DrawGroup group, out int index)
    {
        foreach (DrawGroup candidate in DrawGroups)
        {
            (int first, int count) = SpanOf(candidate);
            if (!TryIndex(pointer, checked((ushort)first), count, out index)) continue;
            group = candidate;
            return true;
        }
        group = default;
        index = 0;
        return false;
    }

    /// <summary>The physical level word a located draw record writes.</summary>
    private static ushort Word(DrawGroup group, int index) => group switch
    {
        DrawGroup.Empty => 0x00ff,
        DrawGroup.Orb => (ushort)(0xc072 + index),
        DrawGroup.OrbBurst => 0x8075,
        DrawGroup.Tank => (ushort)(0xb04a + index),
        DrawGroup.Dynamic => (ushort)(0xb08e + index),
        DrawGroup.ShotReveal => (ushort)(0x8053 + index),
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {(int)group}."),
    };

    /// <summary>The editable artwork identifier of a located draw record.</summary>
    private static string VisualId(DrawGroup group, int index) => group switch
    {
        DrawGroup.Empty => "empty",
        DrawGroup.Orb => $"chozo-orb-{index}",
        DrawGroup.OrbBurst => "chozo-orb-burst",
        DrawGroup.Tank => $"{TankKindName(index / 2)}-tank-{index % 2}",
        DrawGroup.Dynamic => $"dynamic-slot-{index / 2}-frame-{index % 2}",
        DrawGroup.ShotReveal => $"shot-reveal-{index}",
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {(int)group}."),
    };

    /// <summary>Gets the stable visual ID stem for one of the four tank pickup kinds.</summary>
    private static string TankKindName(int kind) => kind switch
    {
        0 => "energy", 1 => "missile", 2 => "super-missile", _ => "power-bomb",
    };

    /// <summary>Checks that a pointer starts an owned six-byte draw record within a group.</summary>
    private static bool TryIndex(ushort pointer, ushort first, int count, out int index)
    {
        int offset = pointer - first;
        index = offset / 6;
        return offset >= 0 && offset % 6 == 0 && index < count;
    }
    /// <summary>Resolves an exact artwork identifier to its compiled collectible draw frame.</summary>
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
