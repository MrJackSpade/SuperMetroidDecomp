using Engine.Actors.Components;

namespace Engine.Actors;

/// <summary>
/// One of the things the player can be doing. The engine provides the standard ones; a game adds its own.
/// </summary>
/// <remarks>
/// An update of a state has a set order. First the player's input is asked what it wants of the state as the last
/// update left it (<see cref="Asked"/>). Then the player moves as the state they are still in (<see cref="Move"/>),
/// and settles where that leaves them. Only then does a change take effect, and by this rank: the state's own end
/// (<see cref="Ended"/>); something that interrupts the player (<see cref="Player.Interrupt"/>), which stays waiting
/// when the state's end comes first or the state holds it off; what the movement itself met; what the input asked for, which when it is to
/// carry on is told to the state (<see cref="CarriedOn"/>); and last, when the input asked for nothing, what the state
/// does unasked (<see cref="Unasked"/>). A state entered this way sets itself up as the update ends and first moves on
/// the next.
/// </remarks>
public abstract class PlayerState : State<Player>
{
    /// <summary>The name of the move values the player stands by.</summary>
    public const string StandingMoves = "standing";

    /// <summary>The name of the move values the player runs by.</summary>
    public const string RunningMoves = "running";

    /// <summary>The name of the move values the player turns around on the ground by.</summary>
    public const string TurningMoves = "turning";

    /// <summary>The name of the move values the player crouches by.</summary>
    public const string CrouchingMoves = "crouching";

    /// <summary>The name of the move values the player steers a jump by.</summary>
    public const string JumpingMoves = "jumping";

    /// <summary>The name of the move values the player steers a fall by.</summary>
    public const string FallingMoves = "falling";

    /// <summary>The name of the move values the player steers a wall jump by.</summary>
    public const string WallJumpingMoves = "wall_jumping";

    /// <summary>The name of the move values the low form moves along the ground by.</summary>
    public const string CrawlingMoves = "crawling";

    /// <summary>The name of the move values the low form steers through the air by.</summary>
    public const string CrawlingInAirMoves = "crawling_in_air";

    /// <summary>The name of the move values the low form steers a jump of its own by.</summary>
    public const string CrawlingJumpingMoves = "crawling_jumping";

    /// <summary>The name of the move values the player is thrown by when knocked back.</summary>
    public const string KnockedBackMoves = "knocked_back";

    /// <summary>Whether the player can use weapons in this state.</summary>
    public virtual bool CanAttack => true;

    /// <summary>Whether the player is in control of themselves in this state.</summary>
    public virtual bool InControl => true;

    /// <summary>
    /// How the view follows the player slowly in this state, when it does: by so many pixels an update toward keeping
    /// them within a part of the view, with the speeds of its ordinary following left as they were. Null for a state
    /// the view follows in the ordinary way, as nearly all are.
    /// </summary>
    public virtual SlowFollowing? ViewFollowsSlowly(Player owner) => null;

    /// <summary>
    /// How this state reads left and right held together (<see cref="Player.Intent"/>): 1 as rightward, -1 as
    /// leftward, 0 as neither. A state reads them as neither unless it says otherwise; most of those the player moves
    /// in read them as one way, the way ahead or the way behind, and so go on or turn.
    /// </summary>
    public virtual int ReadsBothAcross(Player owner) => 0;

    /// <summary>The way the player faces, as a way across: 1 rightward, -1 leftward.</summary>
    protected static int Forward(Player owner) => owner.Facing == Engine.Shared.Facing.Right ? 1 : -1;

    /// <summary>
    /// The form the player is in for this state, taken as the state is entered; null for a state that takes them in
    /// the form they have. The player fits the form before anything of the state is entered
    /// (<see cref="Player.FitForm"/>): where it fits they are put where the fitting leaves them and the state is
    /// entered; where it does not they stay in the state they were in, and nothing of this one happens; and where the
    /// fitting answers with another form they are put in the state that goes with that form instead. A state may ask
    /// for its form unfitted (<see cref="FormAsked.Unfitted"/>): the player takes it where they are, whatever is
    /// around them.
    /// </summary>
    public virtual FormAsked? EntersIn(Player owner) => null;

    /// <summary>
    /// Whether the player is in another's hands in this state: held, at a station or dying. A move
    /// of their own that takes them out of control, as a knockback does, is not that. What a game runs only while
    /// the player plays, as a countdown, reads it.
    /// </summary>
    public virtual bool InAnothersHands => false;

    /// <summary>
    /// What of the player the room's triggers are tried against after this state has moved: their body, unless the
    /// state says otherwise; nothing for a state in which the player meets no trigger.
    /// </summary>
    public virtual Shared.PixelRect? TriggerReach(Player owner) => owner.BodyRect;

    /// <summary>
    /// Whether the player enters a room's exit by coming to overlap it in this state. One that does not is taken
    /// through an exit only by whoever moves them (<see cref="World.Room.EnterExit"/>).
    /// </summary>
    public virtual bool MeetsExits => true;

    /// <summary>
    /// What of the room waits while the player is in this state: nothing, unless the state says otherwise, as one in
    /// which time stands still for everything but the player does. Play asks as each update begins.
    /// </summary>
    public virtual World.Stopped Stops => World.Stopped.Nothing;

    /// <summary>
    /// Whether an interruption has to wait while the player is in this state: it stays waiting, growing older, until
    /// a state that does not hold it off has moved.
    /// </summary>
    public virtual bool HoldsOffInterruptions => false;

    /// <summary>
    /// Asked of a state about to interrupt the player after it has waited <paramref name="waited"/> updates for
    /// states that held it off: whether it still does. A state whose time was running while it waited shortens
    /// itself here.
    /// </summary>
    public virtual bool StillInterrupts(Player owner, int waited) => true;

    /// <summary>
    /// What the player's input asks of this state, decided before the player moves: another state to change to once
    /// this update's movement is done; this state, to carry on in it; or null when the input asks for nothing, which
    /// leaves the state to do what it does unasked. A state the player does not control carries on.
    /// </summary>
    public virtual PlayerState? Asked(Player owner) => this;

    /// <summary>
    /// Moves the player for one update as this state. Returns the state the movement itself brings about, a landing or
    /// a floor walked off, which outranks what the input asked for; or null when it brings about none.
    /// </summary>
    public abstract PlayerState? Move(Player owner);

    /// <summary>
    /// The state that follows this one because its own time is up, whatever was met or asked for; null while it lasts.
    /// Called after the player has moved.
    /// </summary>
    public virtual PlayerState? Ended(Player owner) => null;

    /// <summary>
    /// Called when the input asked the state to carry on and nothing else changed it, after the player has moved: what
    /// of the input's asking takes effect within the state, as a change of stance does.
    /// </summary>
    public virtual void CarriedOn(Player owner)
    {
    }

    /// <summary>
    /// Whether this state's own end gives way to <paramref name="asked"/>, another state the input asked for on the
    /// update it ends. A state's end outranks the input unless the state says otherwise here.
    /// </summary>
    public virtual bool EndGivesWayTo(PlayerState asked) => false;

    /// <summary>
    /// What the state does on an update whose input asked for nothing, when nothing else changed it: the state to
    /// change to, or null to carry on. Called after the player has moved.
    /// </summary>
    public virtual PlayerState? Unasked(Player owner) => null;

    /// <summary>
    /// Called as a throw takes the player in this state (<see cref="Throw"/>): what the state kept of its own
    /// movement that a throw ends, it lets go of here.
    /// </summary>
    public virtual void OnThrown(Player owner)
    {
    }

    /// <summary>
    /// Passes an update of the state while a throw moves the player in its place: a state that lasts a length of
    /// time of its own goes on counting it here.
    /// </summary>
    public virtual void WhileThrown(Player owner)
    {
    }

    /// <summary>Plays one update in the order every state keeps.</summary>
    public sealed override State<Player> Step(Player owner)
    {
        PlayerState? asked = owner.TakeAsked(this);

        // A throw carries the player in place of the state's own movement for as long as it lasts.
        PlayerState? met = owner.Thrown is { } thrown ? thrown.Carry(owner, this) : Move(owner);
        owner.SettleAfterMoving();

        // Something that put the player into another state while they moved has already had its way.
        if (!ReferenceEquals(owner.State, this))
            return owner.State;

        PlayerState? ended = Ended(owner);
        bool givesWay = asked is not null && !ReferenceEquals(asked, this) && EndGivesWayTo(asked);
        if (ended is not null && !givesWay)
        {
            owner.KeepInterruptionWaiting();
            return ended;
        }

        if ((owner.TakeInterruption(this) ?? met) is { } forced)
            return forced;
        if (ReferenceEquals(asked, this))
            CarriedOn(owner);
        return owner.EnteringFrom(this, (asked is null ? Unasked(owner) : asked) ?? this, unasked: asked is null);
    }
}
