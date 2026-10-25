using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Compiled collectible draw geometry and level words. Only the low visual
/// twelve bits may be overridden; pickup, collision, and persistence stay native.
/// </summary>
internal static class RoomPlmCollectibleDrawDefinitions
{
    /// <summary>The six collectible draw groups, in the order their frames are published.</summary>
    internal enum DrawGroup
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

    /// <summary>One collectible draw: a group and the frame within it.</summary>
    /// <param name="Group">The group of frames the draw belongs to.</param>
    /// <param name="Index">The frame within the group, counted from zero.</param>
    internal readonly record struct Draw(DrawGroup Group, [property: IntegerQuantity("frame within the collectible draw group")] int Index);

    /// <summary>The air tile left after an item is removed.</summary>
    internal static PlmFrameDraw<Draw> Empty { get; } = PlmDraw.Of(new Draw(DrawGroup.Empty, 0));

    /// <summary>The solid burst frame drawn when a Chozo orb opens.</summary>
    internal static PlmFrameDraw<Draw> OrbBurst { get; } = PlmDraw.Of(new Draw(DrawGroup.OrbBurst, 0));

    /// <summary>Enumerates all 24 collectible one-cell draws in their published order.</summary>
    internal static IEnumerable<RoomPlmCollectibleDrawFrame> All
    {
        get
        {
            foreach (DrawGroup group in Enum.GetValues<DrawGroup>())
            {
                for (int index = 0; index < CountOf(group); index++)
                {
                    var draw = new Draw(group, index);
                    yield return new(draw, new RoomLevelWord(Word(draw)), VisualId(draw));
                }
            }
        }
    }

    /// <summary>Physical runtime projection without constructing exported artwork names.</summary>
    internal static bool TryGetWord(PlmDraw request, out int word)
    {
        bool found = TryLocate(request, out Draw draw);
        word = found ? Word(draw) : (ushort)0;
        return found;
    }

    /// <summary>The number of frames in a group.</summary>
    private static int CountOf(DrawGroup group) => group switch
    {
        DrawGroup.Empty => 1,
        DrawGroup.Orb => 3,
        DrawGroup.OrbBurst => 1,
        DrawGroup.Tank => 8,
        DrawGroup.Dynamic => 8,
        DrawGroup.ShotReveal => 3,
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {group}."),
    };

    /// <summary>Accepts a collectible draw whose frame lies within its group.</summary>
    private static bool TryLocate(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw frame) => (uint)frame.Index < (uint)CountOf(frame.Group), out draw);

    /// <summary>The physical level word a draw writes; each group advances one tile per frame.</summary>
    private static ushort Word(Draw draw) => draw.Group switch
    {
        DrawGroup.Empty => 0x00ff,
        DrawGroup.Orb => (ushort)(0xc072 + draw.Index),
        DrawGroup.OrbBurst => 0x8075,
        DrawGroup.Tank => (ushort)(0xb04a + draw.Index),
        DrawGroup.Dynamic => (ushort)(0xb08e + draw.Index),
        DrawGroup.ShotReveal => (ushort)(0x8053 + draw.Index),
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {draw.Group}."),
    };

    /// <summary>The editable artwork identifier of a draw.</summary>
    internal static string VisualId(Draw draw) => draw.Group switch
    {
        DrawGroup.Empty => "empty",
        DrawGroup.Orb => $"chozo-orb-{draw.Index}",
        DrawGroup.OrbBurst => "chozo-orb-burst",
        DrawGroup.Tank => $"{TankKindName(draw.Index / 2)}-tank-{draw.Index % 2}",
        DrawGroup.Dynamic => $"dynamic-slot-{draw.Index / 2}-frame-{draw.Index % 2}",
        DrawGroup.ShotReveal => $"shot-reveal-{draw.Index}",
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {draw.Group}."),
    };

    /// <summary>Gets the stable visual ID stem for one of the four tank pickup kinds.</summary>
    private static string TankKindName(int kind) => kind switch
    {
        0 => "energy",
        1 => "missile",
        2 => "super-missile",
        _ => "power-bomb",
    };

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

    /// <summary>The orb cycles forward through its three draws and back through the middle draw.</summary>
    internal static PlmFrameDraw<Draw> OrbFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 4)
            throw new InvalidDataException(
                $"Chozo orb animation index {animationIndex} is outside four authored phases.");
        return PlmDraw.Of(new Draw(DrawGroup.Orb, 2 - Math.Abs(animationIndex - 2)));
    }

    /// <summary>A shot-block reveal shows three consecutive draws; hiding consumes them in reverse.</summary>
    internal static PlmFrameDraw<Draw> ShotRevealFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 3)
            throw new InvalidDataException(
                $"Shot-block reveal index {animationIndex} is outside three authored frames.");
        return PlmDraw.Of(new Draw(DrawGroup.ShotReveal, animationIndex));
    }

    /// <summary>The tank kinds in the order of their draws.</summary>
    private static readonly InWorldCollectibleKind[] TankKinds =
    [
        InWorldCollectibleKind.EnergyTank, InWorldCollectibleKind.MissileTank,
        InWorldCollectibleKind.SuperMissileTank, InWorldCollectibleKind.PowerBombTank,
    ];

    /// <summary>Two draws per tank kind or allocated dynamic graphics slot.</summary>
    internal static PlmFrameDraw<Draw> VisibleFrame(InWorldCollectibleKind kind,
        int animationIndex, int graphicsSlot)
    {
        if ((uint)animationIndex >= 2)
            throw new InvalidDataException(
                $"Collectible animation index {animationIndex} is outside two authored frames.");
        int tank = Array.IndexOf(TankKinds, kind);
        if (tank >= 0)
            return PlmDraw.Of(new Draw(DrawGroup.Tank, tank * 2 + animationIndex));
        if ((uint)graphicsSlot >= 4)
            throw new InvalidDataException(
                $"Dynamic collectible graphics slot {graphicsSlot} is outside four allocations.");
        return PlmDraw.Of(new Draw(DrawGroup.Dynamic, graphicsSlot * 2 + animationIndex));
    }
}
