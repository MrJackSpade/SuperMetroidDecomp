using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Hardware;
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
    internal static void VerifyBossDisplayBindings(BossDisplayDocuments stock)
    {
        int frames = 0;
        foreach ((EnemyDefinitionId definition, EnemyFrameName[] family) in BossDisplayDocuments.Families())
        {
            // Each frame is drawn by the part of the boss that shows it, whose sheet holds its picture.
            var parts = new Dictionary<EnemyDefinitionId, BossDisplayFixture>();
            // One artwork per boss: its parts' fixtures share the body sheets composed from it.
            ImportEnemyArtwork artwork = stock.Build();
            foreach (EnemyFrameName frame in family)
            {
                frames++;
                EnemyDefinitionId part = BossDisplayDocuments.DrawnBy(definition, frame.Frame);
                if (!parts.TryGetValue(part, out BossDisplayFixture? baseline))
                    parts.Add(part, baseline = new BossDisplayFixture(part, artwork));
                baseline.SetFrame(frame.Pointer, fresh: true);
                baseline.ClearBg2();
                AssertBodyOam(baseline.DrawInto, stock.Oam.Frames[frame.Name], baseline.Actor, baseline.Enemies, () => baseline.ActorFrameShowsNothing, $"{definition} {frame.Name}");
                AssertBossBg2(baseline, stock.Writes(frame.Bank, frame.Pointer),
                    PicturedWith(frame) is { } filling ? stock.Writes(filling.Bank, filling.Pointer) : [], $"{definition} {frame.Name}");
                baseline.SetFrame(frame.Pointer, fresh: false);
                baseline.Vram.ExecuteWordTransfer(new ushort[] { 0xbeef }, EnemyBg2FrameLayoutConstants.VramBase, 1);
                byte[] retained = baseline.Vram.Bytes.ToArray();
                AssertBodyOam(baseline.DrawInto, stock.Oam.Frames[frame.Name], baseline.Actor, baseline.Enemies, () => baseline.ActorFrameShowsNothing, $"{definition} {frame.Name}");
                AssertTrue(retained.SequenceEqual(baseline.Vram.Bytes), frame.Name + " retains native new-frame gate");
            }
        }
        Console.WriteLine($"PASS boss display: {frames} exact packed OAM/BG2 poses, empty BG2-only frames " +
            "and native new-frame gating. RAM-only production.");
    }

    /// <summary>
    /// Compares every expected BG2 tilemap word, each authored cell's tile encoded through the codec, with the word the
    /// boss's body layer shows after its ordered frames are drawn (#1466).
    /// </summary>
    /// <param name="fixture">The boss whose body layer shows the frame.</param>
    /// <param name="writes">The frame's writes, in order.</param>
    /// <param name="fillingWrites">
    /// The writes of the part the frame is pictured with (#1513), laid over the frame's own where it leaves a gap; none for
    /// a frame pictured alone.
    /// </param>
    /// <param name="context">Names the frame in a failure message.</param>
    internal static void AssertBossBg2(BossDisplayFixture fixture, EnemyBg2WriteDocument[] writes, EnemyBg2WriteDocument[] fillingWrites, string context)
    {
        // The boss shows each frame whole on the body it owns (#1446, #1515): every word a frame writes shows, and a frame
        // the cartridge writes over another shows that frame elsewhere, so only the written cells are compared.
        // A frame's runs are written in order, so a cell two runs write shows the later run's word.
        ushort[] actual = fixture.Bg2Words;
        var shown = new SortedDictionary<int, ushort>();
        foreach (EnemyBg2WriteDocument write in writes.Concat(fillingWrites))
            for (int index = 0; index < write.Tiles.Length; index++)
                shown[write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X + index] = write.Tiles[index].PackedWord;
        foreach ((int cell, ushort word) in shown)
            AssertEqual(word, actual[cell],
                $"boss BG2 exact ordered destinations and payload: {context}, cell ({cell % EnemyBg2FrameLayout.TilemapWidth}, {cell / EnemyBg2FrameLayout.TilemapWidth}), body shows {fixture.ShownFrames}");
    }

    /// <summary>
    /// The frame <paramref name="frame"/> is pictured with, whose writes fill the gap it leaves for another part (#1513), or
    /// null for a frame pictured alone.
    /// </summary>
    internal static EnemyFrameName? PicturedWith(EnemyFrameName frame) =>
        BossBodySourceDefinitions.FramesPicturedWith.TryGetValue(frame.Frame, out EnemyFrameId filling)
            ? EnemyExtendedFrameDefinitions.Frames.Single(candidate => candidate.Frame == filling)
            : null;

    /// <summary>The boss display documents the boss display suites share, loaded once.</summary>
    private static readonly Lazy<BossDisplayDocuments> stockDocuments = new(() => new BossDisplayDocuments());

    /// <summary>The boss display checks' parts, each a suite of its own over the shared documents, which they only read.</summary>
    internal static RegisteredSuite[] BossDisplaySuites() =>
    [
        new(nameof(VerifyBossDisplayBindings), () => VerifyBossDisplayBindings(stockDocuments.Value)),
        new(nameof(VerifyBossDisplayResources), () => VerifyBossDisplayResources(stockDocuments.Value)),
        new(nameof(VerifyBossDisplayClocks), () => VerifyBossDisplayClocks(stockDocuments.Value)),
    ];

    /// <summary>Runs every part of the boss display checks (<see cref="BossDisplaySuites"/>).</summary>
    [VerificationCommand("--boss-display-bindings")]
    internal static int BossDisplayBindingsCommand()
    {
        foreach (RegisteredSuite suite in BossDisplaySuites())
            Suite(suite.Name, suite.Body);
        return 0;
    }
}
