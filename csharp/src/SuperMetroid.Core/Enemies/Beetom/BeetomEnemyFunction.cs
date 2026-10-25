namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The routine Beetom's main AI runs each frame. Setup functions intentionally remain separate states: on
/// the cartridge, selecting an action consumes one frame and installing that action consumes another before movement
/// begins.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum BeetomEnemyFunction : ushort
{
    /// <summary><c>$A8:B814 Function_Beetom_DecideAction</c>: selects the near or distant decision entry from a strict 96-pixel horizontal proximity check.</summary>
    DecideAction = 1,
    /// <summary><c>$A8:B82F Function_Beetom_DecideAction_SamusNotInProximity</c>: advances the shared RNG and selects an idle or hop setup, storing the low bit as direction.</summary>
    DecideActionSamusNotInProximity,
    /// <summary><c>$A8:B84F Function_Beetom_StartIdling</c>: loads timer $20 and enters the stationary countdown on the next gameplay frame.</summary>
    StartIdling,
    /// <summary><c>$A8:B85F Function_Beetom_StartCrawlingLeft</c>: installs left-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingLeft,
    /// <summary><c>$A8:B873 Function_Beetom_StartCrawlingRight</c>: installs right-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingRight,
    /// <summary><c>$A8:B887 Function_Beetom_StartShortHopRight</c>: despite the inverted native label, initializes the rising leftward short hop and its art.</summary>
    StartShortHopLeft,
    /// <summary><c>$A8:B8A9 Function_Beetom_StartShortHopLeft</c>: despite the inverted native label, initializes the rising rightward short hop and its art.</summary>
    StartShortHopRight,
    /// <summary><c>$A8:B8CB Function_Beetom_StartLongHopLeft</c>: loads the long-hop speed-table index, clears falling, and installs left-facing hop art.</summary>
    StartLongHopLeft,
    /// <summary><c>$A8:B8ED Function_Beetom_StartLongHopRight</c>: loads the long-hop speed-table index, clears falling, and installs right-facing hop art.</summary>
    StartLongHopRight,
    /// <summary><c>$A8:B90F Function_Beetom_DecideAction_SamusInProximity</c>: aims the rising lunge toward Samus and installs the corresponding hop list.</summary>
    DecideActionSamusInProximity,
    /// <summary><c>$A8:B952 Function_Beetom_StartDrainingSamus_FacingLeft</c>: installs left-facing attached art before the drain-following routine begins.</summary>
    StartDrainingLeft,
    /// <summary><c>$A8:B966 Function_Beetom_StartDrainingSamus_FacingRight</c>: installs right-facing attached art before the drain-following routine begins.</summary>
    StartDrainingRight,
    /// <summary><c>$A8:B97A Function_Beetom_StartDropping</c>: restores crawl art from the direction word, clears falling, and selects the straight drop.</summary>
    StartDropping,
    /// <summary><c>$A8:B9A2 Function_Beetom_StartBeingFlung</c>: resets the downward speed-table index to zero after Samus shakes the Beetom off.</summary>
    StartBeingFlung,
    /// <summary><c>$A8:B9B2 Function_Beetom_Idling</c>: decrements the native word timer until its signed result is negative, then requests a new action.</summary>
    Idling,
    /// <summary><c>$A8:B9C1 Function_Beetom_CrawlingLeft</c>: moves left one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingLeft,
    /// <summary><c>$A8:BA24 Function_Beetom_CrawlingRight</c>: moves right one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingRight,
    /// <summary><c>$A8:BA84 Function_Beetom_ShortHopLeft</c>: moves left one quarter pixel per frame while the quadratic vertical index decreases or increases by four.</summary>
    ShortHopLeft,
    /// <summary><c>$A8:BAB7 Function_Beetom_ShortHopRight</c>: moves right one quarter pixel per frame while the quadratic vertical index decreases or increases by four.</summary>
    ShortHopRight,
    /// <summary><c>$A8:BB55 Function_Beetom_LongHopLeft</c>: moves left one quarter pixel per frame while the quadratic vertical index decreases or increases by five.</summary>
    LongHopLeft,
    /// <summary><c>$A8:BB88 Function_Beetom_LongHopRight</c>: moves right one quarter pixel per frame while the quadratic vertical index decreases or increases by five.</summary>
    LongHopRight,
    /// <summary><c>$A8:BC26 Function_Beetom_LungeLeft</c>: lunges left three pixels per frame with vertical-index steps of minus five rising and plus three falling.</summary>
    LungeLeft,
    /// <summary><c>$A8:BC5A Function_Beetom_LungeRight</c>: lunges right three pixels per frame with vertical-index steps of minus five rising and plus three falling.</summary>
    LungeRight,
    /// <summary><c>$A8:BCF8 Function_Beetom_DrainingSamus_FacingLeft</c>: follows Samus until the input-change counter reaches zero, then throws the Beetom right unless blocked.</summary>
    DrainingLeft,
    /// <summary><c>$A8:BD42 Function_Beetom_DrainingSamus_FacingRight</c>: follows Samus until the input-change counter reaches zero, then throws the Beetom left unless blocked.</summary>
    DrainingRight,
    /// <summary><c>$A8:BD9D Function_Beetom_Dropping</c>: falls straight down three pixels per frame until collision selects crawling in the stored direction.</summary>
    Dropping,
    /// <summary><c>$A8:BDC5 Function_Beetom_BeingFlung</c>: falls with an increasing quadratic speed index while moving two pixels per frame opposite its pre-escape facing.</summary>
    BeingFlung,
}
