using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The Ceres Baby's native bank-$A6 draw lists and their instructions, for the #641 ROM oracle.</summary>
internal static class CeresBabyNativeListAddresses
{
    /// <summary>The baby's draw lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each program's native entry: <c>InstList_BabyMetroidCutscene_0</c> and <c>_1</c>.</summary>
    internal static readonly CeresBabyNativeListAddressesEntry[] Entries =
    [
        new CeresBabyNativeListAddressesEntry(CeresBabyProgram.Initial, 0xbf31),
        new CeresBabyNativeListAddressesEntry(CeresBabyProgram.ExpressiveLoop, 0xbf59),
    ];

    /// <summary>The draw list's instructions: the cue variant each becomes, or null for its goto.</summary>
    internal static readonly Dictionary<ushort, Type?> Instructions = new()
    {
        [0xbfc9] = typeof(CeresBabyCue.CryOrGoTo),
        [0xbfe1] = typeof(CeresBabyCue.UpdateColors),
        [0xbff2] = typeof(CeresBabyCue.GoToIfMoving),
        [0xbff8] = null,
    };

    /// <summary>Each palette operand the lists use, by the row it loads.</summary>
    internal static readonly CeresBabyNativeListAddressesPaletteRow[] PaletteRows = [new CeresBabyNativeListAddressesPaletteRow(0, 0xe1f1), new CeresBabyNativeListAddressesPaletteRow(1, 0xe20f), new CeresBabyNativeListAddressesPaletteRow(2, 0xe22d), new CeresBabyNativeListAddressesPaletteRow(3, 0xe24b)];
}
