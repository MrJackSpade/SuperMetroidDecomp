namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The elevatube's ride, which a tube's entry door composes and starts and the room's main runs (#2554).</summary>
internal interface IElevatubeRide
{
    /// <summary>Gives the tube the starting height, speed and acceleration of a ride from <paramref name="entry"/>.</summary>
    /// <param name="entry">The end of the tube Samus enters from.</param>
    void Start(ElevatubeEntry entry);
}
