using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Rooms.Crateria;

/// <summary>
/// The hall of the golden statue at the way into Tourian. The eyes of each of its four guardians glow and dim in
/// turn. A guardian whose boss has been beaten is let go, one at a time: its eyes go out, a light gathers at them
/// and bursts into sparks that fall into the water, its spirit rises out of the room, and it turns grey for good.
/// When all four are grey the statue sinks, shaking the room, and the way down is open from then on. The sinking is
/// played as each update opens, before the update's random number is drawn, as the cartridge played it; the rest
/// with the room's sequences.
/// </summary>
/// <remarks>
/// The statue it is given is what sinks, with the backdrop it is given, which is the picture the statue is part
/// of. Under the statue are a visual for each guardian, named "Kraid", "Phantoon", "Draygon" and "Ridley", one over
/// each for it turned grey, named with "Grey" after, and one named "Base". Its data names, under "statues", each
/// guardian's pictures ("bright", "dim", "dark", "released", "grey"), where they lie from the statue ("offset",
/// "grey_offset"), the steps it greys by as it is let go ("greying_updates": how many updates each is held, the
/// first showing none of its grey, the last all of it, and each between as much more as the one before), whether the guardian stands in the room or is part of the backdrop ("in_room": the backdrop
/// stays with the view as it moves up and down, and what is part of it stays with it and lies behind the room's
/// tiles), the change of colours the light made at its eyes shows on the other guardians' effects from then on
/// ("eye_glow_colours", by each effect's folder), and
/// its effects: "eye_glow",
/// "particle", "tail", "splash" and "soul" by the data files of their timelines (those of the particle and the
/// soul loop). It names "base" (a "picture" and its "offset"); the "dust" the sinking statue raises, by the data
/// file of its timeline, with "dust_puffs", "dust_rounds", "dust_updates" and the "dust_scatter" it is thrown by;
/// "floor_crumble", the change to the terrain that opens the way down; and "quake_sound" and "release_sound".
/// Once the hall
/// has settled as the room is entered, the sequence is gone from the room, which is what the hall's door waits for.
/// </remarks>
public partial class TourianStatueHall : RoomSequence, IOpeningStep
{
    /// <summary>The data file of its pictures and effects.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The statue, which sinks.</summary>
    [Export]
    public NodePath Statue { get; set; } = new();

    /// <summary>The camera zone below the hall, opened once the way down is.</summary>
    [Export]
    public NodePath LowerView { get; set; } = new();

    /// <summary>The backdrop the statue is part of, which sinks with it.</summary>
    [Export]
    public NodePath Backdrop { get; set; } = new();

    /// <summary>The terrain patch that leaves the floor clear when the room is entered with the way already open.</summary>
    [Export]
    public string ClearPatch { get; set; } = "";

    private enum Stage
    {
        Entering,
        Glowing,
        Releasing,
        Done,
    }

    private enum Releases
    {
        None,
        One,
        AllButTheOneUnderWay,
        All,
    }

    private enum Descent
    {
        AwaitingReleases,
        Holding,
        Lowering,
        Open,
    }

    /// <summary>One guardian of the statue.</summary>
    private sealed class Guardian(string name, string boss, string greyFact, int eyeX, int eyeY, int firstUpdates)
    {
        public string Name => name;
        public string Boss => boss;
        public string GreyFact => greyFact;
        public PixelOffset Eye => PixelOffset.FromWhole(eyeX, eyeY);
        public int FirstUpdates => firstUpdates;
        public Stage Stage { get; set; }
        public int Frame { get; set; } = -1;
        public int Left { get; set; }
        public int GreyingFor { get; set; } = -1;

        /// <summary>How many updates each step of its greying is held.</summary>
        public IReadOnlyList<int> GreyingUpdates { get; set; } = [];

        /// <summary>How much of its grey shows once it has been greying <see cref="GreyingFor"/> updates: none in its first step, all in its last.</summary>
        public float GreyShown
        {
            get
            {
                int step = 0;
                for (int passed = GreyingFor - 1; step < GreyingUpdates.Count - 1 && passed >= GreyingUpdates[step]; step++)
                    passed -= GreyingUpdates[step];
                return (float)step / (GreyingUpdates.Count - 1);
            }
        }
    }

    /// <summary>How long the statue waits once all four are grey, how far it sinks, and how far each update.</summary>
    private const int DescentDelay = 301, DescentDistance = 240;
    private static readonly Pixels DescentStep = Pixels.FromNumber(0.25);

    /// <summary>How long a shake is kept going each time it is asked for.</summary>
    private const int ShakeUpdates = 32;

    /// <summary>How long after entering the hall may settle.</summary>
    private const int SettleUpdates = 128;

    /// <summary>How long a light at a guardian's eyes lasts before it bursts, and how many sparks it bursts into.</summary>
    private const int EyeGlowUpdates = 111, Sparks = 4;

    /// <summary>The eyes' pictures while they glow, and for how long each.</summary>
    private static readonly (string Picture, int Updates)[] Glow = [("dim", 12), ("dark", 12), ("dim", 12), ("bright", 16)];

    /// <summary>The eyes' pictures while a guardian is let go.</summary>
    private static readonly (string Picture, int Updates)[] Release = [("dim", 16), ("dark", 16), ("released", 192), ("released", 128)];

    private readonly Guardian[] _guardians =
    [
        new("Kraid", Bosses.Kraid, StoryEvents.KraidStatueGrey, 104, 114, 4),
        new("Phantoon", Bosses.Phantoon, StoryEvents.PhantoonStatueGrey, 132, 144, 6),
        new("Draygon", Bosses.Draygon, StoryEvents.DraygonStatueGrey, 158, 128, 8),
        new("Ridley", Bosses.Ridley, StoryEvents.RidleyStatueGrey, 122, 81, 10),
    ];

    private readonly HashSet<Guardian> _firstGlow = [];
    private readonly List<(PixelPosition At, int Left, Guardian Of)> _glows = [];
    private readonly List<(Effect Spark, PixelVelocity Speed, int Age, Guardian Of)> _sparks = [];
    private PatchSequence? _floorCrumble;
    private int _dustRoundsLeft;
    private int _dustWait;
    private readonly List<(Effect Tail, int Age)> _tails = [];

    /// <summary>
    /// The pictures of the statue that are things of their own on the cartridge, each one of the enemies' shots, in
    /// the order it makes them as the room is set up: the base, and the two guardians that stand in the room.
    /// </summary>
    private static readonly string[] Decorations = ["Base", "Ridley", "Phantoon"];

    /// <summary>What holds each decoration's place among the room's places for what enemies launch; null for one the room had no place for, which is not there.</summary>
    private readonly Dictionary<string, Effect?> _decorationHolds = new(StringComparer.Ordinal);

    /// <summary>
    /// What holds the place of each puff of the sinking statue's dust for as long as the dust is raised: on the
    /// cartridge each puff is one of the enemies' shots that throws itself again each round. Only those the room had a
    /// place for are here, and only they are raised.
    /// </summary>
    private readonly List<Effect> _dustHolds = [];
    private readonly List<(Effect Soul, Pixels Speed)> _souls = [];

    /// <summary>Every effect the guardians have let out, which shows the light last made while it plays; some may be gone.</summary>
    private readonly List<Effect> _letOut = [];

    /// <summary>The colours of the light last made at a guardian's eyes, which the effects let out show; null before any.</summary>
    private ColourChange? _eyesLit;

    /// <summary>The name the light's colours are shown on the effects under.</summary>
    private const string EyesLitSource = "eyes_lit";

    private DataNode? _data;
    private Vector2 _statueHome;
    private Releases _releases;
    private Descent _descent;
    private Pixels _sunk;
    private int _wait;
    private bool _settled;
    private readonly Rooms.QuakeRumble _rumble = new();
    private bool _begun;

    /// <summary>The hall begins as its room is set up, as the cartridge's does: what it clears, opens and shows is so before the first update.</summary>
    protected override void OnStarted() => Begin(Room);

    /// <summary>The statue's sinking, which reads the random number as the update before left it.</summary>
    public void StepOpening(Room room)
    {
        if (!_begun)
            Begin(room);
        StepDescent(room);
    }

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (!_begun)
            Begin(room);
        StepGuardians(room);
        StepEffects(room);
        StepDust(room);

        // The hall has settled once it has stood a while with no guardian part-way through anything. On the cartridge
        // it is the base's decoration that sees this, so without it the hall never settles.
        if (!_settled && elapsed >= SettleUpdates && _firstGlow.Count == 0 && _releases == Releases.None && _decorationHolds["Base"] is not null)
            Settle(room);
        Place(room);
        return false;
    }

    private void Begin(Room room)
    {
        _begun = true;
        _data = room.Content.ReadData(DataFile);
        _statueHome = StatueNode().Position;
        _floorCrumble = PatchSequence.Read(Data().Get("floor_crumble"));
        (_dustRoundsLeft, _dustWait) = (0, 0);
        ActorVisual footing = Visual("Base");
        footing.Bind(room.Content);
        footing.PictureOffset = Offset(Data().Get("base").Get("offset"));
        footing.Show(Data().Get("base").Text("picture"));
        (_releases, _settled, _sunk) = (Releases.None, false, Pixels.Zero);
        _rumble.StartOver();
        _firstGlow.Clear();
        _glows.Clear();
        _sparks.Clear();
        _tails.Clear();
        _souls.Clear();
        _letOut.Clear();
        _dustHolds.Clear();
        _eyesLit = null;
        foreach (Guardian guardian in _guardians)
        {
            (guardian.Stage, guardian.Frame, guardian.Left, guardian.GreyingFor) = (Stage.Entering, -1, 0, -1);
            DataNode pictures = Data().Get("statues").Get(guardian.Name.ToLowerInvariant());
            guardian.GreyingUpdates = [.. pictures.List("greying_updates").Select(updates => updates.AsInt())];
            if (guardian.GreyingUpdates.Count < 2)
                throw pictures.Problem("gives fewer than two steps of greying");
            Visual(guardian.Name).Bind(room.Content);
            Visual(guardian.Name).PictureOffset = Offset(pictures.Get("offset"));
            Visual(guardian.Name + "Grey").PictureOffset = Offset(pictures.Get("grey_offset"));
            ActorVisual grey = Visual(guardian.Name + "Grey");
            grey.Bind(room.Content);
            grey.Show(Picture(guardian, "grey"));
            grey.Tint(new Color(1, 1, 1, 0));

            // A guardian that is part of the backdrop lies behind the room's tiles with it, so the floor hides it as it sinks.
            if (!pictures.Flag("in_room"))
            {
                foreach (ActorVisual visual in new[] { Visual(guardian.Name), grey })
                    (visual.ZAsRelative, visual.ZIndex) = (false, RoomDepth.BehindRoom);
            }
        }

        // The decorations take their places as the room is set up; one the room has no place for is not there.
        _decorationHolds.Clear();
        foreach (string decoration in Decorations)
        {
            Effect hold = Effect.HoldingPlace(PixelPosition.FromWhole(0, 0));
            _decorationHolds[decoration] = room.Spawn(hold) ? hold : null;
            bool there = _decorationHolds[decoration] is not null;
            Visual(decoration).SetShown(there);
            if (decoration != "Base")
                Visual(decoration + "Grey").SetShown(there);
        }

        if (room.Session.Progress.Has(StoryEvents.TourianUnlocked))
        {
            // The way down was opened before: the statue is already sunk and the floor already gone.
            _sunk = Pixels.FromWhole(DescentDistance);
            if (ClearPatch.Length > 0)
                room.Terrain.ApplyPatch(ClearPatch);
            Open(room);
        }
        else
        {
            _descent = Descent.AwaitingReleases;
        }
    }

    /// <summary>Plays one update of each guardian's eyes.</summary>
    private void StepGuardians(Room room)
    {
        foreach (Guardian guardian in _guardians)
        {
            // It greys a step at a time: each step shows so much more of its grey, and the last leaves it grey.
            if (guardian.GreyingFor >= 0 && guardian.GreyingFor < guardian.GreyingUpdates.Sum())
            {
                guardian.GreyingFor++;
                Visual(guardian.Name + "Grey").Tint(new Color(1, 1, 1, guardian.GreyShown));
            }

            if (guardian.Stage == Stage.Done || (guardian.Left > 0 && --guardian.Left > 0))
                continue;
            Advance(room, guardian);
        }
    }

    /// <summary>Takes a guardian on to what its eyes show next.</summary>
    private void Advance(Room room, Guardian guardian)
    {
        switch (guardian.Stage)
        {
            case Stage.Entering:
                if (guardian.Frame < 0)
                {
                    _firstGlow.Add(guardian);
                    if (room.Session.Progress.Has(guardian.GreyFact))
                    {
                        // It was let go before: it is grey, and has nothing more to do.
                        FinishRelease(guardian);
                        Visual(guardian.Name).Show(Picture(guardian, "released"));
                        Visual(guardian.Name + "Grey").Tint(Colors.White);
                        guardian.Stage = Stage.Done;
                        return;
                    }

                    Show(guardian, "bright", guardian.FirstUpdates);
                    guardian.Frame = 0;
                    return;
                }

                StartGlowing(guardian);
                return;

            case Stage.Glowing:
                if (++guardian.Frame < Glow.Length)
                {
                    Show(guardian, Glow[guardian.Frame].Picture, Glow[guardian.Frame].Updates);
                    return;
                }

                // At the end of each round of its glow, one whose boss has been beaten is let go, unless another is
                // part-way through being let go, in which case it glows another round and asks again.
                if (!room.Session.Progress.Has(Bosses.Defeated(guardian.Boss)))
                {
                    _firstGlow.Remove(guardian);
                    StartGlowing(guardian);
                    return;
                }

                if (_releases is Releases.One or Releases.AllButTheOneUnderWay)
                {
                    StartGlowing(guardian);
                    return;
                }

                _releases = _releases is Releases.AllButTheOneUnderWay or Releases.All ? Releases.AllButTheOneUnderWay : Releases.One;
                (guardian.Stage, guardian.Frame) = (Stage.Releasing, 0);
                Show(guardian, Release[0].Picture, Release[0].Updates);
                return;

            case Stage.Releasing:
                guardian.Frame++;
                PixelPosition eye = StatuePlace() + guardian.Eye;
                switch (guardian.Frame)
                {
                    case 2:
                        // A light gathers at its eyes; it is the light that bursts, so one the room has no place
                        // for bursts into nothing.
                        if (Leave(room, guardian, "eye_glow", eye) is not null)
                        {
                            _glows.Add((eye, EyeGlowUpdates, guardian));
                            LightEyes(guardian);
                        }

                        break;
                    case 3:
                        // Its spirit leaves it, and it begins to turn grey.
                        if (Leave(room, guardian, "soul", eye) is { } soul)
                            _souls.Add((soul, Pixels.FromWhole(-4)));
                        guardian.GreyingFor = 0;
                        break;
                    case 4:
                        room.Session.Progress.Record(guardian.GreyFact);
                        FinishRelease(guardian);
                        guardian.Stage = Stage.Done;
                        return;
                }

                Show(guardian, Release[guardian.Frame].Picture, Release[guardian.Frame].Updates);
                return;

            default:
                throw new InvalidOperationException($"A guardian of the statue has no stage {guardian.Stage}.");
        }
    }

    private void StartGlowing(Guardian guardian)
    {
        (guardian.Stage, guardian.Frame) = (Stage.Glowing, 0);
        Show(guardian, Glow[0].Picture, Glow[0].Updates);
    }

    private void FinishRelease(Guardian guardian)
    {
        _releases = _releases is Releases.AllButTheOneUnderWay or Releases.All ? Releases.All : Releases.None;
        _firstGlow.Remove(guardian);
    }

    private void Show(Guardian guardian, string picture, int updates)
    {
        Visual(guardian.Name).Show(Picture(guardian, picture));
        guardian.Left = updates;
    }

    /// <summary>
    /// The light at a guardian's eyes is made: the effects the guardians have let out show its colours from now on,
    /// in place of those of any light made before, as the cartridge colours all the statue's sprites.
    /// </summary>
    private void LightEyes(Guardian guardian)
    {
        ColourChange lit = ColourChange.Read(Data().Get("statues").Get(guardian.Name.ToLowerInvariant()).Get("eye_glow_colours"));
        _eyesLit = lit;
        _letOut.RemoveAll(gone => !GodotObject.IsInstanceValid(gone));
        foreach (Effect effect in _letOut)
            lit.ShowOn(effect, EyesLitSource);
    }

    /// <summary>Plays one update of the lights, sparks, tails and spirits the guardians have let out.</summary>
    private void StepEffects(Room room)
    {
        for (int index = _glows.Count - 1; index >= 0; index--)
        {
            (PixelPosition at, int left, Guardian of) = _glows[index];
            if (--left > 0)
            {
                _glows[index] = (at, left, of);
                continue;
            }

            // The light bursts: the room shakes, and its sparks fly up and out.
            _glows.RemoveAt(index);
            room.Session.Sounds.Request(Data().Text("release_sound"), channel: "room");
            room.Shake(1, ShakeUpdates, ShakeWay.Down);
            for (int spark = 0; spark < Sparks; spark++)
            {
                // A spark the room has no place for is not made, and draws no number for its way.
                if (Leave(room, of, "particle", at) is not { } made)
                    continue;
                int angle = room.Session.Random.Draw().Part(0, 6) - 32;
                var speed = new PixelVelocity(Turns.Sine(angle, Pixels.FromWhole(1)), -Turns.Cosine(angle, Pixels.FromWhole(4)));
                _sparks.Add((made, speed, 0, of));
            }
        }

        Pixels? surface = room.Liquid?.Surface;
        for (int index = _sparks.Count - 1; index >= 0; index--)
        {
            (Effect spark, PixelVelocity speed, int age, Guardian from) = _sparks[index];
            bool wasUnder = surface is { } before && spark.Position.Y >= before;
            spark.Position += speed.OverOneUpdate;
            bool isUnder = surface is { } after && spark.Position.Y >= after;
            if (wasUnder != isUnder && surface is { } line)
                Leave(room, from, "splash", new PixelPosition(spark.Position.X, line - Pixels.FromWhole(4)));

            // Every twelfth update, half-way through its turn, a spark leaves a tail behind it.
            if (age % 12 == 6 && Leave(room, from, "tail", spark.Position) is { } tail)
                _tails.Add((tail, 0));
            if (spark.Position.Y.Whole >= 256)
            {
                room.Remove(spark);
                _sparks.RemoveAt(index);
                continue;
            }

            _sparks[index] = (spark, speed.WithY(speed.Y + Pixels.FromNumber(0.0625)), age + 1, from);
        }

        for (int index = _tails.Count - 1; index >= 0; index--)
        {
            (Effect tail, int age) = _tails[index];
            age++;
            int drop = age switch { 4 => 8, 8 => 4, 12 => 2, _ => 0 };
            tail.Position += PixelOffset.FromWhole(0, drop);
            if (age >= 16)
                _tails.RemoveAt(index);
            else
                _tails[index] = (tail, age);
        }

        for (int index = _souls.Count - 1; index >= 0; index--)
        {
            (Effect soul, Pixels speed) = _souls[index];
            soul.Position += new PixelOffset(Pixels.Zero, speed);
            if (soul.Position.Y.IsNegative)
            {
                room.Remove(soul);
                _souls.RemoveAt(index);
                continue;
            }

            _souls[index] = (soul, speed - Pixels.FromNumber(0.5));
        }
    }

    /// <summary>Plays one update of the statue's sinking.</summary>
    private void StepDescent(Room room)
    {
        switch (_descent)
        {
            case Descent.Open:
                return;

            case Descent.AwaitingReleases:
                if (!_guardians.All(guardian => room.Session.Progress.Has(guardian.GreyFact)))
                    return;
                _releases = _releases is Releases.One or Releases.AllButTheOneUnderWay ? Releases.AllButTheOneUnderWay : Releases.All;
                if (_releases == Releases.AllButTheOneUnderWay)
                    return;
                (_descent, _wait) = (Descent.Holding, DescentDelay);
                return;
        }

        room.Shake(2, ShakeUpdates, ShakeWay.Down);
        _rumble.Step(room, Data().Text("quake_sound"));

        if (_descent == Descent.Holding)
        {
            if (--_wait > 0)
                return;
            (_dustRoundsLeft, _dustWait) = (Data().Int("dust_rounds"), 0);

            // The puffs of dust take their places once, for every round they are raised.
            for (int puff = 0; puff < Data().Int("dust_puffs"); puff++)
            {
                Effect hold = Effect.HoldingPlace(DustAt);
                if (room.Spawn(hold))
                    _dustHolds.Add(hold);
            }

            _descent = Descent.Lowering;
            return;
        }

        _sunk += DescentStep;
        if (_sunk.Whole < DescentDistance)
            return;
        room.Play((_floorCrumble ?? throw new InvalidOperationException("The statue hall has not begun.")).FromStart());
        room.Session.Progress.Record(StoryEvents.TourianUnlocked);
        Open(room);
    }

    /// <summary>The way down is open: the view may follow Samus down, and the hall is settled for good.</summary>
    private void Open(Room room)
    {
        _descent = Descent.Open;
        (GetNode(LowerView) as CameraZone
            ?? throw new InvalidOperationException($"The statue hall \"{Name}\" is to open \"{LowerView}\", but that is no camera zone."))
            .Triggered("open");
        Settle(room);
    }

    private void Settle(Room room)
    {
        _settled = true;
        room.MarkGone(Name);
    }

    /// <summary>Leaves an effect the data names, if it names one, and returns it.</summary>
    /// <summary>
    /// Puts the statue and its backdrop where they are this update. The backdrop stays with the view as it moves up and
    /// down, and so does what is part of it; what stands in the room does not.
    /// </summary>
    private void Place(Room room)
    {
        int view = room.Session.Camera.Top;
        StatueNode().Position = _statueHome + new Vector2(0, _sunk.Whole + view);
        (GetNode(Backdrop) as Backdrop
            ?? throw new InvalidOperationException($"The statue hall \"{Name}\" is given \"{Backdrop}\" as its backdrop, which is none."))
            .LiesAt = new Vector2I(0, _sunk.Whole);
        Visual("Base").Position = new Vector2(0, -view);
        foreach (Guardian guardian in _guardians)
        {
            if (!Data().Get("statues").Get(guardian.Name.ToLowerInvariant()).Flag("in_room"))
                continue;
            Visual(guardian.Name).Position = new Vector2(0, -view);
            Visual(guardian.Name + "Grey").Position = new Vector2(0, -view);
        }
    }

    /// <summary>
    /// Plays one update of the dust the sinking statue raises: so many puffs at a time, each thrown a chance distance
    /// from under the statue, and again as often as its data says, a puff's length apart. Only the puffs that hold a
    /// place are raised, and as the last round ends they give their places back.
    /// </summary>
    private void StepDust(Room room)
    {
        if (_dustHolds.Count == 0 || --_dustWait > 0)
            return;
        if (_dustRoundsLeft == 0)
        {
            foreach (Effect hold in _dustHolds)
                room.Remove(hold);
            _dustHolds.Clear();
            return;
        }

        _dustRoundsLeft--;
        _dustWait = Data().Int("dust_updates");
        DataNode scatter = Data().Get("dust_scatter");
        FrameTimeline dust = FrameTimeline.Read(room.Content.ReadData(Data().Text("dust")));
        for (int puff = 0; puff < _dustHolds.Count; puff++)
        {
            PixelOffset thrown = room.Session.Random.Scatter(scatter.Int("width"), scatter.Int("height"), scatter.Int("middle_x"), scatter.Int("middle_y"));
            room.Spawn(Effect.Of(dust, DustAt + thrown));
        }
    }

    /// <summary>Where the sinking statue's dust is thrown from.</summary>
    private static PixelPosition DustAt => PixelPosition.FromWhole(128, 188);

    /// <summary>A place from the statue, as its data gives one.</summary>
    private static Vector2I Offset(DataNode offset) => new(offset.Int("x"), offset.Int("y"));

    private Effect? Leave(Room room, Guardian guardian, string name, PixelPosition at)
    {
        string file = Data().Get("statues").Get(guardian.Name.ToLowerInvariant()).Text(name);
        if (file.Length == 0)
            return null;
        // Each is one of the enemies' shots on the cartridge: it holds one of the room's places, and with none free it is
        // not made.
        Effect effect = Effect.Of(FrameTimeline.Read(room.Content.ReadData(file)), at, holdsPlace: true);
        if (!room.Spawn(effect))
            return null;

        // It is cut in its own guardian's light; a later light made since shows in its place.
        _letOut.RemoveAll(gone => !GodotObject.IsInstanceValid(gone));
        _letOut.Add(effect);
        _eyesLit?.ShowOn(effect, EyesLitSource);
        return effect;
    }

    private string Picture(Guardian guardian, string which) =>
        Data().Get("statues").Get(guardian.Name.ToLowerInvariant()).Text(which);

    private DataNode Data() => _data ?? throw new InvalidOperationException("The statue hall has not begun.");

    private Node2D StatueNode() => GetNode(Statue) as Node2D
        ?? throw new InvalidOperationException($"The statue hall \"{Name}\" is given \"{Statue}\" as its statue, which is nothing that can be moved.");

    private PixelPosition StatuePlace() => PixelPosition.FromWhole(0, _sunk.Whole);

    private ActorVisual Visual(string name) => StatueNode().GetNodeOrNull<ActorVisual>(name)
        ?? throw new InvalidOperationException($"The statue of \"{Name}\" has no visual named \"{name}\".");
}
