using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The poses of an Alcoon's fireball spit facing one way (#2477): its mouth closed, opening, its throat glowing and its
/// mouth wide as the fireball leaves. Its three spits each way are written once over a facing's poses, as a Work Robot's
/// strides are written once over its feet (#2451).
/// </summary>
/// <param name="MouthClosed">Its mouth closed before it spits.</param>
/// <param name="MouthOpening">Its mouth opening, before and after its throat glows.</param>
/// <param name="ThroatGlowing">Its throat glowing with the fireball.</param>
/// <param name="MouthWide">Its mouth wide as the fireball leaves.</param>
internal sealed partial record AlcoonSpitPoses(F MouthClosed, F MouthOpening, F ThroatGlowing, F MouthWide)
{
    /// <summary>The poses facing left.</summary>
    internal static AlcoonSpitPoses Left { get; } = new(
        F.AlcoonFacingLeftMouthClosed, F.AlcoonFacingLeftMouthOpening, F.AlcoonFacingLeftThroatGlowing, F.AlcoonFacingLeftMouthWide);

    /// <summary>The poses facing right.</summary>
    internal static AlcoonSpitPoses Right { get; } = new(
        F.AlcoonFacingRightMouthClosed, F.AlcoonFacingRightMouthOpening, F.AlcoonFacingRightThroatGlowing, F.AlcoonFacingRightMouthWide);
}
