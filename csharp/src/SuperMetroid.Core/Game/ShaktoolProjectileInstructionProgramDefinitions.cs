namespace SuperMetroid.Core.Game;

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
    /// <summary>Sentinel placed in compiled words where an editable spritemap operand replaces native presentation data.</summary>
    internal const int PresentationOperand = -1;
    /// <summary>Number of spritemap operand words exposed across the three attack-circle programs.</summary>
    public static int PresentationWordCount => 8;

    /// <summary>Maps an editable presentation-word index to its location in the compiled attack-circle programs.</summary>
    /// <param name="index">Zero-based index among the eight spritemap operands.</param>
    /// <returns>The bank-$86 word address containing that presentation operand.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 3) return (ushort)(Front + 2 + index * 4);
        if (index < 6) return (ushort)(Middle + (index == 3 ? 2 : 10 + (index - 4) * 4));
        return (ushort)(Back + 2 + (index - 6) * 8);
    }
    /// <summary>Reports whether the projectile kind is one of Shaktool's three attack-circle effects.</summary>
    /// <param name="kind">Projectile kind to check against the compiled programs.</param>
    /// <returns><see langword="true"/> for the front, middle, or back attack circle.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) =>
        kind is RoomEnemyProjectileKind.ShaktoolAttackFrontCircle or
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle or
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle;
    /// <summary>Returns a compiled mechanics word while rejecting presentation operands and addresses outside the programs.</summary>
    /// <param name="address">Even bank-$86 word address within the three adjacent attack-circle programs.</param>
    /// <returns>The compiled command, duration, or control-flow operand at that address.</returns>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address >= Front && address < End && (address & 1) == 0)
        {
            int value = ProgramWord(address);
            if (value != PresentationOperand) return (ushort)value;
        }
        throw new InvalidDataException($"Shaktool attack-circle mechanics pointer $86:{address:X4} is not compiled.");
    }
    /// <summary>Reconstructs the native word selected by an address in one compiled attack-circle instruction stream.</summary>
    /// <param name="address">The bank-$86 word address whose instruction-stream value is requested.</param>
    /// <returns>The command, duration, presentation sentinel, or branch target occupying that word.</returns>
    internal static int ProgramWord(ushort address)
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
    /// <summary>Walks emitted instruction words and captures only the word at a requested stream position.</summary>
    /// <param name="address">The word address to capture.</param>
    /// <param name="start">The first word address in the selected program.</param>
    private struct WordSelector(ushort address, ushort start)
    {
        /// <summary>Words still to emit before the requested address is reached.</summary>
        private int remaining = (address - start) / 2;
        /// <summary>The captured word, or the sentinel indicating that emission has not reached the address.</summary>
        private int selected = int.MinValue;

        /// <summary>Gets the captured word after emission reaches the selected address.</summary>
        public readonly int Value => selected == int.MinValue
            ? throw new InvalidOperationException("Shaktool projectile program shape is incomplete.") : selected;

        /// <summary>Emits one instruction or operand word into the stream position being selected.</summary>
        /// <param name="command">The literal word to append.</param>
        public void Command(ushort command) => Emit(command);

        /// <summary>Emits a duration followed by the sentinel for the editable presentation operand.</summary>
        /// <param name="duration">The authored tick count loaded by the projectile instruction timer.</param>
        public void Timed(ushort duration) { Emit(duration); Emit(PresentationOperand); }

        /// <summary>Consumes one stream word and captures it when it occupies the requested address.</summary>
        /// <param name="value">The literal instruction or operand value to emit.</param>
        private void Emit(int value) { if (remaining-- == 0) selected = value; }
    }
}
