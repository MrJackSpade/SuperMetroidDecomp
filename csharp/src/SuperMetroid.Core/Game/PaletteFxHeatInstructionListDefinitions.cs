namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive suit palette selected by the Samus-in-heat pre-instruction.</summary>
public enum PaletteFxHeatSuit
{
    /// <summary>Power Suit, selected when neither Varia nor Gravity is equipped.</summary>
    Power,

    /// <summary>Varia Suit, selected when Varia is equipped without Gravity.</summary>
    Varia,

    /// <summary>Gravity Suit, which has native priority when both suit bits are equipped.</summary>
    Gravity,
}

/// <summary>
/// Immutable program selectors consumed by <c>PreInstruction_PaletteFXObject_SamusInHeat</c>.
/// </summary>
/// <remarks>
/// The Norfair palette animator publishes only phases zero through fifteen. These pointers
/// choose engine behavior and therefore belong to the compiled mechanics domain; the
/// selected programs' BGR555 color words remain cartridge-backed presentation data.
/// </remarks>
public static class PaletteFxHeatInstructionListDefinitions
{
    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.gravity</c> at
    /// <c>$8D:E3E0</c>.
    /// </summary>
    internal const ushort GravitySourceTable = 0xe3e0;

    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.varia</c> at
    /// <c>$8D:E400</c>.
    /// </summary>
    internal const ushort VariaSourceTable = 0xe400;

    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.power</c> at
    /// <c>$8D:E420</c>.
    /// </summary>
    internal const ushort PowerSourceTable = 0xe420;

    /// <summary>The sixteen phases published by Norfair palette program $8D:F08E.</summary>
    public const int PhaseCount = 16;

    private static readonly ushort[] GravityPrograms =
    [
        0xe8be, 0xe8e0, 0xe902, 0xe924, 0xe946, 0xe968, 0xe98a, 0xe9ac,
        0xe9ce, 0xe9f0, 0xea12, 0xea34, 0xea56, 0xea78, 0xea9a, 0xeabc,
    ];

    private static readonly ushort[] VariaPrograms =
    [
        0xe692, 0xe6b4, 0xe6d6, 0xe6f8, 0xe71a, 0xe73c, 0xe75e, 0xe780,
        0xe7a2, 0xe7c4, 0xe7e6, 0xe808, 0xe82a, 0xe84c, 0xe86e, 0xe890,
    ];

    private static readonly ushort[] PowerPrograms =
    [
        0xe466, 0xe488, 0xe4aa, 0xe4cc, 0xe4ee, 0xe510, 0xe532, 0xe554,
        0xe576, 0xe598, 0xe5ba, 0xe5dc, 0xe5fe, 0xe620, 0xe642, 0xe664,
    ];

    /// <summary>Returns the compiled program selected by one suit and published heat phase.</summary>
    /// <remarks>
    /// Valid phase p=0..15 selects base + $22*p: Gravity base $E8BE
    /// from source $8D:E3E0, Varia base $E692 from $8D:E400, or Power
    /// base $E466 from $8D:E420. The $22 stride is one duration, fifteen
    /// live BGR555 colors, and one wait word. All 48 source pointers
    /// match the pinned NTSC J/U v1.0 ROM. The native pre-instruction
    /// prioritizes Gravity over Varia; invalid phase or suit fails here.
    /// </remarks>
    public static ushort Resolve(PaletteFxHeatSuit suit, ushort phase)
    {
        if (phase >= PhaseCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(phase), phase, $"Norfair heat palette phase must be 0..{PhaseCount - 1}.");
        }

        return suit switch
        {
            PaletteFxHeatSuit.Power => PowerPrograms[phase],
            PaletteFxHeatSuit.Varia => VariaPrograms[phase],
            PaletteFxHeatSuit.Gravity => GravityPrograms[phase],
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
        };
    }

    /// <summary>
    /// Applies the cartridge's Gravity-before-Varia equipment priority and returns the
    /// program selected for the published heat phase.
    /// </summary>
    public static ushort ResolveForEquippedItems(ushort equippedItems, ushort phase)
    {
        PaletteFxHeatSuit suit = equippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? PaletteFxHeatSuit.Gravity
            : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? PaletteFxHeatSuit.Varia
                : PaletteFxHeatSuit.Power;
        return Resolve(suit, phase);
    }

    /// <summary>Returns the native bank-$8D selector-word address for parity verification.</summary>
    internal static ushort NativeSourceAddress(PaletteFxHeatSuit suit, int phase)
    {
        if ((uint)phase >= PhaseCount)
            throw new ArgumentOutOfRangeException(nameof(phase));

        ushort table = suit switch
        {
            PaletteFxHeatSuit.Power => PowerSourceTable,
            PaletteFxHeatSuit.Varia => VariaSourceTable,
            PaletteFxHeatSuit.Gravity => GravitySourceTable,
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
        };
        return unchecked((ushort)(table + phase * sizeof(ushort)));
    }
}
