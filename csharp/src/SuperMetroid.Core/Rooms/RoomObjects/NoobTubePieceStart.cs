namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How a piece of the broken n00b tube starts moving: the crack and the released-air bubbles start the animation that
/// begins at a segment, and each glass shard starts its own flight (#2044). The tube's enemy-side behaviour turns it into
/// the animation the piece plays.
/// </summary>
internal abstract record NoobTubePieceStart
{
    /// <summary>Only the two cases below start a piece.</summary>
    private NoobTubePieceStart()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the start, through the case it is (#2553).</summary>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <typeparam name="TResult">What the start is made into.</typeparam>
    /// <param name="cases">What each case of the start is made into.</param>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(NoobTubePieceStartCases<TContext, TResult> cases, TContext context);

    /// <summary>A piece that starts the animation beginning at <paramref name="Segment"/>: the crack or a bubble.</summary>
    /// <param name="Segment">The segment the piece's animation begins at.</param>
    internal sealed record AtSegment(NoobTubeProjectileSegment Segment) : NoobTubePieceStart
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NoobTubePieceStartCases<TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).AtSegment(this, context);
    }

    /// <summary>A glass shard, which starts its own flight.</summary>
    /// <param name="Shard">The shard whose flight it starts.</param>
    internal sealed record ShardFlight(NoobTubeShard Shard) : NoobTubePieceStart
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NoobTubePieceStartCases<TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).ShardFlight(this, context);
    }
}

/// <summary>What each case of a <see cref="NoobTubePieceStart"/> is made into (#2553).</summary>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What each case is made into.</typeparam>
/// <param name="AtSegment">Makes a <see cref="NoobTubePieceStart.AtSegment"/> into a result.</param>
/// <param name="ShardFlight">Makes a <see cref="NoobTubePieceStart.ShardFlight"/> into a result.</param>
internal sealed record NoobTubePieceStartCases<TContext, TResult>(
    Func<NoobTubePieceStart.AtSegment, TContext, TResult> AtSegment,
    Func<NoobTubePieceStart.ShardFlight, TContext, TResult> ShardFlight);
