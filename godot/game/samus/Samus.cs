using Engine.Actors;
using Engine.Collision;
using Engine.Content;
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

    /// <summary>
    /// How many updates of dashing each stage of the speed booster's boost takes, from the first to the last. Having
    /// run through them all, she is boosting.
    /// </summary>
    [Export, DataValue("boost_stage_updates")]
    public int[] BoostStageUpdates { get; set; } = [];

    /// <summary>How many updates a stored shine lasts.</summary>
    [Export, DataValue("stored_shine_updates")]
    public int StoredShineUpdates { get; set; } = 180;

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

    /// <summary>How many updates into a charge its sound begins.</summary>
    [Export, DataValue("charge_sound_updates")]
    public int ChargeSoundUpdates { get; set; } = 16;

    /// <summary>How many updates the attack must be held, with a beam that charges, before a special attack is let loose.</summary>
    [Export, DataValue("special_attack_updates")]
    public int SpecialAttackUpdates { get; set; } = 120;

    /// <summary>The echoes a shinespark throws off as it ends, by the data file of their shot; empty for none.</summary>
    [Export(PropertyHint.File), DataValue("shinespark_echo_shot")]
    public string ShinesparkEchoShot { get; set; } = "";

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

    /// <summary>How many updates a spin that has found a wall to jump from stays ready to jump off it.</summary>
    [Export, DataValue("wall_jump_ready_updates")]
    public int WallJumpReadyUpdates { get; set; } = 8;

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

    /// <summary>The widest the X-ray scope's cone opens to each side of its middle, in 256ths of a turn.</summary>
    [Export, DataValue("xray_widest_half_width")]
    public int XrayWidestHalfWidth { get; set; } = 10;

    /// <summary>How many updates collecting a suit holds her for.</summary>
    [Export, DataValue("suit_pickup_updates")]
    public int SuitPickupUpdates { get; set; } = 240;

    /// <summary>How far the X-ray scope sees, in pixels.</summary>
    [Export, DataValue("xray_reach")]
    public int XrayReach { get; set; } = 256;

    private readonly Dictionary<string, BodyPower> _powers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> _muzzles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> _grappleOrigins = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _shotHeights = new(StringComparer.Ordinal);
    private Countdown _shine = Countdown.Over;
    private Countdown _boostStageLeft = Countdown.Over;
    private int _boostStage = -1;
    private bool _autoCancel;
    private bool _spreadCharged;
    private int _spreadHeld;
    private bool _aimedDownLastUpdate;
    private bool _woreVaria;
    private bool _woreGravity;
    private bool _warningOfLowEnergy;
    private bool _chargeSounds;

    /// <summary>
    /// What she has selected to use: the name of an ammunition amount, of the grapple beam or of the X-ray scope, or
    /// empty for the beam alone.
    /// </summary>
    public string Selected { get; private set; } = "";

    /// <summary>Her timelines, by name.</summary>
    public TimelineSet Timelines { get; private set; } = TimelineSet.None;

    /// <summary>Whether the speed booster has brought her to full speed: she is boosting.</summary>
    public bool IsBoosting => BoostStageUpdates.Length > 0 && _boostStage >= BoostStageUpdates.Length;

    /// <summary>How far through the speed booster's stages she is: 0 before the first is done, up to the number of stages when boosting.</summary>
    public int BoostStage => Math.Max(0, _boostStage);

    /// <summary>Whether she has a shine stored.</summary>
    public bool HasStoredShine => _shine.IsRunning;

    /// <summary>The body power of the given name, or null when her data gives her none of that name.</summary>
    public BodyPower? Power(string name) => _powers.GetValueOrDefault(name);

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
                var made = new BodyPower(name, power.Int("damage"), [.. power.List("kinds").Select(kind => new HitKind(kind.AsText()))]);
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

        SpecialAttackShots = [];
        if (data.Has("special_attacks"))
        {
            foreach ((string beam, DataNode shot) in data.Get("special_attacks").Entries())
                SpecialAttackShots[beam] = shot.AsText();
        }
    }

    /// <inheritdoc/>
    protected override void OnReady()
    {
        foreach (string file in WeaponFiles)
            Weapons.Add(WeaponStats.Read(Room.Content.ReadData(file)));
        _woreVaria = Inventory.Owns(Equipment.VariaSuit);
        _woreGravity = Inventory.Owns(Equipment.GravitySuit);
    }

    /// <inheritdoc/>
    protected override void OnEnteredRoom(Room room) => GetNodeOrNull<SamusVisual>("Visual")?.Bind(room.Content);

    /// <summary>Her landing leaves its dust and makes its sound, a harder one from a fast fall.</summary>
    protected override void OnLanded()
    {
        base.OnLanded();
        if (Form != Crawling)
            Room.Session.Sounds.Request(LandingSpeed.Whole >= HardLandingSpeed ? Sounds.HardLand : Sounds.Land, channel: "samus");
    }

    /// <summary>
    /// Throws the morph ball as a bomb of her own lies against it: straight up when the bomb was beneath her middle,
    /// and to the side away from it otherwise. Only the ball at rest in her control is thrown.
    /// </summary>
    public void BombJump(PixelPosition bomb)
    {
        if (State is not CrawlingState)
            return;
        int away = Position.X.Whole - bomb.X.Whole;
        Interrupt(new BombJumpingState(away));
    }

    /// <summary>
    /// Where a shot starts: from her middle, raised by how high the pose she is in holds her arm, at the place her
    /// arm points for the way she faces and aims.
    /// </summary>
    public override PixelPosition MuzzlePoint => Origin(_muzzles);

    /// <summary>Where her grapple beam starts: as a shot does, at the places her data gives the line.</summary>
    public PixelPosition GrapplePoint => Origin(_grappleOrigins);

    /// <summary>
    /// The place of <paramref name="origins"/> for how she stands, faces and aims, raised by how high her pose holds her
    /// arm. Her running places hold while she runs, and while she moonwalks aiming diagonally up.
    /// </summary>
    private PixelPosition Origin(Dictionary<string, Dictionary<string, Dictionary<string, Vector2I>>> origins)
    {
        // In the morph ball there is no arm to fire from: what she lays, she lays where she is.
        if (Form == Crawling)
            return Position;
        string stance = State is RunningState || (State is MoonwalkingState && Aim == Aim.DiagonalUp) ? "running" : "standing";
        string facing = Facing == Facing.Right ? "right" : "left";
        if (!origins.TryGetValue(stance, out var facings) || !facings.TryGetValue(facing, out var aims) ||
            !aims.TryGetValue(AimName(Aim), out Vector2I offset))
            return base.MuzzlePoint;
        string look = SamusLooks.Of(this);
        int height = _shotHeights.TryGetValue($"{look}_{AimName(Aim)}", out int aimed) ? aimed : _shotHeights.GetValueOrDefault(look, 0);
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
    /// Sets her aim: the angle actions aim diagonally, and both together straight up; up aims up, or diagonally up
    /// while she steers; down aims down in the air, or diagonally down while she steers. On the ground down aims only
    /// with a direction. Running with a direction held, she aims level or diagonally and never straight up.
    /// </summary>
    public override void AimFromInput()
    {
        bool up = Input.IsHeld(GameAction.Up), down = Input.IsHeld(GameAction.Down);
        bool angleUp = Input.IsHeld(Actions.AngleUp), angleDown = Input.IsHeld(Actions.AngleDown);
        bool steering = Intent != 0;
        bool grounded = IsOnGround;
        if (State is RunningState && steering)
            Aim = angleUp ? Aim.DiagonalUp : angleDown ? Aim.DiagonalDown : up ? Aim.DiagonalUp : down ? Aim.DiagonalDown : Aim.Forward;
        else if (angleUp && angleDown)
            Aim = Aim.Up;
        else if (angleUp)
            Aim = Aim.DiagonalUp;
        else if (angleDown)
            Aim = Aim.DiagonalDown;
        else if (up)
            Aim = steering ? Aim.DiagonalUp : Aim.Up;
        else if (down && !grounded)
            Aim = steering ? Aim.DiagonalDown : Aim.Down;
        else if (down && steering)
            Aim = Aim.DiagonalDown;
        else
            Aim = Aim.Forward;
    }

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
        if (to is not RunningState)
            return to;
        bool alreadyRunning = from is RunningState;
        RunPose pose = NextRunPose(alreadyRunning);
        bool stepsOn = !alreadyRunning || unasked || pose != _runPose;
        _runPose = pose;
        if (!stepsOn)
            return to;
        Facing headed = from is MoonwalkingState ? Facing.Opposite() : Facing;
        MoveResult step = Room.Mover.MoveAcross(Body, headed.Toward(Pixels.FromWhole(1)), MoveOptions.Plain);
        if (!step.Blocked)
            return to;
        return from is StandingState ? from : new StandingState();
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
        Room.Session.Sounds.Request(Sounds.StoredShine, channel: "samus");
    }

    /// <summary>Uses up the stored shine.</summary>
    public void SpendShine() => _shine = Countdown.Over;

    /// <summary>Brings her straight to full boost, as a shinespark does.</summary>
    public void BoostFully() => _boostStage = BoostStageUpdates.Length;

    /// <summary>Ends her boost.</summary>
    public void EndBoost()
    {
        _boostStage = -1;
        _boostStageLeft = Countdown.Over;
    }

    /// <inheritdoc/>
    protected override void BeforeState()
    {
        // Each update's movement says anew which of her body's powers are at work; they stay so for what touches her
        // afterwards. The speed booster's is at work from the update after she reaches full boost.
        foreach (BodyPower power in BodyPowers)
            power.IsActive = false;
        if (Power(HitKinds.SpeedBoost) is { } boost && IsBoosting && State is not ShinesparkState)
            boost.IsActive = true;
        _shine = _shine.Stepped();
        ChooseItem();

        // A suit that has just become hers holds her while it is put on.
        bool varia = Inventory.Owns(Equipment.VariaSuit), gravity = Inventory.Owns(Equipment.GravitySuit);
        if ((varia && !_woreVaria) || (gravity && !_woreGravity))
            ChangeState(new CollectingSuitState());
        _woreVaria = varia;
        _woreGravity = gravity;
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
                    if (HasStoredShine)
                        return new ShinesparkState();

                    // A jump from a run is a spin; from standing or a crouch it is a plain jump, whatever is held. A
                    // press of down in the same update crouches her instead.
                    if (current is RunningState && !(MayCrouch && CrouchWhileRunning && input.WasPressed(GameAction.Down)))
                        return new SpinJumpingState();
                    return null;
                }

                if (current is RunningState && IsBoosting && input.WasPressed(GameAction.Down))
                {
                    StoreShine();
                    return new CrouchingState();
                }

                if (current is StandingState && Room.Session.Options.IsOn(Toggles.Moonwalk) && input.IsHeld(GameAction.Attack) &&
                    Intent != 0 && Intent > 0 != (Facing == Facing.Right))
                    return new MoonwalkingState();

                // Standing with both angle actions held she aims straight up, and a direction the way she faces does not start a run.
                if (current is StandingState && input.IsHeld(Actions.AngleUp) && input.IsHeld(Actions.AngleDown) &&
                    Intent != 0 && Intent > 0 == (Facing == Facing.Right))
                {
                    AimFromInput();
                    return current;
                }
                if (current is CrouchingState && Intent != 0 && Intent > 0 == (Facing == Facing.Right) && TrySetForm(Standing))
                    return new RunningState();
                if (current is not RunningState && Selected == Equipment.XrayScope && input.IsHeld(Actions.Run))
                    return new XrayState();
                if (Selected == Equipment.GrappleBeam && input.WasPressed(GameAction.Attack) && Grapple is { } grapple)
                    grapple.Fire(this);
                return null;

            case TurningState:
                // A turn becomes a spin jump while the jump and the way she now faces are held; a turn begun aiming
                // needs the jump pressed afresh.
                bool towardFacing = Intent != 0 && Intent > 0 == (Facing == Facing.Right);
                bool jumps = Aim == Aim.Forward ? input.IsHeld(GameAction.Jump) : JumpPressed;
                return towardFacing && jumps ? new SpinJumpingState() : null;

            case WallJumpingState:
                // Steering the other way turns the wall jump into a spin that way; an angle held or the attack makes
                // it a jump; the jump held keeps it; anything else lets it go into a spin.
                if (Intent != 0 && Intent > 0 != (Facing == Facing.Right))
                    return new SpinJumpingState(SpinJumpingState.Start.Turning);
                if (input.IsHeld(Actions.AngleUp) || input.IsHeld(Actions.AngleDown) || input.IsHeld(GameAction.Attack))
                {
                    AimFromInput();
                    return new JumpingState(keepsSpeed: true);
                }

                return input.IsHeld(GameAction.Jump) ? current : new SpinJumpingState(SpinJumpingState.Start.AfterWallJump);

            case JumpingState or FallingState or SpinJumpingState:
                // Down pressed again while already aiming down curls her into the ball in the air.
                if (input.WasPressed(GameAction.Down) && aimedDown && MayCrawl)
                    return new CrawlingState();
                if (Selected == Equipment.GrappleBeam && input.WasPressed(GameAction.Attack) && Grapple is { } inAir)
                    inAir.Fire(this);
                return null;

            case CrawlingState:
                return CrystalFlashState.CanStart(this) ? new CrystalFlashState() : null;

            case KnockedBackState:
                // Jumping out of a knockback while steering carries her off in a damage boost.
                if (JumpPressed && Intent != 0 && Form != Crawling)
                    return new DamageBoostState(Intent > 0 ? Facing.Right : Facing.Left);
                return null;

            default:
                return null;
        }
    }

    /// <summary>Her grapple beam, when her scene gives her one.</summary>
    public SamusGrapple? Grapple => GetNodeOrNull<SamusGrapple>("Grapple");

    /// <summary>Her X-ray scope, when her scene gives her one.</summary>
    public SamusXray? Xray => GetNodeOrNull<SamusXray>("Xray");

    /// <inheritdoc/>
    protected override void Attack(GameInput input)
    {
        string weapon = WeaponInHand();

        // A full charge is remembered for as long as the button stays down, even into the morph ball.
        bool held = input.IsHeld(GameAction.Attack);
        bool beamCharged = Weapons.Selected?.Name == Arsenal.Beam && Weapons.IsCharged;
        if (!held)
            (_spreadCharged, _spreadHeld) = (false, 0);
        else if (beamCharged)
            _spreadCharged = true;

        if (weapon == Arsenal.Beam && held && Weapons.Selected?.Name == Arsenal.Beam && Weapons.ChargedFor >= SpecialAttackUpdates &&
            Selected == Amounts.PowerBombs && SpecialAttackShot() is { } special)
        {
            LetLooseSpecialAttack(special);
            return;
        }

        if (weapon == Arsenal.Bomb && _spreadCharged && Inventory.IsOn(Equipment.Bombs) && Weapons.LiveOf(Arsenal.Bomb).Count == 0)
        {
            // Holding down winds the throw up higher, for a while; otherwise the five bombs go at once.
            Weapons.StepCooldown();
            if (input.IsHeld(GameAction.Down) && _spreadHeld < SpreadWindUpUpdates)
            {
                _spreadHeld++;
                return;
            }

            ThrowBombSpread();
            return;
        }

        if (weapon.Length == 0 || Weapons.All.All(owned => owned.Name != weapon))
        {
            Weapons.StepCooldown();
            Weapons.DropCharge();
            return;
        }

        Weapons.Select(weapon);
        int liveBefore = Weapons.LiveOf(weapon).Count;
        base.Attack(input);
        bool fired = Weapons.LiveOf(weapon).Count > liveBefore;

        // The charge begins to sound a little way into it, on the weapons' channel, so that the shot's own sound
        // takes its place; a charge that ends without a shot is hushed.
        if (weapon == Arsenal.Beam && Weapons.ChargedFor == ChargeSoundUpdates)
        {
            Room.Session.Sounds.Request(Sounds.Charging, channel: "weapons");
            _chargeSounds = true;
        }
        else if (_chargeSounds && Weapons.ChargedFor < ChargeSoundUpdates)
        {
            if (!fired)
                Room.Session.Sounds.Request(Sounds.Silence, channel: "weapons");
            _chargeSounds = false;
        }
        if (fired && _autoCancel && weapon != Arsenal.Beam)
            SelectBeam();
        if (Amounts.All.Contains(Selected) && Inventory.Amount(Selected) == 0)
            SelectBeam();
    }

    /// <summary>How long holding down can go on winding a bomb spread up, and how many updates of it throw a pixel higher.</summary>
    private const int SpreadWindUpUpdates = 192, SpreadUpdatesPerPixel = 64;

    /// <summary>The five bombs of a spread, left to right: how long each lies before it bursts, and how it is thrown.</summary>
    private static readonly (int Fuse, double Across, double Down)[] SpreadBombs =
    [
        (120, -1, 0), (110, -0.5, -1), (100, 0, -1.5), (110, 0.5, -1), (120, 1, 0),
    ];

    /// <summary>
    /// The shot of the special attack she can let loose: she must carry just one of the wave, ice, spazer and plasma
    /// beams. Null when she carries none of them or more than one.
    /// </summary>
    private string? SpecialAttackShot()
    {
        string[] carried = [.. new[] { Equipment.WaveBeam, Equipment.IceBeam, Equipment.Spazer, Equipment.PlasmaBeam }.Where(Inventory.IsOn)];
        return carried.Length == 1 && SpecialAttackShots.TryGetValue(carried[0], out string? shot) ? shot : null;
    }

    /// <summary>Spends a power bomb on a special attack: its four lights are let loose about her.</summary>
    private void LetLooseSpecialAttack(string file)
    {
        Inventory.Spend(Amounts.PowerBombs, 1);
        ShotType shot = ShotType.Read(Room.Content.ReadData(file));
        for (int part = 0; part < 4; part++)
        {
            var light = shot.Launch(Position, PixelVelocity.Zero, HitSide.Player, this, this) as SpecialAttack
                ?? throw new InvalidOperationException($"The shot \"{file}\" is not a special attack's.");
            light.Begin(part, Facing == Facing.Right);
            Room.Spawn(light);
        }

        if (shot.Sound.Length > 0)
            Room.Session.Sounds.Request(shot.Sound, channel: "weapons");
        Weapons.DropCharge();
        Weapons.WaitAtLeast(Weapons.Named(Arsenal.Beam).CooldownFor(Inventory, charged: true));
        if (Inventory.Amount(Amounts.PowerBombs) == 0)
            SelectBeam();
    }

    /// <summary>Throws the five bombs of a spread from where she is, higher the longer down was held.</summary>
    private void ThrowBombSpread()
    {
        WeaponStats bomb = Weapons.Named(Arsenal.Bomb);
        ProjectileSettings settings = bomb.ShotsFor(Inventory, charged: false)[0].Projectile;
        int higher = _spreadHeld / SpreadUpdatesPerPixel;
        foreach ((int fuse, double across, double down) in SpreadBombs)
        {
            var thrown = Weapons.Launch(this, bomb, Position, PixelVelocity.Zero, settings with { LastsUpdates = fuse }) as Bomb
                ?? throw new InvalidOperationException("Samus's bomb is not a bomb.");
            thrown.Throw(new PixelVelocity(Pixels.FromNumber(across), Pixels.FromNumber(down - higher)), Pixels.FromNumber(Medium.Gravity));
        }

        Weapons.WaitAtLeast(bomb.CooldownFor(Inventory, charged: false));
        (_spreadCharged, _spreadHeld) = (false, 0);
    }

    /// <summary>The weapon the attack uses right now: bombs in the morph ball, and otherwise what she has selected.</summary>
    private string WeaponInHand()
    {
        if (Form == Crawling)
            return Selected == Amounts.PowerBombs ? Arsenal.PowerBomb : Arsenal.Bomb;
        return Selected switch
        {
            Amounts.Missiles => Arsenal.Missile,
            Amounts.SuperMissiles => Arsenal.SuperMissile,
            Equipment.GrappleBeam or Equipment.XrayScope => "",
            _ => Arsenal.Beam,
        };
    }

    /// <summary>
    /// Handles the item actions: cancel selects the beam alone; select moves on to the next thing she can use. An item
    /// chosen while cancel is held is put away again once it has been used.
    /// </summary>
    private void ChooseItem()
    {
        if (!State.InControl)
            return;
        if (Input.WasPressed(Actions.ItemCancel))
        {
            SelectBeam();
            return;
        }

        if (!Input.WasPressed(Actions.ItemSelect))
            return;
        string[] ring = ["", Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs, Equipment.GrappleBeam, Equipment.XrayScope];
        int at = Array.IndexOf(ring, Selected);
        do
        {
            at = (at + 1) % ring.Length;
        }
        while (!CanSelect(ring[at]));
        Selected = ring[at];
        _autoCancel = Selected.Length > 0 && Input.IsHeld(Actions.ItemCancel);
        Room.Session.Sounds.Request(Sounds.ItemSelect, channel: "hud");
    }

    /// <summary>Whether she can select something: ammunition she has some of, or equipment that is switched on.</summary>
    private bool CanSelect(string item) =>
        item.Length == 0 || (Amounts.All.Contains(item) ? Inventory.Amount(item) > 0 : Inventory.IsOn(item));

    /// <inheritdoc/>
    protected override bool AcceptsHit(Hit hit) => State is not IUntouchable;

    /// <summary>A hit's damage, lessened by her suit: the Gravity suit's share when it is on, else the Varia suit's.</summary>
    protected override int DamageFrom(Hit hit) => hit.Damage / SuitDivisor;

    /// <inheritdoc/>
    protected override int OngoingDamageTaken(int hundredths) => hundredths / SuitDivisor;

    /// <inheritdoc/>
    protected override void OnRoomDamage()
    {
        if (RoomDamageSound.Length > 0 && RoomDamageSoundEvery > 0 && Room.Session.Clock.Updates % RoomDamageSoundEvery == 0
            && Inventory.Amount(Inventory.Energy) >= RoomDamageSoundLeastEnergy)
            Room.Session.Sounds.Request(RoomDamageSound, channel: "samus");
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

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Facing from)
    {
        Room.Session.Sounds.Request(Sounds.SamusHurt, channel: "samus");
        Weapons.DropCharge();
        if (State is RefillingFromReservesState)
            return;
        if (!hit.Carries(HitKind.Hazard))
        {
            base.OnHurt(hit, from);
            return;
        }

        // A spike's hurt has its own lengths.
        BeInvincibleFor(HazardInvincibleUpdates);
        if (State.InControl)
            KnockBack(from, HazardKnockbackUpdates);
    }

    /// <inheritdoc/>
    protected override void OnMomentumBegan()
    {
        if (!Inventory.IsOn(Equipment.SpeedBooster) || BoostStageUpdates.Length == 0)
            return;
        _boostStage = 0;
        _boostStageLeft = Countdown.Of(BoostStageUpdates[0]);
    }

    /// <inheritdoc/>
    protected override void OnMomentumLost()
    {
        if (State is not ShinesparkState)
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
    public override Pixels LaunchSpeed(bool offWall)
    {
        Pixels speed = base.LaunchSpeed(offWall);
        if (!Inventory.IsOn(Equipment.SpeedBooster))
            return speed;
        return Pixels.FromParts(speed.Whole + DashSpeed.Whole / 2, (speed.Fraction + DashSpeed.Fraction) % Pixels.FractionsPerPixel);
    }

    /// <inheritdoc/>
    protected override void AfterState()
    {
        StepBoost();
        WarnOfLowEnergy();
        Grapple?.Step(this);
        GetNodeOrNull<SamusVisual>("Visual")?.Show(this);
    }

    /// <summary>Moves the speed booster's stages on while she dashes with it switched on.</summary>
    private void StepBoost()
    {
        if (_boostStage < 0 || IsBoosting || State is not RunningState || !HasMomentum || !DashIsHeld)
            return;
        _boostStageLeft = _boostStageLeft.Stepped();
        if (_boostStageLeft.IsRunning)
            return;
        _boostStage++;
        if (IsBoosting)
            Room.Session.Sounds.Request(Sounds.SpeedBooster, channel: "samus");
        else
            _boostStageLeft = Countdown.Of(BoostStageUpdates[_boostStage]);
    }

    /// <summary>Starts the warning as her energy falls low and stops it as her energy recovers. The warning goes on by itself in between.</summary>
    private void WarnOfLowEnergy()
    {
        bool low = Inventory.Amount(Amounts.Energy) <= LowEnergy && State is not DyingState;
        if (low == _warningOfLowEnergy)
            return;
        _warningOfLowEnergy = low;
        Room.Session.Sounds.Request(low ? Sounds.LowEnergy : Sounds.Silence, channel: "warning");
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
