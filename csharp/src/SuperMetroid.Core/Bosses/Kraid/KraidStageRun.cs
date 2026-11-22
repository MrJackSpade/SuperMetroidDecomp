namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The stage Kraid's body is in and the phase of it he runs (#2208, #2554), each stage a case of its own: his rise through
/// the floor, his fight, his growth into the second phase, his live death up to his sinking, and the sunk body's last
/// phases, which a room he was found defeated in starts at. The stages after his rise hold his head's animation
/// (<see cref="Risen"/>), which his rise's last update starts; his rise and his sunk body hold none.
/// </summary>
public abstract partial class KraidStageRun
{
    /// <summary>Only the stages below derive from it.</summary>
    private protected KraidStageRun()
    {
    }

    /// <summary>The stage's name.</summary>
    public abstract KraidBodyStage Stage { get; }

    /// <summary>Hands the stage to the method <paramref name="runner"/> has for its case.</summary>
    /// <param name="runner">What runs one update of the stage.</param>
    internal abstract void RunWith(IKraidStageRunner runner);

    /// <summary>His rise through the floor at the start of the encounter, outside the fight.</summary>
    /// <param name="phase">The phase of the rise he starts at.</param>
    public sealed partial class Rising(KraidRisePhase phase) : KraidStageRun
    {
        /// <summary>The phase of his rise the body runs.</summary>
        public KraidRisePhase Phase { get; internal set; } = phase;

        /// <inheritdoc/>
        public override KraidBodyStage Stage => KraidBodyStage.Rising;

        /// <inheritdoc/>
        internal override void RunWith(IKraidStageRunner runner) => runner.Run(this);
    }

    /// <summary>A stage after his rise, which holds his head's animation.</summary>
    public abstract partial class Risen : KraidStageRun
    {
        /// <summary>A stage holding <paramref name="head"/>.</summary>
        /// <param name="head">His head's animation, which the stage before handed on.</param>
        private protected Risen(KraidRisenHead head) => Head = head;

        /// <summary>His head's animation.</summary>
        public KraidRisenHead Head { get; }

        /// <summary>
        /// Whether Samus's shots collide with him: while he fights and grows, and through his death's fade, the update that
        /// shows his body and the one that starts his sinking.
        /// </summary>
        internal abstract bool ShotsCollide { get; }
    }

    /// <summary>His fight: thinking, attacking with his mouth open and reacting to hits.</summary>
    /// <param name="phase">The phase of the fight he enters.</param>
    /// <param name="head">His head's animation.</param>
    public sealed partial class Fighting(KraidFightPhase phase, KraidRisenHead head) : Risen(head)
    {
        /// <summary>The <c>head</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
        private readonly KraidRisenHead head = head;

        /// <summary>The phase of his fight the body runs.</summary>
        public KraidFightPhase Phase { get; internal set; } = phase;

        /// <inheritdoc/>
        public override KraidBodyStage Stage => KraidBodyStage.Fighting;

        /// <inheritdoc/>
        internal override void RunWith(IKraidStageRunner runner) => runner.Run(this);

        /// <inheritdoc/>
        internal override bool ShotsCollide => true;
    }

    /// <summary>His growth into his second phase, which is part of the fight.</summary>
    /// <param name="phase">The phase of the growth he enters.</param>
    /// <param name="head">His head's animation.</param>
    public sealed partial class Growing(KraidGrowthPhase phase, KraidRisenHead head) : Risen(head)
    {
        /// <summary>The <c>head</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
        private readonly KraidRisenHead head = head;

        /// <summary>The phase of his growth the body runs.</summary>
        public KraidGrowthPhase Phase { get; internal set; } = phase;

        /// <inheritdoc/>
        public override KraidBodyStage Stage => KraidBodyStage.Growing;

        /// <inheritdoc/>
        internal override void RunWith(IKraidStageRunner runner) => runner.Run(this);

        /// <inheritdoc/>
        internal override bool ShotsCollide => true;
    }

    /// <summary>His death from the fight up to his sinking through the floor.</summary>
    /// <param name="phase">The phase of the death he enters.</param>
    /// <param name="head">His head's animation, which plays his dying cry.</param>
    public sealed partial class Dying(KraidDeathPhase phase, KraidRisenHead head) : Risen(head)
    {
        /// <summary>The <c>head</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
        private readonly KraidRisenHead head = head;

        /// <summary>The phase of his death the body runs.</summary>
        public KraidDeathPhase Phase { get; internal set; } = phase;

        /// <inheritdoc/>
        public override KraidBodyStage Stage => KraidBodyStage.Dying;

        /// <inheritdoc/>
        internal override void RunWith(IKraidStageRunner runner) => runner.Run(this);

        /// <inheritdoc/>
        internal override bool ShotsCollide => Phase != KraidDeathPhase.Sink;
    }

    /// <summary>
    /// His death once he has sunk, or the defeated room's body: hiding his body, the room's dark wait, its fade back in and
    /// the finished death.
    /// </summary>
    /// <param name="phase">The phase of the sunk body he enters.</param>
    public sealed partial class Sunk(KraidSunkPhase phase) : KraidStageRun
    {
        /// <summary>The phase of the sunk body's death the body runs.</summary>
        public KraidSunkPhase Phase { get; internal set; } = phase;

        /// <inheritdoc/>
        public override KraidBodyStage Stage => KraidBodyStage.Dying;

        /// <inheritdoc/>
        internal override void RunWith(IKraidStageRunner runner) => runner.Run(this);
    }
}
