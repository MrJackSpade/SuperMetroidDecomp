using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Where Samus's colours lie in her sheets' inks, and which of the frame's sprites show them (#1405).</summary>
internal static class SamusColourDefinitions
{
    /// <summary>The ink row Samus's suit is coloured in.</summary>
    public const int SuitInkRow = 4;

    /// <summary>The ink row whose last colour is the grapple flare's.</summary>
    public const int FlareInkRow = 5;

    /// <summary>The ink row Samus's beams are coloured in, which the Crystal Flash and Hyper Beam effects also show.</summary>
    public const int BeamInkRow = 6;

    /// <summary>The ink row Samus without her suit is coloured in.</summary>
    public const int SuitlessInkRow = 7;

    /// <summary>The sprite colour set Samus without her suit shows.</summary>
    public const int SuitlessSpriteSet = 7;

    /// <summary>The visor's colour within the suit.</summary>
    public const int VisorColour = SamusPaletteRomData.Common.VisorColorOffset;

    /// <summary>The first beam colour the Crystal Flash's bubble shows.</summary>
    public const int CrystalFlashBubbleFirstColour = 10;

    /// <summary>The first beam colour the Hyper Beam's cycle shows.</summary>
    public const int HyperBeamFirstColour = 1;

    /// <summary>The ink rows a sprite sheet is coloured in.</summary>
    public const int InkRows = 8;

    /// <summary>The colours in one ink row.</summary>
    public const int RowColours = 16;

    /// <summary>The beam colours the Crystal Flash's body shows, from the first.</summary>
    public const int CrystalFlashBodyColours = CrystalFlashColorFormat.BodyColorCount;

    /// <summary>The beam colours the Crystal Flash's bubble shows.</summary>
    public const int CrystalFlashBubbleColours = CrystalFlashColorFormat.BubbleColorCount;

    /// <summary>The beam colours the Hyper Beam's cycle shows.</summary>
    public const int HyperBeamColours = HyperBeamFxColorFormat.ColorsPerFrame;

    /// <summary>The sprites that show Samus's colours: her body, her equipment's and her weapons' draws.</summary>
    public static ImmutableHashSet<SpriteSourceKind> Sources { get; } =
    [
        SpriteSourceKind.Samus, SpriteSourceKind.SamusArmCannon, SpriteSourceKind.SamusAtmosphere, SpriteSourceKind.SamusBomb,
        SpriteSourceKind.SamusChargeFlare, SpriteSourceKind.SamusChest, SpriteSourceKind.SamusDeathExplosion,
        SpriteSourceKind.SamusEcho, SpriteSourceKind.SamusGrappleEndpoint, SpriteSourceKind.SamusGrappleFlare,
        SpriteSourceKind.SamusGrappleRope, SpriteSourceKind.SamusProjectile, SpriteSourceKind.SamusProjectileTrail,
    ];
}
