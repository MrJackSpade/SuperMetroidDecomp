namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The fractions of a pixel the camera keeps from one game to the next (#1776). Each game, attract demo or continued game
/// builds its own camera, but the first room's camera starts from the fractions the camera of the game before left: no
/// room or game load sets them, only the camera's own following of Samus and its scroll-boundary stops. The camera follows
/// Samus by adding her movement, fraction and all, so a kept fraction decides on which update the camera reaches its next
/// whole pixel, and with it what the screen shows and what the update's camera-relative checks see.
/// </summary>
/// <remarks>
/// Kept when the session releases or replaces a gameplay runtime (game over, the saved-game map, a continued game, the
/// end of an attract demo, the start of a game or demo over a running one), from that runtime's camera if a room was
/// loaded; consulted once per runtime, when its first room's camera is placed during the room load, before the room's
/// first update; cleared by the soft reset and power-on. Door loads within a game carry the outgoing camera's own
/// fractions and do not consult the service. Evidence: the camera's fractions are written only by Samus's scrolling
/// (<c>$90:95A0</c>, <c>$90:964F</c>, which add her movement with its fraction) and by the four scroll-boundary stops
/// (<c>$80:A641</c>, <c>$80:A6BB</c>, <c>$80:A893</c>, <c>$80:A936</c>, which clear the fraction when the camera is held
/// at its ideal position); the station and room loads (<c>$80:C470</c>, <c>$80:C479</c>) write only the whole camera
/// position; and the reset vector (<c>$80:841C</c>) is the only routine that clears that memory, so every later game
/// starts with the fractions the one before left. The Samus drunk movie's game-over reload confirms it (#1275), and the
/// Ceres save startup check reloads a game over with the dead game's fractions.
/// </remarks>
[CompatibilityService("a game, demo or continued game starts its first room's camera from the fractions of a pixel the previous one's camera left, which decide on which update the camera following Samus reaches each next whole pixel")]
public sealed class CameraFractionsKeptAcrossGames
{
    /// <summary>The kept fractions, on a position of zero whole pixels; none until a game leaves its camera's.</summary>
    public FixedPosition<RoomPixels> Fractions { get; private set; }

    /// <summary>Keeps the fractions of a released game's camera; its whole pixels are not kept.</summary>
    /// <param name="cameraPosition">The released game's camera position.</param>
    public void Keep(FixedPosition<RoomPixels> cameraPosition) =>
        Fractions = FixedPosition<RoomPixels>.Zero.WithFractionsOf(cameraPosition);

    /// <summary>Clears the fractions, as the soft reset and power-on do.</summary>
    public void Clear() => Fractions = FixedPosition<RoomPixels>.Zero;
}
