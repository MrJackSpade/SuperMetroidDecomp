namespace SuperMetroid.Core.Game;

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
    /// <summary>Number of instruction words exposed as projectile mechanics operands.</summary>
    public static int MechanicsWordCount => 57;
    /// <summary>Number of pose-duration words whose addresses identify installed presentation data.</summary>
    public static int PresentationWordCount => 28;

    /// <summary>Returns the mechanics operand at the requested position in the compiled Tourian statue programs.</summary>
    /// <param name="index">Zero-based position among the mechanics words.</param>
    /// <returns>The bank-$86 address and value of that operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: false);
    }
    /// <summary>Returns the ROM address of a pose-duration word used to select presentation data.</summary>
    /// <param name="index">Zero-based position among the pose words.</param>
    /// <returns>The bank-$86 address of the selected pose word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside <see cref="PresentationWordCount"/>.</exception>
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
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6);
        layout.SkipByte(); // Sound ID is a byte, outside the mechanics-word API.
        layout.Word(TourianStatueRomData.Earthquake);
        for (int particle = 0; particle < ParticleBurstCount; particle++) layout.Word(TourianStatueRomData.SpawnParticle);
        layout.Delete();
        layout.Start(Particle);
        for (int pair = 0; pair < 2; pair++)
        {
            layout.Pose(ParticleHold); layout.Pose(ParticleHold);
            if (pair == 0) layout.Word(TourianStatueRomData.SpawnTail);
        }
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero);
        layout.Word(Particle);
        layout.Start(Tail);
        for (int phase = 0; phase < 4; phase++)
        {
            layout.Pose(TailHold);
            if (phase < 3)
            {
                layout.Word(TourianStatueRomData.AddY);
                layout.Word((ushort)(8 >> phase));
            }
        }
        layout.Delete();
        layout.Start(Soul);
        layout.Pose(SoulHold); layout.Pose(SoulHold); layout.Goto(Soul);
        layout.Pose(DecorationInitialHold);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY);
        layout.Word(EnemyProjectileCodePointers.PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess);
        for (int actor = 0; actor < 3; actor++)
        {
            ushort loop = layout.Cursor;
            layout.Pose(StatueHold); layout.Goto(loop);
        }
        return layout.Result;
    }

    /// <summary>Walks the authored instruction layout while capturing one mechanics operand or pose-word address.</summary>
    /// <param name="requested">Zero-based operand position to capture during the walk.</param>
    /// <param name="visual">Whether the requested result is a pose-word address instead of a mechanics operand.</param>
    /// <param name="start">Initial bank-$86 address for the list being traversed.</param>
    private ref struct Layout(int requested, bool visual, ushort start)
    {
        /// <summary>Current bank-$86 cursor in the instruction stream being traversed.</summary>
        internal ushort Cursor { get; private set; } = start;
        /// <summary>Counts mechanics words and pose words encountered; the latter select presentation addresses.</summary>
        private int mechanics, presentation;
        /// <summary>The operand or presentation address captured at the requested position.</summary>
        internal InstructionMechanicsWord Result { get; private set; }
        /// <summary>Moves traversal to another native instruction-list entry.</summary>
        /// <param name="address">Bank-$86 address of the next list entry.</param>
        internal void Start(ushort address) => Cursor = address;
        /// <summary>Advances past a byte-sized operand that is not represented by the mechanics-word view.</summary>
        internal void SkipByte() => Cursor++;
        /// <summary>Accounts for a two-byte instruction operand and captures it when it matches the requested position.</summary>
        /// <param name="value">Value stored in the instruction word.</param>
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(Cursor, value);
            mechanics++; Cursor += sizeof(ushort);
        }
        /// <summary>Consumes a timed pose word and, in presentation mode, records its following data address.</summary>
        /// <param name="duration">Number of interpreter ticks for which the pose remains active.</param>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(Cursor, 0);
            presentation++; Cursor += sizeof(ushort);
        }
        /// <summary>Adds the native projectile-delete opcode to the traversed mechanics words.</summary>
        internal void Delete() => Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        /// <summary>Adds a native GotoY opcode and its destination operand.</summary>
        /// <param name="target">Bank-$86 address to which the instruction transfers control.</param>
        internal void Goto(ushort target)
        {
            Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY); Word(target);
        }
    }
    /// <summary>Determines whether a projectile kind is implemented by the compiled Tourian statue programs.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns><see langword="true"/> for one of the statue's splash, eye-glow, particle, tail, soul, statue, or decoration actors.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.TourianStatueSplash or
        RoomEnemyProjectileKind.TourianStatueEyeGlow or
        RoomEnemyProjectileKind.TourianStatueParticle or
        RoomEnemyProjectileKind.TourianStatueTail or
        RoomEnemyProjectileKind.TourianStatueSoul or
        RoomEnemyProjectileKind.TourianStatueRidley or
        RoomEnemyProjectileKind.TourianStatuePhantoon or
        RoomEnemyProjectileKind.TourianStatueBaseDecoration;

    /// <summary>Resolves a compiled mechanics-word address to the operand stored there.</summary>
    /// <param name="address">Bank-$86 address to look up.</param>
    /// <returns>The mechanics operand at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the compiled mechanics-word locations.</exception>
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
