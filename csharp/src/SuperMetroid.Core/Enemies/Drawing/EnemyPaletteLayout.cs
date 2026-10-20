using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// CGRAM ranges that bosses and enemy actors write directly (#630). Each cell is the colour
/// range named by the native routine; ranges shared between actors alias the hardware palette
/// row that holds them.
/// </summary>
[PpuLayoutBoundary("catalog: enemy and boss CGRAM ranges")]
public static class EnemyPaletteLayout
{
    /// <summary>CGADD $90-$9F (OBJ palette 1): Torizo body row nine written by <c>$AA:C2D0</c> and its health gradient.</summary>
    public static readonly PpuLayoutCell TorizoBody = PpuMemoryLayout.Colors(nameof(TorizoBody), 0x90, 16, "Bgr555 OBJ palette");

    /// <summary>CGADD $F0-$FF (OBJ palette 7): the gold Space Pirate colours the ninja Pirate copies on spawn.</summary>
    public static readonly PpuLayoutCell NinjaSpacePirate = PpuMemoryLayout.Colors(nameof(NinjaSpacePirate),
        NinjaSpacePiratePaletteDefinitions.TargetColor, PpuMemoryLayout.ColorsPerRow, "Bgr555 OBJ palette");

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

    /// <summary>CGADD $91-$9F (OBJ palette 1, colours 1-15): the dead sidehopper's transformation palette.</summary>
    public static readonly PpuLayoutCell DeadSidehopper = PpuMemoryLayout.Colors(nameof(DeadSidehopper), 0x91, 15, "Bgr555 OBJ palette colours");

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

    /// <summary>CGADD $7D-$7E (BG palette 7, colours 13-14): the escape typewriter's source text colours.</summary>
    public static readonly PpuLayoutCell EscapeTypewriterSource = PpuMemoryLayout.Colors(nameof(EscapeTypewriterSource),
        EscapeTypewriterRomData.ColorSource, 2, "Bgr555 BG palette colours");

    /// <summary>CGADD $9D-$9E (OBJ palette 1, colours 13-14): where the escape typewriter copies its text colours.</summary>
    public static readonly PpuLayoutCell EscapeTypewriterText = PpuMemoryLayout.Colors(nameof(EscapeTypewriterText),
        EscapeTypewriterRomData.ColorDestination, 2, "Bgr555 OBJ palette colours");

    /// <summary>CGADD $70-$7F (BG palette 7): Phantoon's body palette, health-banded and faded.</summary>
    public static readonly PpuLayoutCell PhantoonBody = PpuMemoryLayout.Colors(nameof(PhantoonBody),
        PhantoonColorRomData.BodyDestination, PhantoonColorRomData.HealthBandColorCount, "Bgr555 BG palette");

    /// <summary>CGADD $00-$6F (BG palettes 0-6): the Wrecked Ship room colours faded in at power-on.</summary>
    public static readonly PpuLayoutCell WreckedShipPowerOn = PpuMemoryLayout.Colors(nameof(WreckedShipPowerOn), 0x00, 0x70, "Bgr555 BG palettes");

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
