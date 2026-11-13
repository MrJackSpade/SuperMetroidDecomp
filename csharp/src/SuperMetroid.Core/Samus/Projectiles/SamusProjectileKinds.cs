using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileKindDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program each projectile kind starts.</summary>
internal static class SamusProjectileKinds
{
    /// <summary>The program a shot of the directional <paramref name="kind"/> fired in <paramref name="direction"/> starts.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Shots of the kind start one program whatever their direction.</exception>
    internal static SamusProjectileProgram Program(SamusProjectileKind kind, SamusProjectileDirection direction) =>
        DirectionalPrograms.TryGetValue(kind, out SamusProjectileDirectionMap<SamusProjectileProgram>? programs)
            ? programs.For(direction)
            : throw new ArgumentOutOfRangeException(nameof(kind), kind,
                $"Projectile kind {kind} starts one program whatever its direction; no shot reads it by direction.");

    /// <summary>The one program a shot, bomb or particle of <paramref name="kind"/> starts, whatever its direction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Shots of the kind start a program for each direction.</exception>
    internal static SamusProjectileProgram SoleProgram(SamusProjectileKind kind) =>
        SolePrograms.TryGetValue(kind, out SamusProjectileProgram program)
            ? program
            : throw new ArgumentOutOfRangeException(nameof(kind), kind,
                $"Projectile kind {kind} starts a program for each direction; no shot reads it without one.");

    /// <summary>Every program a shot of <paramref name="kind"/> can start.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The kind is undefined.</exception>
    internal static IEnumerable<SamusProjectileProgram> Programs(SamusProjectileKind kind) =>
        DirectionalPrograms.TryGetValue(kind, out SamusProjectileDirectionMap<SamusProjectileProgram>? programs)
            ? programs.Values
            : [SoleProgram(kind)];
}
