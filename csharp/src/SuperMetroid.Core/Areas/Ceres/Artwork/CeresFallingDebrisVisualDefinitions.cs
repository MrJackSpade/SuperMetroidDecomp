using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisProgram;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The Ceres falling tiles' installed frames and their projectile definitions' native list identities.</summary>
internal static class CeresFallingDebrisVisualDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_CeresFallingTile_Light</c> at $86:9750: the list the light tile's projectile definition names.</summary>
    internal const ushort LightDefinitionList = 0x9750;
    /// <summary><c>InstList_EnemyProjectile_CeresFallingTile_Dark</c> at $86:9756: the list the dark tile's projectile definition names.</summary>
    internal const ushort DarkDefinitionList = 0x9756;

    /// <summary>The frames in installed order.</summary>
    internal static IReadOnlyList<CeresFallingDebrisProgram> Frames { get; } = [P.Light, P.Dark];

    /// <summary>The installed artwork key of <paramref name="frame"/>: its native operand's address.</summary>
    internal static ushort PresentationKey(CeresFallingDebrisProgram frame) => frame switch
    {
        P.Light => 0x9752,
        P.Dark => 0x9758,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ceres falling tile."),
    };
}
