using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Whether an encounter's look shows on the sprites' artwork (#1463, #1466): the Ceres Ridley arena's blocks show the
/// sprites' artwork, so a block its getaway's looks show over has a picture with each look and without it.
/// </summary>
/// <param name="Look">The encounter's look.</param>
public sealed record EncounterLookAppearance(SpriteArtworkSource Look) : AppearanceInput
{
    /// <inheritdoc />
    public override string Token => $"{AppearanceInputTokens.EncounterLookPrefix}{Look}";

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">No enemy sheets are installed.</exception>
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Sprites.Shows(Look) ? RoomAppearanceValues.Shown : RoomAppearanceValues.Loaded;
}
