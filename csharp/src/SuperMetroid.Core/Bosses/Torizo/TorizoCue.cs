namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's body animations: what it does, with the values it acts on.</summary>
internal abstract record TorizoCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoCue()
    {
    }

    /// <summary>A branch, call or link to another body animation.</summary>
    /// <param name="Kind">What the branch tests or does.</param>
    /// <param name="Target">The segment a branch, call or link cue continues at or saves.</param>
    internal sealed record Branch(TorizoBranchKind Kind, TorizoBodySegment Target) : TorizoCue;

    /// <summary>A two-way branch or call between two body animations.</summary>
    /// <param name="Kind">What the choice tests.</param>
    /// <param name="Target">The segment a branch, call or link cue continues at or saves.</param>
    /// <param name="Alternate">The segment a two-way branch or call continues at otherwise.</param>
    internal sealed record Choice(TorizoChoiceKind Kind, TorizoBodySegment Target, TorizoBodySegment Alternate) : TorizoCue;

    /// <summary>Stands Bomb Torizo up or sits it down, moving it by a movement-table entry.</summary>
    /// <param name="Kind">Which way the posture changes.</param>
    /// <param name="Step">The posture step the change applies, one of <see cref="BombTorizoMovement.PostureStepCount"/> steps.</param>
    internal sealed record Posture(TorizoPostureKind Kind, int Step) : TorizoCue;

    /// <summary>One walking step of a Torizo, read from its walking table.</summary>
    /// <param name="Kind">Which Torizo walks.</param>
    /// <param name="Step">The walking step, one of the walking Torizo's twenty steps.</param>
    internal sealed record Walk(TorizoWalkKind Kind, int Step) : TorizoCue;

    /// <summary>Spawns a Torizo projectile with a spawn parameter.</summary>
    /// <param name="Kind">What the cue spawns.</param>
    /// <param name="Parameter">
    /// What the spawn places: a swipe's placement index or a sonic boom's parameter.
    /// </param>
    internal sealed record ParameterizedSpawn(TorizoParameterizedSpawnKind Kind, int Parameter) : TorizoCue;

    /// <summary>
    /// Spawns the low-health explosions and flashes; the animation then waits forty updates.
    /// </summary>
    /// <param name="Site">Where the explosions burst.</param>
    internal sealed record LowHealthExplosions(BombTorizoExplosionSite Site) : TorizoCue;

    /// <summary>Selects the main-AI function.</summary>
    /// <param name="Function">The main-AI function a <see cref="SetFunction"/> cue selects.</param>
    internal sealed record SetFunction(TorizoFunction Function) : TorizoCue;

    /// <summary>Installs the body's frame behaviour.</summary>
    /// <param name="FrameBehavior">The frame behaviour a <see cref="SetFrameBehavior"/> cue installs.</param>
    internal sealed record SetFrameBehavior(TorizoFrameBehavior FrameBehavior) : TorizoCue;

    /// <summary>Shows a piece of artwork under the body's poses (the game's common artwork copy).</summary>
    /// <param name="Swap">The artwork a <see cref="ShowArtwork"/> cue shows.</param>
    internal sealed record ShowArtwork(TorizoArtworkSwap Swap) : TorizoCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(TorizoCueKind Kind) : TorizoCue;
}
