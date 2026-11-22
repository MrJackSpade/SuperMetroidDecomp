using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.World.RoomStates;

/// <summary>
/// What the import pictures for the room states' setups and mains (#2485): the sky each sky main scrolls, the mains that
/// set off the escape's random explosions, the setups that spawn the sky's drifting bands, and the states of Ceres
/// Ridley's arena. The simulation composes these behaviours and never asks what they show; only the import relates each to
/// the pictures it installs.
/// </summary>
internal static partial class RoomBehaviorPictureDefinitions
{
    /// <summary>The sky each scrolling-sky main scrolls behind its room; every other main scrolls none.</summary>
    internal static IReadOnlyDictionary<RoomMainBehavior, ScrollingSky> MainSkies { get; } =
        new Dictionary<RoomMainBehavior, ScrollingSky>
        {
            [RoomMainBehavior.ScrollingSkyLand] = ScrollingSky.Land,
            [RoomMainBehavior.ScrollingSkyOcean] = ScrollingSky.Ocean,
            [RoomMainBehavior.ScrollingSkyLandWithEscapeExplosions] = ScrollingSky.Land,
        };

    /// <summary>The mains that set off the escape's random explosions, whose pictures the import installs only for their rooms.</summary>
    internal static IReadOnlyList<RoomMainBehavior> EscapeExplosionMains { get; } =
    [
        RoomMainBehavior.ScrollingSkyLandWithEscapeExplosions, RoomMainBehavior.ShakeScreenWithRandomExplosions,
        RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal, RoomMainBehavior.RandomEscapeExplosions,
        RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal,
    ];

    /// <summary>The setups that spawn the scrolling sky's drifting bands, whichever door their room is entered through.</summary>
    internal static IReadOnlySet<RoomSetupBehavior> SkyBandSetups { get; } =
        new HashSet<RoomSetupBehavior> { RoomSetupBehavior.SpawnScrollingSkyBands, RoomSetupBehavior.LandSkyWithLandingSiteQuake };

    /// <summary>
    /// The two states of Ceres Ridley's arena, whose setup points the room's block characters at what the arena loads for
    /// his fight before the room's first frame ($8F:C97B), because the arena's tileset's characters are the turning floor
    /// of his getaway (#1357, #1648, #1791). Both of the arena's states run it, and no other state does.
    /// </summary>
    internal static IReadOnlySet<RoomStateSettings> CeresRidleyArenaStates { get; } =
        new HashSet<RoomStateSettings> { CeresRoomStateDefinitions.CeresRidleyDefault, CeresRoomStateDefinitions.CeresRidleyWhenAreaBossDead };
}
