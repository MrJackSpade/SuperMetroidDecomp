using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Draygon cannon draw (<see cref="DraygonCannonDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static class DraygonCannonDrawKeys
{
    /// <summary>Each DraygonCannonDraw member's installed key.</summary>
    internal static InstalledKeys<DraygonCannonDraw> Installed { get; } = new(new Dictionary<DraygonCannonDraw, string>
    {
        [DraygonCannonDraw.RightShield] = "right-shield",
        [DraygonCannonDraw.RightShieldFlash] = "right-shield-flash",
        [DraygonCannonDraw.RightSparksFlaring] = "right-sparks-flaring",
        [DraygonCannonDraw.RightSparksSpreading] = "right-sparks-spreading",
        [DraygonCannonDraw.RightSparksReceding] = "right-sparks-receding",
        [DraygonCannonDraw.RightSparksFlat] = "right-sparks-flat",
        [DraygonCannonDraw.LeftShield] = "left-shield",
        [DraygonCannonDraw.LeftShieldFlash] = "left-shield-flash",
        [DraygonCannonDraw.LeftSparksFlaring] = "left-sparks-flaring",
        [DraygonCannonDraw.LeftSparksSpreading] = "left-sparks-spreading",
        [DraygonCannonDraw.LeftSparksReceding] = "left-sparks-receding",
        [DraygonCannonDraw.LeftSparksFlat] = "left-sparks-flat",
    });
}
