namespace SuperMetroid.Core.Game;

/// <summary>
/// Per-physical-slot projection of Kraid's bank-$7E extended workspace. Native code obtains
/// these words by adding the enemy's byte index to the shared `$7800` base; keeping one
/// typed record per slot exposes the same aliasing without flattening it into mystery fields.
/// </summary>
public sealed class KraidPartState
{
    /// <summary>Where the part's typed program resumes; it replaces the native instruction pointer.</summary>
    internal AnimationProgramCursor<KraidPartProgram> Cursor { get; set; }

    /// <summary>
    /// Native `$7E:7800 + slot` aliases a function pointer with the second-phase foot's
    /// think timer. The raw word remains visible; the typed view is used only on functions.
    /// </summary>
    public ushort NextWord { get; internal set; }

    /// <summary>Gets the second-phase foot's think-timer view of the shared next-word alias.</summary>
    public NativeCountdown ThinkTimer
    {
        get => NativeCountdown.FromRaw(NextWord);
        internal set => NextWord = value.RawValue;
    }

    /// <summary>Gets the typed function view of the shared next-word alias when the current part treats it as a bank-$A7 pointer.</summary>
    public KraidAiFunction NextFunction
    {
        get => (KraidAiFunction)NextWord;
        internal set => NextWord = (ushort)value;
    }

    /// <summary>
    /// Fingernail side-selection word stored in the otherwise shared health-threshold area.
    /// It alternates body-side and fixed-left spawns when the sampled RNG chooses that path.
    /// </summary>
    public ushort AlternateSpawnFlag { get; internal set; }
}
