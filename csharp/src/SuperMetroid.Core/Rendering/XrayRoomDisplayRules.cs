using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Rendering;

/// <summary>How the X-ray beam shows each room: which of its blends (<see cref="XrayRoomBlendMode"/>) the room takes.</summary>
public static class XrayRoomDisplayRules
{
    /// <summary>Fireflea's room takes its own blend before the ordinary room and boss exclusions apply.</summary>
    public static XrayRoomBlendMode Select(RoomDefinition room, RoomFxType fx, AreaBoss? boss)
    {
        if (fx == RoomFxType.Fireflea) return XrayRoomBlendMode.Fireflea;
        if (!XrayBlockRevealRules.RevealsBlocks(room, boss))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
