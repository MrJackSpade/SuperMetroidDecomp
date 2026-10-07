namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics for normal and strong Brinstar Pipe Bug programs.</summary>
internal abstract class BrinstarPipeBugInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
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

    public static int MechanicsWordCount => 60;
    public static int PresentationWordCount => 44;

    private static ushort Start(int program) => program < 4
        ? (ushort)(NormalRisingLeft + program / 2 * 64 + program % 2 * 36)
        : (ushort)(StrongRisingLeft + (program - 4) * 20);

    private static int Frames(int program) => program < 4 ? 8 - 2 * (program & 1) : 4;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int program = 0; program < 8; program++)
        {
            int frames = Frames(program);
            if (index < frames + 2)
            {
                ushort address = (ushort)(Start(program) + (index < frames ? index * 4 : frames * 4 + (index - frames) * 2));
                return new(address, ReadMechanicsWord(address));
            }
            index -= frames + 2;
        }
        throw new IndexOutOfRangeException();
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        for (int program = 0; program < 8; program++)
        {
            int frames = Frames(program);
            if (index < frames)
                return (ushort)(Start(program) + index * 4 + 2);
            index -= frames;
        }
        throw new IndexOutOfRangeException();
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value)) return value;
        throw new InvalidDataException(
            $"Brinstar Pipe Bug instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    private static bool TryRead(int address, out ushort value)
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

    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xb30000 &&
        (TryRead((ushort)address, out _) || TryRead((ushort)address - 1, out _));
}