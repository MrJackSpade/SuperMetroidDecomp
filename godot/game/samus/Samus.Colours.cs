using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// What colours her. Once an update, after she has moved and just before she is shown, each of her glows moves on
/// while what makes it glow holds, and the colouring her suit shows is chosen from what she is doing: the first that
/// holds of her suit's glow as she appears where a saved game is taken up, a drained Samus's glow in the hyper beam's colours, the flash of whatever drains her, a hurt's flash, the shine's glow, the screw attack's glow, the speed
/// booster's, a shot's glow and a full charge's pulse; with none, the colours of the suit she wears. Nothing sets her
/// suit's colouring anywhere else while she is played. The count of a hurt's flash lives here with the sounds it times.
/// </summary>
public partial class Samus
{
    /// <summary>The data file her colourings and glows are read from, within the assets tree.</summary>
    [Export]
    public string ColoursFile { get; set; } = "samus/colours.json";

    /// <summary>The count of a hurt's flash on which her hurt is heard.</summary>
    [Export, DataValue("hurt_flash_sound_count")]
    public int HurtFlashSoundCount { get; set; } = 2;

    /// <summary>The counts of a hurt's flash, from its first, that colour her: the hurt's colouring and her suit's turn about.</summary>
    [Export, DataValue("hurt_flash_coloured_counts")]
    public int HurtFlashColouredCounts { get; set; } = 6;

    /// <summary>The count a hurt's flash reaches on which the sound the hurt cut short is asked for again.</summary>
    [Export, DataValue("hurt_flash_recovery_count")]
    public int HurtFlashRecoveryCount { get; set; } = 40;

    /// <summary>The count a hurt's flash ends on reaching.</summary>
    [Export, DataValue("hurt_flash_end_count")]
    public int HurtFlashEndCount { get; set; } = 60;

    /// <summary>The count a flying shinespark holds a hurt's flash at, on every update it moves her.</summary>
    [Export, DataValue("spark_hurt_flash_count")]
    public int SparkHurtFlashCount { get; set; } = 8;

    /// <summary>How many updates a shinespark's glow lasts from its windup's start.</summary>
    [Export, DataValue("spark_windup_glow_updates")]
    public int SparkWindupGlowUpdates { get; set; } = 60;

    /// <summary>How many updates a shinespark's glow lasts from each update it flies or crashes.</summary>
    [Export, DataValue("spark_glow_updates")]
    public int SparkGlowUpdates { get; set; } = 15;

    /// <summary>How many updates a shinespark's glow lasts from the end of its crash.</summary>
    [Export, DataValue("spark_finished_glow_updates")]
    public int SparkFinishedGlowUpdates { get; set; } = 1;

    private SamusColours? _colours;

    /// <summary>Her colours.</summary>
    public SamusColours Colours => _colours ?? throw new InvalidOperationException("Samus's colours are read as she is made ready.");

    /// <summary>
    /// The count of a hurt's flash: none while there is no flash. A knockback starts it at one, and so does energy found
    /// lost after she is shown while there is none; a flying shinespark holds it, and its windup and its crash end it.
    /// </summary>
    private int _hurtFlash;

    /// <summary>Her energy when it was last looked at for a loss, after she was shown.</summary>
    private int _energyAtLastCheck;

    /// <summary>How many more updates the shinespark's glow lasts.</summary>
    private Countdown _sparkGlow = Countdown.Over;

    /// <summary>Where the shine's glow stands in its colourings; the stored shine's and the shinespark's each start it over.</summary>
    private int _shineGlowPlace;

    /// <summary>Where the screw attack's glow stands in its colourings; it starts over whenever it does not show.</summary>
    private int _screwGlowPlace;

    /// <summary>The speed booster's glow, played from the update she reaches full boost and resting on its last colouring; none while she is not boosting.</summary>
    private SamusGlowPlay? _speedGlow;

    /// <summary>Where the pulse of a full charge stands in its colourings; it starts over whenever she is not fully charged.</summary>
    private int _chargePulsePlace;

    /// <summary>The glow a charged shot or a hyper shot started, until it has shown its last colouring; none otherwise.</summary>
    private SamusGlowPlay? _shotGlow;

    /// <summary>The glow of her suit coming to its colours as she appears where a saved game is taken up, until it has shown its last colouring; none otherwise.</summary>
    private SamusGlowPlay? _appearanceGlow;

    /// <summary>Whether something that drains her has hold of her, and whether its flash shows its bright colouring this update.</summary>
    private bool _drainFlashing, _drainFlashBright;

    /// <summary>Whether the charge's sound is to be asked for again at the next look after she is shown.</summary>
    private bool _chargingSoundArmed;

    /// <summary>The suit whose colours she shows: the gravity suit's before the varia suit's, and the power suit's with neither on.</summary>
    public string SuitWorn => Inventory.IsOn(Equipment.GravitySuit) ? "gravity" : Inventory.IsOn(Equipment.VariaSuit) ? "varia" : "power";

    /// <summary>
    /// The colours a scene shows her suit and her beams in, whatever she wears and has switched on: the opening's
    /// flashbacks show her in the opening's own. Nothing while she is shown in her own.
    /// </summary>
    public string SceneColours { get; private set; } = "";

    /// <summary>The colouring her suit rests in: the scene's while one shows her in its own, else that of the suit she wears.</summary>
    private string SuitShown => SceneColours.Length > 0 ? SceneColours : SuitWorn;

    /// <summary>Has her shown in the colours of the scene named, or, with nothing named, in her own again.</summary>
    public void ShowInSceneColours(string scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        SceneColours = scene;
        Colours.ShowSuit(Colours.SuitOf(SuitShown));
        ShowBeamColours();
    }

    /// <summary>The beams whose colours her pictures draw on, in the order their colourings are named for them.</summary>
    private static readonly (string Equipment, string Name)[] ColouredBeams =
        [(Equipment.Spazer, "spazer"), (Equipment.PlasmaBeam, "plasma"), (Equipment.IceBeam, "ice"), (Equipment.WaveBeam, "wave")];

    /// <summary>How many of those beams she owned when last looked at, to tell one newly hers.</summary>
    private int _beamsOwned;

    /// <summary>How many of those beams she owns.</summary>
    private int BeamsOwned => ColouredBeams.Count(beam => Inventory.Owns(beam.Equipment));

    /// <summary>The glows of a crystal flash, each counting its own colourings, while one colours her.</summary>
    private SamusGlowPlay? _flashBody, _flashBubble;

    /// <summary>
    /// Shows the colours of the beams she has switched on, in place of whatever coloured her beams and of whatever
    /// was laid over them: as she is made ready, as a beam becomes hers, as a pause ends and as a crystal flash's
    /// colouring ends.
    /// </summary>
    public void ShowBeamColours()
    {
        if (SceneColours.Length > 0)
        {
            Colours.ShowBeams(Colours.BeamsOf(SceneColours));
            return;
        }

        Colours.ShowBeams(Colours.BeamsOf(BeamsSwitchedOn));
    }

    /// <summary>
    /// The name of the beams she has switched on, as her colourings and the folders of her shots' pictures are named
    /// for them: the beams' names in their own order, or "power" with none.
    /// </summary>
    public string BeamsSwitchedOn
    {
        get
        {
            string[] on = [.. ColouredBeams.Where(beam => Inventory.IsOn(beam.Equipment)).Select(beam => beam.Name)];
            return on.Length == 0 ? "power" : string.Join('_', on);
        }
    }

    /// <summary>
    /// A crystal flash's colouring, laid over her beams' colours, which her pictures draw on while it lasts: the
    /// bubble's colourings turn and the body's pulse, each on its own count, the bubble's laid first on an update
    /// that reaches both. On the update the flash is done the colours of the beams she has switched on show again.
    /// </summary>
    private void StepCrystalFlashColouring()
    {
        if (_crystalFlashColouring == CrystalFlashColouring.Showing)
        {
            if (_flashBubble?.Step() is { } bubble)
                Colours.LayOverBeams(bubble);
            if (_flashBody?.Step() is { } body)
                Colours.LayOverBeams(body);
        }
        else if (_crystalFlashColouring == CrystalFlashColouring.EndingNext)
        {
            ShowBeamColours();
            (_flashBody, _flashBubble) = (null, null);
        }
    }

    /// <summary>Reads her colours and shows the suit she wears. Called as she is made ready.</summary>
    private void ReadColours()
    {
        _colours = SamusColours.Read(Room.Content.ReadData(ColoursFile));
        Colours.ShowSuit(Colours.SuitOf(SuitShown));
        ShowBeamColours();
        _beamsOwned = BeamsOwned;
        _energyAtLastCheck = Inventory.Amount(Amounts.Energy);
    }

    /// <summary>A shinespark's windup begins: its glow colours her for the windup's length, and a hurt's flash ends.</summary>
    public void BeginSparkColouring()
    {
        _shineGlowPlace = 0;
        _sparkGlow = Countdown.Of(SparkWindupGlowUpdates);
        _hurtFlash = 0;
    }

    /// <summary>A shinespark flying or crashing keeps its glow going for <paramref name="updates"/> more updates.</summary>
    public void KeepSparkGlow(int updates) => _sparkGlow = Countdown.Of(updates);

    /// <summary>A flying shinespark holds a hurt's flash at its own count: no hurt colours her, and none is heard, while it flies.</summary>
    public void HoldHurtFlashForSpark() => _hurtFlash = SparkHurtFlashCount;

    /// <summary>A shinespark's crash ends a hurt's flash.</summary>
    public void EndHurtFlash() => _hurtFlash = 0;

    /// <summary>A shot of hers starts a glow of her suit: a charged shot's flash, or a hyper shot's glow. It takes the place of one still playing.</summary>
    private void BeginShotGlow(string glow) => _shotGlow = new SamusGlowPlay(Colours.Glow(glow));

    /// <summary>
    /// Has her appear where a saved game is taken up (<see cref="AppearingState"/>): she faces out of the screen in
    /// no one's hands but the appearance's, and her suit comes to its colours through the glow of the suit she wears,
    /// which plays from her next update.
    /// </summary>
    public void Appear()
    {
        ChangeState(new AppearingState());
        _appearanceGlow = new SamusGlowPlay(Colours.Glow($"loading_{SuitWorn}"));
    }

    /// <summary>The grapple beam's firing, like a beam shot, and the gift of the hyper beam take back the asking again of a charge's sound.</summary>
    public void ForgetChargingSoundResume() => _chargingSoundArmed = false;

    /// <summary>Something that drains her takes hold of her, or lets her go: while it holds her she flashes, bright on one update and in her suit's colours on the next.</summary>
    public void SetDrainFlash(bool held) => (_drainFlashing, _drainFlashBright) = (held, false);

    /// <summary>
    /// Moves her glows on and shows the colouring her suit takes this update. While a suit is being put on she shows
    /// that suit's colours and nothing glows. The screw attack and the speed booster do not glow with her feet in a
    /// liquid without the gravity suit.
    /// </summary>
    private void StepColours()
    {
        string suit = Colours.SuitOf(SuitShown);
        if (State is CollectingSuitState)
        {
            Colours.ShowSuit(suit);
            return;
        }

        string? appearing = EndingGlowShade(ref _appearanceGlow);
        string? drained = State is DrainedState { Glows: true } glowing ? DrainedGlowShade(glowing.GlowStep) : null;
        string? hurt = StepHurtFlash(suit);
        string? drain = DrainFlashShade(suit);
        string? shine = ShineGlowShade();
        string? screw = ScrewGlowShade();
        string? speed = SpeedGlowShade();
        string? shot = EndingGlowShade(ref _shotGlow);
        string? pulse = ChargePulseShade();
        bool dulled = !Inventory.IsOn(Equipment.GravitySuit) && FeetAreInLiquid;
        Colours.ShowSuit(appearing ?? drained ?? drain ?? hurt ?? shine ?? (dulled ? null : screw ?? speed) ?? shot ?? pulse ?? suit);
        StepCrystalFlashColouring();
    }

    /// <summary>
    /// The colouring a drained Samus shows while she glows: the hyper beam's colourings, one for each step of her
    /// glow, round and round. How long each is held is the drained state's to say.
    /// </summary>
    private string DrainedGlowShade(int step)
    {
        IReadOnlyList<SamusGlowStep> steps = Colours.Glow("hyper_beam_rainbow").Steps;
        return steps[step % steps.Count].Shade;
    }

    /// <summary>
    /// A full charge's pulse: with her grapple beam put away, the next colouring of the pulse on every update, in
    /// the suit she wears, the charged spin's own while her body is that weapon. Short of a full charge, or with the
    /// grapple out, it starts over and shows nothing.
    /// </summary>
    private string? ChargePulseShade()
    {
        if (Grapple is { IsOut: true } || Weapons.ChargedFor < ChargedSpinUpdates)
        {
            _chargePulsePlace = 0;
            return null;
        }

        string pulse = Power(HitKinds.ChargedSpin) is { IsActive: true } ? "pseudo_screw_pulse" : "charged_beam_pulse";
        return Colours.Glow($"{pulse}_{SuitWorn}").Next(ref _chargePulsePlace);
    }

    /// <summary>
    /// A glow that plays once through, a shot's or her appearance's: each of its colourings for its updates, and
    /// nothing once it has shown its last, when it is let go.
    /// </summary>
    private static string? EndingGlowShade(ref SamusGlowPlay? played)
    {
        if (played is not { } glow)
            return null;
        glow.Step();
        if (!glow.HasEnded)
            return glow.Shown;
        played = null;
        return null;
    }

    /// <summary>
    /// The screw attack's glow: its next colouring on every update of a spin with the screw attack on, once the spin
    /// has shown its first picture and while it does not show itself ready to jump off a wall; in a wall jump, from
    /// where its spin begins.
    /// </summary>
    private string? ScrewGlowShade()
    {
        bool glows = Inventory.IsOn(Equipment.ScrewAttack) && State switch
        {
            WallJumpingState or SpinJumpingState { IsOfWallJump: true } when _wallJumpUpdates >= 0 => _wallJumpUpdates >= WallJumpLaunchUpdates,
            SpinJumpingState spin => _spinUpdates >= SpinStartUpdates && !spin.IsReadyForWall,
            _ => false,
        };
        if (glows)
            return Colours.Glow($"screw_attack_{SuitWorn}").Next(ref _screwGlowPlace);
        _screwGlowPlace = 0;
        return null;
    }

    /// <summary>The speed booster's glow: at full boost its colourings in turn, each for its updates, resting on the last.</summary>
    private string? SpeedGlowShade()
    {
        if (!IsBoosting)
        {
            _speedGlow = null;
            return null;
        }

        _speedGlow ??= new SamusGlowPlay(Colours.Glow($"speed_booster_rising_{SuitWorn}"));
        _speedGlow.Step();
        return _speedGlow.Shown;
    }

    /// <summary>
    /// The shine's glow: the shinespark's from its windup until its glow runs out, and otherwise the stored shine's
    /// for as long as a shine is stored, the next colouring on every update.
    /// </summary>
    private string? ShineGlowShade()
    {
        _sparkGlow = _sparkGlow.Stepped();
        string? glow = _sparkGlow.IsRunning ? "active_shinespark" : HasStoredShine ? "stored_shine" : null;
        if (glow is not null)
            return Colours.Glow($"{glow}_{SuitWorn}").Next(ref _shineGlowPlace);
        _shineGlowPlace = 0;
        return null;
    }

    /// <summary>The flash of something draining her: its bright colouring on one update and her suit's on the next, turn about, for as long as it holds her.</summary>
    private string? DrainFlashShade(string suit)
    {
        if (!_drainFlashing)
            return null;
        _drainFlashBright = !_drainFlashBright;
        return _drainFlashBright ? Colours.DrainFlashOf(SuitWorn) : suit;
    }

    /// <summary>
    /// Counts a hurt's flash on and says the colouring it shows: on its count of the sound her hurt is heard; through
    /// its coloured counts the hurt's colouring and her suit's show turn about, the hurt's on the odd counts; the
    /// count it reaches for recovery asks again for the sound the hurt cut short; and it ends on reaching its last.
    /// </summary>
    private string? StepHurtFlash(string suit)
    {
        if (_hurtFlash == 0)
            return null;
        int before = _hurtFlash;

        // Pinned in a beam, thrown back, her hurt is not heard: the flash still counts and colours her. Nor is it heard
        // while a scene shows her in its own colours, as the opening's flashbacks do.
        if (before == HurtFlashSoundCount && SceneColours.Length == 0 && State is not DrainedState { Posed: DrainedState.Pose.Pinned })
            Room.Session.Sounds.Request(Sounds.SamusHurt, channel: "samus");
        int after = ++_hurtFlash;
        if (after == HurtFlashRecoveryCount)
            RecoverSoundAfterHurt();
        if (after >= HurtFlashEndCount)
            _hurtFlash = 0;
        return before > HurtFlashColouredCounts ? null : before % 2 != 0 ? Colours.Hurt : suit;
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

    /// <summary>Whether a knockback has begun since she was last looked at.</summary>
    private bool _knockbackBegan;

    /// <summary>
    /// What is looked at once she has been shown: energy found lost since the last look starts a hurt's flash if none
    /// is running, and a knockback begun since then starts one over whatever count it had reached; and the charge's
    /// sound, if armed, is asked for again while the attack is still held.
    /// </summary>
    private void LookAfterShown()
    {
        int energy = Inventory.Amount(Amounts.Energy);
        if (_knockbackBegan || (energy < _energyAtLastCheck && _hurtFlash == 0))
            _hurtFlash = 1;
        _knockbackBegan = false;
        _energyAtLastCheck = energy;
        if (!_chargingSoundArmed)
            return;
        _chargingSoundArmed = false;
        if (Input.IsHeld(GameAction.Attack))
            Room.Session.Sounds.Request(Sounds.ChargingResumed, channel: "weapons", priority: SoundPriority.High);
    }
}
