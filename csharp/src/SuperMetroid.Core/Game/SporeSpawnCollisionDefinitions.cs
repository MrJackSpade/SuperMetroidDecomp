namespace SuperMetroid.Core.Game;

/// <summary>Fixed offset and bank-$A5 hitbox-list identity for Spore Spawn.</summary>
internal readonly record struct SporeSpawnCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>Cartridge rectangle and touch/shot callbacks, independent of OAM artwork.</summary>
internal readonly record struct SporeSpawnCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// The twelve bank-$A5 Spore Spawn extended OAM frames at $EE65-$EF61 and
/// their fixed gameplay hitboxes. The visual catalog currently labels these
/// frames as Draygon; collision must follow the actual Spore Spawn program.
/// </summary>
internal static class SporeSpawnCollisionDefinitions
{
    /// <summary>$A5:EF73, closed-head hitbox with no active touch response.</summary>
    internal const ushort ClosedHead = 0xef73;
    /// <summary>$A5:EF8D, open-head hitbox.</summary>
    internal const ushort OpenHead = 0xef8d;
    /// <summary>$A5:EFA7, extended open-head hitbox.</summary>
    internal const ushort ExtendedHead = 0xefa7;
    /// <summary>$A5:EFC1, first four-rectangle moving head.</summary>
    internal const ushort MovingHead0 = 0xefc1;
    /// <summary>$A5:EFF3, second four-rectangle moving head.</summary>
    internal const ushort MovingHead1 = 0xeff3;
    /// <summary>$A5:F025, third four-rectangle moving head.</summary>
    internal const ushort MovingHead2 = 0xf025;
    /// <summary>$A5:F057, fourth four-rectangle moving head.</summary>
    internal const ushort MovingHead3 = 0xf057;
    /// <summary>$A5:F139, trailing vulnerable point with Spore Spawn shot AI.</summary>
    internal const ushort TrailingShotPoint = 0xf139;
    /// <summary>$A5:F147, mirrored trailing vulnerable point.</summary>
    internal const ushort MirroredTrailingShotPoint = 0xf147;
    /// <summary>$A5:F155, trailing point with dud-shot AI.</summary>
    internal const ushort TrailingDudPoint = 0xf155;
    /// <summary>$A5:F1A9, fifth four-rectangle moving head.</summary>
    internal const ushort MovingHead4 = 0xf1a9;
    /// <summary>$A5:F1DB, sixth four-rectangle moving head.</summary>
    internal const ushort MovingHead5 = 0xf1db;

    private const ushort Touch = EnemyAiCodePointers.BankA5.SporeSpawnTouch;
    private const ushort Shot = EnemyAiCodePointers.BankA5.SporeSpawnShot;
    private const ushort NoOp = EnemyAiCodePointers.BankA0.NoOp;
    private const ushort Dud = EnemyAiCodePointers.BankA0.DudShot;

    private static readonly Dictionary<ushort, SporeSpawnCollisionComponent[]> Frames =
        new()
        {
            [0xee65] = [new(0, 0, ClosedHead)],
            [0xee6f] = [new(0, 0, OpenHead)],
            [0xee79] = [new(0, 0, ExtendedHead), new(0, 0, TrailingShotPoint)],
            [0xee8b] = [new(0, 0, MovingHead0), new(0, 0, MirroredTrailingShotPoint)],
            [0xee9d] = [new(0, 0, MovingHead1), new(0, 0, TrailingDudPoint)],
            [0xeeaf] = [new(0, 0, MovingHead2), new(0, 0, MirroredTrailingShotPoint)],
            [0xeec1] = [new(0, 0, MovingHead3), new(0, 0, TrailingShotPoint)],
            [0xeed3] = [new(0, 0, MovingHead4), new(0, 0, MirroredTrailingShotPoint)],
            [0xeee5] = [new(0, 0, MovingHead5), new(0, 0, TrailingDudPoint)],
            [0xef3d] = [new(0, 0, MovingHead5), new(0, 0, TrailingShotPoint)],
            [0xef4f] = [new(0, 0, MovingHead5), new(0, 0, MirroredTrailingShotPoint)],
            [0xef61] = [new(0, 0, MovingHead5), new(0, 0, TrailingDudPoint)],
        };

    private static readonly Dictionary<ushort, SporeSpawnCollisionHitbox[]> Lists =
        new()
        {
            [ClosedHead] =
            [
                new(-41, -30, 41, 30, NoOp, Dud),
                new(-16, -45, 15, -30, NoOp, Dud),
            ],
            [OpenHead] =
            [
                new(-41, -30, 41, 30, Touch, Dud),
                new(-16, -45, 15, -30, Touch, Dud),
            ],
            [ExtendedHead] =
            [
                new(-44, -35, 43, 33, Touch, Dud),
                new(-16, -49, 15, -35, Touch, Dud),
            ],
            [MovingHead0] =
            [
                new(-45, -38, 44, -9, Touch, Dud),
                new(-45, 8, 44, 35, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -54, 16, -22, Touch, Dud),
            ],
            [MovingHead1] =
            [
                new(-43, -44, 42, -13, Touch, Dud),
                new(-44, 12, 42, 42, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -58, 16, -42, Touch, Dud),
            ],
            [MovingHead2] =
            [
                new(-45, -47, 44, -17, Touch, Dud),
                new(-44, 16, 43, 46, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -62, 16, -45, Touch, Dud),
            ],
            [MovingHead3] =
            [
                new(-44, -50, 45, -21, Touch, Dud),
                new(-43, 20, 43, 50, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -64, 16, -48, Touch, Dud),
            ],
            [TrailingShotPoint] =
                [new(-15, -24, 14, 23, Touch, Shot)],
            [MirroredTrailingShotPoint] =
                [new(-15, -24, 14, 23, Touch, Shot)],
            [TrailingDudPoint] =
                [new(-15, -24, 14, 23, Touch, Dud)],
            [MovingHead4] =
            [
                new(-44, -53, 44, -23, Touch, Dud),
                new(-44, 22, 43, 52, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -68, 16, -48, Touch, Dud),
            ],
            [MovingHead5] =
            [
                new(-44, -55, 43, -25, Touch, Dud),
                new(-45, 24, 43, 55, Touch, Dud),
                new(-15, -25, 14, 24, Touch, Shot),
                new(-16, -69, 16, -48, Touch, Dud),
            ],
        };

    internal static int FrameCount => Frames.Count;
    internal static int ListCount => Lists.Count;

    internal static bool IsFrame(ushort pointer) => Frames.ContainsKey(pointer);

    internal static ReadOnlySpan<SporeSpawnCollisionComponent> ComponentsAt(
        ushort pointer) =>
        Frames.TryGetValue(pointer, out SporeSpawnCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException(
                $"Spore Spawn frame $A5:{pointer:X4} has no compiled collision identity.");

    internal static ReadOnlySpan<SporeSpawnCollisionHitbox> HitboxesAt(
        ushort pointer) =>
        Lists.TryGetValue(pointer, out SporeSpawnCollisionHitbox[]? hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Spore Spawn hitbox list $A5:{pointer:X4} is not compiled.");
}
