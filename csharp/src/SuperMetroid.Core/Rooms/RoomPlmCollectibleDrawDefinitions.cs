namespace SuperMetroid.Core.Rooms;

/// <summary>One native one-block collectible draw list and its physical level word.</summary>
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

    private static readonly RoomPlmCollectibleDrawFrame[] Frames =
    [
        new(0xa2b5, 0x00ff, "empty"),
        new(0xa2c7, 0xc072, "chozo-orb-0"),
        new(0xa2cd, 0xc073, "chozo-orb-1"),
        new(0xa2d3, 0xc074, "chozo-orb-2"),
        new(0xa2d9, 0x8075, "chozo-orb-burst"),
        new(0xa2df, 0xb04a, "energy-tank-0"),
        new(0xa2e5, 0xb04b, "energy-tank-1"),
        new(0xa2eb, 0xb04c, "missile-tank-0"),
        new(0xa2f1, 0xb04d, "missile-tank-1"),
        new(0xa2f7, 0xb04e, "super-missile-tank-0"),
        new(0xa2fd, 0xb04f, "super-missile-tank-1"),
        new(0xa303, 0xb050, "power-bomb-tank-0"),
        new(0xa309, 0xb051, "power-bomb-tank-1"),
        new(0xa30f, 0xb08e, "dynamic-slot-0-frame-0"),
        new(0xa315, 0xb08f, "dynamic-slot-0-frame-1"),
        new(0xa31b, 0xb090, "dynamic-slot-1-frame-0"),
        new(0xa321, 0xb091, "dynamic-slot-1-frame-1"),
        new(0xa327, 0xb092, "dynamic-slot-2-frame-0"),
        new(0xa32d, 0xb093, "dynamic-slot-2-frame-1"),
        new(0xa333, 0xb094, "dynamic-slot-3-frame-0"),
        new(0xa339, 0xb095, "dynamic-slot-3-frame-1"),
        new(0xa3dd, 0x8053, "shot-reveal-0"),
        new(0xa3e3, 0x8054, "shot-reveal-1"),
        new(0xa3e9, 0x8055, "shot-reveal-2"),
    ];

    internal static ReadOnlySpan<RoomPlmCollectibleDrawFrame> All => Frames;

    internal static bool TryGet(ushort pointer, out RoomPlmCollectibleDrawFrame frame)
    {
        foreach (RoomPlmCollectibleDrawFrame candidate in Frames)
        {
            if (candidate.Pointer != pointer) continue;
            frame = candidate;
            return true;
        }
        frame = default;
        return false;
    }

    internal static bool TryGetById(string id, out RoomPlmCollectibleDrawFrame frame)
    {
        foreach (RoomPlmCollectibleDrawFrame candidate in Frames)
        {
            if (!string.Equals(candidate.Id, id, StringComparison.Ordinal)) continue;
            frame = candidate;
            return true;
        }
        frame = default;
        return false;
    }

    internal static ushort OrbFrame(int animationIndex) => animationIndex switch
    {
        0 => OrbFirst,
        1 or 3 => unchecked((ushort)(OrbFirst + 6)),
        2 => unchecked((ushort)(OrbFirst + 12)),
        _ => throw new InvalidDataException(
            $"Chozo orb animation index {animationIndex} is outside four authored phases."),
    };

    internal static ushort ShotRevealFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 3)
            throw new InvalidDataException(
                $"Shot-block reveal index {animationIndex} is outside three authored frames.");
        return checked((ushort)(ShotRevealFirst + animationIndex * 6));
    }

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
