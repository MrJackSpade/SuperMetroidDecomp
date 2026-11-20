namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's body animations: what it does, with the values it acts on.</summary>
internal abstract record TorizoCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this torizo cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the torizo cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    internal abstract TResult Match<TResult>(TorizoCueCases<TResult> cases);


    /// <summary>
    /// A dodge or an attack that leaves the walk for good when its test passes (#2028): the owner continues in the
    /// decision's animation for the way the walk's last step faced and the foot it stepped on (#2175,
    /// <see cref="TorizoBranches.Target"/>).
    /// </summary>
    /// <param name="Kind">The decision and what it tests.</param>
    internal sealed record Branch(TorizoBranchKind Kind) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Branch(this);
    }

    /// <summary>
    /// An attack or a stagger that interrupts the walk when its test passes (#2028): the walk's place is kept, and
    /// <paramref name="Animation"/> resumes the walk there when it ends (<see cref="TorizoCueKind.ResumeWalk"/>).
    /// </summary>
    /// <param name="Kind">The attack or stagger and what it tests.</param>
    /// <param name="Animation">The animation that interrupts the walk, from its start.</param>
    internal sealed record Interrupt(TorizoInterruptKind Kind, TorizoAnimation Animation) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Interrupt(this);
    }

    /// <summary>A decision between two body animations (#2028).</summary>
    /// <param name="Kind">The decision and what it tests.</param>
    /// <param name="Target">The animation the decision continues in from its start when its test passes.</param>
    /// <param name="Alternate">The animation the decision continues in otherwise.</param>
    internal sealed record Choice(TorizoChoiceKind Kind, TorizoAnimation Target, TorizoAnimation Alternate) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Choice(this);
    }

    /// <summary>
    /// Names the animation the Torizo's jump or fall continues in (#2028, #2411): its leaping motion starts it when the Torizo
    /// meets the ground or the ceiling, and a jump's owner starts it once the Torizo no longer rises. A jump names its fall,
    /// a fall its landing.
    /// </summary>
    /// <param name="Landing">The fall or landing animation.</param>
    internal sealed record LandIn(TorizoAnimation Landing) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).LandIn(this);
    }

    /// <summary>Ends an attack chosen on landing: the Torizo walks on in <paramref name="Walking"/>, from its start (#2028).</summary>
    /// <param name="Walking">The walk the attack ends in.</param>
    internal sealed record WalkOn(TorizoAnimation Walking) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).WalkOn(this);
    }

    /// <summary>Ends the animation in the turn away from the way the Torizo faces (#2028).</summary>
    /// <param name="WhenFacingRight">The turn of a Torizo facing right.</param>
    /// <param name="WhenFacingLeft">The turn of a Torizo facing left.</param>
    internal sealed record Turn(TorizoAnimation WhenFacingRight, TorizoAnimation WhenFacingLeft) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Turn(this);
    }

    /// <summary>Stands a Torizo up or sits it down, moving it as its body rises or settles (#1891).</summary>
    /// <param name="Kind">Which way the posture changes.</param>
    /// <param name="Displacement">
    /// How far standing up moves the Torizo, in pixels; sitting down moves it back by as much. The animations of each facing
    /// carry that facing's displacement.
    /// </param>
    internal sealed record Posture(TorizoPostureKind Kind, Offset16<RoomPixels> Displacement) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Posture(this);
    }

    /// <summary>One walking step of a Torizo (#1891).</summary>
    /// <param name="Walker">How the Torizo walks in the state whose walk takes the step, and turns about at a wall (#2472).</param>
    /// <param name="Velocity">
    /// The whole pixels the step moves the Torizo, negative to the left; the animations of each facing carry that facing's
    /// velocity.
    /// </param>
    internal sealed record Walk(TorizoWalker Walker, int Velocity) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Walk(this);
    }

    /// <summary>Spawns an explosive swipe at Bomb Torizo's hand.</summary>
    /// <param name="Hand">Where the hand is relative to a left-facing Torizo; a right-facing Torizo mirrors it.</param>
    internal sealed record Swipe(Offset16<RoomPixels> Hand) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Swipe(this);
    }

    /// <summary>
    /// Spawns the low-health explosions and flashes; the animation then waits before it goes on.
    /// </summary>
    /// <param name="Site">Where the explosions burst.</param>
    internal sealed record LowHealthExplosions(BombTorizoExplosionSite Site) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).LowHealthExplosions(this);
    }

    /// <summary>Selects the main-AI phase.</summary>
    /// <param name="Phase">The main-AI phase a <see cref="SetPhase"/> cue selects.</param>
    internal sealed record SetPhase(TorizoPhase Phase) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).SetPhase(this);
    }

    /// <summary>Shows a look of the body's artwork under its poses.</summary>
    /// <param name="Look">The look a <see cref="ShowArtwork"/> cue shows.</param>
    internal sealed record ShowArtwork(TorizoArtworkLook Look) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).ShowArtwork(this);
    }

    /// <summary>
    /// Records a step the cue's way on the cue's foot (#2165): the posture faces that way, with the opposite leg forward when
    /// the foot is not on that side, and the step counts toward the next decision.
    /// </summary>
    /// <param name="Direction">The way the body stepped.</param>
    /// <param name="Foot">The foot it stepped on.</param>
    internal sealed record Stepped(HorizontalDirection Direction, HorizontalDirection Foot) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Stepped(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(TorizoCueKind Kind) : TorizoCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(TorizoCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="TorizoCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the torizo cue is made into.</typeparam>
/// <param name="Branch">Makes a <see cref="TorizoCue.Branch"/> into the result.</param>
/// <param name="Interrupt">Makes a <see cref="TorizoCue.Interrupt"/> into the result.</param>
/// <param name="Choice">Makes a <see cref="TorizoCue.Choice"/> into the result.</param>
/// <param name="LandIn">Makes a <see cref="TorizoCue.LandIn"/> into the result.</param>
/// <param name="WalkOn">Makes a <see cref="TorizoCue.WalkOn"/> into the result.</param>
/// <param name="Turn">Makes a <see cref="TorizoCue.Turn"/> into the result.</param>
/// <param name="Posture">Makes a <see cref="TorizoCue.Posture"/> into the result.</param>
/// <param name="Walk">Makes a <see cref="TorizoCue.Walk"/> into the result.</param>
/// <param name="Swipe">Makes a <see cref="TorizoCue.Swipe"/> into the result.</param>
/// <param name="LowHealthExplosions">Makes a <see cref="TorizoCue.LowHealthExplosions"/> into the result.</param>
/// <param name="SetPhase">Makes a <see cref="TorizoCue.SetPhase"/> into the result.</param>
/// <param name="ShowArtwork">Makes a <see cref="TorizoCue.ShowArtwork"/> into the result.</param>
/// <param name="Stepped">Makes a <see cref="TorizoCue.Stepped"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="TorizoCue.Signal"/> into the result.</param>
internal sealed record TorizoCueCases<TResult>(Func<TorizoCue.Branch, TResult> Branch, Func<TorizoCue.Interrupt, TResult> Interrupt, Func<TorizoCue.Choice, TResult> Choice, Func<TorizoCue.LandIn, TResult> LandIn, Func<TorizoCue.WalkOn, TResult> WalkOn, Func<TorizoCue.Turn, TResult> Turn, Func<TorizoCue.Posture, TResult> Posture, Func<TorizoCue.Walk, TResult> Walk, Func<TorizoCue.Swipe, TResult> Swipe, Func<TorizoCue.LowHealthExplosions, TResult> LowHealthExplosions, Func<TorizoCue.SetPhase, TResult> SetPhase, Func<TorizoCue.ShowArtwork, TResult> ShowArtwork, Func<TorizoCue.Stepped, TResult> Stepped, Func<TorizoCue.Signal, TResult> Signal);
