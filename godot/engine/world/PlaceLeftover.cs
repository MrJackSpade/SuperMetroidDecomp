using Engine.Shared;

namespace Engine.World;

/// <summary>
/// What the last holder of one of a room's places for what its enemies launch and leave left there as it went: where
/// it was, in whole pixels, and the speed it had. The next thing to take the place finds it
/// (<see cref="Room.FoundInPlace"/>), in the same room or, the session carrying it over, in the next; one that sets
/// no place or speed of its own as it is made starts from them.
/// </summary>
/// <param name="Place">Where the last holder was; the part of a pixel does not last.</param>
/// <param name="Speed">The speed the last holder had.</param>
public readonly record struct PlaceLeftover(PixelPosition Place, PixelVelocity Speed)
{
    /// <summary>This, its place in whole pixels.</summary>
    public PlaceLeftover AtWholePixels() => this with { Place = PixelPosition.FromWhole(Place.X.Whole, Place.Y.Whole) };
}
