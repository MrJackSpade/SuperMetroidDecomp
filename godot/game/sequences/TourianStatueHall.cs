using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Sequences;

/// <summary>
/// The hall of the golden statue at the way into Tourian. The eyes of each of its four guardians glow and dim in
/// turn. A guardian whose boss has been beaten is let go, one at a time: its eyes go out, a light gathers at them
/// and bursts into sparks that fall into the water, its spirit rises out of the room, and it turns grey for good.
/// When all four are grey the statue sinks, shaking the room, and the way down is open from then on.
/// </summary>
/// <remarks>
/// The statue it is given is what sinks; under it are a visual for each guardian, named "Kraid", "Phantoon",
/// "Draygon" and "Ridley", and one over each for it turned grey, named with "Grey" after. Its data names each
/// guardian's pictures ("bright", "dim", "dark", "released", "grey") and its effects: "eye_glow", "particle", "tail",
/// "splash" and "soul" by the data files of their timelines (those of the particle and the soul loop), and "dust" by
/// the data file of its shot. Once the hall
/// has settled as the room is entered, the sequence is gone from the room, which is what the hall's door waits for.
/// </remarks>
public partial class TourianStatueHall : RoomSequence
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

    /// <summary>The terrain patch that leaves the floor crumbling once the statue has sunk.</summary>
    [Export]
    public string CrumblePatch { get; set; } = "";

    /// <summary>The terrain patch that leaves the floor clear when the room is entered with the way already open.</summary>
    [Export]
    public string ClearPatch { get; set; } = "";

    /// <summary>The sound of the room shaking as the statue sinks, by name.</summary>
    [Export]
    public string QuakeSound { get; set; } = Sounds.Earthquake;

    /// <summary>The sound of a guardian's light bursting, by name.</summary>
    [Export]
    public string ReleaseSound { get; set; } = "";

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
    }

    /// <summary>How long the statue waits once all four are grey, how far it sinks, and how far each update.</summary>
    private const int DescentDelay = 301, DescentDistance = 240;
    private static readonly Pixels DescentStep = Pixels.FromNumber(0.25);

    /// <summary>How long a shake is kept going each time it is asked for.</summary>
    private const int ShakeUpdates = 32;

    /// <summary>How long a guardian takes to turn grey, and how long after entering the hall may settle.</summary>
    private const int GreyingUpdates = 128, SettleUpdates = 128;

    /// <summary>How long a light at a guardian's eyes lasts before it bursts, and how many sparks it bursts into.</summary>
    private const int EyeGlowUpdates = 111, Sparks = 4;

    /// <summary>The waits between the sounds of the room shaking, in turn.</summary>
    private static readonly int[] QuakeSoundWaits = [1, 3, 2, 1, 1, 2, 2, 1];

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
    private readonly List<(PixelPosition At, int Left)> _glows = [];
    private readonly List<(Effect Spark, PixelVelocity Speed, int Age)> _sparks = [];
    private readonly List<(Effect Tail, int Age)> _tails = [];
    private readonly List<(Effect Soul, Pixels Speed)> _souls = [];
    private DataNode? _data;
    private Vector2 _statueHome;
    private Releases _releases;
    private Descent _descent;
    private Pixels _sunk;
    private int _wait;
    private bool _settled;
    private int _soundWait;
    private int _soundTurn;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            Begin(room);
        StepGuardians(room);
        StepEffects(room);
        StepDescent(room);

        // The hall has settled once it has stood a while with no guardian part-way through anything.
        if (!_settled && elapsed >= SettleUpdates && _firstGlow.Count == 0 && _releases == Releases.None)
            Settle(room);
        StatueNode().Position = _statueHome + new Vector2(0, _sunk.Whole);
        return false;
    }

    private void Begin(Room room)
    {
        _data = room.Content.ReadData(DataFile);
        _statueHome = StatueNode().Position;
        (_releases, _settled, _sunk, _soundWait, _soundTurn) = (Releases.None, false, Pixels.Zero, 0, 0);
        _firstGlow.Clear();
        _glows.Clear();
        _sparks.Clear();
        _tails.Clear();
        _souls.Clear();
        foreach (Guardian guardian in _guardians)
        {
            (guardian.Stage, guardian.Frame, guardian.Left, guardian.GreyingFor) = (Stage.Entering, -1, 0, -1);
            Visual(guardian.Name).Bind(room.Content);
            ActorVisual grey = Visual(guardian.Name + "Grey");
            grey.Bind(room.Content);
            grey.Show(Picture(guardian, "grey"));
            grey.Tint(new Color(1, 1, 1, 0));
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
            if (guardian.GreyingFor >= 0 && guardian.GreyingFor < GreyingUpdates)
            {
                guardian.GreyingFor++;
                Visual(guardian.Name + "Grey").Tint(new Color(1, 1, 1, (float)guardian.GreyingFor / GreyingUpdates));
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
                if (!room.Session.Progress.Has(guardian.Boss))
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
                        // A light gathers at its eyes.
                        Leave(room, "eye_glow", eye);
                        _glows.Add((eye, EyeGlowUpdates));
                        break;
                    case 3:
                        // Its spirit leaves it, and it begins to turn grey.
                        if (Leave(room, "soul", eye) is { } soul)
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

    /// <summary>Plays one update of the lights, sparks, tails and spirits the guardians have let out.</summary>
    private void StepEffects(Room room)
    {
        for (int index = _glows.Count - 1; index >= 0; index--)
        {
            (PixelPosition at, int left) = _glows[index];
            if (--left > 0)
            {
                _glows[index] = (at, left);
                continue;
            }

            // The light bursts: the room shakes, and its sparks fly up and out.
            _glows.RemoveAt(index);
            if (ReleaseSound.Length > 0)
                room.Session.Sounds.Request(ReleaseSound, channel: "room");
            room.Shake(1, ShakeUpdates, ShakeWay.Down);
            for (int spark = 0; spark < Sparks; spark++)
            {
                int angle = room.Session.Random.Draw().Part(0, 6) - 32;
                var speed = new PixelVelocity(Turns.Sine(angle, Pixels.FromWhole(1)), -Turns.Cosine(angle, Pixels.FromWhole(4)));
                if (Leave(room, "particle", at) is { } made)
                    _sparks.Add((made, speed, 0));
            }
        }

        Pixels? surface = room.Liquid?.Surface;
        for (int index = _sparks.Count - 1; index >= 0; index--)
        {
            (Effect spark, PixelVelocity speed, int age) = _sparks[index];
            bool wasUnder = surface is { } before && spark.Position.Y >= before;
            spark.Position += speed.OverOneUpdate;
            bool isUnder = surface is { } after && spark.Position.Y >= after;
            if (wasUnder != isUnder && surface is { } line)
                Leave(room, "splash", new PixelPosition(spark.Position.X, line - Pixels.FromWhole(4)));

            // Every twelfth update, half-way through its turn, a spark leaves a tail behind it.
            if (age % 12 == 6 && Leave(room, "tail", spark.Position) is { } tail)
                _tails.Add((tail, 0));
            if (spark.Position.Y.Whole >= 256)
            {
                room.Remove(spark);
                _sparks.RemoveAt(index);
                continue;
            }

            _sparks[index] = (spark, speed.WithY(speed.Y + Pixels.FromNumber(0.0625)), age + 1);
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
        if (--_soundWait < 0)
        {
            if (QuakeSound.Length > 0)
                room.Session.Sounds.Request(QuakeSound, channel: "room");
            _soundWait = QuakeSoundWaits[_soundTurn] + room.Session.Random.Last.Part(0, 2);
            _soundTurn = (_soundTurn + 1) % QuakeSoundWaits.Length;
        }

        if (_descent == Descent.Holding)
        {
            if (--_wait > 0)
                return;
            ShotType dust = ShotType.Read(room.Content.ReadData(Data().Text("dust")));
            for (int puff = 0; puff < 4; puff++)
                room.Spawn(dust.Launch(PixelPosition.FromWhole(128, 188), PixelVelocity.Zero, HitSide.Neutral, null, this));
            _descent = Descent.Lowering;
            return;
        }

        _sunk += DescentStep;
        if (_sunk.Whole < DescentDistance)
            return;
        if (CrumblePatch.Length > 0)
            room.Terrain.ApplyPatch(CrumblePatch);
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
    private Effect? Leave(Room room, string name, PixelPosition at)
    {
        string file = Data().Text(name);
        if (file.Length == 0)
            return null;
        Effect effect = Effect.Of(FrameTimeline.Read(room.Content.ReadData(file)), at);
        room.Spawn(effect);
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
