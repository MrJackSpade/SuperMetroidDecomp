using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How the X-ray beam blends a room outside its cone (#1514); each room takes one (<see cref="XrayRoomDisplayRules.Select"/>):
/// what it blends, whether it brightens or darkens, whether it halves, and whether the room's own darkness stays.
/// </summary>
public sealed record XrayRoomBlendMode
{
    /// <summary>Closes the blends to the three declared here.</summary>
    private XrayRoomBlendMode()
    {
    }

    /// <summary>Inside the beam, blocks show what they really are; outside it the whole room blends with a grey tint.</summary>
    public static XrayRoomBlendMode RevealBlocks { get; } = new()
    {
        RevealsBlocks = true,
        Blended = BlendedLayers.Foreground | BlendedLayers.Background | BlendedLayers.Actors | BlendedLayers.Backdrop,
        Halves = true,
        TintLevel = XrayWindowRenderDefinitions.RevealTintLevel,
    };

    /// <summary>The room's backgrounds stay as they are and blocks are not revealed; only the foreground and backdrop blend.</summary>
    public static XrayRoomBlendMode PreserveBackgrounds { get; } = new()
    {
        KeepsBackgrounds = true,
        Blended = BlendedLayers.Foreground | BlendedLayers.Backdrop,
        Halves = true,
    };

    /// <summary>Fireflea's darkness stays outside the beam: the whole room darkens with its shade, whole.</summary>
    public static XrayRoomBlendMode Fireflea { get; } = new()
    {
        Blended = BlendedLayers.Foreground | BlendedLayers.Background | BlendedLayers.Actors | BlendedLayers.Backdrop,
        ForcedOperation = BlendOperation.Subtract,
        KeepsDarkness = true,
        TintLevel = XrayWindowRenderDefinitions.RevealTintLevel,
    };

    /// <summary>Whether blocks inside the beam show what they really are.</summary>
    public bool RevealsBlocks { get; private init; }

    /// <summary>
    /// Whether the room's backgrounds stay as they are, so a room that hides its background under X-ray
    /// (<see cref="Rooms.RoomDefinition.XrayHidesBackground"/>) hides it in this blend.
    /// </summary>
    public bool KeepsBackgrounds { get; private init; }

    /// <summary>What the X-ray blends outside its cone.</summary>
    public BlendedLayers Blended { get; private init; }

    /// <summary>The operation the blend always takes, whatever the room's liquid does; null where it follows the room's own.</summary>
    public BlendOperation? ForcedOperation { get; private init; }

    /// <summary>Whether the X-ray halves the room outside its cone toward what it combines with.</summary>
    public bool Halves { get; private init; }

    /// <summary>
    /// Whether the room's own darkness stays outside the beam: the room blends with its shade alone, not its liquid, the
    /// tint is never lighter than that shade, and the beam's last pass keeps it.
    /// </summary>
    public bool KeepsDarkness { get; private init; }

    /// <summary>
    /// The fade level, in every channel, of the X-ray's own grey tint, zero for none; a blend that keeps the darkness takes
    /// the darkness's shade where that is darker.
    /// </summary>
    public int TintLevel { get; private init; }

    /// <summary>
    /// Whether the X-ray brightens or darkens the room outside its cone: the forced operation, or the room's own liquid's
    /// darkening (<paramref name="roomDarkens"/>) or brightening.
    /// </summary>
    /// <param name="roomDarkens">Whether the room's own effect darkens what it blends.</param>
    public BlendOperation Operation(bool roomDarkens) =>
        ForcedOperation ?? (roomDarkens ? BlendOperation.Subtract : BlendOperation.Add);
}
