namespace SuperMetroid.Core.Escape;

/// <summary>
/// What one update of the escape typewriter did (#2499): typed nothing, typed a glyph, typed a glyph and asked for the
/// key click, or found the warning complete. An update types at most one glyph, and only a typed glyph clicks.
/// </summary>
public abstract record EscapeTypewriterStep
{
    /// <summary>Only the steps below exist.</summary>
    private EscapeTypewriterStep()
    {
    }

    /// <summary>The update typed no glyph: it waited between characters, or it typed a space.</summary>
    public sealed record NothingTyped : EscapeTypewriterStep
    {
        /// <summary>The one update that typed nothing; every such update is the same.</summary>
        internal static NothingTyped Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private NothingTyped()
        {
        }
    }

    /// <summary>The update found every line typed; every later update finds the same, typing and clicking nothing.</summary>
    public sealed record Complete : EscapeTypewriterStep
    {
        /// <summary>The one completed step; every such update is the same.</summary>
        internal static Complete Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private Complete()
        {
        }
    }

    /// <summary>The update typed <paramref name="Glyph"/>, for the owner to write over its background.</summary>
    /// <param name="Glyph">The glyph typed.</param>
    public record Typed(EscapeTypewriterGlyph Glyph) : EscapeTypewriterStep;

    /// <summary>
    /// The update typed <paramref name="Glyph"/> and, as every second glyph does, asks for the key click; the Ceres or Zebes
    /// escape picks which click sound plays.
    /// </summary>
    /// <param name="Glyph">The glyph typed.</param>
    public sealed record TypedWithClick(EscapeTypewriterGlyph Glyph) : Typed(Glyph);
}
