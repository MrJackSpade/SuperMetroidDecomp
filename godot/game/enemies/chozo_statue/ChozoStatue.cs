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
/// the middle lies. The data of one whose hand pulls a plug gives "hand_plug", the top-left of the plug's block in
/// the room. A plug its hand pulls, or a foot breaks through, is made empty at once and breaks up over its block as
/// the effect "plug_crumble".
/// <code>"footstep_scatter": { "width": 16, "height": 16, "middle_x": 0, "middle_y": 3 }, "hand_plug": { "x": 64, "y": 160 }</code>
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

    /// <summary>The effect of a plug breaking up where it was, by the data file of its timeline.</summary>
    [Export, DataValue("plug_crumble")]
    public string PlugCrumble { get; set; } = "";

    /// <summary>How many updates after it is told to that the liquid starts to go down.</summary>
    [Export, DataValue("lowering_delay")]
    public int LoweringDelay { get; set; } = 32;

    private bool _ready;
    private bool _handTaken;
    private bool _awake;
    private bool _carrying;
    private int _lowerIn;
    private (int Width, int Height, int MiddleX, int MiddleY) _footstepScatter = (1, 1, 0, 0);
    private PixelPosition? _handPlug;
    private Collider? _hand;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _handPlug = data.Has("hand_plug") ? PixelPosition.FromWhole(data.Get("hand_plug").Int("x"), data.Get("hand_plug").Int("y")) : null;
        // Only a statue that walks throws up dust.
        if (!data.Has("footstep_scatter"))
            return;
        DataNode scatter = data.Get("footstep_scatter");
        _footstepScatter = (scatter.Int("width"), scatter.Int("height"), scatter.Int("middle_x"), scatter.Int("middle_y"));
    }

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

        // Its hold only stops her taking part, as the cartridge's does: what she was doing is hers again as it lets go.
        samus.HoldAsTheyAre();
        if (RecordsFact.Length > 0)
            Room.Session.Progress.Record(RecordsFact);
        if (HandPatch.Length > 0)
        {
            // The plug its hand pulls breaks up where it was.
            Room.Terrain.ApplyPatch(HandPatch);
            Crumble(_handPlug ?? throw new InvalidOperationException($"The Chozo statue \"{Name}\" pulls a plug from its hand and its data says not where."));
        }

        if (WayPatch.Length > 0)
            Room.Terrain.ApplyPatch(WayPatch);
    }

    /// <summary>
    /// How its strides are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that
    /// holds things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Strides =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a stride across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions StridesAcross = Strides with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>
    /// Takes one stride with Samus in its hand, as the cartridge's statue does: across, with sloped ground no part of
    /// the move; then pressed down as far as it strode, whichever way it walks, so that its feet stay on the ground;
    /// then put where sloped ground has it under its middle.
    /// </summary>
    private void Stride(Player samus, int stride, int holdAcross, int holdDown)
    {
        if (stride != 0)
        {
            Collider body = Body ?? throw new InvalidOperationException($"The Chozo statue \"{Name}\" has no body collider to walk with.");
            Room.Mover.MoveAcross(body, Pixels.FromWhole(stride), StridesAcross);
            Room.Mover.MoveDown(body, Pixels.FromWhole(Math.Abs(stride)), Strides);

            // A move across of no distance meets nothing, and ends as every move across that climbs such ground does.
            Room.Mover.MoveAcross(body, Pixels.Zero, Strides);
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
        int column = Math.DivRem(x, size).Quotient, row = Math.DivRem(y, size).Quotient;
        string plug = $"{PlugPatches}_{column}_{row}";
        if (x < 0 || y < 0 || !Room.Terrain.HasPatch(plug))
            return;
        Room.Terrain.ApplyPatch(plug);
        Crumble(PixelPosition.FromWhole(column * size, row * size));

        // The dust is one of the enemies' shots on the cartridge: it holds one of the room's places while it plays, and
        // with none free it is not made and draws no number for where it is thrown.
        if (FootstepEffect.Length == 0)
            return;
        Effect dust = Effect.Of(FrameTimeline.Read(Room.Content.ReadData(FootstepEffect)), PixelPosition.FromWhole(x, y), holdsPlace: true);
        if (Room.Spawn(dust))
            dust.Position += Random.Scatter(_footstepScatter.Width, _footstepScatter.Height, _footstepScatter.MiddleX, _footstepScatter.MiddleY);
    }

    /// <summary>
    /// A plug breaks up over the block at <paramref name="corner"/>, its top-left, which is already empty: drawn as the
    /// room's own tiles are, behind everything standing in the room.
    /// </summary>
    private void Crumble(PixelPosition corner) => LeaveEffect(PlugCrumble, corner, Standing.Hindmost);

    private void LetGo(Player samus)
    {
        if (!_carrying)
            return;
        _carrying = false;
        if (samus.State is HeldState)
            samus.Release();
    }

    private static int Number(string text) => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
