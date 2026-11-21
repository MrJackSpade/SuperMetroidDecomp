using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The scrolling sky's rows (#1347, #1403), drawn from the sky's image once the room has configured its bands.</summary>
public sealed record SkyBackground : ShownRoomBackground
{
    /// <summary>Keeps the sky's rows, drawn from its image.</summary>
    /// <param name="rows">The sky's rows, drawn from its image.</param>
    public SkyBackground(SkyPictures rows) => Rows = Ensure.NotNull(rows);

    /// <summary>The sky's rows, drawn from its image.</summary>
    internal SkyPictures Rows { get; }

    /// <inheritdoc/>
    public override TResult Map<TResult>(IShownRoomBackgroundMapping<TResult> mapping) => Ensure.NotNull(mapping).Sky(Rows);
}
