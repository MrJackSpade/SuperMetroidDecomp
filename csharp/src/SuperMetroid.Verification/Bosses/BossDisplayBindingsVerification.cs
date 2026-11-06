using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsDocumentsVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsFixtureVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsResourcesVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBodyPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks boss OAM and BG2 poses drawn from loaded display documents and the native new-frame gate.</summary>
internal static class BossDisplayBindingsVerification
{
    /// <summary>Draws every boss body pose from loaded documents and checks its exact OAM, BG2 writes and new-frame gate.</summary>
    internal static void VerifyBossDisplayBindings()
    {
        var stock = new BossDisplayDocuments();
        int frames = 0;
        foreach ((EnemyDefinitionId definition, EnemySpritemapDefinition[] family) in BossDisplayDocuments.Families())
        {
            var baseline = new BossDisplayFixture(definition, stock.Build());
            foreach (EnemySpritemapDefinition frame in family)
            {
                frames++;
                baseline.SetFrame(frame.Pointer, fresh: true);
                baseline.ClearBg2();
                AssertBodyOam(baseline.Draw(), stock.Oam.Frames[frame.Name], baseline.Actor, baseline.Enemies);
                AssertBossBg2(baseline, stock.Writes(frame.Bank, frame.Pointer));
                baseline.SetFrame(frame.Pointer, fresh: false);
                baseline.Vram.ExecuteWordTransfer(new ushort[] { 0xbeef }, EnemyBg2FrameLayoutConstants.VramBase, 1);
                byte[] retained = baseline.Vram.Bytes.ToArray();
                AssertBodyOam(baseline.Draw(), stock.Oam.Frames[frame.Name], baseline.Actor, baseline.Enemies);
                AssertTrue(retained.SequenceEqual(baseline.Vram.Bytes), frame.Name + " retains native new-frame gate");
            }
        }
        Suite(nameof(VerifyBossDisplayResources), () => VerifyBossDisplayResources(stock));
        Suite(nameof(VerifyBossDisplayClocks), () => VerifyBossDisplayClocks(stock));
        Console.WriteLine($"PASS boss display: {frames} exact packed OAM/BG2 poses, empty BG2-only frames " +
            "and native new-frame gating. RAM-only production.");
    }

    /// <summary>
    /// Compares every expected BG2 tilemap word, each authored cell's tile encoded through the codec, with the word the
    /// boss's body layer shows after its ordered frames are drawn (#1466).
    /// </summary>
    internal static void AssertBossBg2(BossDisplayFixture fixture, EnemyBg2WriteDocument[] writes)
    {
        // The boss shows each frame whole on the body it owns (#1446, #1515): every word a frame writes shows, and a frame
        // the cartridge writes over another shows that frame elsewhere, so only the written cells are compared.
        ushort[] actual = fixture.Bg2Words;
        foreach (EnemyBg2WriteDocument write in writes)
            for (int index = 0; index < write.Tiles.Length; index++)
                AssertEqual(write.Tiles[index].PackedWord, actual[write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X + index],
                    "boss BG2 exact ordered destinations and payload");
    }

    /// <summary>Runs the boss display checks.</summary>
    [VerificationCommand("--boss-display-bindings")]
    internal static int BossDisplayBindingsCommand()
    {
        Suite(nameof(VerifyBossDisplayBindings), () => VerifyBossDisplayBindings());
        return 0;
    }
}
