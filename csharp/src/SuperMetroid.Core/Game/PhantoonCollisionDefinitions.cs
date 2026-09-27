using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned component offset and bank-$A7 hitbox-list identity.</summary>
internal readonly record struct PhantoonCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Engine-owned rectangle and touch/shot callbacks for Phantoon.</summary>
internal readonly record struct PhantoonCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Phantoon's three hitbox lists at $A7:E020, $E02E, and $E06C, separate from
/// its editable BG2 frames. The 22 frame roots at $A7:DEDD-$DFFD have zero
/// component offsets; the three tentacle roots each have two components.
/// </summary>
internal static class PhantoonCollisionDefinitions
{
    /// <summary>$A7:E020, the one-point no-op hitbox used by hidden parts.</summary>
    internal const ushort PointList = 0xe020;
    /// <summary>$A7:E02E, Phantoon's five-rectangle full body.</summary>
    internal const ushort FullBodyList = 0xe02e;
    /// <summary>$A7:E06C, the single vulnerable eye rectangle.</summary>
    internal const ushort EyeOnlyList = 0xe06c;
    /// <summary>$A7:DD95, Phantoon's active touch callback.</summary>
    internal const ushort TouchAi = 0xdd95;
    /// <summary>$A7:DD9B, Phantoon's active shot callback.</summary>
    internal const ushort ShotAi = 0xdd9b;

    private static readonly PhantoonCollisionComponent[] Point = [new(0, 0, PointList)];
    private static readonly PhantoonCollisionComponent[] FullBody = [new(0, 0, FullBodyList)];
    private static readonly PhantoonCollisionComponent[] EyeOnly = [new(0, 0, EyeOnlyList)];
    private static readonly PhantoonCollisionComponent[] DoublePoint =
        [new(0, 0, PointList), new(0, 0, PointList)];

    private static readonly PhantoonCollisionHitbox[] PointHitboxes =
        [new(0, 0, 0, 0, EnemyAiCodePointers.BankA0.NoOp,
            EnemyAiCodePointers.BankA0.NoOp)];
    private static readonly PhantoonCollisionHitbox[] FullBodyHitboxes =
    [
        new(-33, -40, 32, 56, TouchAi, ShotAi),
        new(-9, 22, 8, 39, TouchAi, ShotAi),
        new(-23, 52, -16, 71, TouchAi, ShotAi),
        new(15, 53, 22, 70, TouchAi, ShotAi),
        new(-12, 53, 11, 69, TouchAi, ShotAi),
    ];
    private static readonly PhantoonCollisionHitbox[] EyeOnlyHitboxes =
        [new(-9, 22, 8, 39, TouchAi, ShotAi)];

    internal static ReadOnlySpan<PhantoonCollisionComponent> ComponentsAt(ushort pointer)
    {
        if (!PhantoonBg2FrameDefinitions.IsFrame(pointer))
            throw new InvalidDataException($"Phantoon frame $A7:{pointer:X4} has no compiled hitbox identity.");
        return pointer switch
        {
            PhantoonBg2FrameDefinitions.BodyFullHitbox => FullBody,
            PhantoonBg2FrameDefinitions.BodyEyeHitboxOnly => EyeOnly,
            PhantoonBg2FrameDefinitions.Tentacles0 or
                PhantoonBg2FrameDefinitions.Tentacles1 or
                PhantoonBg2FrameDefinitions.Tentacles2 => DoublePoint,
            _ => Point,
        };
    }

    internal static ReadOnlySpan<PhantoonCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer switch
        {
            PointList => PointHitboxes,
            FullBodyList => FullBodyHitboxes,
            EyeOnlyList => EyeOnlyHitboxes,
            _ => throw new InvalidDataException(
                $"Phantoon hitbox list $A7:{pointer:X4} is not compiled."),
        };

}
