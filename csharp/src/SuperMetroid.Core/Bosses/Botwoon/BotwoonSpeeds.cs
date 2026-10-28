namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Selects Botwoon's movement, spacing and spit speeds for its health phase.</summary>
public static class BotwoonSpeeds
{
    /// <summary>Returns the speeds of native health phase zero through two.</summary>
    /// <exception cref="InvalidDataException">The phase is outside the three native speed records.</exception>
    public static BotwoonSpeedDefinition ForHealthPhase(int phase) =>
        (uint)phase < (uint)BotwoonSpeedDefinitions.ByHealthPhase.Length
            ? BotwoonSpeedDefinitions.ByHealthPhase[phase]
            : throw new InvalidDataException($"Botwoon health phase {phase} is outside the three native speed records.");
}
