namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The action Beetom performs each frame. Setup actions intentionally remain separate states: in
/// the game, choosing an action takes one frame and starting that action takes another before movement
/// begins. An action that goes one way goes the way <see cref="BeetomEnemyState.Heading"/> names (#2277).
/// Its values are arbitrary.
/// </summary>
public enum BeetomAction
{
    /// <summary>Selects the near or distant decision entry from a strict 96-pixel horizontal proximity check.</summary>
    DecideAction,
    /// <summary>Advances the shared RNG and selects an idle or hop setup, facing right when the choice is odd.</summary>
    DecideActionSamusNotInProximity,
    /// <summary>Loads the idle's wait and enters the stationary countdown on the next gameplay frame.</summary>
    StartIdling,
    /// <summary>Installs crawling art for its heading and selects movement without moving on this setup frame.</summary>
    StartCrawling,
    /// <summary>Initializes the rising short hop toward its heading and its art.</summary>
    StartShortHop,
    /// <summary>Loads the long-hop speed-curve place, clears falling, and installs hop art for its heading.</summary>
    StartLongHop,
    /// <summary>Aims the rising lunge toward Samus and starts the corresponding hop animation.</summary>
    DecideActionSamusInProximity,
    /// <summary>Installs attached art on its heading's side before the drain-following behaviour begins.</summary>
    StartDraining,
    /// <summary>Restores crawl art from the crawl direction, clears falling, and selects the straight drop.</summary>
    StartDropping,
    /// <summary>Resets the downward speed-curve place to zero after Samus shakes the Beetom off.</summary>
    StartBeingFlung,
    /// <summary>Waits out the idle (<see cref="BeetomMotionDefinitions.IdleDuration"/>), then requests a new action.</summary>
    Idling,
    /// <summary>Moves one quarter pixel per frame toward its heading, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    Crawling,
    /// <summary>Moves one quarter pixel per frame toward its heading while the vertical speed-curve place decreases or increases by four.</summary>
    ShortHop,
    /// <summary>Moves one quarter pixel per frame toward its heading while the vertical speed-curve place decreases or increases by five.</summary>
    LongHop,
    /// <summary>Lunges three pixels per frame toward its heading with curve-place steps of minus five rising and plus three falling.</summary>
    Lunge,
    /// <summary>Follows Samus on its heading's side until the input-change counter reaches zero, then throws the Beetom the other way unless blocked.</summary>
    Draining,
    /// <summary>Falls straight down three pixels per frame until collision selects crawling in the stored direction.</summary>
    Dropping,
    /// <summary>Falls with an increasing speed-curve place while moving two pixels per frame opposite its pre-escape facing.</summary>
    BeingFlung,
}
