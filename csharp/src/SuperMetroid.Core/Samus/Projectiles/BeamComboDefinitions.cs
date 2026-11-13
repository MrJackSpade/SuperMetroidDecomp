namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Identities and definition tables for special beam attacks.</summary>
public static class BeamComboDefinitions
{
    /// <summary>The Plasma special beam attack's expansion/contraction step in pixels.</summary>
    public const int PlasmaRadiusStep = 4;
    /// <summary>The Plasma special beam attack's expansion switches phase at this radius.</summary>
    public const int PlasmaOuterRadius = 192;
    /// <summary>The Plasma special beam attack's contraction switches phase below this radius.</summary>
    public const int PlasmaInnerThreshold = 45;
    /// <summary>The Ice and Plasma particles' orbit turn each update when a right-facing Samus starts the attack: four steps on.</summary>
    public static readonly TrigonometryAngle OrbitTurnOn = new(4);
    /// <summary>The orbit turn each update when a left-facing Samus starts the attack: four steps back, a full circle less four.</summary>
    public static readonly TrigonometryAngle OrbitTurnBack = TrigonometryAngle.Wrapped(-4);
    /// <summary>The turn a circling Spazer particle makes each update, its own way round: four steps.</summary>
    public static readonly TrigonometryAngle SpazerCirclingTurn = new(4);
    /// <summary>The turn a Spazer particle flying up makes each update, toward the point or away from it: two steps.</summary>
    public static readonly TrigonometryAngle SpazerFlyingTurn = new(2);
    /// <summary>The Wave special beam attack's oscillator cap: eight pixels per update.</summary>
    public static readonly SignedEightEight WaveSpeedLimit = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(2048);
    /// <summary>The Wave special beam attack's per-update acceleration: a quarter of a pixel per update.</summary>
    public static readonly SignedEightEight WaveAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);
    /// <summary>The sound of a Wave particle's removal.</summary>
    public static readonly SoundEffectId WaveRemovalSound = SamusSound.WaveSpecialAttackEnd;
    /// <summary>The Wave special beam attack: the sound of the oscillator's pulse.</summary>
    public static readonly SoundEffectId WavePulseSound = SamusSound.WaveSpecialAttack;
}
