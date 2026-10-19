namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How one of Draygon's body rectangles reacts when a Samus projectile hits it.</summary>
internal enum DraygonHitboxShot
{
    /// <summary>The vulnerable eye rectangle: the hit damages Draygon and steepens later swoops.</summary>
    Eye = 1,
    /// <summary>The claws, tail and shell: the projectile is deflected as a dud.</summary>
    Dud,
}
