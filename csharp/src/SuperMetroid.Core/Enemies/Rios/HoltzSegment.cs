namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Holtz's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed animations
/// (#641): one segment per animation a spawn, branch or wake enters, and the wingbeat the idle loops after its entry cue
/// (#2276), in authored order. Their steps are <see cref="HoltzAnimations"/>.
/// </summary>
internal enum HoltzSegment
{
    /// <summary>The idle animation's entry: the cue that hides the flames.</summary>
    Idle,
    /// <summary>The idle wingbeat, looped.</summary>
    IdleWingbeat,
    /// <summary>Its flames hidden, its wings lifting, spreading fully and folding before it swoops.</summary>
    PrepareToSwoop,
    /// <summary>Its flames hidden, held with its wings folded while it swoops down.</summary>
    Descending,
    /// <summary>The opening of the climb back to the perch, which holds its last pose until its cue.</summary>
    AscentOpening,
    /// <summary>The climb back to the perch: its flames shown, held with its wings swept (#2512).</summary>
    Ascending,
    /// <summary>Its flames showing, its wings sweeping and spreading open again after its swoop.</summary>
    Cooldown,
    /// <summary>Its flames flickering from large to small, repeating.</summary>
    Flames,
}
