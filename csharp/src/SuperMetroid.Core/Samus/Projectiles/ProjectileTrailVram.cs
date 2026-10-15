namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Typed view of the installed projectile trail characters in VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: projectile trail VRAM")]
public readonly struct ProjectileTrailVram(SnesVram vram)
{
    /// <summary>Ice/wave trail characters.</summary>
    public VramRegion IceWaveTrail => new(vram, ProjectileTrailVramLayout.IceWaveTrail);

    /// <summary>Missile trail characters.</summary>
    public VramRegion MissileTrail => new(vram, ProjectileTrailVramLayout.MissileTrail);

    /// <summary>Queued-DMA destination of the ice/wave trail characters.</summary>
    public static VramDmaDestination IceWaveTrailDma => VramDmaDestination.At(ProjectileTrailVramLayout.IceWaveTrail);

    /// <summary>Queued-DMA destination of the missile trail characters.</summary>
    public static VramDmaDestination MissileTrailDma => VramDmaDestination.At(ProjectileTrailVramLayout.MissileTrail);
}