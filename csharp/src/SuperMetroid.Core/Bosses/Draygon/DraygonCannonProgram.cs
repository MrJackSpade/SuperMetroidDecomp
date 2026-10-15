namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's cannon PLM programs (#1296), one set per facing, in place of their bank-$84 lists.</summary>
internal enum DraygonCannonProgram
{
    /// <summary>Links the hit program and arms the missile wake (<c>InstList_PLM_DraygonCannonWithShieldFacingRight_0</c>).</summary>
    RightStart,
    /// <summary>Shows the shield and sleeps (<c>..FacingRight_1</c>).</summary>
    RightIdle,
    /// <summary>Counts a hit, flashes the shield and returns to idle (<c>..FacingRight_2</c>).</summary>
    RightHit,
    /// <summary>Destroys the cannon (<c>InstList_PLM_DraygonCannonFacingRight_0</c>).</summary>
    RightDestroy,
    /// <summary>The destroyed cannon's smoke loop (<c>InstList_PLM_DraygonCannonFacingRight_1</c>).</summary>
    RightSmoke,
    /// <summary>Links the hit program and arms the missile wake (<c>InstList_PLM_DraygonCannonWithShieldFacingLeft_0</c>).</summary>
    LeftStart,
    /// <summary>Shows the shield and sleeps (<c>..FacingLeft_1</c>).</summary>
    LeftIdle,
    /// <summary>Counts a hit, flashes the shield and returns to idle (<c>..FacingLeft_2</c>).</summary>
    LeftHit,
    /// <summary>Destroys the cannon (<c>InstList_PLM_DraygonCannonFacingLeft_0</c>).</summary>
    LeftDestroy,
    /// <summary>The destroyed cannon's smoke loop (<c>InstList_PLM_DraygonCannonFacingLeft_1</c>).</summary>
    LeftSmoke,
}
