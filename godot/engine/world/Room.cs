using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
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

    /// <summary>Who takes a turn in the enemies' pass, in the order they joined: the enemies, and the objects that act with them.</summary>
    private readonly List<IRoomEntity> _enemyPass = [];
    private readonly List<Projectile> _projectiles = [];
    private readonly List<Pickup> _pickups = [];
    private readonly List<Effect> _effects = [];
    private readonly List<Remains> _remains = [];

    /// <summary>The places of what the enemies launch and leave behind, which decide whether each is made and when it takes its turn.</summary>
    private Places _places = new(0);

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

    /// <summary>
    /// Takes the player to have entered <paramref name="exit"/>, for whoever moves a player whose state keeps them
    /// from meeting exits by themselves. The first exit entered in an update stands.
    /// </summary>
    public void EnterExit(RoomExit exit)
    {
        ArgumentNullException.ThrowIfNull(exit);
        if (!_exits.Contains(exit))
            throw new InvalidOperationException($"The exit \"{exit.Name}\" is not one of the room \"{Name}\".");
        ExitEntered ??= exit;
    }

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

    /// <summary>
    /// Whether a way out of the room lies over any of <paramref name="area"/>. A way out is open to whoever leaves
    /// by it, and a wall to a shot that meets terrain by its middle or by the edge it leads with.
    /// </summary>
    public bool HasExitOver(PixelRect area) => _exits.Any(exit => exit.Area.Overlaps(area));

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
        _places = new Places(session.Definition.EnemyPlaces);
        EnemiesKilled = 0;
        Entry = entry;
        ShownTurn = null;
        ExitEntered = null;

        _terrain = GetChildren().OfType<Terrain>().SingleOrDefault()
            ?? throw new InvalidOperationException($"The room \"{Name}\" must have exactly one Terrain.");

        // The room's tiles lie behind everything standing in it, wherever the terrain comes among its things.
        _terrain.ZIndex = RoomDepth.Tiles;
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

        // The player has a depth of their own, so nothing is in front of them or behind them by when it came in.
        player.ZIndex = RoomDepth.Player;

        Join(player);

        // Said on the console, so that whoever plays can name the room when they report something.
        GD.Print($"[update {session.UpdateNumber}] room: {(SceneFilePath.Length > 0 ? SceneFilePath : Name.ToString())}, entered at {(entry.EntryPoint.Length > 0 ? entry.EntryPoint : "a place given")}");
        // The player is put where they arrive before anything else enters, so that what enters finds them there.
        PlacePlayer(entry);
        _playerAtUpdateStart = Player.Position;

        // Whoever brings the player in keeping the view puts the view where it belongs.
        // A place play begins at may say where the view is put; otherwise it is put about the player.
        void PlaceFirstView()
        {
            if (!entry.KeepsView)
            {
                if (entry.ViewAt is { } view)
                    Session.Camera.Place(view.X, view.Y);
                else if (entry.At is null && GetNodeOrNull(entry.EntryPoint) is EntryPoint { PlacesView: true } begun)
                    Session.Camera.Place(begun.ViewAt.X, begun.ViewAt.Y, keepsFractions: true);
                else
                    Session.Camera.Place(Player.Position.X.Whole - Session.Camera.Width / 2, Player.Position.Y.Whole - Session.Camera.Height / 2);
            }

            if (ViewHeld)
                Session.Camera.MoveTo(ViewHeldAt.X, ViewHeldAt.Y);
        }

        PlaceFirstView();

        // The room's own pictures are ready before anything enters, so that what enters may change their colours.
        foreach (Backdrop backdrop in GetChildren().OfType<Backdrop>())
        {
            // A backdrop lies behind everything else in its room, wherever it comes among the room's things. The
            // depth is the room's to give: a backdrop a screen shows by itself, as the title's floor, has none.
            backdrop.ZIndex = RoomDepth.Backdrop;
            backdrop.Setup(Content, new Vector2I(_terrain.WidthInPixels, _terrain.HeightInPixels));
            _backdrops.Add(backdrop);
        }

        foreach (Node child in GetChildren())
        {
            switch (child)
            {
                case global::Engine.Actors.Player:
                case global::Engine.World.Terrain:
                    break;
                case IRoomEntity entity:
                    if (HoldsPlace(entity) && !_places.TryTake(entity))
                        throw new InvalidOperationException($"The room \"{Name}\" places more of what enemies launch and leave than it has places for: \"{child.Name}\" has none.");
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
                case RoomSequence sequence:
                    _sequences.Add(sequence);
                    break;
            }
        }

        // What the way in changes of the room, it changes now that everything of the room is in it, and the first
        // view is settled again by what it left: a screen it closed or held to its top is so from the first picture.
        bool arrivalChanged = false;
        foreach (Trigger trigger in _triggers)
            arrivalChanged |= trigger.FireOnArrival(this);
        if (arrivalChanged)
            PlaceFirstView();

        foreach (RoomSequence sequence in _sequences)
            sequence.Setup(this);
        ShowAll();
    }

    /// <summary>
    /// Plays one update of the room, in a fixed order: the opening, in which the liquid moves, the sequences and
    /// projectiles with a part in it play that part, and then the session services that open an update play
    /// theirs; the first part of the player's
    /// update, in which their input is read and their weapons fire; the player's own shots; enemies; the rest of the
    /// player's update, in which they move; triggers and exits; every other projectile, pickups and the remains of
    /// the destroyed; room objects, the terrain, the liquid's pictures, the room's effects and its sequences; then
    /// effects. Groups named in
    /// <paramref name="stopped"/> do not run; the services that open the update always do.
    /// </summary>
    /// <param name="input">The update's input.</param>
    /// <param name="stopped">The groups of things that do not run.</param>
    /// <param name="play">Whoever plays the room and settles what of the update is play's; null for a room stepped by itself.</param>
    public void Step(GameInput input, Stopped stopped = Stopped.Nothing, IRoomPlay? play = null)
    {
        if (_suspended is not null)
            throw new InvalidOperationException($"The room \"{Name}\" was given a new update while one was still suspended for a message.");
        _play = play;
        EnergySettled = EmptyEnergy.NotGone;
        _playerAtUpdateStart = Player.Position;
        OpenUpdate(movesLiquid: !stopped.HasFlag(Stopped.Room), opensProjectiles: !stopped.HasFlag(Stopped.Projectiles));

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
            StepEnemyPass();

            // What has been left to be seen takes its turn as the enemies' pass ends, and stands still when they do.
            // An effect left later in the update first takes a turn on the next.
            StepEffects(holdingPlaces: false);
        }

        // The pass is counted whether or not the enemies were held still through it.
        Session.CountEnemyPass();

        if (!stopped.HasFlag(Stopped.Player))
        {
            Player.FinishUpdate();
            if (Player.State.TriggerReach(Player) is { } reach)
            {
                foreach (Trigger trigger in _triggers.ToList())
                    trigger.Step(this, reach);
            }

            MeetExits();
        }

        if (!stopped.HasFlag(Stopped.Projectiles))
            StepWhatEnemiesLaunched();

        if (!stopped.HasFlag(Stopped.Room))
        {
            StepObjects();
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

        FinishWhatWaited();
        Finish(stopped);
    }

    /// <summary>
    /// Tests the player's body against the room's exits, as each update does once the player has moved: the exit
    /// they arrived by does not take them until they have left it in control of themselves, and a state that meets
    /// no exits is taken by none. For whoever moves the player later in an update than their own movement.
    /// </summary>
    public void MeetExits()
    {
        PixelRect body = Player.BodyRect;
        if (_arrivedAt is not null && Player.State.InControl && !_arrivedAt.Area.Overlaps(body))
            _arrivedAt = null;
        if (Player.State.MeetsExits)
            ExitEntered ??= _exits.FirstOrDefault(exit => exit != _arrivedAt && exit.Area.Overlaps(body));
    }

    /// <summary>What the room's playing sequences hold still between them.</summary>
    public Stopped StoppedBySequences =>
        _sequences.Where(sequence => sequence.IsRunning).Aggregate(Stopped.Nothing, (all, sequence) => all | sequence.Stops);

    /// <summary>What was stopped in the update that is suspended for a message; null when none is.</summary>
    private Stopped? _suspended;

    /// <summary>Whoever plays the room on the update being played, if anyone does.</summary>
    private IRoomPlay? _play;

    /// <summary>What the update last played to its end settled about the player's energy running out.</summary>
    public EmptyEnergy EnergySettled { get; private set; }

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
        FinishWhatWaited();
        Finish(stopped);
    }

    /// <summary>What has asked to finish once this update's messages have been shown, in the order it asked.</summary>
    private readonly List<IFinishesAfterMessages> _finishing = [];

    /// <summary>
    /// Has something of the room finish what it began this update once every message asked for by the time the
    /// room's objects have played has been shown: as this same update goes on, before its end, whether a message was
    /// shown or none was asked for.
    /// </summary>
    public void FinishAfterMessages(IFinishesAfterMessages who)
    {
        ArgumentNullException.ThrowIfNull(who);
        _finishing.Add(who);
    }

    private void FinishWhatWaited()
    {
        IFinishesAfterMessages[] waited = [.. _finishing];
        _finishing.Clear();
        foreach (IFinishesAfterMessages one in waited)
            one.FinishAfterMessages();
    }

    /// <summary>The end of an update, after the room's objects and its terrain.</summary>
    private void Finish(Stopped stopped)
    {
        // The enemies' shots are tried against the player only now, each once where it stands: all have moved, and
        // the room's objects have played. The player says first whether any is tried at all.
        if (!stopped.HasFlag(Stopped.Projectiles) && Player.BeginEnemyShots())
        {
            foreach (Projectile projectile in EnemyShotsInTurn())
            {
                if (!_leaving.Contains(projectile))
                    projectile.MeetTargetsWhereItStands();
            }

            Player.EndEnemyShots();
        }

        // Then the player's shots against the enemies' shots, each of the enemies' in turn against each of the
        // player's, whether or not any was tried against the player.
        if (!stopped.HasFlag(Stopped.Projectiles) && !Player.IsDying)
        {
            var shots = _projectiles.Where(projectile => projectile.Side == HitSide.Player).ToList();
            foreach (Projectile enemyShot in EnemyShotsInTurn())
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

        // Then whatever a projectile has left for the end of the update, and then whatever an enemy has.
        if (!stopped.HasFlag(Stopped.Projectiles) && !Player.IsDying)
        {
            foreach (Projectile projectile in _projectiles.ToList())
            {
                if (projectile is IClosingStep closing && !_leaving.Contains(projectile))
                    closing.StepClosing(this);
            }
        }

        if (!stopped.HasFlag(Stopped.Enemies))
        {
            foreach (Enemy enemy in _enemies.ToList())
            {
                if (enemy is IClosingStep closing && !_leaving.Contains(enemy) && !enemy.IsAsleep && IsActive(enemy))
                    closing.StepClosing(this);
            }
        }

        // The view follows the player before the room's sequences play, so they see where it now is; the shaking
        // comes after them, once all that may start it has run.
        // A sequence that moves the player for the view to follow plays first.
        if (!stopped.HasFlag(Stopped.Room))
        {
            foreach (RoomSequence sequence in _sequences)
            {
                if (sequence.PlaysBeforeView)
                    sequence.Step(this);
            }
        }

        Follow(frozen: stopped.HasFlag(Stopped.Room));

        // Play's own part of the update that comes here: whatever lasts beyond the room plays its update.
        _play?.AfterView(this);
        if (!stopped.HasFlag(Stopped.Room))
        {
            foreach (RoomSequence sequence in _sequences)
            {
                if (!sequence.PlaysBeforeView)
                    sequence.Step(this);
            }
        }

        // Only now is a player with no energy left asked what follows: they die, unless the update has also taken
        // them into an exit or play says the update leads elsewhere, or are saved. Being saved forgets an exit
        // entered on this update; and the room does not shake on the update either begins.
        (bool mayDie, bool mayBeSaved) = _play?.EmptyEnergyTerms(this) ?? (true, true);
        EnergySettled = Player.SettleEmptyEnergy(mayDie && ExitEntered is null, mayBeSaved);
        bool energyGone = EnergySettled is EmptyEnergy.Saved or EmptyEnergy.Dying;
        if (EnergySettled == EmptyEnergy.Saved)
            ExitEntered = null;
        if (!stopped.HasFlag(Stopped.Room) && !energyGone)
        {
            Liquid?.Step();
            foreach (RoomEffect effect in _roomEffects)
                effect.Step(this);
        }

        // An effect that holds one of the places of what the enemies launch takes its turn here, as the update ends.
        StepEffects(holdingPlaces: true);

        RemoveLeaving();
        AddArriving();
        MarkExplored();
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
    /// asked. The effects go on with the enemies, and those that hold a place with the enemies' projectiles. What
    /// left or arrived is settled and everything is shown where it stands.
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
            StepEnemyPass();
            Session.CountEnemyPass();
        }

        if (enemyProjectiles)
            StepWhatEnemiesLaunched(shotsOnly: true);

        // What has been left to be seen plays on with the enemies; an effect that holds one of the places of what
        // the enemies launch waits and goes on with their projectiles.
        if (enemies)
            StepEffects(holdingPlaces: false);
        if (enemyProjectiles)
            StepEffects(holdingPlaces: true);

        if (objects)
            StepObjects();
        RemoveLeaving();
        AddArriving();
        ShowAll();
    }

    /// <summary>The effects take their turns, in the order they were left: those that hold a place, or the rest.</summary>
    private void StepEffects(bool holdingPlaces)
    {
        foreach (Effect effect in _effects.ToList())
        {
            if (effect.HoldsPlace == holdingPlaces && !_leaving.Contains(effect))
                effect.Step();
        }
    }

    /// <summary>
    /// The enemies' pass: each enemy that is active takes its turn, and among them, in the order all joined the
    /// room, each object that acts with the enemies.
    /// </summary>
    private void StepEnemyPass()
    {
        foreach (IRoomEntity entity in _enemyPass.ToList())
        {
            if (_leaving.Contains(entity))
                continue;
            if (entity is Enemy enemy)
            {
                // One that acts with the room's objects takes its turn with them.
                if (!enemy.ActsWithObjects && IsActive(enemy))
                    enemy.Step();
            }
            else
            {
                ((RoomObject)entity).Step();
            }
        }
    }

    /// <summary>
    /// What the enemies have launched and left takes its turns: first every projectile not the player's that holds no
    /// place, in the order launched, and then whatever holds a place, their shots, the remains of the destroyed and
    /// the pickups, in the order of the places. What takes the place of something that has just had its turn, as a
    /// pickup takes that of the remains that leave it, waits for the next update.
    /// </summary>
    /// <param name="shotsOnly">Whether only the projectiles take a turn.</param>
    private void StepWhatEnemiesLaunched(bool shotsOnly = false)
    {
        // What took a place over since the last turns were taken has waited its update.
        _takenOver.Clear();
        var turned = new HashSet<IRoomEntity>();
        _places.TakeTurns(thing =>
        {
            if (_leaving.Contains(thing) || _takenOver.Contains(thing) || !turned.Add(thing))
                return;
            switch (thing)
            {
                case Projectile projectile:
                    projectile.Step();
                    break;
                case Pickup pickup when !shotsOnly:
                    pickup.Step();
                    break;
                case Remains remains when !shotsOnly:
                    remains.Step();
                    break;
            }
        });
        _takenOver.Clear();
    }

    /// <summary>What has taken over a place during the turns being taken, and so waits for the next update's.</summary>
    private readonly HashSet<IRoomEntity> _takenOver = [];

    /// <summary>
    /// Whether something holds one of the room's places for what the enemies launch and leave: a pickup, the remains
    /// of the destroyed, a projectile that is not the player's, and an effect whose leaver said so.
    /// </summary>
    private static bool HoldsPlace(IRoomEntity entity) =>
        entity is Pickup or Remains or Projectile { Side: not HitSide.Player } or Effect { HoldsPlace: true };

    /// <summary>Whether the room has a place free for something more the enemies launch or leave.</summary>
    public bool HasFreePlace => _places.HasFree;

    /// <summary>The place something the enemies launched or left holds in the room, counted from none; null when it holds none.</summary>
    public int? PlaceOf(IRoomEntity entity) => _places.PlaceOf(entity);

    /// <summary>The enemies' projectiles in the order they are tried: that of their places.</summary>
    private List<Projectile> EnemyShotsInTurn() =>
        [.. _places.Held().OfType<Projectile>().Where(projectile => projectile.Side == HitSide.Enemies)];

    /// <summary>The room's objects take their turns, all but those that act with the enemies.</summary>
    private void StepObjects()
    {
        foreach (RoomObject roomObject in _objects.ToList())
        {
            if (!roomObject.ActsWithEnemies && !_leaving.Contains(roomObject))
                roomObject.Step();
        }

        // An enemy that acts with the room's objects takes its whole turn after them, wherever the view is.
        foreach (Enemy enemy in _enemies.ToList())
        {
            if (enemy.ActsWithObjects && !_leaving.Contains(enemy))
                enemy.Step();
        }
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

    /// <summary>
    /// Adds something to the room while it is being played. It first takes part in the phase after the one it was added
    /// in. Something that holds one of the places of what the enemies launch and leave is not added when the room has
    /// none free: it is done with, and this returns false.
    /// </summary>
    public bool Spawn(Node2D node)
    {
        if (node is not IRoomEntity entity)
            throw new ArgumentException($"A {node.GetType().Name} is not something that lives in a room.", nameof(node));
        if (HoldsPlace(entity) && !_places.TryTake(entity))
        {
            node.QueueFree();
            return false;
        }

        AddChild(node);
        Join(entity);
        return true;
    }

    /// <summary>
    /// Adds something to the room in the place of <paramref name="leaving"/>, which is taken out of the room: what is
    /// added holds the place the other held, and waits for the next update for its first turn.
    /// </summary>
    public void SpawnInPlaceOf(IRoomEntity leaving, Node2D node)
    {
        if (node is not IRoomEntity entity)
            throw new ArgumentException($"A {node.GetType().Name} is not something that lives in a room.", nameof(node));
        _places.HandOver(leaving, entity);
        _takenOver.Add(entity);
        Remove(leaving);
        AddChild(node);
        Join(entity);
    }

    /// <summary>
    /// Takes a spent drop out of the room and has the place it held go on being held, for the pause the game's
    /// definition gives (<see cref="GameDefinition.SpentDropUpdates"/>), by something that shows nothing. With no
    /// pause it is only taken out.
    /// </summary>
    public void HoldPlaceOf(IRoomEntity spent, PixelPosition at)
    {
        int updates = Session.Definition.SpentDropUpdates;
        if (updates > 0 && PlaceOf(spent) is not null)
            SpawnInPlaceOf(spent, Effect.HoldingPlace(at, updates));
        else
            Remove(spent);
    }

    /// <summary>
    /// Takes something out of the room. It is out of play at once, and is gone from the room at the end of the update.
    /// </summary>
    public void Remove(IRoomEntity entity)
    {
        if (!_leaving.Contains(entity))
            _leaving.Add(entity);

        // The place it held is free at once, for whatever is made next.
        _places.Free(entity);
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
        // The room's effects first, which take their turn before anything in the room acts, and then its liquid.
        foreach (RoomEffect effect in _roomEffects)
        {
            if (effect.IsActive && effect.DamageHundredths > 0)
                yield return (effect.EffectName, effect.DamageHundredths);
        }

        if (Liquid is not null && Liquid.Type.DamageHundredths > 0 && Liquid.IsSubmerged(point))
            yield return (Liquid.Type.Name, Liquid.Type.DamageHundredths);
    }

    /// <summary>How hard the room is shaking right now; zero when it is still.</summary>
    public int ShakeStrength => _roomEffects.OfType<Earthquake>().FirstOrDefault(quake => quake.IsShaking)?.Strength ?? 0;

    /// <summary>
    /// Whether the room has just begun to shake as <paramref name="shake"/> says: a shake of that strength, length
    /// and way was started on this update and has not yet taken an update of its own. For what answers one kind of
    /// shake, as it starts, and no other.
    /// </summary>
    public bool HasJustBegunShaking(RoomShake shake)
    {
        ArgumentNullException.ThrowIfNull(shake);
        return _roomEffects.OfType<Earthquake>().Any(quake =>
            quake.HasJustBegun && quake.Strength == shake.Strength && quake.Updates == shake.Updates && quake.Way == shake.Way);
    }

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
    /// Shows a change of colours on the room's own pictures, its terrain and its backdrops, at once, in place of
    /// whatever <paramref name="source"/> last asked for there: what something does to the room's colours in its own
    /// turn, or as the room is set up, shown on the update it is done. <see cref="ColourChange.None"/> takes back
    /// what the source asked for.
    /// </summary>
    public void Recolour(string source, ColourChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        change.ShowOn(Terrain, source);
        foreach (Backdrop backdrop in _backdrops)
            change.ShowOn(backdrop, source);
    }

    /// <summary>
    /// Adds a room-wide condition to the room while it is being played: whatever starts one that the room's scene does
    /// not place. It is the room's from then on, set up at once and stepped with the room's other effects from the
    /// next update that steps them.
    /// </summary>
    public void AddEffect(RoomEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        AddChild(effect);
        effect.Setup(this);
        _roomEffects.Add(effect);
    }

    /// <summary>
    /// Gives the room's terrain up as the room is left, to whoever goes on showing it: a door's passage keeps the
    /// screen of the room left in sight while the next scrolls in. The terrain is taken out of the room and is the
    /// taker's to place and to free. The room is done with after this: it is taken down and freed, and nothing more
    /// is asked of it.
    /// </summary>
    public Terrain ReleaseTerrain()
    {
        Terrain terrain = Terrain;
        RemoveChild(terrain);
        return terrain;
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
                _enemyPass.Add(enemy);
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
                if (roomObject.ActsWithEnemies)
                    _enemyPass.Add(roomObject);
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
                    _enemyPass.Remove(enemy);
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
                    _enemyPass.Remove(roomObject);
                    break;
            }

            RemoveChild(node);
            node.Free();
        }

        _leaving.Clear();
    }

    /// <summary>
    /// Whether the room's map is shown and explored. Something of the room may say it is not for as long as the
    /// room lasts, as a boss whose room hides the map does; the cell the player is in is then not marked explored.
    /// </summary>
    public bool MapShown { get; set; } = true;

    /// <summary>Records the map cell the player is in as explored.</summary>
    private void MarkExplored()
    {
        if (Area.Length == 0 || !MapShown)
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
        if (Player.State.ViewFollowsSlowly(Player) is { } slowly)
        {
            Session.Camera.TrackSlowly(Player.Position, slowly.Step, slowly.Middle, frozen);
        }
        else
        {
            // The view leads a player on the way up, which a rise gravity has slowed to rest or past it still is
            // until their state turns it into a fall: by their own word for it, not by the sign of their speed.
            var tracking = new ViewTracking(
                Player.ViewFacing == Facing.Right ? _playerInView.X : _playerInView.Y,
                Player.IsRising, ViewAboveWhileRising, ViewAboveOtherwise);
            Session.Camera.Track(_playerAtUpdateStart, Player.Position, tracking, frozen);
        }

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
        foreach (OverlayEffect overlay in _roomEffects.OfType<OverlayEffect>())
            overlay.Follow(Session.Camera.View);
        Liquid?.Follow(Session.Camera.View);
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
            if (ShownTurn is { } turned && turned.Turns(roomObject))
                EntityNode.Show(roomObject, turned.Turned(roomObject.Position));
        }
    }

    /// <summary>The error for using a room before it is set up.</summary>
    private InvalidOperationException NotSetUp() => new($"The room \"{Name}\" has not been set up.");
}
