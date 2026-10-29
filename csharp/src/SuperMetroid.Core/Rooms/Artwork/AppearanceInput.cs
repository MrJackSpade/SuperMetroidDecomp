namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One named input a room's pictures depend on (#1466): the collectible a collectible slot shows, whether Kraid's backdrop
/// shows, the frame an animation selected, or whether an encounter's look shows on the sprites' artwork the Ceres Ridley
/// arena's blocks show. Extraction records which inputs each block, background cell and written word depends on and
/// pictures it for every value they can take; the room reads the values gameplay published and names the picture by
/// them, so nothing in the room knows which characters a picture was composed from.
/// </summary>
public abstract record AppearanceInput
{
    /// <summary>How a picture's name spells the input.</summary>
    public abstract string Token { get; }

    /// <summary>The value the input takes in <paramref name="appearance"/>, as a picture's name spells it.</summary>
    internal abstract string ValueIn(RoomAppearanceView appearance);
}
