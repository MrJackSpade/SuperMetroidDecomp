using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Rendering;

/// <summary>How the X-ray beam shows each room: which rooms it reveals blocks in, and how it blends them.</summary>
public static class XrayRoomDisplayRules
{
    /// <summary>True only for the room where X-ray, keeping the room's backgrounds, also hides the background layer.</summary>
    public static bool RemovesBackground(RoomHeaderDefinition room) => room == RoomHeaderDefinitions.GlassTunnel;
    /// <summary>The dark grey backdrop shown while the X-ray beam is out.</summary>
    public static RgbColor ActiveBackdrop => new(3, 3, 3);

    /// <summary>
    /// How X-ray blends the room: a reveal room and Fireflea's room blend both
    /// backgrounds, the sprites and the backdrop, an excluded room the foreground and the backdrop; Fireflea's room
    /// always subtracts unhalved, the others halve and keep the room's own subtraction.
    /// </summary>
    public static ColorBlend Blend(XrayRoomBlendMode mode, bool subtract)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        var sources = ColorBlend.Bg1 | ColorBlend.Backdrop;
        if (mode is XrayRoomBlendMode.RevealBlocks or XrayRoomBlendMode.Fireflea)
            sources |= ColorBlend.Bg2 | ColorBlend.Sprites;
        if (mode == XrayRoomBlendMode.Fireflea) return sources | ColorBlend.Subtract;
        return sources | ColorBlend.Half | (subtract ? ColorBlend.Subtract : 0);
    }

    /// <summary>Fireflea's room takes its own blend before the ordinary room and boss exclusions apply.</summary>
    public static XrayRoomBlendMode Select(RoomHeaderDefinition room, RoomFxType fx, AreaBoss? boss)
    {
        if (fx == RoomFxType.Fireflea) return XrayRoomBlendMode.Fireflea;
        if (!XrayBlockRevealRules.RevealsBlocks(room, boss))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
