using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Core.Rendering;

/// <summary>How the X-ray beam shows each room: which of its blends (<see cref="XrayRoomBlendMode"/>) the room takes.</summary>
public static partial class XrayRoomDisplayRules
{
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
