namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A room population record word read as the two independent one-byte settings many species author in it (#1422): a
/// direction beside a pattern, a speed-table row beside a radius, a wait beside a distance. Each species names the
/// settings it reads at spawn; this view only separates them, decoding the word once.
/// </summary>
/// <param name="First">The setting authored first, in the word's low eight bits.</param>
/// <param name="Second">The setting authored second, in the word's high eight bits.</param>
[EncodingOwner("population record word holding two one-byte settings")]
public readonly record struct PopulationParameterSettings(byte First, byte Second)
{
    /// <summary>The bits the first setting occupies below the second.</summary>
    private const int FirstSettingBits = 8;

    /// <summary>The two settings of an authored word, 0 through 65535.</summary>
    public static PopulationParameterSettings Of(int word)
    {
        int authored = Ensure.BetweenInclusive(0, ushort.MaxValue, word);
        return new(unchecked((byte)authored), unchecked((byte)(authored >> FirstSettingBits)));
    }
}
