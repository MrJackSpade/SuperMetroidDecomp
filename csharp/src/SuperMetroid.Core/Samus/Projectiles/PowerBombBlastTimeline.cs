using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;

using SuperMetroid.Core.Assets;namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Power Bomb blast's timeline (#2000): its stages, their radii and durations, the radius growth they share, and the
/// glow each update shows. It has no place of its own: Samus's Power Bomb explosion centres it in the room
/// (<see cref="SamusPowerBombExplosionState"/>), and the blast that destroys Ceres station centres it on the screen.
/// </summary>
/// <remarks>
/// One timeline runs the blast's stages and its glowing oval. Every phase transition and every radius update below follows
/// the game's explosion stages exactly: the radii and their speed are plain lengths
/// (<see cref="PowerBombRadius"/>), and the oval stages' growth holds short of the radius's range
/// (<see cref="PowerBombGrowthHold"/>). Crystal Flash's smaller blast burns through the same radii, speed and glow.
/// </remarks>
public sealed class PowerBombBlastTimeline
{
    /// <summary>The pre-explosion flash's radius.</summary>
    private PowerBombRadius PreExplosionRadius { get; set; }

    /// <summary>The damaging explosion's radius.</summary>
    public PowerBombRadius ExplosionRadius { get; private set; }

    /// <summary>
    /// The per-update radius change of the running stage, shared by the pre-explosion and explosion phases: each start
    /// sets it before any stage reads it, so a finished blast keeps none.
    /// </summary>
    private PowerBombRadius _radiusSpeed;

    /// <summary>
    /// The play of the yellow pre-explosion's or the white explosion's ovals (#1927, #2529, #2553): each update shows the
    /// next oval, and the stage ends on the update that shows its last. Each stage starts its ovals as it is entered; no
    /// other stage reads them.
    /// </summary>
    private PowerBombOvalPlay _ovals = PowerBombOvalPlay.Starting(PowerBombExplosionDefinitions.YellowOvalRadii);

    /// <summary>
    /// The horizontal radius, in pixels, of the authored oval the current frame shows, or null when the frame shows the
    /// blast's own radius instead (#1927). The afterglow keeps the white explosion's last oval.
    /// </summary>
    private int? RenderedOvalRadius { get; set; }

    /// <summary>
    /// The horizontal radius, in whole pixels, of the oval the current frame shows (#1927); zero shows nothing. It is
    /// the authored oval of the yellow pre-explosion or white explosion and the afterglow that keeps the last of them,
    /// otherwise the blast's own radius as the frame built it. A blast that started after the frame chose its oval shows
    /// nothing until the next frame.
    /// </summary>
    public int ShownRadius => RenderedPhase switch
    {
        PowerBombExplosionPhase.Inactive => 0,
        PowerBombExplosionPhase.Running.PreExplosionWhite => RenderedPreExplosionRadius.WholePixels,
        PowerBombExplosionPhase.Running.PreExplosionYellow or PowerBombExplosionPhase.Running.ExplosionWhite or PowerBombExplosionPhase.Running.Afterglow =>
            RenderedOvalRadius ?? RenderedExplosionRadius.WholePixels,
        // The remaining phases: the pending pre-explosion setup, the yellow explosion and Crystal Flash's explosion and
        // afterglow.
        _ => RenderedExplosionRadius.WholePixels,
    };

    /// <summary>Current semantic phase of the blast's resting animation.</summary>
    public PowerBombExplosionPhase Phase { get; private set; } = new PowerBombExplosionPhase.Inactive();

    /// <summary>
    /// Phase whose oval the current update chose.
    /// </summary>
    /// <remarks>
    /// An update can advance <see cref="Phase"/> after choosing its oval. Keeping the shown phase separate prevents the
    /// display from showing the next phase one frame early at the yellow/white transition boundaries.
    /// </remarks>
    private PowerBombExplosionPhase RenderedPhase { get; set; } = new PowerBombExplosionPhase.Inactive();

    /// <summary>Pre-explosion radius consumed before the end-of-frame update.</summary>
    private PowerBombRadius RenderedPreExplosionRadius { get; set; }

    /// <summary>Explosion radius consumed before the end-of-frame update.</summary>
    private PowerBombRadius RenderedExplosionRadius { get; set; }

    /// <summary>The blast's glow: the colour it adds to the scene inside its oval.</summary>
    public RgbColor GlowColor { get; private set; }

    /// <summary>True from the blast's start until its animation ends: its phase is one of a running blast's.</summary>
    public bool IsRunning => Phase is PowerBombExplosionPhase.Running;

    /// <summary>
    /// The fade steps the afterglow has still to take (#2529), one each update: the white explosion's last update primes
    /// the Power Bomb's (<see cref="PowerBombExplosionDefinitions.AfterglowFades"/>), and Crystal Flash's explosion its own
    /// for the band it reached (<see cref="PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps"/>). The update after the
    /// last fade ends the blast. The count is kept apart from the installed colours, so rebinding them never moves that end.
    /// </summary>
    private readonly UpdateCountdown _afterglowFades = UpdateCountdown.Ended();

    /// <summary>Starts the blast: its first update runs the pre-explosion's setup.</summary>
    public void Start()
    {
        // The blast takes its first step on the next update, which sets up the pre-explosion and its speed; its radius
        // first grows on the update after.
        Clear(new PowerBombExplosionPhase.Running.PendingPreExplosionSetup(), PowerBombRadius.Zero);
        GlowColor = RgbColor.Black;
    }

    /// <summary>
    /// Starts Crystal Flash's smaller blast and runs both its setups, after Crystal Flash completes its rise.
    /// </summary>
    public void BeginCrystalFlash()
    {
        Clear(new PowerBombExplosionPhase.Running.CrystalFlashExplosion(), PowerBombExplosionDefinitions.InitialRadius);
        GlowColor = RgbColor.Black;
    }

    /// <summary>
    /// Leaves every blast value as a start or an end does (#2529): running unless <paramref name="phase"/> is
    /// <see cref="PowerBombExplosionPhase.Inactive"/>, both radii at <paramref name="radius"/>, no speed, no oval and no
    /// afterglow fade left, and nothing shown yet. The glow colour is left as it is: a start or a reset blackens it, and an
    /// ordinary afterglow ends on the colour its fades left.
    /// </summary>
    private void Clear(PowerBombExplosionPhase phase, PowerBombRadius radius)
    {
        PreExplosionRadius = radius;
        ExplosionRadius = radius;
        _radiusSpeed = PowerBombRadius.Zero;
        RenderedOvalRadius = null;
        Phase = phase;
        RenderedPhase = new PowerBombExplosionPhase.Inactive();
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        _afterglowFades.End();
    }

    /// <summary>
    /// Runs one update of the blast's active stage step, its glow shown in <paramref name="colors"/>: the installed glow
    /// colours, installed content its owner holds rather than blast state (#2397).
    /// </summary>
    /// <remarks>The update after the afterglow's last fade ends the blast: it stops running (<see cref="IsRunning"/>).</remarks>
    public void StepFrame(InstalledPowerBombGlowColors colors)
    {
        Ensure.NotNull(colors);
        if (Phase is not PowerBombExplosionPhase.Running running)
            return;

        // Every stage first chooses this frame's oval and only then
        // changes its radius or its oval stage's updates, or advances the stage. Record
        // that phase here so the drawn blast is this update's, not the state prepared for the following one.
        RenderedPhase = Phase;

        if (running is PowerBombExplosionPhase.Running.PendingPreExplosionSetup)
        {
            PreExplosionRadius = PowerBombExplosionDefinitions.InitialRadius;
            _radiusSpeed = PowerBombExplosionDefinitions.InitialPreExplosionSpeed;
            Phase = new PowerBombExplosionPhase.Running.PreExplosionWhite();
            RenderedPhase = new PowerBombExplosionPhase.Inactive();
        }
        else if (running is PowerBombExplosionPhase.Running.PreExplosionWhite)
        {
            StepPreExplosionWhite(colors);
        }
        else if (running is PowerBombExplosionPhase.Running.PreExplosionYellow)
        {
            StepPreExplosionYellow(colors);
        }
        else if (running is PowerBombExplosionPhase.Running.ExplosionYellow)
        {
            StepExplosionYellow(colors);
        }
        else if (running is PowerBombExplosionPhase.Running.ExplosionWhite)
        {
            StepExplosionWhite(colors);
        }
        else if (running is PowerBombExplosionPhase.Running.Afterglow)
        {
            StepAfterglow();
        }
        else if (running is PowerBombExplosionPhase.Running.CrystalFlashExplosion)
        {
            StepCrystalFlashExplosion(colors);
        }
        else
        {
            // The remaining member, PowerBombExplosionPhase.Running.CrystalFlashAfterglow.
            StepCrystalFlashAfterglow();
        }
    }

    /// <summary>Clears every blast field during room/runtime reset.</summary>
    public void Reset()
    {
        Clear(new PowerBombExplosionPhase.Inactive(), PowerBombRadius.Zero);
        GlowColor = RgbColor.Black;
    }

    /// <summary>Displays the current white flash radius, then slows expansion until the yellow pre-explosion takes over.</summary>
    private void StepPreExplosionWhite(InstalledPowerBombGlowColors colors)
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        SelectGlowColor(
            colors,
            PowerBombGlowSequence.PreExplosion,
            PreExplosionGlowStep(PreExplosionRadius));

        // The white pre-explosion grows the radius by its speed and slows only while the grown radius is still
        // below the white limit.
        PreExplosionRadius += _radiusSpeed;
        if (PreExplosionRadius < PowerBombExplosionDefinitions.PreExplosionWhiteLimit)
        {
            _radiusSpeed -= PowerBombExplosionDefinitions.PreExplosionSlowing;
            return;
        }

        // Advancing past the wait runs the yellow stage's setup, starts its radius step,
        // and waits again during this same effect pass.
        _ovals = PowerBombOvalPlay.Starting(PowerBombExplosionDefinitions.YellowOvalRadii);
        Phase = new PowerBombExplosionPhase.Running.PreExplosionYellow();
    }

    /// <summary>
    /// Runs one update of an oval stage: returns the authored oval radius this update shows.
    /// </summary>
    /// <param name="stageEnds">Whether this update showed the stage's last oval, which ends it.</param>
    private int ShowNextOval(out bool stageEnds)
    {
        int radius = _ovals.Radius;
        PowerBombOvalPlay? rest = _ovals.AfterShowing;
        stageEnds = rest is null;
        if (rest is { } next)
            _ovals = next;
        return radius;
    }

    /// <summary>
    /// Grows a radius by the shared speed unless the growth would pass the radius's range (<see cref="PowerBombGrowthHold"/>);
    /// the test runs before either value changes, so a held update leaves the radius (and the blast's speed change) as
    /// they were.
    /// </summary>
    /// <returns>True with the grown radius when the radius grows.</returns>
    private bool TryGrowRadius(PowerBombRadius radius, out PowerBombRadius grown)
    {
        if (PowerBombGrowthHold.Holds(radius.WholePixelsGrownBy(_radiusSpeed)))
        {
            grown = radius;
            return false;
        }

        grown = radius + _radiusSpeed;
        return true;
    }

    /// <summary>Shows each yellow oval in turn and primes the damaging radius after the stage's last update.</summary>
    private void StepPreExplosionYellow(InstalledPowerBombGlowColors colors)
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        SelectGlowColor(
            colors,
            PowerBombGlowSequence.PreExplosion,
            PreExplosionGlowStep(PreExplosionRadius));

        // The yellow pre-explosion chooses this frame's oval before the stage counts the update, and the display shows
        // the chosen one after the logic frame has completed.
        RenderedOvalRadius = ShowNextOval(out bool stageEnds);

        // The hold is tested on the growth before either value changes: a growth that would pass the radius's range leaves
        // both the radius and its speed as they were for this frame.
        if (TryGrowRadius(PreExplosionRadius, out PowerBombRadius nextRadius))
        {
            PreExplosionRadius = nextRadius;
            _radiusSpeed -= PowerBombExplosionDefinitions.PreExplosionSlowing;
        }

        if (!stageEnds)
            return;

        // The yellow step initializes the actual damaging radius before the next frame's
        // yellow explosion update. PreExplosionRadius keeps its own value.
        ExplosionRadius = PowerBombExplosionDefinitions.InitialRadius;
        _radiusSpeed = PowerBombRadius.Zero;
        Phase = new PowerBombExplosionPhase.Running.ExplosionYellow();
    }

    /// <summary>Expands the damaging yellow radius with acceleration, then starts the white explosion's ovals.</summary>
    private void StepExplosionYellow(InstalledPowerBombGlowColors colors)
    {
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            colors,
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.GlowColorBand);

        ExplosionRadius += _radiusSpeed;
        if (ExplosionRadius < PowerBombExplosionDefinitions.ExplosionYellowLimit)
        {
            _radiusSpeed += PowerBombExplosionDefinitions.ExplosionAcceleration;
            return;
        }

        // The white explosion starts its expanding ovals (PowerBombExplosionDefinitions.WhiteOvalRadii) on the next update.
        _ovals = PowerBombOvalPlay.Starting(PowerBombExplosionDefinitions.WhiteOvalRadii);
        RenderedOvalRadius = null;
        Phase = new PowerBombExplosionPhase.Running.ExplosionWhite();
    }

    /// <summary>Shows the white ovals (<see cref="PowerBombExplosionDefinitions.WhiteOvalRadii"/>) and primes the afterglow that fades the glow.</summary>
    private void StepExplosionWhite(InstalledPowerBombGlowColors colors)
    {
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            colors,
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.GlowColorBand);

        RenderedOvalRadius = ShowNextOval(out bool stageEnds);

        if (TryGrowRadius(ExplosionRadius, out PowerBombRadius nextRadius))
        {
            ExplosionRadius = nextRadius;
            _radiusSpeed += PowerBombExplosionDefinitions.ExplosionAcceleration;
        }

        if (!stageEnds)
            return;

        // The white explosion primes the afterglow's fades. The afterglow then takes its first fade on the following
        // frame.
        _afterglowFades.Restart(PowerBombExplosionDefinitions.AfterglowFades);
        Phase = new PowerBombExplosionPhase.Running.Afterglow();
    }

    /// <summary>Fades the glow colour's three components one step every update and ends the blast after the last step.</summary>
    private void StepAfterglow()
    {
        if (TakeAfterglowFade())
            return;

        // The update after the last fade ends the blast without fading the colour again, leaving the colour its fades left.
        Clear(new PowerBombExplosionPhase.Inactive(), PowerBombRadius.Zero);
    }

    /// <summary>Uses the explosion curve to expand Crystal Flash only to its smaller radius limit.</summary>
    private void StepCrystalFlashExplosion(InstalledPowerBombGlowColors colors)
    {
        // Crystal Flash's explosion is a deliberately shorter clone of the yellow Power Bomb expansion.
        // It uses the same curve and glow colours, but stops at PowerBombExplosionDefinitions.CrystalFlashRadiusLimit.
        RenderedExplosionRadius = ExplosionRadius;
        SelectGlowColor(
            colors,
            PowerBombGlowSequence.Explosion,
            ExplosionRadius.GlowColorBand);

        ExplosionRadius += _radiusSpeed;
        if (ExplosionRadius < PowerBombExplosionDefinitions.CrystalFlashRadiusLimit)
        {
            _radiusSpeed += PowerBombExplosionDefinitions.ExplosionAcceleration;
            return;
        }

        // As with the ordinary afterglow, the Crystal Flash afterglow fades a step on every update.
        _afterglowFades.Restart(PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps(RenderedExplosionRadius));
        Phase = new PowerBombExplosionPhase.Running.CrystalFlashAfterglow();
    }

    /// <summary>Fades Crystal Flash's afterglow for its authored length, then clears the blast.</summary>
    private void StepCrystalFlashAfterglow()
    {
        // Crystal Flash's afterglow fades a step every update, as the ordinary afterglow does, for the length its
        // explosion set (PowerBombExplosionAfterglow); the installed colours only change what each step shows.
        if (TakeAfterglowFade())
            return;

        // The update after the afterglow's last fade ends the blast and clears every blast value, its speed included.
        Clear(new PowerBombExplosionPhase.Inactive(), PowerBombRadius.Zero);
        // The blast's end turns its glow off, however bright the installed colours left it.
        GlowColor = RgbColor.Black;
    }

    /// <summary>
    /// Takes the afterglow's next fade step, dimming each of the glow colour's three components one level, unless it has
    /// taken them all.
    /// </summary>
    /// <returns>Whether the update took a fade step; once none is left, the update ends the blast instead.</returns>
    private bool TakeAfterglowFade()
    {
        if (_afterglowFades.IsOver)
            return false;
        _afterglowFades.Count();
        GlowColor = GlowColor.FadeLevels.Map((_, channel) => channel == 0 ? 0 : channel - 1).ToColour();
        return true;
    }

    /// <summary>
    /// The pre-explosion colour a flash of <paramref name="radius"/> shows: its glow band, the pre-explosion colours
    /// repeating once the radius passes the last band. The repeat is load-bearing: the white flash ends past that band
    /// and its yellow updates grow it further, so its last colours are the sequence's first ones again.
    /// </summary>
    private static int PreExplosionGlowStep(PowerBombRadius radius) =>
        radius.GlowColorBand % PowerBombGlowColorFormat.Count(PowerBombGlowSequence.PreExplosion);

    /// <summary>Selects the glow colour of <paramref name="colors"/> a sequence shows at a radius-derived step.</summary>
    private void SelectGlowColor(InstalledPowerBombGlowColors colors, PowerBombGlowSequence sequence, int glowStep) =>
        GlowColor = colors.Resolve(sequence, glowStep);
}
