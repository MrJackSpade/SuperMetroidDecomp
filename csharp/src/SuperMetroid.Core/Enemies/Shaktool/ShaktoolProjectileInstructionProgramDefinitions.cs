using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Compiled control for the cartridge's three unused Shaktool attack-circle programs.
/// Their eight spritemap operands select installed presentation frames.
/// </summary>
internal abstract class ShaktoolProjectileInstructionProgramDefinitions
{
    /// <summary>Front attack-circle program at $86:BD68.</summary>
    internal const ushort Front = 0xbd68;

    /// <summary>Middle attack-circle program at $86:BD78.</summary>
    internal const ushort Middle = 0xbd78;

    /// <summary>Back attack-circle program at $86:BD8C.</summary>
    internal const ushort Back = 0xbd8c;

    /// <summary>$86:BD9C begins the adjacent initialization code.</summary>
    internal const ushort End = 0xbd9c;
    /// <summary>$86:BD68/6C/80: common growth-pose cadence. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort GrowthTicks = 4;
    /// <summary>$86:BD78: middle circle delay before installing its movement callback. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort MiddleLaunchTicks = 6;
    /// <summary>$86:BD8C: back circle delay before installing its movement callback. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort BackLaunchTicks = 10;
    /// <summary>$86:BD70/84/94: repeated final-pose hold. Reviewed under #1165 as authored action timing: the interpreter loads it into the instruction timer; relationships around it stay calculated and no simulation quantity derives the magnitude.</summary>
    private const ushort HeldPoseTicks = 119;
    /// <summary>Sentinel separating editable spritemap operands from compiled control words.</summary>
    internal const int PresentationOperand = -1;
    /// <summary>Eight sprite operands occur across the three attack-circle programs.</summary>
    public static int PresentationWordCount => 8;
    /// <summary>Locates a sprite operand in native front, middle, then back program order.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 3) return (ushort)(Front + 2 + index * 4);
        if (index < 6) return (ushort)(Middle + (index == 3 ? 2 : 10 + (index - 4) * 4));
        return (ushort)(Back + 2 + (index - 6) * 8);
    }
    /// <summary>Recognizes the three Shaktool circle projectiles controlled by these lists.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) =>
        kind is RoomEnemyProjectileKind.ShaktoolAttackFrontCircle or
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle or
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle;
    /// <summary>Reads a bounded aligned control word while rejecting presentation slots.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address >= Front && address < End && (address & 1) == 0)
        {
            int value = ProgramWord(address);
            if (value != PresentationOperand) return (ushort)value;
        }
        throw new InvalidDataException($"Shaktool attack-circle mechanics pointer $86:{address:X4} is not compiled.");
    }
    /// <summary>The three attack-circle programs, in address order.</summary>
    private enum CircleProgram
    {
        /// <summary>Front attack circle.</summary>
        Front,
        /// <summary>Middle attack circle.</summary>
        Middle,
        /// <summary>Back attack circle.</summary>
        Back,
    }

    /// <summary>Maps a circle's front, middle, or back identity to its native list start.</summary>
    private static ushort StartOf(CircleProgram program) => program switch
    {
        CircleProgram.Front => Front,
        CircleProgram.Middle => Middle,
        CircleProgram.Back => Back,
        _ => throw new InvalidOperationException($"Undefined {nameof(CircleProgram)} {program}."),
    };

    /// <summary>Reconstructs timing, movement callback, and held-pose loop words at a bank-$86 address.</summary>
    internal static int ProgramWord(ushort address)
    {
        CircleProgram program = address < Middle ? CircleProgram.Front
            : address < Back ? CircleProgram.Middle : CircleProgram.Back;
        ushort start = StartOf(program);
        var writer = new WordSelector(address, start);
        switch (program)
        {
            case CircleProgram.Front:
                writer.Timed(GrowthTicks);
                writer.Timed(GrowthTicks);
                break;
            case CircleProgram.Middle:
                writer.Timed(MiddleLaunchTicks);
                writer.Command((ushort)EnemyProjectileInstruction.PreInstructionInY);
                writer.Command((ushort)EnemyProjectilePreInstruction.ShaktoolsAttack_MiddleBack_Moving);
                writer.Timed(GrowthTicks);
                break;
            case CircleProgram.Back:
                writer.Timed(BackLaunchTicks);
                writer.Command((ushort)EnemyProjectileInstruction.PreInstructionInY);
                writer.Command((ushort)EnemyProjectilePreInstruction.ShaktoolsAttack_MiddleBack_Moving);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CircleProgram)} {program}.");
        }
        ushort heldPose = (ushort)(start + (program == CircleProgram.Middle ? 12 : 8));
        writer.Timed(HeldPoseTicks);
        writer.Command((ushort)EnemyProjectileInstruction.GotoY);
        writer.Command(heldPose);
        return writer.Value;
    }
    /// <summary>Tracks an address through emitted instruction words to select one compiled value.</summary>
    private struct WordSelector(ushort address, ushort start)
    {
        /// <summary>Word offset from the selected program start.</summary>
        private int remaining = (address - start) / 2;

        /// <summary>Selected instruction word, or failure if the emitted program did not reach the address.</summary>
        public int Value { get => field == int.MinValue
            ? throw new InvalidOperationException("Shaktool projectile program shape is incomplete.") : field; private set; } = int.MinValue;
        /// <summary>Emits a control word without a following presentation slot.</summary>
        public void Command(ushort command) => Emit(command);
        /// <summary>Emits a duration followed by one editable sprite operand slot.</summary>
        public void Timed(ushort duration) { Emit(duration); Emit(PresentationOperand); }
        /// <summary>Records the emitted word when it matches the requested address offset.</summary>
        private void Emit(int value) { if (remaining-- == 0) Value = value; }
    }
}
