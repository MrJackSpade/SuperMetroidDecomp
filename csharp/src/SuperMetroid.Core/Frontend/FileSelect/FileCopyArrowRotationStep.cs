namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// One step of the file-copy arrows' colour rotation (#2361): each moves the arrows' colours one place along, and the
/// first follows the last.
/// </summary>
public readonly partial record struct FileCopyArrowRotationStep
{
    /// <summary>The step at <paramref name="step"/> of the rotation.</summary>
    private FileCopyArrowRotationStep(int step) => Step = step;

    /// <summary>The step's place in the rotation, from zero: how many places the colours have moved, and the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>Every step of the rotation, in the order the arrows show them.</summary>
    public static IReadOnlyList<FileCopyArrowRotationStep> All { get; } =
        [.. Enumerable.Range(0, FileCopyArrowDefinitions.RotationSteps).Select(step => new FileCopyArrowRotationStep(step))];

    /// <summary>The arrows' own colours, before any rotation.</summary>
    public static FileCopyArrowRotationStep Unrotated => All[0];

    /// <summary>The step after this one: the first follows the last.</summary>
    public FileCopyArrowRotationStep Next => All[(Step + 1) % All.Count];
}
