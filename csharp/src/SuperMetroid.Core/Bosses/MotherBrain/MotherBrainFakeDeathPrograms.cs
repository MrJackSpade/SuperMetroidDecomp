namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Tells which layout each of Mother Brain's fake-death mutation programs draws.</summary>
internal static class MotherBrainFakeDeathPrograms
{
    /// <summary>The layout <paramref name="program"/> draws.</summary>
    internal static MotherBrainFakeDeathDraw Layout(MotherBrainFakeDeathProgram program) =>
        MotherBrainFakeDeathPlmProgramDefinitions.Layouts.TryGetValue(program, out MotherBrainFakeDeathDraw layout)
            ? layout
            : throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Mother Brain fake-death program.");
}
