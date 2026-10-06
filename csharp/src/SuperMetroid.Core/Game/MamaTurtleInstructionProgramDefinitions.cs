namespace SuperMetroid.Core.Game;

/// <summary>One compiled tatori-family mechanics word at its bank-$A2 address.</summary>
internal readonly record struct MamaTurtleInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, callbacks, and control flow for Mama Turtle and Baby Turtle programs.
/// Interleaved spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal static class MamaTurtleInstructionProgramDefinitions
{
    /// <summary><c>InstList_BabyTurtle_CrawlingLeft</c> at $A2:8B80.</summary>
    internal const ushort BabyCrawlingLeft = 0x8b80;
    /// <summary><c>InstList_BabyTurtle_Spinning</c> at $A2:8BD2.</summary>
    internal const ushort BabySpinning = 0x8bd2;
    /// <summary><c>InstList_MamaTurtle_Spinning</c> at $A2:8C02.</summary>
    internal const ushort MamaSpinning = 0x8c02;
    /// <summary><c>InstList_MamaTurtle_FacingLeft_EnterShell</c> at $A2:8C1C.</summary>
    internal const ushort MamaEnterShellLeft = 0x8c1c;
    /// <summary><c>InstList_BabyTurtle_FacingLeft_Hiding</c> at $A2:8C30.</summary>
    internal const ushort BabyHidingLeft = 0x8c30;
    /// <summary><c>InstList_MamaTurtle_Asleep</c> at $A2:8C44.</summary>
    internal const ushort MamaAsleep = 0x8c44;
    /// <summary><c>InstList_MamaTurtle_FacingLeft_LeaveShell</c> at $A2:8C4A.</summary>
    internal const ushort MamaLeaveShellLeft = 0x8c4a;
    /// <summary><c>InstList_BabyTurtle_FacingLeft_LeaveShell</c> at $A2:8C62.</summary>
    internal const ushort BabyLeaveShellLeft = 0x8c62;
    /// <summary><c>InstList_BabyTurtle_CrawlingRight</c> at $A2:8C72.</summary>
    internal const ushort BabyCrawlingRight = 0x8c72;
    /// <summary><c>InstList_MamaTurtle_FacingRight_EnterShell</c> at $A2:8D00.</summary>
    internal const ushort MamaEnterShellRight = 0x8d00;
    /// <summary><c>InstList_BabyTurtle_FacingRight_Hiding</c> at $A2:8D14.</summary>
    internal const ushort BabyHidingRight = 0x8d14;
    /// <summary><c>InstList_MamaTurtle_FacingRight_LeaveShell</c> at $A2:8D28.</summary>
    internal const ushort MamaLeaveShellRight = 0x8d28;
    /// <summary><c>InstList_BabyTurtle_FacingRight_LeaveShell</c> at $A2:8D40.</summary>
    internal const ushort BabyLeaveShellRight = 0x8d40;
    /// <summary><c>BabyTurtleConstants_travelDistance</c> at $A2:8D50.</summary>
    internal const ushort AdjacentMovementDefinitions = 0x8d50;

    /// <summary>A2:8BD2/8C02 spin holds; uneven spin cadence. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] SpinDurations = [1, 4, 5, 5, 5];
    /// <summary>A2:8C1C left-entry holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] MamaEnterLeftDurations = [32, 5, 5];
    /// <summary>A2:8D00 right-entry holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] MamaEnterRightDurations = [1, 5, 5];
    /// <summary>A2:8C30/8D14 hiding holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] BabyHideDurations = [5, 5, 64];
    /// <summary>A2:8C4A/8D28 exit holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] MamaLeaveDurations = [16, 5, 5, 96];
    /// <summary>A2:8C62/8D40 baby exit holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] BabyLeaveDurations = [5, 47];
    internal static int MechanicsWordCount => 117;
    internal static int PresentationWordCount => 75;

    internal static MamaTurtleInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = BabyCrawlingLeft; address < AdjacentMovementDefinitions; address += 2)
            if (TryControl((ushort)address, out ushort value) && index-- == 0)
                return new((ushort)address, value);
        throw new InvalidDataException("Turtle control layout is incomplete.");
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        // Every sub-$8000 control is a draw duration; its next word owns the artwork selector.
        for (int address = BabyCrawlingLeft; address < AdjacentMovementDefinitions; address += 2)
            if (TryControl((ushort)address, out ushort value) && value < 0x8000 && index-- == 0)
                return (ushort)(address + 2);
        throw new InvalidDataException("Turtle presentation layout is incomplete.");
    }

    internal static ushort ReadMechanicsWord(ushort address) => TryControl(address, out ushort value)
        ? value : throw new InvalidDataException($"Tatori instruction mechanics pointer $A2:{address:X4} is not compiled.");

    internal static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa20000 && TryControl((ushort)(address & 0xfffe), out _);

    private static bool TryControl(ushort address, out ushort value) =>
        TryCrawl(address, BabyCrawlingLeft, out value) || TryCrawl(address, BabyCrawlingRight, out value) ||
        TrySpin(address, BabySpinning, baby: true, out value) || TrySpin(address, MamaSpinning, baby: false, out value) ||
        TryShell(address, MamaEnterShellLeft, ShellProgram.MamaEnterLeft, out value) ||
        TryShell(address, MamaEnterShellRight, ShellProgram.MamaEnterRight, out value) ||
        TryShell(address, BabyHidingLeft, ShellProgram.BabyHide, out value) ||
        TryShell(address, BabyHidingRight, ShellProgram.BabyHide, out value) ||
        TryShell(address, MamaLeaveShellLeft, ShellProgram.MamaLeave, out value) ||
        TryShell(address, MamaLeaveShellRight, ShellProgram.MamaLeave, out value) ||
        TryShell(address, BabyLeaveShellLeft, ShellProgram.BabyLeave, out value) ||
        TryShell(address, BabyLeaveShellRight, ShellProgram.BabyLeave, out value) ||
        TrySleep(address, out value);

    private static bool TryCrawl(ushort address, ushort start, out ushort value)
    {
        value = 0;
        int offset = address - start;
        if ((uint)offset > 80) return false;
        if (offset == 80) { value = MamaTurtleInstructionCodes.LoopOrTurnAroundIfMovedTooFar; return true; }
        int field = offset % 10;
        if (field == 0) value = MamaTurtleInstructionCodes.Crawl;
        else if (field is 2 or 6) value = 10;
        else return false;
        return true;
    }

    private static bool TrySpin(ushort address, ushort start, bool baby, out ushort value)
    {
        value = 0;
        int offset = address - start;
        if (offset == 0) value = SpinDurations[0];
        else if (offset == 4) value = MamaTurtleInstructionCodes.PlaySpinningSound;
        else if (offset == 6) value = SpinDurations[1];
        else if (offset is 10 or 14 or 18) value = SpinDurations[1 + (offset - 6) / 4];
        else if (baby && offset == 22) value = MamaTurtleInstructionCodes.SetSpinningStoppable;
        else if (offset == (baby ? 24 : 22)) value = CommonEnemyInstructionCodes.Goto;
        else if (offset == (baby ? 26 : 24)) value = start;
        else return false;
        return true;
    }

    private enum ShellProgram { MamaEnterLeft, MamaEnterRight, BabyHide, MamaLeave, BabyLeave }

    private static bool TryShell(ushort address, ushort start, ShellProgram program, out ushort value)
    {
        value = 0;
        int frames = program == ShellProgram.MamaLeave ? 4 : program == ShellProgram.BabyLeave ? 2 : 3;
        int offset = address - start;
        if ((uint)offset < 4 * frames && offset % 4 == 0)
        {
            int frame = offset / 4;
            value = program switch
            {
                ShellProgram.MamaEnterLeft => MamaEnterLeftDurations[frame],
                ShellProgram.MamaEnterRight => MamaEnterRightDurations[frame],
                ShellProgram.BabyHide => BabyHideDurations[frame],
                ShellProgram.MamaLeave => MamaLeaveDurations[frame],
                _ => BabyLeaveDurations[frame],
            };
        }
        else if (offset == 4 * frames)
            value = program switch
            {
                ShellProgram.MamaEnterLeft => MamaTurtleInstructionCodes.RiseToHoverRightwards,
                ShellProgram.MamaEnterRight => MamaTurtleInstructionCodes.RiseToHoverLeftwards,
                ShellProgram.BabyHide => MamaTurtleInstructionCodes.LeaveShell,
                ShellProgram.MamaLeave => MamaTurtleInstructionCodes.EnterShell,
                _ => MamaTurtleInstructionCodes.LeftShell,
            };
        else if (offset == 4 * frames + 2) value = program == ShellProgram.BabyLeave ? (ushort)47 : (ushort)0x7fff;
        else if (offset == 4 * frames + 6) value = CommonEnemyInstructionCodes.Sleep;
        else return false;
        return true;
    }

    private static bool TrySleep(ushort address, out ushort value)
    {
        value = 0;
        if (address == MamaAsleep) value = 0x7fff;
        else if (address == MamaAsleep + 4) value = CommonEnemyInstructionCodes.Sleep;
        else return false;
        return true;
    }
}
