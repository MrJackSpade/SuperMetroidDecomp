namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The action Beetom performs each frame. Setup actions intentionally remain separate states: in
/// the game, choosing an action takes one frame and starting that action takes another before movement
/// begins. An action that goes one way goes the way <c>Heading</c> names (#2277).
/// Its values are arbitrary.
/// </summary>
public enum BeetomAction
{
    /// <summary>Decides between lunging at Samus and its own idling, crawling or hopping by whether she is horizontally closer than 96 pixels.</summary>
    DecideAction,
    /// <summary>Takes a random number that chooses idling, crawling or hopping and the way it faces (<see cref="BeetomDecisions.FacesRight"/>).</summary>
    ChooseIdleCrawlOrHop,
    /// <summary>Starts the idle's wait, which the Beetom counts down standing still from the next update.</summary>
    StartIdling,
    /// <summary>Starts the crawling animation for its heading and crawls from the next update.</summary>
    StartCrawling,
    /// <summary>Starts the short hop rising toward its heading, with its animation.</summary>
    StartShortHop,
    /// <summary>Starts the long hop rising from its place on the speed curve, with the hop animation for its heading.</summary>
    StartLongHop,
    /// <summary>Aims the rising lunge toward Samus and starts the corresponding hop animation.</summary>
    StartLunge,
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
    /// <summary>
    /// The short hop's rise (#2531): moves one quarter pixel per frame toward its heading while the vertical speed-curve
    /// place decreases by four, and falls once it passes the apex.
    /// </summary>
    ShortHopRising,
    /// <summary>
    /// The short hop's fall: moves one quarter pixel per frame toward its heading while the vertical speed-curve place
    /// increases by four.
    /// </summary>
    ShortHopFalling,
    /// <summary>
    /// The long hop's rise (#2531): moves one quarter pixel per frame toward its heading while the vertical speed-curve
    /// place decreases by five, and falls once it passes the apex.
    /// </summary>
    LongHopRising,
    /// <summary>
    /// The long hop's fall: moves one quarter pixel per frame toward its heading while the vertical speed-curve place
    /// increases by five.
    /// </summary>
    LongHopFalling,
    /// <summary>
    /// The lunge's rise (#2531): lunges three pixels per frame toward its heading while the vertical speed-curve place
    /// decreases by five, and falls once it passes the apex.
    /// </summary>
    LungeRising,
    /// <summary>
    /// The lunge's fall: lunges three pixels per frame toward its heading while the vertical speed-curve place increases
    /// by three.
    /// </summary>
    LungeFalling,
    /// <summary>Follows Samus on its heading's side until the input-change counter reaches zero, then throws the Beetom the other way unless blocked.</summary>
    Draining,
    /// <summary>Falls straight down three pixels per frame until collision selects crawling in the stored direction.</summary>
    Dropping,
    /// <summary>Falls with an increasing speed-curve place while moving two pixels per frame opposite its pre-escape facing.</summary>
    BeingFlung,
}
