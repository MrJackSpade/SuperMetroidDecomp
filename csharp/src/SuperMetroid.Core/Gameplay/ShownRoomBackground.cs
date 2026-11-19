namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What the background shows in one gameplay frame (#1346, #1392, #2544): nothing (<see cref="Nothing"/>), the room's
/// background plane (<see cref="RoomPlaneBackground"/>), the scrolling sky's rows (<see cref="SkyBackground"/>), the room's
/// library background image (<see cref="LibraryImageBackground"/>) or the X-ray's reveal (<see cref="XrayRevealBackground"/>).
/// A boss's body is its own layer (<see cref="Rendering.BossBodyView"/>, #2573), with nothing behind it. The frame boundary latches one of them and the X-ray's capture
/// replaces it with the reveal, so a backend draws exactly the one the frame shows.
/// </summary>
public abstract record ShownRoomBackground
{
    /// <summary>Only the kinds of background below derive from this.</summary>
    private protected ShownRoomBackground()
    {
    }

    /// <summary>
    /// The background of a frame whose background shows nothing: a room whose library background is cleared to nothing
    /// (Kraid's room once he is dead), a boss's room before the boss shows its body, or behind a boss's body.
    /// </summary>
    public static ShownRoomBackground Nothing { get; } = new NoRoomBackground();

    /// <summary>The value <paramref name="mapping"/> gives this kind of background.</summary>
    /// <typeparam name="TResult">What each kind of background maps to.</typeparam>
    /// <param name="mapping">What each kind of background maps to.</param>
    public abstract TResult Map<TResult>(IShownRoomBackgroundMapping<TResult> mapping);
}
