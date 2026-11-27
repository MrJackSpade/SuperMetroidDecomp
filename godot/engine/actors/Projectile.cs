using Engine.Actors.Components;
using Engine.Collision;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// Something launched to deliver a hit, living on its own once launched. It moves, hits what it meets, and removes
/// itself. It never decides what its target does about the hit, and never knows what its target is. How it behaves is
/// its <see cref="ProjectileSettings"/>; a projectile with behaviour no setting gives is a class on this one.
/// </summary>
public partial class Projectile : Node2D, IRoomEntity, IHittable, Ui.IRecoloured, ILeavesInPlace
{
    /// <summary>A projectile leaves, in the place it held among the room's, where it was and the speed it had.</summary>
    public PlaceLeftover? LeftInPlace => new PlaceLeftover(Position, Velocity);

    /// <inheritdoc/>
    public void Recolour(string source, IReadOnlyList<Shared.ColourSwap> swaps) => _visual?.Recolour(source, swaps);

    /// <inheritdoc/>
    public bool ShowsPicturesIn(string place) => _visual?.ShowsPicturesIn(place) ?? false;

    private Room? _room;
    private Hit? _hit;
    private ProjectileSettings _settings = new();
    private ICollisionOwner? _launcher;
    private Collider? _hurtbox;
    private ActorVisual? _visual;
    private TimelinePlayer? _timeline;
    private Countdown _left;
    private string _direction = "right";
    private int _width = 8;
    private int _height = 8;
    private bool _shookForTarget;

    /// <summary>The way it was launched along each axis: 1, -1, or 0 for an axis it was not launched along.</summary>
    private int _launchedAcross, _launchedDown;
    private readonly HashSet<IHittable> _alreadyHurt = [];

    /// <summary>A projectile delivering <paramref name="hit"/>, launched from <paramref name="at"/> at <paramref name="velocity"/>.</summary>
    /// <param name="hit">The hit it delivers.</param>
    /// <param name="at">Its centre as it is launched.</param>
    /// <param name="velocity">Its launch speed.</param>
    /// <param name="settings">How it behaves.</param>
    /// <param name="launcher">Whatever launched it, which it never hits.</param>
    public static Projectile Of(Hit hit, PixelPosition at, PixelVelocity velocity, ProjectileSettings settings, ICollisionOwner? launcher)
    {
        var projectile = new Projectile { Name = "Projectile" };
        projectile.Launch(hit, at, velocity, settings, launcher);
        return projectile;
    }

    /// <summary>
    /// Gives a projectile made from a scene, or by a class of its own, what <see cref="Of"/> gives a plain one. Called
    /// once, before it is spawned into its room.
    /// </summary>
    public void Launch(Hit hit, PixelPosition at, PixelVelocity velocity, ProjectileSettings settings, ICollisionOwner? launcher)
    {
        ArgumentNullException.ThrowIfNull(hit);
        ArgumentNullException.ThrowIfNull(settings);
        _hit = hit;
        Position = at;
        Velocity = velocity;
        _settings = settings;
        _launcher = launcher;
        _direction = DirectionName(velocity);
        (_launchedAcross, _launchedDown) = (velocity.X.IsZero ? 0 : velocity.X.IsNegative ? -1 : 1, velocity.Y.IsZero ? 0 : velocity.Y.IsNegative ? -1 : 1);
        (_width, _height) = (settings.Width, settings.Height);
    }

    /// <summary>
    /// Sends the projectile off anew at <paramref name="velocity"/> from where it is: its hit travels that way, and
    /// it is of the size, in the picture or timeline and at the start of the weave that its settings give one
    /// launched that way. Whose it is, what it delivers and how it otherwise behaves stay.
    /// </summary>
    internal void LaunchAnew(PixelVelocity velocity)
    {
        Launch(Hit with { Direction = velocity.OverOneUpdate }, Position, velocity, _settings, _launcher);
        if (_hurtbox is not null)
            _hurtbox.Rect = new PixelRect(-_width / 2, -_height / 2, _width, _height);
        if (_settings.Timeline.Length > 0)
            PlayTimeline(_settings.Timeline);
        else if (_settings.Picture.Length > 0)
            _visual?.Show(Named(_settings.Picture));
    }

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>Its speed.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>The hit it delivers.</summary>
    public Hit Hit => _hit ?? throw new InvalidOperationException("The projectile has no hit.");

    /// <summary>How it behaves.</summary>
    public ProjectileSettings Settings => _settings;

    /// <summary>Whose it is.</summary>
    public HitSide Side => Hit.Side;

    /// <summary>Whatever launched it.</summary>
    public ICollisionOwner? Launcher => _launcher;

    /// <summary>The room it is in.</summary>
    protected Room Room => _room ?? throw new InvalidOperationException("The projectile is not in a room.");

    /// <summary>The pixels it covers, in the room.</summary>
    public PixelRect Area => PixelRect.Around(Position.X.Whole, Position.Y.Whole, _width / 2, _height / 2);

    /// <summary>Whether it has ended.</summary>
    public bool HasEnded { get; private set; }

    /// <summary>How many updates it has been in its room.</summary>
    public int Age { get; private set; }

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        ZIndex = _settings.Standing is { } standing ? RoomDepth.Of(standing) : RoomDepth.Launched;
        _left = Countdown.Of(_settings.LastsUpdates);
        if (_settings.MetByShot != ProjectileSettings.MetBy.Ignored)
        {
            _hurtbox = new Collider
            {
                Name = "Hurtbox",
                Role = ColliderRole.Hurtbox,
                Rect = new PixelRect(-_width / 2, -_height / 2, _width, _height),
            };
            AddChild(_hurtbox);
            room.Collision.Register(_hurtbox);
        }

        string picture = Named(_settings.Picture);
        if (_settings.Timeline.Length > 0)
        {
            _timeline = new TimelinePlayer(FrameTimeline.Read(room.Content.ReadData(Named(_settings.Timeline))));
            _laid.Show(this, _timeline.Current.Lies);
            picture = _timeline.Current.Picture;
        }

        if (picture.Length > 0 || _timeline is not null)
        {
            // A lone picture is centred on the projectile unless its settings say where it lies. A timeline's pictures
            // lie where the timeline puts them, and its settings' offset, if they give one, is added to that.
            (int x, int y) = _settings.PictureOffset ?? (_timeline is null ? (-_settings.Width / 2, -_settings.Height / 2) : (0, 0));
            _visual = new ActorVisual { Picture = picture, PictureOffset = new Vector2I(x, y) };
            AddChild(_visual);
            _visual.Bind(room.Content);
            if (_timeline is not null)
                _visual.Show(_timeline.Current);
        }

        OnLaunched(room);
    }

    /// <summary>
    /// A picture's or timeline's name with its placeholders filled in: "{direction}" with the way the projectile was
    /// launched, and "{side}" with "left" or "right", the way across it is going now.
    /// </summary>
    private string Named(string name) => name
        .Replace("{direction}", _direction, StringComparison.Ordinal)
        .Replace("{side}", Velocity.X.IsNegative ? "left" : "right", StringComparison.Ordinal);

    /// <summary>Where the step it shows lies against the room's tiles, when the step or its timeline says.</summary>
    private readonly LaidDepth _laid = new();

    /// <summary>
    /// Starts the projectile on another timeline, by its data file. It is drawn where each step it shows lies against
    /// the room's tiles, or at its own depth where the step says nothing.
    /// </summary>
    protected void PlayTimeline(string file)
    {
        _timeline = new TimelinePlayer(FrameTimeline.Read(Room.Content.ReadData(Named(file))));
        _laid.Show(this, _timeline.Current.Lies);
        _visual?.Show(_timeline.Current);
    }

    /// <summary>Stops showing the projectile and playing its timeline. It goes on being there, unseen.</summary>
    protected void StopShowing()
    {
        _timeline = null;
        _visual?.Show("");
    }

    /// <summary>Called once as the projectile enters its room.</summary>
    protected virtual void OnLaunched(Room room)
    {
    }

    /// <summary>Plays one update: the projectile changes speed as its motion says, moves, and meets what is in its way.</summary>
    internal void Step()
    {
        Room room = Room;
        if (HasEnded)
            return;

        // One an enemy's answer spent goes now, on its own update, having met the enemies whose turns came after.
        if (_spent != Spent.No)
        {
            SettleSpent(room);
            return;
        }

        // One shot down with no end of its own to play goes now, on its own next update: until here it was still
        // shown, where it was shot, and still held its place among what the room's enemies launch and leave.
        if (_shotDown && _settings.ShotTimeline.Length == 0)
        {
            // One that leaves something when shot down rolls for it as an enemy would.
            if (_settings.Drops is { } drops)
                Pickup.Dropped(room, drops, Position);
            End(room);
            return;
        }

        Age++;
        if (_timeline is not null)
        {
            foreach (string cue in _timeline.Step())
            {
                if (!Resize(cue))
                    OnCue(room, cue);
                if (HasEnded)
                    return;
            }

            if (_timeline.IsOver && _shotDown)
            {
                FinishShotDown(room);
                return;
            }

            if (_timeline.IsOver && _settings.LastsUpdates == 0)
            {
                End(room);
                return;
            }

            // The picture is that of the step this update fell in: it changes on the update the step's cues come.
            _visual?.Show(_timeline.Shown);
            _laid.Show(this, _timeline.Shown.Lies);
        }

        // Shot down, it holds still and only meets what is where it lies.
        if (_shotDown)
        {
            Velocity = PixelVelocity.Zero;
            Travel(room);
            return;
        }

        // One that leads with its edge is tried against the terrain where it was launched, before it first moves.
        if (Age == 1 && _settings.MeetsTerrainBy == ProjectileSettings.TerrainBy.FrontEdge && MeetTerrainAtLaunch(room))
            return;

        if (Age <= _settings.LaunchDelayUpdates)
        {
            // It waits where it was launched, meeting whatever is there, before it sets off.
            PixelVelocity launched = Velocity;
            Velocity = PixelVelocity.Zero;
            Travel(room);
            if (HasEnded)
                return;
            Velocity = launched;
        }
        else
        {
            BeforeTravel(room);
            if (HasEnded)
                return;
            Steer(room);
            Travel(room);
            if (HasEnded)
                return;
            GainSpeed();
        }

        if (_settings.LastsUpdates > 0)
        {
            _left = _left.Stepped();
            if (_left.IsOver)
            {
                End(room);
                return;
            }
        }

        if (_settings.ViewMargin >= 0)
        {
            PixelRect view = room.Session.Camera.View;
            int margin = _settings.ViewMargin;
            int below = _settings.ViewMarginBelow >= 0 ? _settings.ViewMarginBelow : margin;
            var allowed = new PixelRect(view.Left - margin, view.Top - margin, view.Width + margin * 2, view.Height + margin + below);
            if (!allowed.Overlaps(Area))
                Leave(room);
        }
        else if (_settings.ViewMarginBelow >= 0)
        {
            // It may go anywhere but too far below the view.
            PixelRect view = room.Session.Camera.View;
            if (Area.Top >= view.Top + view.Height + _settings.ViewMarginBelow)
                Leave(room);
        }
    }

    /// <summary>
    /// Takes the size a cue gives: "size:&lt;width&gt;:&lt;height&gt;" makes the projectile that many pixels wide and tall from
    /// then on, as a shot that swells or spreads as its timeline plays is. Returns whether the cue was one.
    /// </summary>
    private bool Resize(string cue)
    {
        const string size = "size:";
        if (!cue.StartsWith(size, StringComparison.Ordinal))
            return false;
        string[] parts = cue[size.Length..].Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height) || width <= 0 || height <= 0)
            throw new InvalidOperationException($"A projectile's timeline gives the size cue \"{cue}\", which is not two whole numbers above nothing.");
        (_width, _height) = (width, height);
        if (_hurtbox is not null)
            _hurtbox.Rect = new PixelRect(-width / 2, -height / 2, width, height);
        return true;
    }

    /// <summary>
    /// Makes the projectile a shot of another type from now on, where it is and going as it goes: its hit's damage
    /// and kinds, its size and its timeline or picture become that type's. Its side, whoever launched it and how it
    /// otherwise behaves stay.
    /// </summary>
    protected void BecomeShotOf(ShotType type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _hit = new Hit(type.Damage, Hit.Side, [.. type.Kinds]) { Source = Hit.Source, Direction = Hit.Direction };
        (_width, _height) = (type.Settings.Width, type.Settings.Height);
        if (_hurtbox is not null)
            _hurtbox.Rect = new PixelRect(-_width / 2, -_height / 2, _width, _height);
        if (type.Settings.Timeline.Length > 0)
            PlayTimeline(type.Settings.Timeline);
        else if (type.Settings.Picture.Length > 0)
            _visual?.Show(Named(type.Settings.Picture));
    }

    /// <summary>
    /// Called for each cue the projectile's timeline reaches, but a cue of its size, which the projectile takes itself.
    /// A projectile of its own class acts on its cues here.
    /// </summary>
    protected virtual void OnCue(Room room, string cue)
    {
    }

    /// <summary>Called each update before the projectile changes speed and travels. A projectile of its own class acts here.</summary>
    protected virtual void BeforeTravel(Room room)
    {
    }

    /// <summary>
    /// Whether the projectile's own class moves it (in <see cref="BeforeTravel"/>, as by
    /// <see cref="MoveAgainstTerrain"/>), so that its speed carries it nowhere of itself: it then only meets what is
    /// where its class left it. Its speed is still its speed, for whatever reads it.
    /// </summary>
    protected virtual bool MovesItself => false;

    /// <inheritdoc/>
    public virtual bool IsHitBy(HitSide side) =>
        _settings.MetByShot != ProjectileSettings.MetBy.Ignored && side != Side && !HasEnded && (!_shotDown || _meetingShots)
        && _spent == Spent.No;

    /// <summary>How a shot an enemy's answer has spent goes on its next update.</summary>
    private enum Spent
    {
        /// <summary>It is not spent.</summary>
        No,

        /// <summary>It ends, leaving what it leaves.</summary>
        Ends,

        /// <summary>Its target has shown a dud: it goes with nothing of its own left behind.</summary>
        AsDud,
    }

    /// <summary>
    /// Whether an enemy's answer has spent it, and how it then goes. A shot of the player's is tried against each
    /// enemy in that enemy's turn, and the answer that would end it only spends it: it lies where it is, still
    /// delivering its hit to the enemies whose turns come after and met by no shot of theirs, until its own next
    /// update takes it away. So one shot harms every enemy it lies on in one pass.
    /// </summary>
    private Spent _spent;

    /// <summary>Takes a spent shot away as the answer that spent it has it go.</summary>
    private void SettleSpent(Room room)
    {
        Spent how = _spent;
        _spent = Spent.No;
        if (how == Spent.AsDud)
        {
            OnEnded(room);
            Leave(room);
            return;
        }

        End(room);
    }

    /// <summary>Spends the shot on <paramref name="target"/>, unless something has spent it already; it does not meet that target again.</summary>
    private void Spend(IHittable target, Spent how)
    {
        _alreadyHurt.Add(target);
        if (_spent == Spent.No)
            _spent = how;
    }

    /// <summary>
    /// Whether the projectile delivers its hit at the moment. Its settings say whether it ever does; a projectile of
    /// its own class that is harmless for part of its life says when here.
    /// </summary>
    protected virtual bool DeliversHitNow => _settings.DeliversHit;

    /// <summary>Whether it was shot down and is playing its end out where it is, or waits to go on its next update.</summary>
    private bool _shotDown;

    /// <summary>
    /// Whether it is in its turn of the room's pass of the player's shots against the enemies', having stopped shots
    /// as its turn began: shot down by one of them, it is met by the rest of them all the same.
    /// </summary>
    private bool _meetingShots;

    /// <summary>Whether it harms what it meets now: shot down, it does, whatever its settings said of it.</summary>
    private bool HarmsNow => _shotDown || DeliversHitNow;

    /// <summary>
    /// Whether it stays through every touch of its target from here on, whatever its settings say and though it was
    /// shot down: it harms the one it touches again each time that one can be harmed. A kind whose pictures say when
    /// it becomes so sets it, as a cued projectile does on its timeline's cue.
    /// </summary>
    protected bool OutlastsTouch { get; set; }

    /// <summary>How it meets its target now: one that outlasts a touch stays; else, shot down, it ends on it, whatever its settings said of it.</summary>
    private ProjectileSettings.Target MeetsTargetAs =>
        OutlastsTouch ? ProjectileSettings.Target.PassesThroughAndRehits
        : _shotDown ? ProjectileSettings.Target.Ends
        : _settings.MeetingTarget;

    /// <inheritdoc/>
    public virtual HitResult TakeHit(Hit hit, Collider part)
    {
        Room room = Room;
        switch (_settings.MetByShot)
        {
            case ProjectileSettings.MetBy.Destroyed:
                // Shot down, it stays where it is: it holds still, stops shots no longer, harms what it touches and
                // ends on it. One with an end of its own plays it, from the start again for each shot that strikes
                // it, and leaves what it drops as that end is over, in the place it held; one with none goes on its
                // next update (Step).
                _shotDown = true;
                Velocity = PixelVelocity.Zero;
                if (_settings.ShotTimeline.Length > 0)
                    PlayTimeline(_settings.ShotTimeline);
                return HitResult.Destroyed;
            default:
                return HitResult.Ignored;
        }
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);

    /// <summary>The name of the way a projectile launched at <paramref name="velocity"/> is going, for choosing its picture.</summary>
    public static string DirectionName(PixelVelocity velocity)
    {
        int across = velocity.X.IsZero ? 0 : velocity.X.IsNegative ? -1 : 1;
        int down = velocity.Y.IsZero ? 0 : velocity.Y.IsNegative ? -1 : 1;
        return (across, down) switch
        {
            (1, -1) => "up_right",
            (0, -1) => "up",
            (-1, -1) => "up_left",
            (-1, 0) => "left",
            (-1, 1) => "down_left",
            (0, 1) => "down",
            (1, 1) => "down_right",
            _ => "right",
        };
    }

    /// <summary>Sets the way the projectile goes this update, for a motion that decides it before moving.</summary>
    private void Steer(Room room)
    {
        switch (_settings.Motion)
        {
            case ProjectileSettings.Moves.Straight:
            case ProjectileSettings.Moves.Accelerating:
            case ProjectileSettings.Moves.Falling:
                break;
            case ProjectileSettings.Moves.Still:
                Velocity = PixelVelocity.Zero;
                break;
            case ProjectileSettings.Moves.Homing:
                PixelPosition target = Side == HitSide.Player ? NearestEnemy(room) ?? Position : room.Player.Position;
                Velocity = new PixelVelocity(Toward(Velocity.X, target.X - Position.X), Toward(Velocity.Y, target.Y - Position.Y));
                break;
            default:
                throw new InvalidOperationException($"Undefined projectile motion {_settings.Motion}.");
        }
    }

    /// <summary>
    /// Changes the projectile's speed once it has moved this update, for a motion that gains speed: it leaves at the
    /// speed it was launched with, and the change is felt from the next update.
    /// </summary>
    private void GainSpeed()
    {
        switch (_settings.Motion)
        {
            case ProjectileSettings.Moves.Accelerating:
                Velocity = new PixelVelocity(Faster(Velocity.X), Faster(Velocity.Y));
                break;
            case ProjectileSettings.Moves.Falling:
                bool wasRising = Velocity.Y.IsNegative;
                // The limit is on how fast it falls; however fast it was thrown up, it rises as it was thrown.
                Pixels falling = Velocity.Y + _settings.Acceleration;
                if (!_settings.SpeedLimit.IsZero && falling > _settings.SpeedLimit)
                    falling = _settings.SpeedLimit;
                Velocity = Velocity.WithY(falling);
                if (wasRising && !Velocity.Y.IsNegative && _settings.FallingTimeline.Length > 0 && _visual is not null)
                    PlayTimeline(_settings.FallingTimeline);
                break;
        }
    }

    /// <summary>Moves the projectile for this update a pixel at a time, meeting terrain and targets as it goes.</summary>
    private void Travel(Room room)
    {
        // One whose own class moves it goes nowhere here, and only meets what is where it lies.
        PixelOffset offset = MovesItself ? default : Velocity.OverOneUpdate;
        if (LeadsIntoTerrain)
        {
            TravelLeadingWithEdge(room, offset);
            return;
        }

        PixelPosition target = Position + offset;
        int acrossPixels = target.X.Whole - Position.X.Whole, downPixels = target.Y.Whole - Position.Y.Whole;
        int steps = Math.Max(Math.Abs(acrossPixels), Math.Abs(downPixels));
        for (int step = 1; step <= steps; step++)
        {
            var next = new PixelPosition(
                Pixels.FromWhole(Position.X.Whole + Math.Sign(acrossPixels) * (Math.Abs(acrossPixels) * step / steps - Math.Abs(acrossPixels) * (step - 1) / steps)),
                Pixels.FromWhole(Position.Y.Whole + Math.Sign(downPixels) * (Math.Abs(downPixels) * step / steps - Math.Abs(downPixels) * (step - 1) / steps)));
            if (!Advance(room, next))
                return;
        }

        if (steps == 0)
        {
            if (!Advance(room, Position))
                return;
        }

        Position = new PixelPosition(target.X.WithWhole(Position.X.Whole), target.Y.WithWhole(Position.Y.Whole));
    }

    /// <summary>
    /// Moves a projectile that meets terrain where each move ends: the whole of its move across, then, if nothing
    /// stopped it, the whole of its move down. Along each it meets solids and targets a pixel at a time, and the
    /// terrain once, by its leading edge or its middle, where the move ended.
    /// </summary>
    private void TravelLeadingWithEdge(Room room, PixelOffset offset)
    {
        if (offset.X.IsZero && offset.Y.IsZero)
        {
            // Holding still, it only meets what is where it lies.
            Advance(room, Position);
            return;
        }

        if (!offset.X.IsZero && !MoveLeadingWithEdge(room, offset.X, across: true))
            return;
        if (!offset.Y.IsZero)
            MoveLeadingWithEdge(room, offset.Y, across: false);
    }

    /// <summary>Moves the projectile the whole of <paramref name="distance"/> along one axis and tries its leading edge there. Returns whether it carries on.</summary>
    private bool MoveLeadingWithEdge(Room room, Pixels distance, bool across)
    {
        PixelPosition from = Position;
        PixelPosition target = across ? new PixelPosition(from.X + distance, from.Y) : new PixelPosition(from.X, from.Y + distance);
        int pixels = across ? target.X.Whole - from.X.Whole : target.Y.Whole - from.Y.Whole;
        for (int moved = 0; moved < Math.Abs(pixels); moved++)
        {
            PixelPosition next = across
                ? new PixelPosition(Pixels.FromWhole(Position.X.Whole + Math.Sign(pixels)), from.Y)
                : new PixelPosition(from.X, Pixels.FromWhole(Position.Y.Whole + Math.Sign(pixels)));
            if (!Advance(room, next))
                return false;
        }

        if (pixels == 0 && !Advance(room, Position))
            return false;
        Position = target;
        bool carriesOn = _settings.MeetsTerrainBy switch
        {
            ProjectileSettings.TerrainBy.FrontEdge => !MeetTerrainWithEdge(room, across, backward: distance.IsNegative),
            ProjectileSettings.TerrainBy.Middle => !MeetTerrainAtMiddle(room),
            _ => !MeetTerrainWithSide(room, across, backward: distance.IsNegative, from),
        };
        OnMovedToMeetTerrain(room, across, distance);
        return carriesOn && !HasEnded;
    }

    /// <summary>
    /// Called for a projectile that meets terrain where each move ends, once it has moved the whole of
    /// <paramref name="distance"/> across or down and been tried against the terrain there, whether or not that ended
    /// it; not called for a move that something along its way cut short. A projectile of its own class that tries
    /// more of itself against the terrain than its settings say does so here (<see cref="StrikeTerrainAt"/>).
    /// </summary>
    protected virtual void OnMovedToMeetTerrain(Room room, bool across, Pixels distance)
    {
    }

    /// <summary>Whether it meets terrain where each move ends, by what it leads with or by its middle, and not along the way.</summary>
    private bool LeadsIntoTerrain =>
        _settings.MeetsTerrainBy is ProjectileSettings.TerrainBy.FrontEdge or ProjectileSettings.TerrainBy.LeadingSide
            or ProjectileSettings.TerrainBy.Middle;

    /// <summary>
    /// Tries the projectile's middle against the terrain where its move left it. Stopped, it ends there, inside what
    /// it met, and its hit lands on the terrain at that point. Returns whether it was stopped, and so has ended.
    /// </summary>
    private bool MeetTerrainAtMiddle(Room room)
    {
        if (!StrikeTerrainAt(room, Position))
            return false;
        _settings.TerrainShake?.Start(room);
        OnMetTerrain(room);
        End(room);
        return true;
    }

    /// <summary>
    /// Tries the point at <paramref name="point"/> against the terrain as the middle of a projectile that meets
    /// terrain by its middle is tried: the terrain there takes the projectile's hit if it stops the projectile there,
    /// or if the projectile passes through terrain and hits it. Nothing else comes of it here: the projectile neither
    /// moves nor ends, and nothing is told. Returns whether the terrain stops it at that point. For a projectile
    /// whose own class tries a point besides its middle, which says what being stopped there does.
    /// </summary>
    protected bool StrikeTerrainAt(Room room, PixelPosition point)
    {
        ArgumentNullException.ThrowIfNull(room);
        var at = new PixelRect(point.X.Whole, point.Y.Whole, 1, 1);
        bool passes = _settings.MeetingTerrain is ProjectileSettings.Terrain.PassesThrough or ProjectileSettings.Terrain.PassesThroughAndHits;
        bool hitsTerrain = HarmsNow &&
            _settings.MeetingTerrain is ProjectileSettings.Terrain.EndsAndHits or ProjectileSettings.Terrain.PassesThroughAndHits;
        bool stopped = !passes && StopsWithin(room, at);
        if (hitsTerrain && (passes || stopped))
            room.Terrain.TakeHit(Hit, at);
        return stopped;
    }

    /// <summary>The strip one pixel thick the projectile leads with, where it stands.</summary>
    private PixelRect LeadingStrip(bool across, bool backward)
    {
        PixelRect area = Area;
        return across
            ? new PixelRect(backward ? area.Left : area.Right - 1, area.Top, 1, area.Height)
            : new PixelRect(area.Left, backward ? area.Top : area.Bottom - 1, area.Width, 1);
    }

    /// <summary>
    /// Tries the side the projectile leads with against the terrain, where its move from <paramref name="from"/> left
    /// it: any tile the side lies in that stops a body stops it, and it rests against that tile on whole pixels,
    /// unless that would put it back behind where the move began. Returns whether it was stopped, and so has ended.
    /// </summary>
    private bool MeetTerrainWithSide(Room room, bool across, bool backward, PixelPosition from)
    {
        PixelRect side = LeadingStrip(across, backward);
        int way = backward ? -1 : 1;
        bool passes = _settings.MeetingTerrain is ProjectileSettings.Terrain.PassesThrough or ProjectileSettings.Terrain.PassesThroughAndHits;
        bool hitsTerrain = HarmsNow &&
            _settings.MeetingTerrain is ProjectileSettings.Terrain.EndsAndHits or ProjectileSettings.Terrain.PassesThroughAndHits;
        EdgeStop stop = passes ? EdgeStop.None : room.Terrain.StopsEdge(side, across ? way : 0, across ? 0 : way, SlopeTreatment.Wall);
        if (hitsTerrain && (passes || stop.Stopped))
            room.Terrain.TakeHit(Hit, side);
        if (!stop.Stopped)
            return false;

        RestAgainst(stop, across, way, from);
        _settings.TerrainShake?.Start(room);
        OnMetTerrain(room);
        End(room);
        return true;
    }

    /// <summary>
    /// Puts the projectile where a tile that stopped its leading side leaves it: against that tile on whole pixels,
    /// its part of a pixel that way dropped, and no further back than where its move began.
    /// </summary>
    private void RestAgainst(EdgeStop stop, bool across, int way, PixelPosition from)
    {
        int whole = across ? Position.X.Whole : Position.Y.Whole, began = across ? from.X.Whole : from.Y.Whole;
        int rested = stop.Back is { } back ? whole - way * back : whole;
        if (way > 0 ? rested < began : rested > began)
            rested = began;
        Position = across
            ? new PixelPosition(Pixels.FromWhole(rested), Position.Y)
            : new PixelPosition(Position.X, Pixels.FromWhole(rested));
    }

    /// <summary>
    /// Moves the projectile by <paramref name="distance"/>, across or down, and tries the side it then leads with
    /// against the terrain as one that meets terrain by its leading side is tried
    /// (<see cref="ProjectileSettings.TerrainBy.LeadingSide"/>): a move of no distance tries the side where it lies.
    /// Stopped, it rests against the tile on whole pixels, no further back than where it began; it does not end, its
    /// hit lands on nothing and nothing is told. For a projectile whose own class moves it so and says what meeting
    /// terrain then does. Returns whether terrain stopped it.
    /// </summary>
    protected bool MoveAgainstTerrain(Room room, Pixels distance, bool across)
    {
        ArgumentNullException.ThrowIfNull(room);
        PixelPosition from = Position;
        Position = across ? new PixelPosition(Position.X + distance, Position.Y) : new PixelPosition(Position.X, Position.Y + distance);
        int way = distance.IsNegative ? -1 : 1;
        EdgeStop stop = room.Terrain.StopsEdge(LeadingStrip(across, distance.IsNegative), across ? way : 0, across ? 0 : way, SlopeTreatment.Wall);
        if (!stop.Stopped)
            return false;
        RestAgainst(stop, across, way, from);
        return true;
    }

    /// <summary>
    /// Tries a projectile that leads with its edge against the terrain where it was launched, before it has moved:
    /// across first, if it is launched across, then down if that did not stop it. Not yet moving, it is tried as one
    /// going right and down is, whichever way it will go, but for one launched straight left, which is tried going
    /// left. Returns whether the terrain stopped it.
    /// </summary>
    private bool MeetTerrainAtLaunch(Room room) =>
        (_launchedAcross != 0 && MeetTerrainWithEdge(room, across: true, backward: _launchedAcross < 0 && _launchedDown == 0))
        || (!HasEnded && _launchedDown != 0 && MeetTerrainWithEdge(room, across: false, backward: false));

    /// <summary>
    /// Tries the edge the projectile leads with against the terrain, where it stands: its hit lands on every tile the
    /// edge reached, and the terrain stops it only if every one of them does. Returns whether it was stopped, and so
    /// has ended.
    /// </summary>
    /// <param name="room">The room.</param>
    /// <param name="across">Whether it is going across, and leads with a side; otherwise with its top or bottom.</param>
    /// <param name="backward">Whether it is going left, or up.</param>
    private bool MeetTerrainWithEdge(Room room, bool across, bool backward)
    {
        PixelRect edge = LeadingStrip(across, backward);
        // A square of the room a way out lies over stops the edge, and so does one that something standing as part
        // of the room lies in, a door's cap or a gate: such a thing is blocks to the shot, like any others.
        ShotEdgeMeeting met = room.Terrain.MeetShotEdge(
            edge, across, Position.X.Whole, Position.Y.Whole, square => room.HasExitOver(square) || room.HasStandingSolidOver(square, _launcher));
        bool passes = _settings.MeetingTerrain is ProjectileSettings.Terrain.PassesThrough or ProjectileSettings.Terrain.PassesThroughAndHits;
        bool hitsTerrain = HarmsNow &&
            _settings.MeetingTerrain is ProjectileSettings.Terrain.EndsAndHits or ProjectileSettings.Terrain.PassesThroughAndHits;
        if (hitsTerrain && !met.Reached.IsEmpty)
            room.Terrain.TakeHit(Hit, met.Reached);

        // Its hit lands on whatever standing thing lies in the squares the edge reached, as on the tiles there.
        if (!met.Reached.IsEmpty)
            HitStandingThings(room, SquaresOf(room, met.Reached));
        if (passes || !met.Stops)
            return false;

        // It ends where its move left it: what it leaves is put at its front, if its settings say so.
        _settings.TerrainShake?.Start(room);
        OnMetTerrain(room);
        End(room);
        return true;
    }

    /// <summary>
    /// Whether the terrain stops the projectile where it stands, for one that meets it by its body or its middle: its
    /// body by anything solid in it and by the room's edge; its middle by what is solid at that point or a way out of
    /// the room there, and by nothing outside the room.
    /// </summary>
    private bool TerrainStops(Room room)
    {
        if (_settings.MeetsTerrainBy == ProjectileSettings.TerrainBy.Body)
            return room.Terrain.IsSolidIn(Area, SlopeTreatment.Wall);
        return StopsWithin(room, TerrainArea);
    }

    /// <summary>
    /// Whether the terrain stops within <paramref name="area"/> a projectile that does not meet it by its body: by
    /// what is solid there or a way out of the room there, and by nothing outside the room.
    /// </summary>
    private static bool StopsWithin(Room room, PixelRect area)
    {
        if (area.Left < 0 || area.Top < 0 || area.Right > room.Terrain.WidthInPixels || area.Bottom > room.Terrain.HeightInPixels)
            return false;
        return room.Terrain.IsSolidIn(area, SlopeTreatment.Wall) || room.HasExitOver(area);
    }

    /// <summary>
    /// Whether it meets the things that stand as part of the room, a door's cap or a gate, as it meets terrain: where
    /// each move ends, by the squares its edge reached, and not at the first pixel its body touches them. Only one
    /// that leads with its edge does. One tried by its middle gathers speed until a single move can step over such a
    /// thing's whole square, so it still meets them as it moves, as does one that leads with its side.
    /// </summary>
    private bool MeetsStandingThingsAtEnd => _settings.MeetsTerrainBy == ProjectileSettings.TerrainBy.FrontEdge;

    /// <summary>Whether <paramref name="target"/> is a thing that stands as part of the room: one with a solid that stands as terrain.</summary>
    private static bool StandsAsPartOfRoom(IHittable target) => World.Room.IsStandingThing(target);

    /// <summary>The whole squares of the room's tiles that <paramref name="area"/> lies in.</summary>
    private static PixelRect SquaresOf(Room room, PixelRect area)
    {
        int size = room.Terrain.TileSize;
        int left = (int)Math.Floor(area.Left / (double)size) * size, top = (int)Math.Floor(area.Top / (double)size) * size;
        int right = (int)Math.Ceiling(area.Right / (double)size) * size, bottom = (int)Math.Ceiling(area.Bottom / (double)size) * size;
        return new PixelRect(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Lands the hit of a shot of the player's on the things standing as part of the room that lie in
    /// <paramref name="squares"/>, each once: a door's cap or a gate's switch is shot so. What such a thing answers
    /// does not end the shot; the squares it lies in do, as blocks do.
    /// </summary>
    private void HitStandingThings(Room room, PixelRect squares)
    {
        if (!MeetsStandingThingsAtEnd || !HarmsNow || Side == HitSide.Enemies)
            return;
        _alreadyHurt.Add(this);
        foreach ((IHittable target, HitResult result) in room.DeliverHit(
            Hit with { At = Position }, squares, from: _launcher, skipping: _alreadyHurt, landsOn: StandsAsPartOfRoom).ToList())
        {
            OnMetTarget(room, target, result);
            if (result == HitResult.Ignored)
                continue;
            if (!_shookForTarget && _settings.TerrainShake is { } shake)
            {
                _shookForTarget = true;
                shake.Start(room);
            }

            _alreadyHurt.Add(target);
        }
    }

    /// <summary>
    /// Moves the projectile to <paramref name="next"/> and meets what is there. Returns whether it carries on travelling
    /// this update.
    /// </summary>
    private bool Advance(Room room, PixelPosition next)
    {
        PixelPosition previous = Position;
        Position = next;
        PixelRect area = Area;
        bool passes = _settings.MeetingTerrain is ProjectileSettings.Terrain.PassesThrough or ProjectileSettings.Terrain.PassesThroughAndHits;
        bool hitsTerrain = HarmsNow &&
            _settings.MeetingTerrain is ProjectileSettings.Terrain.EndsAndHits or ProjectileSettings.Terrain.PassesThroughAndHits;

        // One that leads with its edge or its side meets the terrain where its move ends, not along the way.
        if (!LeadsIntoTerrain)
        {
            if (passes && hitsTerrain)
                room.Terrain.TakeHit(Hit, TerrainArea);

            if (!passes && TerrainStops(room))
                return StopAgainst(room, previous, next, hitsTerrain);
        }

        // A solid that stands as part of the room may be the very thing the shot is for, as a door's cap is. So what
        // is there is hit first, and only then does the solid stop the shot.
        // No other solid is a wall to a shot: it is solid to the player alone, and stops a shot only by what it does
        // when the shot hits it.
        // One that meets such things where its move ends, as it meets terrain, neither hits nor is stopped by them here.
        bool meetsSolid = !passes && !MeetsStandingThingsAtEnd &&
            room.Collision.Overlapping(area, ColliderRole.Solid, except: _launcher).Any(solid => solid.StandsAsTerrain);

        // An enemy's shot is not tried against what it harms as it moves: the room tries each where it stands once
        // all have moved. The player's shot meets what it comes upon as it moves, but for enemies and their shots:
        // each enemy tries the shots against itself in its own turn, and the room tries them against the enemies'
        // shots once all have moved. An enemy that takes its turn with the room's objects is a fitting of the room
        // and is met as the shot moves, as they are: it may lie within something of the room that stops the shot, as
        // an eye door's eye lies within its cap, and the shot would be gone before that enemy's turn came.
        if (!HarmsNow || Side == HitSide.Enemies)
            return !meetsSolid || StopAgainst(room, previous, next, hitsTerrain);
        if (!MeetTargets(room, area, target => target is not Projectile and not Enemy { ActsWithObjects: false }
            && !(MeetsStandingThingsAtEnd && StandsAsPartOfRoom(target))))
        {
            return false;
        }

        return !meetsSolid || StopAgainst(room, previous, next, hitsTerrain);
    }

    /// <summary>
    /// Tries each of the player's shots against this one of the enemies' shots, in its turn of the room's pass.
    /// Whether it stops shots at all is asked once, as its turn begins: the first of the player's shots to strike it
    /// shoots it down, and the rest that stand in its square on the same update are spent on it all the same, each
    /// starting its end over.
    /// </summary>
    internal void MeetShots(IEnumerable<Projectile> shots)
    {
        if (!IsHitBy(HitSide.Player))
            return;
        _meetingShots = true;
        foreach (Projectile shot in shots)
            shot.MeetShot(this);
        _meetingShots = false;
    }

    /// <summary>
    /// Tries the player's shot against one of the enemies' shots, where each stands. The room does this for every
    /// pair once all have moved and the enemies' shots have been tried against the player. The two meet when they
    /// stand in the same square of the room (<see cref="Engine.Core.GameDefinition.ShotMeetingSquare"/>), whatever their sizes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The game's definition gives the squares no size.</exception>
    private void MeetShot(Projectile enemyShot)
    {
        // One an enemy's answer has spent meets no shot of theirs.
        if (HasEnded || enemyShot.HasEnded || !HarmsNow || !MeetsShots || Side != HitSide.Player || _spent != Spent.No)
            return;
        int square = Room.Session.Definition.ShotMeetingSquare;
        if (square <= 0)
            throw new InvalidOperationException("The game's definition gives no size for the squares in which shots meet.");
        if (SquareOf(Position.X.Whole, square) == SquareOf(enemyShot.Position.X.Whole, square)
            && SquareOf(Position.Y.Whole, square) == SquareOf(enemyShot.Position.Y.Whole, square))
        {
            // It lands on the other where the other stands: the squares, not the two shapes, decided that they met.
            // What the other makes of the hit is the other's own. The shot itself is only marked: it goes on its own
            // next step, with nothing left behind. One that passes through what it meets is not even marked.
            _alreadyHurt.Add(this);
            bool landed = false;
            foreach ((IHittable _, HitResult result) in Room.DeliverHit(
                         Hit with { At = Position }, enemyShot.Area, from: _launcher, skipping: _alreadyHurt,
                         landsOn: target => ReferenceEquals(target, enemyShot)).ToList())
            {
                landed |= result != HitResult.Ignored;
            }

            if (landed && MeetsTargetAs == ProjectileSettings.Target.Ends)
                Spend(enemyShot, Spent.AsDud);
        }
    }

    /// <summary>
    /// Whether the projectile is tried against the other side's shots at all. A projectile of its own class that
    /// never meets them, as a bomb does not, says so here.
    /// </summary>
    protected virtual bool MeetsShots => true;

    /// <summary>The square along one axis that holds a whole pixel, counted from the room's corner.</summary>
    private static int SquareOf(int pixel, int square) => (int)Math.Floor(pixel / (double)square);

    /// <summary>
    /// Tries the player's shot against one enemy, where the shot stands. The enemy asks, through the room, in its
    /// own turn.
    /// </summary>
    internal void MeetEnemy(Enemy enemy)
    {
        if (!HasEnded && HarmsNow && Side == HitSide.Player)
            MeetTargets(Room, Area, target => ReferenceEquals(target, enemy), spends: true);
    }

    /// <summary>
    /// Tries an enemy's shot against what it harms, where it stands. The room does this for each once all have moved
    /// and the room's objects have played.
    /// </summary>
    internal void MeetTargetsWhereItStands()
    {
        if (!HasEnded && HarmsNow)
            MeetTargets(Room, Area, landsOn: null);
    }

    /// <summary>
    /// Delivers the projectile's hit to what lies in <paramref name="area"/>, of the targets <paramref name="landsOn"/>
    /// lets it be tried against, and does what each answer calls for. Returns whether it carries on travelling this
    /// update. Where <paramref name="spends"/> is asked, as it is of the player's shot tried against one enemy in
    /// that enemy's turn, an answer that would end it only spends it (<see cref="_spent"/>).
    /// </summary>
    private bool MeetTargets(Room room, PixelRect area, Func<IHittable, bool>? landsOn, bool spends = false)
    {
        if (_settings.HitsEachUpdate)
            _alreadyHurt.Clear();
        _alreadyHurt.Add(this);
        foreach ((IHittable target, HitResult result) in room.DeliverHit(Hit with { At = Position }, area, from: _launcher, skipping: _alreadyHurt, landsOn: landsOn).ToList())
        {
            OnMetTarget(room, target, result);
            // A thing of the room its hit lands on, a door's cap or a gate, is part of the room as its blocks are, and
            // shakes it as terrain does; anything else, an enemy, as a target does.
            if (result != HitResult.Ignored && !_shookForTarget
                && (target is RoomObject ? _settings.TerrainShake : _settings.TargetShake) is { } shake)
            {
                _shookForTarget = true;
                shake.Start(room);
            }

            if (_settings.HitsEachUpdate)
            {
                _alreadyHurt.Add(target);
                continue;
            }

            switch (result)
            {
                case HitResult.Ignored:
                    break;
                case HitResult.TurnedBack:
                    // Its target sends it another way, and the player whose shot it is launches it anew that way:
                    // it is still theirs, and meets nothing more this update.
                    if (target is not ITurnsShots turner || _launcher is not Player player)
                        throw new InvalidOperationException($"\"{(target as Node)?.Name}\" turned back a hit of \"{Name}\", and either does not say which way or the shot is no player's.");
                    (int across, int down) = turner.TurnedWay(Hit);
                    OnTurned(player, across, down);
                    return false;
                case HitResult.Absorbed:
                    if (spends)
                    {
                        Spend(target, Spent.Ends);
                        return false;
                    }

                    End(room);
                    return false;
                case HitResult.Dud:
                    // One that outlasts a dud plays on: its target has shown the dud and is not met by it again.
                    if (_settings.OutlastsDud)
                    {
                        _alreadyHurt.Add(target);
                        break;
                    }

                    if (spends)
                    {
                        Spend(target, Spent.AsDud);
                        return false;
                    }

                    // Its target has shown the dud: it is gone with nothing of its own left behind.
                    OnEnded(room);
                    Leave(room);
                    return false;
                case HitResult.Hurt:
                case HitResult.Destroyed:
                    if (MeetsTargetAs == ProjectileSettings.Target.Ends || target.Stops(Hit))
                    {
                        if (spends)
                        {
                            Spend(target, Spent.Ends);
                            return false;
                        }

                        End(room);
                        return false;
                    }

                    if (MeetsTargetAs == ProjectileSettings.Target.PassesThrough)
                        _alreadyHurt.Add(target);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined hit result {result}.");
            }
        }

        return true;
    }

    /// <summary>What of the projectile meets terrain along its way: all it covers, or its middle point alone when its settings say so.</summary>
    private PixelRect TerrainArea =>
        _settings.MeetsTerrainBy == ProjectileSettings.TerrainBy.Middle ? new PixelRect(Position.X.Whole, Position.Y.Whole, 1, 1) : Area;

    /// <summary>
    /// Stops the projectile against terrain or something solid it has met in moving from <paramref name="previous"/>
    /// to <paramref name="next"/>: it bounces off, or it ends there. Always returns false: it travels no further this update.
    /// </summary>
    private bool StopAgainst(Room room, PixelPosition previous, PixelPosition next, bool hitsTerrain)
    {
        // One that leads with its edge is stopped here only by something solid that is not terrain, which its
        // hit has already been tried against.
        if (hitsTerrain && !LeadsIntoTerrain)
            room.Terrain.TakeHit(Hit, TerrainArea);
        if (_settings.MeetingTerrain == ProjectileSettings.Terrain.Bounces)
        {
            bool acrossBlocked = room.Terrain.IsSolidIn(TerrainArea.MovedBy(0, previous.Y.Whole - next.Y.Whole), SlopeTreatment.Wall);
            Velocity = acrossBlocked ? Velocity.WithX(-Velocity.X) : Velocity.WithY(-Velocity.Y);
            Position = previous;
            return false;
        }

        Position = previous;
        _settings.TerrainShake?.Start(room);
        OnMetTerrain(room);
        End(room);
        return false;
    }

    /// <summary>
    /// Moves the projectile across by <paramref name="distance"/> a pixel at a time, stopping short of terrain. For a
    /// projectile of its own class that moves itself and meets terrain its own way. Returns whether terrain stopped it.
    /// </summary>
    protected bool MoveAcrossMeetingTerrain(Room room, Pixels distance) => MoveMeetingTerrain(room, new PixelOffset(distance, Pixels.Zero));

    /// <summary>
    /// Moves the projectile down by <paramref name="distance"/>, upward when negative, a pixel at a time, stopping short
    /// of terrain. Returns whether terrain stopped it.
    /// </summary>
    protected bool MoveDownMeetingTerrain(Room room, Pixels distance) => MoveMeetingTerrain(room, new PixelOffset(Pixels.Zero, distance));

    /// <summary>Moves the projectile along one axis until terrain is in the way.</summary>
    private bool MoveMeetingTerrain(Room room, PixelOffset offset)
    {
        PixelPosition target = Position + offset;
        int across = target.X.Whole - Position.X.Whole, down = target.Y.Whole - Position.Y.Whole;
        for (int moved = 0; moved < Math.Abs(across) + Math.Abs(down); moved++)
        {
            if (room.Terrain.IsSolidIn(Area.MovedBy(Math.Sign(across), Math.Sign(down)), SlopeTreatment.Wall))
            {
                // It rests against what stopped it, with nothing left over.
                Position = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
                return true;
            }

            Position += PixelOffset.FromWhole(Math.Sign(across), Math.Sign(down));
        }

        Position = new PixelPosition(target.X.WithWhole(Position.X.Whole), target.Y.WithWhole(Position.Y.Whole));
        return false;
    }

    /// <summary>Called as the projectile is stopped by terrain or a solid, before it ends.</summary>
    protected virtual void OnMetTerrain(Room room)
    {
    }

    /// <summary>
    /// Called when what its hit landed on turned it another way (<see cref="ITurnsShots"/>): -1, 0 or 1 across and
    /// down. A shot of the player's weapons is launched anew that way by them; a projectile of its own class that is
    /// no weapon's shot says here what becomes of it.
    /// </summary>
    protected virtual void OnTurned(Player player, int across, int down)
    {
        ArgumentNullException.ThrowIfNull(player);
        player.Weapons.LaunchAnew(player, this, across, down);
    }

    /// <summary>Called for each target the projectile's hit landed on, with what the hit did.</summary>
    protected virtual void OnMetTarget(Room room, IHittable target, HitResult result)
    {
    }

    /// <summary>
    /// Ends a projectile that was shot down, once its end has played out: what it drops is rolled and left where it
    /// lies, and it is gone with nothing else left behind. Its drop is a thing of its own among what the room's
    /// enemies launch and leave: it takes a free place while the projectile still holds its own, and where the room
    /// has none free nothing is rolled and nothing is left. A roll of nothing takes that place all the same, for the
    /// pause of a spent drop.
    /// </summary>
    private void FinishShotDown(Room room)
    {
        bool holdsPlace = room.PlaceOf(this) is not null;
        if (_settings.Drops is { } drops && (!holdsPlace || room.HasFreePlace))
        {
            Inventory inventory = room.Player.Inventory;
            string? drop = drops.Roll(room.Session.Random, inventory, file => PickupType.Read(room.Content.ReadData(file)).IsUsefulTo(inventory));
            if (drop is not null)
                room.Spawn(Pickup.Of(PickupType.Read(room.Content.ReadData(drop)), Position));
            else if (holdsPlace)
                room.HoldFreePlace(Position);
        }

        OnEnded(room);
        Leave(room);
    }

    /// <summary>Ends the projectile where it is, leaving its ending effect, burst and sound if it has them.</summary>
    protected void End(Room room)
    {
        if (HasEnded)
            return;

        // The player who launched it is told that their shot ends here.
        (_launcher as Player)?.OnShotEnded(this);

        // What it leaves lies at its middle, or at its front: half its size on along each way it was launched.
        PixelPosition at = _settings.EndsAtFront
            ? Position + PixelOffset.FromWhole(_launchedAcross * (_width / 2), _launchedDown * (_height / 2))
            : Position;

        // An effect that holds a place is made once the projectile is out of play, so the place it held is free
        // for it; with none free it is not made.
        if (_settings.EndingEffect.Length > 0 && _settings.EndingEffectHoldsPlace)
            room.Remove(this);
        LeaveEnding(room, at);
        OnEnded(room);
        Leave(room);
    }

    /// <summary>Leaves at <paramref name="at"/> what the projectile's settings say its ending leaves: its effect, its burst and its sound.</summary>
    private void LeaveEnding(Room room, PixelPosition at)
    {
        if (_settings.EndingEffect.Length > 0)
        {
            Effect effect = Effect.Of(
                FrameTimeline.Read(room.Content.ReadData(_settings.EndingEffect)), at, _settings.EndingEffectHoldsPlace, _settings.EndingEffectStanding);
            if (room.Spawn(effect))
                LeftOnEnding?.Invoke(effect);
        }

        if (_settings.EndingBurst.Length > 0)
        {
            Projectile burst = ShotType.Read(room.Content.ReadData(_settings.EndingBurst)).Launch(at, PixelVelocity.Zero, Side, _launcher, Hit.Source);
            if (room.Spawn(burst))
                LeftOnEnding?.Invoke(burst);
        }

        if (_settings.EndingSound.Length > 0)
            room.Session.Sounds.Request(_settings.EndingSound);
    }

    /// <summary>
    /// Has the projectile burst against terrain at <paramref name="at"/> while it goes on itself: the room shakes as
    /// its settings say terrain met shakes it, and what its ending leaves, its effect, its burst and its sound, is
    /// left there, counted with what it leaves as it ends (<see cref="LeftOnEnding"/>). For a projectile whose own
    /// class tries a point besides its middle (<see cref="StrikeTerrainAt"/>).
    /// </summary>
    protected void BurstAgainstTerrainAt(Room room, PixelPosition at)
    {
        ArgumentNullException.ThrowIfNull(room);
        _settings.TerrainShake?.Start(room);
        LeaveEnding(room, at);
    }

    /// <summary>
    /// Told of each thing the projectile leaves in the room as it ends, its effect and its burst: for whoever counts
    /// the projectile as out until what it left is gone too, or shows what it left in colours of their own.
    /// </summary>
    public event Action<Node>? LeftOnEnding;

    /// <summary>Called as the projectile ends, after it has left what its settings say it leaves.</summary>
    protected virtual void OnEnded(Room room)
    {
    }

    /// <summary>Takes the projectile out of the room with no ending.</summary>
    protected void Leave(Room room)
    {
        HasEnded = true;
        room.Remove(this);
    }

    /// <summary>A speed along one axis, gained on in the direction it already has.</summary>
    private Pixels Faster(Pixels speed)
    {
        Pixels gain = _direction.Contains('_', StringComparison.Ordinal) ? _settings.DiagonalAcceleration ?? _settings.Acceleration : _settings.Acceleration;
        return speed.IsZero ? speed : Limit(speed.IsNegative ? speed - gain : speed + gain);
    }

    /// <summary>A speed along one axis, steered toward where its target lies along that axis.</summary>
    private Pixels Toward(Pixels speed, Pixels toTarget) =>
        toTarget.IsZero ? speed : Limit(toTarget.IsNegative ? speed - _settings.Acceleration : speed + _settings.Acceleration);

    /// <summary>A speed along one axis, held to the settings' limit.</summary>
    private Pixels Limit(Pixels speed)
    {
        Pixels limit = _settings.SpeedLimit;
        if (limit.IsZero)
            return speed;
        return speed > limit ? limit : speed < -limit ? -limit : speed;
    }

    /// <summary>Where the nearest live enemy is, or null when there is none.</summary>
    private PixelPosition? NearestEnemy(Room room)
    {
        Enemy? nearest = null;
        long best = long.MaxValue;
        foreach (Enemy enemy in room.Enemies)
        {
            if (enemy.IsDead)
                continue;
            long across = enemy.Position.X.Whole - Position.X.Whole, down = enemy.Position.Y.Whole - Position.Y.Whole;
            long distance = across * across + down * down;
            if (distance < best)
            {
                best = distance;
                nearest = enemy;
            }
        }

        return nearest?.Position;
    }
}
