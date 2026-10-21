using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;

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

    /// <summary>CGADD $90-$9F (OBJ palette 1): Torizo body row nine written by <c>$AA:C2D0</c> and its health gradient.</summary>
    public static readonly PpuLayoutCell TorizoBody = PpuMemoryLayout.Colors(nameof(TorizoBody), 0x90, 16, "Bgr555 OBJ palette");

    /// <summary>CGADD $A0-$AF (OBJ palette 2): Torizo belly row ten.</summary>
    public static readonly PpuLayoutCell TorizoBelly = PpuMemoryLayout.Colors(nameof(TorizoBelly), 0xa0, 16, "Bgr555 OBJ palette");

    /// <summary>CGADD $B0-$BF (OBJ palette 3): the first shared Torizo row (orbs and eye beams).</summary>
    public static readonly PpuLayoutCell TorizoSharedFirst = PpuMemoryLayout.Colors(nameof(TorizoSharedFirst), 0xb0, 16, "Bgr555 OBJ palette");

    /// <summary>CGADD $F0-$FF (OBJ palette 7): the second shared Torizo row.</summary>
    public static readonly PpuLayoutCell TorizoSharedSecond = PpuMemoryLayout.Colors(nameof(TorizoSharedSecond), 0xf0, 16, "Bgr555 OBJ palette");

    /// <summary>CGADD $F0-$FF (OBJ palette 7): Botwoon's health-banded body palette (<c>$B3:9675</c>).</summary>
    public static readonly PpuLayoutCell BotwoonBody = PpuMemoryLayout.Colors(nameof(BotwoonBody),
        BotwoonHealthPaletteDefinitions.DestinationColor, BotwoonHealthPaletteDefinitions.ColorsPerPalette, "Bgr555 OBJ palette");

    /// <summary>CGADD $F1-$FF (OBJ palette 7, colours 1-15): cleared at Ceres Ridley start (target-palette bytes $1E2-$1FE).</summary>
    public static readonly PpuLayoutCell CeresRidleyRevealedObjects = PpuMemoryLayout.Colors(nameof(CeresRidleyRevealedObjects), 0xf1, 15, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $F1-$FF (OBJ palette 7, colours 1-15): the cutscene Baby Metroid palette (target-palette byte $1E2).</summary>
    public static readonly PpuLayoutCell BabyMetroidCutscene = PpuMemoryLayout.Colors(nameof(BabyMetroidCutscene),
        BabyMetroidCutsceneColorRomData.DestinationByteIndex / 2, BabyMetroidCutsceneColorRomData.InitialColorCount, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $60-$6F (BG palette 6): Kraid's room backdrop row, faded during the growth sequence.</summary>
    public static readonly PpuLayoutCell KraidRoomBackdrop = PpuMemoryLayout.Colors(nameof(KraidRoomBackdrop), 0x60, 16, "Bgr555 BG palette");

    /// <summary>CGADD $70-$7F (BG palette 7): Kraid's BG2 body row (health gradient and death arm colours).</summary>
    public static readonly PpuLayoutCell KraidBody = PpuMemoryLayout.Colors(nameof(KraidBody), 0x70, 16, "Bgr555 BG palette");

    /// <summary>CGADD $71-$73 (BG palette 7, colours 1-3): Kraid's eye colours that glow and fade.</summary>
    public static readonly PpuLayoutCell KraidEye = PpuMemoryLayout.Colors(nameof(KraidEye), 0x71, 3, "Bgr555 BG palette colours");

    /// <summary>CGADD $B0-$BF (OBJ palette 3): Kraid's initial target sprite row.</summary>
    public static readonly PpuLayoutCell KraidSprites = PpuMemoryLayout.Colors(nameof(KraidSprites), 0xb0, 16, "Bgr555 OBJ palette");

    /// <summary>CGADD $F0-$FF (OBJ palette 7): Kraid's secondary health-gradient row.</summary>
    public static readonly PpuLayoutCell KraidSecondary = PpuMemoryLayout.Colors(nameof(KraidSecondary), 0xf0, 16, "Bgr555 OBJ palette");

    /// <summary>CGADD $41-$4E (BG palette 4, colours 1-14): Mother Brain's body fade colours.</summary>
    public static readonly PpuLayoutCell MotherBrainBody = PpuMemoryLayout.Colors(nameof(MotherBrainBody),
        MotherBrainDeathRomData.BodyColors, MotherBrainDeathRomData.BodyColorCount, "Bgr555 BG palette colours");

    /// <summary>CGADD $91-$9F (OBJ palette 1, colours 1-15): the brain colours <c>$A9:AF77</c> copies to the corpse palette and the exploded-door colours overwrite.</summary>
    public static readonly PpuLayoutCell MotherBrainBrain = PpuMemoryLayout.Colors(nameof(MotherBrainBrain),
        MotherBrainDeathRomData.BrainColors, MotherBrainDeathRomData.CorpseColorCount, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $B1-$BE (OBJ palette 3, colours 1-14): Mother Brain's leg colours.</summary>
    public static readonly PpuLayoutCell MotherBrainLegs = PpuMemoryLayout.Colors(nameof(MotherBrainLegs),
        MotherBrainDeathRomData.LegColors, MotherBrainDeathRomData.BodyColorCount, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $F1-$FF (OBJ palette 7, colours 1-15): Mother Brain's corpse colours.</summary>
    public static readonly PpuLayoutCell MotherBrainCorpse = PpuMemoryLayout.Colors(nameof(MotherBrainCorpse),
        MotherBrainDeathRomData.CorpseColors, MotherBrainDeathRomData.CorpseColorCount, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $70-$7F (BG palette 7): Phantoon's body palette, health-banded and faded.</summary>
    public static readonly PpuLayoutCell PhantoonBody = PpuMemoryLayout.Colors(nameof(PhantoonBody),
        PhantoonColorRomData.BodyDestination, PhantoonColorRomData.HealthBandColorCount, "Bgr555 BG palette");

    /// <summary>CGADD $71-$7F (BG palette 7, colours 1-15): Norfair Ridley's body colours cleared for the reveal.</summary>
    public static readonly PpuLayoutCell NorfairRidleyBody = PpuMemoryLayout.Colors(nameof(NorfairRidleyBody), 0x71, 15, "Bgr555 BG palette colours");

    /// <summary>CGADD $F1-$FF (OBJ palette 7, colours 1-15): Norfair Ridley's tail and wing colours cleared for the reveal.</summary>
    public static readonly PpuLayoutCell NorfairRidleyTail = PpuMemoryLayout.Colors(nameof(NorfairRidleyTail), 0xf1, 15, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $90-$9F (OBJ palette 1): the Shitroid encounter's sidehopper target row.</summary>
    public static readonly PpuLayoutCell ShitroidSidehopper = PpuMemoryLayout.Colors(nameof(ShitroidSidehopper),
        ShitroidColorRomData.SidehopperDestination, ShitroidColorRomData.TargetColorCount, "Bgr555 OBJ palette");

    /// <summary>CGADD $A0-$AF (OBJ palette 2): the Shitroid's target row.</summary>
    public static readonly PpuLayoutCell ShitroidBody = PpuMemoryLayout.Colors(nameof(ShitroidBody),
        ShitroidColorRomData.ShitroidDestination, ShitroidColorRomData.TargetColorCount, "Bgr555 OBJ palette");

    /// <summary>CGADD $F0-$FF (OBJ palette 7): the drained sidehopper's target row.</summary>
    public static readonly PpuLayoutCell ShitroidDeadSidehopper = PpuMemoryLayout.Colors(nameof(ShitroidDeadSidehopper),
        ShitroidColorRomData.DeadSidehopperDestination, ShitroidColorRomData.TargetColorCount, "Bgr555 OBJ palette");

    /// <summary>CGADD $A5-$A8 (OBJ palette 2, colours 5-8): the Shitroid's cycling membrane colours.</summary>
    public static readonly PpuLayoutCell ShitroidMembrane = PpuMemoryLayout.Colors(nameof(ShitroidMembrane), 0xa5,
        ShitroidColorRomData.NormalColorsPerFrame, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $F0-$FF (OBJ palette 7): Spore Spawn's independent spore palette.</summary>
    public static readonly PpuLayoutCell SporeSpawnSpores = PpuMemoryLayout.Colors(nameof(SporeSpawnSpores),
        SporeSpawnColorRomData.SporeDestination, SporeSpawnColorRomData.ColorsPerFrame, "Bgr555 OBJ palette");

    /// <summary>CGADD $90-$9F (OBJ palette 1): Spore Spawn's body palette.</summary>
    public static readonly PpuLayoutCell SporeSpawnBody = PpuMemoryLayout.Colors(nameof(SporeSpawnBody),
        SporeSpawnColorRomData.SpriteDestination, SporeSpawnColorRomData.ColorsPerFrame, "Bgr555 OBJ palette");

    /// <summary>CGADD $40-$4F (BG palette 4): the level row Spore Spawn's death sequence fades.</summary>
    public static readonly PpuLayoutCell SporeSpawnDeathLevel = PpuMemoryLayout.Colors(nameof(SporeSpawnDeathLevel),
        SporeSpawnColorRomData.LevelDestination, SporeSpawnColorRomData.ColorsPerFrame, "Bgr555 BG palette");

    /// <summary>CGADD $70-$7F (BG palette 7): the background row Spore Spawn's death sequence fades.</summary>
    public static readonly PpuLayoutCell SporeSpawnDeathBackground = PpuMemoryLayout.Colors(nameof(SporeSpawnDeathBackground),
        SporeSpawnColorRomData.BackgroundDestination, SporeSpawnColorRomData.ColorsPerFrame, "Bgr555 BG palette");
}
