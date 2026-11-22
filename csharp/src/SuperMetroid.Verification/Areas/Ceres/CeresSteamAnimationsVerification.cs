using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Samus;
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
        IImportCartridgeSource cartridge = rom;
        int beats = 0;
        foreach ((string name, CeresSteamWay way, NativeProgramEntry<CeresSteamSegment>[] entries) in Ways)
        {
            beats += NativeEnemyAnimationOracle.CompareAll(cartridge, Bank, entries, way.Segments, way.StartingAt,
                CeresSteamFrameDefinitionsConstants.Spritemap,
                new NativeEnemyCueRules<CeresSteamCueKind>(word => Instructions.ContainsKey(word) ? 0 : null,
                    (word, _, cue, at) => AssertEqual(Instructions[word], cue, $"{name} vent {at}: cue"),
                    OwnerLeavesAt: OwnerTakesOver.Contains, ShownUnseen: UnseenFrames.Contains), onlyRunOnInto: OnlyRunOnInto);

            AnimationPlayback<CeresSteamSegment> activation = way.Activation;
            ushort entry = NativeEnemyAnimationOracle.EntryOf(entries, CeresSteamSegment.Dormant);
            int list = (Bank << 16) | entry;
            // The dormant list hides the vent, shows a frame record for one update, then counts its wait down: while the
            // wait runs it replays the list, and once it runs out the vent appears and the puff plays ($A6:F127).
            AssertEqual(1, cartridge.ReadCartridgeWord(list + 2), $"{name} vent: the hidden frame record lasts one update, so the wait counts once an update");
            AssertEqual(CountDownRoutine, cartridge.ReadCartridgeWord(list + 6), $"{name} vent: the wait's count-down follows the frame record");
            AssertEqual(entry, cartridge.ReadCartridgeWord(list + 8), $"{name} vent: the count-down replays the dormant list while the wait runs");
            CeresSteamSegment puffing = activation.Animation.Phases[0].Segment;
            AssertEqual(NativeEnemyAnimationOracle.EntryOf(entries, puffing), cartridge.ReadCartridgeWord(list + 10),
                $"{name} vent: the count-down goes on into the puff the activation plays");

            // The activation appears the vent, as the count-down's end runs on into the appearing routine, and shows the
            // puff's first picture on the same update.
            var raised = new List<CeresSteamCueKind>();
            AnimationPlace place = activation.Place;
            AnimationShown<CeresSteamSegment, EnemyFrameId> shown = way.Segments.PlayToNextFrame(activation.Animation, ref place, raised.Add);
            AssertTrue(raised.SequenceEqual([CeresSteamCueKind.Appear]), $"{name} vent: the activation appears the vent and raises nothing else");
            AssertEqual(way.Segments.Beats(puffing)[0].ShownFrame,
                AssertIs<AnimationShown<CeresSteamSegment, EnemyFrameId>.Frame>(shown, $"{name} vent: the activation's first frame").Shown,
                $"{name} vent: the activation shows the puff's first picture");
        }
        Console.WriteLine($"  Ceres steam animations: {Ways.Sum(listed => listed.Entries.Length)} segments ({beats} beats) match bank $A6, and the dormant count-downs go on into their puffs.");
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
        vent.SetSpawnParameter1(EnemyDefinitionId.SteamVent, (ushort)CeresSteamVariant.Up);
        vent.Bind(EnemyDefinitionId.SteamVent, vent.KeptDefinition);
        // The vent is set up as the population load sets it up, its species building its entity.
        _ = enemies.LoadAuthoredPopulation();

        bool Hidden() => vent.Presence.HasAny(EnemyPresence.Hidden) && vent.Contact.HasAny(EnemyContact.IgnoresSamus);
        int update = 0;
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        void Advance() { update++; vent.AdvanceAnimation(samus); }

        for (int dormant = 1; dormant <= wait; dormant++)
        {
            Advance();
            AssertTrue(Hidden(), $"update {update}: the dormant vent stays hidden through its wait");
        }
        Advance();
        AssertTrue(!Hidden(), $"update {update}: the vent appears, harmful, on the update after its wait's last count");
        AssertEqual<EnemyFrameId?>(EnemyFrameId.CeresSteamUp, vent.Frame, $"update {update}: the vent shows its puff's first picture as it appears");

        int puffUpdates = CeresSteamWay.Up.Segments.Beats(CeresSteamSegment.Puffing).Count * CeresSteamAnimations.PuffPictureUpdates;
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
