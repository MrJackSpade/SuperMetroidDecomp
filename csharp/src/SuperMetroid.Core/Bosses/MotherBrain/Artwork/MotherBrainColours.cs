using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Typed view of Mother Brain's colours (#630, #1405): runs of gameplay's background and sprite colour sets.</summary>
public readonly struct MotherBrainColours(GameplayColours colours)
{
    /// <summary>Body colours.</summary>
    public ColourRun Body => MotherBrainColourDefinitions.Body.In(colours.BackgroundSets);
    /// <summary>Brain colours.</summary>
    public ColourRun Brain => MotherBrainColourDefinitions.Brain.In(colours.SpriteSets);
    /// <summary>Rear-leg colours.</summary>
    public ColourRun RearLegs => MotherBrainColourDefinitions.RearLegs.In(colours.SpriteSets);
    /// <summary>The drain-faded rear-leg colours.</summary>
    public ColourRun DrainedRearLegs => MotherBrainColourDefinitions.DrainedRearLegs.In(colours.SpriteSets);
    /// <summary>The fake-death brain colours.</summary>
    public ColourRun FakeDeathBrain => MotherBrainColourDefinitions.FakeDeathBrain.In(colours.SpriteSets);
    /// <summary>First room-flash slice.</summary>
    public ColourRun RoomFirst => MotherBrainColourDefinitions.RoomFirst.In(colours.BackgroundSets);
    /// <summary>Second room-flash slice.</summary>
    public ColourRun RoomSecond => MotherBrainColourDefinitions.RoomSecond.In(colours.BackgroundSets);
    /// <summary>Mirror of the second room-flash slice.</summary>
    public ColourRun RoomSecondMirror => MotherBrainColourDefinitions.RoomSecondMirror.In(colours.BackgroundSets);
    /// <summary>Phase-two attack sprites.</summary>
    public ColourRun PhaseTwoAttack => MotherBrainColourDefinitions.PhaseTwoAttack.In(colours.SpriteSets);
    /// <summary>Phase-two rear legs.</summary>
    public ColourRun PhaseTwoRearLegs => MotherBrainColourDefinitions.PhaseTwoRearLegs.In(colours.SpriteSets);
    /// <summary>Room-entry glass shards.</summary>
    public ColourRun GlassShards => MotherBrainColourDefinitions.GlassShards.In(colours.SpriteSets);
    /// <summary>Room-entry tube projectiles.</summary>
    public ColourRun TubeProjectiles => MotherBrainColourDefinitions.TubeProjectiles.In(colours.SpriteSets);
    /// <summary>First room-light slice.</summary>
    public ColourRun RecoveryLightsFirst => MotherBrainColourDefinitions.RecoveryLightsFirst.In(colours.BackgroundSets);
    /// <summary>Second room-light slice.</summary>
    public ColourRun RecoveryLightsSecond => MotherBrainColourDefinitions.RecoveryLightsSecond.In(colours.BackgroundSets);
}
