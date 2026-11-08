namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The random fields a Beetom far from Samus reads to choose what it does next.</summary>
internal static class BeetomDecisions
{
    /// <summary>The random field that chooses a distant Beetom's next action among idling and the four hops: bits zero to two.</summary>
    internal static readonly RandomSampleField NextAction = new(0, 3);

    /// <summary>The random bit that turns a distant Beetom to face right: bit zero, the low bit of <see cref="NextAction"/>.</summary>
    internal static readonly RandomSampleField FaceRight = new(0, 1);
}
