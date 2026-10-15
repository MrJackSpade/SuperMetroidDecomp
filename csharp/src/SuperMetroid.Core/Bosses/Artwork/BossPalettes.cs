namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>Typed view of the other boss and set-piece palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: boss and set-piece palettes")]
public readonly struct BossPalettes(SnesCgram cgram)
{
    /// <summary>Draygon opening colours.</summary>
    public CgramPalette DraygonIntro => new(cgram, BossCgramLayout.DraygonIntro);
    /// <summary>Draygon background material.</summary>
    public CgramPalette DraygonBackground => new(cgram, BossCgramLayout.DraygonBackground);
    /// <summary>Draygon sprite material.</summary>
    public CgramPalette DraygonSprite => new(cgram, BossCgramLayout.DraygonSprite);
    /// <summary>Draygon health band.</summary>
    public CgramPalette DraygonHealth => new(cgram, BossCgramLayout.DraygonHealth);
    /// <summary>Crocomire fight body.</summary>
    public CgramPalette CrocomireFightBody => new(cgram, BossCgramLayout.CrocomireFightBody);
    /// <summary>Crocomire initial wall.</summary>
    public CgramPalette CrocomireInitialWall => new(cgram, BossCgramLayout.CrocomireInitialWall);
    /// <summary>Crocomire initial projectile.</summary>
    public CgramPalette CrocomireInitialProjectile => new(cgram, BossCgramLayout.CrocomireInitialProjectile);
    /// <summary>Crocomire skeleton arm.</summary>
    public CgramPalette CrocomireSkeletonArm => new(cgram, BossCgramLayout.CrocomireSkeletonArm);
    /// <summary>Crocomire wall spikes.</summary>
    public CgramPalette CrocomireWallSpikes => new(cgram, BossCgramLayout.CrocomireWallSpikes);
    /// <summary>Chozo statue / glass tube palettes.</summary>
    public CgramPalette ChozoAndTube => new(cgram, BossCgramLayout.ChozoAndTube);
    /// <summary>Tourian statue base.</summary>
    public CgramPalette TourianStatueBase => new(cgram, BossCgramLayout.TourianStatueBase);
    /// <summary>Tourian statue.</summary>
    public CgramPalette TourianStatue => new(cgram, BossCgramLayout.TourianStatue);
    /// <summary>Tourian statue eye.</summary>
    public CgramPalette TourianStatueEye => new(cgram, BossCgramLayout.TourianStatueEye);
    /// <summary>Ceres door animation.</summary>
    public CgramPalette CeresDoorAnimation => new(cgram, BossCgramLayout.CeresDoorAnimation);
}
