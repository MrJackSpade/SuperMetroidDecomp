using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Acceptance for the source-identified mismatch between selected OAM and physical BG2.</summary>
    private static void VerifyBossDisplayBindings()
    {
        var stock = new BossDisplayDocuments(edited: false);
        var edits = new BossDisplayDocuments(edited: true);
        int frames = 0, hitSamples = 0, hits = 0;
        foreach ((EnemyDefinitionId definition, EnemyExtendedFrameDefinition[] family) in BossDisplayDocuments.Families())
        {
            var baseline = new BossDisplayFixture(definition, stock.Build());
            var replacement = new BossDisplayFixture(definition, edits.Build());
            foreach (EnemyExtendedFrameDefinition frame in family)
            {
                frames++;
                baseline.SetFrame(frame.Pointer, fresh: true); replacement.SetFrame(frame.Pointer, fresh: true);
                baseline.ClearBg2(); replacement.ClearBg2();
                AssertBodyOam(baseline.Draw(), stock.Oam.Frames[frame.Name], baseline.Actor);
                string selected = edits.Oam.DisplayFrames![frame.Name];
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
                    var physical = baseline.Hitbox(x, y, shot);
                    AssertEqual(physical, replacement.Hitbox(x, y, shot), frame.Name + " preserves first overlapping callback");
                    hitSamples++; if (physical.Hit) hits++;
                }
                replacement.SetFrame(frame.Pointer, fresh: false);
                replacement.Vram.ExecuteWordTransfer(new ushort[] { 0xbeef }, EnemyBg2FrameLayout.VramBase, 1);
                byte[] retained = replacement.Vram.Bytes.ToArray();
                AssertBodyOam(replacement.Draw(), edits.Oam.Frames[selected], replacement.Actor);
                AssertTrue(retained.AsSpan().SequenceEqual(replacement.Vram.Bytes), frame.Name + " retains native new-frame gate");
                if (edits.Writes(frame.Bank, edits.Selected(frame).Pointer).Length == 0)
                {
                    replacement.SetFrame(frame.Pointer, fresh: true); replacement.Draw();
                    AssertTrue(retained.AsSpan().SequenceEqual(replacement.Vram.Bytes),
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

    private static void AssertBossBg2(SnesVram actual, EnemyBg2WriteDocument[] writes)
    {
        var expected = new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight];
        foreach (EnemyBg2WriteDocument write in writes)
            for (int index = 0; index < write.Tiles.Length; index++)
                expected[write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X + index] = checked((ushort)write.Tiles[index]);
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual.ReadWord(EnemyBg2FrameLayout.VramBase + index), "boss BG2 exact ordered destinations and payload");
    }

    private static void AssertBossDisplayMechanics(BossDisplayFixture baseline, BossDisplayFixture edited, string context)
    {
        AssertAnimationValues(baseline.Actor, edited.Actor, context);
        AssertAnimationValues(baseline.Enemies, edited.Enemies, context);
        AssertSameBytes(baseline.Memory.WorkRam, edited.Memory.WorkRam, context + " preserves all WRAM");
        AssertSameBytes(baseline.Memory.SaveRam, edited.Memory.SaveRam, context + " preserves all SRAM");
        AssertTrue(baseline.Enemies.SoundRequests.SequenceEqual(edited.Enemies.SoundRequests), context + " preserves audio callbacks");
        AssertTrue(baseline.Enemies.MusicRequests.SequenceEqual(edited.Enemies.MusicRequests), context + " preserves music callbacks");
    }
}
