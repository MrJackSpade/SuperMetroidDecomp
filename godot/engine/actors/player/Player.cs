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
public partial class Player : Node2D, IRoomEntity, IHittable, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How far the body reaches to each side of its middle, in pixels.</summary>
    [Export, DataValue("half_width")]
    public int HalfWidth { get; set; } = 5;

    /// <summary>The body's height when upright, in pixels.</summary>
    [Export, DataValue("standing_height")]
    public int StandingHeight { get; set; } = 40;

    /// <summary>The body's height when crouched, in pixels.</summary>
    [Export, DataValue("crouching_height")]
    public int CrouchingHeight { get; set; } = 28;

    /// <summary>The body's height in the low form, in pixels.</summary>
    [Export, DataValue("crawling_height")]
    public int CrawlingHeight { get; set; } = 14;

    /// <summary>The speed gained each update while running, as a number of pixels.</summary>
    [Export, DataValue("run_acceleration")]
    public double RunAcceleration { get; set; } = 0.25;

    /// <summary>The top running speed.</summary>
    [Export, DataValue("run_speed")]
    public double RunSpeed { get; set; } = 2;

    /// <summary>The speed lost each update when not steering.</summary>
    [Export, DataValue("friction")]
    public double Friction { get; set; } = 0.5;

    /// <summary>The speed gained each update while steering in the air.</summary>
    [Export, DataValue("air_acceleration")]
    public double AirAcceleration { get; set; } = 0.25;

    /// <summary>The top speed in the low form.</summary>
    [Export, DataValue("crawl_speed")]
    public double CrawlSpeed { get; set; } = 1.5;

    /// <summary>The upward speed a jump starts with.</summary>
    [Export, DataValue("jump_speed")]
    public double JumpSpeed { get; set; } = 4.5;

    /// <summary>The upward speed a jump is cut to when the jump action is let go early.</summary>
    [Export, DataValue("jump_cut_speed")]
    public double JumpCutSpeed { get; set; } = 1;

    /// <summary>The upward speed a jump in the low form starts with.</summary>
    [Export, DataValue("crawl_jump_speed")]
    public double CrawlJumpSpeed { get; set; } = 3.5;

    /// <summary>The downward speed gained each update while in the air.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.125;

    /// <summary>The fastest the player falls.</summary>
    [Export, DataValue("fall_speed_limit")]
    public double FallSpeedLimit { get; set; } = 5;

    /// <summary>The speed away from the wall a wall jump starts with.</summary>
    [Export, DataValue("wall_jump_across_speed")]
    public double WallJumpAcrossSpeed { get; set; } = 2;

    /// <summary>The upward speed a wall jump starts with.</summary>
    [Export, DataValue("wall_jump_up_speed")]
    public double WallJumpUpSpeed { get; set; } = 4;

    /// <summary>How many updates after a wall jump the player cannot steer.</summary>
    [Export, DataValue("wall_jump_lock_updates")]
    public int WallJumpLockUpdates { get; set; } = 8;

    /// <summary>The speed across a hit knocks the player back with.</summary>
    [Export, DataValue("knockback_across_speed")]
    public double KnockbackAcrossSpeed { get; set; } = 1.5;

    /// <summary>The upward speed a hit knocks the player back with.</summary>
    [Export, DataValue("knockback_up_speed")]
    public double KnockbackUpSpeed { get; set; } = 2;

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

    /// <summary>Whether the player can take the low form.</summary>
    [Export, DataValue("can_crawl")]
    public bool CanCrawl { get; set; }

    /// <summary>Whether the player can jump while in the low form.</summary>
    [Export, DataValue("can_jump_while_crawling")]
    public bool CanJumpWhileCrawling { get; set; }

    /// <summary>Whether the player can jump off a wall.</summary>
    [Export, DataValue("can_wall_jump")]
    public bool CanWallJump { get; set; }

    private Room? _room;
    private StateMachine<Player>? _states;
    private Collider? _body;
    private Collider? _hurtbox;
    private GameInput _input = GameInput.None;
    private Countdown _invincible = Countdown.Over;
    private int _ongoingHundredths;
    private int _addedOngoing;
    private bool _valuesLoaded;
    private readonly Dictionary<string, IReadOnlyList<string>> _ongoingCancelledBy = new(StringComparer.Ordinal);
    private readonly List<BodyPower> _bodyPowers = [];

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>The player's speed.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>The way the player faces.</summary>
    public Facing Facing { get; set; } = Facing.Right;

    /// <summary>Where the player's weapon points.</summary>
    public Aim Aim { get; set; }

    /// <summary>The shape the player's body is in.</summary>
    public BodyForm Form { get; private set; }

    /// <summary>What the player carries.</summary>
    public Inventory Inventory { get; } = new();

    /// <summary>The player's weapons.</summary>
    public Weapons Weapons { get; } = new();

    /// <summary>The player's body powers.</summary>
    public IReadOnlyList<BodyPower> BodyPowers => _bodyPowers;

    /// <summary>Hits the player takes do their damage this many times…</summary>
    public int DamageTakenTimes { get; set; } = 1;

    /// <summary>…over this many.</summary>
    public int DamageTakenOver { get; set; } = 1;

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

    /// <summary>The collider the player moves through terrain with.</summary>
    public Collider Body => _body ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>The collider the player is hit on.</summary>
    public Collider Hurtbox => _hurtbox ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>The pixels the player's body covers, in the room.</summary>
    public PixelRect BodyRect => Body.RoomRect;

    /// <summary>Which way the player is steering: -1 left, 1 right, 0 neither or both.</summary>
    public int Intent => (_input.IsHeld(GameAction.Right) ? 1 : 0) - (_input.IsHeld(GameAction.Left) ? 1 : 0);

    /// <summary>Whether the player stands on terrain or a solid.</summary>
    public bool IsOnGround => Room.Mover.IsOnGround(Body, MoveOptions.Plain);

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

    /// <summary>Where a shot starts: at the edge of the body it leaves from, two thirds of the way up it.</summary>
    public virtual PixelPosition MuzzlePoint
    {
        get
        {
            (int across, int down) = AimDirection;
            int height = HeightOf(Form);
            int y = down < 0 ? -height : down > 0 ? 0 : -(height * 2 / 3);
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
        if (!_valuesLoaded)
        {
            _valuesLoaded = true;
            DataValues.Fill(this, room.Content);
            if (Inventory.Capacity(Inventory.Energy) == 0)
            {
                Inventory.SetCapacity(Inventory.Energy, StartingEnergy);
                Inventory.Fill(Inventory.Energy);
            }

            _body = EntityNode.ColliderOf(this, ColliderRole.Body) ?? AddCollider("Body", ColliderRole.Body);
            _hurtbox = EntityNode.ColliderOf(this, ColliderRole.Hurtbox) ?? AddCollider("Hurtbox", ColliderRole.Hurtbox);
            ApplyForm(BodyForm.Standing);
            _states = new StateMachine<Player>(this, new StandingState());
            OnReady();
        }

        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Bind(room.Content);
    }

    /// <inheritdoc/>
    public virtual void ReadValues(DataNode data)
    {
        _ongoingCancelledBy.Clear();
        if (!data.Has("ongoing_damage_cancelled_by"))
            return;
        foreach ((string source, DataNode equipment) in data.Get("ongoing_damage_cancelled_by").Entries())
            _ongoingCancelledBy[source] = equipment.Items().Select(item => item.AsText()).ToList();
    }

    /// <summary>Called once, the first time the player enters a room, after their values are loaded.</summary>
    protected virtual void OnReady()
    {
    }

    /// <summary>Puts the player where they arrive in a room, with the speed and facing they arrive with.</summary>
    public void Arrive(PixelPosition place, PixelVelocity velocity, Facing facing)
    {
        Position = place;
        Velocity = velocity;
        Facing = facing;
    }

    /// <summary>
    /// Plays one update: what the player is doing runs, weapons fire, active body powers hit what the body touches, and
    /// the room's ongoing damage is taken.
    /// </summary>
    public void Step(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _input = input;
        _invincible = _invincible.Stepped();
        if (IsDead)
            return;

        PlayerState? chosen = ChooseState(State);
        if (chosen is not null)
            States.ChangeTo(chosen);
        States.Step();

        if (State.CanAttack)
            Weapons.Step(this, input);
        DeliverBodyPowers();
        if (State is not DyingState)
        {
            if (IsOnGround)
                Room.Terrain.StandOn(BodyRect);
            TakeOngoingDamage();
        }
    }

    /// <summary>
    /// Lets a game put the player into a state of its own. Called at the start of each update with the state the player
    /// is in; returns the state to change to, or null to leave things as they are.
    /// </summary>
    protected virtual PlayerState? ChooseState(PlayerState current) => null;

    /// <summary>Puts the player into <paramref name="state"/> now.</summary>
    public void ChangeState(PlayerState state) => States.ChangeTo(state);

    /// <summary>Knocks the player back, away from <paramref name="from"/>.</summary>
    public void KnockBack(Facing from) => ChangeState(new KnockedBackState(from.Opposite()));

    /// <summary>Takes hold of the player: they stay where they are put until released.</summary>
    public void Hold() => ChangeState(new HeldState());

    /// <summary>Carries the player: they go where they are taken until released.</summary>
    public void Carry() => ChangeState(new CarriedState());

    /// <summary>Pushes the player at <paramref name="velocity"/> for <paramref name="updates"/> updates.</summary>
    public void Push(PixelVelocity velocity, int updates) => ChangeState(new PushedState(velocity, updates));

    /// <summary>Holds the player at a station until released.</summary>
    public void UseStation() => ChangeState(new UsingStationState());

    /// <summary>Takes control from the player for a scripted sequence, until released.</summary>
    public ScriptedState Script()
    {
        var scripted = new ScriptedState();
        ChangeState(scripted);
        return scripted;
    }

    /// <summary>Gives the player back control after being held, carried, pushed, at a station or scripted.</summary>
    public void Release() => ChangeState(IsOnGround ? new StandingState() : new FallingState());

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
        int damage = hit.Damage * DamageTakenTimes / DamageTakenOver;
        Inventory.Lose(Inventory.Energy, damage);
        if (Inventory.Amount(Inventory.Energy) == 0)
        {
            Die();
            return HitResult.Destroyed;
        }

        _invincible = Countdown.Of(InvincibleUpdates);
        Facing from = hit.Direction.X.IsZero ? Facing : hit.Direction.X.IsNegative ? Facing.Right : Facing.Left;
        if (State.InControl)
            KnockBack(from);
        return HitResult.Hurt;
    }

    /// <summary>Whether a hit can land on the player at the moment. A game with states in which the player cannot be hit says so here.</summary>
    protected virtual bool AcceptsHit(Hit hit) => true;

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);

    /// <summary>Changes the body's shape, if the new shape fits where the player is. Returns whether it did.</summary>
    public bool TrySetForm(BodyForm form)
    {
        if (form == Form)
            return true;
        if (HeightOf(form) > HeightOf(Form) && !Room.Mover.IsFree(BodyRectFor(form).At(Position), this, MoveOptions.Plain))
            return false;
        ApplyForm(form);
        return true;
    }

    /// <summary>Moves the player's speed across toward steering at <paramref name="top"/> speed, or toward rest when not steering.</summary>
    public void Steer(double acceleration, double top)
    {
        Pixels target = Pixels.FromNumber(top) * Intent;
        Pixels step = Pixels.FromNumber(Intent == 0 ? Friction : acceleration);
        Velocity = Velocity.WithX(Velocity.X.Toward(target, step));
        if (Intent != 0)
            Facing = Intent > 0 ? Facing.Right : Facing.Left;
    }

    /// <summary>Slows the player's speed across toward rest.</summary>
    public void SlowToRest() => Velocity = Velocity.WithX(Velocity.X.Toward(Pixels.Zero, Pixels.FromNumber(Friction)));

    /// <summary>Adds gravity to the player's speed, up to the fastest they fall.</summary>
    public void Fall()
    {
        Pixels limit = Pixels.FromNumber(FallSpeedLimit);
        Pixels down = Velocity.Y + Pixels.FromNumber(Gravity);
        Velocity = Velocity.WithY(down > limit ? limit : down);
    }

    /// <summary>Moves the player across by their speed. On the ground they climb slopes and follow ground that falls away.</summary>
    public MoveResult MoveAcross()
    {
        bool grounded = IsOnGround;
        MoveOptions options = MoveOptions.Plain with
        {
            ClimbPerPixel = grounded ? ClimbPerPixel : 1,
            FollowGroundDown = grounded ? FollowGroundDown : 0,
        };
        MoveResult result = Room.Mover.MoveAcross(Body, InLiquid(Velocity.X), options);
        if (result.Blocked)
            Velocity = Velocity.WithX(Pixels.Zero);
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
    public bool IsAgainstWall(Facing side) => Room.Mover.IsAgainstWall(Body, side, MoveOptions.Plain with { Slopes = SlopeTreatment.Wall });

    /// <summary>Sets the player's aim from what is held: up, down while in the air, and each with steering for a diagonal.</summary>
    public void AimFromInput()
    {
        bool up = _input.IsHeld(GameAction.Up), down = _input.IsHeld(GameAction.Down) && !IsOnGround;
        Aim = up ? (Intent != 0 ? Aim.DiagonalUp : Aim.Up) : down ? (Intent != 0 ? Aim.DiagonalDown : Aim.Down) : Aim.Forward;
    }

    /// <summary>The player has finished dying.</summary>
    internal void FinishDying() => IsDead = true;

    /// <summary>The state machine, once the player has entered a room.</summary>
    private StateMachine<Player> States => _states ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>How tall the body is in a form.</summary>
    private int HeightOf(BodyForm form) => form switch
    {
        BodyForm.Standing => StandingHeight,
        BodyForm.Crouching => CrouchingHeight,
        BodyForm.Crawling => CrawlingHeight,
        _ => throw new InvalidOperationException($"Undefined body form {form}."),
    };

    /// <summary>The body's rectangle in a form, from the player's origin at the middle of their feet.</summary>
    private PixelRect BodyRectFor(BodyForm form) => new(-HalfWidth, -HeightOf(form), HalfWidth * 2, HeightOf(form));

    /// <summary>Puts the body and the hurtbox in a form's shape.</summary>
    private void ApplyForm(BodyForm form)
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

    /// <summary>A distance moved this update, as slowed by the liquid the player is in, if any.</summary>
    private Pixels InLiquid(Pixels distance)
    {
        Liquid? liquid = Room.Liquid;
        if (liquid is null || !liquid.IsSubmerged(Position) || Inventory.IsAnyOn(liquid.Type.SlowingCancelledBy))
            return distance;
        return distance.Share(liquid.Type.SpeedTimes, liquid.Type.SpeedOver);
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
    /// Takes this update's ongoing damage: what the room does where the player is, and what blocks they touch do, less
    /// whatever their equipment cancels. Parts of a point carry over to later updates.
    /// </summary>
    private void TakeOngoingDamage()
    {
        int hundredths = _addedOngoing;
        _addedOngoing = 0;
        var middle = new PixelPosition(Position.X, Position.Y - Pixels.FromWhole(HeightOf(Form) / 2));
        foreach ((string source, int amount) in Room.OngoingDamageAt(middle))
        {
            if (Room.Liquid is { } liquid && source == liquid.Type.Name && Inventory.IsAnyOn(liquid.Type.DamageCancelledBy))
                continue;
            if (!IsCancelled(source))
                hundredths += amount;
        }

        foreach (Block block in Room.Terrain.BlocksIn(BodyRect, touchingCounts: true))
        {
            if (block.Type.Contact == BlockContact.Hurts && !IsCancelled("blocks"))
                hundredths += block.Type.ContactDamageHundredths;
        }

        if (hundredths == 0)
            return;
        _ongoingHundredths += hundredths;
        int points = _ongoingHundredths / 100;
        _ongoingHundredths %= 100;
        if (points == 0)
            return;
        Inventory.Lose(Inventory.Energy, points);
        if (Inventory.Amount(Inventory.Energy) == 0)
            Die();
    }

    /// <summary>Whether switched-on equipment cancels ongoing damage from a source.</summary>
    private bool IsCancelled(string source) =>
        _ongoingCancelledBy.TryGetValue(source, out IReadOnlyList<string>? equipment) && Inventory.IsAnyOn(equipment);

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);
}
