using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Which of her sheets' image colour groups show Samus's colours, and which of the frame's sprites show them (#1405,
/// #1464). Besides <see cref="SamusColours"/> only the composers of her sheets read the groups.
/// </summary>
internal static class SamusColourDefinitions
{
    /// <summary>The image colour group of her sheets Samus's suit is coloured in.</summary>
    public static readonly ImageColourGroup SuitColourGroup = ImageColourGroup.Every[4];

    /// <summary>The image colour group of her sheets whose last colour is the grapple flare's.</summary>
    public static readonly ImageColourGroup FlareColourGroup = ImageColourGroup.Every[5];

    /// <summary>The image colour group of her sheets Samus's beams are coloured in, which the Crystal Flash and Hyper Beam effects also show.</summary>
    public static readonly ImageColourGroup BeamColourGroup = ImageColourGroup.Every[6];

    /// <summary>The image colour group of her sheets Samus without her suit is coloured in.</summary>
    public static readonly ImageColourGroup SuitlessColourGroup = ImageColourGroup.Every[7];

    /// <summary>The sprite colour store whose colours Samus's suit shows over its own.</summary>
    public static readonly SpriteColourStore SuitStore = SpriteColourStore.SamusSuit;

    /// <summary>The sprite colour store Samus without her suit shows.</summary>
    public static readonly SpriteColourStore SuitlessStore = SpriteColourStore.EnemiesFourth;

    /// <summary>The sprite colour store whose last colour is Samus's grapple flare: the enemy projectiles' store.</summary>
    public static readonly SpriteColourStore FlareStore = SpriteColourStore.EnemyProjectiles;

    /// <summary>The grapple flare's colour within its sprite set.</summary>
    public const int FlareColour = 15;

    /// <summary>The visor's colour within the suit.</summary>
    public const int VisorColour = 4;

    /// <summary>The first beam colour the Crystal Flash's bubble shows.</summary>
    public const int CrystalFlashBubbleFirstColour = 10;

    /// <summary>The first beam colour the Hyper Beam's cycle shows.</summary>
    public const int HyperBeamFirstColour = 1;

    /// <summary>The colours of one of Samus's colour runs, such as her suit's or her beams'.</summary>
    public const int RowColours = 16;

    /// <summary>The beam colours the Crystal Flash's body shows, from the first.</summary>
    public const int CrystalFlashBodyColours = CrystalFlashColorFormat.BodyColorCount;

    /// <summary>The beam colours the Crystal Flash's bubble shows.</summary>
    public const int CrystalFlashBubbleColours = CrystalFlashColorFormat.BubbleColorCount;

    /// <summary>The beam colours the Hyper Beam's cycle shows.</summary>
    public const int HyperBeamColours = 8;

    /// <summary>The sprites that show Samus's colours: her body, her equipment's and her weapons' draws.</summary>
    public static ImmutableHashSet<SpriteSourceKind> Sources { get; } =
    [
        SpriteSourceKind.Samus, SpriteSourceKind.SamusArmCannon, SpriteSourceKind.SamusAtmosphere, SpriteSourceKind.SamusBomb,
        SpriteSourceKind.SamusChargeFlare, SpriteSourceKind.SamusChest, SpriteSourceKind.SamusDeathExplosion,
        SpriteSourceKind.SamusEcho, SpriteSourceKind.SamusGrappleEndpoint, SpriteSourceKind.SamusGrappleFlare,
        SpriteSourceKind.SamusGrappleRope, SpriteSourceKind.SamusProjectile, SpriteSourceKind.SamusProjectileTrail,
    ];
}
