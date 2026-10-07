namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the cartridge's three unused Shaktool attack-circle programs.
/// Their eight spritemap operands select installed presentation frames.
/// </summary>
internal abstract class ShaktoolProjectileInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Front attack-circle program at $86:BD68.</summary>
    internal const ushort Front = 0xbd68;

    /// <summary>Middle attack-circle program at $86:BD78.</summary>
    internal const ushort Middle = 0xbd78;

    /// <summary>Back attack-circle program at $86:BD8C.</summary>
    internal const ushort Back = 0xbd8c;

    /// <summary>$86:BD9C begins the adjacent initialization code.</summary>
    private const ushort End = 0xbd9c;
    /// <summary>$86:BD68/6C/80: common growth-pose cadence. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort GrowthTicks = 4;
    /// <summary>$86:BD78: middle circle delay before installing its movement callback. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort MiddleLaunchTicks = 6;
    /// <summary>$86:BD8C: back circle delay before installing its movement callback. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort BackLaunchTicks = 10;
    /// <summary>$86:BD70/84/94: repeated final-pose hold. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort HeldPoseTicks = 119;
    private const int PresentationOperand = -1;

    public static int MechanicsWordCount => 18;
    public static int PresentationWordCount => 8;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = Front; address < End; address += 2)
        {
            int value = ProgramWord((ushort)address);
            if (value != PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Shaktool projectile mechanics-word index is inconsistent.");
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 3) return (ushort)(Front + 2 + index * 4);
        if (index < 6) return (ushort)(Middle + (index == 3 ? 2 : 10 + (index - 4) * 4));
        return (ushort)(Back + 2 + (index - 6) * 8);
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) =>
        kind is RoomEnemyProjectileKind.ShaktoolAttackFrontCircle or
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle or
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle;
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address >= Front && address < End && (address & 1) == 0)
        {
            int value = ProgramWord(address);
            if (value != PresentationOperand) return (ushort)value;
        }
        throw new InvalidDataException($"Shaktool attack-circle mechanics pointer $86:{address:X4} is not compiled.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & 0xfffe);
        return word >= Front && word < End && ProgramWord(word) != PresentationOperand;
    }
    private static int ProgramWord(ushort address)
    {
        ushort start = address < Middle ? Front : address < Back ? Middle : Back;
        var writer = new WordSelector(address, start);
        if (start == Front)
        {
            writer.Timed(GrowthTicks);
            writer.Timed(GrowthTicks);
        }
        else
        {
            writer.Timed(start == Middle ? MiddleLaunchTicks : BackLaunchTicks);
            writer.Command(EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY);
            writer.Command(EnemyProjectileCodePointers.PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving);
            if (start == Middle) writer.Timed(GrowthTicks);
        }
        ushort heldPose = (ushort)(start + (start == Middle ? 12 : 8));
        writer.Timed(HeldPoseTicks);
        writer.Command(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY);
        writer.Command(heldPose);
        return writer.Value;
    }
    private struct WordSelector(ushort address, ushort start)
    {
        private int remaining = (address - start) / 2;
        private int selected = int.MinValue;
        public readonly int Value => selected == int.MinValue
            ? throw new InvalidOperationException("Shaktool projectile program shape is incomplete.") : selected;
        public void Command(ushort command) => Emit(command);
        public void Timed(ushort duration) { Emit(duration); Emit(PresentationOperand); }
        private void Emit(int value) { if (remaining-- == 0) selected = value; }
    }
}
