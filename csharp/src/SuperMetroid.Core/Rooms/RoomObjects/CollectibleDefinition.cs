namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What one permanent item does once collected and how it is pictured where it lies (#2481): what it gives Samus, the
/// message its pickup opens, what the pickup changes in the gameplay frame as that message opens, and whether it shows a
/// picture of its own.
/// </summary>
/// <param name="Grant">What collecting it gives Samus.</param>
/// <param name="Message">The message its pickup opens.</param>
/// <param name="Consequence">What its pickup changes in the gameplay frame as the message opens.</param>
/// <param name="Picture">Whether it shows a picture of its own or one of the tank blocks every room loads.</param>
internal sealed partial record CollectibleDefinition(
    CollectibleGrant Grant,
    GameplayMessageId Message,
    CollectiblePickupConsequence Consequence,
    CollectiblePictureSource Picture)
{
    /// <summary>
    /// The placements whose pickup starts the room's lava rising once the pickup's message closes (#2562): the Speed
    /// Booster's Chozo orb, whose pickup leaves the lava's motion to the message's close; none for every other item.
    /// </summary>
    internal IReadOnlySet<CollectiblePresentation> LavaRisingPlacements { get; init; } = new HashSet<CollectiblePresentation>();
}
