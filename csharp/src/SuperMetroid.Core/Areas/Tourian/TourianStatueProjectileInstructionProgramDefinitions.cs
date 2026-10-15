using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Compiled control for the Tourian entrance-statue actors at $86:B79F-$B878. The
/// twenty-eight interleaved spritemap operands select installed presentation data.
/// </summary>
internal abstract class TourianStatueProjectileInstructionProgramDefinitions
{
    /// <summary>Private Tourian projectile deletion program at $86:B79F.</summary>
    internal const ushort Delete = 0xb79f;
    /// <summary>Unlocking-particle water splash program at $86:B7A1.</summary>
    internal const ushort Splash = 0xb7a1;
    /// <summary>Boss-statue eye-glow program at $86:B7B3.</summary>
    internal const ushort EyeGlow = 0xb7b3;
    /// <summary>Unlocking-particle program at $86:B802.</summary>
    internal const ushort Particle = 0xb802;
    /// <summary>Unlocking-particle tail program at $86:B823.</summary>
    internal const ushort Tail = 0xb823;
    /// <summary>Ascending statue-soul program at $86:B84E.</summary>
    internal const ushort Soul = 0xb84e;
    /// <summary>Initial base-decoration program at $86:B85A.</summary>
    internal const ushort BaseDecoration = 0xb85a;
    /// <summary>Looping Ridley-statue program at $86:B86A.</summary>
    internal const ushort Ridley = 0xb86a;
    /// <summary>Looping Phantoon-statue program at $86:B872.</summary>
    internal const ushort Phantoon = 0xb872;

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
        var layout = new Layout(index, visual, Delete);
        layout.Delete();
        for (int pose = 0; pose < 4; pose++) layout.Pose(SplashHold);
        layout.Delete();
        foreach (ushort hold in EyeHolds) layout.Pose(hold);
        layout.Word((ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6);
        layout.SkipByte(); // Sound ID is a byte, outside the mechanics-word API.
        layout.Word((ushort)TourianStatueInstruction.Earthquake);
        for (int particle = 0; particle < ParticleBurstCount; particle++) layout.Word((ushort)TourianStatueInstruction.SpawnParticle);
        layout.Delete();
        layout.Start(Particle);
        for (int pair = 0; pair < 2; pair++)
        {
            layout.Pose(ParticleHold); layout.Pose(ParticleHold);
            if (pair == 0) layout.Word((ushort)TourianStatueInstruction.SpawnTail);
        }
        layout.Word((ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero);
        layout.Word(Particle);
        layout.Start(Tail);
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
        layout.Start(Soul);
        layout.Pose(SoulHold); layout.Pose(SoulHold); layout.Goto(Soul);
        layout.Pose(DecorationInitialHold);
        layout.Word((ushort)EnemyProjectileInstruction.PreInstructionInY);
        layout.Word((ushort)EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess);
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
    /// <summary>Checks whether a projectile kind uses one of these Tourian statue instruction lists.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.TourianStatueSplash or
        RoomEnemyProjectileKind.TourianStatueEyeGlow or
        RoomEnemyProjectileKind.TourianStatueParticle or
        RoomEnemyProjectileKind.TourianStatueTail or
        RoomEnemyProjectileKind.TourianStatueSoul or
        RoomEnemyProjectileKind.TourianStatueRidley or
        RoomEnemyProjectileKind.TourianStatuePhantoon or
        RoomEnemyProjectileKind.TourianStatueBaseDecoration;

    /// <summary>Finds a compiled control word by bank $86 address and rejects visual or unrelated data.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Tourian statue projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
