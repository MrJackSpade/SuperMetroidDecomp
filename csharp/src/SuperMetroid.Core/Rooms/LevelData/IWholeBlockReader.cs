namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What one reader of a whole block makes of each kind of block it can meet (#2554): a whole block is one that stands for
/// itself, not a cell continuing another (<see cref="RoomBlockKind.Whole"/>). The block's kind calls the one member for its
/// own case (<see cref="RoomBlockKind.Whole.Read{TAnswer}(IWholeBlockReader{TAnswer})"/>), handing a reacting kind's member
/// the reaction the block carries, so every reader answers every kind and none can meet a kind it did not answer.
/// </summary>
/// <typeparam name="TAnswer">What the reader makes of a block.</typeparam>
public interface IWholeBlockReader<out TAnswer>
{
    /// <summary>What the reader makes of empty air.</summary>
    TAnswer Air();

    /// <summary>What the reader makes of a slope, whose geometry the block's behaviour selects.</summary>
    TAnswer Slope();

    /// <summary>What the reader makes of air that reacts to nothing and that the X-ray scope does not reveal.</summary>
    TAnswer UnrevealedAir();

    /// <summary>What the reader makes of an ordinary solid block.</summary>
    TAnswer SolidBlock();

    /// <summary>What the reader makes of a door block.</summary>
    TAnswer DoorBlock();

    /// <summary>What the reader makes of spike air.</summary>
    /// <param name="reaction">The spike reaction the block carries.</param>
    TAnswer SpikeAir(SpikeReaction reaction);

    /// <summary>What the reader makes of a solid spike block.</summary>
    /// <param name="reaction">The spike reaction the block carries.</param>
    TAnswer SpikeBlock(SpikeReaction reaction);

    /// <summary>What the reader makes of special air.</summary>
    /// <param name="reaction">The special-air reaction the block carries.</param>
    TAnswer SpecialAir(SpecialAirReaction reaction);

    /// <summary>What the reader makes of a solid special block.</summary>
    /// <param name="reaction">The special-block reaction the block carries.</param>
    TAnswer SpecialBlock(SpecialBlockReaction reaction);

    /// <summary>What the reader makes of shootable air.</summary>
    /// <param name="reaction">The shootable reaction the block carries.</param>
    TAnswer ShootableAir(ShootableReaction reaction);

    /// <summary>What the reader makes of a solid shootable block.</summary>
    /// <param name="reaction">The shootable reaction the block carries.</param>
    TAnswer ShootableBlock(ShootableReaction reaction);

    /// <summary>What the reader makes of bombable air.</summary>
    /// <param name="reaction">The bombable reaction the block carries.</param>
    TAnswer BombableAir(BombableReaction reaction);

    /// <summary>What the reader makes of a solid bombable block.</summary>
    /// <param name="reaction">The bombable reaction the block carries.</param>
    TAnswer BombableBlock(BombableReaction reaction);

    /// <summary>What the reader makes of a grapple block.</summary>
    /// <param name="reaction">The grapple reaction the block carries.</param>
    TAnswer GrappleBlock(GrappleReaction reaction);
}
