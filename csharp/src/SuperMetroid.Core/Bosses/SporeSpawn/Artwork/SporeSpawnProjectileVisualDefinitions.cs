using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileProgram;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The presentation boundary of Spore Spawn's projectiles (#641): the bank-$86 list entries the projectile
/// definitions name, and each typed frame's installed artwork key, which is its native spritemap operand's address.
/// </summary>
internal static class SporeSpawnProjectileVisualDefinitions
{
    /// <summary>The closed ceiling emitter's list at $86:DC00.</summary>
    internal const ushort SpawnerClosedProgram = 0xdc00;
    /// <summary>The falling spore's list at $86:DC1E.</summary>
    internal const ushort SporeProgram = 0xdc1e;
    /// <summary>The stalk segment's list at $86:DC2E.</summary>
    internal const ushort StalkProgram = 0xdc2e;
    /// <summary>The shot spore's list at $86:DC34.</summary>
    internal const ushort SporeShotProgram = 0xdc34;

    /// <summary>The installed projectile frames.</summary>
    internal const int ProjectileFrameCount = 17;

    /// <summary>Every frame in installed order.</summary>
    internal static IReadOnlyList<SporeSpawnProjectileFrame> ProjectileFrames { get; } =
    [
        .. new[] { P.SpawnerClosed, P.SpawnerRelease, P.Spore, P.Stalk, P.SporeShot }.SelectMany(program =>
            Enumerable.Range(0, SporeSpawnProjectileAnimations.PoseCount(program)).Select(phase => new SporeSpawnProjectileFrame(program, phase))),
    ];

    /// <summary>The installed artwork key of <paramref name="frame"/>: its native operand's address.</summary>
    internal static ushort PresentationKey(SporeSpawnProjectileFrame frame) => (frame.Program, frame.Phase) switch
    {
        (P.SpawnerClosed, 0) => 0xdc02,
        (P.SpawnerRelease, 0) => 0xdc08,
        (P.SpawnerRelease, 1) => 0xdc0c,
        (P.SpawnerRelease, 2) => 0xdc12,
        (P.SpawnerRelease, 3) => 0xdc16,
        (P.SpawnerRelease, 4) => 0xdc1a,
        (P.Spore, 0) => 0xdc20,
        (P.Spore, 1) => 0xdc24,
        (P.Spore, 2) => 0xdc28,
        (P.Stalk, 0) => 0xdc30,
        (P.SporeShot, 0) => 0xdc36,
        (P.SporeShot, 1) => 0xdc3c,
        (P.SporeShot, 2) => 0xdc40,
        (P.SporeShot, 3) => 0xdc44,
        (P.SporeShot, 4) => 0xdc4a,
        (P.SporeShot, 5) => 0xdc4e,
        (P.SporeShot, 6) => 0xdc52,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Spore Spawn projectile frame."),
    };
}
