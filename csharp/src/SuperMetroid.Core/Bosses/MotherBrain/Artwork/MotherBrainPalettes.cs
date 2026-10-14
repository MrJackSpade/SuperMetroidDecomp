namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

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
