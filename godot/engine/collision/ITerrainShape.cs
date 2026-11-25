using Engine.Shared;

namespace Engine.Collision;

/// <summary>A room's terrain as collision sees it: which pixels are solid ground.</summary>
public interface ITerrainShape
{
    /// <summary>Whether any pixel of <paramref name="rect"/> is solid terrain, with slopes treated as <paramref name="slopes"/> says.</summary>
    bool IsSolidIn(PixelRect rect, SlopeTreatment slopes);

    /// <summary>Whether the solid terrain in <paramref name="rect"/>, if any, includes sloped ground.</summary>
    bool HasSlopeIn(PixelRect rect);

    /// <summary>
    /// What the terrain makes of a move tried only where it ends (<see cref="WayTried.WhereItEnds"/>). The tiles
    /// <paramref name="edge"/> lies in are tried in turn, from its top or its left end, and the first that stops the
    /// move decides where the body rests.
    /// </summary>
    /// <param name="edge">The strip one pixel thick the body's leading edge would end in.</param>
    /// <param name="across">The way the move goes across: -1 left, 1 right, 0 for a move up or down.</param>
    /// <param name="down">The way the move goes down: -1 up, 1 down, 0 for a move across.</param>
    /// <param name="slopes">How the move treats sloped ground.</param>
    EdgeStop StopsEdge(PixelRect edge, int across, int down, SlopeTreatment slopes);

    /// <summary>
    /// The row a pixel in the column <paramref name="x"/> rests in against the sloped ground, of the kind that holds
    /// things up by their middle, in the tile holding the pixel at <paramref name="x"/>, <paramref name="y"/>: the
    /// last clear row above a floor's solid part, or with <paramref name="ceiling"/> the first clear row below a
    /// ceiling's. Null when that tile is no such floor, or no such ceiling.
    /// </summary>
    int? SlopeRestRow(int x, int y, bool ceiling);
}
