using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Newly authored OAM on BG2-only roots must use the common native wrap/clipping path.</summary>
    private static void VerifyBossDisplayClipping()
    {
        int checks = 0;
        foreach ((ushort definition, EnemyExtendedFrameDefinition[] family) in BossDisplayDocuments.Families())
        {
            EnemyExtendedFrameDefinition? bg2 = family.Cast<EnemyExtendedFrameDefinition?>()
                .FirstOrDefault(frame => frame.HasValue && EnemyExtendedFrameDefinitions.IsBg2Only(frame.Value));
            if (bg2 is not { } frame) continue;
            var author = new BossDisplayDocuments(edited: true);
            string selected = author.Selected(frame).Name;
            var original = author.Oam.Frames[selected][0].Parts[0];
            foreach ((ushort originY, int offset, byte expectedY) in new[]
            {
                ((ushort)252, 8, (byte)240), ((ushort)4, -8, (byte)240),
                (unchecked((ushort)-4), 8, (byte)4), ((ushort)260, -8, (byte)252),
            })
            {
                author.Oam.Frames[selected] = [new() { OffsetX = 0, OffsetY = 0, Parts = [original with { OffsetY = offset }] }];
                var fixture = new BossDisplayFixture(definition, author.Build());
                fixture.SetFrame(frame.Pointer, false); fixture.Actor.YPosition = originY;
                var result = fixture.Draw();
                AssertEqual(4, result.NextByteOffset, "BG2-only replacement emits exactly its authored sprite");
                AssertEqual(expectedY, result.LowTable[1], "new BG2-only OAM obeys complementary native carry/sign wrap tests");
                checks++;
            }
            foreach ((ushort x, ushort y) in new[] { ((ushort)512, (ushort)128), ((ushort)128, (ushort)512) })
            {
                var fixture = new BossDisplayFixture(definition, author.Build());
                fixture.SetFrame(frame.Pointer, false); fixture.Actor.XPosition = x; fixture.Actor.YPosition = y;
                AssertEqual(0, fixture.Draw().NextByteOffset, "off-window extended components are not emitted");
                checks++;
            }
        }
        AssertEqual(12, checks, "both new BG2-only boss families exercise all wrap/component clipping cases");
        Console.WriteLine("PASS BG2-only authored OAM: 12 exact native vertical-wrap and component-window clipping cases.");
    }
}
