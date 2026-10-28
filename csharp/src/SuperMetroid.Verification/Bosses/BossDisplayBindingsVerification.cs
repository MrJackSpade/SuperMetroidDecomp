using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsClippingVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsDocumentsVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsFixtureVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsResourcesVerification;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBodyPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks editable boss OAM and BG2 selections against unchanged animation and collision mechanics.</summary>
internal static class BossDisplayBindingsVerification
{
    /// <summary>Acceptance for the source-identified mismatch between selected OAM and physical BG2.</summary>
    internal static void VerifyBossDisplayBindings()
    {
        var stock = new BossDisplayDocuments(edited: false);
        var edits = new BossDisplayDocuments(edited: true);
        int frames = 0, hitSamples = 0, hits = 0;
        foreach ((EnemyDefinitionId definition, EnemySpritemapDefinition[] family) in BossDisplayDocuments.Families())
        {
            var baseline = new BossDisplayFixture(definition, stock.Build());
            var replacement = new BossDisplayFixture(definition, edits.Build());
            foreach (EnemySpritemapDefinition frame in family)
            {
                frames++;
                baseline.SetFrame(frame.Pointer, fresh: true); replacement.SetFrame(frame.Pointer, fresh: true);
                baseline.ClearBg2(); replacement.ClearBg2();
                AssertBodyOam(baseline.Draw(), stock.Oam.Frames[frame.Name], baseline.Actor);
                string selected = (edits.Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires edits.Oam.DisplayFrames."))[frame.Name];
                AssertBodyOam(replacement.Draw(), edits.Oam.Frames[selected], replacement.Actor);
                AssertBossBg2(baseline.Vram, stock.Writes(frame.Bank, frame.Pointer));
                AssertBossBg2(replacement.Vram, edits.Writes(frame.Bank,
                    edits.Selected(frame).Pointer));
                AssertBossDisplayMechanics(baseline, replacement, frame.Name);
                AssertEqual(frame.Pointer, replacement.Actor.SpritemapPointer, "replacement never changes physical identity");
                // Compare the actual compiled collision walker, not radii or endpoint-only state.
                for (ushort x = 32; x < 240; x += 8)
                for (ushort y = 32; y < 240; y += 8)
                foreach (bool shot in new[] { false, true })
                {
                    var probe = new Position16<RoomPixels>(x, y);
                    var physical = baseline.Hitbox(probe, shot);
                    AssertEqual(physical, replacement.Hitbox(probe, shot), frame.Name + " preserves first overlapping callback");
                    hitSamples++; if (physical.Hit) hits++;
                }
                replacement.SetFrame(frame.Pointer, fresh: false);
                replacement.Vram.ExecuteWordTransfer(new ushort[] { 0xbeef }, EnemyBg2FrameLayoutConstants.VramBase, 1);
                byte[] retained = replacement.Vram.Bytes.ToArray();
                AssertBodyOam(replacement.Draw(), edits.Oam.Frames[selected], replacement.Actor);
                AssertTrue(retained.SequenceEqual(replacement.Vram.Bytes), frame.Name + " retains native new-frame gate");
                if (edits.Writes(frame.Bank, edits.Selected(frame).Pointer).Length == 0)
                {
                    replacement.SetFrame(frame.Pointer, fresh: true); replacement.Draw();
                    AssertTrue(retained.SequenceEqual(replacement.Vram.Bytes),
                        "an OAM-only selected pose does not fabricate a BG2 clear");
                }
            }
        }
        AssertTrue(hits > 0, "collision samples include actual touch and shot hitboxes, not only empty space");
        Suite(nameof(VerifyBossDisplayResources), () => VerifyBossDisplayResources(stock, edits));
        Suite(nameof(VerifyBossDisplayClipping), () => VerifyBossDisplayClipping());
        Suite(nameof(VerifyBossDisplayClocks), () => VerifyBossDisplayClocks(stock, edits));
        Console.WriteLine($"PASS boss display: {frames} exact packed OAM/BG2 replacements, empty BG2-only stock, " +
            $"mixed remaps, native new-frame gating and {hitSamples} compiled collision samples ({hits} hits). RAM-only production.");
    }

    /// <summary>Compares every expected BG2 tilemap word after ordered boss-frame writes.</summary>
    internal static void AssertBossBg2(SnesVram actual, EnemyBg2WriteDocument[] writes)
    {
        var expected = new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight];
        foreach (EnemyBg2WriteDocument write in writes)
            for (int index = 0; index < write.Tiles.Length; index++)
                expected[write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X + index] = checked((ushort)write.Tiles[index]);
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual.ReadWord(EnemyBg2FrameLayoutConstants.VramBase + index), "boss BG2 exact ordered destinations and payload");
    }

    /// <summary>Requires edited presentation to preserve actor animation, WRAM, SRAM, and audio requests.</summary>
    internal static void AssertBossDisplayMechanics(BossDisplayFixture baseline, BossDisplayFixture edited, string context)
    {
        AssertAnimationValues(baseline.Actor, edited.Actor, context);
        AssertAnimationValues(baseline.Enemies, edited.Enemies, context);
        AssertSameBytes(baseline.Memory.WorkRam, edited.Memory.WorkRam, context + " preserves all WRAM");
        AssertTrue(baseline.Enemies.Audio.SoundRequests.SequenceEqual(edited.Enemies.Audio.SoundRequests), context + " preserves audio callbacks");
        AssertTrue(baseline.Enemies.Audio.MusicRequests.SequenceEqual(edited.Enemies.Audio.MusicRequests), context + " preserves music callbacks");
    }

    /// <summary>Runs the boss display and physical-state comparisons.</summary>
    [VerificationCommand("--boss-display-bindings")]
    internal static int BossDisplayBindingsCommand()
    {
        Suite(nameof(VerifyBossDisplayBindings), () => VerifyBossDisplayBindings());
        return 0;
    }
}
