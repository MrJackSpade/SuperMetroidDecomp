using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsDocumentsVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World.Artwork;
namespace SuperMetroid.Verification.Bosses;

/// <summary>Checks authored BG2-only boss OAM against the whole-frame coordinate wrap and component clipping.</summary>
internal static class BossDisplayBindingsClippingVerification
{
    /// <summary>Newly authored OAM on BG2-only roots keeps wrapped coordinates and clips whole components off the window.</summary>
    internal static void VerifyBossDisplayClipping()
    {
        int checks = 0;
        foreach ((EnemyDefinitionId definition, EnemySpritemapDefinition[] family) in BossDisplayDocuments.Families())
        {
            EnemySpritemapDefinition? bg2 = family.Cast<EnemySpritemapDefinition?>()
                .FirstOrDefault(frame => frame.HasValue && EnemyExtendedFrameBounds.IsBg2Only(frame.Value));
            if (bg2 is not { } frame) continue;
            var author = new BossDisplayDocuments(edited: true);
            string selected = author.Selected(frame).Name;
            var original = author.Oam.Frames[selected][0].Parts[0];
            foreach ((ushort originY, int offset, byte expectedY) in new[]
            {
                new VerifyBossDisplayClippingCase(252, 8, 4), new VerifyBossDisplayClippingCase(4, -8, 252),
                new VerifyBossDisplayClippingCase(unchecked((ushort)-4), 8, 4), new VerifyBossDisplayClippingCase(260, -8, 252),
            })
            {
                author.Oam.Frames[selected] = [new() { OffsetX = 0, OffsetY = 0, Parts = [original with { OffsetY = offset }] }];
                var fixture = new BossDisplayFixture(definition, author.Build());
                fixture.SetFrame(frame.Pointer, false); fixture.Actor.Position = fixture.Actor.Position.WithWholeY(originY);
                var result = fixture.Draw();
                // The cartridge parks a part that crosses the vertical boundary at Y=$F0; #1308 drops that
                // presentation quirk, so the whole-frame draw keeps the part at its wrapped coordinate.
                AssertEqual(4, result.NextByteOffset, "BG2-only replacement emits exactly its authored sprite");
                AssertEqual(expectedY, result.LowTable[1], "new BG2-only OAM keeps its wrapped coordinate (#1308)");
                checks++;
            }
            foreach ((ushort x, ushort y) in new[] { new BossDisplayBindingsClippingVerifyBossDisplayClippingCase(512, 128), new BossDisplayBindingsClippingVerifyBossDisplayClippingCase(128, 512) })
            {
                var fixture = new BossDisplayFixture(definition, author.Build());
                fixture.SetFrame(frame.Pointer, false); fixture.Actor.Position = fixture.Actor.Position.WithWhole(new(x, y));
                AssertEqual(0, fixture.Draw().NextByteOffset, "off-window extended components are not emitted");
                checks++;
            }
        }
        AssertEqual(12, checks, "both new BG2-only boss families exercise all wrap/component clipping cases");
        Console.WriteLine("PASS BG2-only authored OAM: 12 exact native vertical-wrap and component-window clipping cases.");
    }
}
