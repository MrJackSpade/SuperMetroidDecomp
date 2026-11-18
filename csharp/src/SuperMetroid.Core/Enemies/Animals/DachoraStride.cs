using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The six poses of a Dachora's running stride facing one way (#2477): its far leg swinging back, passing and reaching, then
/// its near leg. The Dachora's runs at every pace and the escaping Dachora's laps and sprint all play this one stride, so
/// a run that quickens keeps its pose; each is written once over a facing's poses, as a Work Robot's strides are written once
/// over its feet (#2451).
/// </summary>
/// <param name="FarLegBack">Its far leg swung back.</param>
/// <param name="FarLegPassing">Its far leg passing under it.</param>
/// <param name="FarLegReaching">Its far leg reaching forward.</param>
/// <param name="NearLegBack">Its near leg swung back.</param>
/// <param name="NearLegPassing">Its near leg passing under it.</param>
/// <param name="NearLegReaching">Its near leg reaching forward.</param>
internal sealed record DachoraStride(F FarLegBack, F FarLegPassing, F FarLegReaching, F NearLegBack, F NearLegPassing, F NearLegReaching)
{
    /// <summary>The stride running left.</summary>
    internal static DachoraStride Left { get; } = new(
        F.DachoraRunningLeftFarLegBack, F.DachoraRunningLeftFarLegPassing, F.DachoraRunningLeftFarLegReaching,
        F.DachoraRunningLeftNearLegBack, F.DachoraRunningLeftNearLegPassing, F.DachoraRunningLeftNearLegReaching);

    /// <summary>The stride running right.</summary>
    internal static DachoraStride Right { get; } = new(
        F.DachoraRunningRightFarLegBack, F.DachoraRunningRightFarLegPassing, F.DachoraRunningRightFarLegReaching,
        F.DachoraRunningRightNearLegBack, F.DachoraRunningRightNearLegPassing, F.DachoraRunningRightNearLegReaching);

    /// <summary>The stride's poses in the order it shows them.</summary>
    internal IReadOnlyList<F> Poses => [FarLegBack, FarLegPassing, FarLegReaching, NearLegBack, NearLegPassing, NearLegReaching];
}
