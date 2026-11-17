using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's three pieces, which its top hull found when it was placed (#1542) and keeps: the room lists the top
/// hull, then the bottom hull, then the entrance pad. The ship's lights glow on all three (#1733).
/// </summary>
/// <param name="Top">The top hull, which runs the ship.</param>
/// <param name="Bottom">The bottom hull, listed right after the top.</param>
/// <param name="EntrancePad">The entrance pad, listed right after the bottom.</param>
internal sealed record GunshipPieces(RoomEnemyPlace Top, RoomEnemyPlace Bottom, RoomEnemyPlace EntrancePad) : IGlowingEnemy
{
    /// <summary>The three pieces in the room's order, which the ship moves together.</summary>
    internal IEnumerable<RoomEnemyPlace> All
    {
        get
        {
            yield return Top;
            yield return Bottom;
            yield return EntrancePad;
        }
    }

    /// <summary>
    /// Shows frame <paramref name="frame"/> of the lights' glow (#1733) on all three pieces, which draw their lights in
    /// the colour the glow recolours; each frame replaces the one before it. The glow reaches the gunship that started it
    /// (#1783).
    /// </summary>
    public void ShowGlow(ColourEffectFrame frame)
    {
        PictureShading glow = PictureShading.Shaded(ColourEffectShades.Of(ColourEffectColours.GunshipGlow, frame));
        foreach (RoomEnemyPlace piece in All)
            piece.Shading = glow;
    }
}
