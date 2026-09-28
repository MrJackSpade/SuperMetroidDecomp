namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoRightOrbMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Bank-$AA callable Golden Torizo right-facing, right-foot-forward Chozo-orb
/// attack at $AA:CC99-CCDA. Timers, callbacks and movement are compiled
/// mechanics; its ten sprite selectors remain independently editable art.
/// </summary>
internal static class GoldenTorizoRightOrbInstructionProgramDefinitions
{
    internal const ushort Start = 0xcc99;
    internal const ushort End = 0xccdb;

    private static readonly GoldenTorizoRightOrbMechanicsWord[] Words =
    [
        new(0xcc99, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcc9b, 0xd5ed),
        new(0xcc9d, 0x0006),
        new(0xcca1, 0x0003),
        new(0xcca5, 0x0003),
        new(0xcca9, 0x0003),
        new(0xccad, 0x0003),
        new(0xccb1, 0x0006),
        new(0xccb5, CommonEnemyInstructionCodes.SetTimer),
        new(0xccb7, 0x0006),
        new(0xccb9, TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX),
        new(0xccbb, TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs),
        new(0xccbd, CommonEnemyInstructionCodes.WaitFrames),
        new(0xccbf, 0x0006),
        new(0xccc1, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xccc3, 0xccb9),
        new(0xccc5, 0x0003),
        new(0xccc9, 0x0003),
        new(0xcccd, 0x0003),
        new(0xccd1, 0x0003),
        new(0xccd5, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xccd7, 0xd5f1),
        new(0xccd9, TorizoInstructionCodes.Instruction_Torizo_Return),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xcc9f, 0xcca3, 0xcca7, 0xccab, 0xccaf,
        0xccb3, 0xccc7, 0xcccb, 0xcccf, 0xccd3,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoRightOrbMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoRightOrbMechanicsWord word in Words)
        {
            if (word.Address != address) continue;
            value = word.Value;
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (GoldenTorizoRightOrbMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}
