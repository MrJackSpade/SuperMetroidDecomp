namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// A music voice's cut-key setting (#2560): whether the pattern's cut-key effect suppresses the voice's notes, kept as
/// the value the driver stores, the effect's argument plus one wrapped to a byte, so an argument of $FF turns the cut
/// off and the driver-state snapshot records the exact stored byte.
/// </summary>
internal readonly record struct SpcMusicCutKey
{
    private readonly byte _stored;

    private SpcMusicCutKey(byte stored) => _stored = stored;

    /// <summary>The setting of a voice whose track has not run the effect: notes play.</summary>
    internal static SpcMusicCutKey Off => default;

    /// <summary>The setting the cut-key effect with <paramref name="argument"/> leaves.</summary>
    internal static SpcMusicCutKey FromArgument(int argument) => new(unchecked((byte)(argument + 1)));

    /// <summary>Whether the cut holds, suppressing the voice's notes.</summary>
    internal bool Holds => _stored != 0;

    /// <summary>The byte the driver keeps for this setting, written into a driver-state snapshot.</summary>
    internal int DriverByte => _stored;
}
