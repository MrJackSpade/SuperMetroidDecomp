using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Core.Rendering;

/// <summary>How the X-ray beam shows each room: which rooms it reveals blocks in, and how it blends them.</summary>
public static class XrayRoomDisplayRules
{
    /// <summary>True only for the room where X-ray, keeping the room's backgrounds, also hides the background layer.</summary>
    public static bool RemovesBackground(RoomDefinition room) => room == RoomDefinitions.GlassTunnel;
    /// <summary>The dark grey backdrop shown while the X-ray beam is out.</summary>
    public static RgbColor ActiveBackdrop => new(24, 24, 24);

    /// <summary>
    /// What the X-ray blends outside its cone (#1514): in a reveal room and Fireflea's room, the foreground, the background,
    /// the actors and the backdrop; in an excluded room, the foreground and the backdrop.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined mode.</exception>
    public static BlendedLayers Blended(XrayRoomBlendMode mode) => mode switch
    {
        XrayRoomBlendMode.RevealBlocks or XrayRoomBlendMode.Fireflea =>
            BlendedLayers.Foreground | BlendedLayers.Background | BlendedLayers.Actors | BlendedLayers.Backdrop,
        XrayRoomBlendMode.PreserveBackgrounds => BlendedLayers.Foreground | BlendedLayers.Backdrop,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Undefined X-ray room blend."),
    };

    /// <summary>
    /// Whether the X-ray brightens or darkens the room outside its cone: Fireflea's room always darkens it; the others keep
    /// the room's own liquid's darkening (<paramref name="roomDarkens"/>) or brightening.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined mode.</exception>
    public static BlendOperation Operation(XrayRoomBlendMode mode, bool roomDarkens) => mode switch
    {
        XrayRoomBlendMode.Fireflea => BlendOperation.Subtract,
        XrayRoomBlendMode.RevealBlocks or XrayRoomBlendMode.PreserveBackgrounds => roomDarkens ? BlendOperation.Subtract : BlendOperation.Add,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Undefined X-ray room blend."),
    };

    /// <summary>
    /// Whether the X-ray halves the room outside its cone toward what it combines with: every room but Fireflea's, whose
    /// darkness subtracts its shade whole.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined mode.</exception>
    public static bool Halves(XrayRoomBlendMode mode) => mode switch
    {
        XrayRoomBlendMode.Fireflea => false,
        XrayRoomBlendMode.RevealBlocks or XrayRoomBlendMode.PreserveBackgrounds => true,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Undefined X-ray room blend."),
    };

    /// <summary>A room with Fireflea darkness takes its own blend before the ordinary room and boss exclusions apply.</summary>
    /// <param name="room">The room the X-ray shows.</param>
    /// <param name="fireflea">The room's Fireflea darkness, or null in a room without it.</param>
    /// <param name="boss">The room's loaded boss, or null.</param>
    public static XrayRoomBlendMode Select(RoomDefinition room, LatchedFirefleaDarkness? fireflea, AreaBoss? boss)
    {
        if (fireflea is not null) return XrayRoomBlendMode.Fireflea;
        if (!XrayBlockRevealRules.RevealsBlocks(room, boss))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
