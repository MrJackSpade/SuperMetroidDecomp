using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsDocumentsVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsFixtureVerification;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Verification.Bosses;

/// <summary>Checks boss BG2 resource requirements and display bindings.</summary>
internal static class BossDisplayBindingsResourcesVerification
{
    /// <summary>Rejects missing resources and cross-boss bindings before VRAM writes.</summary>
    internal static void VerifyBossDisplayResources(BossDisplayDocuments stock)
    {
        foreach ((EnemyDefinitionId definition, EnemyFrameName[] family) in BossDisplayDocuments.Families())
        {
            EnemyFrameName frame = family.First(frame => stock.Writes(frame.Bank, frame.Pointer).Length != 0);
            // The body's pictures are composed at install (#1466), so missing BG2 frames fail there, before anything draws.
            BossBodyOwner bodyOwner = BossDisplayFixture.BodyOwnerOf(definition)
                ?? throw new InvalidOperationException($"{definition} writes BG2 frames without a body layer.");
            AssertThrows<InvalidDataException>(() => BossBodySheetFixtures.For(stock.Build(omitBg2Bank: frame.Bank).Bosses, bodyOwner),
                "selected BG2 resource is mandatory");
        }
        var json = new EnemyIdentityFixture();
        var bad = new Dictionary<string, EnemyExtendedVisualComponent[]>(stock.Oam.Frames, StringComparer.Ordinal);
        string ordinary = EnemyExtendedFrameDefinitions.Frames.First(frame => !EnemyExtendedFrameBounds.IsBg2Only(frame)).Name;
        bad[ordinary] = [];
        AssertThrows<InvalidDataException>(() => EnemyExtendedFrameDocuments.Load(json.Json(stock.Oam with { Frames = bad })),
            "empty components are permitted only on declared BG2-only physical roots");
        var cross = new Dictionary<string, string>((stock.Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires stock.Oam.DisplayFrames.")), StringComparer.Ordinal);
        string phantoon = EnemyExtendedFrameDefinitions.Frames.First(frame => PhantoonBodyLayerFrameDefinitions.Frames.Contains(frame.Frame)).Name;
        string draygon = EnemyExtendedFrameDefinitions.Frames.First(frame => DraygonBodyLayerFrameDefinitions.Frames.Contains(frame.Frame)).Name;
        cross[phantoon] = draygon;
        AssertThrows<InvalidDataException>(() => EnemyExtendedFrameDocuments.Load(json.Json(stock.Oam with { DisplayFrames = cross })),
            "BG2 bindings cannot select another boss's visuals");
        Console.WriteLine("PASS boss display resources: missing BG2 fails before anything draws, bounded empty-frame rules and cross-family rejection.");
    }

    /// <summary>Focused native duration/goto acceptance, not an AI playthrough or read search.</summary>
    internal static void VerifyBossDisplayClocks(BossDisplayDocuments stock)
    {
        // Phantoon's tentacles: four eight-update poses that loop ($A7:CCD7).
        const int duration = 8;
        const int frames = 150;
        var beats = PhantoonAnimations.Segments.Beats(PhantoonPartProgram.Tentacles);
        int poseCount = beats.Count;
        BossDisplayFixture tentacles = NewTentacleFixture(stock.Build());
        int transitions = 0;
        for (int frame = 0; frame < frames; frame++)
        {
            tentacles.Step(); tentacles.Draw();
            int index = frame / duration % poseCount;
            AssertEqual(PhantoonPartFrameSpritemapsConstants.Spritemap(AssertIs<AnimationBeat<PhantoonPartFrame, PhantoonPartCue>.Frame>(beats[index], "pose beat").Shown), tentacles.Actor.SpritemapPointer, "exact physical pose sequence");
            AssertEqual((ushort)(duration - frame % duration), tentacles.Actor.AnimationClock.Hold.RawValue, "exact native hold/goto clock");
            bool fresh = frame % duration == 0;
            AssertEqual(fresh, tentacles.Actor.AnimationClock.ShowsNewFrame, "exact new-frame schedule");
            if (fresh) transitions++;
        }
        Console.WriteLine($"PASS boss clocks: {frames} production animation steps/{transitions} exact native frame admissions.");
    }

    /// <summary>Phantoon's tentacles in their own slot beside the body, starting their program on a one-update timer.</summary>
    private static BossDisplayFixture NewTentacleFixture(ImportEnemyArtwork artwork)
    {
        var fixture = new BossDisplayFixture(EnemyDefinitionId.PhantoonTentacles, artwork, slot: 2);
        RoomEnemySlot body = fixture.Enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.PhantoonBody, body.Definition);
        var state = new PhantoonEnemyState(body)
        {
            Eye = fixture.Enemies.SlotTable.Slots[1],
            Tentacles = fixture.Actor,
            Mouth = fixture.Enemies.SlotTable.Slots[3],
        };
        fixture.Enemies.PhantoonBehavior.Combat.Phantoon = state;
        fixture.SetFrame(0, false);
        state.PartPlayback(fixture.Actor) = PhantoonEnemyState.Starting(PhantoonPartProgram.Tentacles);
        fixture.Actor.AnimationClock.StepOnNextUpdate();
        return fixture;
    }
}
