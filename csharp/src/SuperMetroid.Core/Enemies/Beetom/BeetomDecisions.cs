namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The random fields a Beetom far from Samus reads to choose what it does next.</summary>
internal static class BeetomDecisions
{
    /// <summary>The random field that chooses a distant Beetom's next action among idling and the four hops: the sample modulo 8.</summary>
    internal static readonly RandomSampleField NextAction = new(0, 3);

    /// <summary>The random choice that turns a distant Beetom to face right: an odd sample, the parity of <see cref="NextAction"/>'s choice, so an odd action choice faces right.</summary>
    internal static readonly RandomSampleField FaceRight = new(0, 1);
}
