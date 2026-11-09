namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The reaction a reacting block selects: an entry of its collision kind's common reactions, or of the reactions its
/// room's area gives that kind. The kind decides what each entry does; equal entries of different kinds are unrelated.
/// </summary>
public readonly record struct BlockReaction
{
    /// <summary>The last entry a reaction list has.</summary>
    public const int LastEntry = 127;

    /// <summary>A reaction of <paramref name="scope"/> at <paramref name="entry"/>.</summary>
    /// <param name="scope">The list the reaction is an entry of.</param>
    /// <param name="entry">The entry, 0 through <see cref="LastEntry"/>.</param>
    public BlockReaction(BlockReactionScope scope, int entry)
    {
        Scope = Enum.IsDefined(scope) ? scope : throw new ArgumentOutOfRangeException(nameof(scope), scope, "Undefined reaction scope.");
        Entry = Ensure.BetweenInclusive(0, LastEntry, entry);
    }

    /// <summary>The list the reaction is an entry of.</summary>
    public BlockReactionScope Scope { get; }

    /// <summary>The reaction's entry in its list.</summary>
    public int Entry { get; }

    /// <summary>Whether the reaction is one the room's area gives the block's kind.</summary>
    public bool IsAreaSpecific => Scope == BlockReactionScope.AreaSpecific;

    /// <summary>The common reaction at <paramref name="entry"/>.</summary>
    public static BlockReaction Common(int entry) => new(BlockReactionScope.Common, entry);

    /// <summary>The area's reaction at <paramref name="entry"/>.</summary>
    public static BlockReaction AreaSpecific(int entry) => new(BlockReactionScope.AreaSpecific, entry);

    /// <summary>Every reaction: each common entry in order, then each area entry in order.</summary>
    public static IEnumerable<BlockReaction> All =>
        Enumerable.Range(0, LastEntry + 1).Select(Common)
            .Concat(Enumerable.Range(0, LastEntry + 1).Select(AreaSpecific));

    /// <summary>Whether this is one of the first <paramref name="count"/> common reactions.</summary>
    /// <param name="count">How many common reactions the dispatching kind defines.</param>
    public bool IsCommonEntryBelow(int count) => Scope == BlockReactionScope.Common && Entry < count;

    /// <summary>Whether this is one of the first <paramref name="count"/> area reactions.</summary>
    /// <param name="count">How many area reactions the dispatching kind defines.</param>
    public bool IsAreaEntryBelow(int count) => Scope == BlockReactionScope.AreaSpecific && Entry < count;

    /// <summary>Whether a shootable block with this reaction is the collision probe (<see cref="ShotBlockReaction.CollisionProbe"/>).</summary>
    public bool IsShotCollisionProbe => ShotBlockReactions.IsCollisionProbe(this);

    /// <summary>Names the reaction's list and entry.</summary>
    public override string ToString() => $"{(IsAreaSpecific ? "area" : "common")} {Entry}";
}
