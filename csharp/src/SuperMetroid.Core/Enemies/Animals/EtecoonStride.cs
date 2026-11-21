using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The poses of an Etecoon's running stride facing one way (#2477): stretched, bunched, pushing off and bunched again. The
/// Etecoon's run and the escaping Etecoons' runs and thanking steps all play this one stride, each written once over a
/// facing's poses, as a Work Robot's strides are written once over its feet (#2451).
/// </summary>
/// <param name="Stretched">Stretched out mid-leap.</param>
/// <param name="Bunched">Bunched up between leaps.</param>
/// <param name="PushingOff">Pushing off into the next leap.</param>
internal sealed partial record EtecoonStride(F Stretched, F Bunched, F PushingOff)
{
    /// <summary>The stride running left.</summary>
    internal static EtecoonStride Left { get; } = new(F.EtecoonRunningLeftStretched, F.EtecoonRunningLeftBunched, F.EtecoonRunningLeftPushingOff);

    /// <summary>The stride running right.</summary>
    internal static EtecoonStride Right { get; } = new(F.EtecoonRunningRightStretched, F.EtecoonRunningRightBunched, F.EtecoonRunningRightPushingOff);

    /// <summary>The stride's poses in the order it shows them: bunched both before and after it pushes off.</summary>
    internal IReadOnlyList<F> Poses => [Stretched, Bunched, PushingOff, Bunched];
}
