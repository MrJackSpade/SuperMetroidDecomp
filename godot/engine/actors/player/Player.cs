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
public abstract partial class Player : Node2D, IRoomEntity, IHittable, IHasValues
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

    /// <summary>
    /// The name of the form the body takes while the player aims straight down in the air, for a game whose player is
    /// smaller then; otherwise the form of the jump or the fall is kept.
    /// </summary>
    public const string AimingDown = "aiming_down";

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

    /// <summary>
    /// The most whole pixels one move of the player's own across goes, either way: its own speed with whatever is
    /// given it from outside, a carry or a push, counted together. Only the whole pixels are held to it; the move's
    /// part of a pixel is kept.
    /// </summary>
    [Export, DataValue("most_whole_across")]
    public int MostWholeAcross { get; set; } = 15;

    /// <summary>How many updates after being hurt the player cannot be hurt again.</summary>
    [Export, DataValue("invincible_updates")]
    public int InvincibleUpdates { get; set; } = 60;

    /// <summary>How many updates each picture of turning around on the ground is held, in order: the turn lasts as long as they do. None turns at once.</summary>
    [Export, DataValue("turn_pictures")]
    public int[] TurnPictures { get; set; } = [];

    /// <summary>The effect left at the player's feet on landing, by the data file of its timeline; empty for none.</summary>
    [Export(PropertyHint.File), DataValue("land_effect")]
    public string LandEffect { get; set; } = "";

    /// <summary>The most pixels climbed for each pixel moved across a slope.</summary>
    [Export, DataValue("climb_per_pixel")]
    public int ClimbPerPixel { get; set; } = 2;

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

    /// <summary>The equipment that must be switched on for the player to take the low form; empty when none is needed.</summary>
    [Export, DataValue("crawl_needs")]
    public string CrawlNeeds { get; set; } = "";

    /// <summary>The equipment that must be switched on for the player to jump while in the low form; empty when none is needed.</summary>
    [Export, DataValue("jump_while_crawling_needs")]
    public string JumpWhileCrawlingNeeds { get; set; } = "";

    /// <summary>How near a wall must be, in pixels, for the player to jump off it: the game's player finds the wall.</summary>
    [Export, DataValue("wall_jump_reach")]
    public int WallJumpReach { get; set; } = 1;

    /// <summary>How many updates each picture of crouching down is held, in order; none crouches at once.</summary>
    [Export, DataValue("crouch_pictures")]
    public int[] CrouchPictures { get; set; } = [];

    /// <summary>How many updates each picture of standing up from a crouch is held, in order; none stands at once.</summary>
    [Export, DataValue("stand_pictures")]
    public int[] StandPictures { get; set; } = [];

    /// <summary>How many updates each picture of taking the low form is held, in order; none takes it at once.</summary>
    [Export, DataValue("crawl_enter_pictures")]
    public int[] CrawlEnterPictures { get; set; } = [];

    /// <summary>How many updates each picture of leaving the low form is held, in order; none leaves it at once.</summary>
    [Export, DataValue("crawl_leave_pictures")]
    public int[] CrawlLeavePictures { get; set; } = [];

    /// <summary>
    /// How many updates each picture of the player's settling after landing on their feet is held, in order. Settled,
    /// a jump already held may once more count as pressed afresh, where the game gives the player that.
    /// </summary>
    [Export, DataValue("landing_pictures")]
    public int[] LandingPictures { get; set; } = [];

    /// <summary>
    /// The pictures of the player's settling after landing on their feet out of <paramref name="from"/>, by how many
    /// updates each is held: those of their landings. A game whose landings differ by what they end says so here.
    /// </summary>
    public virtual IReadOnlyList<int> SettlingPicturesAfter(PlayerState from) => LandingPictures;

    /// <summary>
    /// How many updates each picture of a jump's gathering is held, in order: a jump from the ground gathers itself
    /// that long, moving nothing, before it rises.
    /// </summary>
    [Export, DataValue("jump_start_pictures")]
    public int[] JumpStartPictures { get; set; } = [];

    /// <summary>
    /// A jump still held as the player lands or finishes a turn counts as pressed afresh on the next update when it
    /// has been held for fewer updates than this; zero for no such thing.
    /// </summary>
    [Export, DataValue("jump_buffer_updates")]
    public int JumpBufferUpdates { get; set; }

    /// <summary>The upward speed a jump is cut to when the jump action is let go early, as a number of pixels.</summary>
    [Export, DataValue("jump_cut_speed")]
    public double JumpCutSpeed { get; set; } = 1;

    /// <summary>The upward speed the low form bounces with on landing from a fast fall; zero for no bounce.</summary>
    [Export, DataValue("crawl_bounce_speed")]
    public double CrawlBounceSpeed { get; set; }

    /// <summary>The upward speed of the low form's second bounce, which follows its first.</summary>
    [Export, DataValue("crawl_second_bounce_speed")]
    public double CrawlSecondBounceSpeed { get; set; }

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

    /// <summary>Whether the player may take the low form now.</summary>
    public bool MayCrawl => CanCrawl && HasWhatItNeeds(CrawlNeeds);

    /// <summary>Whether the player may jump while in the low form now.</summary>
    public bool MayJumpWhileCrawling => CanJumpWhileCrawling && HasWhatItNeeds(JumpWhileCrawlingNeeds);

    /// <summary>Whether the player's jumps start with their boosted speeds now.</summary>
    public bool JumpsAreBoosted => JumpBoostNeeds.Length > 0 && Inventory.IsOn(JumpBoostNeeds);

    /// <summary>The state that crouches the player down: the crouch, by way of the change into it where that takes time.</summary>
    public PlayerState Crouch() => Changing(Crouching, new CrouchingState(), PictureCount.Begin(this, CrouchPictures), fallsInAir: false, attacks: true);

    /// <summary>The state that stands the player up from a crouch.</summary>
    public PlayerState StandUp() => Changing(Standing, new StandingState(), PictureCount.Begin(this, StandPictures), fallsInAir: false, attacks: true);

    /// <summary>The state that puts the player into the low form.</summary>
    public PlayerState TakeLowForm() => Changing(Crawling, new CrawlingState(), PictureCount.Begin(this, CrawlEnterPictures), fallsInAir: true, attacks: false);

    /// <summary>The state that brings the player out of the low form, into a crouch.</summary>
    public PlayerState LeaveLowForm() => Changing(Crouching, new CrouchingState(), PictureCount.Begin(this, CrawlLeavePictures), fallsInAir: true, attacks: false);

    /// <summary>
    /// How many updates longer each picture of the player's timed changes is held where they are now: as many as the
    /// medium they are in holds them (<see cref="PlayerMedium.PictureHold"/>), and in a medium that holds them no
    /// longer, as out of any liquid, as many as whatever clings to the player halves their speed
    /// (<see cref="SpeedHalvings"/>). A change asks as each of its pictures begins (<see cref="PictureCount"/>), and
    /// says whether the picture begins the change: a game whose rule differs between the two says so here.
    /// </summary>
    /// <param name="asChangeBegins">Whether the picture asked for is the first of a change the player now begins.</param>
    public virtual int PictureHold(bool asChangeBegins) => Medium.PictureHold > 0 ? Medium.PictureHold : SpeedHalvings;

    /// <summary>A posture's state, by way of the change into it where that takes time.</summary>
    private static PlayerState Changing(string form, PlayerState then, PictureCount count, bool fallsInAir, bool attacks) =>
        count.IsRunning ? new ChangingPostureState(form, then, count, fallsInAir, attacks) : then;

    /// <summary>Whether the equipment a mechanic needs is switched on; a mechanic that needs none always has it.</summary>
    private bool HasWhatItNeeds(string equipment) => equipment.Length == 0 || Inventory.IsOn(equipment);

    private Room? _room;
    private StateMachine<Player>? _states;
    private Collider? _body;
    private Collider? _hurtbox;
    private GameInput _input = GameInput.None;
    private Countdown _invincible = Countdown.Over;
    private PlayerMedium? _medium;
    private int _addedOngoing;
    private bool _valuesLoaded;
    private ISpeedReadings? _readings;
    private IBodyReadings? _bodyReadings;

    /// <summary>How the game reads the player's speed, once they have entered a room.</summary>
    private ISpeedReadings Readings => _readings ?? throw new InvalidOperationException("The player is not in a room.");
    private readonly Dictionary<string, IReadOnlyList<string>> _ongoingCancelledBy = new(StringComparer.Ordinal);
    private readonly List<BodyPower> _bodyPowers = [];

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>
    /// The player's speed, apart from any dash. Across, a state that steers keeps it as a size carried the way the
    /// player travels (<see cref="Steer"/>); a state that throws the player about sets it outright.
    /// </summary>
    public PixelVelocity Velocity
    {
        get => _velocity;
        set
        {
            // An upward speed is a rise. A speed left as it was keeps what it was: a rise that gravity has brought to
            // rest, or past it, is still a rise until the state the player is in turns it into a fall.
            IsRising = value.Y.IsNegative || (value.Y == _velocity.Y && IsRising);
            IsAdrift = IsAdrift && value.Y == _velocity.Y;
            _velocity = value;
        }
    }

    private PixelVelocity _velocity;

    /// <summary>
    /// Whether the player is on the way up. Gravity slowing a rise to rest, or past it, does not end it: the state
    /// the player is in does, at the top (<see cref="TurnAtTop"/>), and so does anything that stops them.
    /// </summary>
    public bool IsRising { get; private set; }

    /// <summary>
    /// Whether the player is adrift: left in the air neither rising nor falling, by something that put them there
    /// without starting a fall. Gravity draws an adrift player down as it wears a rise away, with no fastest fall,
    /// and there is no top for a state to turn into a fall: they are adrift until they land, or until something gives
    /// them a rise, a fall or a speed up or down of its own.
    /// </summary>
    public bool IsAdrift { get; private set; }

    /// <summary>Leaves the player adrift, at rest up and down.</summary>
    public void Drift()
    {
        _velocity = _velocity.WithY(Pixels.Zero);
        (IsRising, IsAdrift) = (false, true);
    }

    /// <summary>Whether gravity has slowed the player's rise past rest, so that it is due to become a fall.</summary>
    public bool IsPastTop => IsRising && _velocity.Y > Pixels.Zero;

    /// <summary>Starts the player rising at <paramref name="speed"/>.</summary>
    public void Rise(Pixels speed)
    {
        _velocity = _velocity.WithY(-speed);
        (IsRising, IsAdrift) = (true, false);
    }

    /// <summary>Puts the player at rest up and down, about to fall.</summary>
    public void FallFromRest()
    {
        _velocity = _velocity.WithY(Pixels.Zero);
        (IsRising, IsAdrift) = (false, false);
    }

    /// <summary>
    /// The speed the player came down onto their feet at on this update, as gravity left it; none when they did not,
    /// or came down in some other way.
    /// </summary>
    public Pixels LandingSpeed { get; private set; }

    /// <summary>The size of the speed the player came down at this update, the way they were held to be going (<see cref="HeldSpeedUpOrDown"/>).</summary>
    private Pixels _landingHeld;

    /// <summary>
    /// The size of the player's speed up or down the way they are held to be going: a fall's is its speed downward; a
    /// rise's, and that of a player adrift, its speed upward, which is less than nothing once gravity has worn it
    /// past rest and they come down.
    /// </summary>
    public Pixels HeldSpeedUpOrDown => IsRising || IsAdrift ? -_velocity.Y : _velocity.Y;

    /// <summary>
    /// Whether the player's speed up or down reads as at least <paramref name="whole"/> whole pixels an update, as
    /// the game reads speeds (<see cref="ISpeedReadings.ReachesWhole"/>). What asks how fast they rise or fall asks this.
    /// </summary>
    public bool MovesUpOrDownAtLeast(int whole) => Readings.ReachesWhole(HeldSpeedUpOrDown, whole);

    /// <summary>Whether the speed the player came down at this update reads as at least <paramref name="whole"/> whole pixels an update, read the same way.</summary>
    public bool LandedAtLeast(int whole) => Readings.ReachesWhole(_landingHeld, whole);

    /// <summary>
    /// Lands the player on their feet at <paramref name="speed"/>: they are at rest up and down and across, steered,
    /// and a jump already held may count as pressed afresh on the next update. A dash is left as it is.
    /// </summary>
    public void Land(Pixels speed)
    {
        LandingSpeed = speed;
        _landingHeld = IsRising || IsAdrift ? -speed : speed;
        _cameDown = true;
        FallFromRest();
        ComeToRest();
        ArmJumpBuffer();
    }

    /// <summary>Turns a rise that gravity has slowed past rest into a fall from rest. Returns whether it did.</summary>
    public bool TurnAtTop()
    {
        if (!IsPastTop)
            return false;
        FallFromRest();
        return true;
    }

    /// <summary>How the player's own speed across stands to the way they face.</summary>
    public Steering Steering { get; set; }

    /// <summary>The way a steered speed carries the player: the way they face, or against it while they are reversing.</summary>
    public Facing Travel => Steering == Steering.Reversing ? Facing.Opposite() : Facing;

    /// <summary>
    /// The way the view leads: the way the player faces, unless they are being carried the other way, by a hit's
    /// throw for as long as it has time to run (whatever state they are thrown in, and not on the update after it,
    /// when a knockback sets them down) or by the speed a turn has not yet spent. A game with other ways of going
    /// backward says so here.
    /// </summary>
    public virtual Facing ViewFacing => Thrown is not null || Steering == Steering.Reversing ? Facing.Opposite() : Facing;

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

    /// <summary>
    /// The input of the update before this one on which the player was to be seen in play: an update of their room's,
    /// whether or not they were played in it, or one of whatever shows them between rooms and says so
    /// (<see cref="SeeInput"/>). What takes a press made an update too soon reads it here.
    /// </summary>
    public GameInput InputBefore { get; private set; } = GameInput.None;

    /// <summary>
    /// Takes <paramref name="input"/> as the input of an update the player was to be seen in play on, once whatever
    /// of that update reads the one before has read it. The room does this on each of its updates; a mode that shows
    /// the player without playing them does on each of its own.
    /// </summary>
    public void SeeInput(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        InputBefore = input;
    }

    /// <summary>What the player is doing.</summary>
    public PlayerState State => (PlayerState)States.Current;

    /// <summary>Whether the player is dying: their energy is gone, nothing saved them, and the game plays their death.</summary>
    public bool IsDying => State is DyingState;

    /// <summary>Whether the player cannot be hurt just now, having just been hurt.</summary>
    public bool IsInvincible => _invincible.IsRunning;

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

    /// <summary>
    /// Which way the player is steering: -1 left, 1 right, 0 neither. Left and right held together are no direction of
    /// their own: they are read as the state the player is in reads them (<see cref="PlayerState.ReadsBothAcross"/>),
    /// one way, the other, or neither. The way behind the player held by itself steers them that way unless the state
    /// they are in passes over it (<see cref="PlayerState.PassesOverBehind"/>).
    /// </summary>
    public int Intent
    {
        get
        {
            bool right = _input.IsHeld(GameAction.Right), left = _input.IsHeld(GameAction.Left);
            if (right == left)
                return right ? State.ReadsBothAcross(this) : 0;
            bool behind = right != (Facing == Facing.Right);
            return behind && State.PassesOverBehind(this) ? 0 : right ? 1 : -1;
        }
    }

    /// <summary>
    /// How the player's body meets terrain: stopped by terrain and solids, held up by sloped ground under its middle
    /// column where the slope's tile says it holds things so, and climbing such ground as steeply as the player can,
    /// on the ground and in the air alike. Every move and test of the player's body uses it. The game's definition may
    /// say more of how its player's body is moved (<see cref="Core.GameDefinition.BodyMoves"/>).
    /// </summary>
    public MoveOptions BodyMoves =>
        Room.Session.Definition.BodyMoves(
            Room.Session, MoveOptions.Plain with { Slopes = SlopeTreatment.ClimbUnderMiddle, ClimbPerPixel = ClimbPerPixel });

    /// <summary>
    /// The stretches of the room that moves of the player's body reached on this update, made or only tried: for each
    /// move tried only where it ends, the part of the body's leading edge there that came before whatever stopped it
    /// (<see cref="MoveResult.Reached"/>).
    /// </summary>
    private readonly List<PixelRect> _reached = [];

    /// <summary>
    /// Moves the player's body across through the room's mover. Every move of the player's body is made by this or
    /// by <see cref="MoveBodyDown"/>, or tried by <see cref="TryBodyAcross"/> or <see cref="TryBodyDown"/>, whoever
    /// moves it, so that what the move reached is known (<see cref="Reaches"/>).
    /// </summary>
    public MoveResult MoveBodyAcross(Pixels distance, MoveOptions options, Pixels? triedAs = null, Facing? stillToward = null) =>
        Noted(Room.Mover.MoveAcross(Body, distance, options, triedAs, stillToward));

    /// <summary>Moves the player's body down through the room's mover, upward when the distance is negative (<see cref="MoveBodyAcross"/>).</summary>
    public MoveResult MoveBodyDown(Pixels distance, MoveOptions options) => Noted(Room.Mover.MoveDown(Body, distance, options));

    /// <summary>Tries a move of the player's body across without making it; what it reached counts as a made move's does.</summary>
    public MoveResult TryBodyAcross(Pixels distance, MoveOptions options, out PixelPosition rest) =>
        Noted(Room.Mover.TryAcross(Body, distance, options, out rest));

    /// <summary>Tries a move of the player's body down without making it; what it reached counts as a made move's does.</summary>
    public MoveResult TryBodyDown(Pixels distance, MoveOptions options, out PixelPosition rest, PixelRect? shapedAs = null) =>
        Noted(Room.Mover.TryDown(Body, distance, options, out rest, shapedAs));

    /// <summary>
    /// Where the player's body met blocks on this update: the tile of terrain that stopped each move of the body, made
    /// or only tried, and wherever a state that moves the body a way of its own said it met them (<see cref="MeetBlocksAt"/>).
    /// </summary>
    private readonly List<PixelRect> _met = [];

    /// <summary>
    /// Says that the player's body met whatever block lies at <paramref name="point"/> on this update: for a state
    /// that moves the body a way of its own, not through the room's mover, and feels for blocks point by point as a
    /// swing on a line does. A block there that acts on contact acts as one that stopped a move of the body does.
    /// </summary>
    public void MeetBlocksAt(PixelPosition point) => _met.Add(new PixelRect(point.X.Whole, point.Y.Whole, 1, 1));

    /// <summary>Notes what a move of the body reached, and the tile that stopped it.</summary>
    private MoveResult Noted(MoveResult result)
    {
        if (result.Reached is { IsEmpty: false } reached)
            _reached.Add(reached);
        if (result.StoppedByTile is { } tile)
            _met.Add(tile);
        return result;
    }

    /// <summary>
    /// The blocks the player is in, for what a block does to whoever is in it: those down the middle column of their
    /// body at their feet, their middle and their head, whichever of the three the game counts for a body lying as
    /// theirs does (<see cref="IBodyReadings"/>). A block beside that column, or one the body only lies against, is
    /// not one they are in.
    /// </summary>
    private List<Block> BlocksStoodIn()
    {
        IBodyReadings readings = _bodyReadings ?? throw new InvalidOperationException("The player is not in a room.");
        PixelRect body = BodyRect;
        int x = Position.X.Whole;
        var blocks = new List<Block>();
        foreach (int y in readings.HeightsStoodAt(body.Bottom - 1, Position.Y.Whole, body.Top, Room.Terrain.TileSize))
        {
            foreach (Block block in Room.Terrain.BlocksIn(new PixelRect(x, y, 1, 1)))
            {
                if (!blocks.Contains(block))
                    blocks.Add(block);
            }
        }

        return blocks;
    }

    /// <summary>
    /// The blocks the player's body met on this update: of each move stopped by terrain, the first tile its leading
    /// edge was stopped by, and what a state said it met. A solid block that acts on contact, as a spike does, acts on
    /// a player who met it so, and on no one who only lies against it.
    /// </summary>
    private List<Block> BlocksMet()
    {
        var blocks = new List<Block>();
        foreach (PixelRect tile in _met)
        {
            foreach (Block block in Room.Terrain.BlocksIn(tile))
            {
                if (!blocks.Contains(block))
                    blocks.Add(block);
            }
        }

        return blocks;
    }

    /// <summary>
    /// Whether the player reaches into <paramref name="area"/>: their body lies in it, or a move of their body on this
    /// update was tried into it, though something beside it stopped them short of it. What is taken by being touched
    /// is touched so: a thing standing on a block is touched by a player who runs against that block.
    /// </summary>
    public bool Reaches(PixelRect area) => BodyRect.Overlaps(area) || MovesReached(area);

    /// <summary>
    /// Whether a move of the player's body on this update, made or only tried, was tried into <paramref name="area"/>:
    /// whether the body's leading edge, where the move was tried, lay in it before anything stopped the move. A body
    /// that only lies in the area, having moved no edge into it on this update, has not.
    /// </summary>
    public bool MovesReached(PixelRect area) => _reached.Exists(reached => reached.Overlaps(area));

    /// <summary>Whether the player stands on terrain or a solid.</summary>
    public bool IsOnGround => Room.Mover.IsOnGround(Body, BodyMoves) || StandsInBlockThatHoldsUp();

    /// <summary>
    /// Whether the player stands on terrain or a solid by the shape of the form they are in, where they are: for a
    /// form just taken, whose shape the body has only from their next update on, and whose fitting has already put
    /// them where it stands.
    /// </summary>
    public bool IsOnGroundInForm =>
        !Room.Mover.IsFree(BodyRectFor(Form).At(Position).MovedBy(0, 1), this, BodyMoves) || StandsInBlockThatHoldsUp();

    /// <summary>
    /// Whether the player's feet are in something that holds them up without being solid, as the surface of quicksand
    /// does. A fall asks this once it has moved: with the feet left in such a thing it has landed.
    /// </summary>
    internal bool IsHeldUpWhereTheyStand => StandsInBlockThatHoldsUp();

    /// <summary>Whether the player's feet are in a block that pulls down and yet holds up what stands in it, as quicksand does.</summary>
    private bool StandsInBlockThatHoldsUp() =>
        Room.Terrain.BlocksIn(FeetRect).Any(block => block.Type.Contact == BlockContact.PullsDown && block.Type.HoldsUp);

    /// <summary>The bottom row of the player's body.</summary>
    private PixelRect FeetRect => new(BodyRect.Left, BodyRect.Bottom - 1, BodyRect.Width, 1);

    /// <summary>
    /// The medium the player is moving in: the liquid their feet are in, when it hinders them and they have a medium
    /// of its name, and otherwise air. It is settled as it is first asked in an update, by the feet of the form they
    /// then have, and again whenever their form changes within the update.
    /// </summary>
    public PlayerMedium Medium => _medium ??= FindMedium();

    /// <summary>
    /// Whether <paramref name="point"/> of the player is in <paramref name="liquid"/>: the pixel row it lies in is
    /// below the surface's line, not on it.
    /// </summary>
    public static bool IsUnder(Liquid liquid, PixelPosition point)
    {
        ArgumentNullException.ThrowIfNull(liquid);
        return liquid.IsSubmerged(point);
    }

    /// <summary>Whether the player's feet are in the room's liquid, whatever their equipment makes of it.</summary>
    public bool FeetAreInLiquid => Room.Liquid is { } liquid && IsIn(liquid, Feet);

    /// <summary>Whether a point of the player is in a liquid the player meets: one held back, as behind glass, is not met (<see cref="Liquid.IsMet"/>).</summary>
    private static bool IsIn(Liquid liquid, PixelPosition point) => liquid.IsMet && IsUnder(liquid, point);

    /// <summary>
    /// The speed the player last came down onto ground at, for a landing that is not on their feet. A state that
    /// brings the player down some other way than <see cref="Land"/> says so here.
    /// </summary>
    public void CameDownAt(Pixels speed)
    {
        LandingSpeed = speed;
        _landingHeld = IsRising || IsAdrift ? -speed : speed;
        _cameDown = true;
    }

    /// <summary>Whether the player's feet are in a liquid that hinders them.</summary>
    public bool IsHinderedByLiquid => HinderingLiquid(Feet) is not null;

    /// <summary>Whether the top of the player's body is in a liquid that hinders them.</summary>
    public bool IsWhollyInLiquid => HinderingLiquid(Head) is not null;

    /// <summary>The move values of the state named <paramref name="state"/>, in the medium the player is in.</summary>
    public MoveValues Moves(string state) => Medium.MovesFor(state);

    /// <summary>Which way a shot leaves: one step across and one step down, each -1, 0 or 1.</summary>
    public virtual (int Across, int Down) AimDirection
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
            _readings = room.Session.Definition.CreateSpeedReadings();
            _bodyReadings = room.Session.Definition.CreateBodyReadings();
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
            PublishForm();
            _states = new StateMachine<Player>(this, new StandingState(), AdmitAndTell);
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

    /// <summary>
    /// Called as an item becomes the player's: what it grants is theirs, and its message, if it has one, has yet to
    /// be shown.
    /// </summary>
    public virtual void OnItemCollected()
    {
    }

    /// <summary>
    /// Called as an item's pickup ends: its message has been shown, and the update it was collected on goes on.
    /// </summary>
    public virtual void OnItemPickupEnded()
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
    /// Plays one whole update with nothing between its two parts: <see cref="BeginUpdate"/>, then
    /// <see cref="FinishUpdate"/>. A room plays the two parts itself, with its enemies between them.
    /// </summary>
    public void Step(GameInput input)
    {
        BeginUpdate(input);
        FinishUpdate();
    }

    /// <summary>
    /// Plays the first part of an update, which comes before the room's enemies act: weapons fire from the state, the
    /// place and the aim the last update left the player in, and then the input is asked what it wants of that state.
    /// </summary>
    public void BeginUpdate(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _input = input;

        // An interruption still waiting as an update begins has waited an update more: one a state held off, one
        // that did not fit, and one asked for only once the player had moved on the update before.
        if (_interruption is not null)
            _interruptionWaited++;
        _medium = null;
        _reached.Clear();
        _met.Clear();
        _invincible = _invincible.Stepped();
        (_asked, _askedOf) = (null, null);

        // The form taken since the last update began gives the body its shape only now.
        PublishForm();

        // What the blocks the player is in do to them is settled now, by the body just published and before they
        // attack, the enemies act or they move: it is part of this update's movement. While their state holds the
        // room still, the blocks do nothing to them.
        _stoodIn = BlocksStoodIn();
        _blockMove = default;
        if (!State.Stops.HasFlag(World.Stopped.Room))
            NoteBlocksStoodIn();
        TakeJumpBuffer();
        BeforeState();

        // A state that moves the player before the enemies act moves them now; a throw moves them in its place, later.
        _movedEarly = Thrown is null && State is { MovesBeforeEnemies: true } early ? (early, early.Move(this)) : null;
        if (AttacksNow)
            Attack(input);
        else
            WaitToAttack();
        PlayerState state = State;
        _asked = AskedOf(state);
        _askedOf = state;

        // A state that gives way to what the input asks before the update's movement is left now: the player is in
        // the state asked for through the rest of the update, and moves as it, having been asked nothing more.
        if (_asked is { } wanted && !ReferenceEquals(wanted, state) && state.GivesWayBeforeMoving(this, wanted))
        {
            ChangeState(EnteringFrom(state, wanted, unasked: false));
            (_asked, _askedOf) = (null, null);
        }

        // From here until the player has moved, what moves them from outside is given to that move.
        _ownMoveToCome = true;
    }

    /// <summary>
    /// Whether the player's own move of this update is still to come: from the end of the update's first part until
    /// their state has moved them. What something outside gives them to move by meanwhile is part of that move.
    /// </summary>
    private bool _ownMoveToCome;

    /// <summary>
    /// Moves the player by <paramref name="offset"/> at the word of something outside them, as an enemy that carries
    /// or pushes them does. While their own move of this update is still to come the offset is given to it
    /// (<see cref="_blockMove"/>): they are not moved now, and what comes between, the enemies that take their turns
    /// later among them, meets them where they were. At any other time, with them not being played or already moved,
    /// they are moved at once, as far as <paramref name="atOnce"/> lets them go.
    /// </summary>
    public void MoveFromOutside(PixelOffset offset, MoveOptions atOnce)
    {
        ArgumentNullException.ThrowIfNull(atOnce);
        if (_ownMoveToCome)
        {
            _blockMove = new PixelOffset(_blockMove.X + offset.X, _blockMove.Y + offset.Y);
            return;
        }

        if (!offset.X.IsZero)
            MoveBodyAcross(offset.X, atOnce);
        if (!offset.Y.IsZero)
            MoveBodyDown(offset.Y, atOnce);
    }

    /// <summary>
    /// Whether this update's attack is played: whether the state the player is in can attack, unless the game's
    /// player says otherwise, as one does that still owes a shot from the state it has just left.
    /// </summary>
    protected virtual bool AttacksNow => State.CanAttack;

    /// <summary>
    /// Plays the rest of an update, which comes after the room's enemies have acted: the player moves as the state
    /// they are in, settles, and whatever change is due takes effect; then active body powers hit what the body touches.
    /// </summary>
    public void FinishUpdate()
    {
        BeforeMoving();
        MovedAs = State;
        States.Step();
        _ownMoveToCome = false;
        DeliverBodyPowers();
        AfterState();
        CountJumpHeld();
    }

    /// <summary>
    /// The state the player last moved as: what they were doing through the latest update's movement, whatever they
    /// have become since. Nothing before their first.
    /// </summary>
    public PlayerState? MovedAs { get; private set; }

    /// <summary>The state that moved the player in the first part of this update, and what its move answered; null when none did.</summary>
    private (PlayerState State, PlayerState? Met)? _movedEarly;

    /// <summary>
    /// What the move <paramref name="state"/> made in the first part of this update answered
    /// (<see cref="PlayerState.MovesBeforeEnemies"/>); null when it answered nothing, or made none because the player
    /// was put in it since.
    /// </summary>
    internal PlayerState? TakeEarlyMove(PlayerState state)
    {
        (PlayerState State, PlayerState? Met)? moved = _movedEarly;
        _movedEarly = null;
        return moved is { } early && ReferenceEquals(early.State, state) ? early.Met : null;
    }

    /// <summary>What the input asked, in the first part of this update, of the state the player was then in; and that state.</summary>
    private PlayerState? _asked;
    private PlayerState? _askedOf;

    /// <summary>
    /// What this update's input asked of <paramref name="state"/>, which is about to move the player. A state the
    /// player was put into since the input was asked was asked nothing: it carries on.
    /// </summary>
    internal PlayerState? TakeAsked(PlayerState state)
    {
        PlayerState? asked = ReferenceEquals(_askedOf, state) ? _asked : state;
        (_asked, _askedOf) = (null, null);
        return asked;
    }

    /// <summary>
    /// Settles the player where this update's movement left them, before any change of state takes effect: they stand
    /// on what is under them, land, are moved by the blocks they are in, splash, and take the room's ongoing damage.
    /// A hurt taken here interrupts them on this same update.
    /// </summary>
    internal void SettleAfterMoving()
    {
        if (State is DyingState)
            return;
        bool onGround = IsOnGround;

        // A body whose moves are tried every pixel of the way stands on the ground it rests on, and has landed when
        // it has come to rest on ground it was not on. One tried only where its moves end may lie on the ground an
        // update before the move that stops on it: that one stands on the terrain that stopped a move down of its
        // own on this update, and has landed when the state it is in says it came down.
        bool triedWhereItEnds = BodyMoves.Tries == WayTried.WhereItEnds;
        if (triedWhereItEnds ? _groundStoppedMove : onGround)
        {
            // Stopped by one tile of the ground, the player stands on that tile alone.
            PixelRect body = BodyRect;
            Room.Terrain.StandOn(triedWhereItEnds && _stoppedOnTile is { } tile ? new PixelRect(tile.Left, body.Top, tile.Width, body.Height) : body);
        }

        if (triedWhereItEnds ? _cameDown : onGround && !_wasOnGround)
            OnLanded();
        (LandingSpeed, _landingHeld) = (Pixels.Zero, Pixels.Zero);
        (_cameDown, _groundStoppedMove, _stoppedOnTile) = (false, false, null);
        _wasOnGround = onGround;

        // While the player's state holds the room still, the room does nothing to them either: its blocks do not
        // carry or pull them, and whatever damage the update gathered for them is let go, not taken.
        bool roomHeldStill = State.Stops.HasFlag(World.Stopped.Room);
        if (!roomHeldStill)
            MoveByWhatBlocksLeft();
        Splash();
        if (roomHeldStill)
            _addedOngoing = 0;
        else
            TakeOngoingDamage();
    }

    /// <summary>Whether a state said, on this update, that the player came down onto ground.</summary>
    private bool _cameDown;

    /// <summary>Whether terrain stopped a move down of the player's own on this update, and the tile that last did.</summary>
    private bool _groundStoppedMove;
    private PixelRect? _stoppedOnTile;

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
        bool inLiquid = liquid is not null && IsIn(liquid, Feet);
        bool crossed = _feetWereInLiquid is bool before && before != inLiquid;
        _feetWereInLiquid = inLiquid;
        if (!crossed || liquid is null)
            return;
        if (liquid.Type.SplashEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(liquid.Type.SplashEffect)), new PixelPosition(Position.X, liquid.Surface)));
        string sound = inLiquid ? liquid.Type.SplashSound : liquid.Type.SplashOutSound;
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "liquid");
        OnSplashed(liquid, inLiquid);
    }

    /// <summary>
    /// Called as the player's feet have crossed a liquid's surface, once the splash is left and its sound asked for:
    /// what a game's player asks for here follows the splash's sound. The engine's player does nothing.
    /// </summary>
    /// <param name="liquid">The liquid whose surface was crossed.</param>
    /// <param name="entered">Whether the feet went into it; otherwise they came out.</param>
    protected virtual void OnSplashed(Liquid liquid, bool entered)
    {
    }

    /// <summary>Called at the start of each update, before weapons fire and the input is asked.</summary>
    protected virtual void BeforeState()
    {
    }

    /// <summary>Called as the second part of each update begins, after the room's enemies have acted and before the player moves.</summary>
    protected virtual void BeforeMoving()
    {
    }

    /// <summary>Called at the end of each update, after the player's state, weapons, body powers and ongoing damage.</summary>
    protected virtual void AfterState()
    {
    }

    /// <summary>
    /// Lets a game's player ask for a state of its own. Called at the start of each update, before the player moves,
    /// with the state the player is in; returns the state the input asks for, which is entered once this update's
    /// movement is done unless something outranks it, or null to leave the asking to the state itself.
    /// </summary>
    protected virtual PlayerState? ChooseState(PlayerState current) => null;

    /// <summary>
    /// Called as the player, having moved with nothing ended, interrupting or met, is about to be in
    /// <paramref name="to"/>: the state their input asked for, the state they do unasked, or
    /// <paramref name="from"/> itself when they carry on. A game whose player does something of its own at that moment
    /// does it here, and returns the state to be in.
    /// </summary>
    /// <param name="from">The state the player moved as.</param>
    /// <param name="to">The state they are about to be in.</param>
    /// <param name="unasked">Whether this update's input asked for nothing.</param>
    protected abstract PlayerState Entering(PlayerState from, PlayerState to, bool unasked);

    /// <summary>Tells the game's player that they are about to be in a state, and gives the state to be in.</summary>
    internal PlayerState EnteringFrom(PlayerState from, PlayerState to, bool unasked) => Entering(from, to, unasked);

    /// <summary>
    /// Called as the player leaves <paramref name="from"/> for <paramref name="to"/>, whatever takes them there: what
    /// their input asked for, what their movement met, or something that interrupts them. The state has been
    /// admitted, and neither has <paramref name="from"/> been left nor <paramref name="to"/> entered. A game does
    /// here what passing between two of its states does to its player however it comes about; what only an asked
    /// change does, and whatever names another state, is <see cref="Entering"/>'s.
    /// </summary>
    protected virtual void Leaving(PlayerState from, PlayerState to)
    {
    }

    /// <summary>Admits a state asked for (<see cref="Admit"/>) and tells the game's player of the change it makes.</summary>
    private State<Player> AdmitAndTell(State<Player> asked)
    {
        State<Player> admitted = Admit(asked);
        if (!ReferenceEquals(admitted, States.Current))
            Leaving(State, (PlayerState)admitted);
        return admitted;
    }

    /// <summary>
    /// Called as a jump from the ground has gathered itself and is about to rise in the stance it began in. A game
    /// whose player does something else at that moment returns the state to be in instead, which takes effect as a
    /// state's own end does; null to jump.
    /// </summary>
    protected virtual PlayerState? JumpGathered(JumpingState jump) => null;

    /// <summary>Tells the game's player that a jump has gathered itself, and gives the state to be in instead, if any.</summary>
    internal PlayerState? AfterGathering(JumpingState jump) => JumpGathered(jump);

    /// <summary>What the player's input asks of <paramref name="state"/>: the game's own answer, and failing that the state's.</summary>
    internal PlayerState? AskedOf(PlayerState state) => ChooseState(state) ?? state.Asked(this);

    /// <summary>
    /// What the state this update's input asked for holds still from the asking
    /// (<see cref="PlayerState.StopsAsItIsAsked"/>): nothing when nothing was asked. The room reads it once the
    /// update's first part has been played.
    /// </summary>
    internal World.Stopped StopsAsAsked => _asked?.StopsAsItIsAsked ?? World.Stopped.Nothing;

    /// <summary>
    /// Lets the state the player is in play its part as an update of play begins
    /// (<see cref="PlayerState.AsUpdateBegins"/>), and puts the player in the state it gives way to, if it does.
    /// Play calls this before it decides anything of the update, and so before the update's first part
    /// (<see cref="BeginUpdate"/>).
    /// </summary>
    internal void GiveWayAsUpdateBegins()
    {
        if (State.AsUpdateBegins(this) is { } next && !ReferenceEquals(next, State))
            ChangeState(next);
    }

    /// <summary>
    /// The state waiting to interrupt the player, if any, and how many updates it has waited: how many have begun
    /// with it waiting (<see cref="BeginUpdate"/>).
    /// </summary>
    private PlayerState? _interruption;
    private int _interruptionWaited;

    /// <summary>
    /// Interrupts the player with <paramref name="state"/> from outside: they are put into it when they have next
    /// moved as the state they are in, ahead of whatever that movement met and whatever their input asked for. Asked
    /// for before they move on an update, it has waited nothing when it is taken; asked for once they have moved, it
    /// waits for their movement on the next and has waited an update by then.
    /// </summary>
    public void Interrupt(PlayerState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _interruption = state;
        _interruptionWaited = 0;
    }

    /// <summary>
    /// Takes the state waiting to interrupt the player, who has just moved as <paramref name="from"/>, leaving none
    /// waiting. Null when none was, when <paramref name="from"/> holds it off for another update, or when it has
    /// waited too long to interrupt at all.
    /// </summary>
    internal PlayerState? TakeInterruption(PlayerState from)
    {
        if (_interruption is not { } waiting)
            return null;
        if (from.HoldsOffInterruptions)
            return null;

        _interruption = null;
        return waiting.StillInterrupts(this, _interruptionWaited) ? waiting : null;
    }

    /// <summary>
    /// Passes an update in a state the player cannot attack in: the wait between their shots counts down all the
    /// same. A game with states in which it stands still says so here.
    /// </summary>
    protected virtual void WaitToAttack() => Weapons.StepCooldown();

    /// <summary>Uses the player's weapons for one update. A game whose player attacks in ways of its own does so here.</summary>
    protected virtual void Attack(GameInput input) => Weapons.Step(this, input);

    /// <summary>
    /// Puts the player into <paramref name="state"/> now. Whatever was waiting to interrupt them is dropped, and a
    /// throw carrying them is over.
    /// </summary>
    public void ChangeState(PlayerState state)
    {
        _interruption = null;
        Thrown = null;
        States.ChangeTo(state);
    }

    /// <summary>The throw carrying the player, while a hit has thrown them; null otherwise.</summary>
    public Throw? Thrown { get; private set; }

    /// <summary>
    /// Begins a throw: from now it moves the player in place of the state they are in, and they rise by the medium's
    /// knockback speed. The state they are in and the game's player are told.
    /// </summary>
    internal void BeThrown(Throw thrown)
    {
        ArgumentNullException.ThrowIfNull(thrown);
        Thrown = thrown;
        Rise(Pixels.FromNumber(Medium.KnockbackUpSpeed));
        State.OnThrown(this);
        OnThrown();
    }

    /// <summary>Ends the throw carrying the player, if one is.</summary>
    public void EndThrow() => Thrown = null;

    /// <summary>Lets a throw that was carrying the player go on carrying them, as it stands, into the state they have just been put in.</summary>
    internal void GoOnThrown(Throw thrown) => Thrown = thrown;

    /// <summary>Called as a throw takes the player. A game whose player does something of its own as a knockback begins does it here.</summary>
    protected virtual void OnThrown()
    {
    }

    /// <summary>Knocks the player back, away from <paramref name="from"/>, once they have next moved.</summary>
    public void KnockBack(Facing from) => Interrupt(new KnockedBackState(from.Opposite()));

    /// <summary>Knocks the player back, away from <paramref name="from"/>, for <paramref name="updates"/> updates, once they have next moved.</summary>
    public void KnockBack(Facing from, int updates) => Interrupt(new KnockedBackState(from.Opposite(), updates));

    /// <summary>The state a runner is in once a wall has stopped them, which the game names.</summary>
    protected internal abstract PlayerState StoppedByWall();

    /// <summary>
    /// Whether the player moved through the latest update as one who has run into a wall: the game says, by the state
    /// it has for it. What is used by being run against asks this.
    /// </summary>
    public abstract bool MovedAgainstWall { get; }

    /// <summary>Takes hold of the player: they stay where they are put until released.</summary>
    public void Hold() => ChangeState(new HeldState(State is HeldState held ? held.Was : State));

    /// <summary>
    /// Takes hold of the player as they are: what they were doing is kept beneath the hold, neither left nor played,
    /// with the speed they had, and letting go (<see cref="Release"/>) has them go on in it as though nothing had
    /// happened. A player already held stays held as they are.
    /// </summary>
    public void HoldAsTheyAre()
    {
        if (State is HeldState)
            return;
        _interruption = null;
        States.Cover(new HeldState(State, keepsSpeed: true));
    }

    /// <summary>Holds the player at a station until released.</summary>
    /// <param name="facingOut">Whether the player turns to face out of the screen while held.</param>
    public void UseStation(bool facingOut = false) => ChangeState(new UsingStationState { FacingOut = facingOut });

    /// <summary>
    /// Gives the player back control after being held or at a station: in what they were doing, where the hold kept
    /// it (<see cref="HoldAsTheyAre"/>), and otherwise in the state at rest.
    /// </summary>
    public void Release()
    {
        if (!States.Covers)
        {
            ChangeState(StateAtRest());
            return;
        }

        _interruption = null;
        States.Uncover();
    }

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

    /// <summary>Starts the player dying. Play hands over to the game's death on the update the room settles it (<see cref="Core.GameDefinition.CreateDeathMode"/>).</summary>
    public void Die()
    {
        if (State is DyingState)
            return;
        PlayerState from = State;
        OnDying(from);

        // A state whose movement goes on is kept beneath the death, not left, and goes on moving the player; any
        // other is left, a throw that was carrying them going on.
        if (Thrown is null && from is not HeldState && from.MovesOnWhileDying)
            States.Cover(new DyingState(null, from));
        else
            ChangeState(new DyingState(Thrown));
    }

    /// <summary>
    /// Called as the player begins to die, still in <paramref name="from"/>, the state they were in. A game whose
    /// player keeps something of what they were doing through their death notes it here.
    /// </summary>
    protected virtual void OnDying(PlayerState from)
    {
    }

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => side != HitSide.Player;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        if (IsDying || ShrugsOff(hit))
            return HitResult.Ignored;

        // A body that is an attack strikes whatever touches it, whether or not hits land on the player just now: this
        // is where an enemy is struck by it.
        if (hit.Carries(HitKind.Contact) && hit.Source is IHittable toucher && BodyPowerStops(toucher))
            return HitResult.Ignored;
        if (!AcceptsHit(hit))
            return HitResult.Ignored;
        // A hit that takes the last of the player's energy is a hit like any other: what follows from having none is
        // settled once, as the update ends (SettleEmptyEnergy).
        Inventory.Lose(Inventory.Energy, DamageFrom(hit));
        _invincible = Countdown.Of(InvincibleUpdates);
        OnHurt(hit, SideOf(hit));
        return HitResult.Hurt;
    }

    /// <summary>
    /// The side a hit comes from, which the player is knocked away from. A hit that says where it is, as a shot's
    /// does, comes from the side of that place, by whole pixels, one level with the player's own counting as from
    /// their left; one that does not comes from behind the way it was travelling; and one with neither from the way
    /// the player faces.
    /// </summary>
    private Facing SideOf(Hit hit)
    {
        if (hit.At is { } at)
            return Position.X.Whole >= at.X.Whole ? Facing.Left : Facing.Right;
        return hit.Direction.X.IsZero ? Facing : hit.Direction.X.IsNegative ? Facing.Right : Facing.Left;
    }

    /// <summary>
    /// Takes a hit's energy from the player and nothing more: they are not knocked back and not made invincible. For
    /// something that feeds on the player while it clings to them.
    /// </summary>
    public void Sap(Hit hit)
    {
        if (IsDying)
            return;
        Inventory.Lose(Inventory.Energy, DamageFrom(hit));
    }

    /// <summary>
    /// Settles what follows from the player having no energy left. The room asks once in each update, when its
    /// sequences have played: until then a player whose energy is gone plays the update on like any other, is hit
    /// again, and may be given energy back. With none left they are saved if what they are saves them
    /// (<see cref="SurvivesEmptyEnergy"/>), and otherwise die, unless <paramref name="mayDie"/> is false, as when the
    /// update has also taken them into an exit: then they go on with none, to be asked again on a later update.
    /// </summary>
    /// <param name="mayDie">Whether the player may begin to die on this update.</param>
    /// <param name="mayBeSaved">Whether what the player is may save them on this update.</param>
    /// <returns>What was settled.</returns>
    internal EmptyEnergy SettleEmptyEnergy(bool mayDie, bool mayBeSaved = true)
    {
        if (IsDying || Inventory.Amount(Inventory.Energy) > 0)
            return EmptyEnergy.NotGone;
        if (mayBeSaved && SurvivesEmptyEnergy())
            return EmptyEnergy.Saved;
        if (!mayDie)
            return EmptyEnergy.Withheld;
        Die();
        return EmptyEnergy.Dying;
    }

    /// <summary>The energy <paramref name="hit"/> would take from the player as they are equipped now.</summary>
    public int EnergyLostTo(Hit hit) => DamageFrom(hit);

    /// <summary>
    /// Whether the enemies' shots are tried against the player on this update. The room asks once, when all the
    /// shots have moved and its objects have played, before any is tried; none is, once the player is dead or dying.
    /// A game with times at which no shot can reach the player says so here.
    /// </summary>
    protected internal virtual bool BeginEnemyShots() => !IsDying;

    /// <summary>Called when the enemies' shots have all been tried against the player on this update.</summary>
    protected internal virtual void EndEnemyShots()
    {
    }

    /// <summary>
    /// Whether a hit is shrugged off because the player was hurt a moment ago. It is while they are invincible,
    /// unless the game says otherwise.
    /// </summary>
    protected virtual bool ShrugsOff(Hit hit) => IsInvincible;

    /// <summary>Whether a hit can land on the player at the moment. A game with states in which the player cannot be hit says so here.</summary>
    protected virtual bool AcceptsHit(Hit hit) => true;

    /// <summary>The energy a hit takes from the player, which the game says: its equipment may lessen a hit.</summary>
    protected abstract int DamageFrom(Hit hit);

    /// <summary>
    /// Called when the player's energy has run out, before they die. A game that has something to fall back on, such
    /// as a reserve, uses it here and returns true.
    /// </summary>
    protected virtual bool SurvivesEmptyEnergy() => false;

    /// <summary>
    /// Called when a hit has hurt the player without killing them: whether it knocks them back, and for how long it
    /// leaves them safe if not the usual, is the game's to say.
    /// </summary>
    /// <param name="hit">The hit.</param>
    /// <param name="from">The side the hit came from.</param>
    protected abstract void OnHurt(Hit hit, Facing from);

    /// <summary>Makes the player unable to be hurt for <paramref name="updates"/> updates, as after a hit.</summary>
    public void BeInvincibleFor(int updates) => _invincible = Countdown.Of(updates);

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);

    /// <summary>The form named <paramref name="form"/> when the player has one of that name, and otherwise the upright form.</summary>
    public string FormOr(string form) => Forms.ContainsKey(form) ? form : Standing;

    /// <summary>
    /// Changes the body's form, if the new form fits (<see cref="FitForm"/>), and returns whether the player is in
    /// it. For a change in the midst of a state; the form a state is entered in is the state's to name
    /// (<see cref="PlayerState.EntersIn"/>).
    /// </summary>
    public bool TrySetForm(string form, bool aroundMiddle = false, bool atOnce = false)
    {
        if (form == Form)
            return true;

        // A fitting that answers with another form is taken only where a state is being entered (Admit): here, in
        // the midst of a state, the player stays as they are.
        FormFit fit = FitForm(form, aroundMiddle);
        if (fit.Form is not null && fit.Form != form)
            return false;
        if (TakeFit(fit) != form)
            return false;

        // A form asked for at once has its body from this moment, for whatever of the update is left to meet.
        if (atOnce)
            PublishForm();
        return true;
    }

    /// <summary>
    /// Takes <paramref name="form"/> where the player is, with no room felt for it, as a pose the cartridge sets
    /// outright is taken: the middle of the body stays where it is, or, with <paramref name="feetKept"/>, the foot of
    /// the body does and the middle moves by the difference of the two heights. A change of form is no move of the
    /// player's: with the feet kept, the view takes them to have stood as much higher or lower. For a change in the
    /// midst of a state, or one made just before a state is entered whose form it then already is, so that the state
    /// fits nothing (<see cref="PlayerState.EntersIn"/>). With <paramref name="atOnce"/> the body is the new form's
    /// from this moment.
    /// </summary>
    public void TakeFormOutright(string form, bool feetKept = false, bool atOnce = false)
    {
        if (form == Form)
            return;
        if (feetKept)
        {
            int lift = HalfHeightOf(Form) - HalfHeightOf(form);
            Room.Session.Camera.ShiftWherePlayerStoodDown(lift);
            Position = new PixelPosition(Position.X, Position.Y + Pixels.FromWhole(lift));
        }

        ApplyForm(form);
        if (atOnce)
            PublishForm();
    }

    /// <summary>Whether the body could take <paramref name="form"/> where it is, as <see cref="TrySetForm"/> would place it.</summary>
    public bool CanTakeForm(string form, bool aroundMiddle = false) => FormAnsweredFor(form, aroundMiddle) == form;

    /// <summary>
    /// The form the fitting answers with when <paramref name="form"/> is asked for where the body is, changing
    /// nothing: the form itself where it fits, another where the game's fitting puts the player in one in its place,
    /// and null where they would stay as they are.
    /// </summary>
    public string? FormAnsweredFor(string form, bool aroundMiddle = false) => form == Form ? form : FitForm(form, aroundMiddle).Form;

    /// <summary>
    /// Fits <paramref name="form"/> to the body where it is, changing nothing: the form taken, where that leaves the
    /// player, and whether the view takes them to have stood there. The engine's own fitting takes the first place
    /// the form fits: on the ground with the feet where they are; in the air, or when <paramref name="aroundMiddle"/>
    /// is asked for, with the middle of the body where it is, or failing that the feet, or failing that the head;
    /// and with no such place the player stays as they are. A game whose player fits its forms another way, or is put
    /// in another form when one does not fit, fits them here.
    /// </summary>
    protected virtual FormFit FitForm(string form, bool aroundMiddle) =>
        PlaceInForm(form, aroundMiddle) is { } place
            ? new FormFit(form, place, place.Y.Whole != Position.Y.Whole)
            : new FormFit(null, Position, false);

    /// <summary>Puts the player where a fitting leaves them, in the form it gives, and returns that form: null when they stay in the form they have.</summary>
    private string? TakeFit(FormFit fit)
    {
        // A change of form that moves the middle of the body is not a move of the player's, and it comes once the
        // update's movement is made, whether or not the input asked for the state it is made for: the view takes
        // them to have stood where the change leaves them, and follows nothing of this update's way down.
        if (fit.StoodThere)
            Room.Session.Camera.TakePlayerToHaveStoodAtY(fit.Place.Y.Whole);
        Position = fit.Place;
        if (fit.Form is { } taken)
            ApplyForm(taken);
        return fit.Form;
    }

    /// <summary>
    /// The state the player enters when <paramref name="asked"/> is asked for (<see cref="PlayerState.EntersIn"/>).
    /// A state that names the form it is entered in has that form fitted first: fitted, it is entered; not fitted,
    /// the player stays in the state they are in; answered with another form, the player is in the state that goes
    /// with that form, or stays in the one they are in when it is of that kind already. A form asked for unfitted
    /// (<see cref="FormAsked.Unfitted"/>) is taken where they are and the state entered.
    /// </summary>
    private State<Player> Admit(State<Player> asked)
    {
        // A knockback that arrives while a throw is under way does not begin again: the throw takes its way and its
        // time and goes on as it was, and the player stays in the state they are in.
        if (asked is KnockedBackState renewing && Thrown is { } under)
        {
            renewing.Renew(this, under);
            return States.Current;
        }

        // The low form is not knocked back into a state of the knockback's: it is thrown as it is, in its own state,
        // which goes on being asked its input while the throw moves it.
        if (asked is KnockedBackState knockback && Form == Crawling)
        {
            BeThrown(knockback.ThrowOfLowForm(this));
            return State is CrawlingState ? States.Current : StateFor(Crawling);
        }

        if (asked is not PlayerState state || state.EntersIn(this) is not { } wants || wants.Form == Form)
            return asked;
        if (wants.Unfitted)
        {
            ApplyForm(wants.Form);
            if (wants.AtOnce)
                PublishForm();
            return asked;
        }

        string? taken = TakeFit(FitForm(wants.Form, wants.AroundMiddle));
        if (taken == wants.Form)
        {
            // A form asked for at once has its body from this moment, as one set at once within a state has.
            if (wants.AtOnce)
                PublishForm();
            return asked;
        }

        // Not entered, a state that waits when its form does not fit goes on waiting to interrupt.
        if (state.WaitsWhenItDoesNotFit)
            _interruption = state;

        if (taken is null)
            return States.Current;
        PlayerState instead = StateFor(taken);
        if (States.Current.GetType() != instead.GetType())
            return instead;

        // Staying in the state of the form the fitting answered with, they are in it plainly: their aim is level, as
        // a crouch taken in place of a taller form is the plain crouch.
        Aim = Aim.Forward;
        return States.Current;
    }

    /// <summary>The state that goes with a form the player was put in when the form a state asked for did not fit.</summary>
    protected virtual PlayerState StateFor(string form) =>
        form == Crouching ? new CrouchingState() : form == Crawling ? new CrawlingState() : StateAtRest();

    /// <summary>Where the player would be in another form: the first place the form fits, or null when it fits nowhere.</summary>
    private PixelPosition? PlaceInForm(string form, bool aroundMiddle)
    {
        int change = HalfHeightOf(form) - HalfHeight;
        PixelPosition keepingFeet = Position + PixelOffset.FromWhole(0, -change);
        PixelPosition keepingHead = Position + PixelOffset.FromWhole(0, change);
        PixelPosition[] candidates = IsOnGround && !aroundMiddle ? [keepingFeet] : [Position, keepingFeet, keepingHead];
        foreach (PixelPosition candidate in candidates)
        {
            if (change <= 0 || Room.Mover.IsFree(BodyRectFor(form).At(candidate), this, BodyMoves))
                return candidate;
        }

        return null;
    }

    /// <summary>
    /// Changes the player's own speed across for one update on the ground by <paramref name="moves"/>. Steered, the
    /// speed gains, and the top speed replaces a speed that reads as past it. Unsteered or reversing, it slows, and a
    /// speed slowed past rest is at rest and steered again; one slowed exactly to rest keeps its condition for one
    /// update more. The speed then carries the player the way they travel.
    /// </summary>
    /// <param name="moves">The move values of what the player is doing.</param>
    /// <param name="heading">The way the player is headed when that is not the way they face, as walking backwards.</param>
    public void Steer(MoveValues moves, Facing? heading = null)
    {
        ArgumentNullException.ThrowIfNull(moves);
        Pixels speed = Velocity.X.Abs();
        if (Steering == Steering.Steered)
            speed = Gained(speed, moves, out _);
        else
            speed = SlowedToRest(speed, moves);
        CarryAcross(speed, heading);
    }

    /// <summary>
    /// Changes the player's own speed across for one update in the air by <paramref name="moves"/>. Only a reversal
    /// slows there: steered or unsteered, the speed gains. Returns whether the top speed replaced it.
    /// </summary>
    public bool SteerInAir(MoveValues moves)
    {
        ArgumentNullException.ThrowIfNull(moves);
        Pixels speed = Velocity.X.Abs();
        bool capped = false;
        if (Steering == Steering.Reversing)
            speed = SlowedToRest(speed, moves);
        else
            speed = Gained(speed, moves, out capped);
        CarryAcross(speed, heading: null);
        return capped;
    }

    /// <summary>A speed after one update's gain, the top speed in its place when it reads as past it.</summary>
    private Pixels Gained(Pixels speed, MoveValues moves, out bool capped)
    {
        Pixels faster = speed + moves.Gain;
        capped = Readings.IsPastTop(faster, moves.Top);
        return capped ? moves.Top : faster;
    }

    /// <summary>A speed after one update's slowing; slowed past rest, it is at rest and the player is steered again.</summary>
    private Pixels SlowedToRest(Pixels speed, MoveValues moves)
    {
        Pixels slower = speed - moves.Loss;
        if (!slower.IsNegative)
            return slower;
        Steering = Steering.Steered;
        return Pixels.Zero;
    }

    /// <summary>Sets the player's own speed across to a size, carried the way they travel.</summary>
    private void CarryAcross(Pixels speed, Facing? heading)
    {
        Facing way = heading is { } headed ? (Steering == Steering.Reversing ? headed.Opposite() : headed) : Travel;
        Velocity = Velocity.WithX(way.Toward(speed));
    }

    /// <summary>Brings the player's own speed across to rest and makes them steered. A dash is left as it is.</summary>
    public void ComeToRest()
    {
        Velocity = Velocity.WithX(Pixels.Zero);
        Steering = Steering.Steered;
    }

    /// <summary>Adds the dash's speed into the player's own speed, the way that speed already goes, and leaves no dash.</summary>
    public void FoldDash()
    {
        if (DashSpeed.IsZero)
            return;
        Pixels speed = Velocity.X.Abs() + DashSpeed;
        Velocity = Velocity.WithX(Velocity.X.IsZero ? Travel.Toward(speed) : Velocity.X.IsNegative ? -speed : speed);
        DashSpeed = Pixels.Zero;
    }

    /// <summary>
    /// Turns the player to face the other way. Their dash's speed is added into their own, which goes on carrying
    /// them the old way, reversing, until it is slowed to rest.
    /// </summary>
    public void TurnAround()
    {
        FoldDash();
        Facing = Facing.Opposite();
        Steering = Steering.Reversing;
    }

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
        DashSpeed = Readings.IsAtCap(DashSpeed, cap) ? cap : DashSpeed + Pixels.FromNumber(DashAcceleration);
    }

    /// <summary>Whether the dash action is held.</summary>
    public bool DashIsHeld => DashAction.Length > 0 && _input.IsHeld(new GameAction(DashAction));

    /// <summary>Ends the player's momentum and their dash with it.</summary>
    public void LoseMomentum()
    {
        DashSpeed = Pixels.Zero;
        EndMomentum();
    }

    /// <summary>
    /// Ends the player's momentum and leaves the dash's speed as it is: it still carries them until something that
    /// keeps a dash only with momentum next looks at it.
    /// </summary>
    public void EndMomentum()
    {
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

    /// <summary>Stops the player's movement across, dash and momentum included, and makes them steered.</summary>
    public void StopAcross()
    {
        ComeToRest();
        LoseMomentum();
    }

    /// <summary>
    /// Brings the player's own speed across to rest, makes them steered and leaves no dash speed. Their momentum is
    /// left as it is, for whatever ends it.
    /// </summary>
    public void ClearSpeedAcross()
    {
        ComeToRest();
        DashSpeed = Pixels.Zero;
    }

    /// <summary>
    /// How far the player moves across this update: their speed, and their dash on top of it in the way they are
    /// going, which at rest is the way they travel.
    /// </summary>
    public Pixels SpeedAcross
    {
        get
        {
            if (DashSpeed.IsZero)
                return Velocity.X;
            bool leftward = Velocity.X.IsZero ? Travel == Facing.Left : Velocity.X.IsNegative;
            return Velocity.X + (leftward ? -DashSpeed : DashSpeed);
        }
    }

    /// <summary>
    /// Adds the medium's gravity to the player's speed: to a fall, up to the fastest they fall; to a rise, however far
    /// it takes it, which leaves a rise slowed to rest or past it for the state to turn into a fall; and to the speed
    /// of a player adrift as to a rise.
    /// </summary>
    public void Fall()
    {
        PlayerMedium medium = Medium;
        Pixels down = IsRising || IsAdrift ? _velocity.Y + Pixels.FromNumber(medium.Gravity) : AfterGravity(_velocity.Y, medium);
        _velocity = _velocity.WithY(down);
    }

    /// <summary>
    /// The player's speed downward after one update of <paramref name="medium"/>'s gravity: how it gains, and where
    /// the gain stops, is the game's to say.
    /// </summary>
    protected abstract Pixels AfterGravity(Pixels speed, PlayerMedium medium);

    /// <summary>
    /// The upward speed a jump from the ground, or off a wall, starts with now, for a body launched in
    /// <paramref name="form"/>. It is that of the medium the lowest row of that body lies in, about the player's
    /// middle, asked afresh: the body a jump is made in is not as tall as the one that stood, so its feet can be clear
    /// of a shallow liquid the standing feet were in, and the jump is then one through air.
    /// </summary>
    public virtual Pixels LaunchSpeed(bool offWall, string form)
    {
        PlayerMedium medium = MediumAt(new PixelPosition(Position.X, Pixels.FromWhole(Position.Y.Whole + HalfHeightOf(form) - 1)));
        double speed = offWall
            ? (JumpsAreBoosted ? medium.BoostedWallJumpSpeed : medium.WallJumpSpeed)
            : (JumpsAreBoosted ? medium.BoostedJumpSpeed : medium.JumpSpeed);
        return Pixels.FromNumber(speed);
    }

    /// <summary>
    /// Moves the player across by their speed and dash. On the ground they climb slopes, which take their share of the
    /// move; the state they are in then presses them to the ground (<see cref="PressToGround"/>). Meeting a wall stops
    /// them and ends their momentum.
    /// </summary>
    /// <param name="byDashAlone">Whether only a dash's speed carries the player this update, their own speed left as it is.</param>
    public MoveResult MoveAcross(bool byDashAlone = false)
    {
        MoveResult result = MoveAcrossBy(Slowed(InLiquid(byDashAlone ? Travel.Toward(DashSpeed) : SpeedAcross)));
        if (result.Blocked)
            StopAcross();
        return result;
    }

    /// <summary>
    /// Moves the player across by a distance, leftward when it is negative, whatever their speed: every move of
    /// their own across is one of these. Slopes ahead take their share of it, an active body power clears the way
    /// first, and the body moves as the game has it move. A state that moves the player by a speed of its own, and
    /// not their speed across, moves them so.
    /// </summary>
    public MoveResult MoveAcrossBy(Pixels reach)
    {
        _reachAcross = reach.Abs();

        // What a block that carries gave the player this update is part of this move, and is taken once.
        Pixels carried = _blockMove.X;
        _blockMove = _blockMove with { X = Pixels.Zero };
        Pixels distance = OverSlopes(reach) + carried;
        distance = distance.WithWhole(Math.Clamp(distance.Whole, -MostWholeAcross, MostWholeAcross));
        ClearWay(distance, Pixels.Zero);

        // The slopes take their share of how far the player goes, not of how far ahead the move is tried. A move of
        // nothing is tried the way their speed would carry them.
        MoveResult result = MoveBodyAcross(distance, BodyMoves, triedAs: reach + carried, stillToward: Travel);
        if (result.PutOnSlope)
            _putOnSlope = true;
        OnBodyMoved(across: true, distance, result.Moved);
        return result;
    }

    /// <summary>
    /// What is given the player to move by on this update, over and above their own movement: by the blocks they
    /// stood in as it began (across by one that carries, down by one that pulls), and by whatever outside them moves
    /// them before their own move (<see cref="MoveFromOutside"/>), each adding to it. Each part is made part of the
    /// player's own move that way as it is made, once; what their state made no move of is moved by itself once they
    /// have moved.
    /// </summary>
    private PixelOffset _blockMove;

    /// <summary>
    /// The blocks the player was in as this update began (<see cref="BlocksStoodIn"/>), by the body their form had
    /// just given them and before they or anything else moved: it is these that carry, pull and hurt them on this
    /// update, wherever their move then takes them.
    /// </summary>
    private List<Block> _stoodIn = [];

    /// <summary>
    /// Takes what a block that pulls gave the player to go down by this update, to be part of the move being made:
    /// the player's own moves down take it themselves, and a state that moves the player up or down by a distance of
    /// its own adds it to that distance.
    /// </summary>
    public Pixels TakeBlockPull()
    {
        Pixels pull = _blockMove.Y;
        _blockMove = _blockMove with { Y = Pixels.Zero };
        return pull;
    }

    /// <summary>
    /// Called as one of the player's own shots ends where it is, leaving what its ending leaves: stopped by terrain,
    /// spent on what it struck, or run out. One that only leaves the room, or is spent as a dud, is not told. A game
    /// in which that changes something of the player says so here.
    /// </summary>
    protected internal virtual void OnShotEnded(Projectile shot)
    {
    }

    /// <summary>
    /// Called as a shot of <paramref name="weapon"/> has been launched from the player's weapons, with the
    /// projectiles it launched, which hold their places by now (<see cref="Weapons.PlaceOf"/>) and have not yet had a
    /// turn. A game in which firing changes something of the player, or of the shot as it starts, says so here.
    /// </summary>
    protected internal virtual void OnFired(WeaponStats weapon, IReadOnlyList<Projectile> shots)
    {
    }

    /// <summary>
    /// Called as the room the player is in is left, before they are taken out of it: their shots are still in it,
    /// and go with it. A game in which something of them outlasts the room says so here.
    /// </summary>
    protected internal virtual void OnLeavingRoom(Room room)
    {
    }

    /// <summary>
    /// Called each time the player's own movement has moved their body along one axis: how far it set out to go,
    /// negative leftward or upward, and how far it went. A game that keeps a record of her moves keeps it here.
    /// </summary>
    protected virtual void OnBodyMoved(bool across, Pixels asked, Pixels moved)
    {
    }

    /// <summary>
    /// The speed a shot of <paramref name="weapon"/> leaves at, fired the way given (-1, 0 or 1 across and down) with
    /// <paramref name="own"/> as its own speed, for a weapon whose shots inherit the player's movement: what a shot
    /// inherits is the game's rule.
    /// </summary>
    public abstract PixelVelocity ShotVelocity(WeaponStats weapon, PixelVelocity own, int across, int down);

    /// <summary>Moves the player down by their speed, upward when it is negative.</summary>
    public MoveResult MoveDown()
    {
        Pixels distance = InLiquid(Velocity.Y) + TakeBlockPull();
        MoveResult result = MoveDownBy(distance, clearsWay: true);
        if (result.Blocked)
            Velocity = Velocity.WithY(Pixels.Zero);
        return result;
    }

    /// <summary>
    /// Moves the player down by a distance, upward when it is negative, whatever their speed: every move of their
    /// own up or down is one of these. A move of some distance that no solid stopped ends the hold sloped ground had
    /// taken of them.
    /// </summary>
    /// <param name="distance">How far, negative upward.</param>
    /// <param name="clearsWay">Whether an active body power first clears the way, as a move by the player's own speed does.</param>
    public MoveResult MoveDownBy(Pixels distance, bool clearsWay = false)
    {
        // A block that stops outright the bearer of an active body power is not entered by a move down: the move goes
        // nowhere, and is stopped as by ground.
        if (distance > Pixels.Zero && IsStoppedOutrightBelow(distance))
        {
            (_groundStoppedMove, _stoppedOnTile) = (true, null);
            OnBodyMoved(across: false, distance, Pixels.Zero);
            return new MoveResult(Pixels.Zero, true, true, null);
        }

        if (clearsWay)
            ClearWay(Pixels.Zero, distance);
        MoveResult result = MoveBodyDown(distance, BodyMoves);
        if (!distance.IsZero && result.BlockedBy is null)
            _putOnSlope = false;
        if (result.PutOnSlope)
            _putOnSlope = true;
        if (result.BlockedByTerrain && !distance.IsNegative)
            (_groundStoppedMove, _stoppedOnTile) = (true, result.StoppedByTile);
        OnBodyMoved(across: false, distance, result.Moved);
        return result;
    }

    /// <summary>How far the player last set out to move across, before any slope took its share.</summary>
    private Pixels _reachAcross;

    /// <summary>
    /// Whether a move across has put the player where sloped ground holds them under their middle, and no move up or
    /// down has been made since. Only a body whose moves are tried where they end is ever put so.
    /// </summary>
    private bool _putOnSlope;

    /// <summary>
    /// Presses a player on the ground down onto it once they have moved across: down by as far as they set out to move
    /// across this update and one pixel more, stopping at ground. Returns whether ground stopped them; when it did not
    /// they have walked off it, and are already that far down. A block that pulls takes the place of the press: the
    /// player goes down by what it gave them this update as one standing still does (<see cref="StandingSink"/>),
    /// and is on the ground if that was stopped or the block holds them up. A player standing in something that holds
    /// them up without being solid otherwise stays as they are.
    /// </summary>
    public bool PressToGround()
    {
        if (!_blockMove.Y.IsZero)
        {
            Pixels given = TakeBlockPull();

            // Taken upward, they go up by as much and stand as they stood: nothing is felt for beneath them, so one
            // who rides something rising does not walk off it.
            if (given.IsNegative)
            {
                MoveDownBy(given);
                return true;
            }

            return MoveDownBy(StandingSink(given)).Blocked | StandsInBlockThatHoldsUp();
        }
        if (StandsInBlockThatHoldsUp())
            return true;

        // One whom sloped ground has just taken hold of is pressed down one pixel only.
        Pixels distance = _putOnSlope ? Pixels.FromWhole(1) : _reachAcross + Pixels.FromWhole(1);
        return MoveDownBy(distance).Blocked;
    }

    /// <summary>
    /// A distance across after the slopes ahead have taken their share of it. A player with no speed up or down, about
    /// to carry the leading edge of their body into sloped ground, moves only the share of the distance each such
    /// slope lets through, counted as the slopes count it: in 256ths of a pixel, the finer part dropped. Every move
    /// of the player's own across takes it (<see cref="MoveAcrossBy"/>), and so does one a game's player makes
    /// besides, by the body they have as it is made.
    /// </summary>
    protected Pixels OverSlopes(Pixels distance)
    {
        if (distance.IsZero || !Velocity.Y.IsZero)
            return distance;
        PixelRect body = BodyRect;
        int target = (Position.X + distance).Whole;
        int leading = distance.IsNegative ? target - HalfWidth : target + HalfWidth - 1;
        foreach (int share in Room.Terrain.GroundSharesIn(leading, body.Top, body.Bottom))
        {
            long parts = ((long)distance.Whole * 256 + distance.Fraction / 256) * share;
            long whole = Math.DivRem(parts, Pixels.FractionsPerPixel, out long fraction);
            if (fraction < 0)
            {
                whole--;
                fraction += Pixels.FractionsPerPixel;
            }

            distance = Pixels.FromParts((int)whole, (int)fraction);
        }

        return distance;
    }

    /// <summary>
    /// Moves the player down by the speed they have, upward when it is negative, and lets gravity act on that speed
    /// for the next update. Stopped by a floor or a ceiling, their speed up and down is at rest.
    /// </summary>
    /// <param name="keepsSpeedWhenStopped">Whether a floor or ceiling that stops the move leaves the speed as gravity left it.</param>
    public MoveResult MoveDownUnderGravity(bool keepsSpeedWhenStopped = false)
    {
        Pixels distance = InLiquid(Velocity.Y) + TakeBlockPull();
        Fall();
        MoveResult result = MoveDownBy(distance, clearsWay: true);
        if (result.Blocked && !keepsSpeedWhenStopped)
            FallFromRest();
        return result;
    }

    /// <summary>
    /// Steers the player for one update in the air by <paramref name="moves"/> and moves them across. Steered with
    /// neither direction held, they stop across at once and do not move across this update: no move across is made
    /// at all, so whatever they were given to move by across (<see cref="MoveFromOutside"/>, a block that carries)
    /// is lost with it. Unsteered or reversing, they carry on. Returns the move, or null when there was none.
    /// </summary>
    public MoveResult? SteerAndMoveInAir(MoveValues moves)
    {
        SteerInAir(moves);
        bool directionHeld = _input.IsHeld(GameAction.Left) || _input.IsHeld(GameAction.Right);
        if (Steering == Steering.Steered && !directionHeld)
        {
            ComeToRest();
            _blockMove = _blockMove with { X = Pixels.Zero };
            return null;
        }

        return MoveAcross();
    }

    /// <summary>
    /// Sets the player's steering afresh as they take up a stance in the air: unsteered while a dash still carries
    /// them, and otherwise steered. Their speed is left as it is.
    /// </summary>
    public void SteerAfresh() => Steering = DashSpeed.IsZero ? Steering.Steered : Steering.Unsteered;

    /// <summary>
    /// Whether the jump counts as pressed this update: pressed since the update before, or still held from a little
    /// before a landing or the end of a turn on the last update, where the game gives the player that.
    /// </summary>
    public bool JumpPressed => _input.WasPressed(GameAction.Jump) || _jumpBuffered;

    /// <summary>
    /// Lets a jump already held count as pressed afresh on the next update, if it has not been held too long. Called
    /// as the player lands on their feet and as a turn ends, on the ground or in the air. A chance taken starts the
    /// count of how long the jump has been held again, whether or not the press then does anything.
    /// </summary>
    public void ArmJumpBuffer() => _jumpBufferArmed = true;

    private bool _jumpBufferArmed, _jumpBuffered, _jumpWasHeld;
    private int _jumpHeldFor;

    /// <summary>Works out, at the start of an update, whether a held jump counts as pressed afresh; the chance is then spent.</summary>
    private void TakeJumpBuffer()
    {
        _jumpBuffered = _jumpBufferArmed && _jumpHeldFor != 0 && _jumpHeldFor < JumpBufferUpdates;
        _jumpBufferArmed = false;
        if (_jumpBuffered)
            _jumpHeldFor = 0;
    }

    /// <summary>
    /// Notes what is held on an update the player does not play, as a screen laid over play passes it by: whether
    /// the jump was held is taken from it for their next update of play, and how long it has been held stands as it
    /// was. So a jump held as play is taken up again goes on counting from where it stood, whatever was let go and
    /// pressed meanwhile.
    /// </summary>
    public void NoteHeld(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _jumpWasHeld = input.IsHeld(GameAction.Jump);
    }

    /// <summary>Counts, at the end of an update, how many updates running the jump has been held beyond its first.</summary>
    private void CountJumpHeld()
    {
        bool held = _input.IsHeld(GameAction.Jump);
        _jumpHeldFor = held && _jumpWasHeld ? _jumpHeldFor + 1 : 0;
        _jumpWasHeld = held;
    }

    /// <summary>
    /// Delivers each active body power's hit to the blocks in the way of a move, before the player makes it: a body
    /// that is a weapon breaks what it can as it meets it, and is not stopped by what it breaks.
    /// </summary>
    private void ClearWay(Pixels across, Pixels down)
    {
        if (!_bodyPowers.Any(power => power.IsActive))
            return;
        int stepsAcross = (Position.X + across).Whole - Position.X.Whole, stepsDown = (Position.Y + down).Whole - Position.Y.Whole;
        if (stepsAcross == 0 && stepsDown == 0)
            return;
        PixelRect body = BodyRect;
        var way = new PixelRect(
            stepsAcross < 0 ? body.Left + stepsAcross : stepsAcross > 0 ? body.Right : body.Left,
            stepsDown < 0 ? body.Top + stepsDown : stepsDown > 0 ? body.Bottom : body.Top,
            stepsAcross != 0 ? Math.Abs(stepsAcross) : body.Width,
            stepsDown != 0 ? Math.Abs(stepsDown) : body.Height);
        foreach (BodyPower power in _bodyPowers)
        {
            if (power.IsActive)
                Room.Terrain.TakeHit(power.Hit with { Source = this }, way);
        }
    }

    /// <summary>
    /// Whether a wall lies within <paramref name="reach"/> pixels of the player's body on the given side. Sloped ground
    /// that holds things up by their middle is no wall.
    /// </summary>
    public bool IsWithinReachOfWall(Facing side, int reach)
    {
        PixelRect body = BodyRect;
        var beside = side == Facing.Left
            ? new PixelRect(body.Left - reach, body.Top, reach, body.Height)
            : new PixelRect(body.Right, body.Top, reach, body.Height);
        return !Room.Mover.IsFree(beside, this, MoveOptions.Plain with { Slopes = SlopeTreatment.IgnoreMiddleHeld });
    }

    /// <summary>Whether the input holds anything that aims the weapon. A game with aiming actions of its own counts them here.</summary>
    public virtual bool AimIsHeld => _input.IsHeld(GameAction.Up) || _input.IsHeld(GameAction.Down);

    /// <summary>Sets the player's aim from what is held, by the game's own rule for it.</summary>
    public abstract void AimFromInput();

    /// <summary>The state machine, once the player has entered a room.</summary>
    private StateMachine<Player> States => _states ?? throw new InvalidOperationException("The player is not in a room.");

    /// <summary>How far the body reaches above and below its middle in a form.</summary>
    public int HalfHeightOf(string form) =>
        Forms.TryGetValue(form, out int halfHeight) ? halfHeight : throw new InvalidOperationException($"The player has no form named \"{form}\".");

    /// <summary>The body's rectangle in a form, from the player's place at its middle.</summary>
    private PixelRect BodyRectFor(string form) => new(-HalfWidth, -HalfHeightOf(form), HalfWidth * 2, HalfHeightOf(form) * 2);

    /// <summary>
    /// Takes a form. The body and the hurtbox keep the shape they had for the rest of the update: the form's own
    /// shape is theirs from the player's next update on (<see cref="PublishForm"/>), so whatever meets the player
    /// before then meets the body they had. The medium they move in is asked afresh, though: it goes by the feet of
    /// the form they have as it is asked, and those have moved.
    /// </summary>
    private void ApplyForm(string form)
    {
        Form = form;
        _medium = null;
    }

    /// <summary>Puts the body and the hurtbox in the shape of the form the player is in.</summary>
    private void PublishForm()
    {
        Body.Rect = BodyRectFor(Form);
        Hurtbox.Rect = BodyRectFor(Form);
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
        return liquid is not null && IsIn(liquid, point) && !Inventory.IsAnyOn(liquid.Type.SlowingCancelledBy) ? liquid : null;
    }

    /// <summary>Works out the medium the player is in.</summary>
    private PlayerMedium FindMedium() => MediumAt(Feet);

    /// <summary>The medium a point of the player is in: the liquid it is under, when that hinders them and they have a medium of its name, and otherwise air.</summary>
    private PlayerMedium MediumAt(PixelPosition point)
    {
        Liquid? liquid = HinderingLiquid(point);
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

    /// <summary>
    /// A distance across moved this update, halved as many times as the player is slowed. Their own move across is
    /// (<see cref="MoveAcross"/>); a state that moves them across by a speed of its own asks it of that speed.
    /// </summary>
    public Pixels Slowed(Pixels distance)
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
            if (!power.IsActive)
                continue;
            HitResult result = toucher.TakeHit(power.Hit with { Source = this }, hurtbox);
            OnBodyPowerLanded(power, toucher, result);
            if (result is HitResult.Hurt or HitResult.Destroyed)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Delivers each active body power's hit to whatever the body touches that is no enemy. An enemy is struck in its
    /// own turn, as its touch is tried against the player (<see cref="BodyPowerStops"/>), and nowhere else: one the
    /// player's move has just brought them against is struck on the next update, when its turn comes.
    /// </summary>
    private void DeliverBodyPowers()
    {
        foreach (BodyPower power in _bodyPowers)
        {
            if (!power.IsActive)
                continue;
            PixelRect body = BodyRect;
            var reach = new PixelRect(body.Left - 1, body.Top - 1, body.Width + 2, body.Height + 2);
            Hit hit = power.Hit with { Source = this };
            foreach ((IHittable target, HitResult result) in Room.DeliverHit(hit, reach, from: this, landsOn: target => target is not Enemy))
                OnBodyPowerLanded(power, target, result);

            Room.Terrain.TakeHit(hit, reach);
        }
    }

    /// <summary>Called each time an active body power's hit has been delivered to something, with what became of it. The engine's player does nothing.</summary>
    /// <param name="power">The power whose hit it was.</param>
    /// <param name="target">What it was delivered to.</param>
    /// <param name="result">What became of the hit.</param>
    protected virtual void OnBodyPowerLanded(BodyPower power, IHittable target, HitResult result)
    {
    }

    /// <summary>
    /// Moves the player by whatever the blocks gave them to move by this update that their state made no move of: a
    /// state that moved them neither across nor down is carried and pulled all the same, by a move of its own, and one
    /// with no speed up or down is pulled as one standing still is.
    /// </summary>
    private void MoveByWhatBlocksLeft()
    {
        if (!_blockMove.X.IsZero)
        {
            Pixels across = _blockMove.X;
            _blockMove = _blockMove with { X = Pixels.Zero };
            OnBodyMoved(across: true, across, MoveBodyAcross(across, BodyMoves).Moved);
        }

        if (!_blockMove.Y.IsZero)
        {
            Pixels pull = TakeBlockPull();
            Pixels down = Velocity.Y.IsZero ? StandingSink(pull) : pull;
            OnBodyMoved(across: false, down, MoveBodyDown(down, BodyMoves).Moved);
        }
    }

    /// <summary>
    /// Notes what the blocks the player is in give them to move by on this update (<see cref="_blockMove"/>): one
    /// that carries, across, and one that pulls, down; of several, the one that moves them furthest. What such a
    /// block does to their speed and their momentum it does now.
    /// </summary>
    private void NoteBlocksStoodIn()
    {
        Pixels across = Pixels.Zero, down = Pixels.Zero;
        foreach (Block block in _stoodIn)
        {
            Pixels speed = block.Type.ContactSpeed;
            if (block.Type.Contact == BlockContact.Carries)
            {
                // Level means slower than a pixel an update up or down, whichever way: a slow rise is still carried.
                if (block.Type.CarriesOnlyLevel && Velocity.Y.Abs().Whole != 0)
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
                if ((sand.HoldsUp || sand.StopsFall) && Velocity.Y > Pixels.Zero)
                    Velocity = Velocity.WithY(Pixels.Zero);
                Pixels pull = moving
                    ? (eased ? sand.Eased.MovingContactSpeed : sand.MovingContactSpeed)
                    : (eased ? sand.Eased.ContactSpeed : speed);
                if (pull > down)
                    down = pull;
            }
        }

        _blockMove = new PixelOffset(across, down);
    }

    /// <summary>
    /// Whether a move down by <paramref name="distance"/> would leave the bottom row of the player's body in a block
    /// that stops outright the bearer of a body power they have active (<see cref="BlockType.StopsBodyPowers"/>).
    /// </summary>
    private bool IsStoppedOutrightBelow(Pixels distance)
    {
        if (!_bodyPowers.Any(power => power.IsActive))
            return false;
        PixelRect body = BodyRect;
        int row = body.Bottom - 1 + ((Position.Y + distance).Whole - Position.Y.Whole);
        foreach (Block under in Room.Terrain.BlocksIn(new PixelRect(body.Left, row, body.Width, 1)))
        {
            if (_bodyPowers.Any(power => power.IsActive && under.Type.StopsBodyPowers.Contains(power.Name)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// How far a player standing still is taken down by a block's pull on one update: the pull and one pixel more, as a
    /// standing player is pressed to the ground by one pixel more than they go; held to the limit of whatever block
    /// the bottom row of their body would then lie in, where one gives a limit (<see cref="BlockType.StandingLimit"/>),
    /// as sand's surface lets one who stands on it sink only a little at a time. An upward pull is as it is.
    /// </summary>
    private Pixels StandingSink(Pixels pull)
    {
        if (pull.IsNegative)
            return pull;
        Pixels pressed = pull + Pixels.FromWhole(1);
        PixelRect body = BodyRect;
        // The bottom row of the body they have, as far down as the press would carry it in whole pixels.
        int row = body.Bottom - 1 + ((Position.Y + pressed).Whole - Position.Y.Whole);
        foreach (Block under in Room.Terrain.BlocksIn(new PixelRect(body.Left, row, body.Width, 1)))
        {
            Pixels limit = under.Type.StandingLimit;
            if (under.Type.Contact == BlockContact.PullsDown && limit > Pixels.Zero && pressed > limit)
                pressed = limit;
        }

        return pressed;
    }

    /// <summary>
    /// Takes this update's ongoing damage: what the room does where the player is, and what blocks they touch do, less
    /// whatever their equipment cancels. Parts of a point carry over to later updates. The room's liquid harms them
    /// by where their feet are: while the lowest row of their body is under its surface. A block that hurts whoever
    /// is in it hurts by where they stood as the update began; a solid one, by having stopped a move of theirs on it.
    /// </summary>
    private void TakeOngoingDamage()
    {
        int hundredths = _addedOngoing;
        _addedOngoing = 0;
        int fromRoom = 0;
        foreach ((string source, int amount) in Room.OngoingDamageAt(Feet))
        {
            if (Room.Liquid is { } liquid && source == liquid.Type.Name && Inventory.IsAnyOn(liquid.Type.DamageCancelledBy))
                continue;
            if (IsCancelled(source))
                continue;
            fromRoom += amount;
            if (amount > 0)
                OnRoomDamage(source);
        }

        hundredths += fromRoom;

        int blockHit = 0;
        // The blocks they were in as the update began, and the solid ones that stopped a move of theirs on it.
        foreach (Block block in _stoodIn.Concat(BlocksMet()).Distinct())
        {
            if (block.Type.Contact != BlockContact.Hurts || IsCancelled("blocks"))
                continue;
            hundredths += block.Type.ContactDamageHundredths;
            blockHit = Math.Max(blockHit, block.Type.ContactHit);
        }

        // A block that hits, as a spike does, lands the hardest hit of those touched.
        if (blockHit > 0)
            TakeHit(new Hit(blockHit, HitSide.Neutral, HitKind.Hazard), Hurtbox);

        int points = OngoingPointsTaken(hundredths);
        if (points > 0)
            Inventory.Lose(Inventory.Energy, points);
    }

    /// <summary>
    /// How many whole points this update's ongoing damage takes, of the hundredths of a point that reached the
    /// player. The game says: what it keeps of a part of a point, what its equipment lessens, and when a point is taken.
    /// </summary>
    protected abstract int OngoingPointsTaken(int hundredths);

    /// <summary>
    /// Called on each update for each thing of the room itself that does the player ongoing damage, its heat or its
    /// liquid, in the order the room names them. The engine's player does nothing.
    /// </summary>
    /// <param name="source">What does the damage, by the name it is cancelled by.</param>
    protected virtual void OnRoomDamage(string source)
    {
    }

    /// <summary>Whether switched-on equipment cancels ongoing damage from a source.</summary>
    private bool IsCancelled(string source) =>
        _ongoingCancelledBy.TryGetValue(source, out IReadOnlyList<string>? equipment) && Inventory.IsAnyOn(equipment);

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);
}
