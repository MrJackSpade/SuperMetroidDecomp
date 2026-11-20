namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animation segments of Bomb and Golden Torizo's drool, explosive swipes, statue fragments, flying sonic booms and landing dust (#641, #1788, #2235, #2554): each animation begins at the
/// segment named for what it shows; the runs it repeats, loops on or moves on to are segments of their own
/// (<see cref="TorizoProjectileAnimations"/>).
/// </summary>
internal enum TorizoProjectileSegment
{
    /// <summary>
    /// Bomb Torizo's drool forming, unseen for two updates: a drool that forms plays it once or twice before
    /// <see cref="DroolNoDelay"/>'s release and fall (<see cref="TorizoProjectileAnimations.DroolFormingShort"/>,
    /// <see cref="TorizoProjectileAnimations.DroolFormingLong"/>).
    /// </summary>
    DroolForming,
    /// <summary>Bomb Torizo's drool released at once: it starts falling, harmless, then shows its falling pose for good.</summary>
    DroolNoDelay,
    /// <summary>A released drool drop's falling pose, held while it falls.</summary>
    DroolFalling,
    /// <summary>A drool drop reaching a wall: it stops and is deleted.</summary>
    DroolWallImpact,
    /// <summary>A drool drop splashing on the floor: it stops, plays its three splash poses, then is deleted.</summary>
    DroolFloorImpact,
    /// <summary>Torizo's explosive swipe: its sound and five poses, then deletion.</summary>
    ExplosiveSwipe,
    /// <summary>The breaking statue hand's top middle piece (#2240): it rests 128 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentTopMiddle,
    /// <summary>The top right piece: it rests 120 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentTopRight,
    /// <summary>The middle left piece: it rests 112 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentMiddleLeft,
    /// <summary>The centre piece: it rests 104 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentCentre,
    /// <summary>The middle right piece: it rests 96 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentMiddleRight,
    /// <summary>The bottom left piece: it rests 88 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentBottomLeft,
    /// <summary>The bottom middle piece: it rests 80 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentBottomMiddle,
    /// <summary>The bottom right piece: it rests 72 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentBottomRight,
    /// <summary>A sonic boom thrown left: its sound and two firing poses, then its flying pose for good.</summary>
    SonicBoomFiredLeft,
    /// <summary>A sonic boom flying left, its flying pose held.</summary>
    SonicBoomFlyingLeft,
    /// <summary>A sonic boom thrown right: its sound and two firing poses, then its flying pose for good.</summary>
    SonicBoomFiredRight,
    /// <summary>A sonic boom flying right, its flying pose held.</summary>
    SonicBoomFlyingRight,
    /// <summary>The dust Torizo's right foot raises on landing: its four poses, rising, then deletion.</summary>
    LandingDustRightFoot,
    /// <summary>The dust Torizo's left foot raises on landing: its four poses, rising, then deletion.</summary>
    LandingDustLeftFoot,
}
