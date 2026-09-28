namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned physical components of the shared falling-left extended
/// frame $AA:B014. Editable OAM data cannot change these hitboxes.
/// </summary>
internal static class TorizoFallingLeftCollisionDefinitions
{
    internal const byte Bank = 0xaa;
    internal const ushort Frame = TorizoFallingLeftInstructionProgramDefinitions.FallingFrame;

    private static readonly GoldenTorizoCollisionComponent[] Components =
    [
        new(-16, -30, 0x87c7),
        new(-4, -24, 0x8858),
        new(0, 0, 0x891c),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] Body =
        [new(-18, -38, 7, 9, 0xc977, 0xc97c)];

    internal static bool TryGetComponents(ushort frame,
        out ReadOnlyMemory<GoldenTorizoCollisionComponent> components)
    {
        if (frame == Frame)
        {
            components = Components;
            return true;
        }
        components = default;
        return false;
    }

    internal static ReadOnlySpan<GoldenTorizoCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer switch
        {
            0x891c => Body,
            0x87c7 or 0x8858 => [],
            _ => throw new InvalidDataException(
                $"Torizo falling-left hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
