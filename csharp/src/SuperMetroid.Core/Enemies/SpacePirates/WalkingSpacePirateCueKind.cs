namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// What a cue of the walking Space Pirates' animations does: continue at the walk or laser Samus's side and height select,
/// or begin the stand or walk the Pirate's main AI then runs.
/// </summary>
internal enum WalkingSpacePirateCueKind
{
    /// <summary>Continues at the walk or laser animation Samus's side and height select.</summary>
    ChooseMovement,
    /// <summary>Stands still while the animation poses the Pirate: a flinch, a laser volley or a look around.</summary>
    StandStill,
    /// <summary>Walks left on its patrol.</summary>
    WalkLeft,
    /// <summary>Walks right on its patrol.</summary>
    WalkRight,
}
