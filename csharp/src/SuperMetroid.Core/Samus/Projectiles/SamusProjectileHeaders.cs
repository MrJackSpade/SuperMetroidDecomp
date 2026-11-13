using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileHeaderDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program each projectile definition starts.</summary>
internal static class SamusProjectileHeaders
{
    /// <summary>The program a shot from the directional <paramref name="header"/> fired in <paramref name="direction"/> starts.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The definition's shots start one program whatever their direction.</exception>
    internal static SamusProjectileProgram Program(SamusProjectileHeader header, SamusProjectileDirection direction) =>
        DirectionalPrograms.TryGetValue(header, out SamusProjectileDirectionMap<SamusProjectileProgram>? programs)
            ? programs.For(direction)
            : throw new ArgumentOutOfRangeException(nameof(header), header,
                $"Projectile header {header} starts one program whatever its direction; no shot reads it by direction.");

    /// <summary>The one program a shot, bomb or particle from <paramref name="header"/> starts, whatever its direction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The definition's shots start a program for each direction.</exception>
    internal static SamusProjectileProgram SoleProgram(SamusProjectileHeader header) =>
        SolePrograms.TryGetValue(header, out SamusProjectileProgram program)
            ? program
            : throw new ArgumentOutOfRangeException(nameof(header), header,
                $"Projectile header {header} starts a program for each direction; no shot reads it without one.");

    /// <summary>Every program a shot from <paramref name="header"/> can start.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The definition is undefined.</exception>
    internal static IEnumerable<SamusProjectileProgram> Programs(SamusProjectileHeader header) =>
        DirectionalPrograms.TryGetValue(header, out SamusProjectileDirectionMap<SamusProjectileProgram>? programs)
            ? programs.Values
            : [SoleProgram(header)];
}
