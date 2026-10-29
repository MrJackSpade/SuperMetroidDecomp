using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The colour runs the <see cref="RidleyColours"/> view names (#1405).</summary>
public static class RidleyColourDefinitions
{
    /// <summary>Ceres door/container and Baby palettes: the second enemy store's colours, then the third's.</summary>
    public static readonly ImmutableArray<SpriteColourRun> CeresStart =
        [new(SpriteColourStore.EnemiesSecond, 0, 16), new(SpriteColourStore.EnemiesThird, 0, 16)];

    /// <summary>Ceres eye colours.</summary>
    public static readonly SpriteColourRun CeresEyeFade = new(SpriteColourStore.EnemiesFourth, 12, 3);

    /// <summary>Ceres body fade, the first enemy store's copy.</summary>
    public static readonly SpriteColourRun CeresBodyFadeShadow = new(SpriteColourStore.EnemiesFirst, 1, 11);

    /// <summary>Ceres body fade, in the fourth enemy store.</summary>
    public static readonly SpriteColourRun CeresBodyFade = new(SpriteColourStore.EnemiesFourth, 1, 11);

    /// <summary>Shared health shade.</summary>
    public static readonly SpriteColourRun Health = new(SpriteColourStore.EnemiesFourth, 1, 14);

    /// <summary>Ceres alarm text.</summary>
    public static readonly BackgroundColourRun CeresAlarm = new(BackgroundColourSet.TilesFifth, 1, 3);

    /// <summary>Ceres getaway backdrop.</summary>
    public static readonly BackgroundColourRun CeresRetreatBackground = new(BackgroundColourSet.TilesFourth, 1, 15);

    /// <summary>Ceres shared retreat shade, BG copy.</summary>
    public static readonly BackgroundColourRun CeresRetreatSharedBackground = new(BackgroundColourSet.TilesFirst, 1, 8);

    /// <summary>Ceres shared retreat shade, OBJ copy.</summary>
    public static readonly SpriteColourRun CeresRetreatSharedObject = new(SpriteColourStore.EnemiesFourth, 1, 8);

    /// <summary>Ceres Baby shade.</summary>
    public static readonly SpriteColourRun CeresBaby = new(SpriteColourStore.EnemiesThird, 1, 15);

    /// <summary>Ceres Mode 7 zoom shade.</summary>
    public static readonly BackgroundColourRun CeresFloorZoom = new(BackgroundColourSet.TilesFourth, 1, 15);

    /// <summary>Norfair initial palettes: the second enemy store's colours, then the third's.</summary>
    public static readonly ImmutableArray<SpriteColourRun> NorfairInitial =
        [new(SpriteColourStore.EnemiesSecond, 0, 16), new(SpriteColourStore.EnemiesThird, 0, 16)];

    /// <summary>The first set whose colours the self-destruct warning keeps before its graphics replace their tiles.</summary>
    public const BackgroundColourSet CeresWarningFirstSource = BackgroundColourSet.Hud;

    /// <summary>The set the warning copies <see cref="CeresWarningFirstSource"/>'s kept colours into.</summary>
    public const BackgroundColourSet CeresWarningFirstDestination = BackgroundColourSet.TilesFifth;

    /// <summary>The second set whose colours the self-destruct warning keeps before its graphics replace their tiles.</summary>
    public const BackgroundColourSet CeresWarningSecondSource = BackgroundColourSet.HudItemsAndEffects;

    /// <summary>The set the warning copies <see cref="CeresWarningSecondSource"/>'s kept colours into.</summary>
    public const BackgroundColourSet CeresWarningSecondDestination = BackgroundColourSet.TilesFourth;

    /// <summary>Norfair reveal fade.</summary>
    public static readonly BackgroundColourRun NorfairReveal = new(BackgroundColourSet.TilesSixth, 1, 14);
}
