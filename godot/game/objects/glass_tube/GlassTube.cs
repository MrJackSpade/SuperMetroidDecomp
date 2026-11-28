using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// The glass tube of Maridia. Only a power bomb harms it. Once one has gone off against it, the next thing Samus
/// does starts it cracking: she is held where she is, the crack spreads, and the tube bursts into shards and
/// bubbles, shaking the room. From then on it is broken for good and the water is in. The shards and bubbles are
/// its own, and it stays until the last of them is gone: a shard flies out and then sinks, swaying along an arc; a
/// bubble rises, and sways as it goes. Each draws a random number for where its sway starts as it begins to sway.
/// A shard is shown by turns of the short count at its own column and mirrored across the tube, or hidden, and is
/// taken away once it is far enough below the view.
/// </summary>
/// <remarks>
/// Its data names its pictures "intact", "cracking" and "shattered"; its effect "crack"; the timelines of a "bubble"
/// and of the ten "shards", and, under "mirrors", each shard's picture with its mirror image; the terrain patch that
/// opens it, "open_patch"; and its sounds. Its hurtbox is what a power bomb's blast reaches. A piece's timeline gives
/// the cues "reflected" and "hidden", how a shard flickers; "rise" and "fall", which set it moving or sinking;
/// "sway", on which it draws the number its sway starts at; and "end", which takes it away.
/// </remarks>
public partial class GlassTube : RoomObject, IHittable
{
    /// <summary>The picture of it whole.</summary>
    [Export, DataValue("intact")]
    public string IntactPicture { get; set; } = "";

    /// <summary>The picture of it cracking.</summary>
    [Export, DataValue("cracking")]
    public string CrackingPicture { get; set; } = "";

    /// <summary>The picture of it burst.</summary>
    [Export, DataValue("shattered")]
    public string ShatteredPicture { get; set; } = "";

    /// <summary>The effect of the crack, by the data file of its timeline.</summary>
    [Export, DataValue("crack")]
    public string CrackEffect { get; set; } = "";

    /// <summary>The timeline of a bubble of the air let out, by its data file.</summary>
    [Export, DataValue("bubble")]
    public string BubbleTimeline { get; set; } = "";

    /// <summary>The terrain patch that opens it.</summary>
    [Export, DataValue("open_patch")]
    public string OpenPatch { get; set; } = "";

    /// <summary>The sound of it bursting, by name.</summary>
    [Export, DataValue("break_sound")]
    public string BreakSound { get; set; } = "";

    /// <summary>The sound of a shot that does nothing to it.</summary>
    [Export, DataValue("dud_sound")]
    public string DudSound { get; set; } = "";

    private enum Phase
    {
        Intact,
        WaitingForSamus,
        Cracking,
        Shattered,
        Settling,
    }

    /// <summary>How a piece moves: not yet, flying straight, or swaying as it goes.</summary>
    private enum Motion
    {
        Still,
        Flying,
        Swaying,
    }

    /// <summary>How a shard is shown on the turns of the short count: as it is, by turns mirrored across the tube, or by turns hidden.</summary>
    private enum Flicker
    {
        None,
        Reflected,
        Hidden,
    }

    /// <summary>Where the crack starts, from where the tube stands, in pixels.</summary>
    private static readonly PixelOffset CrackAt = PixelOffset.FromWhole(96, 48);

    /// <summary>Where each shard starts from the crack, and how it flies.</summary>
    private static readonly (PixelOffset From, PixelVelocity Speed)[] Shards =
    [
        (PixelOffset.FromWhole(-56, 8), Speed(-1.5, 1.25)), (PixelOffset.FromWhole(-64, -12), Speed(-1.5, -1)),
        (PixelOffset.FromWhole(-20, -26), Speed(-0.625, -1.625)), (PixelOffset.FromWhole(-40, -24), Speed(-1.125, -1.125)),
        (PixelOffset.FromWhole(-64, -32), Speed(-1.125, -1.125)), (PixelOffset.FromWhole(-48, 28), Speed(-1.25, 1.75)),
        (PixelOffset.FromWhole(-24, 16), Speed(-0.375, 2.25)), (PixelOffset.FromWhole(-40, -8), Speed(-1.375, -0.375)),
        (PixelOffset.FromWhole(0, -24), Speed(0, -1.125)), (PixelOffset.FromWhole(-8, 16), Speed(-0.25, 1.5)),
    ];

    /// <summary>Where each bubble starts, from where the tube stands.</summary>
    private static readonly PixelOffset[] Bubbles =
    [
        PixelOffset.FromWhole(40, 80), PixelOffset.FromWhole(80, 72), PixelOffset.FromWhole(104, 84),
        PixelOffset.FromWhole(120, 32), PixelOffset.FromWhole(152, 64), PixelOffset.FromWhole(184, 84),
    ];

    /// <summary>How fast a bubble rises, the whole of its life.</summary>
    private static readonly PixelVelocity BubbleSpeed = new(Pixels.Zero, Pixels.FromWhole(-5));

    /// <summary>How fast a shard sinks once it sways.</summary>
    private static readonly Pixels ShardSinkSpeed = Pixels.FromNumber(0.75);

    /// <summary>How many steps of a turn a shard's sway, and a bubble's, goes round each update.</summary>
    private const int ShardSwaySteps = 1, BubbleSwaySteps = 2;

    /// <summary>How long the burst tube stays before it is done and the water is in.</summary>
    private const int ShatteredUpdates = 96;

    /// <summary>How far below the top of the view a shard is taken away.</summary>
    private const int ShardGoneBelowView = 288;

    /// <summary>Twice the column, in the room, that a shard is mirrored across.</summary>
    private const int MirrorSpan = 256;

    /// <summary>One of its shards or bubbles: where it is, how it moves and is shown, and the visual that shows it.</summary>
    private sealed class Piece(bool isShard, FrameTimeline timeline, ActorVisual visual, PixelPosition at, PixelVelocity speed)
    {
        public bool IsShard { get; } = isShard;

        public TimelinePlayer Timeline { get; } = new(timeline);

        public ActorVisual Visual { get; } = visual;

        /// <summary>Its own column, which it is shown at or mirrored across the tube from.</summary>
        public Pixels X { get; set; } = at.X;

        public Pixels Y { get; set; } = at.Y;

        public PixelVelocity Speed { get; set; } = speed;

        public Motion Moving { get; set; } = isShard ? Motion.Flying : Motion.Still;

        public Flicker Flickers { get; set; }

        /// <summary>Whether its flicker shows it at its mirror image, or hides it, on this turn of the short count.</summary>
        public bool AwayFromOwnColumn { get; set; }

        /// <summary>Where its sway is round the turn, in the cartridge's 256 steps of it.</summary>
        public int Sway { get; set; }
    }

    private readonly List<string> _shardTimelines = [];
    private readonly Dictionary<string, string> _mirrors = new(StringComparer.Ordinal);
    private readonly List<Piece> _pieces = [];
    private Phase _phase;
    private int _timer;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _shardTimelines.Clear();
        _shardTimelines.AddRange(data.List("shards").Select(shard => shard.AsText()));
        if (_shardTimelines.Count != Shards.Length)
            throw data.Problem($"must name {Shards.Length} shards");
        _mirrors.Clear();
        foreach ((string picture, DataNode mirrored) in data.Get("mirrors").Entries())
            _mirrors[picture] = mirrored.AsText();
    }

    /// <summary>The water the tube holds back.</summary>
    /// <exception cref="InvalidOperationException">The tube's room has no liquid.</exception>
    private Liquid Water => Room.Liquid ?? throw new InvalidOperationException($"The glass tube \"{Name}\" stands in a room with no liquid to hold back.");

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        if (Room.Session.Progress.Has(StoryEvents.MaridiaTubeBroken))
        {
            // It was broken before: it is open, and the water is in.
            Water.LetIn();
            Room.Terrain.ApplyPatch(OpenPatch);
            Room.Remove(this);
            return;
        }

        _phase = Phase.Intact;
        Show(IntactPicture);
    }

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => side != HitSide.Enemies && _phase == Phase.Intact;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        if (_phase != Phase.Intact)
            return HitResult.Ignored;
        if (!hit.Carries(new HitKind(HitKinds.PowerBomb)))
        {
            Sound(DudSound);
            return HitResult.Absorbed;
        }

        _phase = Phase.WaitingForSamus;
        return HitResult.Hurt;
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player samus = Room.Player;
        switch (_phase)
        {
            case Phase.Intact:
                break;

            case Phase.WaitingForSamus:
                GameInput input = samus.Input;
                bool acted = input.WasPressed(Actions.Run) || input.WasPressed(Actions.ItemCancel) || input.WasPressed(GameAction.Left)
                    || input.WasPressed(GameAction.Right) || input.WasPressed(GameAction.Jump) || input.WasPressed(GameAction.Attack);
                if (!acted)
                    break;

                // She is stopped as she is while it breaks, and goes on from there with the speed she had.
                samus.HoldAsTheyAre();
                // The crack stays where it is until its own timeline has it fall.
                Leave(CrackEffect, CrackAt, PixelVelocity.Zero);
                Show(CrackingPicture);
                (_phase, _timer) = (Phase.Cracking, 50);
                break;

            case Phase.Cracking:
                if (--_timer > 0)
                    break;
                Sound(BreakSound);
                // Its own picture is shown before its pieces are let out, whose visuals are theirs.
                Show(ShatteredPicture);
                _pieces.Clear();
                for (int shard = 0; shard < Shards.Length; shard++)
                    LetOut(true, _shardTimelines[shard], CrackAt + Shards[shard].From, Shards[shard].Speed);
                foreach (PixelOffset bubble in Bubbles)
                    LetOut(false, BubbleTimeline, bubble, BubbleSpeed);

                Room.Shake(1, 64, ShakeWay.Both, ShakeReach.TilesAndBackdrops);
                Room.Terrain.ApplyPatch(OpenPatch);
                (_phase, _timer) = (Phase.Shattered, ShatteredUpdates);
                break;

            case Phase.Shattered:
                StepPieces();
                if (--_timer > 0)
                    break;
                Room.Session.Progress.Record(StoryEvents.MaridiaTubeBroken);
                Water.LetIn();
                if (samus.State is HeldState)
                    samus.Release();
                _phase = Phase.Settling;
                if (_pieces.Count == 0)
                    Room.Remove(this);
                break;

            case Phase.Settling:
                // It is done once the last of its pieces is gone.
                StepPieces();
                if (_pieces.Count == 0)
                    Room.Remove(this);
                break;

            default:
                throw new InvalidOperationException($"The glass tube has no phase {_phase}.");
        }
    }

    private static PixelVelocity Speed(double across, double down) => new(Pixels.FromNumber(across), Pixels.FromNumber(down));

    /// <summary>
    /// Lets out a shard or a bubble at <paramref name="from"/>, going at <paramref name="speed"/>. It shows nothing
    /// until its first update, the one after the burst, plays its timeline's first step.
    /// </summary>
    private void LetOut(bool isShard, string timelineFile, PixelOffset from, PixelVelocity speed)
    {
        FrameTimeline timeline = FrameTimeline.Read(Room.Content.ReadData(timelineFile));
        TimelineStep first = timeline.Steps[0];
        var visual = new ActorVisual
        {
            Name = isShard ? "Shard" : "Bubble", PictureOffset = new Vector2I(first.Offset.X, first.Offset.Y),
            ZAsRelative = false, ZIndex = RoomDepth.Launched,
        };
        AddChild(visual);
        visual.Bind(Room.Content);
        visual.SetShown(false);
        _pieces.Add(new Piece(isShard, timeline, visual, Position + from, speed));
    }

    /// <summary>Plays an update of each of its pieces in turn, and takes away those that are gone.</summary>
    private void StepPieces()
    {
        for (int index = 0; index < _pieces.Count; index++)
        {
            Piece piece = _pieces[index];
            if (StepPiece(piece))
                continue;
            piece.Visual.SetShown(false);
            piece.Visual.QueueFree();
            _pieces.RemoveAt(index--);
        }
    }

    /// <summary>
    /// Plays one update of a piece: it moves, and a shard far enough below the view is taken away before its turn to
    /// be shown, as the cartridge's moves come before its animations; then its timeline's cues are acted on and it is
    /// shown. Returns whether it is still there.
    /// </summary>
    private bool StepPiece(Piece piece)
    {
        Move(piece);
        if (piece.IsShard && piece.Y.Whole - Room.Session.Camera.Top >= ShardGoneBelowView)
            return false;

        // A flickering shard is at its own column on the odd turns of the short count.
        bool atOwnColumn = Room.Session.Clock.ShortUpdates % 2 == 1;
        foreach (string cue in piece.Timeline.Step())
        {
            switch (cue)
            {
                case "reflected":
                    (piece.Flickers, piece.AwayFromOwnColumn) = (Flicker.Reflected, !atOwnColumn);
                    break;
                case "hidden":
                    (piece.Flickers, piece.AwayFromOwnColumn) = (Flicker.Hidden, !atOwnColumn);
                    break;
                case "rise":
                    piece.Moving = Motion.Flying;
                    break;
                case "fall":
                    piece.Moving = Motion.Swaying;
                    break;
                case "sway":
                    // The sway starts at the step of the first half turn the top seven bits of the number name; a
                    // shard sinks from then on.
                    piece.Sway = Room.Session.Random.Draw().Part(9, 7);
                    if (piece.IsShard)
                        piece.Speed = piece.Speed.WithY(ShardSinkSpeed);
                    break;
                case "end":
                    return false;
                default:
                    throw new InvalidOperationException($"A piece of the glass tube's timeline gives the cue \"{cue}\", which it does not know.");
            }
        }

        ShowPiece(piece);
        return true;
    }

    /// <summary>Moves a piece an update as it moves now.</summary>
    private static void Move(Piece piece)
    {
        switch (piece.Moving)
        {
            case Motion.Still:
                break;
            case Motion.Flying:
                (piece.X, piece.Y) = (piece.X + piece.Speed.X, piece.Y + piece.Speed.Y);
                break;
            case Motion.Swaying:
                (piece.X, piece.Y) = (piece.X + SwaySpeed(piece.Sway), piece.Y + piece.Speed.Y);
                piece.Sway = (piece.Sway + (piece.IsShard ? ShardSwaySteps : BubbleSwaySteps)) & 255;
                break;
            default:
                throw new InvalidOperationException($"A piece of the glass tube moves in no way {piece.Moving}.");
        }
    }

    /// <summary>
    /// How far across a piece sways an update at a step of its sway: a quarter of the cartridge's sine there, in
    /// 256ths of a pixel, rounded down. The cartridge keeps the step it reads to the turn's second and fourth
    /// quarters: one in the first or third is read a quarter turn on.
    /// </summary>
    private static Pixels SwaySpeed(int sway)
    {
        int step = sway % 128 < 64 ? (sway + 64) & 255 : sway;
        int twoFiftySixths = CartridgeAngles.SignedSine(step) >> 2;
        return Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 255) * 256);
    }

    /// <summary>Shows a piece where it is, at its own column or mirrored across the tube, or hidden, as its flicker has it.</summary>
    private void ShowPiece(Piece piece)
    {
        string picture = piece.Timeline.Shown.Picture;
        bool mirrored = piece.Flickers == Flicker.Reflected && piece.AwayFromOwnColumn && picture.Length > 0;
        int column = piece.X.Whole;
        if (mirrored)
        {
            picture = _mirrors.TryGetValue(picture, out string? image)
                ? image
                : throw new InvalidOperationException($"The glass tube's shard picture \"{picture}\" has no mirror image in its data.");
            column = MirrorSpan - column;
        }

        piece.Visual.Show(picture);
        piece.Visual.SetShown(!(piece.Flickers == Flicker.Hidden && piece.AwayFromOwnColumn));
        piece.Visual.Position = new Vector2(column - Position.X.Whole, piece.Y.Whole - Position.Y.Whole);
    }

    private void Leave(string effect, PixelOffset from, PixelVelocity speed)
    {
        if (effect.Length == 0)
            return;
        Effect left = Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), Position + from);
        left.Velocity = speed;
        Room.Spawn(left);
    }

    /// <summary>Shows one of its own pictures; its pieces' visuals show theirs.</summary>
    private void Show(string picture)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            if (!_pieces.Any(piece => piece.Visual == visual))
                visual.Show(picture);
        }
    }

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}
