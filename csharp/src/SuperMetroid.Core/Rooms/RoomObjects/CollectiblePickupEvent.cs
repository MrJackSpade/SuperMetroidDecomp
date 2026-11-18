namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One defined permanent pickup completed by the current room-object pass.</summary>
/// <param name="Kind">The item whose inventory effect and collected-item persistence have been applied.</param>
/// <param name="Message">The gameplay-message identity requested for the synchronous pickup notification.</param>
/// <param name="Consequence">What the pickup changes in the gameplay frame as its message opens (#2383).</param>
public readonly record struct CollectiblePickupEvent(
    InWorldCollectibleKind Kind,
    GameplayMessageId Message,
    CollectiblePickupConsequence Consequence);

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

/// <summary>What the gameplay frame does for a pickup's consequence (#2383).</summary>
internal interface ICollectiblePickupConsequences
{
    /// <summary>Shows the newly equipped beam combination's colours.</summary>
    void ShowEquippedBeamColours();

    /// <summary>Holds the transformation to <paramref name="suit"/> until the pickup's message closes.</summary>
    void AwaitSuitTransformation(SamusSuitPickupKind suit);
}
