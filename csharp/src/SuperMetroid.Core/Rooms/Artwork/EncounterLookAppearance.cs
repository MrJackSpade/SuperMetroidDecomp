using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Whether an encounter shows one of its looks (#1463, #1466, #1538): the Ceres Ridley arena's blocks show the escape's
/// timer and warning once the getaway has brought them up, so a block the look shows over has a picture with the look and
/// without it.
/// </summary>
/// <param name="Look">The encounter's look the block shows over.</param>
public sealed record EncounterLookAppearance(EncounterLook Look) : AppearanceInput
{
    /// <summary>The encounter's look the block shows over.</summary>
    public EncounterLook Look { get; } = Look;

    /// <inheritdoc />
    public override string Token { get; } = $"{AppearanceInputTokens.EncounterLookPrefix}{Look.Encounter} {Look.Name}";

    /// <inheritdoc />
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Encounters.Shows(Look) ? RoomAppearanceValues.Shown : RoomAppearanceValues.Loaded;
}
