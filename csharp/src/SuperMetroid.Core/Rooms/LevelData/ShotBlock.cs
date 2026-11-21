namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A shootable reaction the shot-block response handles, with what the response does with it (#2553): only
/// <see cref="ShotBlockReactions.ShotBlocks"/> holds one, one for each handled reaction, so the response never meets a
/// reaction it does not handle.
/// </summary>
public sealed class ShotBlock
{
    /// <summary>Pairs a handled reaction with what the response does with it.</summary>
    internal ShotBlock(ShootableReaction reaction, ShotBlockReaction response)
    {
        Reaction = reaction;
        Response = response;
    }

    /// <summary>The struck block's reaction.</summary>
    public ShootableReaction Reaction { get; }

    /// <summary>What the shot-block response does with <see cref="Reaction"/>.</summary>
    public ShotBlockReaction Response { get; }
}
