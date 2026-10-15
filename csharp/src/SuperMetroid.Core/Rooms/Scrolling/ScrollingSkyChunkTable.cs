namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The two bank-$88 scrolling-sky chunk pointer tables.</summary>
public enum ScrollingSkyChunkTable
{
    /// <summary>$88:AD9C, land sky chunk pointers used by the scrolling-sky room main.</summary>
    Land = 0x88ad9c,
    /// <summary>$88:ADA6, ocean sky chunk pointers passed by RoomMainAsm_ScrollingSkyOcean ($88:AF99).</summary>
    Ocean = 0x88ada6,
}
