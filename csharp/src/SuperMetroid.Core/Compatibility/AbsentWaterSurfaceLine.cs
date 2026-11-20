using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where Samus's air-bubble test and diving splash find a water surface the room does not have (#1546): on the line just
/// above the room's top. Leaving water for a room configured without a water surface (a lava or acid room, or water that
/// names none) still runs the exit splash and the bubble test against the surface, so a mouth anywhere in the room lies
/// under that line: on an update whose frame counter admits a bubble and whose bubble slot is free, the bubble spawns,
/// draws a random number and requests its sound, and the diving splash sits on the line.
/// </summary>
/// <remarks>
/// Consulted whenever the bubble test runs, during Samus's animation's liquid handling on entering, staying in or leaving
/// water, and whenever the diving splash is placed or pinned to the surface as it animates. Evidence: the bubble and
/// splash routines compare with and copy the room effect's surface word without testing its sign, and a room effect
/// that names no surface stores the word 65535, which a signed comparison reads as the line above the room.
/// </remarks>
[CompatibilityService("Samus's air-bubble test and diving splash read a missing water surface as the line just above the room")]
public static class AbsentWaterSurfaceLine
{
    /// <summary>The line an absent water surface is read at: one pixel above the room's top.</summary>
    private const int LineAboveRoom = -1;

    /// <summary>The surface line the bubble test and the diving splash read: <paramref name="surface"/>, or the line above the room.</summary>
    /// <param name="surface">The room's water surface, or none.</param>
    [SuppressMessage("Design", "SME6476", Justification = "The line above the room is the real line the game's tests read for a room with no water surface; callers keep the surface optional, and this compatibility service is where its absence is read as that line.")]
    public static int Read(int? surface) => surface ?? LineAboveRoom;

    /// <summary>
    /// The surface line as a place in the room, where the diving splash stands: <paramref name="surface"/>, or the line
    /// above the room counted back from the far end of the room's coordinates (<see cref="OffRoomPlaces"/>), 65535.
    /// </summary>
    /// <param name="surface">The room's water surface, or none.</param>
    public static int PlaceOf(int? surface) => OffRoomPlaces.Place(Read(surface));
}
