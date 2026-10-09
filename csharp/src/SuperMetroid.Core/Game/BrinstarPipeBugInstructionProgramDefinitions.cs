namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics for normal and strong Brinstar Pipe Bug programs.</summary>
internal abstract class BrinstarPipeBugInstructionProgramDefinitions
{
    /// <summary><c>InstList_Zeb_FacingLeft_Rising</c> at $B3:87AB.</summary>
    internal const ushort NormalRisingLeft = 0x87ab;
    /// <summary><c>InstList_Zeb_FacingLeft_Shooting</c> at $B3:87CF.</summary>
    internal const ushort NormalShootingLeft = 0x87cf;
    /// <summary><c>InstList_Zeb_FacingRight_Rising</c> at $B3:87EB.</summary>
    internal const ushort NormalRisingRight = 0x87eb;
    /// <summary><c>InstList_Zeb_FacingRight_Shooting</c> at $B3:880F.</summary>
    internal const ushort NormalShootingRight = 0x880f;
    /// <summary><c>InstList_Zebbo_FacingLeft_Rising</c> at $B3:8A1D.</summary>
    internal const ushort StrongRisingLeft = 0x8a1d;
    /// <summary><c>InstList_Zebbo_FacingLeft_Shooting</c> at $B3:8A31.</summary>
    internal const ushort StrongShootingLeft = 0x8a31;
    /// <summary><c>InstList_Zebbo_FacingRight_Rising</c> at $B3:8A45.</summary>
    internal const ushort StrongRisingRight = 0x8a45;
    /// <summary><c>InstList_Zebbo_FacingRight_Shooting</c> at $B3:8A59.</summary>
    internal const ushort StrongShootingRight = 0x8a59;

    /// <summary>Calculates the bank-$B3 start address for one of the four normal or four strong pipe-bug programs.</summary>
    /// <param name="program">Program slot: normal variants occupy 0 through 3 and strong variants 4 through 7.</param>
    /// <returns>The instruction-list address associated with the selected variant.</returns>
    internal static ushort Start(int program) => program < 4
        ? (ushort)(NormalRisingLeft + program / 2 * 64 + program % 2 * 36)
        : (ushort)(StrongRisingLeft + (program - 4) * 20);

    /// <summary>Returns the number of animation frames encoded by a pipe-bug instruction program.</summary>
    /// <param name="program">Program slot, using the same normal and strong ordering as <see cref="Start"/>.</param>
    /// <returns>Eight frames for normal rising, six for normal shooting, or four for a strong variant.</returns>
    internal static int Frames(int program) => program < 4 ? 8 - 2 * (program & 1) : 4;

    /// <summary>Reads a compiled mechanics word for a normal or strong pipe-bug instruction list.</summary>
    /// <param name="address">Bank-$B3 address of an instruction word represented by these compiled programs.</param>
    /// <returns>The mechanics, control-flow, or list-start word at that address.</returns>
    /// <exception cref="InvalidDataException">The address does not belong to a compiled pipe-bug program word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value)) return value;
        throw new InvalidDataException(
            $"Brinstar Pipe Bug instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    /// <summary>Attempts to resolve an address in a compiled pipe-bug program without throwing for unrelated addresses.</summary>
    /// <param name="address">Bank-$B3 address to look up.</param>
    /// <param name="value">Receives the compiled word on success, or zero when the address is not represented.</param>
    /// <returns><see langword="true"/> when the address identifies a word in one of the eight programs.</returns>
    internal static bool TryRead(int address, out ushort value)
    {
        for (int program = 0; program < 8; program++)
        {
            int offset = address - Start(program);
            int frames = Frames(program);
            if (offset < 0 || offset >= frames * 4 + 4 || (offset & 1) != 0)
                continue;
            if (offset >= frames * 4)
            {
                value = offset == frames * 4 ? CommonEnemyInstructionCodes.Goto : Start(program);
                return true;
            }
            if (offset % 4 == 0)
            {
                bool shooting = (program & 1) != 0;
                value = (ushort)(program < 4 ? shooting ? 1 : 2
                    : shooting ? 3 : 2 - ((offset / 4) & 1));
                return true;
            }
        }
        value = 0;
        return false;
    }
}
