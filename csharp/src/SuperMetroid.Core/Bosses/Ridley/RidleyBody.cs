using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The parts of Ridley's body both encounters run (#2229): his flight, his reveal, his body animation, his touch and hurt
/// flash, the drawing of his tail and wings, and his tail. Ceres's and Lower Norfair's encounters each run the same parts;
/// neither runs through the other.
/// </summary>
/// <param name="Flight">His flight.</param>
/// <param name="Reveal">The reveal both encounters open with.</param>
/// <param name="Animation">His body animation and its shared cues.</param>
/// <param name="Contact">What his body does to Samus.</param>
/// <param name="Drawing">The drawing of his tail and wings.</param>
/// <param name="Tail">His tail.</param>
[RoomLifetime]
internal sealed record RidleyBody(
    RidleyFlight Flight, RidleyReveal Reveal, RidleyBodyAnimation Animation, RidleyBodyContact Contact, RidleyDrawing Drawing,
    RidleyTail Tail);
