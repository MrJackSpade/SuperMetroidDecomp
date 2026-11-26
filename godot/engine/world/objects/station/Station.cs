using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A station: it holds the player while it works, then grants or records its result. What it does is a setting: save
/// the game, reveal the area's map, or refill listed inventory amounts. How the player comes to use it is another:
/// standing in it, or running against one of its sides. It may ask first, show itself working, and say when it is
/// done.
/// </summary>
/// <remarks>
/// A station the player runs against is solid: the solid collider of its scene is given the station's own area.
/// </remarks>
public partial class Station : RoomObject
{
    /// <summary>What a station does when it has finished working.</summary>
    public enum Does
    {
        /// <summary>It asks for the game to be saved, resuming at this station.</summary>
        Saves,

        /// <summary>It reveals the map of the room's area.</summary>
        RevealsMap,

        /// <summary>It fills the listed inventory amounts.</summary>
        Refills,
    }

    /// <summary>How the player comes to use a station.</summary>
    public enum Use
    {
        /// <summary>
        /// Standing still with their middle inside it. The station puts the player at the middle of its width and
        /// turns them to face out while it works. Used or refused, it cannot be used again until the room is entered
        /// anew; nor can it when the room was entered at the place its saved games resume at.
        /// </summary>
        StandingIn,

        /// <summary>Running against its left or right side, facing it and aiming straight ahead.</summary>
        RunningAgainst,
    }

    /// <summary>Which way the player must face to use a station.</summary>
    public enum Faces
    {
        /// <summary>Either way.</summary>
        EitherWay,

        /// <summary>Left.</summary>
        Left,

        /// <summary>Right.</summary>
        Right,
    }

    /// <summary>What the station does.</summary>
    [Export]
    public Does Kind { get; set; }

    /// <summary>The inventory amounts a refilling station fills, by name.</summary>
    [Export]
    public string[] Refills { get; set; } = [];

    /// <summary>How the player comes to use it.</summary>
    [Export]
    public Use HowUsed { get; set; }

    /// <summary>The station's width and height, in pixels; its top-left is where it stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(32, 48);

    /// <summary>How many updates the station works for.</summary>
    [Export]
    public int WorkUpdates { get; set; } = 60;

    /// <summary>Which way the player must face to use it.</summary>
    [Export]
    public Faces MustFace { get; set; }

    /// <summary>
    /// For a station that saves: the entry point of its room a game saved here resumes at. Empty for the station
    /// itself.
    /// </summary>
    [Export]
    public string ResumesAt { get; set; } = "";

    /// <summary>The message that asks whether to use it, by name, answered yes or no; empty when it does not ask.</summary>
    [Export]
    public string AsksFirst { get; set; } = "";

    /// <summary>The message shown once it has finished, by name; empty for none.</summary>
    [Export]
    public string DoneMessage { get; set; } = "";

    /// <summary>How many updates that message stays up before it can be dismissed.</summary>
    [Export]
    public int MessageHoldUpdates { get; set; }

    /// <summary>
    /// Whether the game pauses, on its pause screen, once that message has been dismissed. The station then keeps
    /// hold of the player until that pause has ended.
    /// </summary>
    [Export]
    public bool PausesAfter { get; set; }

    /// <summary>The data file of the timeline its visuals show at rest; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string RestTimeline { get; set; } = "";

    /// <summary>
    /// The data file of the timeline its visuals show while it works; empty for a station whose display at rest runs
    /// on unbroken through its work.
    /// </summary>
    [Export(PropertyHint.File)]
    public string WorkingTimeline { get; set; } = "";

    /// <summary>
    /// The data file of the timeline its visuals show at rest once it has nothing more to do: a map station whose
    /// area's map is revealed. Empty for the one it shows at rest.
    /// </summary>
    [Export(PropertyHint.File)]
    public string DoneTimeline { get; set; } = "";

    /// <summary>The data file of the timeline of an effect it leaves as it starts to work; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string WorkingEffect { get; set; } = "";

    /// <summary>
    /// The data file of the timeline of the effect it leaves instead when the player uses it from its right side;
    /// empty for the same one.
    /// </summary>
    [Export(PropertyHint.File)]
    public string WorkingEffectOnRight { get; set; } = "";

    /// <summary>Where that effect is left, in pixels from where the station stands.</summary>
    [Export]
    public Vector2I WorkingEffectAt { get; set; }

    /// <summary>The sound of it starting to work, by name; empty for none.</summary>
    [Export]
    public string StartSound { get; set; } = "";

    /// <summary>The sound of it finishing, by name; empty for none.</summary>
    [Export]
    public string DoneSound { get; set; } = "";

    /// <summary>How many updates after it has finished that sound is asked for; none for the update it finishes on.</summary>
    [Export]
    public int DoneSoundAfter { get; set; }

    /// <summary>How many updates after it has finished it takes no one: until it has come back to rest.</summary>
    [Export]
    public int RestUpdates { get; set; }

    private Countdown _working = Countdown.Over;

    /// <summary>The updates left before the sound of its finishing is asked for; over when none is waiting.</summary>
    private Countdown _doneSoundWait = Countdown.Over;

    /// <summary>The updates left before it takes anyone again.</summary>
    private Countdown _resting = Countdown.Over;
    private bool _busy;
    private bool _asking;
    private bool _usedThisVisit;
    private TimelinePlayer? _timeline;

    /// <summary>Whether its first update in the room is still to come: what it showed as the room was set up stands through that one.</summary>
    private bool _beforeFirstUpdate;

    /// <summary>The fact recorded when an area's map is revealed, for <paramref name="area"/>.</summary>
    public static string MapRevealedFact(string area) => $"map revealed: {area}";

    /// <summary>The region of the room the station covers.</summary>
    public PixelRect Area => new(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_busy, _asking) = (false, false);
        (_doneSoundWait, _resting) = (Countdown.Over, Countdown.Over);
        // A game that resumes at a station begins with the player standing in it.
        _usedThisVisit = HowUsed == Use.StandingIn && Room.Entry is { } entry && entry.EntryPoint == (ResumesAt.Length > 0 ? ResumesAt : Name);
        if (HowUsed == Use.RunningAgainst)
        {
            Collider solid = EntityNode.ColliderOf(this, ColliderRole.Solid)
                ?? throw new InvalidOperationException($"The station \"{Name}\" is run against, and its scene has no solid collider.");
            solid.Rect = new PixelRect(0, 0, Size.X, Size.Y);
        }

        ShowAtRest();
        _beforeFirstUpdate = true;
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player player = Room.Player;

        // Its first picture is the one its first update shows; its display moves on from the update after.
        if (_beforeFirstUpdate)
            _beforeFirstUpdate = false;
        else
            StepTimeline();

        // The sound of its finishing comes a set time after it, and until it is back at rest it takes no one.
        if (_doneSoundWait.IsRunning)
        {
            _doneSoundWait = _doneSoundWait.Stepped();
            if (_doneSoundWait.IsOver)
                Sound(DoneSound);
        }

        _resting = _resting.Stepped();
        bool resting = _resting.IsRunning;
        // A question still up is answered as it hands back, not here.
        if (_asking)
            return;

        if (_busy)
        {
            _working = _working.Stepped();
            if (_working.IsRunning)
                return;
            _busy = false;
            _usedThisVisit = true;
            Finish(player);
            ShowAtRest();
            if (DoneSoundAfter > 0)
                _doneSoundWait = Countdown.Of(DoneSoundAfter);
            else
                Sound(DoneSound);
            _resting = Countdown.Of(RestUpdates);
            if (DoneMessage.Length > 0)
                Room.Session.RequestMessage(DoneMessage, holdUpdates: MessageHoldUpdates);
            // A station that pauses the game keeps hold of the player through the pause, and lets them go as it ends.
            if (PausesAfter)
                Room.Session.RequestPause(whenOver: player.Release);
            else
                player.Release();
            return;
        }

        if (resting || !FacesRightWay(player) || !CanBeUsed(player))
            return;
        string effect;
        if (HowUsed == Use.StandingIn)
        {
            // Only one who stands plainly is taken: not settling from a landing, and aiming straight ahead.
            if (_usedThisVisit || player.State is not StandingState { IsSettling: false } || player.Aim != Aim.Forward
                || !Area.Contains(player.Position.X.Whole, player.Position.Y.Whole))
            {
                return;
            }

            effect = WorkingEffect;
        }
        else
        {
            if (SideRunAgainst(player) is not { } side)
                return;
            effect = side == Facing.Right && WorkingEffectOnRight.Length > 0 ? WorkingEffectOnRight : WorkingEffect;
        }

        if (AsksFirst.Length > 0)
        {
            // The answer is acted on as the question hands back, in the update that asked it and before the rest of
            // that update is played: a yes takes hold of the player and begins the work there.
            _asking = true;
            Room.Session.RequestQuestion(AsksFirst, yes =>
            {
                _asking = false;
                if (!yes)
                {
                    _usedThisVisit = true;
                    return;
                }

                Hold(Room.Player);
                BeginWork(effect);
            });
            return;
        }

        Hold(player);
        BeginWork(effect);
    }

    /// <summary>
    /// Which side of the station the player has run against, or null when they have not: on the ground, having
    /// moved through this update as one who has run into a wall (<see cref="Player.MovedAgainstWall"/>), facing the
    /// station and against it, aiming straight ahead.
    /// </summary>
    private Facing? SideRunAgainst(Player player)
    {
        if (!player.MovedAgainstWall || player.Aim != Aim.Forward)
            return null;
        PixelRect area = Area, body = player.BodyRect;
        if (body.Bottom <= area.Top || area.Bottom <= body.Top)
            return null;
        if (body.Right == area.Left && player.Facing == Facing.Right)
            return Facing.Left;
        if (body.Left == area.Right && player.Facing == Facing.Left)
            return Facing.Right;
        return null;
    }

    /// <summary>Takes hold of the player; one who stands in the station is put at the middle of its width, facing out.</summary>
    private void Hold(Player player)
    {
        bool standsIn = HowUsed == Use.StandingIn;
        if (standsIn)
            player.Position = PixelPosition.FromWhole(Position.X.Whole + Size.X / 2, player.Position.Y.Whole);
        player.UseStation(facingOut: standsIn);
    }

    private void BeginWork(string effect)
    {
        _busy = true;
        _working = Countdown.Of(WorkUpdates);
        if (WorkingTimeline.Length > 0)
            Show(WorkingTimeline);
        Sound(StartSound);
        if (effect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), Position + PixelOffset.FromWhole(WorkingEffectAt.X, WorkingEffectAt.Y)));
    }

    private bool FacesRightWay(Player player) => MustFace switch
    {
        Faces.EitherWay => true,
        Faces.Left => player.Facing == Facing.Left,
        Faces.Right => player.Facing == Facing.Right,
        _ => throw new InvalidOperationException($"Undefined facing {MustFace}."),
    };

    /// <summary>
    /// Whether the station has anything to do for the player: a refill station does nothing for one who is full, and
    /// a map station nothing once its area's map is revealed.
    /// </summary>
    private bool CanBeUsed(Player player) => Kind switch
    {
        Does.Refills => Refills.Any(amount => !player.Inventory.IsFull(amount)),
        Does.RevealsMap => !MapIsRevealed,
        _ => true,
    };

    /// <summary>Whether the map of the room's area has been revealed.</summary>
    private bool MapIsRevealed => Room.Area.Length > 0 && Room.Session.Progress.Has(MapRevealedFact(Room.Area));

    /// <summary>Shows the station at rest: as one with nothing more to do, where it has a look for that and it is so.</summary>
    private void ShowAtRest() =>
        Show(DoneTimeline.Length > 0 && Kind == Does.RevealsMap && MapIsRevealed ? DoneTimeline : RestTimeline);

    /// <summary>Grants or records the station's result.</summary>
    private void Finish(Player player)
    {
        switch (Kind)
        {
            case Does.Saves:
                Room.Session.RequestSave(Room.SceneFilePath, ResumesAt.Length > 0 ? ResumesAt : Name);
                break;
            case Does.RevealsMap:
                if (Room.Area.Length == 0)
                    throw new InvalidOperationException($"The map station \"{Name}\" is in a room that is in no area.");
                Room.Session.Progress.Record(MapRevealedFact(Room.Area));
                break;
            case Does.Refills:
                foreach (string amount in Refills)
                    player.Inventory.Fill(amount);
                break;
            default:
                throw new InvalidOperationException($"Undefined station kind {Kind}.");
        }
    }

    /// <summary>Starts its visuals on a timeline; empty leaves them as they are.</summary>
    private void Show(string timeline)
    {
        if (timeline.Length == 0)
            return;
        _timeline = new TimelinePlayer(FrameTimeline.Read(Room.Content.ReadData(timeline)));
        ShowStep();
    }

    private void StepTimeline()
    {
        if (_timeline is null)
            return;
        _timeline.Step();
        ShowStep();
    }

    private void ShowStep()
    {
        if (_timeline is null)
            return;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_timeline.Shown);
    }

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}
