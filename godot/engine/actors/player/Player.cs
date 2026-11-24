using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// The engine's player controller: what a metroidvania's player character is. It owns the player's place and speed,
/// facing and aim, what they are doing, their inventory and weapons, and their hurt and invincible state. It turns
/// input into movement and actions, moves with the room's mover, fires, takes hits, collects, and applies to itself
/// what the room reports. It knows no enemy, room object or item by type. A game's player is this, plus what is its own.
/// </summary>
/// <remarks>
/// The player's place is the middle of their body. Their body is as wide as <see cref="HalfWidth"/> to each side, and
/// as tall as the half-height of the form it is in above and below.
/// </remarks>
public partial class Player : Node2D, IRoomEntity, IHittable, IHasValues
{
    /// <summary>The name of the upright form.</summary>
    public const string Standing = "standing";

    /// <summary>The name of the crouched form.</summary>
    public const string Crouching = "crouching";

    /// <summary>The name of the low form.</summary>
    public const string Crawling = "crawling";

    /// <summary>The name of the form a jump takes, for a game whose player is not upright in the air; otherwise the upright form is used.</summary>
    public const string Jumping = "jumping";

    /// <summary>The name of the form a fall takes, for a game whose player is not upright in the air; otherwise the upright form is used.</summary>
    public const string Falling = "falling";

    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How far the body reaches to each side of its middle, in pixels.</summary>
    [Export, DataValue("half_width")]
    public int HalfWidth { get; set; } = 5;

    /// <summary>
    /// How far the body reaches above and below its middle in each form, in pixels, by the form's name. The engine's
    /// states use "standing", "crouching" and "crawling"; a game's own states may add forms.
    /// </summary>
    [Export, DataValue("forms", ReadByOwner = true)]
    public Godot.Collections.Dictionary<string, int> Forms { get; set; } = new() { [Standing] = 20, [Crouching] = 14, [Crawling] = 7 };

    /// <summary>How the player moves in each medium. One of them is named "air".</summary>
    [Export, DataValue("mediums", ReadByOwner = true)]
    public Godot.Collections.Array<PlayerMedium> Mediums { get; set; } = [];

    /// <summary>How many updates a knockback lasts.</summary>
    [Export, DataValue("knockback_updates")]
    public int KnockbackUpdates { get; set; } = 12;

    /// <summary>How many updates after being hurt the player cannot be hurt again.</summary>
    [Export, DataValue("invincible_updates")]
    public int InvincibleUpdates { get; set; } = 60;

    /// <summary>How many updates turning around on the ground takes; zero turns at once.</summary>
    [Export, DataValue("turn_updates")]
    public int TurnUpdates { get; set; }

    /// <summary>How many updates dying takes.</summary>
    [Export, DataValue("dying_updates")]
    public int DyingUpdates { get; set; } = 120;

    /// <summary>The effect left at the player's feet on landing, by the data file of its timeline; empty for none.</summary>
    [Export(PropertyHint.File), DataValue("land_effect")]
    public string LandEffect { get; set; } = "";

    /// <summary>The most pixels climbed for each pixel walked across a slope.</summary>
    [Export, DataValue("climb_per_pixel")]
    public int ClimbPerPixel { get; set; } = 2;

    /// <summary>The most pixels the player is brought down to stay on ground that falls away as they walk.</summary>
    [Export, DataValue("follow_ground_down")]
    public int FollowGroundDown { get; set; } = 3;

    /// <summary>The energy the player starts a new game able to hold, and with.</summary>
    [Export, DataValue("starting_energy")]
    public int StartingEnergy { get; set; } = 99;

    /// <summary>Whether the player can crouch.</summary>
    [Export, DataValue("can_crouch")]
    public bool CanCrouch { get; set; } = true;

    /// <summary>Whether pressing down while running crouches; when not, the player runs on.</summary>
    [Export, DataValue("crouch_while_running")]
    public bool CrouchWhileRunning { get; set; } = true;

    /// <summary>Whether the player can take the low form.</summary>
    [Export, DataValue("can_crawl")]
    public bool CanCrawl { get; set; }

    /// <summary>Whether the player can jump while in the low form.</summary>
    [Export, DataValue("can_jump_while_crawling")]
    public bool CanJumpWhileCrawling { get; set; }

    /// <summary>Whether the player can jump off a wall.</summary>
    [Export, DataValue("can_wall_jump")]
    public bool CanWallJump { get; set; }

    /// <summary>The equipment that must be switched on for the player to crouch; empty when none is needed.</summary>
    [Export, DataValue("crouch_needs")]
    public string CrouchNeeds { get; set; } = "";

    /// <summary>The equipment that must be switched on for the player to take the low form; empty when none is needed.</summary>
    [Export, DataValue("crawl_needs")]
    public string CrawlNeeds { get; set; } = "";

    /// <summary>The equipment that must be switched on for the player to jump while in the low form; empty when none is needed.</summary>
    [Export, DataValue("jump_while_crawling_needs")]
    public string JumpWhileCrawlingNeeds { get; set; } = "";

    /// <summary>The equipment that must be switched on for the player to jump off a wall; empty when none is needed.</summary>
    [Export, DataValue("wall_jump_needs")]
    public string WallJumpNeeds { get; set; } = "";

    /// <summary>How near a wall must be, in pixels, for the player to jump off it.</summary>
    [Export, DataValue("wall_jump_reach")]
    public int WallJumpReach { get; set; } = 1;

    /// <summary>How many updates after a wall jump the player cannot steer.</summary>
    [Export, DataValue("wall_jump_lock_updates")]
    public int WallJumpLockUpdates { get; set; } = 8;

    /// <summary>The speed across a wall jump starts with, away from the wall.</summary>
    [Export, DataValue("wall_jump_across_speed")]
    public double WallJumpAcrossSpeed { get; set; } = 2;

    /// <summary>Whether a wall jump can start from any jump or fall; when not, only from a state of the game's that allows it.</summary>
    [Export, DataValue("wall_jump_from_any_jump")]
    public bool WallJumpFromAnyJump { get; set; } = true;

    /// <summary>
    /// Whether the player steers a wall jump from its start, by the wall jump's move values; when not, they leave the
    /// wall at the set speed and cannot steer until the lock is over.
    /// </summary>
    [Export, DataValue("wall_jump_steered")]
    public bool WallJumpSteered { get; set; }

    /// <summary>Whether a wall jump needs the player to be steering away from the wall.</summary>
    [Export, DataValue("wall_jump_needs_steering_away")]
    public bool WallJumpNeedsSteeringAway { get; set; }

    /// <summary>The upward speed a jump is cut to when the jump action is let go early, as a number of pixels.</summary>
    [Export, DataValue("jump_cut_speed")]
    public double JumpCutSpeed { get; set; } = 1;

    /// <summary>The upward speed the low form bounces with on landing from a fast fall; zero for no bounce.</summary>
    [Export, DataValue("crawl_bounce_speed")]
    public double CrawlBounceSpeed { get; set; }

    /// <summary>The least falling speed, in whole pixels, at which the low form bounces on landing.</summary>
    [Export, DataValue("crawl_bounce_fall_speed")]
    public int CrawlBounceFallSpeed { get; set; } = 3;

    /// <summary>The equipment that, switched on, gives jumps their boosted speeds; empty when nothing does.</summary>
    [Export, DataValue("jump_boost_needs")]
    public string JumpBoostNeeds { get; set; } = "";

    /// <summary>
    /// The action that, held while running, builds a dash: speed on top of running that is kept as momentum through
    /// the air. Empty when the player cannot dash.
    /// </summary>
    [Export, DataValue("dash_action")]
    public string DashAction { get; set; } = "";

    /// <summary>The speed a dash gains each update, as a number of pixels.</summary>
    [Export, DataValue("dash_acceleration")]
    public double DashAcceleration { get; set; } = 0.0625;

    /// <summary>The most speed a dash adds.</summary>
    [Export, DataValue("dash_cap")]
    public double DashCap { get; set; } = 2;

    /// <summary>The equipment that, switched on, raises the most speed a dash adds; empty when nothing does.</summary>
    [Export, DataValue("dash_boost_needs")]
    public string DashBoostNeeds { get; set; } = "";

    /// <summary>The most speed a dash adds while that equipment is switched on.</summary>
    [Export, DataValue("boosted_dash_cap")]
    public double BoostedDashCap { get; set; } = 2;

    /// <summary>Whether the player may crouch now: the game allows it and the equipment it needs is switched on.</summary>
    public bool MayCrouch => CanCrouch && HasWhatItNeeds(CrouchNeeds);

    /// <summary>Whether the player may take the low form now.</summary>
    public bool MayCrawl => CanCrawl && HasWhatItNeeds(CrawlNeeds);

    /// <summary>Whether the player may jump while in the low form now.</summary>
    public bool MayJumpWhileCrawling => CanJumpWhileCrawling && HasWhatItNeeds(JumpWhileCrawlingNeeds);

    /// <summary>Whether the player may jump off a wall now.</summary>
    public bool MayWallJump => CanWallJump && HasWhatItNeeds(WallJumpNeeds);

    /// <summary>Whether the player's jumps start with their boosted speeds now.</summary>
    public bool JumpsAreBoosted => JumpBoostNeeds.Length > 0 && Inventory.IsOn(JumpBoostNeeds);

    /// <summary>Whether the equipment a mechanic needs is switched on; a mechanic that needs none always has it.</summary>
    private bool HasWhatItNeeds(string equipment) => equipment.Length == 0 || Inventory.IsOn(equipment);

    private Room? _room;
    private StateMachine<Player>? _states;
    private Collider? _body;
    private Collider? _hurtbox;
    private GameInput _input = GameInput.None;
    private Countdown _invincible = Countdown.Over;
    private PlayerMedium? _medium;
    private int _ongoingHundredths;
    private int _addedOngoing;
    private bool _valuesLoaded;
    private readonly Dictionary<string, IReadOnlyList<string>> _ongoingCancelledBy = new(StringComparer.Ordinal);
    private readonly List<BodyPower> _bodyPowers = [];

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>The player's speed, apart from any dash.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>The speed a dash is adding across, in the way the player is going.</summary>
    public Pixels DashSpeed { get; private set; }

    /// <summary>Whether the player has built momentum by dashing, which keeps their dash speed until it is lost.</summary>
    public bool HasMomentum { get; private set; }

    /// <summary>The way the player faces.</summary>
    public Facing Facing { get; set; } = Facing.Right;

    /// <summary>Where the player's weapon points.</summary>
    public Aim Aim { get; set; }

    /// <summary>The form the player's body is in, by name.</summary>
    public string Form { get; private set; } = Standing;

    /// <summary>How far the body reaches above and below its middle right now, in pixels.</summary>
    public int HalfHeight => HalfHeightOf(Form);

    /// <summary>What the player carries.</summary>
    public Inventory Inventory { get; } = new();

    /// <summary>The player's weapons.</summary>
    public Weapons Weapons { get; } = new();

    /// <summary>The player's body powers.</summary>
    public IReadOnlyList<BodyPower> BodyPowers => _bodyPowers;

    /// <summary>The room the player is in.</summary>
    public Room Room => _room ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>The input of the update being played.</summary>
    public GameInput Input => _input;

    /// <summary>What the player is doing.</summary>
    public PlayerState State => (PlayerState)States.Current;

    /// <summary>Whether the player has finished dying.</summary>
    public bool IsDead { get; private set; }

    /// <summary>Whether the player cannot be hurt just now, having just been hurt.</summary>
    public bool IsInvincible => _invincible.IsRunning;

    /// <summary>The updates left in which the player cannot be hurt.</summary>
    public int InvincibleUpdatesLeft => _invincible.Remaining;

    /// <summary>The collider the player moves through terrain with.</summary>
    public Collider Body => _body ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>The collider the player is hit on.</summary>
    public Collider Hurtbox => _hurtbox ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>The pixels the player's body covers, in the room.</summary>
    public PixelRect BodyRect => Body.RoomRect;

    /// <summary>The middle of the player's feet: the lowest row of their body.</summary>
    public PixelPosition Feet => new(Position.X, Pixels.FromWhole(Position.Y.Whole + HalfHeight - 1));

    /// <summary>The middle of the top of the player's body.</summary>
    public PixelPosition Head => new(Position.X, Pixels.FromWhole(Position.Y.Whole - HalfHeight));

    /// <summary>Which way the player is steering: -1 left, 1 right, 0 neither or both.</summary>
    public int Intent => (_input.IsHeld(GameAction.Right) ? 1 : 0) - (_input.IsHeld(GameAction.Left) ? 1 : 0);

    /// <summary>Whether the player stands on terrain or a solid.</summary>
    public bool IsOnGround => Room.Mover.IsOnGround(Body, MoveOptions.Plain) || StandsInBlockThatHoldsUp();

    /// <summary>Whether the player's feet are in a block that pulls down and yet holds up what stands in it, as quicksand does.</summary>
    private bool StandsInBlockThatHoldsUp() =>
        Room.Terrain.BlocksIn(FeetRect).Any(block => block.Type.Contact == BlockContact.PullsDown && block.Type.HoldsUp);

    /// <summary>The bottom row of the player's body.</summary>
    private PixelRect FeetRect => new(BodyRect.Left, BodyRect.Bottom - 1, BodyRect.Width, 1);

    /// <summary>
    /// The medium the player is moving in this update: the liquid their feet are in, when it hinders them and they
    /// have a medium of its name, and otherwise air.
    /// </summary>
    public PlayerMedium Medium => _medium ??= FindMedium();

    /// <summary>Whether the player's feet are in a liquid that hinders them.</summary>
    public bool IsHinderedByLiquid => HinderingLiquid(Feet) is not null;

    /// <summary>Whether the top of the player's body is in a liquid that hinders them.</summary>
    public bool IsWhollyInLiquid => HinderingLiquid(Head) is not null;

    /// <summary>The move values of the state named <paramref name="state"/>, in the medium the player is in.</summary>
    public MoveValues Moves(string state) => Medium.MovesFor(state);

    /// <summary>Which way a shot leaves: one step across and one step down, each -1, 0 or 1.</summary>
    public (int Across, int Down) AimDirection
    {
        get
        {
            int forward = Facing == Facing.Right ? 1 : -1;
            return Aim switch
            {
                Aim.Forward => (forward, 0),
                Aim.DiagonalUp => (forward, -1),
                Aim.Up => (0, -1),
                Aim.DiagonalDown => (forward, 1),
                Aim.Down => (0, 1),
                _ => throw new InvalidOperationException($"Undefined aim {Aim}."),
            };
        }
    }

    /// <summary>Where a shot starts: at the edge of the body it leaves from, a third of the way down it.</summary>
    public virtual PixelPosition MuzzlePoint
    {
        get
        {
            (int across, int down) = AimDirection;
            int y = down < 0 ? -HalfHeight : down > 0 ? HalfHeight : -(HalfHeight / 3);
            return Position + PixelOffset.FromWhole(across * HalfWidth, y);
        }
    }

    /// <summary>
    /// Adds to the ongoing damage the player takes this update, from something that is touching them. It is taken with
    /// the room's own, unless the player's equipment cancels its source.
    /// </summary>
    public void AddOngoingDamage(string source, int hundredths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentOutOfRangeException.ThrowIfNegative(hundredths);
        if (!IsCancelled(source))
            _addedOngoing += hundredths;
    }

    /// <summary>Gives the player a body power, not yet active.</summary>
    public void AddBodyPower(BodyPower power)
    {
        ArgumentNullException.ThrowIfNull(power);
        _bodyPowers.Add(power);
    }

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        _medium = null;
        if (!_valuesLoaded)
        {
            _valuesLoaded = true;
            DataValues.Fill(this, room.Content);
            if (Mediums.Count == 0)
                Mediums.Add(PlayerMedium.Plain());
            if (Mediums.All(medium => medium.MediumName != PlayerMedium.Air))
                throw new InvalidOperationException("The player has no medium named \"air\".");
            if (Inventory.Capacity(Inventory.Energy) == 0)
            {
                Inventory.SetCapacity(Inventory.Energy, StartingEnergy);
                Inventory.Fill(Inventory.Energy);
            }

            _body = EntityNode.ColliderOf(this, ColliderRole.Body) ?? AddCollider("Body", ColliderRole.Body);
            _hurtbox = EntityNode.ColliderOf(this, ColliderRole.Hurtbox) ?? AddCollider("Hurtbox", ColliderRole.Hurtbox);
            ApplyForm(Standing);
            _states = new StateMachine<Player>(this, new StandingState());
            OnReady();
        }

        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Bind(room.Content);
        OnEnteredRoom(room);
    }

    /// <inheritdoc/>
    public virtual void ReadValues(DataNode data)
    {
        _ongoingCancelledBy.Clear();
        if (data.Has("ongoing_damage_cancelled_by"))
        {
            foreach ((string source, DataNode equipment) in data.Get("ongoing_damage_cancelled_by").Entries())
                _ongoingCancelledBy[source] = equipment.Items().Select(item => item.AsText()).ToList();
        }

        Forms = [];
        foreach ((string form, DataNode halfHeight) in data.Get("forms").Entries())
            Forms[form] = halfHeight.AsInt();
        Mediums = [];
        foreach ((string name, DataNode medium) in data.Get("mediums").Entries())
            Mediums.Add(PlayerMedium.Read(name, medium));
    }

    /// <summary>Called once, the first time the player enters a room, after their values are loaded.</summary>
    protected virtual void OnReady()
    {
    }

    /// <summary>Called each time the player enters a room, the first time after <see cref="OnReady"/>.</summary>
    protected virtual void OnEnteredRoom(Room room)
    {
    }

    /// <summary>Puts the player where they arrive in a room, with the speed and facing they arrive with.</summary>
    public void Arrive(PixelPosition place, PixelVelocity velocity, Facing facing)
    {
        Position = place;
        Velocity = velocity;
        Facing = facing;

        // Arriving is neither a landing nor a crossing of a surface.
        (_wasOnGround, _feetWereInLiquid) = (true, null);
    }

    /// <summary>
    /// Plays one update: what the player is doing runs, weapons fire, active body powers hit what the body touches, and
    /// the room's ongoing damage is taken.
    /// </summary>
    public void Step(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _input = input;
        _medium = null;
        _invincible = _invincible.Stepped();
        if (IsDead)
            return;

        BeforeState();
        PlayerState? chosen = ChooseState(State);
        if (chosen is not null)
            States.ChangeTo(chosen);
        States.Step();

        if (State.CanAttack)
            Attack(input);
        DeliverBodyPowers();
        if (State is not DyingState)
        {
            bool onGround = IsOnGround;
            if (onGround)
                Room.Terrain.StandOn(BodyRect);
            if (onGround && !_wasOnGround)
                OnLanded();
            _wasOnGround = onGround;
            BeMovedByBlocks();
            Splash();
            TakeOngoingDamage();
        }

        AfterState();
    }

    /// <summary>Whether the player's feet were on the ground, and in the room's liquid, at the end of the last update.</summary>
    private bool _wasOnGround = true;
    private bool? _feetWereInLiquid;

    /// <summary>
    /// Called as the player comes down onto the ground. The engine's player leaves its landing effect at its feet, if
    /// its scene names one.
    /// </summary>
    protected virtual void OnLanded()
    {
        if (LandEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(LandEffect)), Feet));
    }

    /// <summary>Leaves the liquid's splash, and asks for its sound, as the player's feet cross its surface either way.</summary>
    private void Splash()
    {
        Liquid? liquid = Room.Liquid;
        bool inLiquid = liquid is not null && liquid.IsSubmerged(Feet);
        bool crossed = _feetWereInLiquid is bool before && before != inLiquid;
        _feetWereInLiquid = inLiquid;
        if (!crossed || liquid is null)
            return;
        if (liquid.Type.SplashEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(liquid.Type.SplashEffect)), new PixelPosition(Position.X, liquid.Surface)));
        string sound = inLiquid ? liquid.Type.SplashSound : liquid.Type.SplashOutSound;
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "liquid");
    }

    /// <summary>Called at the start of each update, before the player's state runs.</summary>
    protected virtual void BeforeState()
    {
    }

    /// <summary>Called at the end of each update, after the player's state, weapons, body powers and ongoing damage.</summary>
    protected virtual void AfterState()
    {
    }

    /// <summary>
    /// Lets a game put the player into a state of its own. Called at the start of each update with the state the player
    /// is in; returns the state to change to, or null to leave things as they are.
    /// </summary>
    protected virtual PlayerState? ChooseState(PlayerState current) => null;

    /// <summary>Uses the player's weapons for one update. A game whose player attacks in ways of its own does so here.</summary>
    protected virtual void Attack(GameInput input) => Weapons.Step(this, input);

    /// <summary>Puts the player into <paramref name="state"/> now.</summary>
    public void ChangeState(PlayerState state) => States.ChangeTo(state);

    /// <summary>Knocks the player back, away from <paramref name="from"/>.</summary>
    public void KnockBack(Facing from) => ChangeState(new KnockedBackState(from.Opposite()));

    /// <summary>Knocks the player back, away from <paramref name="from"/>, for <paramref name="updates"/> updates.</summary>
    public void KnockBack(Facing from, int updates) => ChangeState(new KnockedBackState(from.Opposite(), updates));

    /// <summary>Takes hold of the player: they stay where they are put until released.</summary>
    public void Hold() => ChangeState(new HeldState());

    /// <summary>Carries the player: they go where they are taken until released.</summary>
    public void Carry() => ChangeState(new CarriedState());

    /// <summary>Pushes the player at <paramref name="velocity"/> for <paramref name="updates"/> updates.</summary>
    public void Push(PixelVelocity velocity, int updates) => ChangeState(new PushedState(velocity, updates));

    /// <summary>Holds the player at a station until released.</summary>
    /// <param name="facingOut">Whether the player turns to face out of the screen while held.</param>
    public void UseStation(bool facingOut = false) => ChangeState(new UsingStationState { FacingOut = facingOut });

    /// <summary>Takes control from the player for a scripted sequence, until released.</summary>
    public ScriptedState Script()
    {
        var scripted = new ScriptedState();
        ChangeState(scripted);
        return scripted;
    }

    /// <summary>Gives the player back control after being held, carried, pushed, at a station or scripted.</summary>
    public void Release() => ChangeState(StateAtRest());

    /// <summary>
    /// The state the player is in when nothing else has hold of them: on the ground or in the air, upright or in the
    /// form they are already in.
    /// </summary>
    public virtual PlayerState StateAtRest()
    {
        if (Form == Crawling)
            return new CrawlingState();
        return IsOnGround ? new StandingState() : new FallingState();
    }

    /// <summary>Starts the player dying.</summary>
    public void Die()
    {
        if (State is not DyingState)
            ChangeState(new DyingState());
    }

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => side != HitSide.Player;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        if (IsDead || IsInvincible || State is DyingState || !AcceptsHit(hit))
            return HitResult.Ignored;
        if (hit.Carries(HitKind.Contact) && hit.Source is IHittable toucher && BodyPowerStops(toucher))
            return HitResult.Ignored;
        Inventory.Lose(Inventory.Energy, DamageFrom(hit));
        if (Inventory.Amount(Inventory.Energy) == 0 && !SurvivesEmptyEnergy())
        {
            Die();
            return HitResult.Destroyed;
        }

        _invincible = Countdown.Of(InvincibleUpdates);
        Facing from = hit.Direction.X.IsZero ? Facing : hit.Direction.X.IsNegative ? Facing.Right : Facing.Left;
        OnHurt(hit, from);
        return HitResult.Hurt;
    }

    /// <summary>
    /// Takes a hit's energy from the player and nothing more: they are not knocked back and not made invincible. For
    /// something that feeds on the player while it clings to them.
    /// </summary>
    public void Sap(Hit hit)
    {
        if (IsDead || State is DyingState)
            return;
        Inventory.Lose(Inventory.Energy, DamageFrom(hit));
        if (Inventory.Amount(Inventory.Energy) == 0 && !SurvivesEmptyEnergy())
            Die();
    }

    /// <summary>The energy <paramref name="hit"/> would take from the player as they are equipped now.</summary>
    public int EnergyLostTo(Hit hit) => DamageFrom(hit);

    /// <summary>Whether a hit can land on the player at the moment. A game with states in which the player cannot be hit says so here.</summary>
    protected virtual bool AcceptsHit(Hit hit) => true;

    /// <summary>The energy a hit takes from the player. A game whose equipment lessens hits says so here.</summary>
    protected virtual int DamageFrom(Hit hit) => hit.Damage;

    /// <summary>
    /// Called when the player's energy has run out, before they die. A game that has something to fall back on, such
    /// as a reserve, uses it here and returns true.
    /// </summary>
    protected virtual bool SurvivesEmptyEnergy() => false;

    /// <summary>Called when a hit has hurt the player without killing them. The engine's player is knocked back if in control.</summary>
    /// <param name="hit">The hit.</param>
    /// <param name="from">The side the hit came from.</param>
    protected virtual void OnHurt(Hit hit, Facing from)
    {
        if (State.InControl)
            KnockBack(from);
    }

    /// <summary>Makes the player unable to be hurt for <paramref name="updates"/> updates, as after a hit.</summary>
    public void BeInvincibleFor(int updates) => _invincible = Countdown.Of(updates);

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);

    /// <summary>The form named <paramref name="form"/> when the player has one of that name, and otherwise the upright form.</summary>
    public string FormOr(string form) => Forms.ContainsKey(form) ? form : Standing;

    /// <summary>
    /// Changes the body's form, if the new form fits. On the ground the feet stay where they are; in the air, or when
    /// <paramref name="aroundMiddle"/> is asked for, the middle of the body does, or failing that the feet, or failing
    /// that the head. Returns whether the form changed.
    /// </summary>
    public bool TrySetForm(string form, bool aroundMiddle = false)
    {
        if (form == Form)
            return true;
        int change = HalfHeightOf(form) - HalfHeight;
        PixelPosition keepingFeet = Position + PixelOffset.FromWhole(0, -change);
        PixelPosition keepingHead = Position + PixelOffset.FromWhole(0, change);
        PixelPosition[] candidates = IsOnGround && !aroundMiddle ? [keepingFeet] : [Position, keepingFeet, keepingHead];
        foreach (PixelPosition candidate in candidates)
        {
            if (change > 0 && !Room.Mover.IsFree(BodyRectFor(form).At(candidate), this, MoveOptions.Plain))
                continue;
            Position = candidate;
            ApplyForm(form);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Changes the player's speed across by steering with <paramref name="moves"/>: toward the way they steer, up to
    /// the top speed; against the way they are going, slowing; and with no steering, as the values say.
    /// </summary>
    public void Steer(MoveValues moves)
    {
        ArgumentNullException.ThrowIfNull(moves);
        int intent = Intent;
        Pixels speed = Velocity.X;
        if (intent != 0 && (speed.IsZero || speed.IsNegative == intent < 0))
        {
            Pixels faster = speed + moves.Gain * intent;
            speed = faster.Abs() > moves.Top ? moves.Top * intent : faster;
            Facing = intent > 0 ? Facing.Right : Facing.Left;
        }
        else if (intent != 0)
        {
            speed = speed.Toward(Pixels.Zero, moves.Loss);
        }
        else
        {
            speed = moves.WhenNotSteering switch
            {
                MoveValues.NotSteering.Slows => speed.Toward(Pixels.Zero, moves.Loss),
                MoveValues.NotSteering.Stops => Pixels.Zero,
                MoveValues.NotSteering.Coasts => speed,
                _ => throw new InvalidOperationException($"Undefined answer {moves.WhenNotSteering} to not steering."),
            };
        }

        Velocity = Velocity.WithX(speed);
    }

    /// <summary>Slows the player's speed across toward rest by <paramref name="moves"/>' deceleration.</summary>
    public void Slow(MoveValues moves) => Velocity = Velocity.WithX(Velocity.X.Toward(Pixels.Zero, moves.Loss));

    /// <summary>
    /// Builds or keeps the player's dash for one update. While <paramref name="building"/>, which a state says when
    /// the player is running freely with the dash action held, the dash gains speed up to its cap and becomes
    /// momentum. Otherwise a dash that became momentum is kept, and one that did not is lost.
    /// </summary>
    public void StepDash(bool building)
    {
        if (!building || DashAction.Length == 0)
        {
            if (!HasMomentum)
                DashSpeed = Pixels.Zero;
            return;
        }

        if (!HasMomentum)
        {
            HasMomentum = true;
            OnMomentumBegan();
        }

        Pixels cap = Pixels.FromNumber(DashBoostNeeds.Length > 0 && Inventory.IsOn(DashBoostNeeds) ? BoostedDashCap : DashCap);
        DashSpeed = DashSpeed >= cap ? cap : DashSpeed + Pixels.FromNumber(DashAcceleration);
    }

    /// <summary>Whether the dash action is held.</summary>
    public bool DashIsHeld => DashAction.Length > 0 && _input.IsHeld(new GameAction(DashAction));

    /// <summary>Ends the player's momentum and their dash with it.</summary>
    public void LoseMomentum()
    {
        DashSpeed = Pixels.Zero;
        if (!HasMomentum)
            return;
        HasMomentum = false;
        OnMomentumLost();
    }

    /// <summary>Called as a dash becomes momentum.</summary>
    protected virtual void OnMomentumBegan()
    {
    }

    /// <summary>Called as the player's momentum is lost.</summary>
    protected virtual void OnMomentumLost()
    {
    }

    /// <summary>Stops the player's movement across, dash and momentum included.</summary>
    public void StopAcross()
    {
        Velocity = Velocity.WithX(Pixels.Zero);
        LoseMomentum();
    }

    /// <summary>How far the player moves across this update: their speed, and their dash on top of it in the way they are going.</summary>
    public Pixels SpeedAcross
    {
        get
        {
            if (DashSpeed.IsZero)
                return Velocity.X;
            bool leftward = Velocity.X.IsZero ? Facing == Facing.Left : Velocity.X.IsNegative;
            return Velocity.X + (leftward ? -DashSpeed : DashSpeed);
        }
    }

    /// <summary>Adds the medium's gravity to the player's speed, up to the fastest they fall.</summary>
    public void Fall() => Velocity = Velocity.WithY(AfterGravity(Velocity.Y, Medium));

    /// <summary>The player's speed downward after one update of <paramref name="medium"/>'s gravity.</summary>
    protected virtual Pixels AfterGravity(Pixels speed, PlayerMedium medium)
    {
        Pixels limit = Pixels.FromNumber(medium.FallSpeedLimit);
        Pixels faster = speed + Pixels.FromNumber(medium.Gravity);
        return faster > limit ? limit : faster;
    }

    /// <summary>The upward speed a jump from the ground, or off a wall, starts with now.</summary>
    public virtual Pixels LaunchSpeed(bool offWall)
    {
        PlayerMedium medium = Medium;
        double speed = offWall
            ? (JumpsAreBoosted ? medium.BoostedWallJumpSpeed : medium.WallJumpSpeed)
            : (JumpsAreBoosted ? medium.BoostedJumpSpeed : medium.JumpSpeed);
        return Pixels.FromNumber(speed);
    }

    /// <summary>
    /// Moves the player across by their speed and dash. On the ground they climb slopes and follow ground that falls
    /// away. Meeting a wall stops them and ends their momentum.
    /// </summary>
    public MoveResult MoveAcross()
    {
        bool grounded = IsOnGround;
        MoveOptions options = MoveOptions.Plain with
        {
            ClimbPerPixel = grounded ? ClimbPerPixel : 1,
            FollowGroundDown = grounded ? FollowGroundDown : 0,
        };
        MoveResult result = Room.Mover.MoveAcross(Body, Slowed(InLiquid(SpeedAcross)), options);
        if (result.Blocked)
            StopAcross();
        return result;
    }

    /// <summary>Moves the player down by their speed, upward when it is negative.</summary>
    public MoveResult MoveDown()
    {
        MoveResult result = Room.Mover.MoveDown(Body, InLiquid(Velocity.Y), MoveOptions.Plain);
        if (result.Blocked)
            Velocity = Velocity.WithY(Pixels.Zero);
        return result;
    }

    /// <summary>Whether the player is against a wall on the given side.</summary>
    public bool IsAgainstWall(Facing side) => IsWithinReachOfWall(side, 1);

    /// <summary>Whether a wall lies within <paramref name="reach"/> pixels of the player's body on the given side.</summary>
    public bool IsWithinReachOfWall(Facing side, int reach)
    {
        PixelRect body = BodyRect;
        var beside = side == Facing.Left
            ? new PixelRect(body.Left - reach, body.Top, reach, body.Height)
            : new PixelRect(body.Right, body.Top, reach, body.Height);
        return !Room.Mover.IsFree(beside, this, MoveOptions.Plain with { Slopes = SlopeTreatment.Wall });
    }

    /// <summary>Sets the player's aim from what is held: up, down while in the air, and each with steering for a diagonal.</summary>
    public virtual void AimFromInput()
    {
        bool up = _input.IsHeld(GameAction.Up), down = _input.IsHeld(GameAction.Down) && !IsOnGround;
        Aim = up ? (Intent != 0 ? Aim.DiagonalUp : Aim.Up) : down ? (Intent != 0 ? Aim.DiagonalDown : Aim.Down) : Aim.Forward;
    }

    /// <summary>The player has finished dying.</summary>
    internal void FinishDying() => IsDead = true;

    /// <summary>The state machine, once the player has entered a room.</summary>
    private StateMachine<Player> States => _states ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>How far the body reaches above and below its middle in a form.</summary>
    public int HalfHeightOf(string form) =>
        Forms.TryGetValue(form, out int halfHeight) ? halfHeight : throw new InvalidOperationException($"The player has no form named \"{form}\".");

    /// <summary>The body's rectangle in a form, from the player's place at its middle.</summary>
    private PixelRect BodyRectFor(string form) => new(-HalfWidth, -HalfHeightOf(form), HalfWidth * 2, HalfHeightOf(form) * 2);

    /// <summary>Puts the body and the hurtbox in a form's shape.</summary>
    private void ApplyForm(string form)
    {
        Form = form;
        Body.Rect = BodyRectFor(form);
        Hurtbox.Rect = BodyRectFor(form);
    }

    /// <summary>Adds a collider the player's scene did not supply.</summary>
    private Collider AddCollider(string name, ColliderRole role)
    {
        var collider = new Collider { Name = name, Role = role };
        AddChild(collider);
        return collider;
    }

    /// <summary>The room's liquid, when <paramref name="point"/> is under it and the player's equipment does not let them move in it as in air.</summary>
    private Liquid? HinderingLiquid(PixelPosition point)
    {
        Liquid? liquid = Room.Liquid;
        return liquid is not null && liquid.IsSubmerged(point) && !Inventory.IsAnyOn(liquid.Type.SlowingCancelledBy) ? liquid : null;
    }

    /// <summary>Works out the medium the player is in.</summary>
    private PlayerMedium FindMedium()
    {
        Liquid? liquid = HinderingLiquid(Feet);
        PlayerMedium? inLiquid = liquid is null ? null : Mediums.FirstOrDefault(medium => medium.MediumName == liquid.Type.Name);
        return inLiquid ?? Mediums.First(medium => medium.MediumName == PlayerMedium.Air);
    }

    /// <summary>
    /// How many times over the player's speed across is halved by things clinging to them. Whatever clings raises it
    /// and lowers it again; it is never taken above <see cref="MostSpeedHalvings"/>.
    /// </summary>
    public int SpeedHalvings { get; set; }

    /// <summary>The most times the player's speed across can be halved.</summary>
    [Export]
    public int MostSpeedHalvings { get; set; } = 4;

    /// <summary>A distance across moved this update, halved as many times as the player is slowed.</summary>
    private Pixels Slowed(Pixels distance)
    {
        int halvings = Math.Clamp(SpeedHalvings, 0, MostSpeedHalvings);
        return halvings == 0 ? distance : distance.Share(1, 1 << halvings);
    }

    /// <summary>
    /// A distance moved this update, slowed by the liquid the player is in when they have no medium of its name: such
    /// a player moves by air's values and the liquid's own share of them.
    /// </summary>
    private Pixels InLiquid(Pixels distance)
    {
        Liquid? liquid = HinderingLiquid(Feet);
        if (liquid is null || Medium.MediumName == liquid.Type.Name)
            return distance;
        return distance.IsNegative
            ? -(-distance).Share(liquid.Type.SpeedTimes, liquid.Type.SpeedOver)
            : distance.Share(liquid.Type.SpeedTimes, liquid.Type.SpeedOver);
    }

    /// <summary>
    /// Whether an active body power saves the player from the touch of <paramref name="toucher"/>: the power's hit is
    /// delivered to it there and then, and the touch does nothing when that hit hurt or destroyed it.
    /// </summary>
    private bool BodyPowerStops(IHittable toucher)
    {
        if (toucher is not Node node || !toucher.IsHitBy(HitSide.Player))
            return false;
        Collider? hurtbox = EntityNode.CollidersOf(node).FirstOrDefault(collider => collider.Enabled && collider.Role == ColliderRole.Hurtbox);
        if (hurtbox is null)
            return false;
        foreach (BodyPower power in _bodyPowers)
        {
            if (power.IsActive && toucher.TakeHit(power.Hit with { Source = this }, hurtbox) is HitResult.Hurt or HitResult.Destroyed)
                return true;
        }

        return false;
    }

    /// <summary>Delivers each active body power's hit to whatever the body touches.</summary>
    private void DeliverBodyPowers()
    {
        foreach (BodyPower power in _bodyPowers)
        {
            if (!power.IsActive)
                continue;
            PixelRect body = BodyRect;
            var reach = new PixelRect(body.Left - 1, body.Top - 1, body.Width + 2, body.Height + 2);
            Hit hit = power.Hit with { Source = this };
            Room.DeliverHit(hit, reach, from: this);
            Room.Terrain.TakeHit(hit, reach);
        }
    }

    /// <summary>
    /// Lets the blocks the player is in move them, over and above their own movement: one that carries takes them
    /// across, and one that pulls takes them down. Of several, the one that moves them furthest does.
    /// </summary>
    private void BeMovedByBlocks()
    {
        Pixels across = Pixels.Zero, down = Pixels.Zero;
        foreach (Block block in Room.Terrain.BlocksIn(BodyRect))
        {
            Pixels speed = block.Type.ContactSpeed;
            if (block.Type.Contact == BlockContact.Carries)
            {
                if (block.Type.CarriesOnlyLevel && Velocity.Y.Whole != 0)
                    continue;
                if ((speed.IsNegative ? -speed : speed) > (across.IsNegative ? -across : across))
                    across = speed;
            }
            else if (block.Type.Contact == BlockContact.PullsDown)
            {
                BlockType sand = block.Type;
                if (sand.StopsMomentum && HasMomentum)
                    LoseMomentum();
                if (sand.PullsFeetOnly && !block.Rect.Overlaps(FeetRect))
                    continue;
                bool eased = sand.EasedBy.Count > 0 && Inventory.IsAnyOn(sand.EasedBy);
                Pixels limit = eased ? sand.Eased.RiseLimit : sand.RiseLimit;
                bool moving = Velocity.Y != Pixels.Zero;
                if (limit > Pixels.Zero && Velocity.Y < -limit)
                    Velocity = Velocity.WithY(-limit);
                if (sand.HoldsUp && Velocity.Y > Pixels.Zero)
                    Velocity = Velocity.WithY(Pixels.Zero);
                Pixels pull = moving
                    ? (eased ? sand.Eased.MovingContactSpeed : sand.MovingContactSpeed)
                    : (eased ? sand.Eased.ContactSpeed : speed);
                if (pull > down)
                    down = pull;
            }
        }

        if (across != Pixels.Zero)
            Room.Mover.MoveAcross(Body, across, MoveOptions.Plain);
        if (down != Pixels.Zero)
            Room.Mover.MoveDown(Body, down, MoveOptions.Plain);
    }

    /// <summary>
    /// Takes this update's ongoing damage: what the room does where the player is, and what blocks they touch do, less
    /// whatever their equipment cancels. Parts of a point carry over to later updates.
    /// </summary>
    private void TakeOngoingDamage()
    {
        int hundredths = _addedOngoing;
        _addedOngoing = 0;
        foreach ((string source, int amount) in Room.OngoingDamageAt(Position))
        {
            if (Room.Liquid is { } liquid && source == liquid.Type.Name && Inventory.IsAnyOn(liquid.Type.DamageCancelledBy))
                continue;
            if (!IsCancelled(source))
                hundredths += amount;
        }

        int blockHit = 0;
        foreach (Block block in Room.Terrain.BlocksIn(BodyRect, touchingCounts: true))
        {
            if (block.Type.Contact != BlockContact.Hurts || IsCancelled("blocks"))
                continue;
            hundredths += block.Type.ContactDamageHundredths;
            blockHit = Math.Max(blockHit, block.Type.ContactHit);
        }

        // A block that hits, as a spike does, lands the hardest hit of those touched.
        if (blockHit > 0)
            TakeHit(new Hit(blockHit, HitSide.Neutral, HitKind.Hazard), Hurtbox);

        hundredths = OngoingDamageTaken(hundredths);
        if (hundredths == 0)
            return;
        _ongoingHundredths += hundredths;
        int points = _ongoingHundredths / 100;
        _ongoingHundredths %= 100;
        if (points == 0)
            return;
        Inventory.Lose(Inventory.Energy, points);
        if (Inventory.Amount(Inventory.Energy) == 0 && !SurvivesEmptyEnergy())
            Die();
    }

    /// <summary>
    /// The ongoing damage the player takes this update, in hundredths of a point, out of what reached them. A game
    /// whose equipment lessens it says so here.
    /// </summary>
    protected virtual int OngoingDamageTaken(int hundredths) => hundredths;

    /// <summary>Whether switched-on equipment cancels ongoing damage from a source.</summary>
    private bool IsCancelled(string source) =>
        _ongoingCancelledBy.TryGetValue(source, out IReadOnlyList<string>? equipment) && Inventory.IsAnyOn(equipment);

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);
}
