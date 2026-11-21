namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The special beam attacks' sizes, speeds, turns, lifetimes and sounds.</summary>
public static class BeamComboDefinitions
{
    /// <summary>The Plasma special beam attack's expansion/contraction step in pixels.</summary>
    public const int PlasmaRadiusStep = 4;
    /// <summary>The Plasma special beam attack's expansion switches phase at this radius.</summary>
    public const int PlasmaOuterRadius = 192;
    /// <summary>The Plasma special beam attack's contraction switches phase below this radius.</summary>
    public const int PlasmaInnerThreshold = 45;
    /// <summary>The radius in pixels a Plasma special beam attack's ring starts at, before it expands.</summary>
    public const int PlasmaStartRadius = 40;
    /// <summary>The radius in pixels of the circle an Ice special beam attack's particle orbits Samus on before it spreads.</summary>
    public const int IceOrbitRadius = 32;
    /// <summary>The updates an Ice special beam attack's particle orbits Samus before it turns outward and spreads.</summary>
    public const int IceOrbitLifetime = 600;
    /// <summary>The radius in pixels of the circle an Ice special beam attack's particle starts spreading from.</summary>
    public const int IceSpreadStartRadius = 40;
    /// <summary>The pixels an Ice special beam attack's spreading particle's circle widens by each update.</summary>
    public const int IceSpreadRadiusStep = 8;
    /// <summary>The updates a Wave special beam attack's particle homes on Samus before it ends.</summary>
    public const int WaveLifetime = 600;
    /// <summary>The radius in pixels of the circle a Spazer special beam attack's particle circles Samus on at the start.</summary>
    public const int SpazerCirclingRadius = 40;
    /// <summary>The radius in pixels of the circle a Spazer special beam attack's particle starts flying up on, toward the point above Samus.</summary>
    public const int SpazerFlyingStartRadius = 160;
    /// <summary>
    /// The pixels a flying Spazer special beam attack particle's circle narrows by each update toward the point, and widens
    /// by each update away from it. It divides <see cref="SpazerFlyingStartRadius"/>, so the narrowing circle meets zero exactly.
    /// </summary>
    public const int SpazerFlyingRadiusStep = 5;
    /// <summary>The radius in pixels at which a Spazer special beam attack's particle flying away from the point starts to fall.</summary>
    public const int SpazerFallingRadius = 96;
    /// <summary>
    /// The screen line a flying Spazer special beam attack particle starts to fall above, in pixels from the screen's top
    /// (#2280).
    /// </summary>
    public const int SpazerFallScreenLine = 16;
    /// <summary>
    /// The pixels a Spazer special beam attack particle steps sideways as it starts to fall: the beam pair to the right,
    /// the trailing pair to the left.
    /// </summary>
    public const int SpazerFallSideStep = 16;
    /// <summary>The pixels a falling Spazer special beam attack particle drops each update.</summary>
    public const int SpazerFallSpeed = 8;
    /// <summary>The pixels below the camera's top edge at which a falling Spazer special beam attack particle ends.</summary>
    public const int SpazerFallEndBelowCameraTop = 248;
    /// <summary>The Ice and Plasma particles' orbit turn each update when a right-facing Samus starts the attack: four steps on.</summary>
    public static readonly TrigonometryAngle OrbitTurnOn = new(4);
    /// <summary>The orbit turn each update when a left-facing Samus starts the attack: four steps back, a full circle less four.</summary>
    public static readonly TrigonometryAngle OrbitTurnBack = TrigonometryAngle.Wrapped(-4);
    /// <summary>The turn a circling Spazer particle makes each update, its own way round: four steps.</summary>
    public static readonly TrigonometryAngle SpazerCirclingTurn = new(4);
    /// <summary>The turn a Spazer particle flying up makes each update, toward the point or away from it: two steps.</summary>
    public static readonly TrigonometryAngle SpazerFlyingTurn = new(2);
    /// <summary>The Wave special beam attack's oscillator cap: eight pixels per update.</summary>
    public static readonly SignedEightEight WaveSpeedLimit = SignedEightEight.FromPixels(8);
    /// <summary>The Wave special beam attack's per-update acceleration: a quarter of a pixel per update.</summary>
    public static readonly SignedEightEight WaveAcceleration = SignedEightEight.FromPixelFraction(1, 4);
    /// <summary>The stop of the Wave special beam attack's sound as one of its particles is removed.</summary>
    public static readonly SoundEffectId WaveRemovalStop = SamusSoundStop.WaveSpecialAttack;
    /// <summary>The Wave special beam attack: the sound of the oscillator's pulse.</summary>
    public static readonly SoundEffectId WavePulseSound = SamusSound.WaveSpecialAttack;

    /// <summary>The sound each special beam attack starts with.</summary>
    internal static readonly IReadOnlyDictionary<SamusComboKind, SoundEffectId> ActivationSounds = new Dictionary<SamusComboKind, SoundEffectId>
    {
        [SamusComboKind.Wave] = SamusSound.WaveSpecialAttack,
        [SamusComboKind.Ice] = SamusSound.IceSpecialAttack,
        [SamusComboKind.Spazer] = SamusSound.SpazerSpecialAttack,
        [SamusComboKind.Plasma] = SamusSound.PlasmaSpecialAttack,
    };

    /// <summary>How far above Samus the Spazer special attack's particles centre their flight once they stop circling her.</summary>
    public const int SpazerFlightCentreHeight = 114;

    /// <summary>How far left of the view's left edge an Ice or Plasma special attack particle may fly before it ends, in pixels.</summary>
    public const int ViewLeftMargin = 32;

    /// <summary>How far right of the view's left edge an Ice or Plasma special attack particle ends, in pixels.</summary>
    public const int ViewRightReach = 288;

    /// <summary>How far below the view's top edge an Ice or Plasma special attack particle must stay, in pixels.</summary>
    public const int ViewTopReach = 16;

    /// <summary>How far below the view's top edge an Ice or Plasma special attack particle ends, in pixels.</summary>
    public const int ViewBottomReach = 256;
}
