namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// Literal bank-$A8 function pointers stored in Ki-Hunter variable A. Keeping the ROM
/// addresses as enum values makes a debugger watch directly comparable with enemy WRAM.
/// </summary>
public enum KiHunterEnemyFunction : ushort
{
    /// <summary><c>$A8:F268 Function_Kihunter_Winged_IdleFlying</c>: patrols within the spawn-height band, reflects movement on collision, and starts a swoop toward nearby Samus below.</summary>
    FlyingPatrol = 0xf268,
    /// <summary><c>$A8:F3B8 Function_Kihunter_Winged_Swoop</c>: follows an accelerating elliptic dive, changing art at the swipe angle and backing off on collision.</summary>
    Swooping = 0xf3b8,
    /// <summary><c>$A8:F4ED Function_Kihunter_Winged_BackOff</c>: moves upward away from a blocked dive until collision or passage above the spawn Y restores patrol.</summary>
    RecoveringFromSwoop = 0xf4ed,
    /// <summary><c>$A8:F55A Function_Kihunter_Wingless_InitialFalling</c>: applies gravity after clipping wings or a ground-bound spawn, then prepares a hop on landing.</summary>
    FallingAfterWingLoss = 0xf55a,
    /// <summary><c>$A8:F58B Function_Kihunter_Wingless_PrepareToHop</c>: aims a two-pixel-per-frame hop toward Samus with an RNG-selected initial upward speed of seven or eight pixels per frame.</summary>
    StartGroundJump = 0xf58b,
    /// <summary><c>$A8:F5E3 RTL_A8F5E3</c>: leaves movement to the jump, landing, or spit animation until its instruction stream selects the next function.</summary>
    NoOp = 0xf5e3,
    /// <summary><c>$A8:F5F0 Function_Kihunter_Wingless_Hop</c>: advances the collision-bearing ballistic hop and starts the landing animation after downward collision.</summary>
    GroundJump = 0xf5f0,
    /// <summary><c>$A8:F68B Function_Kihunter_Wingless_Thinking</c>: counts down the post-landing or post-spit wait, then spits within 96 horizontal pixels or prepares another hop.</summary>
    GroundWait = 0xf68b,
    /// <summary><c>$A8:F6B3 Function_Kihunter_Wingless_FireAcidSpit</c>: installs the acid-spit list facing Samus and suspends movement while bytecode owns the attack.</summary>
    SelectAcidSpit = 0xf6b3,
    /// <summary><c>$A8:F6F3 Function_KihunterWings_Attached</c>: copies X and Y from the physically preceding body slot, native enemy minus $40.</summary>
    FollowBody = 0xf6f3,
    /// <summary><c>$A8:F7CF Function_KihunterWings_Falling</c>: dispatches the detached wing's secondary arc function stored in its extended variable zero.</summary>
    DetachedWing = 0xf7cf,
}
