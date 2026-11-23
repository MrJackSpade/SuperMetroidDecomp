using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// The escape's red flashes start over Mother Brain's room's pictures (#2638): her third phase's death starts them in her
/// room, which no FX record of hers names, and every block sheet her room's states draw with is pre-coloured in every
/// frame of the flashes over the room's own colour groups, so colouring them does not throw as the escape starts.
/// </summary>
internal static class MotherBrainEscapeFlashShadesVerification
{
    /// <summary>
    /// The escape's flashes over the room picture's own colour groups (its background, level and Arkanoid blocks and orbs);
    /// the shutters' flash recolours the shutters, which no room picture draws.
    /// </summary>
    private static readonly ColourEffectId[] Flashes =
    [
        ColourEffectId.TourianBackgroundRedFlashing, ColourEffectId.TourianGeneralLevelRedFlashing,
        ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs,
    ];

    /// <summary>Checks each flash frame's shade on Mother Brain's room's block sheets.</summary>
    internal static void VerifyMotherBrainRoomPicturesHoldEscapeFlashes()
    {
        PictureShade[] shades = [.. ColourEffectShownFrames.ShownBy(Flashes).Select(shown => ColourEffectShades.Of(shown.Colours, shown.Frame))];
        AssertTrue(shades.Length > 0, "the escape's flashes show frames");
        foreach (RoomStateSettings state in RoomStateLists.GetStates(RoomDefinitions.MotherBrain))
        {
            SpriteImage image = InstalledFixtures.RoomAssets.BlockSheets.Sheets[RoomTilesets.Get(state.GraphicsSet)].Sheet.Image;
            foreach (PictureShade shade in shades)
                AssertTrue(image.Shades.ContainsKey(shade), $"Mother Brain's room picture {image.Name} is pre-coloured in escape flash shade {shade.Key}");
        }
    }

    /// <summary>Runs Mother Brain's room's escape flash shade check.</summary>
    [VerificationCommand("--mother-brain-escape-flash-shades")]
    internal static int MotherBrainEscapeFlashShadesCommand()
    {
        Suite(nameof(VerifyMotherBrainRoomPicturesHoldEscapeFlashes), VerifyMotherBrainRoomPicturesHoldEscapeFlashes);
        return 0;
    }
}
