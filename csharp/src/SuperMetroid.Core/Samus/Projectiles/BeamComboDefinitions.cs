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
    /// <summary>The Wave special beam attack's oscillator cap: eight pixels per update.</summary>
    public static readonly SignedEightEight WaveSpeedLimit = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(2048);
    /// <summary>The Wave special beam attack's per-update acceleration: a quarter of a pixel per update.</summary>
    public static readonly SignedEightEight WaveAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);
    /// <summary>The sound of a Wave particle's removal.</summary>
    public static readonly SoundEffectId WaveRemovalSound = SamusSound.WaveSpecialAttackEnd;
    /// <summary>The Wave special beam attack: the sound of the oscillator's pulse.</summary>
    public static readonly SoundEffectId WavePulseSound = SamusSound.WaveSpecialAttack;
}
