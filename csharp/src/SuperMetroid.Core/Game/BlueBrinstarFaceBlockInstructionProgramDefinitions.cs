namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from all three Blue Brinstar face-block programs.</summary>
/// <remarks>
/// Frame durations and terminal sleeps are immutable simulation data. The seven
/// interleaved spritemap pointers select separately installed presentation data.
/// </remarks>
internal abstract class BlueBrinstarFaceBlockInstructionProgramDefinitions
{
    /// <summary><c>$A8:E80C</c>, the three-frame animation when Samus approaches from the left.</summary>
    internal const ushort SamusLeft = 0xe80c;

    /// <summary><c>$A8:E81A</c>, the three-frame animation when Samus approaches from the right.</summary>
    internal const ushort SamusRight = 0xe81a;

    /// <summary><c>$A8:E828</c>, the one-frame neutral program installed at initialization.</summary>
    internal const ushort Initial = 0xe828;
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - SamusLeft;
        return (uint)offset < 28 ? offset % 14 % 4 == 2 : address == Initial + 2;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - SamusLeft;
        if ((uint)offset < 28 && offset % 14 % 4 == 0)
        {
            int local = offset % 14;
            // Wait facing forward, show the intermediate and final turn poses, then sleep.
            return local == 12 ? (ushort)CommonEnemyInstruction.Sleep : (ushort)(local == 0 ? 48 : 16);
        }
        if (address == Initial) return 1;
        if (address == Initial + 4) return (ushort)CommonEnemyInstruction.Sleep;
        throw new InvalidDataException($"Blue Brinstar face-block instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }
}
