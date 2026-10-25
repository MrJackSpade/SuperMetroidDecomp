namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The two scrolling-sky chunk tables, which pick the sky page each 256-pixel band of the sky shows.</summary>
public enum ScrollingSkyChunkTable
{
    /// <summary>The land sky, which the scrolling-sky room main uses.</summary>
    Land = 1,
    /// <summary>The ocean sky, which the ocean scrolling-sky room main uses.</summary>
    Ocean,
}
