namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How the X-ray beam dims a room outside its cone (#1514); each room takes one (<see cref="XrayRoomDisplayRules.Select"/>):
/// a grey wash over the whole room, the room's backgrounds kept as they are, or the room's own darkness kept.
/// </summary>
public sealed record XrayRoomBlendMode
{
    /// <summary>Closes the dimmings to the three declared here.</summary>
    private XrayRoomBlendMode()
    {
    }

    /// <summary>
    /// Inside the beam, blocks show what they really are; outside it the whole room, its actors and its backdrop wash grey,
    /// halfway toward its liquid or fog.
    /// </summary>
    public static XrayRoomBlendMode RevealBlocks { get; } = new()
    {
        RevealsBlocks = true,
        WashesGrey = true,
    };

    /// <summary>
    /// The room's backgrounds and actors stay as they are and blocks are not revealed; only the foreground and the backdrop
    /// dim, halfway toward the room's liquid or fog.
    /// </summary>
    public static XrayRoomBlendMode PreserveBackgrounds { get; } = new()
    {
        KeepsBackgrounds = true,
    };

    /// <summary>Fireflea's darkness stays outside the beam: the whole room, its actors and its backdrop darken with its shade, whole.</summary>
    public static XrayRoomBlendMode Fireflea { get; } = new()
    {
        KeepsDarkness = true,
        WashesGrey = true,
    };

    /// <summary>Whether blocks inside the beam show what they really are.</summary>
    public bool RevealsBlocks { get; private init; }

    /// <summary>
    /// Whether the room's backgrounds and actors stay as they are, so a room that hides its background under X-ray
    /// (<see cref="Rooms.RoomDefinition.XrayHidesBackground"/>) hides it in this dimming.
    /// </summary>
    public bool KeepsBackgrounds { get; private init; }

    /// <summary>
    /// Whether the room's own darkness stays outside the beam: the room darkens with its shade alone, whole, not toward its
    /// liquid, the tint is never lighter than that shade, and the beam's last pass keeps it.
    /// </summary>
    public bool KeepsDarkness { get; private init; }

    /// <summary>
    /// Whether the room outside the cone takes the X-ray's grey (<see cref="XrayWindowRenderDefinitions.RevealTintLevel"/>);
    /// a darkness it keeps takes its shade where that is darker.
    /// </summary>
    public bool WashesGrey { get; private init; }
}
