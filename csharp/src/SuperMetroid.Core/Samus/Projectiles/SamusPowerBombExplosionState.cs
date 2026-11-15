using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Power Bomb's blast: its stages, their radii and durations, and the oval of added colour each frame shows
/// (<see cref="ShownOval"/>).
/// </summary>
/// <remarks>
/// The game ran the blast's stages and its glowing window as two cooperating effects, the second with no lifetime of its
/// own; one owner keeps the meaningful state. Every phase transition and every radius update below follows
/// the game's explosion stages exactly: the radii and their speed are plain lengths
/// (<see cref="PowerBombRadius"/>), and the oval stages' growth holds short of 256 pixels
/// (<see cref="PowerBombGrowthHold"/>).
/// </remarks>
public sealed class SamusPowerBombExplosionState
{
    /// <summary>Installed glow colours: installed content, not game state, bound again when a game is restored.</summary>
    public InstalledPowerBombGlowColors? PresentationColors { get; set; }

    /// <summary>Room-pixel centre copied from the projectile when its fuse reaches zero.</summary>
    public Position16<RoomPixels> Position { get; private set; }

    /// <summary>The pre-explosion flash's radius.</summary>
    public PowerBombRadius PreExplosionRadius { get; private set; }

    /// <summary>The damaging explosion's radius.</summary>
    public PowerBombRadius ExplosionRadius { get; internal set; }

    /// <summary>
    /// The per-frame radius change, deliberately shared by the pre-explosion and explosion phases. Cleanup leaves
    /// it in place; the next pre-explosion setup replaces it.
    /// </summary>
    public PowerBombRadius RadiusSpeed { get; private set; }

    /// <summary>
    /// The updates the yellow pre-explosion or the white explosion has run so far (#1927): each shows the oval its
    /// update selects, and the stage ends after its duration (<see cref="PowerBombExplosionDefinitions.PreExplosionYellowUpdates"/>,
    /// <see cref="PowerBombExplosionDefinitions.ExplosionWhiteUpdates"/>).
    /// </summary>
    private int _ovalStageUpdates;

    /// <summary>
    /// The horizontal radius, in pixels, of the authored oval the current frame shows, or null when the frame shows the
    /// blast's own radius instead (#1927). The afterglow keeps the white explosion's last oval.
    /// </summary>
    public int? RenderedOvalRadius { get; private set; }

    /// <summary>
    /// The oval the current frame shows (#1927): centred on the blast, the authored oval of the yellow pre-explosion or
    /// white explosion and the afterglow that keeps the last of them, otherwise the blast's own radius as the frame
    /// built it, and the glow's colour. A blast that spawned after the frame chose its oval shows nothing until the
    /// next frame.
    /// </summary>
    public PowerBombBlastOval ShownOval => PowerBombBlastOval.Of(Position, ShownOvalRadius(), GlowColor);

    /// <summary>The horizontal radius, in whole pixels, of <see cref="ShownOval"/>; zero shows nothing.</summary>
    /// <exception cref="InvalidOperationException">The shown phase is undefined.</exception>
    private int ShownOvalRadius() => RenderedPhase switch
    {
        PowerBombExplosionPhase.Inactive => 0,
        PowerBombExplosionPhase.PreExplosionWhite => RenderedPreExplosionRadius.WholePixels,
        PowerBombExplosionPhase.PreExplosionYellow or PowerBombExplosionPhase.ExplosionWhite or PowerBombExplosionPhase.Afterglow =>
            RenderedOvalRadius ?? RenderedExplosionRadius.WholePixels,
        PowerBombExplosionPhase.PendingPreExplosionSetup or PowerBombExplosionPhase.ExplosionYellow or
            PowerBombExplosionPhase.CrystalFlashExplosion or PowerBombExplosionPhase.CrystalFlashAfterglow =>
            RenderedExplosionRadius.WholePixels,
        _ => throw new InvalidOperationException($"Undefined {nameof(PowerBombExplosionPhase)} {RenderedPhase}."),
    };

    /// <summary>Current semantic phase of the explosion's resting animation.</summary>
    public PowerBombExplosionPhase Phase { get; private set; }

    /// <summary>
    /// Phase whose oval the current update chose.
    /// </summary>
    /// <remarks>
    /// An update can advance <see cref="Phase"/> after choosing its oval. Keeping the shown phase separate prevents the
    /// display from showing the next phase one frame early at the yellow/white transition boundaries.
    /// </remarks>
    public PowerBombExplosionPhase RenderedPhase { get; internal set; }

    /// <summary>Pre-explosion radius consumed before the end-of-frame update.</summary>
    public PowerBombRadius RenderedPreExplosionRadius { get; internal set; }

    /// <summary>Explosion radius consumed before the end-of-frame update.</summary>
    public PowerBombRadius RenderedExplosionRadius { get; internal set; }

    /// <summary>The explosion's glow: the colour it adds to the scene inside its window.</summary>
    public RgbColor GlowColor { get; internal set; }

    /// <summary>True from placement until cleanup releases the one-at-a-time Power Bomb lock.</summary>
    public bool IsArmed { get; private set; }

    /// <summary>
    /// True from the explosion's spawn until its animation ends: X-ray, the pause check, Shaktool's room, Mother Brain
    /// and the sound queues test it.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Afterglow steps left, 32 when the final white-explosion frame primes them; each step consumes one, and the
    /// step that reaches zero ends the explosion instead of fading the colors. It is only stepped while positive.
    /// </summary>
    private int _afterglowStepsRemaining;

    /// <summary>
    /// Crystal Flash completion count, kept separate from the editable colour so
    /// rebinding colors cannot change the effect's cleanup frame after restoration.
    /// </summary>
    private int _crystalFlashAfterglowStepsRemaining;

    /// <summary>
    /// Implements the morph ball's early power-bomb lock before a Power Bomb takes its place.
    /// </summary>
    public void Arm()
    {
        IsArmed = true;
    }

    /// <summary>
    /// Implements the explosion's spawn after the fuse has copied the projectile's world position.
    /// </summary>
    public void Spawn(Position16<RoomPixels> position)
    {
        Position = position;
        IsActive = true;

        // The explosion starts after this update's effect pass. The NEXT pass runs its
        // setup and installs the radius step; that step cannot run until the pass after
        // setup. Executing setup here would skip one update.
        PreExplosionRadius = PowerBombRadius.Zero;
        ExplosionRadius = PowerBombRadius.Zero;
        _ovalStageUpdates = 0;
        RenderedOvalRadius = null;
        Phase = PowerBombExplosionPhase.PendingPreExplosionSetup;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        _afterglowStepsRemaining = 0;
        _crystalFlashAfterglowStepsRemaining = 0;
        GlowColor = RgbColor.Black;
    }

    /// <summary>
    /// Implements Crystal Flash's explosion request and both setup calls after
    /// Crystal Flash completes its ten-step rise.
    /// </summary>
    public void BeginCrystalFlash(Position16<RoomPixels> position)
    {
        // Crystal Flash releases the one-at-a-time Power Bomb lock before starting its two
        // explosion effects. Their animation still shares the Power Bomb's status,
        // position, radius, speed, glow colour, and window.
        IsArmed = false;
        Position = position;
        IsActive = true;
        PreExplosionRadius = PowerBombExplosionDefinitions.InitialRadius;
        ExplosionRadius = PowerBombExplosionDefinitions.InitialRadius;
        RadiusSpeed = PowerBombRadius.Zero;
        _ovalStageUpdates = 0;
        RenderedOvalRadius = null;
        Phase = PowerBombExplosionPhase.CrystalFlashExplosion;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        GlowColor = RgbColor.Black;
        _afterglowStepsRemaining = 0;
        _crystalFlashAfterglowStepsRemaining = 0;
    }

    /// <summary>
    /// Runs one update of the explosion's active stage step.
    /// </summary>
    /// <returns>True only on the frame that cleanup completes.</returns>
    public bool StepFrame()
    {
        if (!IsActive)
            return false;

        // Every stage first chooses this frame's oval and only then
        // changes its radius or its oval stage's updates, or advances the stage. Record
        // that input phase here so post-logic desktop composition sees the generated
        // frame rather than the state prepared for the following one.
        RenderedPhase = Phase;

        switch (Phase)
        {
            case PowerBombExplosionPhase.PendingPreExplosionSetup:
                PreExplosionRadius = PowerBombExplosionDefinitions.InitialRadius;
                RadiusSpeed = PowerBombExplosionDefinitions.InitialPreExplosionSpeed;
                Phase = PowerBombExplosionPhase.PreExplosionWhite;
                RenderedPhase = PowerBombExplosionPhase.Inactive;
                return false;

            case PowerBombExplosionPhase.PreExplosionWhite:
                StepPreExplosionWhite();
                return false;

            case PowerBombExplosionPhase.PreExplosionYellow:
                StepPreExplosionYellow();
                return false;

            case PowerBombExplosionPhase.ExplosionYellow:
                StepExplosionYellow();
                return false;

            case PowerBombExplosionPhase.ExplosionWhite:
                StepExplosionWhite();
                return false;

            case PowerBombExplosionPhase.Afterglow:
                return StepAfterglow();

            case PowerBombExplosionPhase.CrystalFlashExplosion:
                StepCrystalFlashExplosion();
                return false;

            case PowerBombExplosionPhase.CrystalFlashAfterglow:
                return StepCrystalFlashAfterglow();

            case PowerBombExplosionPhase.Inactive:
            default:
                throw new InvalidOperationException($"Active power-bomb status has invalid phase {Phase}.");
        }
    }

    /// <summary>
    /// Releases the armed flag after cleanup when Crystal Flash did not take ownership.
    /// </summary>
    public void ReleaseFlag() => IsArmed = false;

    /// <summary>Clears every explosion field during room/runtime reset.</summary>
    public void Reset()
    {
        IsArmed = false;
        IsActive = false;
        Position = Position16<RoomPixels>.Zero;
        PreExplosionRadius = PowerBombRadius.Zero;
        ExplosionRadius = PowerBombRadius.Zero;
        RadiusSpeed = PowerBombRadius.Zero;
        _ovalStageUpdates = 0;
        RenderedOvalRadius = null;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        GlowColor = RgbColor.Black;
        _afterglowStepsRemaining = 0;
        _crystalFlashAfterglowStepsRemaining = 0;
    }

    /// <summary>Displays the current white flash radius, then slows expansion until the yellow pre-explosion takes over.</summary>
    private void StepPreExplosionWhite()
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.PreExplosion,
            PreExplosionGlowStep(PreExplosionRadius));

        // The white pre-explosion grows the radius by its speed and slows only while the grown radius is still
        // below the white limit.
        PreExplosionRadius += RadiusSpeed;
        if (PreExplosionRadius < PowerBombExplosionDefinitions.PreExplosionWhiteLimit)
        {
            RadiusSpeed -= PowerBombExplosionDefinitions.PreExplosionSlowing;
            return;
        }

        // Advancing past the wait runs the yellow stage's setup, installs its radius step,
        // and waits again during this same effect pass.
        _ovalStageUpdates = 0;
        Phase = PowerBombExplosionPhase.PreExplosionYellow;
    }

    /// <summary>
    /// Runs one update of an oval stage: returns the authored oval radius this update shows and counts the update.
    /// </summary>
    /// <param name="radii">The stage's oval radius for each of its updates.</param>
    /// <param name="duration">The updates the stage lasts.</param>
    /// <param name="stageEnds">Whether this update was the stage's last.</param>
    /// <exception cref="InvalidOperationException">The stage has already run its whole duration.</exception>
    private int ShowNextOval(IReadOnlyList<int> radii, int duration, out bool stageEnds)
    {
        if (_ovalStageUpdates >= duration)
            throw new InvalidOperationException(
                $"Power Bomb phase {Phase} ran past its {duration} updates.");
        int radius = radii[_ovalStageUpdates];
        _ovalStageUpdates++;
        stageEnds = _ovalStageUpdates == duration;
        return radius;
    }

    /// <summary>
    /// Grows a radius by the shared speed unless the growth would reach 256 pixels (<see cref="PowerBombGrowthHold"/>);
    /// the test runs before either value changes, so a held update leaves the radius (and the caller's speed change) as
    /// they were.
    /// </summary>
    /// <returns>True with the grown radius when the radius grows.</returns>
    private bool TryGrowRadius(PowerBombRadius radius, out PowerBombRadius grown)
    {
        if (PowerBombGrowthHold.Holds(radius.WholePixelsGrownBy(RadiusSpeed)))
        {
            grown = radius;
            return false;
        }

        grown = radius + RadiusSpeed;
        return true;
    }

    /// <summary>Shows each yellow oval in turn and primes the damaging radius after the stage's last update.</summary>
    private void StepPreExplosionYellow()
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.PreExplosion,
            PreExplosionGlowStep(PreExplosionRadius));

        // The yellow pre-explosion chooses this frame's oval before the stage counts the update, and the display shows
        // the chosen one after the logic frame has completed.
        RenderedOvalRadius = ShowNextOval(PowerBombExplosionDefinitions.YellowOvalRadii,
            PowerBombExplosionDefinitions.PreExplosionYellowUpdates, out bool stageEnds);

        // The hold is tested on the growth before either value changes: a growth that would reach 256 pixels leaves
        // both the radius and its speed as they were for this frame.
        if (TryGrowRadius(PreExplosionRadius, out PowerBombRadius nextRadius))
        {
            PreExplosionRadius = nextRadius;
            RadiusSpeed -= PowerBombExplosionDefinitions.PreExplosionSlowing;
        }

        if (!stageEnds)
            return;

        // The yellow step initializes the actual damaging radius before the next frame's
        // yellow explosion update. PreExplosionRadius keeps its own value.
        ExplosionRadius = PowerBombExplosionDefinitions.InitialRadius;
        RadiusSpeed = PowerBombRadius.Zero;
        Phase = PowerBombExplosionPhase.ExplosionYellow;
    }

    /// <summary>Expands the damaging yellow radius with acceleration, then starts the white explosion's ovals.</summary>
    private void StepExplosionYellow()
    {
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.GlowColorBand);

        ExplosionRadius += RadiusSpeed;
        if (ExplosionRadius < PowerBombExplosionDefinitions.ExplosionYellowLimit)
        {
            RadiusSpeed += PowerBombExplosionDefinitions.ExplosionAcceleration;
            return;
        }

        // The white explosion starts its seventeen expanding ovals on the next update.
        _ovalStageUpdates = 0;
        RenderedOvalRadius = null;
        Phase = PowerBombExplosionPhase.ExplosionWhite;
    }

    /// <summary>Shows the seventeen white ovals and primes the 32-step afterglow that fades the glow.</summary>
    private void StepExplosionWhite()
    {
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.GlowColorBand);

        RenderedOvalRadius = ShowNextOval(PowerBombExplosionDefinitions.WhiteOvalRadii,
            PowerBombExplosionDefinitions.ExplosionWhiteUpdates, out bool stageEnds);

        if (TryGrowRadius(ExplosionRadius, out PowerBombRadius nextRadius))
        {
            ExplosionRadius = nextRadius;
            RadiusSpeed += PowerBombExplosionDefinitions.ExplosionAcceleration;
        }

        if (!stageEnds)
            return;

        // The white explosion primes 32 afterglow steps. The afterglow then fades the first step on the following
        // frame.
        _afterglowStepsRemaining = 32;
        Phase = PowerBombExplosionPhase.Afterglow;
    }

    /// <summary>
    /// Fades the glow colour's three components one step every update and ends the effect after the last step. The delay
    /// the game keeps between steps never holds one back: the afterglow starts it at zero, so it runs past zero on the
    /// first update, and each step's reload of its count leaves it run past zero.
    /// </summary>
    private bool StepAfterglow()
    {
        // A count reaching one advances the animation without one final color decrement.
        ConsumeAfterglowStep();
        if (_afterglowStepsRemaining == 0)
        {
            IsActive = false;
            PreExplosionRadius = PowerBombRadius.Zero;
            ExplosionRadius = PowerBombRadius.Zero;
            // Cleanup does not clear the shared radius growth speed. Retain it
            // until the next pre-explosion setup, as on the game.
            _ovalStageUpdates = 0;
            RenderedOvalRadius = null;
            Phase = PowerBombExplosionPhase.Inactive;
            RenderedPhase = PowerBombExplosionPhase.Inactive;
            RenderedPreExplosionRadius = PowerBombRadius.Zero;
            RenderedExplosionRadius = PowerBombRadius.Zero;
            return true;
        }

        GlowColor = GlowColor.FadeLevels.Map((_, channel) => channel == 0 ? 0 : channel - 1).ToColour();
        return false;
    }

    /// <summary>Uses the explosion curve to expand Crystal Flash only to its smaller radius limit.</summary>
    private void StepCrystalFlashExplosion()
    {
        // Crystal Flash's explosion is a deliberately shorter clone of the yellow Power Bomb expansion.
        // It uses the same curve and glow colours, but stops at radius 32.
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.GlowColorBand);

        ExplosionRadius += RadiusSpeed;
        if (ExplosionRadius < PowerBombExplosionDefinitions.CrystalFlashRadiusLimit)
        {
            RadiusSpeed += PowerBombExplosionDefinitions.ExplosionAcceleration;
            return;
        }

        // As with the ordinary afterglow, the Crystal Flash afterglow fades a step on every update.
        _crystalFlashAfterglowStepsRemaining =
            PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps(RenderedExplosionRadius);
        Phase = PowerBombExplosionPhase.CrystalFlashAfterglow;
    }

    /// <summary>Fades Crystal Flash for its compiled lifetime, then clears the shared Power Bomb state.</summary>
    private bool StepCrystalFlashAfterglow()
    {
        // Crystal Flash's afterglow wakes once the stock components reach zero, fading a step every update as the
        // ordinary afterglow does. The compiled count tracks the same number of steps; editable colors only affect
        // the fade displayed inside the retained final window.
        if (_crystalFlashAfterglowStepsRemaining != 0)
        {
            _crystalFlashAfterglowStepsRemaining--;
            GlowColor = GlowColor.FadeLevels.Map((_, channel) => channel == 0 ? 0 : channel - 1).ToColour();
            return false;
        }

        // The awakened effect immediately ends the blast and clears every
        // shared Power Bomb value. The lock is already released by Crystal Flash's start,
        // but assigning it again mirrors the cleanup and makes reset atomic.
        IsArmed = false;
        IsActive = false;
        PreExplosionRadius = PowerBombRadius.Zero;
        ExplosionRadius = PowerBombRadius.Zero;
        RadiusSpeed = PowerBombRadius.Zero;
        _ovalStageUpdates = 0;
        RenderedOvalRadius = null;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        // Replacement colors may be brighter than the stock lifetime count. Once
        // the window is released, do not retain their unused display remainder.
        GlowColor = RgbColor.Black;
        return true;
    }

    /// <summary>
    /// Consumes one afterglow step. The afterglow phase is entered with 32 steps and ends on the step that reaches
    /// zero, so a step is never taken from zero; one would mean the phase and its count disagree.
    /// </summary>
    private void ConsumeAfterglowStep()
    {
        if (_afterglowStepsRemaining <= 0)
            throw new InvalidOperationException(
                $"Power Bomb afterglow stepped with {_afterglowStepsRemaining} steps remaining; the phase ends when the count reaches zero.");
        _afterglowStepsRemaining--;
    }

    /// <summary>
    /// The pre-explosion colour a flash of <paramref name="radius"/> shows: its glow band, the sixteen colours repeating
    /// from 128 pixels on. The repeat is load-bearing: the white flash ends at 146 pixels and its yellow updates grow it
    /// toward 255, so its last colours are the sequence's first ones again.
    /// </summary>
    private static int PreExplosionGlowStep(PowerBombRadius radius) =>
        radius.GlowColorBand % PowerBombGlowColorFormat.Count(PowerBombGlowSequence.PreExplosion);

    /// <summary>Selects the installed glow colour a sequence shows at a radius-derived step.</summary>
    private void SelectGlowColor(PowerBombGlowSequence sequence, int glowStep)
    {
        RgbColor color = (PresentationColors ?? throw new InvalidOperationException(
                "The Power Bomb's glow requires installed glow colours."))
            .Resolve(sequence, glowStep);
        GlowColor = color;
    }
}
