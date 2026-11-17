namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The action Beetom performs each frame. Setup actions intentionally remain separate states: in
/// the game, choosing an action takes one frame and starting that action takes another before movement
/// begins. An action that goes one way goes the way <see cref="BeetomEnemyState.Heading"/> names (#2277).
/// Its values are arbitrary.
/// </summary>
public enum BeetomAction
{
    /// <summary>Decides between lunging at Samus and its own idling, crawling or hopping by whether she is horizontally closer than 96 pixels.</summary>
    DecideAction,
    /// <summary>Takes a random number that chooses idling, crawling or hopping, and faces right on an odd number.</summary>
    DecideActionSamusNotInProximity,
    /// <summary>Starts the idle's wait, which the Beetom counts down standing still from the next update.</summary>
    StartIdling,
    /// <summary>Starts the crawling animation for its heading and crawls from the next update.</summary>
    StartCrawling,
    /// <summary>Starts the short hop rising toward its heading, with its animation.</summary>
    StartShortHop,
    /// <summary>Starts the long hop rising from its place on the speed curve, with the hop animation for its heading.</summary>
    StartLongHop,
    /// <summary>Aims the rising lunge toward Samus and starts the corresponding hop animation.</summary>
    DecideActionSamusInProximity,
    /// <summary>Starts the clinging animation on its heading's side, then follows Samus as it drains her.</summary>
    StartDraining,
    /// <summary>Starts the crawling animation for its crawl direction and drops straight down.</summary>
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
