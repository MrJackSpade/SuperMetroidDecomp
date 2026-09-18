namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive animation phases shared by Skree and Metaree. Native AI advances
/// through the first three values; the fourth selector is an unused authored list.
/// </summary>
public enum SkreeMetareeAnimationPhase : ushort
{
    Idling = 0,
    PreparingAttack = 1,
    Diving = 2,
    StopAnimating = 3,
}

/// <summary>Compiled fixed animation-program selectors for Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitions
{
    /// <summary>
    /// Metaree idling, preparation, launched-attack, and unused stop-animation lists at
    /// <c>$A3:894E-$A3:8955</c>.
    /// </summary>
    private static readonly ushort[] MetareeInstructionLists =
    [
        0x8910,
        0x8924,
        0x8930,
        0x8946,
    ];

    /// <summary>
    /// Skree idling, preparation, launched-attack, and unused stop-animation lists at
    /// <c>$A3:C69C-$A3:C6A3</c>.
    /// </summary>
    private static readonly ushort[] SkreeInstructionLists =
    [
        0xc65e,
        0xc672,
        0xc67e,
        0xc694,
    ];

    /// <summary>Returns the authored Metaree list for one animation phase.</summary>
    internal static ushort MetareeInstructionList(SkreeMetareeAnimationPhase phase) =>
        Select(MetareeInstructionLists, phase, "Metaree");

    /// <summary>Returns the authored Skree list for one animation phase.</summary>
    internal static ushort SkreeInstructionList(SkreeMetareeAnimationPhase phase) =>
        Select(SkreeInstructionLists, phase, "Skree");

    private static ushort Select(
        ushort[] instructionLists,
        SkreeMetareeAnimationPhase phase,
        string enemyName)
    {
        int index = (int)phase;
        if ((uint)index >= instructionLists.Length)
        {
            throw new InvalidDataException(
                $"{enemyName} animation phase ${index:X4} exceeds its four-entry table.");
        }

        return instructionLists[index];
    }
}
