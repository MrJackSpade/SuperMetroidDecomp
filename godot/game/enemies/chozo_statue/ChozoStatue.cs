using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Chozo statue that comes to life. It sits with a hand held out; when Samus drops into the hand as a ball, and
/// what it waits for has happened, it wakes. One kind closes its hand on her and carries her off across the room,
/// its feet breaking through what they tread on, and sets her down. The other lowers the room's liquid. Nothing
/// hurts it and it hurts nothing; it is solid.
/// </summary>
/// <remarks>
/// Its timelines are "idle", whose cue "await_hand" says it is ready, "activated", and "lowered" for one whose work
/// was done before. The cues of "activated" are "sound:" with a sound's name; "move:" with how far it steps this
/// update and where it holds Samus from its middle, across and down, both across for a statue facing right;
/// "footstep:" with how far across from its middle its foot comes down; "lower_liquid"; "unlock", which gives Samus
/// back her freedom; and "release", which also puts the room back as it was.
/// </remarks>
public partial class ChozoStatue : Enemy
{
    /// <summary>What must be true for it to wake when Samus drops into its hand; null when nothing need be.</summary>
    [Export]
    public Condition? WakesWhen { get; set; }

    /// <summary>What must be true for its hand to notice her at all; null when nothing need be.</summary>
    [Export]
    public Condition? ArmedWhen { get; set; }

    /// <summary>Its open hand, in pixels from its middle.</summary>
    [Export]
    public Rect2I Hand { get; set; } = new(-48, -16, 16, 16);

    /// <summary>The fact recorded when it wakes; one whose fact is already recorded has done its work. Empty for none.</summary>
    [Export]
    public string RecordsFact { get; set; } = "";

    /// <summary>What is told "wake" as it wakes and "release" once it has set Samus down.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldOfIt { get; set; } = [];

    /// <summary>The terrain patch that clears its hand as it wakes; empty for none.</summary>
    [Export, DataValue("hand_patch")]
    public string HandPatch { get; set; } = "";

    /// <summary>The terrain patch that opens its way as it wakes; it is taken back when it sets Samus down. Empty for none.</summary>
    [Export, DataValue("way_patch")]
    public string WayPatch { get; set; } = "";

    /// <summary>How far below its middle its feet come down, in pixels.</summary>
    [Export, DataValue("footstep_depth")]
    public int FootstepDepth { get; set; } = 28;

    /// <summary>The effect of a foot breaking through, by the data file of its timeline.</summary>
    [Export, DataValue("footstep_effect")]
    public string FootstepEffect { get; set; } = "";

    /// <summary>How many updates after it is told to that the liquid starts to go down.</summary>
    [Export, DataValue("lowering_delay")]
    public int LoweringDelay { get; set; } = 32;

    private bool _ready;
    private bool _handTaken;
    private bool _awake;
    private bool _carrying;
    private int _lowerIn;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part) => HitResult.Absorbed;

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // It hurts nothing.
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        (_ready, _handTaken, _awake, _carrying, _lowerIn) = (false, false, false, false, 0);
        if (RecordsFact.Length > 0 && Room.Session.Progress.Has(RecordsFact))
        {
            // Its work was done before.
            _awake = true;
            if (HandPatch.Length > 0)
                Room.Terrain.ApplyPatch(HandPatch);
            Play("lowered");
            return;
        }

        Play("idle");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        Player samus = Room.Player;
        switch (parts[0])
        {
            case "await_hand":
                _ready = true;
                break;

            case "sound":
                Sound(parts[1]);
                break;

            case "move":
                Step(samus, Number(parts[1]), Number(parts[2]), Number(parts[3]));
                break;

            case "footstep":
                Tread(Number(parts[1]));
                break;

            case "lower_liquid":
                _lowerIn = LoweringDelay;
                break;

            case "unlock":
                LetGo(samus);
                break;

            case "release":
                LetGo(samus);
                if (WayPatch.Length > 0)
                    Room.Terrain.RevertPatch(WayPatch);
                Tell("release");
                break;

            default:
                throw new InvalidOperationException($"The Chozo statue's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        if (_lowerIn > 0 && --_lowerIn == 0)
            Room.Liquid?.Triggered("fall");
        if (_awake)
            return;
        if (!_handTaken && SamusIsInHand(samus))
            TakeHand(samus);
        if (!_handTaken || !_ready)
            return;
        _awake = true;
        Play("activated", restart: true);
    }

    /// <summary>Whether Samus has dropped into its hand as a ball, with everything it waits for done.</summary>
    private bool SamusIsInHand(Player samus)
    {
        if (ArmedWhen is { } armed && !armed.IsTrue(Room))
            return false;
        if (samus.Form != Player.Crawling || samus.Velocity.Y.IsNegative || !samus.State.InControl)
            return false;
        var hand = new PixelRect(Position.X.Whole + Hand.Position.X, Position.Y.Whole + Hand.Position.Y, Hand.Size.X, Hand.Size.Y);
        if (!hand.OverlapsOrTouches(samus.BodyRect))
            return false;
        return WakesWhen is not { } wakes || wakes.IsTrue(Room);
    }

    /// <summary>Its hand has her: she is held, and the room is made ready for what it does.</summary>
    private void TakeHand(Player samus)
    {
        _handTaken = true;
        _carrying = true;
        samus.Hold();
        if (RecordsFact.Length > 0)
            Room.Session.Progress.Record(RecordsFact);
        if (HandPatch.Length > 0)
            Room.Terrain.ApplyPatch(HandPatch);
        if (WayPatch.Length > 0)
            Room.Terrain.ApplyPatch(WayPatch);
        Tell("wake");
    }

    /// <summary>Takes one stride with Samus in its hand, keeping its feet on the ground.</summary>
    private void Step(Player samus, int stride, int holdAcross, int holdDown)
    {
        int way = Facing == Facing.Right ? 1 : -1;
        if (stride != 0)
        {
            Collider body = Body ?? throw new InvalidOperationException($"The Chozo statue \"{Name}\" has no body collider to walk with.");
            Room.Mover.MoveAcross(body, Pixels.FromWhole(stride * way), MoveOptions.Plain);
            Room.Mover.MoveDown(body, Pixels.FromWhole(Math.Abs(stride)), MoveOptions.Plain);
        }

        if (_carrying)
            samus.Position = Position + PixelOffset.FromWhole(holdAcross * way, holdDown);
    }

    /// <summary>Its foot comes down: what it treads on that cannot bear it gives way.</summary>
    private void Tread(int across)
    {
        var foot = new PixelRect(Position.X.Whole + across - 8, Position.Y.Whole + FootstepDepth, 16, 1);
        if (Room.Terrain.BlocksIn(foot, touchingCounts: true).Count == 0)
            return;
        Room.Terrain.StandOn(foot);
        if (FootstepEffect.Length > 0)
            LeaveEffect(FootstepEffect, PixelPosition.FromWhole(Position.X.Whole + across, Position.Y.Whole + FootstepDepth));
    }

    private void LetGo(Player samus)
    {
        if (!_carrying)
            return;
        _carrying = false;
        if (samus.State is HeldState)
            samus.Release();
    }

    private void Tell(string what)
    {
        foreach (NodePath path in ToldOfIt)
            (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"The Chozo statue is to tell \"{path}\" that it has woken, but that is nothing that can be told.")).Triggered(what);
    }

    private static int Number(string text) => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
