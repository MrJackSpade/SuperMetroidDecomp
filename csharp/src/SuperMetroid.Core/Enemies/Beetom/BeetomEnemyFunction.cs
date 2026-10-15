namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// Literal bank-$A8 function pointer stored in Beetom's common variable two. Setup functions
/// intentionally remain separate states: on the cartridge, selecting an action consumes one
/// frame and installing that action consumes another before movement begins.
/// </summary>
public enum BeetomEnemyFunction : ushort
{
    /// <summary><c>$A8:B814 Function_Beetom_DecideAction</c>: selects the near or distant decision entry from a strict 96-pixel horizontal proximity check.</summary>
    DecideAction = 0xb814,
    /// <summary><c>$A8:B82F Function_Beetom_DecideAction_SamusNotInProximity</c>: advances the shared RNG and selects an idle or hop setup, storing the low bit as direction.</summary>
    DecideActionSamusNotInProximity = 0xb82f,
    /// <summary><c>$A8:B84F Function_Beetom_StartIdling</c>: loads timer $20 and enters the stationary countdown on the next gameplay frame.</summary>
    StartIdling = 0xb84f,
    /// <summary><c>$A8:B85F Function_Beetom_StartCrawlingLeft</c>: installs left-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingLeft = 0xb85f,
    /// <summary><c>$A8:B873 Function_Beetom_StartCrawlingRight</c>: installs right-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingRight = 0xb873,
    /// <summary><c>$A8:B887 Function_Beetom_StartShortHopRight</c>: despite the inverted native label, initializes the rising leftward short hop and its art.</summary>
    StartShortHopLeft = 0xb887,
    /// <summary><c>$A8:B8A9 Function_Beetom_StartShortHopLeft</c>: despite the inverted native label, initializes the rising rightward short hop and its art.</summary>
    StartShortHopRight = 0xb8a9,
    /// <summary><c>$A8:B8CB Function_Beetom_StartLongHopLeft</c>: loads the long-hop speed-table index, clears falling, and installs left-facing hop art.</summary>
    StartLongHopLeft = 0xb8cb,
    /// <summary><c>$A8:B8ED Function_Beetom_StartLongHopRight</c>: loads the long-hop speed-table index, clears falling, and installs right-facing hop art.</summary>
    StartLongHopRight = 0xb8ed,
    /// <summary><c>$A8:B90F Function_Beetom_DecideAction_SamusInProximity</c>: aims the rising lunge toward Samus and installs the corresponding hop list.</summary>
    DecideActionSamusInProximity = 0xb90f,
    /// <summary><c>$A8:B952 Function_Beetom_StartDrainingSamus_FacingLeft</c>: installs left-facing attached art before the drain-following routine begins.</summary>
    StartDrainingLeft = 0xb952,
    /// <summary><c>$A8:B966 Function_Beetom_StartDrainingSamus_FacingRight</c>: installs right-facing attached art before the drain-following routine begins.</summary>
    StartDrainingRight = 0xb966,
    /// <summary><c>$A8:B97A Function_Beetom_StartDropping</c>: restores crawl art from the direction word, clears falling, and selects the straight drop.</summary>
    StartDropping = 0xb97a,
    /// <summary><c>$A8:B9A2 Function_Beetom_StartBeingFlung</c>: resets the downward speed-table index to zero after Samus shakes the Beetom off.</summary>
    StartBeingFlung = 0xb9a2,
    /// <summary><c>$A8:B9B2 Function_Beetom_Idling</c>: decrements the native word timer until its signed result is negative, then requests a new action.</summary>
    Idling = 0xb9b2,
    /// <summary><c>$A8:B9C1 Function_Beetom_CrawlingLeft</c>: moves left one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingLeft = 0xb9c1,
    /// <summary><c>$A8:BA24 Function_Beetom_CrawlingRight</c>: moves right one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingRight = 0xba24,
    /// <summary><c>$A8:BA84 Function_Beetom_ShortHopLeft</c>: moves left one quarter pixel per frame while the quadratic vertical index decreases or increases by four.</summary>
    ShortHopLeft = 0xba84,
    /// <summary><c>$A8:BAB7 Function_Beetom_ShortHopRight</c>: moves right one quarter pixel per frame while the quadratic vertical index decreases or increases by four.</summary>
    ShortHopRight = 0xbab7,
    /// <summary><c>$A8:BB55 Function_Beetom_LongHopLeft</c>: moves left one quarter pixel per frame while the quadratic vertical index decreases or increases by five.</summary>
    LongHopLeft = 0xbb55,
    /// <summary><c>$A8:BB88 Function_Beetom_LongHopRight</c>: moves right one quarter pixel per frame while the quadratic vertical index decreases or increases by five.</summary>
    LongHopRight = 0xbb88,
    /// <summary><c>$A8:BC26 Function_Beetom_LungeLeft</c>: lunges left three pixels per frame with vertical-index steps of minus five rising and plus three falling.</summary>
    LungeLeft = 0xbc26,
    /// <summary><c>$A8:BC5A Function_Beetom_LungeRight</c>: lunges right three pixels per frame with vertical-index steps of minus five rising and plus three falling.</summary>
    LungeRight = 0xbc5a,
    /// <summary><c>$A8:BCF8 Function_Beetom_DrainingSamus_FacingLeft</c>: follows Samus until the input-change counter reaches zero, then throws the Beetom right unless blocked.</summary>
    DrainingLeft = 0xbcf8,
    /// <summary><c>$A8:BD42 Function_Beetom_DrainingSamus_FacingRight</c>: follows Samus until the input-change counter reaches zero, then throws the Beetom left unless blocked.</summary>
    DrainingRight = 0xbd42,
    /// <summary><c>$A8:BD9D Function_Beetom_Dropping</c>: falls straight down three pixels per frame until collision selects crawling in the stored direction.</summary>
    Dropping = 0xbd9d,
    /// <summary><c>$A8:BDC5 Function_Beetom_BeingFlung</c>: falls with an increasing quadratic speed index while moving two pixels per frame opposite its pre-escape facing.</summary>
    BeingFlung = 0xbdc5,
}
