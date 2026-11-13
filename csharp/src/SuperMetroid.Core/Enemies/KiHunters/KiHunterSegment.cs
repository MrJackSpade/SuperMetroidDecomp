namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="KiHunterAnimations"/>.
/// </summary>
internal enum KiHunterSegment
{
    /// <summary>The flying left animation.</summary>
    FlyingLeft,
    /// <summary>The swoop left animation.</summary>
    SwoopLeft,
    /// <summary>The flying right animation.</summary>
    FlyingRight,
    /// <summary>The swoop right animation.</summary>
    SwoopRight,
    /// <summary>The wings left animation.</summary>
    WingsLeft,
    /// <summary>The wings right animation.</summary>
    WingsRight,
    /// <summary>The detached wings animation.</summary>
    DetachedWings,
    /// <summary>The jump left animation.</summary>
    JumpLeft,
    /// <summary>The jump right animation.</summary>
    JumpRight,
    /// <summary>The land left animation.</summary>
    LandLeft,
    /// <summary>The land right animation.</summary>
    LandRight,
    /// <summary>The spit left animation.</summary>
    SpitLeft,
    /// <summary>The spit right animation.</summary>
    SpitRight,
}
