using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// One room of the game: a scene. It owns its terrain, everything placed in it, its collision, and the lists of what
/// is alive in it. It sets itself up when entered, steps everything in it in a fixed order, and forgets everything
/// when it is left; only facts recorded in progress survive. It holds no enemy's or object's behaviour.
/// </summary>
public partial class Room : Node2D
{
    /// <summary>The name of the area the room is in; empty for a room on no map.</summary>
    [Export]
    public string Area { get; set; } = "";

    /// <summary>The map cell of the room's top-left corner.</summary>
    [Export]
    public Vector2I MapCell { get; set; }

    /// <summary>The width and height of one map cell, in room pixels.</summary>
    [Export]
    public Vector2I MapCellSize { get; set; } = new(256, 256);

    /// <summary>The room's versions, in order; the first whose condition holds is used. Empty for a room with one version.</summary>
    [Export]
    public Godot.Collections.Array<RoomVersion> Versions { get; set; } = [];

    /// <summary>
    /// How many updates a status lasts in this room, by the status's name, in place of what the hit that gave it says.
    /// A status not listed lasts as long as the hit says.
    /// </summary>
    [Export]
    public Godot.Collections.Dictionary<string, int> StatusUpdates { get; set; } = [];

    /// <summary>How many updates a status lasts in this room, when a hit says it lasts <paramref name="updates"/>.</summary>
    public int StatusUpdatesFor(string status, int updates) => StatusUpdates.TryGetValue(status, out int inRoom) ? inRoom : updates;

    private GameSession? _session;
    private Player? _player;
    private Terrain? _terrain;
    private CollisionWorld? _world;
    private Mover? _mover;

    private readonly List<Enemy> _enemies = [];
    private readonly List<Projectile> _projectiles = [];
    private readonly List<Pickup> _pickups = [];
    private readonly List<Effect> _effects = [];
    private readonly List<Remains> _remains = [];

    /// <summary>Things waiting to be added to the room: each once what it waits for has gone, and then after its count of updates.</summary>
    private readonly List<(Node2D Node, List<Node> After, int Updates)> _arriving = [];
    private readonly List<RoomObject> _objects = [];
    private readonly List<RoomExit> _exits = [];
    private readonly List<ViewFloor> _viewFloors = [];
    private Vector2I _playerInView;
    private PixelPosition _playerAtUpdateStart;
    private readonly List<Trigger> _triggers = [];
    private readonly List<RoomEffect> _roomEffects = [];
    private readonly List<RoomSequence> _sequences = [];
    private readonly List<Backdrop> _backdrops = [];
    private readonly List<IRoomEntity> _leaving = [];
    private readonly List<PatchSequence> _terrainChanges = [];

    /// <summary>
    /// The exit the player arrived at, until they have been in control and clear of it once. It takes no one until
    /// then, so that a player something brings in through it, as an elevator does, is not taken straight back.
    /// </summary>
    private RoomExit? _arrivedAt;
    private readonly HashSet<string> _gone = new(StringComparer.Ordinal);

    /// <summary>The session the room is being played in.</summary>
    public GameSession Session => _session ?? throw NotSetUp();

    /// <summary>Where the room's assets are loaded from.</summary>
    public ContentLibrary Content => _content ?? Session.Content;

    /// <summary>The game's files as this room reads them: with pictures for its own set of graphics.</summary>
    private ContentLibrary? _content;

    /// <summary>The player.</summary>
    public Player Player => _player ?? throw NotSetUp();

    /// <summary>The room's terrain.</summary>
    public Terrain Terrain => _terrain ?? throw NotSetUp();

    /// <summary>The room's collision.</summary>
    public CollisionWorld Collision => _world ?? throw NotSetUp();

    /// <summary>The room's mover.</summary>
    public Mover Mover => _mover ?? throw NotSetUp();

    /// <summary>The room's liquid, when it has one.</summary>
    public Liquid? Liquid { get; private set; }

    /// <summary>The version of the room's settings in use.</summary>
    public RoomVersion? Version { get; private set; }

    /// <summary>The exit the player has entered, once they have; the mode in control begins the transition.</summary>
    public RoomExit? ExitEntered { get; private set; }

    /// <summary>The live enemies, in the order they joined the room.</summary>
    public IReadOnlyList<Enemy> Enemies => _enemies;

    /// <summary>The live projectiles, in the order they were launched.</summary>
    public IReadOnlyList<Projectile> Projectiles => _projectiles;

    /// <summary>The live pickups, in the order they appeared.</summary>
    public IReadOnlyList<Pickup> Pickups => _pickups;

    /// <summary>The room objects, in the order they joined the room.</summary>
    public IReadOnlyList<RoomObject> Objects => _objects;

    /// <summary>The room's exits.</summary>
    public IReadOnlyList<RoomExit> Exits => _exits;

    /// <summary>How many enemies have been killed in the room since it was set up, counting each time one that comes back is killed.</summary>
    public int EnemiesKilled { get; private set; }

    /// <summary>Counts an enemy's death.</summary>
    internal void CountKilled() => EnemiesKilled++;

    /// <summary>Whether any enemy in the room is alive.</summary>
    public bool HasLiveEnemies => _enemies.Any(enemy => !enemy.IsDead && !_leaving.Contains(enemy));

    /// <summary>
    /// Sets the room up for the player's arrival: chooses its version, loads its terrain, keeps the placed things whose
    /// conditions hold, puts each in its starting state, and places the player. A room set up to be played asks for
    /// its version's music once the version is chosen, before anything of the room enters, so that music its things
    /// ask for as they enter comes after the room's own.
    /// </summary>
    public void Setup(GameSession session, Player player, RoomEntry entry, bool asksForMusic = false)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(entry);
        _session = session;
        _player = player;
        _gone.Clear();
        EnemiesKilled = 0;
        Entry = entry;
        ShownTurn = null;
        ExitEntered = null;

        _terrain = GetChildren().OfType<Terrain>().SingleOrDefault()
            ?? throw new InvalidOperationException($"The room \"{Name}\" must have exactly one Terrain.");
        // The conditions that choose a version, and that decide which placed things are present, are asked before the
        // room has any enemies: they are about what has happened, not about what is in the room.
        _world = new CollisionWorld(_terrain);
        _mover = new Mover(_world);
        Version = Versions.FirstOrDefault(version => version.When is null || version.When.IsTrue(this));
        if (Version is not null && Version.LayoutFile.Length > 0)
            _terrain.LayoutFile = Version.LayoutFile;
        _terrain.Load(Session.Content);

        // The view takes up the room's screens and how each lets it in.
        int columns = Math.Max(1, (_terrain.WidthInPixels + Camera.Screen - 1) / Camera.Screen);
        int rows = Math.Max(1, (_terrain.HeightInPixels + Camera.Screen - 1) / Camera.Screen);
        ScreenScroll[] screens = Version is { Screens.Length: > 0 }
            ? [.. Version.Screens.Select(screen => (ScreenScroll)screen)]
            : [.. Enumerable.Repeat(ScreenScroll.Open, columns * rows)];
        Session.ScrollCells.Load(columns, rows, screens, Version?.ScrollLayout ?? "", Session.Content);
        Session.Camera.EnterRoom(Session.ScrollCells, columns, rows);
        _playerInView = PlayerInView;
        // What stands in the room is drawn with the graphics the room's layout is drawn with.
        _content = Session.Content.DrawnWith(_terrain.Graphics);
        foreach (string patch in Version?.Patches ?? [])
            _terrain.ApplyPatch(patch);
        if (asksForMusic)
            Session.Sounds.EnterRoom(this);

        foreach (Node child in GetChildren())
        {
            if (child is IPlaced { PresentWhen: { } present } && !present.IsTrue(this))
            {
                RemoveChild(child);
                child.Free();
            }
        }

        if (player.GetParent() != this)
        {
            player.GetParent()?.RemoveChild(player);
            AddChild(player);
        }

        Join(player);
        // The player is put where they arrive before anything else enters, so that what enters finds them there.
        PlacePlayer(entry);
        _playerAtUpdateStart = Player.Position;

        // Whoever brings the player in keeping the view puts the view where it belongs.
        if (!entry.KeepsView)
            Session.Camera.Place(Player.Position.X.Whole - Session.Camera.Width / 2, Player.Position.Y.Whole - Session.Camera.Height / 2);
        if (ViewHeld)
            Session.Camera.MoveTo(ViewHeldAt.X, ViewHeldAt.Y);
        foreach (Node child in GetChildren())
        {
            switch (child)
            {
                case global::Engine.Actors.Player:
                case global::Engine.World.Terrain:
                    break;
                case IRoomEntity entity:
                    Join(entity);
                    break;
                case RoomExit exit:
                    _exits.Add(exit);
                    break;
                case CameraZone zone:
                    zone.Setup(this);
                    break;
                case ViewFloor floor:
                    floor.Setup();
                    _viewFloors.Add(floor);
                    break;
                case Trigger trigger:
                    trigger.Setup();
                    _triggers.Add(trigger);
                    break;
                case Liquid liquid:
                    liquid.Setup(Content);
                    Liquid = liquid;
                    break;
                case RoomEffect effect:
                    effect.Setup(this);
                    _roomEffects.Add(effect);
                    break;
                case Backdrop backdrop:
                    backdrop.Setup(Content, new Vector2I(_terrain.WidthInPixels, _terrain.HeightInPixels));
                    _backdrops.Add(backdrop);
                    break;
                case RoomSequence sequence:
                    _sequences.Add(sequence);
                    break;
            }
        }

        foreach (RoomSequence sequence in _sequences)
            sequence.Setup(this);
        ShowAll();
    }

    /// <summary>
    /// Plays one update of the room, in a fixed order: the opening, in which the liquid moves, the sequences and
    /// projectiles with a part in it play that part, and then the session services that open an update play
    /// theirs; the room's effects, all but its shaking, and the liquid's pictures; the first part of the player's
    /// update, in which their input is read and their weapons fire; the player's own shots; enemies; the rest of the
    /// player's update, in which they move; triggers and exits; every other projectile, pickups and the remains of
    /// the destroyed; room objects, the terrain, the room's shaking and its sequences; then effects. Groups named in
    /// <paramref name="stopped"/> do not run; the services that open the update always do.
    /// </summary>
    public void Step(GameInput input, Stopped stopped = Stopped.Nothing)
    {
        if (_suspended is not null)
            throw new InvalidOperationException($"The room \"{Name}\" was given a new update while one was still suspended for a message.");
        _playerAtUpdateStart = Player.Position;
        OpenUpdate(movesLiquid: !stopped.HasFlag(Stopped.Room), opensProjectiles: !stopped.HasFlag(Stopped.Projectiles));

        // The room's conditions play on before anyone acts: one that is started or told something during an update
        // first shows it in the next. The shaking is not among them: it is stepped once all that may start it has run.
        if (!stopped.HasFlag(Stopped.Room))
        {
            foreach (RoomEffect effect in _roomEffects)
            {
                if (effect is not Earthquake)
                    effect.Step(this);
            }

            Liquid?.Step(Session.Camera.View);
        }

        if (!stopped.HasFlag(Stopped.Player))
            Player.BeginUpdate(input);

        // The player's shots move, and hit, before the enemies act; a shot fired this update among them.
        if (!stopped.HasFlag(Stopped.Projectiles))
        {
            foreach (Projectile projectile in _projectiles.ToList())
            {
                if (projectile.Side == HitSide.Player && !_leaving.Contains(projectile))
                    projectile.Step();
            }
        }

        if (!stopped.HasFlag(Stopped.Enemies))
        {
            foreach (Enemy enemy in _enemies.ToList())
            {
                if (!_leaving.Contains(enemy) && IsActive(enemy))
                    enemy.Step();
            }
        }

        if (!stopped.HasFlag(Stopped.Player))
        {
            Player.FinishUpdate();
            PixelRect body = Player.BodyRect;
            foreach (Trigger trigger in _triggers.ToList())
                trigger.Step(this, body);
            if (_arrivedAt is not null && Player.State.InControl && !_arrivedAt.Area.Overlaps(body))
                _arrivedAt = null;
            ExitEntered ??= _exits.FirstOrDefault(exit => exit != _arrivedAt && exit.Area.Overlaps(body));
        }

        if (!stopped.HasFlag(Stopped.Projectiles))
        {
            foreach (Projectile projectile in _projectiles.ToList())
            {
                if (projectile.Side != HitSide.Player && !_leaving.Contains(projectile))
                    projectile.Step();
            }

            foreach (Pickup pickup in _pickups.ToList())
            {
                if (!_leaving.Contains(pickup))
                    pickup.Step();
            }

            // After the pickups, so that a drop the remains leave is first stepped in the update after it appears.
            foreach (Remains remains in _remains.ToList())
            {
                if (!_leaving.Contains(remains))
                    remains.Step();
            }
        }

        if (!stopped.HasFlag(Stopped.Room))
        {
            foreach (RoomObject roomObject in _objects.ToList())
            {
                if (!_leaving.Contains(roomObject))
                    roomObject.Step();
            }

            foreach (PatchSequence change in _terrainChanges.ToList())
                change.Step(this);
            _terrainChanges.RemoveAll(change => change.IsOver);
            Terrain.Step(rect => Collision.Overlapping(rect, ColliderRole.Body).Count > 0);
            foreach (string sound in Terrain.TakeBreakSounds())
                Session.Sounds.Request(sound, channel: "room");
        }

        // A message asked for by now is shown before the update goes on: the rest of the update waits for it.
        if (Session.HasMessageWaiting)
        {
            _suspended = stopped;
            return;
        }

        Finish(stopped);
    }

    /// <summary>What was stopped in the update that is suspended for a message; null when none is.</summary>
    private Stopped? _suspended;

    /// <summary>
    /// Whether the update last begun stopped after the room's objects because a message had been asked for: the rest
    /// of it, the shaking, the room's sequences, the effects, what leaves and arrives, and the view, has yet to be
    /// played.
    /// </summary>
    public bool UpdateIsSuspended => _suspended is not null;

    /// <summary>Plays the rest of the update that was suspended for a message.</summary>
    public void FinishSuspendedUpdate()
    {
        Stopped stopped = _suspended ?? throw new InvalidOperationException($"The room \"{Name}\" has no suspended update to finish.");
        _suspended = null;
        Finish(stopped);
    }

    /// <summary>The end of an update, after the room's objects and its terrain.</summary>
    private void Finish(Stopped stopped)
    {
        // The enemies' shots are tried against the player only now, each once where it stands: all have moved, and
        // the room's objects have played. The player says first whether any is tried at all.
        if (!stopped.HasFlag(Stopped.Projectiles) && Player.BeginEnemyShots())
        {
            foreach (Projectile projectile in _projectiles.ToList())
            {
                if (projectile.Side == HitSide.Enemies && !_leaving.Contains(projectile))
                    projectile.MeetTargetsWhereItStands();
            }

            Player.EndEnemyShots();
        }

        // Then the player's shots against the enemies' shots, each of the enemies' in turn against each of the
        // player's, whether or not any was tried against the player.
        if (!stopped.HasFlag(Stopped.Projectiles) && !Player.IsDead && Player.State is not DyingState)
        {
            var shots = _projectiles.Where(projectile => projectile.Side == HitSide.Player).ToList();
            foreach (Projectile enemyShot in _projectiles.Where(projectile => projectile.Side == HitSide.Enemies).ToList())
            {
                if (_leaving.Contains(enemyShot))
                    continue;
                foreach (Projectile shot in shots)
                {
                    if (!_leaving.Contains(shot))
                        shot.MeetShot(enemyShot);
                }
            }
        }

        if (!stopped.HasFlag(Stopped.Room))
        {
            foreach (Earthquake quake in _roomEffects.OfType<Earthquake>())
                quake.Step(this);
            foreach (RoomSequence sequence in _sequences)
                sequence.Step(this);
        }

        foreach (Effect effect in _effects.ToList())
        {
            if (!_leaving.Contains(effect))
                effect.Step();
        }

        RemoveLeaving();
        AddArriving();
        MarkExplored();
        Follow(frozen: stopped.HasFlag(Stopped.Room));
        ShowAll();
    }

    /// <summary>
    /// Opens an update of the room without playing it: the liquid moves and the sequences and projectiles with a part
    /// in the opening play that part, unless told not to, and then the session's services that open an update play
    /// theirs, unless told that the session's update is not opening. <see cref="Step"/> opens its own update; this
    /// is for a mode that plays only some of a room's update, as a passage between rooms does.
    /// </summary>
    public void OpenUpdate(bool movesLiquid = true, bool opensProjectiles = true, bool opensSession = true)
    {
        if (movesLiquid)
        {
            if (Liquid is { } liquid)
            {
                liquid.Move();
                foreach (ILiquidWatcher watcher in Session.Services.OfType<ILiquidWatcher>())
                    watcher.LiquidMoved(Session, liquid);
            }

            foreach (RoomSequence sequence in _sequences)
                sequence.OpenUpdate(this);
        }

        if (opensProjectiles)
        {
            foreach (Projectile projectile in _projectiles.ToList())
            {
                if (projectile is IOpeningStep opening && !_leaving.Contains(projectile))
                    opening.StepOpening(this);
            }
        }

        if (opensSession)
            Session.OpenUpdate();
    }

    /// <summary>
    /// Plays the parts of an update a passage between rooms asks for, with the player and the view left where they
    /// are: the terrain's own changes, the enemies, the enemies' projectiles, the room's objects, each only when
    /// asked. What left or arrived is settled and everything is shown where it stands.
    /// </summary>
    public void StepForTransition(bool terrain = false, bool enemies = false, bool enemyProjectiles = false, bool objects = false)
    {
        if (terrain)
        {
            Terrain.Step(rect => Collision.Overlapping(rect, ColliderRole.Body).Count > 0);
            foreach (string sound in Terrain.TakeBreakSounds())
                Session.Sounds.Request(sound, channel: "room");
        }

        if (enemies)
        {
            foreach (Enemy enemy in _enemies.ToList())
            {
                if (!_leaving.Contains(enemy) && IsActive(enemy))
                    enemy.Step();
            }
        }

        if (enemyProjectiles)
        {
            foreach (Projectile projectile in _projectiles.ToList())
            {
                if (projectile.Side != HitSide.Player && !_leaving.Contains(projectile))
                    projectile.Step();
            }
        }

        if (objects)
        {
            foreach (RoomObject roomObject in _objects.ToList())
            {
                if (!_leaving.Contains(roomObject))
                    roomObject.Step();
            }
        }

        RemoveLeaving();
        AddArriving();
        ShowAll();
    }

    /// <summary>Shows everything where it stands, with the room where the view has it: for whoever has moved the view or the player outside an update.</summary>
    public void ShowNow() => ShowAll();

    /// <summary>
    /// Adds something to the room later: once everything in <paramref name="after"/> has left the room, and then
    /// <paramref name="updates"/> updates on. The room owns it until then, and frees it if the room is left first.
    /// </summary>
    public void SpawnLater(Node2D node, IEnumerable<Node> after, int updates)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        if (node is not IRoomEntity)
            throw new ArgumentException($"A {node.GetType().Name} is not something that lives in a room.", nameof(node));
        _arriving.Add((node, [.. after], updates));
    }

    /// <summary>Counts down what is waiting to be added to the room, and adds what is due.</summary>
    private void AddArriving()
    {
        for (int index = _arriving.Count - 1; index >= 0; index--)
        {
            (Node2D node, List<Node> after, int updates) = _arriving[index];
            after.RemoveAll(waited => !IsInstanceValid(waited) || waited.GetParent() != this);
            if (after.Count > 0)
                continue;
            if (updates > 0)
            {
                _arriving[index] = (node, after, updates - 1);
                continue;
            }

            _arriving.RemoveAt(index);
            Spawn(node);
        }
    }

    /// <summary>Adds something to the room while it is being played. It first takes part in the phase after the one it was added in.</summary>
    public void Spawn(Node2D node)
    {
        if (node is not IRoomEntity entity)
            throw new ArgumentException($"A {node.GetType().Name} is not something that lives in a room.", nameof(node));
        AddChild(node);
        Join(entity);
    }

    /// <summary>
    /// Takes something out of the room. It is out of play at once, and is gone from the room at the end of the update.
    /// </summary>
    public void Remove(IRoomEntity entity)
    {
        if (!_leaving.Contains(entity))
            _leaving.Add(entity);
    }

    /// <summary>Whether something has been taken out of the room during this update.</summary>
    public bool IsLeaving(IRoomEntity entity) => _leaving.Contains(entity);

    /// <summary>How the room was entered this time.</summary>
    public RoomEntry? Entry { get; private set; }

    /// <summary>
    /// The turn the room is shown with, or null while it is level. The player and the room's objects are drawn where
    /// the turn puts them; where they are, and everything that acts on it, is unchanged.
    /// </summary>
    public RoomTurn? ShownTurn { get; set; }

    /// <summary>Records that the thing placed in the room under <paramref name="name"/> is gone: dead, or opened.</summary>
    public void MarkGone(string name) => _gone.Add(name);

    /// <summary>Whether the thing placed in the room under <paramref name="name"/> is gone.</summary>
    public bool IsGone(string name) => _gone.Contains(name);

    /// <summary>
    /// Lands a hit on everything that can be hit within <paramref name="area"/>: each owner of a hurtbox there that
    /// takes hits from the hit's side, once, in the order the hurtboxes were registered. Each decides what the hit
    /// does to it.
    /// </summary>
    /// <param name="hit">The hit.</param>
    /// <param name="area">Where it lands, in room pixels.</param>
    /// <param name="from">Whatever delivers the hit, which it never lands on.</param>
    /// <param name="skipping">Targets the hit passes over, such as those a piercing shot has already hurt.</param>
    /// <param name="landsOn">Which targets the hit is tried against at all; every one, when null.</param>
    public IReadOnlyList<(IHittable Target, HitResult Result)> DeliverHit(
        Hit hit, PixelRect area, ICollisionOwner? from = null, IReadOnlySet<IHittable>? skipping = null, Func<IHittable, bool>? landsOn = null)
    {
        var results = new List<(IHittable, HitResult)>();
        var seen = new HashSet<ICollisionOwner>();
        foreach (Collider hurtbox in Collision.Overlapping(area, ColliderRole.Hurtbox, except: from))
        {
            ICollisionOwner owner = hurtbox.CollisionOwner;
            if (owner is not IHittable target || !target.IsHitBy(hit.Side) || !seen.Add(owner))
                continue;
            if (skipping is not null && skipping.Contains(target))
                continue;
            if (landsOn is not null && !landsOn(target))
                continue;
            if (owner is IRoomEntity entity && _leaving.Contains(entity))
                continue;
            results.Add((target, target.TakeHit(hit, hurtbox)));
        }

        return results;
    }

    /// <summary>
    /// Tries the player's shots against one enemy, each where it stands. An enemy asks this in its own turn, before
    /// it acts: the player's shots do not hit enemies as they move.
    /// </summary>
    internal void TryPlayerShotsAgainst(Enemy enemy)
    {
        foreach (Projectile projectile in _projectiles.ToList())
        {
            if (projectile.Side == HitSide.Player && !_leaving.Contains(projectile))
                projectile.MeetEnemy(enemy);
        }
    }

    /// <summary>
    /// Plays a change to the terrain that takes some updates. The room steps it once an update until it is over,
    /// whatever becomes of whoever set it off.
    /// </summary>
    public void Play(PatchSequence change)
    {
        ArgumentNullException.ThrowIfNull(change);
        _terrainChanges.Add(change);
    }

    /// <summary>The ongoing damage the room does at <paramref name="point"/> this update, each amount with its source's name.</summary>
    public IEnumerable<(string Source, int Hundredths)> OngoingDamageAt(PixelPosition point)
    {
        if (Liquid is not null && Liquid.Type.DamageHundredths > 0 && Liquid.IsSubmerged(point))
            yield return (Liquid.Type.Name, Liquid.Type.DamageHundredths);
        foreach (RoomEffect effect in _roomEffects)
        {
            if (effect.IsActive && effect.DamageHundredths > 0)
                yield return (effect.EffectName, effect.DamageHundredths);
        }
    }

    /// <summary>How hard the room is shaking right now; zero when it is still.</summary>
    public int ShakeStrength => _roomEffects.OfType<Earthquake>().FirstOrDefault(quake => quake.IsActive)?.Strength ?? 0;

    /// <summary>Starts the room shaking. A room with no earthquake of its own is given one.</summary>
    public void Shake(int strength, int updates, ShakeWay way = ShakeWay.Across)
    {
        Earthquake? quake = _roomEffects.OfType<Earthquake>().FirstOrDefault();
        if (quake is null)
        {
            quake = new Earthquake { Name = "Earthquake", EffectName = "earthquake", StartsActive = false };
            AddChild(quake);
            quake.Setup(this);
            _roomEffects.Add(quake);
        }

        quake.Shake(strength, updates, way);
    }

    /// <summary>
    /// Takes the room down as it is left: the player is taken out of it to go on to the next room, and everything else
    /// goes with the room.
    /// </summary>
    public void Teardown()
    {
        if (_player is not null && _player.GetParent() == this)
        {
            foreach (Collider collider in EntityNode.CollidersOf(_player))
                Collision.Unregister(collider);
            RemoveChild(_player);
        }

        foreach ((Node2D node, _, _) in _arriving)
            node.Free();
        _arriving.Clear();
    }

    /// <summary>
    /// Whether an enemy takes its turn this update: one that is always active or frozen does, and otherwise one whose
    /// middle lies within the view widened as the game's definition says, and to either side by the enemy's own
    /// half-width besides. The edges count as within.
    /// </summary>
    private bool IsActive(Enemy enemy)
    {
        if (enemy.AlwaysActive || enemy.IsFrozen)
            return true;
        PixelRect view = Session.Camera.View;
        GameDefinition game = Session.Definition;
        int across = enemy.BodyHalfWidth + game.EnemyTurnMarginAcross;
        int x = enemy.Position.X.Whole, y = enemy.Position.Y.Whole;
        return x >= view.Left - across && x <= view.Left + view.Width + across &&
            y >= view.Top - game.EnemyTurnMarginAbove && y <= view.Top + view.Height + game.EnemyTurnMarginBelow;
    }

    /// <summary>Makes something part of the room: registers its colliders, lists it, and tells it it has entered.</summary>
    private void Join(IRoomEntity entity)
    {
        switch (entity)
        {
            case Enemy enemy:
                _enemies.Add(enemy);
                break;
            case Projectile projectile:
                _projectiles.Add(projectile);
                break;
            case Pickup pickup:
                _pickups.Add(pickup);
                break;
            case Effect effect:
                _effects.Add(effect);
                break;
            case Remains remains:
                _remains.Add(remains);
                break;
            case RoomObject roomObject:
                _objects.Add(roomObject);
                break;
        }

        // The thing enters first, since entering is when it sizes or makes its colliders; then they are registered.
        entity.EnterRoom(this);
        foreach (Collider collider in EntityNode.CollidersOf((Node)entity))
            Collision.Register(collider);
    }

    /// <summary>Puts the player where their way in says.</summary>
    private void PlacePlayer(RoomEntry entry)
    {
        if (entry.At is { } at)
        {
            // Put at a place of their own, the player has still come in by the exit the entry names, if it names one.
            _arrivedAt = entry.EntryPoint.Length > 0 ? GetNodeOrNull(entry.EntryPoint) as RoomExit : null;
            Player.Arrive(at, entry.Velocity, entry.Facing);
            return;
        }

        Node point = GetNodeOrNull(entry.EntryPoint)
            ?? throw new InvalidOperationException($"The room \"{Name}\" has no entry point named \"{entry.EntryPoint}\".");
        _arrivedAt = point as RoomExit;
        PixelPosition place = point switch
        {
            RoomExit exit => exit.ArrivalPoint,
            IRoomEntity entity and Node2D placed => EntityNode.StartingPlace(placed, entity.Position),
            Node2D node => EntityNode.PlaceOf(node),
            _ => throw new InvalidOperationException($"The entry point \"{entry.EntryPoint}\" has no place in the room."),
        };
        Player.Arrive(place, entry.Velocity, entry.Facing);
    }

    /// <summary>Takes everything that left during the update out of the room for good.</summary>
    private void RemoveLeaving()
    {
        foreach (IRoomEntity entity in _leaving)
        {
            var node = (Node)entity;
            foreach (Collider collider in EntityNode.CollidersOf(node))
                Collision.Unregister(collider);
            switch (entity)
            {
                case Enemy enemy:
                    _enemies.Remove(enemy);
                    break;
                case Projectile projectile:
                    _projectiles.Remove(projectile);
                    break;
                case Pickup pickup:
                    _pickups.Remove(pickup);
                    break;
                case Effect effect:
                    _effects.Remove(effect);
                    break;
                case Remains remains:
                    _remains.Remove(remains);
                    break;
                case RoomObject roomObject:
                    _objects.Remove(roomObject);
                    break;
            }

            RemoveChild(node);
            node.Free();
        }

        _leaving.Clear();
    }

    /// <summary>Records the map cell the player is in as explored.</summary>
    private void MarkExplored()
    {
        if (Area.Length == 0)
            return;
        Session.Progress.MarkExplored(
            Area, MapCell.X + Player.Position.X.Whole / MapCellSize.X, MapCell.Y + Player.Position.Y.Whole / MapCellSize.Y);
    }

    /// <summary>
    /// Where in the view the player is kept: how many pixels from its left edge while they face right, and while they
    /// face left. The room's own until something says otherwise for a while (<see cref="KeepPlayerInViewAt"/>).
    /// </summary>
    [Export]
    public Vector2I PlayerInView { get; set; } = new(128, 128);

    /// <summary>How many pixels above the player the view's top makes for while they move up.</summary>
    [Export]
    public int ViewAboveWhileRising { get; set; } = 112;

    /// <summary>How many pixels above the player the view's top makes for while they move any other way.</summary>
    [Export]
    public int ViewAboveOtherwise { get; set; } = 112;

    /// <summary>Whether the view stays where the room holds it and does not follow the player.</summary>
    [Export]
    public bool ViewHeld { get; set; }

    /// <summary>Where the view's top-left corner stands in a room that holds it.</summary>
    [Export]
    public Vector2I ViewHeldAt { get; set; }

    /// <summary>Keeps the player elsewhere in the view until the room is next set up or this is called again: what a boss that fills the view asks for.</summary>
    public void KeepPlayerInViewAt(int facingRight, int facingLeft) => _playerInView = new Vector2I(facingRight, facingLeft);

    /// <summary>Moves the view to follow the player, as far as the room's screens let it, and brings it down to any floor that holds it.</summary>
    private void Follow(bool frozen)
    {
        if (ViewHeld)
        {
            Session.Camera.MoveTo(ViewHeldAt.X, ViewHeldAt.Y);
            return;
        }

        if (Player.State is DyingState)
            return;
        var tracking = new ViewTracking(
            Player.ViewFacing == Facing.Right ? _playerInView.X : _playerInView.Y,
            Player.Velocity.Y.IsNegative, ViewAboveWhileRising, ViewAboveOtherwise);
        Session.Camera.Track(_playerAtUpdateStart, Player.Position, tracking, frozen);
        foreach (ViewFloor floor in _viewFloors)
        {
            if (floor.IsHolding)
                Session.Camera.KeepAtOrBelow(floor.LowestTop);
        }
    }

    /// <summary>Puts every node where its exact place says, with the room where the view has it.</summary>
    private void ShowAll()
    {
        var viewTopLeft = new Vector2I(Session.Camera.Left, Session.Camera.Top);

        // The room is put on screen where the view has it, thrown about by any shaking. The player is not thrown about
        // with it: they are shown where they would be were the room still.
        Vector2I shake = _roomEffects.OfType<Earthquake>().FirstOrDefault(quake => quake.IsActive)?.ViewOffset ?? Vector2I.Zero;
        Position = new Vector2(shake.X - viewTopLeft.X, shake.Y - viewTopLeft.Y);
        foreach (Backdrop backdrop in _backdrops)
            backdrop.Follow(viewTopLeft);
        EntityNode.Show(Player, (ShownTurn is { } turn ? turn.Turned(Player.Position) : Player.Position) - PixelOffset.FromWhole(shake.X, shake.Y));
        foreach (Enemy enemy in _enemies)
            enemy.ShowPosition();
        foreach (Projectile projectile in _projectiles)
            projectile.ShowPosition();
        foreach (Pickup pickup in _pickups)
            pickup.ShowPosition();
        foreach (Effect effect in _effects)
            effect.ShowPosition();
        foreach (Remains remains in _remains)
            remains.ShowPosition();
        foreach (RoomObject roomObject in _objects)
        {
            roomObject.ShowPosition();
            if (ShownTurn is { } turned)
                EntityNode.Show(roomObject, turned.Turned(roomObject.Position));
        }
    }

    /// <summary>The error for using a room before it is set up.</summary>
    private InvalidOperationException NotSetUp() => new($"The room \"{Name}\" has not been set up.");
}
