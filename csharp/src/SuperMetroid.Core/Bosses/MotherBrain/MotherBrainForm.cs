namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Which stage of the encounter Mother Brain is in. Shot reactions, bomb reactions and the footstep sound branch on
/// it; the encounter only ever moves forward through these stages, apart from the corpse interval that returns to
/// <see cref="SecondPhase"/>. The backing values are the snapshot word the verification import decodes.
/// </summary>
public enum MotherBrainForm
{
    /// <summary>The brain is still inside its glass tank: only missiles and Super Missiles reach it, and bombs do nothing.</summary>
    Glass = 0,

    /// <summary>The fake death after the glass phase: shots and bombs still cost walk credit but only produce dud hits.</summary>
    FakeDeath = 1,

    /// <summary>The revived, standing second phase, and the corpse interval before her final revival.</summary>
    SecondPhase = 2,

    /// <summary>Drained by the Baby Metroid; planted footsteps also queue the drained-form footstep sound.</summary>
    DrainedByBabyMetroid = 3,

    /// <summary>The final phase after the Baby's attack: a beam hit spends Hyper Beam walk credit and recoils on overdraw.</summary>
    ThirdPhase = 4,
}
