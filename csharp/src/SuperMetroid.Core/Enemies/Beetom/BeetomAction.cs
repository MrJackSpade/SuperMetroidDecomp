namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The action Beetom performs each frame. Setup actions intentionally remain separate states: in
/// the game, selecting an action consumes one frame and installing that action consumes another before movement
/// begins.
/// Its values are arbitrary.
/// </summary>
public enum BeetomAction
{
    /// <summary>Selects the near or distant decision entry from a strict 96-pixel horizontal proximity check.</summary>
    DecideAction,
    /// <summary>Advances the shared RNG and selects an idle or hop setup, facing right when the choice is odd.</summary>
    DecideActionSamusNotInProximity,
    /// <summary>Loads a 32-update timer and enters the stationary countdown on the next gameplay frame.</summary>
    StartIdling,
    /// <summary>Installs left-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingLeft,
    /// <summary>Installs right-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingRight,
    /// <summary>Initializes the rising leftward short hop and its art.</summary>
    StartShortHopLeft,
    /// <summary>Initializes the rising rightward short hop and its art.</summary>
    StartShortHopRight,
    /// <summary>Loads the long-hop speed-curve place, clears falling, and installs left-facing hop art.</summary>
    StartLongHopLeft,
    /// <summary>Loads the long-hop speed-curve place, clears falling, and installs right-facing hop art.</summary>
    StartLongHopRight,
    /// <summary>Aims the rising lunge toward Samus and starts the corresponding hop animation.</summary>
    DecideActionSamusInProximity,
    /// <summary>Installs left-facing attached art before the drain-following behaviour begins.</summary>
    StartDrainingLeft,
    /// <summary>Installs right-facing attached art before the drain-following behaviour begins.</summary>
    StartDrainingRight,
    /// <summary>Restores crawl art from the crawl direction, clears falling, and selects the straight drop.</summary>
    StartDropping,
    /// <summary>Resets the downward speed-curve place to zero after Samus shakes the Beetom off.</summary>
    StartBeingFlung,
    /// <summary>Decrements the idle timer until its signed result is negative, then requests a new action.</summary>
    Idling,
    /// <summary>Moves left one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingLeft,
    /// <summary>Moves right one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingRight,
    /// <summary>Moves left one quarter pixel per frame while the vertical speed-curve place decreases or increases by four.</summary>
    ShortHopLeft,
    /// <summary>Moves right one quarter pixel per frame while the vertical speed-curve place decreases or increases by four.</summary>
    ShortHopRight,
    /// <summary>Moves left one quarter pixel per frame while the vertical speed-curve place decreases or increases by five.</summary>
    LongHopLeft,
    /// <summary>Moves right one quarter pixel per frame while the vertical speed-curve place decreases or increases by five.</summary>
    LongHopRight,
    /// <summary>Lunges left three pixels per frame with curve-place steps of minus five rising and plus three falling.</summary>
    LungeLeft,
    /// <summary>Lunges right three pixels per frame with curve-place steps of minus five rising and plus three falling.</summary>
    LungeRight,
    /// <summary>Follows Samus until the input-change counter reaches zero, then throws the Beetom right unless blocked.</summary>
    DrainingLeft,
    /// <summary>Follows Samus until the input-change counter reaches zero, then throws the Beetom left unless blocked.</summary>
    DrainingRight,
    /// <summary>Falls straight down three pixels per frame until collision selects crawling in the stored direction.</summary>
    Dropping,
    /// <summary>Falls with an increasing speed-curve place while moving two pixels per frame opposite its pre-escape facing.</summary>
    BeingFlung,
}
