namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Whether Kraid's room shows his backdrop (#1352, #1466): his rise shows it over the room's tiles, and the blocks and
/// cells it shows over have a picture with it and without it.
/// </summary>
public sealed record KraidBackdropAppearance : AppearanceInput
{
    /// <summary>The one backdrop input.</summary>
    public static KraidBackdropAppearance Instance { get; } = new();

    /// <summary>Creates the one backdrop input.</summary>
    private KraidBackdropAppearance()
    {
    }

    /// <inheritdoc />
    public override string Token => AppearanceInputTokens.KraidBackdrop;

    /// <inheritdoc />
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Room.KraidBackdropShown ? RoomAppearanceValues.Shown : RoomAppearanceValues.Loaded;
}
