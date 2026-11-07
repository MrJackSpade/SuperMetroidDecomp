using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks how a room's encounters show their looks and how a frame names its finished picture of the look shown (#1538): a
/// room starts with every encounter in the look its room's load gives it, an encounter shows the look it last named, a
/// room load restarts every encounter, and a frame's picture names each changing encounter's look.
/// </summary>
internal static class EncounterLooksVerification
{
    /// <summary>Shows the gunship's and Bomb Torizo's looks as their encounters do and checks the looks shown and the picture names.</summary>
    internal static void VerifyEncounterLooks()
    {
        var looks = new EncounterLooks();
        AssertEqual(EncounterLook.Loaded(EncounterArtwork.Gunship), looks.Current(EncounterArtwork.Gunship), "an encounter starts in its room's load");
        AssertTrue(!looks.Shows(GunshipTakeoffDefinitions.TakeoffLook), "the takeoff look does not show before the takeoff");

        looks.Show(GunshipTakeoffDefinitions.TakeoffLook);
        AssertTrue(looks.Shows(GunshipTakeoffDefinitions.TakeoffLook), "the gunship shows the look it last named");
        AssertEqual(EncounterLook.Loaded(EncounterArtwork.BombTorizo), looks.Current(EncounterArtwork.BombTorizo), "another encounter keeps its own look");

        // A Torizo's swap names its look; the swap shown last is the look shown.
        EncounterLook crumble = new TorizoArtworkSwap(13).Look(EncounterArtwork.BombTorizo);
        EncounterLook gut = new TorizoArtworkSwap(0).Look(EncounterArtwork.BombTorizo);
        looks.Show(crumble);
        looks.Show(gut);
        AssertTrue(looks.Shows(gut) && !looks.Shows(crumble), "a Torizo shows the look of the swap it made last");
        AssertTrue(EncounterLookUploads.IndexOf(ComposedEncounter.BombTorizo, crumble.Name) < EncounterLookUploads.IndexOf(ComposedEncounter.BombTorizo, gut.Name),
            "the import composes the gut's look over the crumbled statue's");

        looks.Restart();
        AssertEqual(EncounterLook.Loaded(EncounterArtwork.Gunship), looks.Current(EncounterArtwork.Gunship), "a room load shows every encounter's room's load again");

        AssertEqual("frame", EncounterLook.PictureName("frame", []), "a frame no look changes is pictured under its own name");
        AssertEqual("frame|Gunship=Takeoff", EncounterLook.PictureName("frame", [GunshipTakeoffDefinitions.TakeoffLook]),
            "a changed frame's picture names the encounter and the look it shows");
        AssertThrows<ArgumentException>(() => new EncounterLook(EncounterArtwork.Gunship, "Take|off"), "a look's name holds no picture name's separator");
        Console.WriteLine("Encounter looks: encounters show the look they last named, restart per room, and name the finished pictures frames show.");
    }

    /// <summary>Runs the encounter looks check.</summary>
    [VerificationCommand("--encounter-looks")]
    internal static int EncounterLooksCommand()
    {
        Suite(nameof(VerifyEncounterLooks), VerifyEncounterLooks);
        return 0;
    }
}
