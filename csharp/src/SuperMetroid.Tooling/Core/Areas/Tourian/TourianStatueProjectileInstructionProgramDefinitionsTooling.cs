using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using D = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileInstructionProgramDefinitions;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Development-tool members of <see cref="TourianStatueProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TourianStatueProjectileInstructionProgramDefinitions))]
internal abstract class TourianStatueProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>$86:B7B3-B7D9: eye-pose holds, slowing 8/7/6/5 before the 48-tick stare. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] EyeHolds = [8, 8, 8, 7, 7, 7, 6, 6, 5, 48];
    /// <summary>Splash/particle/tail/soul and statue holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort SplashHold = 8, ParticleHold = 3, TailHold = 4, SoulHold = 8,
        DecorationInitialHold = 128, StatueHold = 0x0777;
    /// <summary>$86:B7E0-B7E6: the authored four-particle burst count; it sets how many particle frames repeat, not a physical quantity.</summary>
    private const int ParticleBurstCount = 4;
    /// <summary>Number of compiled timing, opcode, and control operands across the statue programs.</summary>
    public static int MechanicsWordCount => 57;
    /// <summary>Number of spritemap operands resolved from installed presentation data.</summary>
    public static int PresentationWordCount => 28;

    /// <summary>Gets a compiled control word by its order in the native statue instruction range.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: false);
    }
    /// <summary>Gets the bank $86 address of an indexed visual selector without reading its native value.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: true).Address;
    }

    /// <summary>
    /// $86:B79F-B878: compose timed poses and native control operands; embedded native
    /// functions separate the eye, particle, tail and soul lists. Tail displacement
    /// halves from eight to two pixels after its first three four-tick poses.
    /// </summary>
    private static InstructionMechanicsWord Select(int index, bool visual)
    {
        var layout = new Layout(index, visual, D.Delete);
        layout.Delete();
        for (int pose = 0; pose < 4; pose++) layout.Pose(SplashHold);
        layout.Delete();
        foreach (ushort hold in EyeHolds) layout.Pose(hold);
        layout.Word((ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6);
        layout.SkipByte(); // Sound ID is a byte, outside the mechanics-word API.
        layout.Word((ushort)TourianStatueInstruction.Earthquake);
        for (int particle = 0; particle < ParticleBurstCount; particle++) layout.Word((ushort)TourianStatueInstruction.SpawnParticle);
        layout.Delete();
        layout.Start(D.Particle);
        for (int pair = 0; pair < 2; pair++)
        {
            layout.Pose(ParticleHold); layout.Pose(ParticleHold);
            if (pair == 0) layout.Word((ushort)TourianStatueInstruction.SpawnTail);
        }
        layout.Word((ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero);
        layout.Word(D.Particle);
        layout.Start(D.Tail);
        for (int phase = 0; phase < 4; phase++)
        {
            layout.Pose(TailHold);
            if (phase < 3)
            {
                layout.Word((ushort)TourianStatueInstruction.AddY);
                layout.Word((ushort)(8 >> phase));
            }
        }
        layout.Delete();
        layout.Start(D.Soul);
        layout.Pose(SoulHold); layout.Pose(SoulHold); layout.Goto(D.Soul);
        layout.Pose(DecorationInitialHold);
        layout.Word((ushort)EnemyProjectileInstruction.PreInstructionInY);
        layout.Word(EnemyProjectilePreInstructionWords.Word(EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess));
        for (int actor = 0; actor < 3; actor++)
        {
            ushort loop = layout.Cursor;
            layout.Pose(StatueHold); layout.Goto(loop);
        }
        return layout.Result;
    }

    /// <summary>Walks native program addresses while selecting one mechanics or presentation operand.</summary>
    private ref struct Layout(int requested, bool visual, ushort start)
    {
        /// <summary>Current bank $86 address as the program layout advances.</summary>
        internal ushort Cursor { get; private set; } = start;
        /// <summary>Counts encountered control words and visual selectors independently.</summary>
        private int mechanics, presentation;
        /// <summary>The requested address and value found during layout traversal.</summary>
        internal InstructionMechanicsWord Result { get; private set; }
        /// <summary>Moves the layout cursor to the start of another native instruction list.</summary>
        internal void Start(ushort address) => Cursor = address;
        /// <summary>Skips the one-byte sound ID outside the word-oriented mechanics catalog.</summary>
        internal void SkipByte() => Cursor++;
        /// <summary>Records an indexed mechanics word and advances by one native word.</summary>
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(Cursor, value);
            mechanics++; Cursor += sizeof(ushort);
        }
        /// <summary>Records a pose duration and reserves its following visual selector word.</summary>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(Cursor, 0);
            presentation++; Cursor += sizeof(ushort);
        }
        /// <summary>Appends the projectile deletion opcode.</summary>
        internal void Delete() => Word((ushort)EnemyProjectileInstruction.Delete);
        /// <summary>Appends a native goto opcode and its target address.</summary>
        internal void Goto(ushort target)
        {
            Word((ushort)EnemyProjectileInstruction.GotoY); Word(target);
        }
    }
    /// <summary>Matches either byte of a statue projectile mechanics word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
