using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Room and enemy definition values used by CanXrayShowBlocks ($91:D143).</summary>
public static class XrayRoomDisplayRules
{
    /// <summary>True only for the excluded room whose preserved display also drops BG2 ($88:81A4).</summary>
    public static bool RemovesBg2(RoomHeaderDefinition room) => room == RoomHeaderDefinitions.GlassTunnel;
    /// <summary>$91:D2BC installs RGB5(3,3,3) as CGRAM entry zero after setup.</summary>
    public static RgbColor ActiveBackdrop => new(3, 3, 3);

    /// <summary>
    /// How X-ray blends the room ($88:817B, $88:81A4 and $88:81DB): a reveal room and Fireflea's room blend both
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

    /// <summary>Preserves the native Fireflea precedence over the ordinary room/boss exclusions.</summary>
    public static XrayRoomBlendMode Select(RoomHeaderDefinition room, RoomFxType fx, int bossId)
    {
        if (fx == RoomFxType.Fireflea) return XrayRoomBlendMode.Fireflea;
        if (!XrayBlockRevealRules.RevealsBlocks(room, bossId))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
