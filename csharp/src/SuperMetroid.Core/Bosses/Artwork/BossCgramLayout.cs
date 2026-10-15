using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>CGRAM ranges written by the other boss and set-piece palette routines (#630).</summary>
[PpuLayoutBoundary("catalog: boss and set-piece palettes")]
public static class BossCgramLayout
{
    /// <summary>CGADD $90-$A8: Draygon's 25 opening colours across OBJ palettes 1-2.</summary>
    public static readonly PpuLayoutCell DraygonIntro = PpuMemoryLayout.Colors(nameof(DraygonIntro),
        DraygonColorRomData.IntroDestination, DraygonColorRomData.IntroCount, "Bgr555 opening colours");

    /// <summary>CGADD $50-$5F: BG palette 5, Draygon's background material colours.</summary>
    public static readonly PpuLayoutCell DraygonBackground = PpuMemoryLayout.Colors(nameof(DraygonBackground),
        DraygonColorRomData.BackgroundDestination, DraygonColorRomData.BackgroundCount, "Bgr555 BG palette 5");

    /// <summary>CGADD $F0-$FF: OBJ palette 7, Draygon's sprite material colours.</summary>
    public static readonly PpuLayoutCell DraygonSprite = PpuMemoryLayout.Colors(nameof(DraygonSprite),
        DraygonColorRomData.SpriteDestination, DraygonColorRomData.SpriteCount, "Bgr555 OBJ palette 7");

    /// <summary>CGADD $59-$5C: Draygon's four health-band colours.</summary>
    public static readonly PpuLayoutCell DraygonHealth = PpuMemoryLayout.Colors(nameof(DraygonHealth),
        DraygonColorRomData.HealthDestination, DraygonColorRomData.HealthBandColorCount, "Bgr555 health band");

    /// <summary>CGADD $70-$77: Crocomire's fight body/head BG inks ($A4:B89D).</summary>
    public static readonly PpuLayoutCell CrocomireFightBody = PpuMemoryLayout.Colors(nameof(CrocomireFightBody),
        CrocomirePaletteRomData.FightBodyDestination, CrocomirePaletteRomData.FightBodyCount, "Bgr555 body inks");

    /// <summary>CGADD $A0-$B0: Crocomire's seventeen initial wall words.</summary>
    public static readonly PpuLayoutCell CrocomireInitialWall = PpuMemoryLayout.Colors(nameof(CrocomireInitialWall),
        CrocomirePaletteRomData.InitialWallDestination, CrocomirePaletteRomData.InitialWallCount, "Bgr555 wall inks");

    /// <summary>CGADD $D0-$E0: Crocomire's seventeen initial projectile words.</summary>
    public static readonly PpuLayoutCell CrocomireInitialProjectile = PpuMemoryLayout.Colors(nameof(CrocomireInitialProjectile),
        CrocomirePaletteRomData.InitialProjectileDestination, CrocomirePaletteRomData.InitialProjectileCount, "Bgr555 projectile inks");

    /// <summary>CGADD $90-$9F: Crocomire's skeleton-arm OBJ palette ($A4:B8FD).</summary>
    public static readonly PpuLayoutCell CrocomireSkeletonArm = PpuMemoryLayout.Colors(nameof(CrocomireSkeletonArm),
        CrocomirePaletteRomData.SkeletonArmDestination, CrocomirePaletteRomData.SkeletonArmCount, "Bgr555 OBJ palette 1");

    /// <summary>CGADD $B0-$BF: Crocomire's wall-spike OBJ palette ($A4:B91D).</summary>
    public static readonly PpuLayoutCell CrocomireWallSpikes = PpuMemoryLayout.Colors(nameof(CrocomireWallSpikes),
        CrocomirePaletteRomData.WallSpikesDestination, CrocomirePaletteRomData.WallSpikesCount, "Bgr555 OBJ palette 3");

    /// <summary>CGADD $90-$AF: the Chozo statue / glass tube OBJ palettes 1-2 ($AA:E716).</summary>
    public static readonly PpuLayoutCell ChozoAndTube = PpuMemoryLayout.Colors(nameof(ChozoAndTube),
        ChozoAndTubeColorRomData.Destination, ChozoAndTubeColorRomData.ColorCount, "Bgr555 OBJ palettes 1-2");

    /// <summary>CGADD $F0-$FF: the Tourian statue base OBJ palette 7.</summary>
    public static readonly PpuLayoutCell TourianStatueBase = PpuMemoryLayout.Colors(nameof(TourianStatueBase),
        TourianStatuePaletteRomData.BaseCgramIndex, TourianStatuePaletteRomData.BaseColorCount, "Bgr555 OBJ palette 7");

    /// <summary>CGADD $A0-$AF: the Tourian statue OBJ palette 2.</summary>
    public static readonly PpuLayoutCell TourianStatue = PpuMemoryLayout.Colors(nameof(TourianStatue),
        TourianStatuePaletteRomData.StatueCgramIndex, TourianStatuePaletteRomData.StatueColorCount, "Bgr555 OBJ palette 2");

    /// <summary>CGADD $F9-$FC: the Tourian statue eye glow.</summary>
    public static readonly PpuLayoutCell TourianStatueEye = PpuMemoryLayout.Colors(nameof(TourianStatueEye),
        TourianStatuePaletteRomData.EyeCgramIndex, TourianStatuePaletteRomData.EyeColorCount, "Bgr555 eye glow");

    /// <summary>CGADD $29-$2E (byte offset $0052): the Ceres door animation colours.</summary>
    public static readonly PpuLayoutCell CeresDoorAnimation = PpuMemoryLayout.Colors(nameof(CeresDoorAnimation),
        CeresDoorVisualRomData.AnimationTargetColor, CeresDoorVisualRomData.AnimationColorCount, "Bgr555 door animation");
}
