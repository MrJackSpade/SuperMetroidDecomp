namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The five draws of a breakable Grapple block: the intact block, its three breakup frames and the cleared air.</summary>
internal enum GrappleBlockDraw
{
    /// <summary>The intact block, with Grapple collision.</summary>
    Intact,
    /// <summary>The first breakup frame, already air.</summary>
    Break0,
    /// <summary>The second breakup frame.</summary>
    Break1,
    /// <summary>The third breakup frame.</summary>
    Break2,
    /// <summary>The cleared block: blank air.</summary>
    Blank,
}
