namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What each case of a <see cref="NoobTubePieceStart"/> is made into (#2553).</summary>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What each case is made into.</typeparam>
/// <param name="AtSegment">Makes a <see cref="NoobTubePieceStart.AtSegment"/> into a result.</param>
/// <param name="ShardFlight">Makes a <see cref="NoobTubePieceStart.ShardFlight"/> into a result.</param>
internal sealed record NoobTubePieceStartCases<TContext, TResult>(
    Func<NoobTubePieceStart.AtSegment, TContext, TResult> AtSegment,
    Func<NoobTubePieceStart.ShardFlight, TContext, TResult> ShardFlight);
