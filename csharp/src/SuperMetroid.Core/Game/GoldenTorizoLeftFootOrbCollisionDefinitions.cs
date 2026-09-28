namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned components of the five new left-foot-forward right-facing
/// Chozo-orb poses at $AA:AC06-AC6E. The opening $AA:ABEC frame is already
/// owned by the right-facing sonic attack. All these poses share the same
/// body and empty limb hitbox lists as the right-foot-forward orb attack.
/// </summary>
internal static class GoldenTorizoLeftFootOrbCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xac06, [new(9, -31, 0x87c7), new(4, -25, 0x89c8), new(0, 0, 0x8a88)]),
        (0xac20, [new(9, -31, 0x87c7), new(4, -25, 0x89d8), new(0, 0, 0x8a88)]),
        (0xac3a, [new(9, -31, 0x87c7), new(4, -25, 0x89e8), new(0, 0, 0x8a88)]),
        (0xac54, [new(4, -25, 0x89f8), new(9, -31, 0x87c7), new(0, 0, 0x8a88)]),
        (0xac6e, [new(4, -25, 0x89f8), new(9, -31, 0x87c7), new(0, 0, 0x8a88)]),
    ];

    internal static int FrameCount => Frames.Length;
    internal static ushort FramePointer(int index) => Frames[index].Pointer;

    internal static bool TryGetComponents(ushort frame,
        out ReadOnlyMemory<GoldenTorizoCollisionComponent> components)
    {
        foreach ((ushort pointer, GoldenTorizoCollisionComponent[] owned) in Frames)
        {
            if (pointer != frame) continue;
            components = owned;
            return true;
        }
        components = default;
        return false;
    }

    internal static bool HasFrame(ushort frame)
    {
        foreach ((ushort pointer, _) in Frames)
            if (pointer == frame) return true;
        return false;
    }

    internal static ReadOnlySpan<GoldenTorizoCollisionHitbox> HitboxesAt(ushort pointer) =>
        GoldenTorizoRightOrbCollisionDefinitions.HitboxesAt(pointer);
}
