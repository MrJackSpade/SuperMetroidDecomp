namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a reacting collision kind reads of a block's behaviour (#1742, #2554): the kind's reaction, or no reaction, as a
/// behaviour a kind change brings to the kind may give it, where the game reads unrelated data.
/// </summary>
/// <typeparam name="TReaction">The kind's reactions.</typeparam>
public abstract record BlockKindReading<TReaction>
    where TReaction : struct, Enum
{
    /// <summary>Only the readings below are choices.</summary>
    private BlockKindReading()
    {
    }

    /// <summary>The reading of a behaviour that gives the kind no reaction.</summary>
    private static readonly BlockKindReading<TReaction> None = new NoReaction();

    /// <summary>The behaviour gives the kind <paramref name="Reaction"/>.</summary>
    /// <param name="Reaction">The kind's reaction.</param>
    public sealed record Reacts(TReaction Reaction) : BlockKindReading<TReaction>;

    /// <summary>The behaviour gives the kind no reaction.</summary>
    public sealed record NoReaction : BlockKindReading<TReaction>;

    /// <summary>The reading of a behaviour whose reading for the kind is <paramref name="read"/>, or none.</summary>
    internal static BlockKindReading<TReaction> Of(TReaction? read) => read is { } reaction ? new Reacts(reaction) : None;
}
