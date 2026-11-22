using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// The Tourian entrance statues grey out over the statue room's pictures (#2630): the room is Crateria's, and every block
/// sheet its states draw with is pre-coloured in every frame of the grey-outs of the statues drawn in the room's picture,
/// so colouring them does not throw as it did when Kraid's statue greyed out.
/// </summary>
internal static class StatueGreyOutShadesVerification
{
    /// <summary>
    /// The grey-outs of the statues drawn in the room's picture, Kraid's and Draygon's, which the statue room's unlocking
    /// animations spawn; Phantoon's and Ridley's statues are sprites, whose grey-outs recolour no room picture.
    /// </summary>
    private static readonly ColourEffectId[] GreyOuts =
    [
        ColourEffectId.GreyOutTourianStatueKraid, ColourEffectId.GreyOutTourianStatueDraygon,
    ];

    /// <summary>Checks each grey-out frame's shade on the statue room's block sheets.</summary>
    internal static void VerifyStatueRoomPicturesHoldGreyOuts()
    {
        PictureShade[] shades = [.. ColourEffectShownFrames.ShownBy(GreyOuts).Select(shown => ColourEffectShades.Of(shown.Colours, shown.Frame))];
        AssertTrue(shades.Length > 0, "the statues' grey-outs show frames");
        foreach (RoomStateSettings state in RoomStateLists.GetStates(RoomDefinitions.Statues))
        {
            AssertTrue(state.SetupBehavior == RoomSetupBehavior.RunStatueUnlockingAnimations, "the statue room's state runs the statues' unlocking animations");
            SpriteImage image = InstalledFixtures.RoomAssets.BlockSheets.Sheets[RoomTilesets.Get(state.GraphicsSet)].Sheet.Image;
            foreach (PictureShade shade in shades)
                AssertTrue(image.Shades.ContainsKey(shade), $"statue room picture {image.Name} is pre-coloured in grey-out shade {shade.Key}");
        }
    }

    /// <summary>Runs the statue room's grey-out shade check.</summary>
    [VerificationCommand("--statue-grey-out-shades")]
    internal static int StatueGreyOutShadesCommand()
    {
        Suite(nameof(VerifyStatueRoomPicturesHoldGreyOuts), VerifyStatueRoomPicturesHoldGreyOuts);
        return 0;
    }
}
