namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles selected by
/// the 22 Kraid-arm extended frames at $A7:8F59-92B4. The second ten frames
/// reuse the first ten physical layouts but have independent editable OAM.
/// </summary>
internal static class KraidArmCollisionDefinitions
{
    internal const byte Bank = 0xa7;

    private static readonly KraidArmCollisionComponent[] Phase0 =
        [new(-36, -33, 0x92d1), new(-28, -24, 0x93f7), new(0, 0, 0x9439),
         new(-36, -40, 0x92d1), new(-28, -31, 0x93f7)];
    private static readonly KraidArmCollisionComponent[] Phase1 =
        [new(-38, -33, 0x92eb), new(-30, -26, 0x93f7), new(0, 0, 0x9439),
         new(-36, -40, 0x92d1), new(-28, -31, 0x93f7)];
    private static readonly KraidArmCollisionComponent[] Phase2 =
        [new(-48, -13, 0x92f9), new(-38, -13, 0x9371), new(0, 0, 0x941f),
         new(-45, -27, 0x92eb), new(-37, -19, 0x93f7)];
    private static readonly KraidArmCollisionComponent[] Phase3 =
        [new(-46, -13, 0x9313), new(-37, -13, 0x9371), new(0, 0, 0x941f),
         new(-45, -19, 0x92f9), new(-36, -18, 0x9371)];
    private static readonly KraidArmCollisionComponent[] Phase4 =
        [new(-45, 8, 0x9321), new(-38, 2, 0x937f), new(0, 0, 0x9411),
         new(-46, 3, 0x9313), new(-39, -3, 0x937f)];
    private static readonly KraidArmCollisionComponent[] Phase5 =
        [new(-44, 8, 0x9321), new(-37, 2, 0x937f), new(0, 0, 0x9411),
         new(-46, 4, 0x9321), new(-39, -2, 0x937f)];
    private static readonly KraidArmCollisionComponent[] Phase6 =
        [new(-39, 10, 0x933b), new(-38, 0, 0x9399), new(0, 0, 0x9411),
         new(-43, 10, 0x933b), new(-41, -2, 0x9399)];
    private static readonly KraidArmCollisionComponent[] Phase7 =
        [new(-39, 10, 0x933b), new(-38, 0, 0x9399), new(0, 0, 0x9411),
         new(-43, 9, 0x933b), new(-41, -2, 0x9399)];
    private static readonly KraidArmCollisionComponent[] Phase8 =
        [new(-39, 10, 0x9349), new(-38, 0, 0x9399), new(0, 0, 0x9411),
         new(-43, 9, 0x933b), new(-41, -2, 0x9399)];
    private static readonly KraidArmCollisionComponent[] Phase9 =
        [new(-39, 10, 0x9349), new(-38, 0, 0x9399), new(0, 0, 0x9411),
         new(-42, 9, 0x9349), new(-42, -2, 0x9399)];
    private static readonly KraidArmCollisionComponent[] Lunge =
        [new(0, 0, 0x946f)];
    private static readonly KraidArmCollisionComponent[] Dying =
        [new(0, 0, 0x947d)];

    private static readonly (ushort Pointer, KraidArmCollisionComponent[] Components)[] Frames =
    [
        (0x8f59, Phase0), (0x8f83, Phase1), (0x8fad, Phase2), (0x8fd7, Phase3),
        (0x9001, Phase4), (0x902b, Phase5), (0x9055, Phase6), (0x907f, Phase7),
        (0x90a9, Phase8), (0x90d3, Phase9),
        (0x90fd, Phase0), (0x9127, Phase1), (0x9151, Phase2), (0x917b, Phase3),
        (0x91a5, Phase4), (0x91cf, Phase5), (0x91f9, Phase6), (0x9223, Phase7),
        (0x924d, Phase8), (0x9277, Phase9),
        (0x92a1, Lunge), (0x92ab, Dying),
    ];

    internal static int FrameCount => Frames.Length;
    internal static ushort FramePointer(int index) => Frames[index].Pointer;

    private static readonly KraidArmCollisionHitbox[] Hitboxes =
    [
        new(-13, -11, -3, -5, 0x9490, 0x94b6),
        new(-9, -5, 1, 2, 0x9490, 0x94b6),
        new(-16, -5, 1, 2, 0x9490, 0x94b6),
        new(-9, -2, 1, 7, 0x9490, 0x94b6),
        new(-12, 3, -6, 12, 0x9490, 0x94b6),
        new(-6, -1, 1, 14, 0x9490, 0x94b6),
        new(-3, -2, 6, 9, 0x9490, 0x94b6),
        new(2, 7, 11, 11, 0x9490, 0x94b6),
        new(-1, -4, 14, 4, 0x9490, 0x94b6),
        new(-3, -7, 6, 2, 0x9490, 0x94b6),
        new(4, -12, 10, -1, 0x9490, 0x94b6),
        new(-15, -5, 2, 4, 0x9490, 0x94b6),
        new(-11, 2, -4, 10, 0x9490, 0x94b6),
        new(-6, -3, 3, 5, 0x9490, 0x94b6),
        new(-4, -2, 3, 13, 0x9490, 0x94b6),
        new(-12, -12, -3, -3, 0x9490, 0x94b6),
        new(-6, -6, 3, 2, 0x9490, 0x94b6),
        new(-45, -9, 4, 8, 0x948b, 0x94b6),
        new(-28, -17, -12, 0, 0x948b, 0x94b6),
        new(-42, -23, -28, -6, 0x948b, 0x94b6),
        new(-22, -25, -8, -5, 0x948b, 0x94b6),
        new(-35, -35, -19, -17, 0x948b, 0x94b6),
        new(-64, -48, -32, -16, 0x948b, 0x94b6),
        new(-64, -4, 0, 4, 0x948b, 0x94b6),
    ];

    internal static bool TryGetComponents(ushort pointer,
        out ReadOnlyMemory<KraidArmCollisionComponent> components)
    {
        foreach ((ushort frame, KraidArmCollisionComponent[] owned) in Frames)
        {
            if (frame != pointer) continue;
            components = owned;
            return true;
        }
        components = default;
        return false;
    }

    /// <summary>
    /// Bank-$A7 hitbox lists $92D1-$948A; the first overlapping rectangle wins.
    /// The native callbacks are Kraid-arm touch $9490 or background touch $948B,
    /// and Kraid-arm shot $94B6.
    /// </summary>
    internal static ReadOnlySpan<KraidArmCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer switch
        {
            0x92d1 => Hitboxes.AsSpan(0, 2),
            0x92eb => Hitboxes.AsSpan(2, 1),
            0x92f9 => Hitboxes.AsSpan(3, 2),
            0x9313 => Hitboxes.AsSpan(5, 1),
            0x9321 => Hitboxes.AsSpan(6, 2),
            0x933b => Hitboxes.AsSpan(8, 1),
            0x9349 => Hitboxes.AsSpan(9, 2),
            0x9371 => Hitboxes.AsSpan(11, 1),
            0x937f => Hitboxes.AsSpan(12, 2),
            0x9399 => Hitboxes.AsSpan(14, 1),
            0x93f7 => Hitboxes.AsSpan(15, 2),
            0x9411 => Hitboxes.AsSpan(17, 1),
            0x941f => Hitboxes.AsSpan(18, 2),
            0x9439 => Hitboxes.AsSpan(20, 2),
            0x946f => Hitboxes.AsSpan(22, 1),
            0x947d => Hitboxes.AsSpan(23, 1),
            _ => throw new InvalidDataException(
                $"Kraid-arm hitbox list $A7:{pointer:X4} is not compiled."),
        };
}

internal readonly record struct KraidArmCollisionComponent(short X, short Y, ushort HitboxPointer);
internal readonly record struct KraidArmCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
