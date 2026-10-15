using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Fixed VRAM destinations of the installed projectile trail characters (#630).</summary>
[PpuLayoutBoundary("catalog: projectile trail VRAM destinations")]
public static class ProjectileTrailVramLayout
{
    /// <summary>VMADD $6380-$63FF: the eight ice/wave trail characters inside the standard OBJ table (bytes $C700).</summary>
    public static readonly PpuLayoutCell IceWaveTrail = new(nameof(IceWaveTrail), PpuLayoutRegion.Vram,
        ProjectileTrailAtlasDefinitions.IceWaveDestinationWord, 0x80,
        "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6480-$64BF: the four missile trail characters inside the standard OBJ table (bytes $C900).</summary>
    public static readonly PpuLayoutCell MissileTrail = new(nameof(MissileTrail), PpuLayoutRegion.Vram,
        ProjectileTrailAtlasDefinitions.MissileDestinationWord, 0x40,
        "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);
}