
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

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
