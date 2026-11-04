using System.Globalization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Whether an encounter has played its artwork sequence through one step (#1463, #1466, #1513): the Ceres Ridley arena's
/// blocks show the escape's pages once the getaway has brought them up, so a block a page shows over has a picture with the
/// page and without it.
/// </summary>
/// <param name="Sequence">The encounter's artwork sequence.</param>
/// <param name="Step">The step whose page the block shows once the sequence reaches it.</param>
public sealed record EncounterLookAppearance(EncounterArtworkSequence Sequence, int Step) : AppearanceInput
{
    /// <summary>The encounter's artwork sequence.</summary>
    public EncounterArtworkSequence Sequence { get; } = Sequence;

    /// <summary>The step whose page the block shows once the sequence reaches it.</summary>
    public int Step { get; } = Ensure.AtLeastZero(Step);

    /// <inheritdoc />
    public override string Token { get; } =
        $"{AppearanceInputTokens.EncounterLookPrefix}{Sequence} {Step.ToString(CultureInfo.InvariantCulture)}";

    /// <inheritdoc />
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Encounters.Reached(Sequence, Step) ? RoomAppearanceValues.Shown : RoomAppearanceValues.Loaded;
}
