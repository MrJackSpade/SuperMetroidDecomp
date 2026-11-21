namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Samus's air-bubble test and diving splash behave in a room with no water surface (#1546, #2558). Leaving water for
/// a room configured without a water surface (a lava or acid room, or water that names none) still runs the exit splash
/// and the bubble test, which then behave as though the surface lay on the line just above the room's top: a mouth
/// anywhere but two or more pixels above the room's top counts as under water, so on an update whose frame counter admits
/// a bubble and whose bubble slot is free, the bubble spawns, draws a random number and requests its sound; and the
/// diving splash stands on that line, counted back from the far end of the room's coordinates.
/// </summary>
/// <remarks>
/// Consulted whenever the bubble test runs, during Samus's animation's liquid handling on entering, staying in or leaving
/// water, and whenever the diving splash is placed or pinned to the surface as it animates, in a room with no water
/// surface. Evidence: the bubble and splash routines compare with and copy the room effect's surface word without testing
/// its sign, and a room effect that names no surface stores the word 65535, which a signed comparison reads as the line
/// above the room.
/// </remarks>
[CompatibilityService("Samus's air-bubble test and diving splash in a room with no water surface behave as though the surface lay just above the room")]
public static partial class AbsentWaterSurfaceLine
{
    /// <summary>The line the game's tests find a missing surface on: one pixel above the room's top.</summary>
    private const int LineAboveRoom = -1;

    /// <summary>
    /// Whether a mouth on <paramref name="mouthY"/> counts as under the water surface a room does not have: anywhere but
    /// two or more pixels above the room's top.
    /// </summary>
    /// <param name="mouthY">The room line of Samus's mouth.</param>
    public static bool MouthIsUnder(int mouthY) => mouthY >= LineAboveRoom;

    /// <summary>
    /// The room place the diving splash stands on in a room with no water surface: the line above the room, counted back
    /// from the far end of the room's coordinates (<see cref="OffRoomPlaces"/>), 65535.
    /// </summary>
    public static int SplashPlace => OffRoomPlaces.Place(LineAboveRoom);
}
