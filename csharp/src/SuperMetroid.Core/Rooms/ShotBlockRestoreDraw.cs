namespace SuperMetroid.Core.Rooms;

/// <summary>The three bank-$84 shot-block collision-parent restoration draw lists, valued by native address.</summary>
internal enum ShotBlockRestoreDraw : ushort
{
    /// <summary>Final two-block horizontal collision-parent restoration, <c>$84:A47B</c>.</summary>
    Horizontal = 0xa47b,
    /// <summary>Final two-block vertical collision-parent restoration, <c>$84:A483</c>.</summary>
    Vertical = 0xa483,
    /// <summary>Final four-block square collision-parent restoration, <c>$84:A48B</c>.</summary>
    Square = 0xa48b,
}
