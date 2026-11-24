using Engine.Shared;

namespace Engine.World;

/// <summary>
/// A turn a room is shown with: everything standing in it is drawn where turning it about a point puts it. It
/// changes only where things are shown, never where they are.
/// </summary>
/// <param name="Radians">How far it is turned, clockwise on screen.</param>
/// <param name="Centre">The point it is turned about, in the room.</param>
public readonly record struct RoomTurn(double Radians, PixelPosition Centre)
{
    /// <summary>Where a place in the room is shown under this turn, to the whole pixel.</summary>
    public PixelPosition Turned(PixelPosition place)
    {
        int across = place.X.Whole - Centre.X.Whole, down = place.Y.Whole - Centre.Y.Whole;
        double cosine = Math.Cos(Radians), sine = Math.Sin(Radians);
        return PixelPosition.FromWhole(
            Centre.X.Whole + (int)Math.Round(across * cosine - down * sine),
            Centre.Y.Whole + (int)Math.Round(across * sine + down * cosine));
    }
}
