using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// This game's player: the engine's player controller plus what is hers. Her morph ball is the engine's low form,
/// which the morph ball switches on and the spring ball lets jump; wall jumping, crouching and the dash are the
/// engine's, switched on for her in her scene. Hers alone are the item she has selected, her suits and reserve tanks,
/// the speed booster's boost and stored shine, and her own states.
/// </summary>
public partial class Samus : Player
{
    /// <summary>The name of the form her body takes in a spin jump.</summary>
    public const string Spinning = "spinning";

    /// <summary>The data files of her weapons.</summary>
    [Export, DataValue("weapons")]
    public string[] WeaponFiles { get; set; } = [];

    /// <summary>With the Varia suit on, a hit does its damage over this many.</summary>
    [Export, DataValue("varia_damage_over")]
    public int VariaDamageOver { get; set; } = 2;

    /// <summary>With the Gravity suit on, a hit does its damage over this many.</summary>
    [Export, DataValue("gravity_damage_over")]
    public int GravityDamageOver { get; set; } = 4;

    /// <summary>The energy at or below which the warning sounds.</summary>
    [Export, DataValue("low_energy")]
    public int LowEnergy { get; set; } = 30;

    /// <summary>How many updates each picture of her running stride lasts, from its first to its last.</summary>
    [Export, DataValue("run_stride_updates")]
    public int[] RunStrideUpdates { get; set; } = [];

    /// <summary>How many updates each picture of the stride lasts while a dash carries her, with the speed booster off.</summary>
    [Export, DataValue("dash_stride_updates")]
    public int[] DashStrideUpdates { get; set; } = [];

    /// <summary>
    /// How many updates each picture of the stride lasts while the speed booster's dash carries her, at each stage of
    /// its boost: a stride's worth for the first stage, then the next stage's, to the last, at which she is boosting.
    /// </summary>
    [Export, DataValue("boost_stride_updates")]
    public int[] BoostStrideUpdates { get; set; } = [];

    /// <summary>How many strides she must complete, dashing, at each stage of the boost before the stage is done.</summary>
    [Export, DataValue("boost_stride_counts")]
    public int[] BoostStrideCounts { get; set; } = [];

    /// <summary>How many updates a stored shine lasts.</summary>
    [Export, DataValue("stored_shine_updates")]
    public int StoredShineUpdates { get; set; } = 180;

    /// <summary>How many updates after a shine is stored its sound is heard.</summary>
    [Export, DataValue("stored_shine_sound_updates")]
    public int StoredShineSoundUpdates { get; set; } = 10;

    /// <summary>The count of a hurt on which it is heard.</summary>
    [Export, DataValue("hurt_sound_count")]
    public int HurtSoundCount { get; set; } = 2;

    /// <summary>The count a hurt reaches on which the sound it cut short is asked for again.</summary>
    [Export, DataValue("hurt_recovery_count")]
    public int HurtRecoveryCount { get; set; } = 40;

    /// <summary>The count a hurt ends on reaching.</summary>
    [Export, DataValue("hurt_end_count")]
    public int HurtEndCount { get; set; } = 60;

    /// <summary>The count a flying shinespark holds a hurt at, on every update it moves her.</summary>
    [Export, DataValue("spark_hurt_count")]
    public int SparkHurtCount { get; set; } = 8;

    /// <summary>How many pixels a shinespark begun out of a jump lifts her as its windup starts.</summary>
    [Export, DataValue("shinespark_lift")]
    public int ShinesparkLift { get; set; } = 1;

    /// <summary>How many updates a shinespark winds up for before it launches straight up by itself.</summary>
    [Export, DataValue("shinespark_windup_updates")]
    public int ShinesparkWindupUpdates { get; set; } = 30;

    /// <summary>The speed across a shinespark launches at, as a number of pixels.</summary>
    [Export, DataValue("shinespark_speed")]
    public double ShinesparkSpeed { get; set; } = 8;

    /// <summary>The most speed a shinespark reaches across; it gains the pull of gravity each update.</summary>
    [Export, DataValue("shinespark_top_speed")]
    public double ShinesparkTopSpeed { get; set; } = 15;

    /// <summary>The upward speed a rising shinespark gains in its first update; the gain itself grows by the pull of gravity each update.</summary>
    [Export, DataValue("shinespark_rise_gain")]
    public double ShinesparkRiseGain { get; set; } = 7;

    /// <summary>The most whole pixels a shinespark rises in one update.</summary>
    [Export, DataValue("shinespark_most_rise")]
    public int ShinesparkMostRise { get; set; } = 14;

    /// <summary>The energy below which a shinespark ends.</summary>
    [Export, DataValue("shinespark_least_energy")]
    public int ShinesparkLeastEnergy { get; set; } = 30;

    /// <summary>
    /// The special attacks, by the one beam she must carry for each: the data file of the shot of its four lights.
    /// </summary>
    [Export, DataValue("special_attacks", ReadByOwner = true)]
    public Godot.Collections.Dictionary<string, string> SpecialAttackShots { get; set; } = [];

    /// <summary>How long holding down can go on winding a bomb spread up.</summary>
    [Export, DataValue("bomb_spread_wind_up_updates")]
    public int BombSpreadWindUpUpdates { get; set; } = 192;

    /// <summary>How many updates of winding a bomb spread up throw it a pixel an update higher.</summary>
    [Export, DataValue("bomb_spread_updates_per_pixel")]
    public int BombSpreadUpdatesPerPixel { get; set; } = 64;

    /// <summary>How long each bomb of a spread lies before it bursts, left to right.</summary>
    [Export, DataValue("bomb_spread_fuses")]
    public int[] BombSpreadFuses { get; set; } = [];

    /// <summary>How each bomb of a spread is thrown, left to right, across and down, in pixels an update.</summary>
    [Export, DataValue("bomb_spread_throws", ReadByOwner = true)]
    public Godot.Collections.Array<Vector2> BombSpreadThrows { get; set; } = [];

    /// <summary>
    /// The special attacks whose last two lights are shots of their own, by the beam: the data file of that shot.
    /// </summary>
    [Export, DataValue("special_attack_trails", ReadByOwner = true)]
    public Godot.Collections.Dictionary<string, string> SpecialAttackTrailShots { get; set; } = [];

    /// <summary>How many updates into a charge its sound begins.</summary>
    [Export, DataValue("charge_sound_updates")]
    public int ChargeSoundUpdates { get; set; } = 16;

    /// <summary>How many updates the attack must be held, with a beam that charges, before a special attack is let loose.</summary>
    [Export, DataValue("special_attack_updates")]
    public int SpecialAttackUpdates { get; set; } = 120;

    /// <summary>The bubble her breath lets out under water, by the data file of its timeline; empty for none.</summary>
    [Export(PropertyHint.File), DataValue("breath_bubble_effect")]
    public string BreathBubbleEffect { get; set; } = "";

    /// <summary>A bubble may rise on every count of the clock that this divides.</summary>
    [Export, DataValue("breath_bubble_every")]
    public int BreathBubbleEvery { get; set; } = 128;

    /// <summary>How many updates a bubble holds the next one off.</summary>
    [Export, DataValue("breath_bubble_hold_updates")]
    public int BreathBubbleHoldUpdates { get; set; } = 1;

    /// <summary>How far above the top of her body her mouth is taken to be, when it is asked whether it is under water.</summary>
    [Export, DataValue("breath_mouth_above_top")]
    public int BreathMouthAboveTop { get; set; } = 24;

    /// <summary>How far below the top of her body a bubble starts.</summary>
    [Export, DataValue("breath_bubble_below_top")]
    public int BreathBubbleBelowTop { get; set; } = 6;

    /// <summary>The two sounds of a bubble, by name; a number drawn picks one.</summary>
    [Export, DataValue("breath_low_sound")]
    public string BreathLowSound { get; set; } = "";

    /// <inheritdoc cref="BreathLowSound"/>
    [Export, DataValue("breath_high_sound")]
    public string BreathHighSound { get; set; } = "";

    /// <summary>The dust a wall jump kicks up as it begins, by the data file of its timeline; empty for none.</summary>
    [Export(PropertyHint.File), DataValue("wall_jump_effect")]
    public string WallJumpEffect { get; set; } = "";

    /// <summary>How many pixels behind her middle a wall jump's dust is left.</summary>
    [Export, DataValue("wall_jump_effect_behind")]
    public int WallJumpEffectBehind { get; set; } = 6;

    /// <summary>The updates left before another bubble may rise.</summary>
    private Countdown _bubbleWait = Countdown.Over;

    /// <summary>The echoes a shinespark throws off as it ends, by the data file of their shot; empty for none.</summary>
    [Export(PropertyHint.File), DataValue("shinespark_echo_shot")]
    public string ShinesparkEchoShot { get; set; } = "";

    /// <summary>The pool of her weapons' places an echo let go takes one of, by name.</summary>
    [Export, DataValue("shinespark_echo_pool")]
    public string ShinesparkEchoPool { get; set; } = "";

    /// <summary>How far a crash's echoes have turned about her, which one crash leaves for the next.</summary>
    public Compatibility.CrashOrbitTravel OrbitTravel { get; } = new();

    /// <summary>How many updates after something of the room itself hurts her, as a spike does, she cannot be hurt again.</summary>
    [Export, DataValue("hazard_invincible_updates")]
    public int HazardInvincibleUpdates { get; set; } = 60;

    /// <summary>The sound of the room's heat or liquid wearing her down, by name; empty for none.</summary>
    [Export, DataValue("room_damage_sound")]
    public string RoomDamageSound { get; set; } = "";

    /// <summary>That sound is heard on every so many updates of the game's clock.</summary>
    [Export, DataValue("room_damage_sound_every")]
    public int RoomDamageSoundEvery { get; set; } = 8;

    /// <summary>The least energy she must have for that sound to be heard.</summary>
    [Export, DataValue("room_damage_sound_least_energy")]
    public int RoomDamageSoundLeastEnergy { get; set; } = 71;

    /// <summary>How many updates such a hurt throws her back for.</summary>
    [Export, DataValue("hazard_knockback_updates")]
    public int HazardKnockbackUpdates { get; set; } = 10;

    /// <summary>How many updates of its fuse a bomb she laid has left when it throws the morph ball lying against it.</summary>
    [Export, DataValue("bomb_jump_fuse_left")]
    public int BombJumpFuseLeft { get; set; } = 8;

    /// <summary>The least falling speed, in whole pixels, at which her landing is a hard one, with its own sound.</summary>
    [Export, DataValue("hard_landing_speed")]
    public int HardLandingSpeed { get; set; } = 5;

    /// <summary>How many updates she is held where a shinespark ended.</summary>
    [Export, DataValue("shinespark_crash_updates")]
    public int ShinesparkCrashUpdates { get; set; } = 30;

    /// <summary>How much energy the reserve tanks give back each update while refilling her.</summary>
    [Export, DataValue("reserve_refill_per_update")]
    public int ReserveRefillPerUpdate { get; set; } = 1;

    /// <summary>The energy below which a crystal flash can start.</summary>
    [Export, DataValue("crystal_flash_energy_below")]
    public int CrystalFlashEnergyBelow { get; set; } = 51;

    /// <summary>How much of each kind of ammunition a crystal flash uses up.</summary>
    [Export, DataValue("crystal_flash_ammunition")]
    public int CrystalFlashAmmunition { get; set; } = 10;

    /// <summary>How much energy each piece of ammunition becomes in a crystal flash.</summary>
    [Export, DataValue("crystal_flash_energy_each")]
    public int CrystalFlashEnergyEach { get; set; } = 50;

    /// <summary>How many updates pass between one piece of ammunition and the next in a crystal flash.</summary>
    [Export, DataValue("crystal_flash_drain_every")]
    public int CrystalFlashDrainEvery { get; set; } = 8;

    /// <summary>How many pixels a jump from a crouch with her arm level lifts her before it begins.</summary>
    [Export, DataValue("crouch_jump_lift")]
    public int CrouchJumpLift { get; set; } = 10;

    /// <summary>How many updates each picture of her settling after landing from a spin or a wall jump is held, in order.</summary>
    [Export, DataValue("spin_landing_pictures")]
    public int[] SpinLandingPictures { get; set; } = [];

    /// <summary>
    /// How many updates each picture of a spin that has found a wall to jump from is held, in order: the spin stays
    /// ready to jump off the wall as long as they last.
    /// </summary>
    [Export, DataValue("wall_jump_ready_pictures")]
    public int[] WallJumpReadyPictures { get; set; } = [];

    /// <summary>How many updates of the charge it takes for a spin jump to hurt what it touches.</summary>
    [Export, DataValue("charged_spin_updates")]
    public int ChargedSpinUpdates { get; set; } = 60;

    /// <summary>
    /// How much faster the X-ray scope's cone opens each update, in 256ths of a turn: it opens by nothing at first,
    /// and each update by this much more than the update before.
    /// </summary>
    [Export, DataValue("xray_widening_step")]
    public double XrayWideningStep { get; set; } = 0.03125;

    /// <summary>
    /// The angles of the X-ray scope from straight up, in 256ths of a turn, at which her look changes: from looking up
    /// to raised, to level, to lowered, to looking down.
    /// </summary>
    [Export, DataValue("xray_aim_bands")]
    public int[] XrayAimBands { get; set; } = [];

    /// <summary>How many updates the X-ray scope takes to open, in which the run action is not looked at.</summary>
    [Export, DataValue("xray_warm_up_updates")]
    public int XrayWarmUpUpdates { get; set; } = 8;

    /// <summary>How many more updates the X-ray scope holds her and the room once the run action is found let go; the update after them lets go.</summary>
    [Export, DataValue("xray_closing_updates")]
    public int XrayClosingUpdates { get; set; } = 2;

    /// <summary>The widest the X-ray scope's cone opens to each side of its middle, in 256ths of a turn.</summary>
    [Export, DataValue("xray_widest_half_width")]
    public int XrayWidestHalfWidth { get; set; } = 10;

    /// <summary>
    /// How many updates collecting a suit holds her for. The cartridge's light gives her control back on its 165th
    /// step: a wait of 2, the light appearing over 16, widening evenly over 24 and by its curve over 23, the suit shown
    /// on one, the light closing over 83 and thinning out over 15, and the step that ends it.
    /// </summary>
    [Export, DataValue("suit_pickup_updates")]
    public int SuitPickupUpdates { get; set; } = 164;

    /// <summary>
    /// On which update of collecting a suit the suit is first seen on her: the cartridge's light shows it on its
    /// 66th step, once it has waited, appeared and widened. Until then she is seen as she was.
    /// </summary>
    [Export, DataValue("suit_shown_on_update")]
    public int SuitShownOnUpdate { get; set; } = 66;

    /// <summary>How far the X-ray scope sees, in pixels.</summary>
    [Export, DataValue("xray_reach")]
    public int XrayReach { get; set; } = 256;

    private readonly Dictionary<string, BodyPower> _powers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> _muzzles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> _grappleOrigins = new(StringComparer.Ordinal);

    /// <summary>Where the flare at the end of her arm cannon lies from her place: by stance, facing and aim, and while she turns by the aim the turn holds.</summary>
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> _flarePlaces = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Vector2I> _flareTurningPlaces = new(StringComparer.Ordinal);

    /// <summary>Where the grapple's flare lies from her place, and so where its line is laid from: by stance, facing and aim.</summary>
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> _grappleFlarePlaces = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _shotHeights = new(StringComparer.Ordinal);
    private Countdown _shine = Countdown.Over;
    private int _boostStage = -1;
    private int _strideAt, _strideLeft, _stridesLeft;
    private bool _ranFromStart;
    private bool _spunAtStart;
    private bool _boostSounding;
    private bool _autoCancel;
    private bool _spreadCharged;
    private int _spreadHeld;
    private bool _ballCharge;
    private bool _aimedDownLastUpdate;
    private bool _woreVaria;
    private bool _woreGravity;
    private bool _warningOfLowEnergy;
    private bool _chargeSounds;
    private string? _liquidLastEntered;
    private string _selectedShown = "";
    private Aim _aimAtUpdateStart;

    /// <summary>The jump from the ground she is in while it gathers itself; none once it has gathered.</summary>
    private JumpingState? _gatheringJump;

    /// <summary>
    /// The names in her shot heights of the heights she fires from while settling from a landing, while a jump
    /// gathers, while she crouches down and stands up, and in the turn a moonwalk makes with the jump.
    /// </summary>
    private const string LandingHeights = "landing", JumpStartHeights = "jump_start",
        CrouchChangeHeights = "changing_to_crouch", StandChangeHeights = "changing_to_stand",
        MoonwalkJumpTurnHeights = "moonwalk_jump_turn";

    /// <summary>
    /// What she has selected to use: the name of an ammunition amount, of the grapple beam or of the X-ray scope, or
    /// empty for the beam alone.
    /// </summary>
    public string Selected { get; private set; } = "";

    /// <summary>Her timelines, by name.</summary>
    public TimelineSet Timelines { get; private set; } = TimelineSet.None;

    /// <summary>Whether the speed booster has brought her to full speed: she is boosting.</summary>
    public bool IsBoosting => BoostStrideCounts.Length > 0 && _boostStage >= FullBoostStage;

    /// <summary>How far through the speed booster's stages she is: 0 before the first is done, up to the last when boosting.</summary>
    public int BoostStage => Math.Max(0, _boostStage);

    /// <summary>The picture of her running stride she is at, counted from nought: the stride the boost is counted by, each picture lasting as her dash and the boost's stage have it.</summary>
    public int StridePicture => _strideAt;

    /// <summary>The last stage of the boost: the one at which she is boosting.</summary>
    private int FullBoostStage => BoostStrideCounts.Length - 1;

    /// <summary>Whether she has a shine stored.</summary>
    public bool HasStoredShine => _shine.IsRunning;

    /// <summary>Whether something that has hold of her keeps her from her scope, as the giant Metroid does while it drains her.</summary>
    public bool ScopeWithheld { get; set; }

    /// <summary>
    /// Whether she was standing, running, crouching or stopped by a wall as her last update ended, and as the one
    /// before it ended: the scope starts only from two such updates together.
    /// </summary>
    private bool _atRestForScope, _atRestForScopeBefore;

    /// <summary>
    /// Whether the X-ray scope starts on this update, asked of a Samus who stands, runs, crouches or is stopped by a
    /// wall: it is the item chosen and the run action is held; she was so on her update before as well; she is not
    /// settling from a landing, nor moving up or down; her grapple beam is put away; nothing that has hold of her
    /// withholds it; and no power bomb's blast is under way.
    /// </summary>
    private bool ScopeStarts(GameInput input) =>
        Selected == Equipment.XrayScope && input.IsHeld(Actions.Run)
        && _atRestForScopeBefore && !ScopeWithheld
        && State is not StandingState { IsSettling: true }
        && Velocity.Y.IsZero
        && Grapple is not { IsOut: true }
        && !Room.Projectiles.OfType<PowerBomb>().Any(bomb => bomb.IsExploding);

    /// <summary>
    /// Whether she reads as holding a shine, as a jump into a windup and her body's flicker test it: a stored shine,
    /// or a crystal flash's colouring still running, which the cartridge keeps in the stored shine's timer.
    /// </summary>
    public bool ReadsAsHoldingShine => HasStoredShine || _crystalFlashColouring != CrystalFlashColouring.None;

    /// <summary>Where a crystal flash's colouring of her is.</summary>
    private enum CrystalFlashColouring
    {
        /// <summary>None is running.</summary>
        None,

        /// <summary>It runs, as it goes on doing when the flash is cut short.</summary>
        Showing,

        /// <summary>The flash is done: it runs until the end of the update after.</summary>
        EndingNext,

        /// <summary>It runs until the end of this update.</summary>
        Ending,
    }

    private CrystalFlashColouring _crystalFlashColouring;

    /// <summary>A crystal flash's colouring of her begins.</summary>
    public void BeginCrystalFlashColouring()
    {
        _crystalFlashColouring = CrystalFlashColouring.Showing;
        (_flashBody, _flashBubble) = (new SamusGlowPlay(Colours.Glow("crystal_flash_body")), new SamusGlowPlay(Colours.Glow("crystal_flash_bubble")));
    }

    /// <summary>The crystal flash is done, and its colouring ends at the end of the update after.</summary>
    public void FinishCrystalFlashColouring() => _crystalFlashColouring = CrystalFlashColouring.EndingNext;

    /// <summary>The power bomb of hers whose blast ended as this update opened; none if none.</summary>
    private PowerBomb? _blastEnded;

    /// <summary>The power bomb kept for the crystal flash its blast started, until the flash's own blast begins.</summary>
    private PowerBomb? _flashBomb;

    /// <summary>
    /// A power bomb of hers has ended its blast as this update opens: a crystal flash may start this update, if she
    /// lies exactly where it lies, and her sounds are then asked for again. The bomb waits to be told. Beneath a
    /// message, on an update she does not play, the flash is offered at once, by what was held on the last update she
    /// played, as the cartridge offers it whatever the game is doing; nothing is asked for again then.
    /// </summary>
    public void PowerBombBlastEnded(PowerBomb bomb)
    {
        ArgumentNullException.ThrowIfNull(bomb);
        if (Room.UpdateIsSuspended)
            OfferCrystalFlash(bomb);
        else
            _blastEnded = bomb;
    }

    /// <summary>
    /// The blast a crystal flash of hers started has ended: her sounds are asked for again, unless it ended beneath a
    /// message, and no flash is offered.
    /// </summary>
    public void CrystalFlashBlastEnded()
    {
        if (!Room.UpdateIsSuspended)
            ResumeSoundsAfterBlast();
    }

    /// <summary>The rise of her crystal flash is done: the flash's own blast begins where she has risen to, and the power bomb's hold is let go.</summary>
    public void BeginCrystalFlashBlast()
    {
        _flashBomb?.BeginFlashBlast(Position);
        _flashBomb = null;
    }

    /// <summary>A crystal flash that ends before its rise is done lets the bomb kept for it go.</summary>
    public void GiveUpCrystalFlashBlast()
    {
        _flashBomb?.LeaveAfterBlast(Room);
        _flashBomb = null;
    }

    /// <summary>As a blast ends, the sounds it turned away are asked for again.</summary>
    private void ResumeSoundsAfterBlast()
    {
        // The blast turned every sound away, so as it ends her spin's sound is asked for again, a wall jump's too, or
        // else a charge's that had begun to sound.
        if (SpinSound is { } spinSound)
            Room.Session.Sounds.Request(spinSound, channel: "samus", priority: SoundPriority.High);
        else if (ChargeIsSounding)
            Room.Session.Sounds.Request(Sounds.ChargingResumed, channel: "weapons", priority: SoundPriority.High);
    }

    /// <summary>
    /// Starts a crystal flash if a power bomb's blast ended where she lies as this update opened, as the cartridge
    /// offers one only then; she may be doing anything. Her sounds are then asked for again, by what she is doing
    /// once the flash has been offered: a flash begun from a spin asks for the charge's sound, not the spin's.
    /// </summary>
    private void StartCrystalFlashIfOffered()
    {
        if (_blastEnded is not { } bomb)
            return;
        _blastEnded = null;
        OfferCrystalFlash(bomb);
        ResumeSoundsAfterBlast();
    }

    /// <summary>
    /// Offers her the crystal flash as <paramref name="bomb"/>'s blast ends: it starts if she lies exactly where the
    /// bomb lies and may start one, and otherwise the bomb leaves.
    /// </summary>
    private void OfferCrystalFlash(PowerBomb bomb)
    {
        PixelPosition at = bomb.Position;
        if (at.X.Whole == Position.X.Whole && at.Y.Whole == Position.Y.Whole && CrystalFlashState.CanStart(this))
        {
            // The bomb is kept, and with it the hold that allows one power bomb at a time, until the flash's own blast begins.
            _flashBomb = bomb;
            ChangeState(new CrystalFlashState());
        }
        else
        {
            bomb.LeaveAfterBlast(Room);
        }
    }

    /// <summary>The body power of the given name, or null when her data gives her none of that name.</summary>
    public BodyPower? Power(string name) => _powers.GetValueOrDefault(name);

    /// <summary>What is left of a knockback she left a damage boost with, which still runs out.</summary>
    private Countdown _knockbackCarried = Countdown.Over;

    /// <summary>
    /// Takes on what is left of the knockback a damage boost was counting as she leaves the boost. It goes on
    /// counting at the end of each of her updates, this one too, and as it runs out her rise is over, whatever she
    /// is doing by then.
    /// </summary>
    public void CarryKnockbackOn(Countdown left) => _knockbackCarried = left;

    /// <summary>Ends what she carries of a knockback she left a damage boost with, as a shinespark's windup ends a knockback.</summary>
    public void EndCarriedKnockback() => _knockbackCarried = Countdown.Over;

    /// <summary>Whether the enemies' shots are being tried against her, all of which land once the trying has begun.</summary>
    private bool _shotsLand;

    /// <summary>The body powers under which no enemy's shot is tried against her. A charged spin is not one of them.</summary>
    private static readonly string[] ShotProofPowers = [HitKinds.SpeedBoost, HitKinds.Shinespark, HitKinds.ScrewAttack];

    /// <summary>
    /// The cartridge looks once at whether she was lately hurt before it tries the enemies' shots against her, and
    /// tries none while her body is a weapon: speed boosting, shinesparking or screw attacking.
    /// </summary>
    protected internal override bool BeginEnemyShots()
    {
        if (!base.BeginEnemyShots() || IsInvincible || ShotProofPowers.Any(name => Power(name) is { IsActive: true }))
            return false;
        _shotsLand = true;
        return true;
    }

    /// <inheritdoc/>
    protected internal override void EndEnemyShots() => _shotsLand = false;

    /// <summary>Every shot that overlaps her on one update lands: the first does not make her shrug off the rest.</summary>
    protected override bool ShrugsOff(Hit hit) => !_shotsLand && !_landsRegardless && base.ShrugsOff(hit);

    private bool _landsRegardless;

    /// <summary>
    /// Lands a hit on her whether or not a hit a moment ago has made her safe: the few things of the cartridge that
    /// test for her themselves and do not ask, as Mother Brain's rings. She is hurt, safe afresh and knocked back as
    /// by any hit.
    /// </summary>
    public HitResult TakeHitRegardless(Hit hit)
    {
        _landsRegardless = true;
        HitResult result = TakeHit(hit, Hurtbox);
        _landsRegardless = false;
        return result;
    }

    /// <summary>How many updates longer a weapon's hit makes an enemy flash than a blow of her body does.</summary>
    private const int WeaponFlashUpdates = 8;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        Timelines = TimelineSet.ReadFrom(data);

        _powers.Clear();
        if (data.Has("body_powers"))
        {
            foreach ((string name, DataNode power) in data.Get("body_powers").Entries())
            {
                // An enemy her body harms makes no cry: the blow has a sound of its own (OnBodyPowerLanded). And it
                // flashes for its own count of updates, without the eight a weapon's hit adds, which its data counts in.
                var made = new BodyPower(name, power.Int("damage"), [.. power.List("kinds").Select(kind => new HitKind(kind.AsText()))])
                {
                    Quiet = true,
                    FlashFewer = WeaponFlashUpdates,
                };
                _powers[name] = made;
                AddBodyPower(made);
            }
        }

        ReadOrigins(data, "muzzles", _muzzles);
        ReadOrigins(data, "grapple_origins", _grappleOrigins);

        _shotHeights.Clear();
        if (data.Has("shot_heights"))
        {
            foreach ((string look, DataNode height) in data.Get("shot_heights").Entries())
                _shotHeights[look] = height.AsInt();
        }

        if (data.Has("grapple"))
            Grapple?.ReadValues(data.Get("grapple"));

        _flarePlaces.Clear();
        _flareTurningPlaces.Clear();
        if (data.Has("charge_flare"))
        {
            DataNode flare = data.Get("charge_flare");
            ReadOrigins(flare, "places", _flarePlaces);
            ReadOrigins(flare, "grapple_places", _grappleFlarePlaces);
            foreach ((string held, DataNode place) in flare.Get("turning_places").Entries())
                _flareTurningPlaces[held] = new Vector2I(place.Int("x"), place.Int("y"));
            Flare?.ReadValues(flare);
        }

        if (data.Has("arm_cannon"))
            GetNodeOrNull<SamusVisual>("Visual")?.ReadArmCannon(data.Get("arm_cannon"));
        if (data.Has("suit_light"))
            SuitLight?.ReadValues(data.Get("suit_light"));

        SpecialAttackShots = [];
        if (data.Has("special_attacks"))
        {
            foreach ((string beam, DataNode shot) in data.Get("special_attacks").Entries())
                SpecialAttackShots[beam] = shot.AsText();
        }

        BombSpreadThrows = [];
        foreach (DataNode throws in data.List("bomb_spread_throws"))
            BombSpreadThrows.Add(new Vector2((float)throws.Distance("across").ToNumber(), (float)throws.Distance("down").ToNumber()));

        SpecialAttackTrailShots = [];
        foreach ((string beam, DataNode shot) in data.Get("special_attack_trails").Entries())
            SpecialAttackTrailShots[beam] = shot.AsText();
    }

    /// <inheritdoc/>
    protected override void OnReady()
    {
        foreach (string file in WeaponFiles)
            Weapons.Add(WeaponStats.Read(Room.Content.ReadData(file)));
        _woreVaria = Inventory.Owns(Equipment.VariaSuit);
        _woreGravity = Inventory.Owns(Equipment.GravitySuit);
        ReadColours();
    }

    /// <inheritdoc/>
    protected override void OnEnteredRoom(Room room)
    {
        GetNodeOrNull<SamusVisual>("Visual")?.Bind(room.Content);

        // Her shots do not come with her into a room, and the count her crash's echoes left goes with them, as does
        // the hold a shot put on her bombs.
        OrbitTravel.Clear();
        _bombsHeldOff = Countdown.Over;
        (_blastEnded, _flashBomb) = (null, null);

        // A room with no liquid forgets the medium she was last in; one with a liquid keeps it, as the cartridge does.
        if (!room.GetChildren().OfType<Liquid>().Any())
            _liquidLastEntered = null;

        // Unless the player has chosen to put items away themselves, a new room finds her with the beam alone
        // selected: the old option of icon cancel, whose one effect this is.
        if (!room.Session.Options.IsOn(Toggles.ManualItemCancel))
            SelectBeam();
    }

    /// <summary>
    /// The medium she was in as her last update ended: that of the liquid her feet were then in, whatever her suit
    /// makes of it, or air. The cartridge remembers it as her animation runs, and her space jump reads it.
    /// </summary>
    public PlayerMedium MediumLastEntered => Mediums.First(medium => medium.MediumName == (_liquidLastEntered ?? PlayerMedium.Air));

    /// <summary>
    /// Out of water in a spin or a wall jump, without the gravity suit, her spin's sound is asked for again, after the
    /// splash's own, as the cartridge asks as she leaves.
    /// </summary>
    protected override void OnSplashed(Liquid liquid, bool entered)
    {
        if (!entered && liquid.Type.Name == WaterName && !Inventory.IsOn(Equipment.GravitySuit)
            && MovedAs is SpinJumpingState or WallJumpingState)
        {
            Room.Session.Sounds.Request(Sounds.SpinJumpResumed, channel: "samus");
        }
    }

    /// <summary>
    /// Her landing leaves its dust and makes its sound, a harder one from a fast fall, on her feet or as a ball. A
    /// landing that is not a hard one and came down at a whole number of pixels an update, none included, does neither.
    /// </summary>
    protected override void OnLanded()
    {
        // Come down aiming straight down she is in the plain landing, which aims level from this update on.
        if (Aim == Aim.Down && State is JumpingState or FallingState)
            Aim = Aim.Forward;

        bool hard = LandedAtLeast(HardLandingSpeed);
        if (!hard && LandingSpeed.Fraction == 0)
            return;
        base.OnLanded();
        Room.Session.Sounds.Request(hard ? Sounds.HardLand : Sounds.Land, channel: "samus");
    }

    /// <summary>A landing out of a spin or a wall jump takes a spin's landing to settle.</summary>
    public override IReadOnlyList<int> SettlingPicturesAfter(PlayerState from) =>
        from is SpinJumpingState or WallJumpingState ? SpinLandingPictures : base.SettlingPicturesAfter(from);

    /// <summary>
    /// With her feet under a liquid that does not hinder her, as with the gravity suit, a picture that follows another
    /// is held no longer at all, while the first picture of a change begun there is held as out of any liquid: the
    /// cartridge gives a pose it begins her slowing for its delay, and takes the delay away on every update she
    /// stands in such a liquid.
    /// </summary>
    public override int PictureHold(bool asChangeBegins) =>
        !asChangeBegins && FeetAreInLiquid && Medium.MediumName == PlayerMedium.Air ? 0 : base.PictureHold(asChangeBegins);

    /// <summary>
    /// How many updates a fired beam, and a fired missile or super missile, keep her own bombs from throwing her: the
    /// update it is fired on and those after it (the cartridge's ProjectileTimingDefinitions).
    /// </summary>
    private const int BeamHoldsBombsOff = 10, MissileHoldsBombsOff = 20;

    /// <summary>What is left of the hold a shot of hers put on her own bombs.</summary>
    private Countdown _bombsHeldOff = Countdown.Over;

    /// <summary>A beam or a missile fired holds her own bombs off her for its count.</summary>
    protected internal override void OnFired(WeaponStats weapon)
    {
        if (weapon.Name == Arsenal.Beam)
            _bombsHeldOff = Countdown.Of(BeamHoldsBombsOff);
        else if (weapon.Name is Arsenal.Missile or Arsenal.SuperMissile)
            _bombsHeldOff = Countdown.Of(MissileHoldsBombsOff);
    }

    /// <summary>
    /// Whether what she is doing is one of the cartridge's movements of the air, which is not whether anything lies
    /// under her: a jump, a spin, a fall, a wall jump, the flight out of a swing on the grapple beam, and the ball off
    /// the ground. Thrown by a bomb she is as she was thrown (<see cref="BombJumpingState.MovesAsAirborne"/>). Thrown
    /// back by a hurt or boosted by one, turning about in the air, changing her posture, on the grapple beam, or in
    /// any of her special moves she is not, wherever she is. What chooses by her movement and not by her place, as
    /// Mother Brain chooses her attacks, asks this.
    /// </summary>
    public bool MovesAsAirborne => State switch
    {
        JumpingState or SpinJumpingState or FallingState or WallJumpingState or GrappleReleasedState => true,
        CrawlingState crawling => crawling.IsAirborne,
        BombJumpingState thrown => thrown.MovesAsAirborne,
        _ => false,
    };

    /// <summary>
    /// Throws her as a bomb of her own lies against her: straight up when the bomb was beneath her middle, and to the
    /// side away from it otherwise. The ball in her control is thrown as it is, and one already being thrown is
    /// thrown afresh. Upright she is thrown in a jump's pose from standing, a wall stop, a run, a crouch, a fall,
    /// walking backwards, a wall jump or the grapple, where the jump's body fits as the throw is taken
    /// (<see cref="BombJumpingState.EntersIn"/>); a jump, a spin, a turn and every other state refuse the throw, an
    /// upright throw under way among them.
    /// </summary>
    public void BombJump(PixelPosition bomb)
    {
        // A shot just fired holds her own bombs off her, and so does her body being a weapon. A hit's throw is not
        // given up for a bomb's.
        if (_bombsHeldOff.IsRunning || Thrown is not null || BodyPowers.Any(power => power.IsActive))
            return;
        int away = Position.X.Whole - bomb.X.Whole;
        if (State is CrawlingState or BombJumpingState { Upright: false })
        {
            Interrupt(new BombJumpingState(away, ballWasAirborne: MovesAsAirborne));
            return;
        }

        bool upright = State is StandingState or WallStoppedState or RunningState or CrouchingState or FallingState
            or MoonwalkingState or WallJumpingState or GrapplingState;
        if (upright)
            Interrupt(new BombJumpingState(away, upright: true));
    }

    /// <summary>
    /// Where a shot starts: from her middle, raised by how high the pose she is in holds her arm, at the place her
    /// arm points for the way she faces and aims.
    /// </summary>
    public override PixelPosition MuzzlePoint
    {
        get
        {
            // A shot leaves her arm from a whole pixel: the fraction of a pixel her own place has is not the shot's.
            // What she lays as a ball is laid where she is.
            // A shot handed out of a moonwalk's turn leaves from the muzzle of the way it is handed.
            PixelPosition at = Origin(_muzzles, way: HandedWay);
            return Form == Crawling ? at : PixelPosition.FromWhole(at.X.Whole, at.Y.Whole);
        }
    }

    /// <summary>
    /// How far, in whole pixels, the view takes her to have moved at most on an update something that holds her has
    /// placed her: a Yapping Maw, Draygon, or the grapple's line as it takes hold of her or stops her at an angle.
    /// </summary>
    public const int PlacedViewCap = 12;

    /// <summary>
    /// Has the view take her to have moved no more than <see cref="PlacedViewCap"/> each way on this update, once
    /// something has put her where she now is: the view follows the placement by that much and its one pixel more.
    /// </summary>
    public void CapViewAfterPlacing()
    {
        Room.Session.Camera.CapFollowUpOrDown(Position.Y, PlacedViewCap);
        Room.Session.Camera.CapFollowAcross(Position.X, PlacedViewCap);
    }

    /// <summary>
    /// Holds the view to following her by <see cref="PlacedViewCap"/> and its one pixel more, each way, once a Yapping
    /// Maw or Draygon has placed her, as the cartridge's cap for those two does: where she stands that far or further
    /// from where the view takes her to have stood, she is taken to have stood that far off, on the side she moved
    /// toward, past her and not behind her. Only the distance reaches the view, which goes the way she went; the
    /// side decides which of the two parts of a pixel is taken from the other. A view that has tracked nothing since
    /// it was placed is left as it is.
    /// </summary>
    public static void CapViewOfHeld(Player held)
    {
        ArgumentNullException.ThrowIfNull(held);
        Camera view = held.Room.Session.Camera;
        if (view.PlayerTakenToHaveStoodAt is not { } stood)
            return;
        int down = held.Position.Y.Whole - stood.Y, across = held.Position.X.Whole - stood.X;
        if (Math.Abs(down) >= PlacedViewCap)
            view.TakePlayerToHaveStoodAtY(held.Position.Y.Whole + Math.Sign(down) * PlacedViewCap);
        if (Math.Abs(across) >= PlacedViewCap)
            view.TakePlayerToHaveStoodAtX(held.Position.X.Whole + Math.Sign(across) * PlacedViewCap);
    }

    /// <summary>Whether a charge has been held long enough to have begun to sound.</summary>
    public bool ChargeIsSounding => ChargeReaches(ChargeSoundUpdates);

    /// <summary>
    /// Whether a hyper shot's count runs: from the shot until her flare has counted its fast ring of sparks back onto
    /// its first picture (<see cref="SamusFlare"/>), the cartridge counts her as past every charge.
    /// </summary>
    private bool _hyperShotCounts;

    /// <summary>
    /// Whether her beam's charge reads as held for at least <paramref name="updates"/> updates, as every rule of hers
    /// that asks of it reads it: her weapons' charge, or past every count while a hyper shot's count runs. One update
    /// asks whether she holds a charge at all.
    /// </summary>
    public bool ChargeReaches(int updates) => _hyperShotCounts || Weapons.ChargedFor >= updates;

    /// <summary>
    /// Her flare has counted its fast ring of sparks back onto its first picture: a hyper shot's count is over, and a
    /// charge kept through the gift of the hyper beam is gone with it, as the cartridge clears it.
    /// </summary>
    public void FlareCountedBack() => ForgetCharge();

    /// <summary>
    /// Where what holds the first of her shots' places stands, a shot or what a shot left there, and the room's corner
    /// when nothing holds it. The cartridge's Metroids, struck by a shot or a bomb, are pushed away from here, and
    /// not from what struck them.
    /// </summary>
    public PixelPosition FirstShotStands => Weapons.StandsIn(ShotsPool, 0) ?? PixelPosition.FromWhole(0, 0);

    /// <summary>
    /// Where what holds the highest of her shots' places that holds anything stands; null when none does. The
    /// cartridge's pirates watch that one alone, and look no lower when it is far off: a walking pirate counts a shot
    /// and what a shot left, a ninja only a shot still flying (<paramref name="flyingOnly"/>).
    /// </summary>
    public PixelPosition? HighestShotStands(bool flyingOnly)
    {
        string pool = ShotsPool;
        for (int place = Weapons.PlacesIn(pool) - 1; place >= 0; place--)
        {
            if (Weapons.StandsIn(pool, place, flyingOnly) is { } stands)
                return stands;
        }

        return null;
    }

    /// <summary>Empties one of her shots' places, counted from nought: what holds it is gone with no ending of its own.</summary>
    public void ClearShotPlace(int place) => Weapons.ClearPlace(ShotsPool, place, Room);

    /// <summary>The pool her beams, missiles and super missiles share their places in.</summary>
    private string ShotsPool => Weapons.All.First(weapon => weapon.Name == Arsenal.Beam).Pool;

    /// <summary>The view also leads the other way while she moonwalks, and through a damage boost, which the knockback still carries.</summary>
    public override Facing ViewFacing =>
        State is MoonwalkingState or DamageBoostState ? Facing.Opposite() : base.ViewFacing;

    /// <summary>Where her grapple beam starts: as a shot does, at the places her data gives the line.</summary>
    public PixelPosition GrapplePoint => Origin(_grappleOrigins);

    /// <summary>
    /// Where the grapple's line leaves a standing body from its place, for the way she faces and aims, with nothing
    /// taken off for how high her pose holds her: what a line that holds her in place puts her by.
    /// </summary>
    public PixelOffset StandingGrappleOrigin =>
        _grappleOrigins.TryGetValue("standing", out var facings) && facings.TryGetValue(Facing == Facing.Right ? "right" : "left", out var aims) &&
        aims.TryGetValue(AimName(Aim), out Vector2I offset)
            ? PixelOffset.FromWhole(offset.X, offset.Y)
            : throw new InvalidOperationException($"Samus's data gives the grapple no place to leave a standing body aiming {AimName(Aim)}.");

    /// <summary>
    /// The place of <paramref name="origins"/> for how she stands, faces and aims, raised by how high her pose holds her
    /// arm. Her running places hold while she runs, and while she moonwalks aiming diagonally up. Given a
    /// <paramref name="way"/>, the place is that of its facing and aim in place of hers.
    /// </summary>
    private PixelPosition Origin(Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> origins, bool moonwalkRuns = true,
        (Facing Facing, Aim Aim)? way = null)
    {
        // In the morph ball there is no arm to fire from: what she lays, she lays where she is.
        if (Form == Crawling)
            return Position;
        (Facing faces, Aim aims) = way ?? (Facing, Aim);
        string stance = State is RunningState || (moonwalkRuns && State is MoonwalkingState && Aim == Aim.DiagonalUp) ? "running" : "standing";
        string facing = faces == Facing.Right ? "right" : "left";
        if (!origins.TryGetValue(stance, out var facings) || !facings.TryGetValue(facing, out var aimed) ||
            !aimed.TryGetValue(AimName(aims), out Vector2I offset))
            return base.MuzzlePoint;
        return Raised(offset, way?.Aim);
    }

    /// <summary>
    /// Whether her pose holds her arm cannon out, so that what shows at its end shows: standing, running, crouching,
    /// in a plain jump or a fall, moonwalking and turning; not in the ball, a spin or a wall jump, nor while she is
    /// thrown, held, facing forward or doing anything else that keeps her from firing.
    /// </summary>
    public bool HoldsArmOut =>
        Form != Crawling && State is StandingState or RunningState or CrouchingState or JumpingState or FallingState or MoonwalkingState or TurningState or AirTurningState;

    /// <summary>
    /// Where the flare at the end of her arm cannon lies: at the place her data gives it for how she stands, faces
    /// and aims, raised as a shot's start is. Its running places hold only while she runs; while she turns it lies at
    /// the place of the aim the turn holds, the same whichever way she turns.
    /// </summary>
    public PixelPosition FlarePoint
    {
        get
        {
            if (State is not (TurningState or AirTurningState))
                return Origin(_flarePlaces, moonwalkRuns: false);
            string held = Aim switch { Aim.Up or Aim.DiagonalUp => "up", Aim.Forward => "neutral", _ => "down" };
            return _flareTurningPlaces.TryGetValue(held, out Vector2I turning)
                ? Raised(turning)
                : throw new InvalidOperationException($"Samus's data gives the flare no place while she turns aiming {held}.");
        }
    }

    /// <summary>
    /// Where the flare of her grapple beam lies, and its line is laid from. Swinging, it is the point her body hangs
    /// from. Held in place by the line, it is the place her data gives the flare for a standing body, as it stands.
    /// Otherwise, as the line reaches out, it is that place for how she stands, faces and aims, raised as a shot's
    /// start is; the running places hold only while she runs.
    /// </summary>
    public PixelPosition GrappleFlarePoint => State switch
    {
        GrapplingState { RopeStart: { } hangsFrom } => hangsFrom,
        GrappleLockedState => _grappleFlarePlaces.TryGetValue("standing", out var facings) && facings.TryGetValue(Facing == Facing.Right ? "right" : "left", out var aims)
            && aims.TryGetValue(AimName(Aim), out Vector2I standing)
                ? Position + PixelOffset.FromWhole(standing.X, standing.Y)
                : throw new InvalidOperationException($"Samus's data gives the grapple's flare no place on a standing body aiming {AimName(Aim)}."),
        _ => Origin(_grappleFlarePlaces, moonwalkRuns: false),
    };

    /// <summary>
    /// A place her data gives from her own, raised by how high her pose holds her arm; for a shot handed to her turn,
    /// by the height of the <paramref name="handedAim"/> it was handed with, as the cartridge's turn took its pose from it.
    /// </summary>
    private PixelPosition Raised(Vector2I offset, Aim? handedAim = null)
    {
        // Settling from a landing, gathering a jump, crouching down and standing up hold her arm at heights of their
        // own, as the cartridge's poses do, and so does the turn a moonwalk makes with the jump. Coming out of the
        // ball is a change she cannot fire through, and has none.
        string look = State switch
        {
            StandingState { IsSettling: true } => LandingHeights,
            JumpingState jump when ReferenceEquals(jump, _gatheringJump) => JumpStartHeights,
            ChangingPostureState { CanAttack: true, Then: CrouchingState } => CrouchChangeHeights,
            ChangingPostureState { CanAttack: true, Then: StandingState } => StandChangeHeights,
            TurningState turn when ReferenceEquals(turn, _moonwalkJumpTurn) => MoonwalkJumpTurnHeights,
            _ => SamusLooks.Of(this),
        };
        string hold = handedAim is { } handed ? AimName(handed) : HoldName;
        int height = _shotHeights.TryGetValue($"{look}_{hold}", out int aimed) ? aimed : _shotHeights.GetValueOrDefault(look, 0);
        return Position + PixelOffset.FromWhole(offset.X, offset.Y - height);
    }

    /// <summary>Reads a group of places by stance, the way she faces and her aim.</summary>
    private static void ReadOrigins(DataNode data, string group, Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> into)
    {
        into.Clear();
        if (!data.Has(group))
            return;
        foreach ((string stance, DataNode facings) in data.Get(group).Entries())
        {
            into[stance] = new(StringComparer.Ordinal);
            foreach ((string facing, DataNode aims) in facings.Entries())
            {
                into[stance][facing] = new(StringComparer.Ordinal);
                foreach ((string aim, DataNode offset) in aims.Entries())
                    into[stance][facing][aim] = new Vector2I(offset.Int("x"), offset.Int("y"));
            }
        }
    }

    /// <summary>
    /// Sets her aim: the angle actions aim diagonally, and both together straight up on the ground; crouched, the way she
    /// faces held (with no new press of up) aims level, for it stands her up; up aims up, or diagonally up
    /// while she steers, except in a crouch, where it does not aim; down aims down in the air, or diagonally down while she steers. On the ground down aims only
    /// with a direction. Running with a direction held, she aims level or diagonally and never straight up. In a jump
    /// or a fall up and down win over the angle actions, and standing still up does; elsewhere the angle actions win.
    /// In a jump or a fall an aim straight down stays, with nothing held or only the jump, until something else is asked.
    /// </summary>
    public override void AimFromInput()
    {
        bool up = Input.IsHeld(GameAction.Up), down = Input.IsHeld(GameAction.Down);
        bool angleUp = Input.IsHeld(Actions.AngleUp), angleDown = Input.IsHeld(Actions.AngleDown);
        bool steering = Intent != 0;
        bool grounded = IsOnGround;

        // In the air an aim straight down stays until something else is asked: a direction, another aim, or the attack.
        bool staysDown = !grounded && Aim == Aim.Down && State is JumpingState or FallingState
            && !up && !angleUp && !angleDown && !steering && !Input.IsHeld(GameAction.Attack);
        if (staysDown)
            return;
        if (State is RunningState && steering)
            Aim = angleUp ? Aim.DiagonalUp : angleDown ? Aim.DiagonalDown : up ? Aim.DiagonalUp : down ? Aim.DiagonalDown : Aim.Forward;
        else if (State is JumpingState or FallingState)
            // In a jump or a fall up and down are tested before the angle actions, the angle-up action first of those.
            Aim = up ? (steering ? Aim.DiagonalUp : Aim.Up) : down ? (steering ? Aim.DiagonalDown : Aim.Down)
                : angleUp ? Aim.DiagonalUp : angleDown ? Aim.DiagonalDown : Aim.Forward;
        else if (angleUp && angleDown)
            // On the ground both angle actions aim straight up; in the air the angle-up action is tested first.
            Aim = grounded ? Aim.Up : Aim.DiagonalUp;
        else if (State is CrouchingState && !Input.WasPressed(GameAction.Up) && Input.IsHeld(Facing == Facing.Right ? GameAction.Right : GameAction.Left))
            // Crouched, the way she faces held stands her up with her arm level, whatever angle or down is held with it.
            Aim = Aim.Forward;
        else if (up && !steering && State is StandingState or RunningState)
            // Standing, up alone is tested before either angle action.
            Aim = Aim.Up;
        else if (angleUp)
            Aim = Aim.DiagonalUp;
        else if (angleDown)
            Aim = Aim.DiagonalDown;
        else if (up && State is not CrouchingState)
            // Crouched, up alone stands her up with her arm level; she aims up once she has stood.
            Aim = steering ? Aim.DiagonalUp : Aim.Up;
        else if (down && !grounded)
            Aim = steering ? Aim.DiagonalDown : Aim.Down;
        else if (down && steering)
            Aim = Aim.DiagonalDown;
        else
            Aim = Aim.Forward;
    }

    /// <summary>The name of the liquid type that ends a boosted run.</summary>
    private const string LavaName = "lava";

    /// <summary>The name of the liquid she breathes bubbles in.</summary>
    private const string WaterName = "water";

    /// <summary>The name of a room's heat among what does her ongoing damage.</summary>
    private const string HeatName = "heat";

    /// <summary>Up, down and her two angle actions aim.</summary>
    public override bool AimIsHeld => base.AimIsHeld || Input.IsHeld(Actions.AngleUp) || Input.IsHeld(Actions.AngleDown);

    /// <summary>The poses she runs in. Each is the same run; changing from one to another is what moves her a pixel on.</summary>
    private enum RunPose
    {
        /// <summary>Running with her arm at her side.</summary>
        Plain,

        /// <summary>Running with her arm held out, having fired.</summary>
        Firing,

        /// <summary>Running aiming diagonally up.</summary>
        AimedUp,

        /// <summary>Running aiming diagonally down.</summary>
        AimedDown,
    }

    /// <summary>The pose she last ran in.</summary>
    private RunPose _runPose;

    /// <summary>
    /// The pose she runs in next: as she aims; else with her arm out once she has fired, for as long as she runs on
    /// without aiming; a run just begun has not fired yet.
    /// </summary>
    private RunPose NextRunPose(bool alreadyRunning) => Aim switch
    {
        Aim.DiagonalUp => RunPose.AimedUp,
        Aim.DiagonalDown => RunPose.AimedDown,
        _ => alreadyRunning && (_runPose == RunPose.Firing || Input.IsHeld(GameAction.Attack)) ? RunPose.Firing : RunPose.Plain,
    };

    /// <summary>
    /// As the cartridge does, a run about to be taken up in a pose she is not already running in first moves her one
    /// pixel on, the way she was headed: as a run starts, as her running pose changes (her arm coming out or her aim
    /// changing), and on each update she runs on with nothing asked of her. A wall in that pixel stops the run and she
    /// stands against it.
    /// </summary>
    protected override PlayerState Entering(PlayerState from, PlayerState to, bool unasked)
    {
        // Crouching out of a run at full boost stores a shine.
        if (from is RunningState && IsBoosting && to is ChangingPostureState { Then: CrouchingState } or CrouchingState)
            StoreShine();

        // Standing up from a crouch, the cartridge's change of pose tests the ground a little below her feet, among the
        // blocks she stands on, as the standing pose: on an elevator's blocks that is a stand at its door, an update
        // before her first move upright makes one.
        if (from is CrouchingState && to is StandingState or ChangingPostureState { Then: StandingState })
            NoteElevatorStand(new PixelRect(BodyRect.Left, BodyRect.Bottom, BodyRect.Width, 1));

        // A jump from a crouch with her arm level starts lifted off the ground, as the cartridge has it: by the lift,
        // once the taller body of the jump has been fitted. Her arm is as it was before this update's input, as the
        // old pose she jumped from was.
        if (from is CrouchingState && to is JumpingState)
        {
            // The jump's taller body is fitted first, her feet where they were; with no room for it she stays crouched.
            if (!TrySetForm(FormOr(Jumping)))
                return from;

            // Then she is lifted as she is, whatever is above her: a plain change of where she is, which the view
            // follows as it follows a move.
            if (_aimAtUpdateStart == Aim.Forward)
                Position += PixelOffset.FromWhole(0, -CrouchJumpLift);
        }

        // A fresh press of the attack on the update a jump's pose is taken up, or changed, hands the shot to the
        // next update's firing.
        if (to is JumpingState handing && Input.WasPressed(GameAction.Attack) && (!ReferenceEquals(from, to) || handing.TookUpStance))
            _shotHanded = true;

        // Every turn out of a moonwalk hands a shot to the next update's firing, whatever is pressed, to leave the way
        // the moonwalk aimed: the way she faced and her aim as this update began, as the moonwalk's pose had them.
        // The turn the jump makes of it fires that shot from heights of its own.
        if (from is MoonwalkingState moonwalk && to is TurningState turn)
        {
            (_shotHanded, _wayHanded) = (true, (Facing, _aimAtUpdateStart));
            _moonwalkJumpTurn = ReferenceEquals(moonwalk.JumpTurn, turn) ? turn : null;
        }

        if (to is JumpingState jump)
        {
            if (ReferenceEquals(from, to))
            {
                // A stance taken up within a jump spends the shine, and the windup starts lifted.
                if (jump.TookUpStance && SpendsShine(jump.Stance))
                    return new ShinesparkState(lifted: true);
            }
            else if (jump.FromGround)
            {
                Aim = FirstJumpAim(from);
                _gatheringJump = jump;
            }
            else if (from is SpinJumpingState or WallJumpingState or DamageBoostState or AirTurningState && SpendsShine(jump.Stance) &&
                CanTakeForm(FormOr(Jumping), aroundMiddle: true))
            {
                // Straightened out of a spin or a wall jump where the taller body fits, let go of a damage boost, or
                // come out of a turn in the air aiming up.
                return new ShinesparkState(lifted: false);
            }
        }

        if (to is not RunningState)
            return to;
        bool alreadyRunning = from is RunningState;

        // A run taken up afresh starts its stride again.
        if (!alreadyRunning && RunStrideUpdates.Length > 0)
            (_strideAt, _strideLeft) = (0, RunStrideUpdates[0] + PictureHold(asChangeBegins: true));
        RunPose pose = NextRunPose(alreadyRunning);
        bool stepsOn = !alreadyRunning || unasked || pose != _runPose;
        _runPose = pose;
        if (!stepsOn)
            return to;
        Facing headed = from is MoonwalkingState ? Facing.Opposite() : Facing;

        // Stopped by a wall or by something solid, the run does not begin: she is left in the wall-stop pose, aiming
        // as she was. The step is not put on a slope's surface afterwards, as the cartridge's is not.
        PixelPosition stood = Position;
        MoveResult step = MoveBodyAcross(headed.Toward(Pixels.FromWhole(1)), BodyMoves with { Slopes = SlopeTreatment.IgnoreMiddleHeld });
        if (!step.Blocked)
            return to;

        // Something solid found ahead, an enemy frozen or solid, stops the run without moving her at all: the
        // cartridge only feels for it. Terrain's stop leaves her brought up against it.
        if (!step.BlockedByTerrain)
            Position = stood;
        return from is WallStoppedState ? from : new WallStoppedState();
    }

    /// <summary>
    /// The aim of the first pose of a jump from the ground, as the cartridge chooses it on the press: from standing,
    /// up with up held, else diagonally up with the angle-up action held, else diagonally down with the angle-down
    /// action held, else level; from anything else, level.
    /// </summary>
    private Aim FirstJumpAim(PlayerState from)
    {
        if (from is not StandingState)
            return Aim.Forward;
        return Input.IsHeld(GameAction.Up) ? Aim.Up
            : Input.IsHeld(Actions.AngleUp) ? Aim.DiagonalUp
            : Input.IsHeld(Actions.AngleDown) ? Aim.DiagonalDown
            : Aim.Forward;
    }

    /// <summary>A wall she runs into leaves her in the wall-stop pose, whose input is asked in its own order.</summary>
    protected internal override PlayerState StoppedByWall() => new WallStoppedState();

    /// <summary>She has run into a wall once she moves in the wall-stop pose: from the update after the wall stopped her.</summary>
    public override bool MovedAgainstWall => MovedAs is WallStoppedState;

    /// <summary>
    /// Whether taking up <paramref name="stance"/> in a jump spends a stored shine on a shinespark: aiming level with
    /// her arm at rest, aiming up, or aiming diagonally up.
    /// </summary>
    private bool SpendsShine(AirStance stance) =>
        ReadsAsHoldingShine && (stance.Aim is Aim.Up or Aim.DiagonalUp || stance == new AirStance(Aim.Forward));

    /// <summary>The first pose of a jump from the ground spends a stored shine as it is taken up, and the windup starts lifted.</summary>
    protected override PlayerState? JumpGathered(JumpingState jump)
    {
        _gatheringJump = null;
        return SpendsShine(jump.Stance) ? new ShinesparkState(lifted: true) : null;
    }

    /// <summary>Selects one of the things she can select; given nothing, the beam alone.</summary>
    /// <exception cref="ArgumentException">She cannot select it now.</exception>
    public void Select(string item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Length > 0 && !CanSelect(item))
            throw new ArgumentException($"Samus cannot select \"{item}\" now.", nameof(item));
        Selected = item;
    }

    /// <summary>Selects the beam alone.</summary>
    public void SelectBeam()
    {
        Selected = "";
        _autoCancel = false;
    }

    /// <summary>Stores a shine, as crouching at full boost does.</summary>
    public void StoreShine()
    {
        _shine = Countdown.Of(StoredShineUpdates);
        _crystalFlashColouring = CrystalFlashColouring.None;
        _shineGlowPlace = 0;
    }

    /// <summary>
    /// Counts the stored shine's time down, once an update, and asks for its sound a while after it is stored. As on
    /// the cartridge, neither is done while a drained Samus glows, nor on the update a charged shot's or a hyper
    /// shot's glow ends and her suit is shown again.
    /// </summary>
    private void CountStoredShine()
    {
        if (!_shine.IsRunning)
            return;
        if (State is DrainedState { Glows: true } || _shotGlow is { EndsOnNextStep: true })
            return;
        if (_shine.Remaining == StoredShineUpdates - StoredShineSoundUpdates)
            Room.Session.Sounds.Request(Sounds.StoredShine, channel: "samus", priority: SoundPriority.High);
        _shine = _shine.Stepped();
    }

    /// <summary>
    /// The count of the updates of a hurt: none while she is not hurt. A knockback starts it at one, and so does energy
    /// found lost after she is shown while there is none; a flying shinespark holds it, and its windup and its crash
    /// end it.
    /// </summary>
    private int _hurt;

    /// <summary>A flying shinespark holds a hurt at its own count: none is heard while it flies, nor colours her.</summary>
    public void HoldHurtForSpark() => _hurt = SparkHurtCount;

    /// <summary>A shinespark's windup and its crash end a hurt.</summary>
    public void EndHurt() => _hurt = 0;

    /// <summary>
    /// Counts a hurt on, once an update, just before her colours are chosen, and says the count it stood at, which her
    /// colours read for its flash (none with no hurt): on its count of the sound she is heard to be hurt; the count it
    /// reaches for recovery asks again for the sound the hurt cut short; and it ends on reaching its last. While a
    /// suit is being put on it is not counted, as nothing but the suit colours her.
    /// </summary>
    private int CountHurt()
    {
        if (_hurt == 0 || State is CollectingSuitState)
            return 0;
        int before = _hurt;

        // Pinned in a beam, thrown back, her hurt is not heard: it is still counted and colours her. Nor is it heard
        // while a scene shows her, as the opening's flashbacks do.
        if (before == HurtSoundCount && !IsShownByScene && State is not DrainedState { Posed: DrainedState.Pose.Pinned })
            Room.Session.Sounds.Request(Sounds.SamusHurt, channel: "samus");
        int after = ++_hurt;
        if (after == HurtRecoveryCount)
            RecoverSoundAfterHurt();
        if (after >= HurtEndCount)
            _hurt = 0;
        return before;
    }

    /// <summary>
    /// Asks again, at the priority that gives way late, for the sound a hurt cut short: her spin's while she spins, or
    /// else arms the charge's sound to be asked for again where a charge has begun to sound and the attack is held;
    /// with her grapple beam out, the grapple's own sound and nothing else. A wall jump asks for neither: which of the
    /// spin's sounds is its own is not kept.
    /// </summary>
    private void RecoverSoundAfterHurt()
    {
        if (Grapple is { IsOut: true })
        {
            Room.Session.Sounds.Request(Sounds.GrappleLatch, channel: "weapons", priority: SoundPriority.High);
            return;
        }

        if (SpinSound is { } spinSound)
            Room.Session.Sounds.Request(spinSound, channel: "samus", priority: SoundPriority.High);
        else if (ChargeIsSounding && Input.IsHeld(GameAction.Attack))
            _chargingSoundArmed = true;
    }

    /// <summary>
    /// Ends the stored shine, and with it any crystal flash's colouring that read as one: used up by a shinespark or a
    /// crystal flash, or ended as the X-ray's colouring of her takes their place.
    /// </summary>
    public void SpendShine() => (_shine, _crystalFlashColouring) = (Countdown.Over, CrystalFlashColouring.None);

    /// <summary>Whether the spring ball was switched on in the last update she played: what a pause's end compares with.</summary>
    private bool _springBallWasOn;

    /// <summary>Brings her straight to full boost, as a shinespark does.</summary>
    public void BoostFully() => _boostStage = FullBoostStage;

    /// <summary>Ends her boost.</summary>
    public void EndBoost() => _boostStage = -1;

    /// <summary>
    /// What follows, as a pause ends, from what was switched on or off on its pages. Her beams show the colours of
    /// those now switched on. With the speed booster switched on, a run that has its momentum and no boost begun
    /// begins one; with it switched off her boost and her run's momentum end and her echoes are gone at once. With
    /// the spring ball switched either way, the ball is taken to be on the ground where it is.
    /// </summary>
    public void ReconcileAfterPause()
    {
        ShowBeamColours();

        // The spring ball switched on or off puts the ball in its pose of the ground where it is, in the air too,
        // with its speed left as it was: so a ball in the air may then jump as from the ground.
        if (State is CrawlingState ball && Inventory.IsOn(Equipment.SpringBall) != _springBallWasOn)
            ball.TakeAsOnGround();
        if (Inventory.IsOn(Equipment.SpeedBooster))
        {
            if (HasMomentum && _boostStage < 0)
                OnMomentumBegan();
            return;
        }

        // Her run's speed stays until her next move looks at it, as the cartridge's does.
        EndBoost();
        EndMomentum();
        GetNodeOrNull<SamusVisual>("Visual")?.ClearEchoes();
    }

    /// <inheritdoc/>
    protected override void BeforeMoving()
    {
        // Her shots have been fired: the record of her moves that they inherit from starts over before she moves. Not
        // while her reserves refill her or she is dying: the cartridge clears it only where it handles her firing,
        // which it does not then (it does under the X-ray scope), so what her last moves recorded is still there for
        // the first shot afterwards.
        if (State is not (RefillingFromReservesState or DyingState))
            _shotInheritance.Clear();

        // Each update's movement says anew which of her body's powers are at work; they stay so for what touches her
        // afterwards, the enemies of the next update among it. The speed booster's is at work from the update after
        // she reaches full boost.
        foreach (BodyPower power in BodyPowers)
            power.IsActive = false;
        if (Power(HitKinds.SpeedBoost) is { } boost && IsBoosting && State is not ShinesparkState)
            boost.IsActive = true;
    }

    /// <inheritdoc/>
    protected override void BeforeState()
    {
        // The tester's assists keep her ammunition from being found at none within her update.
        Assists.TesterAssists.Of(Room.Session).LendAmmo(Room.Session);
        _springBallWasOn = Inventory.IsOn(Equipment.SpringBall);
        _bombsHeldOff = _bombsHeldOff.Stepped();
        // A shot handed over by the last update's change of pose is this update's to fire, or is lost.
        (_shotHandedNow, _shotHanded) = (_shotHanded, false);
        (_wayHandedNow, _wayHanded) = (_wayHanded, null);
        StartCrystalFlashIfOffered();
        _ranFromStart = State is RunningState;
        _spunAtStart = State is SpinJumpingState or WallJumpingState;
        _aimAtUpdateStart = Aim;
        ChooseItem();

        // The suits she owns as her turn begins are what an item collected this update is told from. A beam that
        // became hers otherwise than by an item, as the hyper beam does, shows its colours now.
        (_woreVaria, _woreGravity) = (Inventory.Owns(Equipment.VariaSuit), Inventory.Owns(Equipment.GravitySuit));
        ShowNewBeamColours();
    }

    /// <summary>Whether an item collected this update gave her a suit, which is put on as the item's message closes.</summary>
    private bool _suitToPutOn;

    /// <summary>
    /// The suit that is hers but is not seen on her yet: one just collected, from the item's touch until the light
    /// that puts it on shows it. Empty when every suit she wears is seen.
    /// </summary>
    private string _suitUnseen = "";

    /// <summary>The suit being put on is seen on her from now, as its light shows it.</summary>
    public void ShowSuitPutOn() => _suitUnseen = "";

    /// <summary>The suit that is hers but not yet seen on her, by its equipment's name; empty when none is.</summary>
    public string SuitBeingPutOn => _suitUnseen;

    /// <summary>Shows the colours of the beams she has switched on, if she owns a beam she did not when this last looked.</summary>
    private void ShowNewBeamColours()
    {
        int beams = BeamsOwned;
        if (beams > _beamsOwned)
            ShowBeamColours();
        _beamsOwned = beams;
    }

    /// <inheritdoc/>
    public override void OnItemCollected()
    {
        // A suit clears the beam's charge as it becomes hers, before its message; a beam's colours show as its
        // message opens.
        bool varia = Inventory.Owns(Equipment.VariaSuit), gravity = Inventory.Owns(Equipment.GravitySuit);
        if ((varia && !_woreVaria) || (gravity && !_woreGravity))
        {
            ForgetCharge();
            _suitToPutOn = true;
            _suitUnseen = gravity && !_woreGravity ? Equipment.GravitySuit : Equipment.VariaSuit;
        }

        (_woreVaria, _woreGravity) = (varia, gravity);
        ShowNewBeamColours();
    }

    /// <inheritdoc/>
    public override void OnItemPickupEnded()
    {
        if (!_suitToPutOn)
            return;
        _suitToPutOn = false;

        // A suit that has just become hers holds her while it is put on, from the update its message closes. Putting
        // it on ends an elevator's ride and disarms its door.
        ElevatorJourney.Of(Room.Session).End();
        ChangeState(new CollectingSuitState());
    }

    /// <inheritdoc/>
    protected override PlayerState? ChooseState(PlayerState current)
    {
        GameInput input = Input;
        bool aimedDown = _aimedDownLastUpdate;
        _aimedDownLastUpdate = Aim == Aim.Down;

        switch (current)
        {
            case StandingState or RunningState or CrouchingState:
                if (JumpPressed)
                {
                    // A jump from a run is a spin; from standing or a crouch it is a plain jump, whatever is held. A
                    // press of down in the same update crouches her instead.
                    if (current is RunningState && !(CanCrouch && CrouchWhileRunning && input.WasPressed(GameAction.Down)))
                        return new SpinJumpingState();
                    return null;
                }

                // On the update her landing's own end falls, that end outranks what the input asks of her stance: the
                // moonwalk and the aim straight up wait an update, as a run and a turn do. A press of down crouches her
                // before the moonwalk is tried, as the cartridge's list has it: the standing state crouches her.
                bool crouchAsked = CanCrouch && input.WasPressed(GameAction.Down);
                if (current is StandingState { LandingEndsNow: false } && !crouchAsked && Room.Session.Options.IsOn(Toggles.Moonwalk) &&
                    input.IsHeld(GameAction.Attack) && Intent != 0 && Intent > 0 != (Facing == Facing.Right))
                {
                    // The moonwalk is taken up aiming as asked, angle-down tested first whichever way she faces.
                    Aim = MoonwalkingState.AimOf(this, angleDownFirst: true);
                    return new MoonwalkingState();
                }

                // Standing with both angle actions held she aims straight up, and a direction the way she faces does not start a run.
                if (current is StandingState { LandingEndsNow: false } && input.IsHeld(Actions.AngleUp) && input.IsHeld(Actions.AngleDown) &&
                    Intent != 0 && Intent > 0 == (Facing == Facing.Right))
                {
                    AimFromInput();
                    return current;
                }
                if (ScopeStarts(input))
                    return new XrayState();
                if (Selected == Equipment.GrappleBeam && input.WasPressed(GameAction.Attack) && Grapple is { } grapple)
                    grapple.Fire(this);

                // Settling from a landing she keeps the aim she came down in while nothing is asked of her that the
                // landing answers: the standing state, asked, would lower it.
                return KeepsLandingAim(current) ? current : null;

            case WallStoppedState:
                // Stopped against a wall she may still look through the scope or fire the grapple.
                if (ScopeStarts(input))
                    return new XrayState();
                if (Selected == Equipment.GrappleBeam && input.WasPressed(GameAction.Attack) && Grapple is { } againstWall)
                    againstWall.Fire(this);
                return null;

            case TurningState { Crouched: false }:
                // A turn becomes a spin jump while the jump and the way she now faces are held; a turn begun aiming
                // needs the jump pressed afresh.
                bool towardFacing = Intent != 0 && Intent > 0 == (Facing == Facing.Right);
                bool jumps = Aim == Aim.Forward ? input.IsHeld(GameAction.Jump) : JumpPressed;
                return towardFacing && jumps ? new SpinJumpingState() : null;

            case WallJumpingState:
                // The first of these that holds, as the cartridge has it: a press of down curls her up; the direction
                // behind her turns the wall jump into a spin that way; the angle-up action makes it a jump aiming
                // diagonally up, the angle-down action one aiming diagonally down, the attack one with her arm held
                // out level; the jump held keeps it; anything else lets it go into a spin.
                if (input.WasPressed(GameAction.Down) && MayCrawl)
                    return TakeLowForm();
                if (input.IsHeld(Facing == Facing.Right ? GameAction.Left : GameAction.Right))
                    return new SpinJumpingState(SpinJumpingState.Start.Turning);
                if (input.IsHeld(Actions.AngleUp))
                    return new JumpingState(new AirStance(Aim.DiagonalUp));
                if (input.IsHeld(Actions.AngleDown))
                    return new JumpingState(new AirStance(Aim.DiagonalDown));
                if (input.IsHeld(GameAction.Attack))
                    return new JumpingState(new AirStance(Aim.Forward, AirArm.Out));
                return input.IsHeld(GameAction.Jump) ? current : new SpinJumpingState(SpinJumpingState.Start.AfterWallJump);

            case JumpingState { GatheringListens: true } when Facing == Facing.Left && input.IsHeld(GameAction.Right):
                // The cartridge's left-facing list for a jump's first pose alone turns her on Right held without the
                // jump too; the right-facing one has no such rule for Left.
                return new AirTurningState(fromJump: true);

            case JumpingState or FallingState or SpinJumpingState:
                // Down pressed again while already aiming down curls her into the ball in the air.
                if (input.WasPressed(GameAction.Down) && aimedDown && MayCrawl)
                    return TakeLowForm();
                if (Selected == Equipment.GrappleBeam && input.WasPressed(GameAction.Attack) && Grapple is { } inAir)
                    inAir.Fire(this);
                return null;

            case KnockedBackState:
                // Holding the jump and the direction behind her in a knockback carries her off that way in a damage
                // boost. The ball is never in this state: it is thrown as it is, and has no boost.
                bool behind = input.IsHeld(Facing == Facing.Right ? GameAction.Left : GameAction.Right);
                // The throw she leaves still has this update of its own to run.
                if (behind && input.IsHeld(GameAction.Jump))
                    return new DamageBoostState(Facing.Opposite(), Math.Max(0, (Thrown?.UpdatesLeft ?? 0) - 1));
                return null;

            default:
                return null;
        }
    }

    /// <summary>
    /// Whether she keeps, on this update, the aim she landed in: she is settling from a landing she came down aiming up
    /// or diagonally in, and nothing is held that the landing's pose answers (no direction, no aim, no fresh jump and
    /// no fresh press of down; the attack alone is nothing to it), as the cartridge's aimed landings keep their aim
    /// until they end in the aimed stand. On the update the landing's own end falls the standing state asks nothing of
    /// her aim itself.
    /// </summary>
    private bool KeepsLandingAim(PlayerState current)
    {
        if (current is not StandingState { IsSettling: true, LandingEndsNow: false } || Aim is not (Aim.Up or Aim.DiagonalUp or Aim.DiagonalDown))
            return false;
        GameInput input = Input;
        return Intent == 0 && !JumpPressed && !input.WasPressed(GameAction.Down) &&
            !input.IsHeld(GameAction.Up) && !input.IsHeld(Actions.AngleUp) && !input.IsHeld(Actions.AngleDown);
    }

    /// <summary>Her grapple beam, when her scene gives her one.</summary>
    public SamusGrapple? Grapple => GetNodeOrNull<SamusGrapple>("Grapple");

    /// <summary>The flare at the end of her arm cannon while she holds a charge.</summary>
    public SamusFlare? Flare => GetNodeOrNull<SamusFlare>("Flare");

    /// <summary>Her X-ray scope, when her scene gives her one.</summary>
    public SamusXray? Xray => GetNodeOrNull<SamusXray>("Xray");

    /// <summary>The light that puts a suit on her as she collects it, when her scene gives her one.</summary>
    public SamusSuitLight? SuitLight => GetNodeOrNull<SamusSuitLight>("SuitLight");

    /// <summary>The wait between her shots that the X-ray scope holds her weapons at, on every update its time stands still.</summary>
    private const int ScopeShotWait = 32;

    /// <summary>
    /// The wait between her shots counts down whatever she is doing, as the cartridge counts it before it asks what
    /// her movement lets her weapons do. It stands where the cartridge does nothing of her weapons at all: facing
    /// out of the screen, through the refill from her reserves and through her death. Under the X-ray scope it is
    /// put back to a wait of its own on every update, so the first shot after the scope waits that out.
    /// </summary>
    protected override void WaitToAttack()
    {
        if (State is RefillingFromReservesState or DyingState)
            return;
        if (State is XrayState)
        {
            Weapons.SetWait(ScopeShotWait);
            return;
        }

        if (!SamusLooks.FacesOut(this))
            base.WaitToAttack();
    }

    /// <inheritdoc/>
    protected override void Attack(GameInput input)
    {
        string weapon = WeaponInHand();

        // A turn out of a moonwalk fires only the beam handed to it: the cartridge's turning pose has no aim of its own
        // for a missile or for the hyper beam, which take no handed shot, so it launches neither and waits as a turn
        // does otherwise.
        if (HandedWay is not null && (weapon != Arsenal.Beam || Inventory.IsOn(Equipment.HyperBeam)))
        {
            WaitToAttack();
            return;
        }

        // A full charge is remembered for as long as the button stays down, even into the morph ball.
        bool held = input.IsHeld(GameAction.Attack);
        bool beamCharged = Weapons.Selected is { Name: Arsenal.Beam, ChargeUpdates: > 0 } selectedBeam && ChargeReaches(selectedBeam.ChargeUpdates);
        if (!held)
            (_spreadCharged, _spreadHeld) = (false, 0);
        else if (beamCharged)
            _spreadCharged = true;

        // A beam charge carried into the morph ball stays there until the ball uses it up.
        _ballCharge = Form == Crawling && (_ballCharge || (Weapons.Selected?.Name == Arsenal.Beam && ChargeReaches(1)));

        // A handed shot lets the charge go before a special attack is tried, as the cartridge's beam input has it.
        if (!_shotHandedNow && weapon == Arsenal.Beam && held && Weapons.Selected?.Name == Arsenal.Beam && Weapons.ChargedFor >= SpecialAttackUpdates &&
            Selected == Amounts.PowerBombs)
        {
            // Every try at a special attack forgets the item that was to cancel itself, whether or not her beams
            // have an attack to let loose.
            _autoCancel = false;
            if (SpecialAttackBeam() is { } special)
            {
                LetLooseSpecialAttack(special);
                return;
            }
        }

        if (weapon == Arsenal.Bomb && _spreadCharged && Inventory.IsOn(Equipment.Bombs) && Weapons.LiveOf(Arsenal.Bomb).Count == 0)
        {
            // Holding down winds the throw up higher, for a while; otherwise the five bombs go at once.
            Weapons.StepCooldown();
            if (input.IsHeld(GameAction.Down) && _spreadHeld < BombSpreadWindUpUpdates)
            {
                _spreadHeld++;
                return;
            }

            ThrowBombSpread();
            return;
        }

        if (BallUsesUpCharge(held))
        {
            ForgetCharge();
            Room.Session.Sounds.Request(Sounds.SamusStop, channel: "weapons", priority: SoundPriority.High);
        }

        // With the grapple beam chosen, the X-ray scope with the run held, or in a spin or a wall jump, the beam's own
        // input does not run, so its charge stands as it is, as the cartridge has it.
        if (weapon.Length == 0)
        {
            Weapons.StepCooldown();
            return;
        }

        if (Weapons.All.All(owned => owned.Name != weapon))
        {
            // With no such weapon nothing fires. A charge carried into the ball stands, as the ball's own rules left it.
            Weapons.StepCooldown();
            if (!_ballCharge)
                Weapons.DropCharge();
            return;
        }

        Weapons.Select(weapon);
        Engine.Actors.Projectile[] liveBefore = [.. Weapons.LiveOf(weapon)];
        bool fullyCharged = Weapons.IsCharged;
        if (_shotHandedNow && weapon == Arsenal.Beam && Weapons.Selected is { } beam && beam.Charges(Inventory))
        {
            // A handed shot makes a beam that charges let its charge go at once, before the hold is asked about: a
            // full charge as a charged shot, anything less as a plain one, if the wait and the places allow one at
            // all. The charge starts over either way.
            // The attack still held remembers no full charge after it, for the ball's spread of bombs.
            bool full = Weapons.IsCharged;
            Weapons.StepCooldown();
            Weapons.DropCharge();
            (_spreadCharged, _spreadHeld) = (false, 0);
            Weapons.Fire(this, beam, full);
        }
        else
        {
            base.Attack(input);
        }

        bool fired = Weapons.LiveOf(weapon).Count > liveBefore.Length;

        // A shot is shown in her colours where they change its pictures, and so is what it leaves as it ends, its
        // effect and its burst: the opening's flashbacks show her missiles and their blasts in the opening's colours.
        if (fired)
        {
            foreach (Engine.Actors.Projectile shot in Weapons.LiveOf(weapon).Except(liveBefore))
            {
                Colours.ShowOnNew(shot);
                shot.LeftOnEnding += left =>
                {
                    if (left is Engine.Ui.IRecoloured hers)
                        Colours.ShowOnNew(hers);
                };
            }
        }

        if (fired && weapon == Arsenal.Beam)
        {
            // A beam fired takes back the asking again of a charge's sound.
            _chargingSoundArmed = false;

            // A hyper shot starts the hyper beam's glow of her suit, and a charged shot its flash. A hyper shot also
            // takes the place of whatever charge she held with its own count, which reads as past every charge until
            // her flare has counted back.
            if (Inventory.IsOn(Equipment.HyperBeam))
            {
                BeginShotGlow("hyper_shot_glow");
                ForgetCharge();
                _hyperShotCounts = true;
                (Flare ?? throw new InvalidOperationException("A hyper shot's count is kept by her flare, and she has none.")).CountBackFromLast();
            }
            else if (fullyCharged)
                BeginShotGlow("charged_shot_flash");
        }

        // The charge begins to sound a little way into it, on the weapons' channel, so that the shot's own sound
        // takes its place; a charge that ends without a shot is hushed.
        if (weapon == Arsenal.Beam && Weapons.ChargedFor == ChargeSoundUpdates)
        {
            Room.Session.Sounds.Request(Sounds.Charging, channel: "weapons", priority: SoundPriority.High);
            _chargeSounds = true;
        }
        else if (_chargeSounds && Weapons.ChargedFor < ChargeSoundUpdates)
        {
            if (!fired)
                Room.Session.Sounds.Request(Sounds.SamusStop, channel: "weapons", priority: SoundPriority.Essential);
            _chargeSounds = false;
        }
        if (fired && _autoCancel && weapon != Arsenal.Beam)
            SelectBeam();
        if (Amounts.All.Contains(Selected) && Inventory.Amount(Selected) == 0)
            SelectBeam();
    }

    /// <summary>
    /// The beam whose special attack she can let loose: she must carry just one of the wave, ice, spazer and plasma
    /// beams. Null when she carries none of them or more than one.
    /// </summary>
    private string? SpecialAttackBeam()
    {
        string[] carried = [.. new[] { Equipment.WaveBeam, Equipment.IceBeam, Equipment.Spazer, Equipment.PlasmaBeam }.Where(Inventory.IsOn)];
        return carried.Length == 1 && SpecialAttackShots.ContainsKey(carried[0]) ? carried[0] : null;
    }

    /// <summary>
    /// Spends a power bomb on a special attack: its four lights are let loose about her. An attack with a trailing
    /// pair lets its last two loose as that shot.
    /// </summary>
    private void LetLooseSpecialAttack(string beam)
    {
        Inventory.Spend(Amounts.PowerBombs, 1);
        ShotType shot = ShotType.Read(Room.Content.ReadData(SpecialAttackShots[beam]));
        ShotType trailing = SpecialAttackTrailShots.TryGetValue(beam, out string? trail) ? ShotType.Read(Room.Content.ReadData(trail)) : shot;
        var lights = new SpecialAttack[4];
        for (int part = 0; part < lights.Length; part++)
        {
            lights[part] = (part < 2 ? shot : trailing).Launch(Position, PixelVelocity.Zero, HitSide.Player, this, this) as SpecialAttack
                ?? throw new InvalidOperationException($"The special attack of \"{beam}\" has a shot that is not a special attack's.");
        }

        for (int part = 0; part < lights.Length; part++)
        {
            lights[part].Begin(part, Facing == Facing.Right, lights);

            // The leading pair become the trailing pair's shot as they start to fall, as the cartridge's do.
            if (part < 2 && !ReferenceEquals(trailing, shot))
                lights[part].FallsAs = trailing;

            // Each light takes the place of its part among her shots, the first four: a shot there, flying or burst,
            // is gone without a sound.
            Weapons.TakePlace(ShotsPool, part, lights[part], Room);
            Room.Spawn(lights[part]);
        }

        if (shot.Sound.Length > 0)
            Room.Session.Sounds.Request(shot.Sound, channel: "weapons");
        ForgetCharge();

        // The attack sets her wait to that of her beams' charged shot, whatever it stood at.
        Weapons.SetWait(Weapons.Named(Arsenal.Beam).CooldownFor(Inventory, charged: true));
        if (Inventory.Amount(Amounts.PowerBombs) == 0)
            SelectBeam();
    }

    /// <summary>Throws the five bombs of a spread from where she is, higher the longer down was held.</summary>
    private void ThrowBombSpread()
    {
        WeaponStats bomb = Weapons.Named(Arsenal.Bomb);
        ProjectileSettings settings = bomb.ShotsFor(Inventory, charged: false)[0].Projectile;
        if (BombSpreadFuses.Length != BombSpreadThrows.Count)
            throw new InvalidOperationException($"A bomb spread has {BombSpreadFuses.Length} fuses and {BombSpreadThrows.Count} throws.");
        int higher = _spreadHeld / BombSpreadUpdatesPerPixel;
        for (int each = 0; each < BombSpreadFuses.Length; each++)
        {
            var thrown = Weapons.Launch(this, bomb, Position, PixelVelocity.Zero, settings with { LastsUpdates = BombSpreadFuses[each] }) as Bomb
                ?? throw new InvalidOperationException("Samus's bomb is not a bomb.");
            Vector2 throws = BombSpreadThrows[each];
            thrown.Throw(new PixelVelocity(Pixels.FromNumber(throws.X), Pixels.FromNumber(throws.Y - higher)), Pixels.FromNumber(Medium.Gravity));
        }

        Weapons.WaitAtLeast(bomb.CooldownFor(Inventory, charged: false));
        ForgetCharge();
        Room.Session.Sounds.Request(Sounds.SamusStop, channel: "weapons", priority: SoundPriority.High);
    }

    /// <summary>
    /// Whether the morph ball uses up the beam charge carried into it this update, as the cartridge has it: letting the
    /// attack go does; with it held, a bomb refused for want of a fresh press does, unless she has no bombs to lay or a
    /// power bomb of hers is still out.
    /// </summary>
    private bool BallUsesUpCharge(bool held)
    {
        if (!_ballCharge)
            return false;
        if (!held)
            return true;
        if (Selected == Amounts.PowerBombs)
            return !Weapons.LiveOf(Arsenal.PowerBomb).Any(bomb => !bomb.HasEnded);
        return Inventory.IsOn(Equipment.Bombs);
    }

    /// <summary>
    /// A shinespark's windup uses up her charge; one that had begun to sound stops her sounds, at a priority that
    /// gives way late, as the cartridge does.
    /// </summary>
    public void SpendChargeOnShinespark()
    {
        if (ChargeIsSounding)
            Room.Session.Sounds.Request(Sounds.SamusStop, channel: "weapons", priority: SoundPriority.High);
        ForgetCharge();
    }

    /// <summary>
    /// Throws the beam's charge away, and everything that remembers it: a charge dropped so is never hushed later, as
    /// the cartridge hushes only a charge let go that fires nothing.
    /// </summary>
    public void ForgetCharge()
    {
        Weapons.DropCharge();
        (_spreadCharged, _spreadHeld, _ballCharge, _chargeSounds, _hyperShotCounts) = (false, 0, false, false, false);
    }

    /// <summary>
    /// The weapon the attack uses right now: bombs in the morph ball, and otherwise what she has selected. The grapple
    /// beam fires none; the X-ray scope fires the beam while the run is let go, as the cartridge's does, and none while
    /// the run is held. A spin jump and a wall jump fire none and leave the charge as it is, as the cartridge's do: the
    /// attack straightens a spin out instead, and the jump it becomes fires.
    /// </summary>
    private string WeaponInHand()
    {
        if (State is SpinJumpingState or WallJumpingState)
            return "";
        if (Form == Crawling)
            return Selected == Amounts.PowerBombs ? Arsenal.PowerBomb : Arsenal.Bomb;
        return Selected switch
        {
            Amounts.Missiles => Arsenal.Missile,
            Amounts.SuperMissiles => Arsenal.SuperMissile,
            Equipment.GrappleBeam => "",
            Equipment.XrayScope => Input.IsHeld(Actions.Run) ? "" : Arsenal.Beam,
            _ => Arsenal.Beam,
        };
    }

    /// <summary>
    /// Handles the item actions: cancel selects the beam alone; select moves on to the next thing she can use. An item
    /// chosen while cancel is held is put away again once it has been used. The X-ray scope leaves her the choice, as
    /// the cartridge's HUD goes on taking it while the X-ray holds time still. Either press forgets the charge, even one
    /// that leaves the choice as it was, unless it lands on the grapple beam while the line is out.
    /// </summary>
    private void ChooseItem()
    {
        // Only being in another's hands stops the choice. A state that merely takes her pose out of her control (a
        // knockback, a shinespark, a crystal flash, the scope, a boss's grip) still lets her choose.
        if (State.InAnothersHands)
            return;
        if (Input.WasPressed(Actions.ItemCancel))
        {
            SelectBeam();
        }
        else if (Input.WasPressed(Actions.ItemSelect))
        {
            string[] ring = ["", Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs, Equipment.GrappleBeam, Equipment.XrayScope];
            int at = Array.IndexOf(ring, Selected);
            do
            {
                at = (at + 1) % ring.Length;
            }
            while (!CanSelect(ring[at]));
            Selected = ring[at];
            _autoCancel = Selected.Length > 0 && Input.IsHeld(Actions.ItemCancel);
        }
        else
        {
            return;
        }

        // As the cartridge's HUD does, the charge is dropped with no sound, and with it the marks of a charge that a bomb
        // spread and the morph ball read: its sound is not hushed now or later.
        if (Selected != Equipment.GrappleBeam || Grapple is not { IsOut: true })
            ForgetCharge();
    }

    /// <summary>
    /// Sounds a change of what she has selected as the HUD comes to show it, however it came about, as the cartridge's
    /// HUD does: silently in a spin or a wall jump, with the grapple beam out, or while the X-ray holds time still.
    /// </summary>
    private void ShowSelection()
    {
        if (Selected == _selectedShown)
            return;
        _selectedShown = Selected;
        if (State is not (SpinJumpingState or WallJumpingState or XrayState) && Grapple is not { IsOut: true })
            Room.Session.Sounds.Request(Sounds.ItemSelect, channel: "hud");
    }

    /// <summary>Whether she can select something: ammunition she has some of, or equipment that is switched on.</summary>
    private bool CanSelect(string item) =>
        item.Length == 0 || (Amounts.All.Contains(item) ? Inventory.Amount(item) > 0 : Inventory.IsOn(item));

    /// <inheritdoc/>
    protected override bool AcceptsHit(Hit hit) => State is not IUntouchable;

    /// <summary>A hit's damage, lessened by her suit: the Gravity suit's share when it is on, else the Varia suit's.</summary>
    protected override int DamageFrom(Hit hit) => hit.Damage / SuitDivisor;

    /// <summary>The part of one energy she holds beyond her whole energy, which ongoing damage is taken from first.</summary>
    private readonly Compatibility.EnergyFraction _energyFraction = new();

    /// <summary>Her suit divides ongoing damage, and it comes off the part of an energy she holds, a whole energy borrowed at once.</summary>
    protected override int OngoingPointsTaken(int hundredths) =>
        _energyFraction.Take(hundredths, SuitDivisor, Inventory.Amount(Inventory.Energy));

    /// <summary>
    /// Each thing of the room that burns her asks for the sound of her energy counting down, on the clock's cadence,
    /// while she has energy enough: its heat at the usual priority, and a liquid at the one that gives way sooner. In
    /// a liquid that burns in a heated room both ask, as on the cartridge.
    /// </summary>
    protected override void OnRoomDamage(string source)
    {
        if (RoomDamageSound.Length > 0 && RoomDamageSoundEvery > 0 && Room.Session.Clock.Updates % RoomDamageSoundEvery == 0
            && Inventory.Amount(Inventory.Energy) >= RoomDamageSoundLeastEnergy)
        {
            Room.Session.Sounds.Request(RoomDamageSound, channel: "samus", priority: source == HeatName ? SoundPriority.Normal : SoundPriority.Low);
        }
    }

    /// <summary>What her suit divides damage by.</summary>
    private int SuitDivisor =>
        Inventory.IsOn(Equipment.GravitySuit) ? GravityDamageOver : Inventory.IsOn(Equipment.VariaSuit) ? VariaDamageOver : 1;

    /// <summary>With energy gone, reserve tanks that are switched on refill her before she can die.</summary>
    protected override bool SurvivesEmptyEnergy()
    {
        if (!Inventory.IsOn(Equipment.ReserveTank) || Inventory.Amount(Amounts.ReserveEnergy) == 0)
            return false;
        if (State is not RefillingFromReservesState)
            ChangeState(new RefillingFromReservesState());
        return true;
    }

    /// <summary>
    /// Her body's blow on an enemy asks for the contact's sound, only when no sound of the enemies' waits, on every
    /// blow that harms. A charged spin's touch on an enemy spends its charge whether or not it harmed. The safety a
    /// hurt left her is ended by the enemies' own turns while her body is an attack, not here.
    /// </summary>
    protected override void OnBodyPowerLanded(BodyPower power, IHittable target, HitResult result)
    {
        if (target is not Enemy || result == HitResult.Ignored)
            return;
        if (power.Name == HitKinds.ChargedSpin)
        {
            Weapons.DropCharge();
            _hyperShotCounts = false;
        }
        if (result is not (HitResult.Hurt or HitResult.Destroyed))
            return;
        Room.Session.Sounds.Request(Sounds.EnemyContactKill, channel: "enemies", priority: SoundPriority.WhenIdle);
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Facing from)
    {
        // A hurt keeps her charge, as the cartridge's does: a knockback neither counts it on nor lets it go. Her hurt
        // is heard from the hurt's flash, on its second count.

        // A new hurt's knockback takes the place of whatever was left of the last.
        _knockbackCarried = Countdown.Over;
        if (State is RefillingFromReservesState)
            return;
        // A state that holds a knockback off still has it waiting: it begins once she is out of that state, if its
        // time has not run out by then.
        // A ball already being thrown is not thrown afresh, as an upright Samus already knocked back is not.
        bool takesKnockback = (State.InControl || State.HoldsOffInterruptions) && Thrown is null;
        if (!hit.Carries(HitKind.Hazard))
        {
            if (takesKnockback)
                KnockBack(from);
            return;
        }

        // A spike's hurt has its own lengths.
        BeInvincibleFor(HazardInvincibleUpdates);
        if (takesKnockback)
            KnockBack(from, HazardKnockbackUpdates);
    }

    /// <summary>
    /// A knockback beginning starts a hurt from its first count, whatever count one had reached. It begins as her
    /// movement ends, before a hurt is counted, so it is counted on once on this same update, as the cartridge's is:
    /// its knockback is set up before it draws her, and the hurt's count moves on as she is drawn.
    /// </summary>
    protected override void OnThrown() => _hurt = 1;

    /// <inheritdoc/>
    protected override void OnMomentumBegan()
    {
        if (!Inventory.IsOn(Equipment.SpeedBooster) || BoostStrideCounts.Length == 0)
            return;
        _boostStage = 0;
        _stridesLeft = BoostStrideCounts[0];
    }

    /// <summary>
    /// Her boost ends with her momentum, except in a shinespark and as an elevator's ride takes her: the cartridge's
    /// setup for the ride clears her run's speed and leaves the boost's count as it stood, so a boost she had, a
    /// shinespark's not yet spent among them, outlasts the ride.
    /// </summary>
    protected override void OnMomentumLost()
    {
        if (State is not (ShinesparkState or RidingElevatorState))
            EndBoost();
    }

    /// <summary>The engine's gravity, except that her fall stops gaining speed once its whole pixels reach the limit, as the original's does.</summary>
    protected override Pixels AfterGravity(Pixels speed, PlayerMedium medium)
    {
        if (!speed.IsNegative && speed.Whole == (int)medium.FallSpeedLimit)
            return speed;
        return speed + Pixels.FromNumber(medium.Gravity);
    }

    /// <summary>
    /// A jump's launch speed, raised by her dash while the speed booster is switched on. As in the original, half the
    /// dash's whole pixels, an odd one dropped, are added to the jump's whole pixels, and the dash's fraction to the
    /// jump's fraction, with nothing carried from the one sum to the other.
    /// </summary>
    public override Pixels LaunchSpeed(bool offWall, string form)
    {
        Pixels speed = base.LaunchSpeed(offWall, form);
        if (!Inventory.IsOn(Equipment.SpeedBooster))
            return speed;
        return Pixels.FromParts(speed.Whole + DashSpeed.Whole / 2, (speed.Fraction + DashSpeed.Fraction) % Pixels.FractionsPerPixel);
    }

    private readonly Compatibility.ShotInheritance _shotInheritance = new();

    /// <summary>Whether this update's change of pose handed a shot to the next update's firing, and whether the last one's did.</summary>
    private bool _shotHanded, _shotHandedNow;

    /// <summary>
    /// The way a shot handed out of a moonwalk's turn leaves, the moonwalk's facing and aim, handed this update and
    /// handed by the last; none for a jump's, which leaves the way she aims.
    /// </summary>
    private (Facing Facing, Aim Aim)? _wayHanded, _wayHandedNow;

    /// <summary>The last turn a moonwalk made with the jump, whose shots leave from heights of their own.</summary>
    private TurningState? _moonwalkJumpTurn;

    /// <summary>The way the shot handed to the turn she is in leaves; none when she is in no such turn.</summary>
    private (Facing Facing, Aim Aim)? HandedWay => State is TurningState ? _wayHandedNow : null;

    /// <summary>
    /// A turn out of a moonwalk fires the shot handed to it, though a turn cannot attack, as the cartridge's turning
    /// pose runs the beam's input while it holds one.
    /// </summary>
    protected override bool AttacksNow => base.AttacksNow || HandedWay is not null;

    /// <summary>A shot handed out of a moonwalk's turn leaves the way the moonwalk aimed, not the way she now faces.</summary>
    public override (int Across, int Down) AimDirection
    {
        get
        {
            if (HandedWay is not { } way)
                return base.AimDirection;
            int forward = way.Facing == Facing.Right ? 1 : -1;
            return way.Aim switch
            {
                Aim.Forward => (forward, 0),
                Aim.DiagonalUp => (forward, -1),
                Aim.DiagonalDown => (forward, 1),
                _ => throw new InvalidOperationException($"A moonwalk does not aim {way.Aim}."),
            };
        }
    }

    /// <inheritdoc/>
    protected override void OnBodyMoved(bool across, Pixels asked, Pixels moved) => _shotInheritance.Record(across, asked, moved);

    /// <summary>A shot of hers inherits of her movement what the cartridge's does, whichever way it is fired.</summary>
    public override PixelVelocity ShotVelocity(WeaponStats weapon, PixelVelocity own, int across, int down)
    {
        Pixels along = across != 0 ? own.X.Abs() : own.Y.Abs();
        return _shotInheritance.Launch(across, down, along, Room.Session.Camera.Speed.Y);
    }

    /// <summary>
    /// Her step down was stopped over <paramref name="under"/>, the row of pixels beneath her feet: when what stopped
    /// it first is the blocks an elevator rests on, she has stood at its door, which the elevator hears of on its
    /// next turn. Her blocks are tried from the left on an even update and from the right on an odd one.
    /// </summary>
    public void NoteElevatorStand(PixelRect under)
    {
        bool fromLeft = Room.Session.Clock.Updates % 2 == 0;
        if (Room.Objects.OfType<Elevator>().Any(elevator => elevator.IsStoodOn(under, fromLeft)))
            ElevatorJourney.Of(Room.Session).NoteStand();
    }

    /// <summary>
    /// Lets a bubble out, on the clock's cadence, while her mouth is under water and no bubble holds it off: the bubble
    /// rises from by her head, and a number is drawn for which of its two sounds is heard. In a room with no water her
    /// mouth is under unless it is above the room, as the cartridge reads the water's line there.
    /// </summary>
    private void Breathe()
    {
        int top = Head.Y.Whole, mouth = top - BreathMouthAboveTop;
        bool under = Room.Liquid is { } liquid && liquid.Type.Name == WaterName ? mouth >= liquid.Surface.Whole : mouth >= -1;
        if (!under || Room.Session.Clock.Updates % BreathBubbleEvery != 0 || _bubbleWait.IsRunning)
            return;
        if (BreathBubbleEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(BreathBubbleEffect)), PixelPosition.FromWhole(Position.X.Whole, top + BreathBubbleBelowTop)));
        _bubbleWait = Countdown.Of(BreathBubbleHoldUpdates);
        string sound = Room.Session.Random.Draw().Part(0, 1) == 1 ? BreathLowSound : BreathHighSound;
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "samus");
    }

    /// <summary>
    /// Leaves a wall jump's dust at the bottom row of her body, behind the way she now faces, as the cartridge does;
    /// none with her feet in a liquid, whatever her suit.
    /// </summary>
    private void KickUpWallJumpDust()
    {
        if (WallJumpEffect.Length == 0 || FeetAreInLiquid)
            return;
        int x = Position.X.Whole + Facing.Opposite().Toward(Pixels.FromWhole(WallJumpEffectBehind)).Whole;
        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(WallJumpEffect)), PixelPosition.FromWhole(x, Feet.Y.Whole)));
    }

    /// <inheritdoc/>
    protected override void AfterState()
    {
        (_atRestForScopeBefore, _atRestForScope) =
            (_atRestForScope, State is StandingState or RunningState or CrouchingState or WallStoppedState);

        // Having moved upright and settled, on the ground, she stands on whatever is under her feet.
        if (MovedAs is StandingState { MovedSettling: false } && IsOnGround)
            NoteElevatorStand(new PixelRect(BodyRect.Left, BodyRect.Bottom, BodyRect.Width, 1));

        // Lava ends a run the speed booster has begun to boost, and the dash with it, whatever suit she wears.
        if (_boostStage >= 0 && Room.Liquid is { } liquid && liquid.Type.Name == LavaName && FeetAreInLiquid)
            LoseMomentum();

        // Her breath: with her feet in water, and on the update they leave it for the air, a bubble may rise.
        bool feetInLiquid = Room.Liquid is not null && FeetAreInLiquid;
        if (feetInLiquid ? Room.Liquid!.Type.Name == WaterName : _liquidLastEntered == WaterName)
            Breathe();
        _bubbleWait = _bubbleWait.Stepped();
        _liquidLastEntered = Room.Liquid is { } around && FeetAreInLiquid ? around.Type.Name : null;
        if (_ranFromStart && State is RunningState)
        {
            StepStride();
            StepFootsteps();
        }

        WarnOfLowEnergy();
        Grapple?.Step(this);
        Flare?.Step(this);
        ShowSelection();
        Assists.TesterAssists.Of(Room.Session).TakeAmmoBack(Room.Session);
        if (_knockbackCarried.IsRunning)
        {
            _knockbackCarried = _knockbackCarried.Stepped();
            if (_knockbackCarried.IsOver)
                FallFromRest();
        }

        // A wall jump begun on this update kicks up its dust, once its body has been taken.
        if (State is WallJumpingState && !ReferenceEquals(State, _spinSeen))
            KickUpWallJumpDust();

        // Her spin, her stored shine and a hurt are counted on, and then what colours her is chosen, just before she is shown.
        CountSpin();
        CountStoredShine();
        int hurt = CountHurt();
        StepColours(hurt);
        _crystalFlashColouring = _crystalFlashColouring switch
        {
            CrystalFlashColouring.EndingNext => CrystalFlashColouring.Ending,
            CrystalFlashColouring.Ending => CrystalFlashColouring.None,
            _ => _crystalFlashColouring,
        };
        KeepRunningArm();
        GetNodeOrNull<SamusVisual>("Visual")?.Show(this);
        SuitLight?.Show(this);

        // Once she is dying the cartridge no longer shows her the usual way, so nothing is looked at after she is shown,
        // and a spin or a boost her death ends asks for no stop.
        if (State is not DyingState)
        {
            LookAfterShown();
            StopEndedSounds();
        }
    }

    /// <summary>
    /// As the cartridge does once she has been shown: on the update her boost ends the speed booster's sound is
    /// stopped, and on the update a spin or a wall jump ends the spin's is. Quicksand that stops her momentum forgets
    /// the boost's sound without stopping it.
    /// </summary>
    private void StopEndedSounds()
    {
        if (Room.Terrain.BlocksIn(BodyRect).Any(block => block.Type.Contact == BlockContact.PullsDown && block.Type.StopsMomentum))
            _boostSounding = false;
        if (_boostSounding && !IsBoosting)
        {
            _boostSounding = false;
            Room.Session.Sounds.Request(Sounds.SpeedBoosterStop, channel: "samus", priority: SoundPriority.Essential);
        }

        if (_spunAtStart && State is not (SpinJumpingState or WallJumpingState))
        {
            // A scene's showing of her asks for no stop of a spin's sound, having asked for none of the sound.
            if (!IsShownByScene)
                Room.Session.Sounds.Request(Sounds.SpinJumpStop, channel: "samus", priority: SoundPriority.Essential);

            // A charge that had begun to sound is asked for again at the next look after she is shown, if the attack
            // is held now and still held then.
            if (ChargeIsSounding && Input.IsHeld(GameAction.Attack))
                _chargingSoundArmed = true;
        }
    }

    /// <summary>The pictures of her stride on which a foot comes down, counted from nought.</summary>
    private static readonly int[] FootContacts = [2, 7];

    /// <summary>
    /// Asks for her footsteps' sound on the last update of each picture of her stride that brings a foot down, as the
    /// cartridge does: not in a boss's room, not while she holds a shine as the game's tests see it, not once the
    /// boost is at its full stage, and not while a scene is showing her. The request counts among the sounds waiting
    /// whether or not it is heard.
    /// </summary>
    private void StepFootsteps()
    {
        if (_strideLeft != 1 || !FootContacts.Contains(_strideAt))
            return;
        if (Room.HasBoss || ReadsAsHoldingShine || IsBoosting || IsShownByScene)
            return;
        Room.Session.Sounds.Request(Sounds.Footsteps, channel: "samus");
    }

    /// <summary>The strides a boost taken down a stage has to complete before it climbs back, as many as the cartridge's count of them holds.</summary>
    private const int StridesToClimbBack = 255;

    /// <summary>
    /// The stage whose pace the stride takes after the boost's echo was turned away: the cartridge picks it by the
    /// number the echo's sound has, which is what the request leaves where it would leave how many were waiting.
    /// </summary>
    private const int PaceAfterEchoTurnedAway = 3;

    /// <summary>
    /// Moves her running stride on by one update. The speed booster's boost is counted in strides: each one completed
    /// with momentum and the dash held counts one off the stage she is at, and with none left the stage is done. How
    /// long each picture of the stride lasts depends on what carries her, so the stages come quicker as they go.
    /// </summary>
    private void StepStride()
    {
        if (RunStrideUpdates.Length == 0 || --_strideLeft > 0)
            return;
        _strideAt++;
        if (_strideAt < RunStrideUpdates.Length)
        {
            _strideLeft = StridePictureUpdates(_strideAt);
            return;
        }

        // The stride is complete, and begins again.
        _strideAt = 0;
        bool dashing = HasMomentum && DashIsHeld;
        if (!dashing)
        {
            _strideLeft = RunStrideUpdates[0] + StrideSlowing;
            return;
        }

        if (_boostStage < 0)
        {
            _strideLeft = DashStrideUpdates[0] + StrideSlowing;
            return;
        }

        // A stride counted off a stage with none left takes the boost down a stage, to climb back from there.
        if (_stridesLeft > 0)
            _stridesLeft--;
        else if (_boostStage > 0)
            (_boostStage, _stridesLeft) = (_boostStage - 1, StridesToClimbBack);
        else
            throw new InvalidOperationException("The boost's first stage has no strides left and no stage to go down to.");
        if (_stridesLeft > 0)
        {
            _strideLeft = RunStrideUpdates[0] + StrideSlowing;
            return;
        }

        // The stage is done. Reaching the last, she is boosting, and the stride starts again at the pace the echo's
        // request picks by how many sounds it found waiting.
        int pace = _boostStage;
        if (!IsBoosting)
        {
            _boostStage++;
            pace = _boostStage;
            if (IsBoosting)
            {
                SoundAdmission echo = Room.Session.Sounds.Request(Sounds.SpeedBooster, channel: "samus");
                _boostSounding = true;
                if (echo.Admitted && echo.Waiting == BoostStrideCounts.Length)
                {
                    // A stage past the last: no strides to count, and a first picture as long as whatever lies there.
                    _stridesLeft = 0;
                    _strideLeft = Room.Session.Service<Compatibility.LeftoverStridePace>().Updates(0) + StrideSlowing;
                    return;
                }

                pace = echo.Admitted ? echo.Waiting : PaceAfterEchoTurnedAway;
            }
        }

        _stridesLeft = BoostStrideCounts[pace];
        _strideLeft = BoostStrideUpdates[pace * RunStrideUpdates.Length] + StrideSlowing;
    }

    /// <summary>How many updates a picture of her stride lasts now: by her dash and the boost's stage while she has momentum, and otherwise its own length, each slowed as she is.</summary>
    private int StridePictureUpdates(int picture)
    {
        if (!HasMomentum)
            return RunStrideUpdates[picture] + StrideSlowing;
        return StrideSlowing + (Inventory.IsOn(Equipment.SpeedBooster)
            ? BoostStrideUpdates[BoostStage * RunStrideUpdates.Length + picture]
            : DashStrideUpdates[picture]);
    }

    /// <summary>
    /// The updates added to each picture of her stride that follows another: as many as her pictures are then held
    /// longer, by the liquid she is in or by what clings to her, as the cartridge adds its delay to every picture of
    /// her animation.
    /// </summary>
    private int StrideSlowing => PictureHold(asChangeBegins: false);

    /// <summary>Starts the warning as her energy falls low and stops it as her energy recovers. The warning goes on by itself in between.</summary>
    private void WarnOfLowEnergy()
    {
        bool low = Inventory.Amount(Amounts.Energy) <= LowEnergy && State is not DyingState;
        if (low == _warningOfLowEnergy)
            return;
        _warningOfLowEnergy = low;
        Room.Session.Sounds.Request(low ? Sounds.LowEnergy : Sounds.RoomStop, channel: "warning");
    }

    /// <summary>Whether she runs with her arm held out: the attack was held in a run aiming level, and the run has aimed level since.</summary>
    private bool _runsArmOut;

    /// <summary>The whole pixels an update a plain fall must reach for its fast-fall pictures to be shown.</summary>
    [Export, DataValue("fast_fall_look_speed")]
    public int FastFallLookSpeed { get; set; } = 5;

    /// <summary>Whether a plain fall has reached that speed, and has been a plain fall since.</summary>
    private bool _fallsFast;

    /// <summary>
    /// The name of how she holds her weapon, for her looks and the heights her shots leave from: her aim's, or,
    /// aiming level, how her arm is held where that has pictures of its own. In a jump steering the way she faces it
    /// is "moving"; in a jump or a fall with the attack held, or once it has been held in a run, "arm_out"; in a
    /// plain fall that has reached its fast-fall speed, "fast". Seized, it is the grab's own.
    /// </summary>
    internal string HoldName => State is GrabbedState grabbed
        ? grabbed.Hold
        : Aim != Aim.Forward
            ? AimName(Aim)
            // Held, it is that of what she was doing as she was taken hold of.
            : (State is HeldState held ? held.Was : State) switch
            {
                JumpingState { Arm: AirArm.Forward } => "moving",
                JumpingState { Arm: AirArm.Out } or FallingState { Arm: AirArm.Out } => "arm_out",
                FallingState when _fallsFast => "fast",
                RunningState when _runsArmOut => "arm_out",
                _ => AimName(Aim),
            };

    /// <summary>
    /// Keeps whether she runs with her arm held out, as the cartridge's running poses do: the attack held with her
    /// aim level takes the pose up, and it is kept, whether or not the attack is still held, until the run ends or
    /// aims another way.
    /// </summary>
    private void KeepRunningArm()
    {
        _runsArmOut = State is RunningState && Aim == Aim.Forward && (_runsArmOut || Input.IsHeld(GameAction.Attack));

        // A plain fall, her arm at rest and her aim level, takes up its fast-fall pictures once it falls fast enough
        // and keeps them while it stays a plain fall, as the cartridge's fall does once its animation has got there.
        _fallsFast = State is FallingState { Arm: AirArm.AtRest } && Aim == Aim.Forward && (_fallsFast || MovesUpOrDownAtLeast(FastFallLookSpeed));
    }

    /// <summary>The name an aim has in her data file.</summary>
    internal static string AimName(Aim aim) => aim switch
    {
        Aim.Forward => "forward",
        Aim.DiagonalUp => "diagonal_up",
        Aim.Up => "up",
        Aim.DiagonalDown => "diagonal_down",
        Aim.Down => "down",
        _ => throw new InvalidOperationException($"Undefined aim {aim}."),
    };
}
