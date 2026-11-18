namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a permanent pickup changes in the gameplay frame as its message opens (#2383): a beam shows the newly equipped
/// combination's colours, a suit starts its transformation once its message closes, and every other item changes
/// nothing there, its inventory already applied.
/// </summary>
public abstract record CollectiblePickupConsequence
{
    /// <summary>Only the consequences below derive from this.</summary>
    private protected CollectiblePickupConsequence()
    {
    }

    /// <summary>A pickup whose inventory is all it changes.</summary>
    public static CollectiblePickupConsequence Nothing { get; } = new NoChange();

    /// <summary>A beam pickup, which shows the newly equipped combination's colours.</summary>
    public static CollectiblePickupConsequence ShowsEquippedBeams { get; } = new EquippedBeams();

    /// <summary>A suit pickup, whose transformation to <paramref name="suit"/> starts once its message closes.</summary>
    public static CollectiblePickupConsequence TransformsSuit(SamusSuitPickupKind suit) => new SuitTransformation(suit);

    /// <summary>Applies the consequence through <paramref name="consequences"/>.</summary>
    internal abstract void Apply(ICollectiblePickupConsequences consequences);

    /// <summary>A pickup whose inventory is all it changes.</summary>
    private sealed record NoChange : CollectiblePickupConsequence
    {
        /// <inheritdoc/>
        internal override void Apply(ICollectiblePickupConsequences consequences)
        {
        }
    }

    /// <summary>A beam pickup.</summary>
    private sealed record EquippedBeams : CollectiblePickupConsequence
    {
        /// <inheritdoc/>
        internal override void Apply(ICollectiblePickupConsequences consequences) =>
            Ensure.NotNull(consequences).ShowEquippedBeamColours();
    }

    /// <summary>A suit pickup.</summary>
    /// <param name="Suit">The suit the transformation reveals.</param>
    private sealed record SuitTransformation(SamusSuitPickupKind Suit) : CollectiblePickupConsequence
    {
        /// <inheritdoc/>
        internal override void Apply(ICollectiblePickupConsequences consequences) =>
            Ensure.NotNull(consequences).AwaitSuitTransformation(Suit);
    }
}
