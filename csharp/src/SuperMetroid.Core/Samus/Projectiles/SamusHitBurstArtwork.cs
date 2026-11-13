using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What the burst a hit of Samus's weapon shows at an enemy it hurts draws (#1713): it is one of her weapon effects, so it
/// shows the sheet of the beams she has equipped, as her charge flare does (#1586), and its draw is one of hers, which
/// shows her beams' current colouring with her other draws.
/// </summary>
/// <param name="Objects">The context's projectile sheets.</param>
/// <param name="EquippedBeams">The beam combination she has equipped as the burst is drawn.</param>
public readonly record struct SamusHitBurstArtwork(ProjectileDrawArtwork Objects, SamusBeamCombination EquippedBeams)
{
    /// <summary>Publishes the burst's composition <paramref name="picture"/> centred on <paramref name="origin"/>.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="instance">The burst's position among the room's live sprite objects, in spawn order.</param>
    /// <param name="picture">The burst composition its animation shows.</param>
    /// <param name="origin">Where its centre lies on screen.</param>
    /// <exception cref="InvalidOperationException">No projectile sheets are installed.</exception>
    internal void Draw(SpriteFrameBuffer sprites, int instance, ProjectileSpriteId picture, Position16<ScreenPixels> origin)
    {
        Ensure.NotNull(sprites);
        SpriteSheetFrame image = Objects.SheetOf(EquippedBeams).Frame(ProjectileSheetFrames.HitBurstFrame(picture));
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusHitBurst, instance), image, origin, SpriteMirror.None));
    }
}
