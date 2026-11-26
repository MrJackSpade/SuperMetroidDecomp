using Engine.Shared;

namespace Engine.World;

/// <summary>
/// A turn a room is shown with: the player, and the objects of the room it names, are drawn where turning them about
/// a point puts them. It changes only where things are shown, never where they are, and what it does not name is
/// shown where it stands.
/// </summary>
/// <param name="Radians">How far it is turned, clockwise on screen.</param>
/// <param name="Centre">The point it is turned about, in the room.</param>
/// <param name="Objects">The objects of the room shown turned with the player; none where it names none.</param>
public readonly record struct RoomTurn(double Radians, PixelPosition Centre, IReadOnlyCollection<RoomObject>? Objects = null)
{
    /// <summary>Whether an object of the room is one of those shown turned.</summary>
    public bool Turns(RoomObject thing) => Objects is not null && Objects.Contains(thing);

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
