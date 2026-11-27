using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A backdrop whose picture slides sideways in bands of rows, each at its own speed, as the layers of a sky do. The
/// picture lies in the room as its <see cref="Backdrop.ScrollShare"/> says, and every band of it is shown moved along
/// by how far that band has drifted, in whole pixels, coming round again at the picture's width. Rows no band covers
/// are shown where they lie.
/// </summary>
/// <remarks>
/// Its data file lists the bands from the top down, each from its first row to the next band's, the last to "to":
/// <code>
/// { "bands": [ { "from": 0, "drift": 0.5 }, { "from": 16, "drift": 0.75 } ], "to": 1280 }
/// </code>
/// A drift is in pixels an update. The bands start undrifted as the room is set up, and drift on each time the room
/// shows itself.
/// </remarks>
[Tool]
public partial class BandedBackdrop : Backdrop
{
    /// <summary>The data file of its bands, as a path within the assets tree.</summary>
    [Export(PropertyHint.File)]
    public string BandsFile { get; set; } = "";

    /// <summary>Each band: its first row, the row just past its last, how far it drifts an update, and how far it has drifted.</summary>
    private (int From, int To, Pixels Drift, Pixels Drifted)[] _bands = [];

    /// <inheritdoc/>
    internal override void Setup(ContentLibrary content, Vector2I roomSize)
    {
        base.Setup(content, roomSize);
        if (BandsFile.Length == 0)
            throw new InvalidOperationException($"The banded backdrop \"{Name}\" names no file of bands.");
        DataNode data = content.ReadData(BandsFile);
        IReadOnlyList<DataNode> listed = data.List("bands");
        int end = data.Int("to");
        var bands = new (int, int, Pixels, Pixels)[listed.Count];
        for (int index = 0; index < listed.Count; index++)
        {
            int from = listed[index].Int("from"), to = index + 1 < listed.Count ? listed[index + 1].Int("from") : end;
            if (to <= from)
                throw listed[index].Problem($"begins at row {from}, at or past where it ends, {to}");
            bands[index] = (from, to, listed[index].Distance("drift"), Pixels.Zero);
        }

        _bands = bands;
    }

    /// <inheritdoc/>
    internal override void Follow(Vector2I viewTopLeft, Vector2I thrown)
    {
        base.Follow(viewTopLeft, thrown);
        for (int index = 0; index < _bands.Length; index++)
            _bands[index].Drifted += _bands[index].Drift;
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (Texture is not { } texture)
            return;
        var size = new Vector2I(texture.GetWidth(), texture.GetHeight());
        int row = 0;
        foreach ((int from, int to, _, Pixels drifted) in _bands)
        {
            Rows(texture, size, row, from, 0);
            Rows(texture, size, from, to, drifted.Whole);
            row = to;
        }

        Rows(texture, size, row, size.Y, 0);
    }

    /// <summary>Lays the picture's rows from one to just short of another across the room, moved along by a drift.</summary>
    private void Rows(Texture2D texture, Vector2I size, int from, int to, int drifted)
    {
        to = Math.Min(to, size.Y);
        if (to <= from)
            return;
        var rows = new Rect2(0, from, size.X, to - from);

        // A drift moves the band the way a scrolled picture moves: on by so many pixels, toward the left.
        for (int x = -(((drifted % size.X) + size.X) % size.X); x < RoomSize.X; x += size.X)
            DrawTextureRectRegion(texture, new Rect2(x, from, size.X, to - from), rows);
    }
}
