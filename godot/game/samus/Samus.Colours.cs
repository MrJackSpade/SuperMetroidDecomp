using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// Her colour step: once an update, after she has moved and just before she is shown, what colours her takes its turn
/// in the cartridge's order, and each that takes her colours keeps those after it from theirs. The count of a hurt's
/// flash lives here with the sounds it times, and the stored shine's time with the glow it shows.
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

    /// <summary>The suit whose colours were last looked at for a change of what she wears.</summary>
    private string _suitWorn = "";

    /// <summary>Which of the shine's two glows colours her.</summary>
    private enum ShineColouring
    {
        /// <summary>Neither.</summary>
        None,

        /// <summary>The stored shine's, for as long as the shine is stored.</summary>
        Stored,

        /// <summary>The shinespark's, from its windup until its glow runs out.</summary>
        Spark,
    }

    private ShineColouring _shineColouring;

    /// <summary>How many more updates the shinespark's glow lasts.</summary>
    private Countdown _sparkGlow = Countdown.Over;

    /// <summary>Where the shine's glow stands in its colourings; the stored shine's and the shinespark's each start it over.</summary>
    private int _shineGlowPlace;

    /// <summary>Where the pulse of a full charge stands in its colourings; it starts over whenever she is not fully charged.</summary>
    private int _chargePulsePlace;

    /// <summary>The glow a charged shot or a hyper shot started, until the update after its last colouring; none otherwise.</summary>
    private SamusGlowPlay? _shotGlow;

    /// <summary>Whether something that drains her has hold of her, and which colouring its flash shows next.</summary>
    private bool _drainFlashing, _drainFlashShowsSuitNext;

    /// <summary>Whether the charge's sound is to be asked for again at the next look after she is shown.</summary>
    private bool _chargingSoundArmed;

    /// <summary>The suit whose colours she shows: the gravity suit's before the varia suit's, and the power suit's with neither on.</summary>
    public string SuitWorn => Inventory.IsOn(Equipment.GravitySuit) ? "gravity" : Inventory.IsOn(Equipment.VariaSuit) ? "varia" : "power";

    /// <summary>Shows the colours of the suit she wears, in place of whatever coloured her.</summary>
    public void ShowSuitColours() => Colours.ShowSuit(Colours.SuitOf(SuitWorn));

    /// <summary>Reads her colours and shows the suit she wears. Called as she is made ready.</summary>
    private void ReadColours()
    {
        _colours = SamusColours.Read(Room.Content.ReadData(ColoursFile));
        _suitWorn = SuitWorn;
        ShowSuitColours();
        _energyAtLastCheck = Inventory.Amount(Amounts.Energy);
    }

    /// <summary>
    /// A change of the suit she wears shows that suit's colours, as putting one on or off on the pause screen does.
    /// Collecting one shows them itself.
    /// </summary>
    private void ShowSuitIfChanged()
    {
        string worn = SuitWorn;
        if (worn == _suitWorn)
            return;
        _suitWorn = worn;
        if (State is not CollectingSuitState)
            ShowSuitColours();
    }

    /// <summary>The stored shine's glow begins, from its first colouring.</summary>
    private void BeginStoredShineColouring() => (_shineColouring, _shineGlowPlace) = (ShineColouring.Stored, 0);

    /// <summary>
    /// The stored shine's glow is taken from her as the shine is spent or given up: her suit's colours show until
    /// whatever took it colours her.
    /// </summary>
    private void EndStoredShineColouring()
    {
        if (_shineColouring != ShineColouring.Stored)
            return;
        (_shineColouring, _shineGlowPlace) = (ShineColouring.None, 0);
        ShowSuitColours();
    }

    /// <summary>A shinespark's windup begins: its glow takes her colours for the windup's length, and a hurt's flash ends.</summary>
    public void BeginSparkColouring()
    {
        (_shineColouring, _shineGlowPlace) = (ShineColouring.Spark, 0);
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

    /// <summary>The grapple beam's firing, like a beam shot, and the gift of the hyper beam take back the asking again of a charge's sound.</summary>
    public void ForgetChargingSoundResume() => _chargingSoundArmed = false;

    /// <summary>
    /// What boards an elevator with her ends a shot's glow and starts the charge's pulse over, leaving her colours as
    /// they are, as the cartridge clears her shots there.
    /// </summary>
    public void ForgetShotGlow() => (_shotGlow, _chargePulsePlace) = (null, 0);

    /// <summary>
    /// A charged spin's touch on an enemy uses its charge up, as the cartridge does before it asks whether the touch
    /// harms: the charge is let go, its pulse starts over and her suit's colours show at once.
    /// </summary>
    private void SpendChargedSpin()
    {
        Weapons.DropCharge();
        _chargePulsePlace = 0;
        ShowSuitColours();
    }

    /// <summary>Something that drains her takes hold of her, or lets her go: while it holds her she flashes, one colouring an update.</summary>
    public void SetDrainFlash(bool held) => (_drainFlashing, _drainFlashShowsSuitNext) = (held, false);

    /// <summary>
    /// Plays her colour step. It does not run while a suit is being put on or she is dying. In the cartridge's order:
    /// the charge's colours, which on the update a shot's glow ends keep the shine's glow from its turn; the shine's
    /// glow; then the flash of whatever drains her, which keeps a hurt's flash from its turn; then a hurt's flash.
    /// </summary>
    private void StepColours()
    {
        if (State is CollectingSuitState or DyingState)
            return;
        if (!StepChargeColouring())
            StepShineColouring();
        if (StepDrainFlash())
            return;
        StepHurtFlash();
    }

    /// <summary>
    /// The charge's colours. With no shot's glow playing, a full charge with her grapple beam put away shows the next
    /// colouring of its pulse on every update, in the suit she wears, the charged spin's own while her body is that
    /// weapon; short of a full charge, or with the grapple out, the pulse starts over. A shot's glow shows each of
    /// its colourings for its updates, and on the update after its last her suit's colours come back. Returns whether
    /// this was that update: nothing else that glows takes its turn on it.
    /// </summary>
    private bool StepChargeColouring()
    {
        if (_shotGlow is not { } glow)
        {
            if (Grapple is { IsOut: true } || Weapons.ChargedFor < ChargedSpinUpdates)
            {
                _chargePulsePlace = 0;
                return false;
            }

            string pulse = Power(HitKinds.ChargedSpin) is { IsActive: true } ? "pseudo_screw_pulse" : "charged_beam_pulse";
            if (Colours.Glow($"{pulse}_{SuitWorn}").Next(ref _chargePulsePlace) is { } pulsed)
                Colours.ShowSuit(pulsed);
            return false;
        }

        string? shade = glow.Step();
        if (glow.HasEnded)
        {
            _shotGlow = null;
            ShowSuitColours();
            return true;
        }

        if (shade is not null)
            Colours.ShowSuit(shade);
        return false;
    }

    /// <summary>
    /// The stored shine's and the shinespark's glow. The stored shine's time is counted here and nowhere else, so it
    /// stands still on an update her colour step does not run; its sound is asked for here, a while after it is stored.
    /// As either runs out her suit's colours show again.
    /// </summary>
    private void StepShineColouring()
    {
        switch (_shineColouring)
        {
            case ShineColouring.None:
                return;
            case ShineColouring.Stored:
                if (_shine.Remaining == StoredShineUpdates - StoredShineSoundUpdates)
                    Room.Session.Sounds.Request(Sounds.StoredShine, channel: "samus", priority: SoundPriority.High);
                _shine = _shine.Stepped();
                if (_shine.IsRunning)
                {
                    ShowNextShineShade("stored_shine");
                    return;
                }

                break;
            case ShineColouring.Spark:
                _sparkGlow = _sparkGlow.Stepped();
                if (_sparkGlow.IsRunning)
                {
                    ShowNextShineShade("active_shinespark");
                    return;
                }

                break;
            default:
                throw new InvalidOperationException($"Undefined shine colouring {_shineColouring}.");
        }

        (_shineColouring, _shineGlowPlace) = (ShineColouring.None, 0);
        ShowSuitColours();
    }

    /// <summary>Shows the next colouring of one of the shine's glows, in the suit she wears, from where the glow stands.</summary>
    private void ShowNextShineShade(string glow)
    {
        if (Colours.Glow($"{glow}_{SuitWorn}").Next(ref _shineGlowPlace) is { } shade)
            Colours.ShowSuit(shade);
    }

    /// <summary>
    /// The flash of something draining her: the speed booster's bright colouring on one update and her suit's on the
    /// next, turn about, for as long as it holds her. Returns whether it took her colours.
    /// </summary>
    private bool StepDrainFlash()
    {
        if (!_drainFlashing)
            return false;
        if (_drainFlashShowsSuitNext)
            ShowSuitColours();
        else
            Colours.ShowSuit(Colours.DrainFlashOf(SuitWorn));
        _drainFlashShowsSuitNext = !_drainFlashShowsSuitNext;
        return true;
    }

    /// <summary>
    /// A hurt's flash: on its count of the sound her hurt is heard; through its coloured counts the hurt's colouring
    /// and her suit's show turn about, the hurt's on the odd counts; the count it reaches for recovery asks again for
    /// the sound the hurt cut short; and it ends on reaching its last.
    /// </summary>
    private void StepHurtFlash()
    {
        if (_hurtFlash == 0)
            return;
        int before = _hurtFlash;
        if (before == HurtFlashSoundCount)
            Room.Session.Sounds.Request(Sounds.SamusHurt, channel: "samus");
        if (before <= HurtFlashColouredCounts)
        {
            if (before % 2 != 0)
                Colours.ShowSuit(Colours.Hurt);
            else
                ShowSuitColours();
        }

        int after = ++_hurtFlash;
        if (after == HurtFlashRecoveryCount)
            RecoverSoundAfterHurt();
        if (after >= HurtFlashEndCount)
            _hurtFlash = 0;
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

        if (State is SpinJumpingState spin)
            Room.Session.Sounds.Request(spin.ShownSound, channel: "samus", priority: SoundPriority.High);
        else if (State is not WallJumpingState && ChargeIsSounding && Input.IsHeld(GameAction.Attack))
            _chargingSoundArmed = true;
    }

    /// <summary>
    /// What the cartridge does once she has been shown: energy found lost since the last look starts a hurt's flash
    /// if none is running, and the charge's sound, if armed, is asked for again while the attack is still held.
    /// </summary>
    private void LookAfterShown()
    {
        int energy = Inventory.Amount(Amounts.Energy);
        if (energy < _energyAtLastCheck && _hurtFlash == 0)
            _hurtFlash = 1;
        _energyAtLastCheck = energy;
        if (!_chargingSoundArmed)
            return;
        _chargingSoundArmed = false;
        if (Input.IsHeld(GameAction.Attack))
            Room.Session.Sounds.Request(Sounds.ChargingResumed, channel: "weapons", priority: SoundPriority.High);
    }
}
