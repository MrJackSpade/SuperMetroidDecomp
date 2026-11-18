using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Areas.Ceres.CeresSteamNativeListAddresses;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>#1290: the Ceres steam vents' typed animations, each matched step by step to its native bank-$A6 list.</summary>
internal static class CeresSteamAnimationsVerification
{
    /// <summary>
    /// Every segment matches its native list and every animation the lists' control flow; each dormant list's count-down
    /// replays the list and goes on into the puff the owner's activation plays (#2480).
    /// </summary>
    internal static void VerifyCeresSteamAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int beats = NativeEnemyAnimationOracle.CompareAll(cartridge, Bank, Entries, CeresSteamAnimations.Segments, CeresSteamAnimations.StartingAt,
            CeresSteamFrameDefinitionsConstants.Spritemap,
            new NativeEnemyCueRules<CeresSteamCueKind>(word => Instructions.ContainsKey(word) ? 0 : null,
                (word, _, cue, at) => AssertEqual(Instructions[word], cue, $"{at}: cue"),
                OwnerLeavesAt: OwnerTakesOver.Contains, ShownUnseen: UnseenFrames.Contains), onlyRunOnInto: OnlyRunOnInto);

        foreach ((CeresSteamSegment dormant, AnimationPlayback<CeresSteamSegment> activation) in CeresSteamAnimations.ActivationAfter)
        {
            ushort entry = NativeEnemyAnimationOracle.EntryOf(Entries, dormant);
            int list = (Bank << 16) | entry;
            // The dormant list hides the vent, shows a frame record for one update, then counts its wait down: while the
            // wait runs it replays the list, and once it runs out the vent appears and the puff plays ($A6:F127).
            AssertEqual(1, cartridge.ReadCartridgeWord(list + 2), $"{dormant}: the hidden frame record lasts one update, so the wait counts once an update");
            AssertEqual(CountDownRoutine, cartridge.ReadCartridgeWord(list + 6), $"{dormant}: the wait's count-down follows the frame record");
            AssertEqual(entry, cartridge.ReadCartridgeWord(list + 8), $"{dormant}: the count-down replays the dormant list while the wait runs");
            CeresSteamSegment puffing = activation.Animation.Phases[0].Segment;
            AssertEqual(NativeEnemyAnimationOracle.EntryOf(Entries, puffing), cartridge.ReadCartridgeWord(list + 10),
                $"{dormant}: the count-down goes on into the puff the activation plays");

            // The activation appears the vent, as the count-down's end runs on into the appearing routine, and shows the
            // puff's first picture on the same update.
            var raised = new List<CeresSteamCueKind>();
            AnimationPlace place = activation.Place;
            AnimationShown<CeresSteamSegment, EnemyFrameId> shown = CeresSteamAnimations.Segments.PlayToNextFrame(activation.Animation, ref place, raised.Add);
            AssertTrue(raised.SequenceEqual([CeresSteamCueKind.Appear]), $"{dormant}: the activation appears the vent and raises nothing else");
            AssertEqual(CeresSteamAnimations.Segments.Beats(puffing)[0].ShownFrame,
                AssertIs<AnimationShown<CeresSteamSegment, EnemyFrameId>.Frame>(shown, $"{dormant}: the activation's first frame").Shown,
                $"{dormant}: the activation shows the puff's first picture");
        }
        Console.WriteLine($"  Ceres steam animations: {Entries.Length} segments ({beats} beats) match bank $A6, and the dormant count-downs go on into their puffs.");
    }

    /// <summary>
    /// #2480: a dormant vent hides on its first update and stays hidden for its drawn wait, appearing with its puff's first
    /// picture on the wait's last update, as the cartridge's list counts the wait on the updates after it hides; after its
    /// puff, it hides for its 64-update rest and appears with the puff again as the rest ends.
    /// </summary>
    internal static void VerifyCeresSteamDormantWait()
    {
        // A drawn wait of five updates: the cartridge's selection, four, plus one.
        const int wait = 5;
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => wait - 1;
        RoomEnemyPlace vent = enemies.Places.InUpdateOrder[0];
        vent.SetSpawnParameter1(EnemyDefinitionId.CeresSteam, (ushort)CeresSteamVariant.Up);
        vent.Bind(EnemyDefinitionId.CeresSteam, vent.Definition);
        ((IEnemyInitialization)(vent.Entity ?? throw new InvalidOperationException("The vent bound no entity."))).Initialize();
        AssertEqual(wait, vent.RequireSpeciesState<CeresSteamEnemyState>().UpdatesToActivation, "the vent draws its wait");

        bool Hidden() => vent.Presence.HasAny(EnemyPresence.Hidden) && vent.Contact.HasAny(EnemyContact.IgnoresSamus);
        int update = 0;
        void Advance() { update++; CeresSteamBehavior.AdvanceCeresSteamAnimation(vent); }

        for (int dormant = 1; dormant <= wait; dormant++)
        {
            Advance();
            AssertTrue(Hidden(), $"update {update}: the dormant vent stays hidden through its wait");
        }
        Advance();
        AssertTrue(!Hidden(), $"update {update}: the vent appears, harmful, on the update after its wait's last count");
        AssertEqual<EnemyFrameId?>(EnemyFrameId.CeresSteamUp, vent.Frame, $"update {update}: the vent shows its puff's first picture as it appears");

        int puffUpdates = CeresSteamAnimations.Segments.Beats(CeresSteamSegment.UpPuffing).Count * CeresSteamAnimations.PuffPictureUpdates;
        for (int puff = 1; puff < puffUpdates; puff++)
        {
            Advance();
            AssertTrue(!Hidden(), $"update {update}: the vent stays shown through its puff");
        }
        for (int rest = 0; rest < CeresSteamAnimations.RestUpdates; rest++)
        {
            Advance();
            AssertTrue(Hidden(), $"update {update}: the vent stays hidden through its rest");
        }
        Advance();
        AssertTrue(!Hidden(), $"update {update}: the vent appears again as its rest ends");
        AssertEqual<EnemyFrameId?>(EnemyFrameId.CeresSteamUp, vent.Frame, $"update {update}: the vent puffs again as it appears");
        Console.WriteLine($"  Ceres steam dormant wait: hidden {wait} updates, puffed {puffUpdates}, rested {CeresSteamAnimations.RestUpdates}, puffed again.");
    }
}
