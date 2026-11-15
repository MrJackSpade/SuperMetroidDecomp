namespace SuperMetroid.Core.Fx;

/// <summary>Lava's choice of bubbling sound (#1415).</summary>
public static class LiquidEffects
{
    /// <summary>
    /// The lava sounds: enemy-library sounds chosen by the low three
    /// RNG bits.
    /// </summary>
    public static SoundEffectId LavaAmbientSound(int randomIndex) => LavaAcidWaveDefinitions.AmbientSoundChoice.Of(randomIndex) switch
    {
        0 or 3 or 6 => EnemySound.LavaBubbling1,
        1 or 4 or 7 => EnemySound.LavaBubbling2,
        _ => EnemySound.LavaBubbling3,
    };
}
