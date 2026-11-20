using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Core.World;

/// <summary>
/// The statue room's statues as its enemies built them (#2554): the statues' room they were built over and the actors
/// that show them, which the statues' sequence takes together.
/// </summary>
/// <param name="Room">The statues' room.</param>
/// <param name="Actors">The statue actors the room's enemies built over it.</param>
internal sealed record RoomTourianStatues(TourianStatueRoom Room, ITourianStatueActors Actors);
