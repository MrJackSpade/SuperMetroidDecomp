using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// The frames Golden Torizo's right-orb program selects, in native order: the frames the
    /// former per-program collision catalog duplicated. Collision itself comes from the single
    /// TorizoCollisionDefinitions owner.
    /// </summary>
    private static ushort[] GoldenTorizoRightOrbFrames()
    {
        var frames = new List<ushort>();
        for (int index = 0; index < GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(SuperMetroid.Core.Assets.CompiledEnemyVisualSelectors.TryGet(
                    TorizoCollisionDefinitions.Bank, operand, out ushort frame),
                $"Golden Torizo right-orb selector $AA:{operand:X4} is compiled");
            if (!frames.Contains(frame)) frames.Add(frame);
        }
        frames.Sort();
        return [.. frames];
    }
}
