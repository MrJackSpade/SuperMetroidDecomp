using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks how an encounter's artwork sequence advances and how a frame names its picture of the step reached (#1513): a
/// room starts with no step reached, a look shown again takes its next step or, coming round, its first, and a frame shows
/// the picture its variation gives the step reached, or its loaded picture.
/// </summary>
internal static class EncounterArtworkProgressVerification
{
    /// <summary>Advances Bomb Torizo's and Mother Brain's sequences as their programs do and checks the steps and picture names.</summary>
    internal static void VerifyEncounterArtworkProgress()
    {
        var progress = new EncounterArtworkProgress();
        AssertEqual<int?>(null, progress.StepOf(EncounterArtworkSequence.BombTorizo), "no step is reached before the first is shown");
        AssertTrue(!progress.Reached(EncounterArtworkSequence.CeresEscapePages, 0), "no page is reached before the first is shown");

        // Bomb Torizo's statue crumbles (13 to 16, then 17 to 20 twice), then its gut blows up (0 to 3).
        IReadOnlyList<TorizoArtworkSwap> bomb = TorizoArtworkSwapDefinitions.BombSequence;
        int StepOfSwap(int ordinal) => bomb.ToList().IndexOf(new TorizoArtworkSwap(ordinal));
        foreach (int ordinal in new[] { 13, 14, 15, 16, 17, 18, 19, 20 })
            progress.Show(EncounterArtworkSequence.BombTorizo, bomb, new TorizoArtworkSwap(ordinal));
        AssertEqual<int?>(StepOfSwap(20), progress.StepOf(EncounterArtworkSequence.BombTorizo), "the crumble's first pass reaches its last swap");
        progress.Show(EncounterArtworkSequence.BombTorizo, bomb, new TorizoArtworkSwap(17));
        AssertEqual<int?>(StepOfSwap(17), progress.StepOf(EncounterArtworkSequence.BombTorizo), "the crumble coming round takes its first step again");
        foreach (int ordinal in new[] { 18, 19, 20, 0, 1, 2, 3 })
            progress.Show(EncounterArtworkSequence.BombTorizo, bomb, new TorizoArtworkSwap(ordinal));
        AssertEqual<int?>(StepOfSwap(3), progress.StepOf(EncounterArtworkSequence.BombTorizo), "the gut's swaps follow the crumble's");
        AssertThrows<InvalidDataException>(() => progress.Show(EncounterArtworkSequence.BombTorizo, bomb, new TorizoArtworkSwap(21)),
            "a swap Bomb Torizo's programs never make has no step");

        // Mother Brain shows her attack pages after her legs, and again after the Baby.
        IReadOnlyList<MotherBrainSpritePage> pages = MotherBrainSpritePageDefinitions.Sequence;
        MotherBrainSpritePage attack = MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.Attack, 0);
        int firstAttack = pages.ToList().IndexOf(attack);
        int secondAttack = pages.ToList().FindIndex(firstAttack + 1, page => page == attack);
        AssertTrue(secondAttack > firstAttack, "Mother Brain's sequence shows her attack pages twice");
        progress.Show(EncounterArtworkSequence.MotherBrainPages, pages, attack);
        AssertEqual<int?>(firstAttack, progress.StepOf(EncounterArtworkSequence.MotherBrainPages), "the attack pages after the legs take their first steps");
        progress.Show(EncounterArtworkSequence.MotherBrainPages, pages, MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.BabyMetroid, 3));
        progress.Show(EncounterArtworkSequence.MotherBrainPages, pages, attack);
        AssertEqual<int?>(secondAttack, progress.StepOf(EncounterArtworkSequence.MotherBrainPages), "the attack pages after the Baby take their next steps");

        progress.Show(EncounterArtworkSequence.CeresEscapePages, 2);
        AssertTrue(progress.Reached(EncounterArtworkSequence.CeresEscapePages, 2) && !progress.Reached(EncounterArtworkSequence.CeresEscapePages, 3),
            "a page is reached once its step is");
        progress.Restart();
        AssertEqual<int?>(null, progress.StepOf(EncounterArtworkSequence.MotherBrainPages), "a room load starts every sequence afresh");

        // A frame shows its loaded picture until a step changes it, and a step that leaves it alike shares an earlier picture.
        var variation = new FrameArtworkVariation(EncounterArtworkSequence.CrocomireSkeleton, [null, 1, 1, 3]);
        AssertEqual<int?>(null, variation.PictureAt(0), "a step that leaves the frame as loaded shows its loaded picture");
        AssertEqual<int?>(1, variation.PictureAt(2), "a step that leaves the frame alike shares the earlier step's picture");
        AssertThrows<InvalidDataException>(() => variation.PictureAt(4), "a step past the sequence has no picture");
        AssertEqual("frame", FrameArtworkVariation.PictureName("frame", []), "the loaded picture is the frame's own name");
        AssertEqual("frame|CrocomireSkeleton=3", FrameArtworkVariation.PictureName("frame", [new EncounterArtworkStep(EncounterArtworkSequence.CrocomireSkeleton, 3)]),
            "a step's picture names the sequence and the step it shows");
        Console.WriteLine("Encounter artwork progress: steps advance, come round, restart per room, and name the pictures frames show.");
    }

    /// <summary>Runs the encounter artwork progress check.</summary>
    [VerificationCommand("--encounter-artwork-progress")]
    internal static int EncounterArtworkProgressCommand()
    {
        Suite(nameof(VerifyEncounterArtworkProgress), VerifyEncounterArtworkProgress);
        return 0;
    }
}
