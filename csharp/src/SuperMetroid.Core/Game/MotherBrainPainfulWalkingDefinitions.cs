namespace SuperMetroid.Core.Game;

/// <summary>Paired forward/backward stages of Mother Brain's drained stagger.</summary>
internal static class MotherBrainPainfulWalkingDefinitions
{
    /// <summary>$A9:BEEE/$BEFE/$C049 each describe eight stages, paired by walking direction.</summary>
    internal const int StageCount = 8;

    /// <summary>$A9:BEEE: the first pair is fastest; subsequent pairs add two animation ticks.</summary>
    internal static ushort AnimationDelay(int stage)
    {
        int pair = Pair(stage);
        return (ushort)(pair == 0 ? 2 : 4 + pair * 2);
    }

    /// <summary>$A9:BEFE: each pair halves angular scale while its gain decreases from five to two.</summary>
    internal static ushort NeckAngleDelta(int stage)
    {
        int pair = Pair(stage);
        return (ushort)(((5 - pair) << 8) >> pair);
    }

    /// <summary>$A9:C049: each successive forward/backward pair adds sixteen pause ticks.</summary>
    internal static ushort FunctionTimer(int stage) => (ushort)((Pair(stage) + 1) * 16);

    /// <summary>Maps a forward/backward walking stage to its shared zero-based timing and angle pair.</summary>
    /// <param name="stage">Walking stage from zero through <see cref="StageCount"/> minus one.</param>
    /// <exception cref="IndexOutOfRangeException">The stage is outside the compiled walking sequence.</exception>
    private static int Pair(int stage) => (uint)stage < StageCount
        ? stage / 2 : throw new IndexOutOfRangeException();
}
