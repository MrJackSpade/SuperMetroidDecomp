namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Whether Kraid's room shows his backdrop (#1352, #1466): his rise shows it over the room's blocks, and those blocks and
/// cells it shows over have a picture with it and without it.
/// </summary>
public sealed record KraidBackdropAppearance : AppearanceInput
{
    /// <summary>Creates the backdrop input; every instance is the same input.</summary>
    internal KraidBackdropAppearance()
    {
    }

    /// <inheritdoc />
    public override string Token => AppearanceInputTokens.KraidBackdrop;

    /// <inheritdoc />
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Room.KraidBackdropShown ? RoomAppearanceValues.Shown : RoomAppearanceValues.Loaded;
}
