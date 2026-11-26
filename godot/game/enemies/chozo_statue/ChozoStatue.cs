using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Chozo statue that comes to life. It sits with a hand held out; when Samus comes down onto the hand as a ball,
/// and what it waits for has happened, it wakes. One kind closes its hand on her and carries her off across the
/// room, its feet breaking through what they tread on, and sets her down. The other lowers the room's liquid.
/// Nothing hurts it and it hurts nothing; it is solid.
/// </summary>
/// <remarks>
/// Its timelines are "idle", whose cue "await_hand" says it is ready, "activated", and "lowered" for one whose work
/// was done before. The cues of "activated" are "sound:" with a sound's name; "move:" with how far across it steps
/// this update and where it holds Samus from its middle, across and down; "footstep:" with how far across from its
/// middle its foot comes down; "lower_liquid", which sets the room's liquid falling after its delay; "settle_liquid",
/// which puts that liquid where it lies lowered at once; "unlock", which gives Samus back her freedom; and "release", which
/// also puts the room back as it was. Its collider "Hand" is its open hand, which she can stand on until it takes
/// her: it is there wherever the room's own blocks leave the place free. The data of one that walks gives
/// "footstep_scatter", how the dust of a footstep is thrown: the width and height of the spread and how far into it
/// the middle lies.
/// <code>"footstep_scatter": { "width": 16, "height": 16, "middle_x": 0, "middle_y": 3 }</code>
/// </remarks>
public partial class ChozoStatue : Enemy
{
    /// <summary>What must be true for it to wake when Samus drops into its hand; null when nothing need be.</summary>
    [Export]
    public Condition? WakesWhen { get; set; }

    /// <summary>What must be true for its hand to notice her at all; null when nothing need be.</summary>
    [Export]
    public Condition? ArmedWhen { get; set; }

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

    /// <summary>The terrain patch that shuts its way as its room is set up, where the room's layout leaves it part open. Empty for none.</summary>
    [Export, DataValue("shut_patch")]
    public string ShutPatch { get; set; } = "";

    /// <summary>
    /// The terrain patches of what its feet break through, one for each block that can be: this name, "_", the
    /// block's column, "_" and its row. A foot that comes down in a block with no such patch breaks nothing. Empty
    /// for none.
    /// </summary>
    [Export, DataValue("plug_patches")]
    public string PlugPatches { get; set; } = "";

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
    private (int Width, int Height, int MiddleX, int MiddleY) _footstepScatter = (1, 1, 0, 0);
    private Collider? _hand;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        // Only a statue that walks throws up dust.
        if (!data.Has("footstep_scatter"))
            return;
        DataNode scatter = data.Get("footstep_scatter");
        _footstepScatter = (scatter.Int("width"), scatter.Int("height"), scatter.Int("middle_x"), scatter.Int("middle_y"));
    }

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
        // It stands in front of everything, Samus in its hand too.
        Standing = Standing.Frontmost;
        (_ready, _handTaken, _awake, _carrying, _lowerIn) = (false, false, false, false, 0);
        _hand = GetNode<Collider>("Hand");
        _hand.Enabled = false;
        if (ShutPatch.Length > 0)
            Room.Terrain.ApplyPatch(ShutPatch);
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
                Stride(samus, Number(parts[1]), Number(parts[2]), Number(parts[3]));
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

            case "settle_liquid":
                // The liquid it set falling is put where it lies lowered at once: it falls slowly, and has far to go.
                if (Room.Liquid is { } liquid)
                    liquid.PutAt(liquid.LowSurface);
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
        // Its hand can be stood on wherever the room's own blocks leave the place free, until it has taken her.
        Collider hand = _hand ?? throw new InvalidOperationException($"The Chozo statue \"{Name}\" is not in a room.");
        hand.Enabled = !_handTaken && !Room.Terrain.IsSolidIn(hand.RoomRect, SlopeTreatment.Wall);
        if (hand.Enabled && SamusIsInHand(samus, hand.RoomRect))
            TakeHand(samus);
        if (!_handTaken || !_ready)
            return;
        _awake = true;
        Play("activated", restart: true);
    }

    /// <summary>Whether Samus has come down onto its hand as a ball, with everything it waits for done.</summary>
    private bool SamusIsInHand(Player samus, PixelRect hand)
    {
        if (ArmedWhen is { } armed && !armed.IsTrue(Room))
            return false;
        if (samus.Form != Player.Crawling || samus.Velocity.Y.IsNegative || !samus.State.InControl)
            return false;
        // The original does not notice a plain ball that faces left; one with the spring ball it notices either way.
        if (samus.Facing == Facing.Left && !samus.Inventory.Owns(Equipment.SpringBall))
            return false;
        PixelRect body = samus.BodyRect;
        if (body.Bottom != hand.Top || body.Left >= hand.Right || body.Right <= hand.Left)
            return false;
        return WakesWhen is not { } wakes || wakes.IsTrue(Room);
    }

    /// <summary>Its hand has her: she is held, and the room is made ready for what it does.</summary>
    private void TakeHand(Player samus)
    {
        _handTaken = true;
        _carrying = true;
        if (_hand is { } hand)
            hand.Enabled = false;
        samus.Hold();
        if (RecordsFact.Length > 0)
            Room.Session.Progress.Record(RecordsFact);
        if (HandPatch.Length > 0)
            Room.Terrain.ApplyPatch(HandPatch);
        if (WayPatch.Length > 0)
            Room.Terrain.ApplyPatch(WayPatch);
        Tell("wake");
    }

    /// <summary>Takes one stride with Samus in its hand, pressing down as far as it strode so that its feet stay on the ground.</summary>
    private void Stride(Player samus, int stride, int holdAcross, int holdDown)
    {
        if (stride != 0)
        {
            Collider body = Body ?? throw new InvalidOperationException($"The Chozo statue \"{Name}\" has no body collider to walk with.");
            Room.Mover.MoveAcross(body, Pixels.FromWhole(stride), MoveOptions.Plain);
            Room.Mover.MoveDown(body, Pixels.FromWhole(Math.Abs(stride)), MoveOptions.Plain);
        }

        if (_carrying)
            samus.Position = Position + PixelOffset.FromWhole(holdAcross, holdDown);
    }

    /// <summary>
    /// Its foot comes down: what lies in the block it comes down in, where that can be broken through, gives way, and
    /// dust is thrown up a chance distance from the foot.
    /// </summary>
    private void Tread(int across)
    {
        if (PlugPatches.Length == 0)
            return;
        int x = Position.X.Whole + across, y = Position.Y.Whole + FootstepDepth;
        int size = Room.Terrain.TileSize;
        string plug = $"{PlugPatches}_{Math.DivRem(x, size).Quotient}_{Math.DivRem(y, size).Quotient}";
        if (x < 0 || y < 0 || !Room.Terrain.HasPatch(plug))
            return;
        Room.Terrain.ApplyPatch(plug);
        PixelOffset thrown = Random.Scatter(_footstepScatter.Width, _footstepScatter.Height, _footstepScatter.MiddleX, _footstepScatter.MiddleY);
        if (FootstepEffect.Length > 0)
            LeaveEffect(FootstepEffect, PixelPosition.FromWhole(x, y) + thrown);
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
