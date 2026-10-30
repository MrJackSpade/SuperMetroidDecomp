namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Native identities and definition tables for bank-$90 special beam attacks.</summary>
public static class BeamComboDefinitions
{
    /// <summary>ProjPreInstr_PlasmaSba expansion/contraction step in pixels.</summary>
    public const int PlasmaRadiusStep = 4;
    /// <summary>ProjPreInstr_PlasmaSbaFunc_0 ($90:D7E1) switches phase at this radius.</summary>
    public const int PlasmaOuterRadius = 192;
    /// <summary>ProjPreInstr_PlasmaSbaFunc_1 ($90:D7FA) switches phase below this radius.</summary>
    public const int PlasmaInnerThreshold = 45;
    /// <summary>ProjPreInstr_WaveSba ($90:DA08) oscillator cap, in native signed 8.8 units.</summary>
    public const int WaveSpeedLimit = 2048;
    /// <summary>ProjPreInstr_WaveSba ($90:DA08) per-frame acceleration, in signed 8.8 units.</summary>
    public const int WaveAcceleration = 64;
    /// <summary>$90:DA17: the sound of a Wave particle's removal.</summary>
    public static readonly SoundEffectId WaveRemovalSound = Library1Sound.WaveSpecialAttackEnd;
    /// <summary>ProjPreInstr_WaveSba ($90:DA08): the sound of the oscillator's pulse.</summary>
    public static readonly SoundEffectId WavePulseSound = Library1Sound.WaveSpecialAttack;
    /// <summary>
    /// The echo-and-trail selector table's entries trail the echo beam-table rows by two: an echo's entry is its
    /// beam combination's table index less two.
    /// </summary>
    internal const int EchoTableLeadingRows = 2;
}
