using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The power-bomb explosion's two cooperating per-line display effects, represented as one owner.
/// </summary>
/// <remarks>
/// The game uses one effect to advance the explosion's stages and a second to configure
/// the glowing window. The second effect has no independent lifetime: it waits until
/// the first ends both. Keeping one owner therefore exposes the meaningful state without
/// inventing a second animation. Every phase transition and every 8.8 fixed-point radius
/// update below follows the game's explosion stages exactly.
/// </remarks>
public sealed class SamusPowerBombExplosionState
{
    /// <summary>Installed glow colours: installed content, not game state, bound again when a game is restored.</summary>
    public InstalledPowerBombGlowColors? PresentationColors { get; set; }

    /// <summary>Room-pixel centre copied from the projectile when its fuse reaches zero.</summary>
    public Position16<RoomPixels> Position { get; private set; }

    /// <summary>The pre-explosion flash's unsigned 8.8 radius.</summary>
    public PowerBombRadius PreExplosionRadius { get; private set; }

    /// <summary>The damaging explosion's unsigned 8.8 radius.</summary>
    public PowerBombRadius ExplosionRadius { get; internal set; }

    /// <summary>
    /// The per-frame radius change, deliberately shared by the pre-explosion and explosion phases. Cleanup leaves
    /// it in place; the next pre-explosion setup replaces it.
    /// </summary>
    public PowerBombRadius RadiusSpeed { get; private set; }

    /// <summary>The next authored window shape a shape phase consumes, or null when no shape stream is active.</summary>
    public PowerBombShapeCursor? Shape { get; private set; }

    /// <summary>
    /// The window shape the current rendered frame consumed before the cursor advanced, or null when the frame used none.
    /// </summary>
    public PowerBombShapeCursor? RenderedShape { get; private set; }

    /// <summary>Current semantic phase of the explosion's sleeping animation.</summary>
    public PowerBombExplosionPhase Phase { get; private set; }

    /// <summary>
    /// Phase whose per-line window was produced during the current update.
    /// </summary>
    /// <remarks>
    /// A frame behaviour can advance <see cref="Phase"/> after generating its final table.
    /// Keeping the consumed phase separate prevents the host renderer from displaying the
    /// next phase one frame early at the yellow/white transition boundaries.
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
    /// Implements the morph-ball handler's early power-bomb lock before a projectile slot is initialized.
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
        // Starting the explosion does not clear the previous shape. The next effect
        // setup owns its replacement, even when the previous explosion has ended.
        Shape = null;
        RenderedShape = null;
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
        Shape = null;
        RenderedShape = null;
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

        // Every frame behaviour first builds this frame's window and only then
        // mutates its radius/shape position or advances the sleeping animation. Record
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
        Shape = null;
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        GlowColor = RgbColor.Black;
        _afterglowStepsRemaining = 0;
        _crystalFlashAfterglowStepsRemaining = 0;
    }

    /// <summary>Displays the current white flash radius, then slows expansion until the yellow shape list takes over.</summary>
    private void StepPreExplosionWhite()
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.PreExplosion,
            (PreExplosionRadius.WholePixels >> 3) & 15);

        // The white pre-explosion uses addition wrapping at 65536. It subtracts acceleration only while
        // the newly computed radius is still below 37376.
        PreExplosionRadius = AdvanceRadiusWrapping(PreExplosionRadius);
        if (PreExplosionRadius < PowerBombExplosionDefinitions.PreExplosionWhiteLimit)
        {
            RadiusSpeed = RadiusSpeed.WrappingSubtract(PowerBombExplosionDefinitions.PreExplosionAcceleration);
            return;
        }

        // Advancing past the wait runs the yellow stage's setup, installs its radius step,
        // and waits again during this same effect pass.
        Shape = new PowerBombShapeCursor(PowerBombShapeStream.Yellow, 0);
        Phase = PowerBombExplosionPhase.PreExplosionYellow;
    }

    /// <summary>
    /// Consumes the cursor's shape: returns the shape this frame's window is built from and moves the cursor to the
    /// next shape of its stream.
    /// </summary>
    /// <param name="finishedShapes">Whether the consumed shape was the stream's last.</param>
    private PowerBombShapeCursor ConsumeShape(out bool finishedShapes)
    {
        PowerBombShapeCursor shape = Shape ?? throw new InvalidOperationException(
            $"Power Bomb phase {Phase} consumes a window shape with no shape stream active.");
        PowerBombShapeCursor next = shape with { Index = shape.Index + 1 };
        finishedShapes = next.Index == PowerBombShapeCursor.CountOf(shape.Stream);
        Shape = next;
        return shape;
    }

    /// <summary>Advances a radius by the shared speed with the game's wraparound at 65536.</summary>
    private PowerBombRadius AdvanceRadiusWrapping(PowerBombRadius radius) => radius.WrappingAdd(RadiusSpeed);

    /// <summary>
    /// Advances a radius by the shared speed unless the sum passes 65535; the overflow test runs before either
    /// value changes, so an overflowing frame leaves the radius (and the caller's speed update) frozen.
    /// </summary>
    /// <returns>True with the advanced radius when the sum fits.</returns>
    private bool TryAdvanceRadiusWithoutOverflow(PowerBombRadius radius, out PowerBombRadius advanced) =>
        radius.TryAddWithoutCarry(RadiusSpeed, out advanced);

    /// <summary>Consumes each yellow shape before advancing to the next and primes the damaging radius at the last shape.</summary>
    private void StepPreExplosionYellow()
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.PreExplosion,
            (PreExplosionRadius.WholePixels >> 3) & 15);

        // The yellow pre-explosion builds this frame's two window tables before advancing to the next
        // shape. Preserve that consumed shape for a host compositor running
        // after the logic frame has completed.
        RenderedShape = ConsumeShape(out bool finishedShapes);

        // The overflow test is performed on the sum before either state value is changed.
        // If it would exceed 65535, both radius and speed remain frozen for this frame.
        if (TryAdvanceRadiusWithoutOverflow(PreExplosionRadius, out PowerBombRadius nextRadius))
        {
            PreExplosionRadius = nextRadius;
            RadiusSpeed = RadiusSpeed.WrappingSubtract(PowerBombExplosionDefinitions.PreExplosionAcceleration);
        }

        if (!finishedShapes)
            return;

        // The yellow step initializes the actual damaging radius before the next frame's
        // yellow explosion frame behaviour. PreExplosionRadius keeps its own value.
        ExplosionRadius = PowerBombExplosionDefinitions.InitialRadius;
        RadiusSpeed = PowerBombRadius.Zero;
        Phase = PowerBombExplosionPhase.ExplosionYellow;
    }

    /// <summary>Expands the damaging yellow radius with acceleration, then selects white window shapes.</summary>
    private void StepExplosionYellow()
    {
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.WholePixels >> 3);

        ExplosionRadius = AdvanceRadiusWrapping(ExplosionRadius);
        if (ExplosionRadius < PowerBombExplosionDefinitions.ExplosionYellowLimit)
        {
            RadiusSpeed = RadiusSpeed.WrappingAdd(PowerBombExplosionDefinitions.ExplosionAcceleration);
            return;
        }

        // The next step selects the first of seventeen expanding
        // white shape tables, then installs the white explosion and sleeps.
        Shape = new PowerBombShapeCursor(PowerBombShapeStream.White, 0);
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.ExplosionWhite;
    }

    /// <summary>Consumes seventeen white shapes and primes the 32-step afterglow that fades the glow.</summary>
    private void StepExplosionWhite()
    {
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.WholePixels >> 3);

        RenderedShape = ConsumeShape(out bool finishedShapes);

        if (TryAdvanceRadiusWithoutOverflow(ExplosionRadius, out PowerBombRadius nextRadius))
        {
            ExplosionRadius = nextRadius;
            RadiusSpeed = RadiusSpeed.WrappingAdd(PowerBombExplosionDefinitions.ExplosionAcceleration);
        }

        if (!finishedShapes)
            return;

        // The white explosion primes 32 afterglow steps. The explosion then installs the afterglow
        // frame behaviour; it fades the first step on the following frame.
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
            Shape = null;
            RenderedShape = null;
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
            ExplosionRadius.WholePixels >> 3);

        ExplosionRadius = AdvanceRadiusWrapping(ExplosionRadius);
        if (ExplosionRadius < PowerBombExplosionDefinitions.CrystalFlashRadiusLimit)
        {
            RadiusSpeed = RadiusSpeed.WrappingAdd(PowerBombExplosionDefinitions.ExplosionAcceleration);
            return;
        }

        // As with the ordinary afterglow, the Crystal Flash afterglow fades a step on every active frame behaviour.
        _crystalFlashAfterglowStepsRemaining =
            PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps(RenderedExplosionRadius);
        Phase = PowerBombExplosionPhase.CrystalFlashAfterglow;
    }

    /// <summary>Fades Crystal Flash for its compiled lifetime, then clears the shared Power Bomb state.</summary>
    private bool StepCrystalFlashAfterglow()
    {
        // Crystal Flash's afterglow wakes once the stock components reach zero, fading a step every update as the
        // ordinary afterglow does. The compiled operand tracks the same number of steps; editable colors only affect
        // the fade displayed inside the retained final window.
        if (_crystalFlashAfterglowStepsRemaining != 0)
        {
            _crystalFlashAfterglowStepsRemaining--;
            GlowColor = GlowColor.FadeLevels.Map((_, channel) => channel == 0 ? 0 : channel - 1).ToColour();
            return false;
        }

        // The awakened effect immediately ends both per-line effects and clears every
        // shared Power Bomb value. The lock is already released by Crystal Flash's start,
        // but assigning it again mirrors the cleanup and makes reset atomic.
        IsArmed = false;
        IsActive = false;
        PreExplosionRadius = PowerBombRadius.Zero;
        ExplosionRadius = PowerBombRadius.Zero;
        RadiusSpeed = PowerBombRadius.Zero;
        Shape = null;
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        // Replacement colors may be brighter than the stock lifetime operand. Once
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

    /// <summary>Selects the installed glow colour a sequence shows at a radius-derived step.</summary>
    private void SelectGlowColor(PowerBombGlowSequence sequence, int colorIndex)
    {
        RgbColor color = (PresentationColors ?? throw new InvalidOperationException(
                "The Power Bomb's glow requires installed glow colours."))
            .Resolve(sequence, colorIndex);
        GlowColor = color;
    }
}
