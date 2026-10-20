namespace SuperMetroid.Core.Programs;

/// <summary>The result of advancing a program: a new frame, a wait, a hold or a stop.</summary>
/// <typeparam name="TFrame">The set's frame identity.</typeparam>
internal abstract record AnimationProgramAdvance<TFrame>
    where TFrame : struct
{
    /// <summary>Closes the hierarchy to the results declared here.</summary>
    private AnimationProgramAdvance()
    {
    }

    /// <summary>A new frame is shown for <see cref="Duration"/> updates.</summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Duration">The updates until the next advance.</param>
    internal sealed record Show(TFrame Frame, int Duration) : AnimationProgramAdvance<TFrame>;

    /// <summary>The current frame stays for <see cref="Duration"/> more updates.</summary>
    /// <param name="Duration">The updates until the next advance.</param>
    internal sealed record Wait(int Duration) : AnimationProgramAdvance<TFrame>;

    /// <summary>The program holds its last frame for good.</summary>
    internal sealed record Hold : AnimationProgramAdvance<TFrame>
    {
        /// <summary>The one hold result.</summary>
        internal static Hold Instance { get; } = new();
    }

    /// <summary>The program stopped; its owner deletes the actor.</summary>
    internal sealed record Stop : AnimationProgramAdvance<TFrame>
    {
        /// <summary>The one stop result.</summary>
        internal static Stop Instance { get; } = new();
    }
}
