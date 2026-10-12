using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>CGRAM ranges written by the Ceres and Norfair Ridley palette routines (#630).</summary>
[PpuLayoutBoundary("catalog: Ridley palettes")]
public static class RidleyCgramLayout
{
    /// <summary>CGADD $A0-$BF (native byte offset $0140): the door/container and initial Baby OBJ palettes 2-3 from $A6:E16F.</summary>
    public static readonly PpuLayoutCell CeresStart = PpuMemoryLayout.Colors(nameof(CeresStart),
        CeresRidleyPaletteRomData.StartCgramIndex, CeresRidleyPaletteRomData.StartColorCount, "Bgr555 OBJ palettes 2-3");

    /// <summary>CGADD $FC-$FE: Ceres Ridley's three eye colours, OBJ palette 7 colours $C-$E ($A6:E2AA rows).</summary>
    public static readonly PpuLayoutCell CeresEyeFade = PpuMemoryLayout.Colors(nameof(CeresEyeFade),
        CeresRidleyPaletteRomData.EyeFadeCgramIndex, CeresRidleyPaletteRomData.EyeFadeColorCount, "Bgr555 eye shade");

    /// <summary>CGADD $91-$9B (byte offset $0122): OBJ palette 1 colours 1-$B receiving the $A6:E30A body fade.</summary>
    public static readonly PpuLayoutCell CeresBodyFadeShadow = PpuMemoryLayout.Colors(nameof(CeresBodyFadeShadow),
        CeresRidleyPaletteRomData.BodyFadeBgCgramIndex, CeresRidleyPaletteRomData.BodyFadeColorCount, "Bgr555 body fade");

    /// <summary>CGADD $F1-$FB (byte offset $01E2): OBJ palette 7 colours 1-$B receiving the $A6:E30A body fade.</summary>
    public static readonly PpuLayoutCell CeresBodyFade = PpuMemoryLayout.Colors(nameof(CeresBodyFade),
        CeresRidleyPaletteRomData.BodyFadeObjCgramIndex, CeresRidleyPaletteRomData.BodyFadeColorCount, "Bgr555 body fade");

    /// <summary>CGADD $F1-$FE (byte offset $01E2): OBJ palette 7 colours 1-$E, the shared $A6:E46A health shade.</summary>
    public static readonly PpuLayoutCell Health = PpuMemoryLayout.Colors(nameof(Health),
        CeresRidleyPaletteRomData.HealthCgramIndex, CeresRidleyPaletteRomData.HealthColorCount, "Bgr555 health shade");

    /// <summary>CGADD $61-$63 (byte offset $00C2): BG palette 6 colours 1-3, the $A6:C1DF EMERGENCY-text cycle.</summary>
    public static readonly PpuLayoutCell CeresAlarm = PpuMemoryLayout.Colors(nameof(CeresAlarm),
        CeresRidleyPaletteRomData.AlarmCgramIndex, CeresRidleyPaletteRomData.AlarmColorCount, "Bgr555 alarm text");

    /// <summary>CGADD $51-$5F (byte offset $00A2): BG palette 5 colours 1-$F, the $A6:A9E3 getaway backdrop.</summary>
    public static readonly PpuLayoutCell CeresRetreatBackground = PpuMemoryLayout.Colors(nameof(CeresRetreatBackground),
        CeresRidleyPaletteRomData.RetreatBgCgramIndex, CeresRidleyPaletteRomData.RetreatBgColorCount, "Bgr555 getaway backdrop");

    /// <summary>CGADD $21-$28 (byte offset $0042): BG palette 2 colours 1-8 receiving the $A6:AA01 shared retreat shade.</summary>
    public static readonly PpuLayoutCell CeresRetreatSharedBackground = PpuMemoryLayout.Colors(nameof(CeresRetreatSharedBackground),
        CeresRidleyPaletteRomData.RetreatSharedBgCgramIndex, CeresRidleyPaletteRomData.RetreatSharedColorCount, "Bgr555 retreat shade");

    /// <summary>CGADD $F1-$F8 (byte offset $01E2): OBJ palette 7 colours 1-8 receiving the $A6:AA01 shared retreat shade.</summary>
    public static readonly PpuLayoutCell CeresRetreatSharedObject = PpuMemoryLayout.Colors(nameof(CeresRetreatSharedObject),
        CeresRidleyPaletteRomData.RetreatSharedObjCgramIndex, CeresRidleyPaletteRomData.RetreatSharedColorCount, "Bgr555 retreat shade");

    /// <summary>CGADD $B1-$BF (byte offset $0162): OBJ palette 3 colours 1-$F, the Ceres Baby shade written by $A6:BFE1.</summary>
    public static readonly PpuLayoutCell CeresBaby = PpuMemoryLayout.Colors(nameof(CeresBaby),
        CeresRidleyPaletteRomData.BabyCgramIndex, CeresRidleyPaletteRomData.BabyColorCount, "Bgr555 Baby shade");

    /// <summary>CGADD $51-$5F (byte offset $00A2): BG palette 5 colours 1-$F, the Mode 7 zoom shade written by $A6:B0EF.</summary>
    public static readonly PpuLayoutCell CeresMode7Zoom = PpuMemoryLayout.Colors(nameof(CeresMode7Zoom),
        CeresRidleyPaletteRomData.Mode7ZoomCgramIndex, CeresRidleyPaletteRomData.Mode7ZoomColorCount, "Bgr555 zoom shade");

    /// <summary>CGADD $A0-$BF (byte offset $0140): Norfair Ridley's initial OBJ palettes 2-3.</summary>
    public static readonly PpuLayoutCell NorfairInitial = PpuMemoryLayout.Colors(nameof(NorfairInitial),
        NorfairRidleyPaletteRomData.InitialCgramIndex, NorfairRidleyPaletteRomData.InitialColorCount, "Bgr555 OBJ palettes 2-3");

    /// <summary>CGADD $71-$7E (byte offset $00E2): BG palette 7 colours 1-$E, the Norfair reveal fade.</summary>
    public static readonly PpuLayoutCell NorfairReveal = PpuMemoryLayout.Colors(nameof(NorfairReveal),
        NorfairRidleyPaletteRomData.RevealCgramIndex, NorfairRidleyPaletteRomData.RevealColorCount, "Bgr555 reveal fade");
}

/// <summary>Typed view of the Ridley palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: Ridley palettes")]
public readonly struct RidleyPalettes(SnesCgram cgram)
{
    /// <summary>Ceres door/container and Baby palettes.</summary>
    public CgramPalette CeresStart => new(cgram, RidleyCgramLayout.CeresStart);
    /// <summary>Ceres eye colours.</summary>
    public CgramPalette CeresEyeFade => new(cgram, RidleyCgramLayout.CeresEyeFade);
    /// <summary>Ceres body fade, OBJ palette 1 copy.</summary>
    public CgramPalette CeresBodyFadeShadow => new(cgram, RidleyCgramLayout.CeresBodyFadeShadow);
    /// <summary>Ceres body fade, OBJ palette 7.</summary>
    public CgramPalette CeresBodyFade => new(cgram, RidleyCgramLayout.CeresBodyFade);
    /// <summary>Shared health shade.</summary>
    public CgramPalette Health => new(cgram, RidleyCgramLayout.Health);
    /// <summary>Ceres alarm text.</summary>
    public CgramPalette CeresAlarm => new(cgram, RidleyCgramLayout.CeresAlarm);
    /// <summary>Ceres getaway backdrop.</summary>
    public CgramPalette CeresRetreatBackground => new(cgram, RidleyCgramLayout.CeresRetreatBackground);
    /// <summary>Ceres shared retreat shade, BG copy.</summary>
    public CgramPalette CeresRetreatSharedBackground => new(cgram, RidleyCgramLayout.CeresRetreatSharedBackground);
    /// <summary>Ceres shared retreat shade, OBJ copy.</summary>
    public CgramPalette CeresRetreatSharedObject => new(cgram, RidleyCgramLayout.CeresRetreatSharedObject);
    /// <summary>Ceres Baby shade.</summary>
    public CgramPalette CeresBaby => new(cgram, RidleyCgramLayout.CeresBaby);
    /// <summary>Ceres Mode 7 zoom shade.</summary>
    public CgramPalette CeresMode7Zoom => new(cgram, RidleyCgramLayout.CeresMode7Zoom);
    /// <summary>Norfair initial palettes.</summary>
    public CgramPalette NorfairInitial => new(cgram, RidleyCgramLayout.NorfairInitial);
    /// <summary>Norfair reveal fade.</summary>
    public CgramPalette NorfairReveal => new(cgram, RidleyCgramLayout.NorfairReveal);
}

/// <summary>CGRAM ranges written by Mother Brain's body, brain, leg and room palette routines (#630).</summary>
[PpuLayoutBoundary("catalog: Mother Brain palettes")]
public static class MotherBrainCgramLayout
{
    /// <summary>CGADD $41-$4F: BG palette 4 colours 1-$F, Mother Brain's body.</summary>
    public static readonly PpuLayoutCell Body = PpuMemoryLayout.Colors(nameof(Body),
        MotherBrainRainbowPaletteRomData.BodyColor, MotherBrainRainbowPaletteRomData.ColorCount, "Bgr555 body");

    /// <summary>CGADD $91-$9F: OBJ palette 1 colours 1-$F, the brain, which mirrors the body colours.</summary>
    public static readonly PpuLayoutCell Brain = PpuMemoryLayout.Colors(nameof(Brain),
        MotherBrainRainbowPaletteRomData.BrainColor, MotherBrainRainbowPaletteRomData.ColorCount, "Bgr555 body mirror");

    /// <summary>CGADD $B1-$BF: OBJ palette 3 colours 1-$F, the rear legs.</summary>
    public static readonly PpuLayoutCell RearLegs = PpuMemoryLayout.Colors(nameof(RearLegs),
        MotherBrainRainbowPaletteRomData.SecondaryColor, MotherBrainRainbowPaletteRomData.ColorCount, "Bgr555 rear legs");

    /// <summary>CGADD $B4-$B8: the five rear-leg colours the drain and revival fades replace.</summary>
    public static readonly PpuLayoutCell DrainedRearLegs = PpuMemoryLayout.Colors(nameof(DrainedRearLegs),
        MotherBrainDrainedPaletteRomData.BackLegColor, MotherBrainDrainedPaletteRomData.BackLegCount, "Bgr555 rear-leg fade");

    /// <summary>CGADD $91-$93 (byte offset $0122): the three brain colours of the fake-death fade.</summary>
    public static readonly PpuLayoutCell FakeDeathBrain = PpuMemoryLayout.Colors(nameof(FakeDeathBrain),
        MotherBrainFakeDeathPaletteRomData.BrainColor, MotherBrainFakeDeathPaletteRomData.ColorCount, "Bgr555 brain fade");

    /// <summary>CGADD $34-$3F (byte offset $0068): the first room-flash slice in BG palette 3.</summary>
    public static readonly PpuLayoutCell RoomFirst = PpuMemoryLayout.Colors(nameof(RoomFirst),
        MotherBrainRoomColorRomData.FirstColor, MotherBrainRoomColorRomData.SliceColors, "Bgr555 room flash");

    /// <summary>CGADD $53-$5E (byte offset $00A6): the second room-flash slice in BG palette 5.</summary>
    public static readonly PpuLayoutCell RoomSecond = PpuMemoryLayout.Colors(nameof(RoomSecond),
        MotherBrainRoomColorRomData.SecondColor, MotherBrainRoomColorRomData.SliceColors, "Bgr555 room flash");

    /// <summary>CGADD $73-$7E (byte offset $00E6): BG palette 7's mirror of the second room slice.</summary>
    public static readonly PpuLayoutCell RoomSecondMirror = PpuMemoryLayout.Colors(nameof(RoomSecondMirror),
        MotherBrainRoomColorRomData.MirroredSecondColor, MotherBrainRoomColorRomData.SliceColors, "Bgr555 room flash");

    /// <summary>CGADD $A1-$AF (byte offset $0142): OBJ palette 2 colours 1-$F, the phase-two attack sprites.</summary>
    public static readonly PpuLayoutCell PhaseTwoAttack = PpuMemoryLayout.Colors(nameof(PhaseTwoAttack),
        MotherBrainRoomColorRomData.PhaseTwoAttackColor, MotherBrainRoomColorRomData.PhaseTwoColors, "Bgr555 attack sprites");

    /// <summary>CGADD $B1-$BF (byte offset $0162): OBJ palette 3 colours 1-$F, the phase-two rear legs.</summary>
    public static readonly PpuLayoutCell PhaseTwoRearLegs = PpuMemoryLayout.Colors(nameof(PhaseTwoRearLegs),
        MotherBrainRoomColorRomData.PhaseTwoRearLegColor, MotherBrainRoomColorRomData.PhaseTwoColors, "Bgr555 rear legs");

    /// <summary>CGADD $B1-$BF (byte offset $0162): OBJ palette 3 colours 1-$F, the room-entry glass shards.</summary>
    public static readonly PpuLayoutCell GlassShards = PpuMemoryLayout.Colors(nameof(GlassShards),
        MotherBrainRoomColorRomData.InitialGlassShardColor, MotherBrainRoomColorRomData.InitialColors, "Bgr555 glass shards");

    /// <summary>CGADD $F1-$FF (byte offset $01E2): OBJ palette 7 colours 1-$F, the tube projectiles.</summary>
    public static readonly PpuLayoutCell TubeProjectiles = PpuMemoryLayout.Colors(nameof(TubeProjectiles),
        MotherBrainRoomColorRomData.InitialTubeProjectileColor, MotherBrainRoomColorRomData.InitialColors, "Bgr555 tube projectiles");

    /// <summary>CGADD $31-$3E (byte offset $0062): BG palette 3 colours 1-$E, the first room-light slice ($AD:F209/$AD:F24B).</summary>
    public static readonly PpuLayoutCell RecoveryLightsFirst = PpuMemoryLayout.Colors(nameof(RecoveryLightsFirst),
        MotherBrainRoomColorRomData.RecoveryLightsFirstColor, MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination, "Bgr555 room lights");

    /// <summary>CGADD $51-$5E (byte offset $00A2): BG palette 5 colours 1-$E, the second room-light slice.</summary>
    public static readonly PpuLayoutCell RecoveryLightsSecond = PpuMemoryLayout.Colors(nameof(RecoveryLightsSecond),
        MotherBrainRoomColorRomData.RecoveryLightsSecondColor, MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination, "Bgr555 room lights");
}

/// <summary>Typed view of Mother Brain's palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: Mother Brain palettes")]
public readonly struct MotherBrainPalettes(SnesCgram cgram)
{
    /// <summary>Body colours.</summary>
    public CgramPalette Body => new(cgram, MotherBrainCgramLayout.Body);
    /// <summary>Brain colours.</summary>
    public CgramPalette Brain => new(cgram, MotherBrainCgramLayout.Brain);
    /// <summary>Rear-leg colours.</summary>
    public CgramPalette RearLegs => new(cgram, MotherBrainCgramLayout.RearLegs);
    /// <summary>The drain-faded rear-leg colours.</summary>
    public CgramPalette DrainedRearLegs => new(cgram, MotherBrainCgramLayout.DrainedRearLegs);
    /// <summary>The fake-death brain colours.</summary>
    public CgramPalette FakeDeathBrain => new(cgram, MotherBrainCgramLayout.FakeDeathBrain);
    /// <summary>First room-flash slice.</summary>
    public CgramPalette RoomFirst => new(cgram, MotherBrainCgramLayout.RoomFirst);
    /// <summary>Second room-flash slice.</summary>
    public CgramPalette RoomSecond => new(cgram, MotherBrainCgramLayout.RoomSecond);
    /// <summary>Mirror of the second room-flash slice.</summary>
    public CgramPalette RoomSecondMirror => new(cgram, MotherBrainCgramLayout.RoomSecondMirror);
    /// <summary>Phase-two attack sprites.</summary>
    public CgramPalette PhaseTwoAttack => new(cgram, MotherBrainCgramLayout.PhaseTwoAttack);
    /// <summary>Phase-two rear legs.</summary>
    public CgramPalette PhaseTwoRearLegs => new(cgram, MotherBrainCgramLayout.PhaseTwoRearLegs);
    /// <summary>Room-entry glass shards.</summary>
    public CgramPalette GlassShards => new(cgram, MotherBrainCgramLayout.GlassShards);
    /// <summary>Room-entry tube projectiles.</summary>
    public CgramPalette TubeProjectiles => new(cgram, MotherBrainCgramLayout.TubeProjectiles);
    /// <summary>First room-light slice.</summary>
    public CgramPalette RecoveryLightsFirst => new(cgram, MotherBrainCgramLayout.RecoveryLightsFirst);
    /// <summary>Second room-light slice.</summary>
    public CgramPalette RecoveryLightsSecond => new(cgram, MotherBrainCgramLayout.RecoveryLightsSecond);
}

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
