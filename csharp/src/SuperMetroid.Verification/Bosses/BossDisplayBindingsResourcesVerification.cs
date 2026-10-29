using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsDocumentsVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsFixtureVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsVerification;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Verification.Bosses;

/// <summary>Checks boss BG2 resource requirements, display bindings, and legacy document inheritance.</summary>
internal static class BossDisplayBindingsResourcesVerification
{
    /// <summary>Rejects missing resources and cross-boss bindings before VRAM writes, then checks schema-26 merge behavior.</summary>
    internal static void VerifyBossDisplayResources(BossDisplayDocuments stock, BossDisplayDocuments edits)
    {
        foreach ((EnemyDefinitionId definition, EnemySpritemapDefinition[] family) in BossDisplayDocuments.Families())
        {
            EnemySpritemapDefinition frame = family.First(frame => edits.Writes(frame.Bank, edits.Selected(frame).Pointer).Length != 0);
            // The body's pictures are composed at install (#1466), so missing BG2 frames fail there, before anything draws.
            BossBodyOwner bodyOwner = BossDisplayFixture.BodyOwnerOf(definition)
                ?? throw new InvalidOperationException($"{definition} writes BG2 frames without a body layer.");
            AssertThrows<InvalidDataException>(() => BossBodySheetFixtures.For(edits.Build(omitBg2Bank: frame.Bank).Bosses, bodyOwner),
                "selected BG2 resource is mandatory");
            var noBindings = new BossDisplayFixture(definition, edits.Build(extended: false));
            noBindings.SetFrame(frame.Pointer, fresh: true);
            byte[] unchanged = noBindings.Vram.Bytes.ToArray();
            AssertThrows<InvalidDataException>(() => noBindings.Draw(), "BG2 cannot bypass mandatory shared composition bindings");
            AssertTrue(unchanged.SequenceEqual(noBindings.Vram.Bytes), "missing composition cannot write BG2 then fail");
        }
        var json = new EnemyIdentityFixture();
        var bad = new Dictionary<string, EnemyExtendedVisualComponent[]>(stock.Oam.Frames, StringComparer.Ordinal);
        string ordinary = EnemyExtendedFrameDefinitions.Frames.First(frame => !EnemyExtendedFrameBounds.IsBg2Only(frame)).Name;
        bad[ordinary] = [];
        AssertThrows<InvalidDataException>(() => EnemyExtendedFrameDocuments.Load(json.Json(stock.Oam with { Frames = bad })),
            "empty components are permitted only on declared BG2-only physical roots");
        var cross = new Dictionary<string, string>((edits.Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires edits.Oam.DisplayFrames.")), StringComparer.Ordinal);
        string phantoon = EnemyExtendedFrameDefinitions.Frames.First(frame => PhantoonBg2FrameDefinitions.FrameIds.Contains(frame.Frame)).Name;
        string draygon = EnemyExtendedFrameDefinitions.Frames.First(frame => DraygonBg2FrameDefinitions.FrameIds.Contains(frame.Frame)).Name;
        cross[phantoon] = draygon;
        AssertThrows<InvalidDataException>(() => EnemyExtendedFrameDocuments.Load(json.Json(edits.Oam with { DisplayFrames = cross })),
            "BG2 bindings cannot select another boss's visuals");
        // Version 26 retains every prior user edit/binding and inherits the 56 new
        // stock roots. It must not discard or silently synthesize the new bindings.
        // Schema 26 named frames by where the cartridge stores them (#1465).
        EnemySpritemapDefinition[] old = EnemyExtendedFrameDefinitions.Frames.ToArray()[..EnemyExtendedFrameDefinitions.PreBg2BossBindingsFrameCount];
        Dictionary<string, string> legacyNames = EnemyExtendedFrameDefinitions.Frames
            .ToDictionary(frame => frame.Name, frame => frame.LegacyName, StringComparer.Ordinal);
        var legacy = edits.Oam with
        {
            Version = EnemyExtendedFrameSchema.PreBg2BossBindings.Revision,
            Frames = old.ToDictionary(frame => frame.LegacyName, frame => edits.Oam.Frames[frame.Name]),
            DisplayFrames = old.ToDictionary(frame => frame.LegacyName, frame => legacyNames[edits.Oam.DisplayFrames[frame.Name]]),
        };
        // Older Draygon bindings cannot point at the new BG2 keys. Use an authored
        // old OAM target just as a real schema-26 file does.
        string oldDraygon = old.First(frame => frame.Name.StartsWith("draygon_", StringComparison.Ordinal) &&
            !DraygonBg2FrameDefinitions.FrameIds.Contains(frame.Frame)).LegacyName;
        foreach (string name in legacy.DisplayFrames.Keys.ToArray())
            if (!legacy.Frames.ContainsKey(legacy.DisplayFrames[name])) legacy.DisplayFrames[name] = oldDraygon;
        EnemyExtendedFrameCatalog baseline = (stock.Build().Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.Build().ExtendedFrames."));
        EnemyExtendedFrameCatalog merged = EnemyExtendedFrameDocuments.Load(json.Json(legacy), baseline);
        AssertEqual(merged.ContentIdentity, EnemyExtendedFrameDocuments.Load(json.Json(legacy), baseline).ContentIdentity,
            "schema-26 merge persists identically across reload");
        // Later schema versions also add ordinary OAM roots; only BG2-only roots must stay empty.
        foreach (EnemySpritemapDefinition frame in EnemyExtendedFrameDefinitions.Frames[EnemyExtendedFrameDefinitions.PreBg2BossBindingsFrameCount..])
        {
            AssertTrue(merged.TryGetDisplay(frame.Bank, frame.Pointer, out var inherited), "legacy file inherits each newly declared root");
            if (EnemyExtendedFrameBounds.IsBg2Only(frame))
                AssertEqual(0, inherited.Length, "native BG2-only roots do not fabricate OAM during legacy merge");
            AssertEqual(frame.Pointer, merged.GetDisplayPointer(frame.Bank, frame.Pointer), "new stock binding remains identity");
        }
        Console.WriteLine("PASS boss display resources: missing binding/BG2 fail before VRAM writes, bounded empty-frame rules, cross-family rejection and schema-26 inheritance.");
    }

    /// <summary>Focused native duration/goto acceptance, not an AI playthrough or read search.</summary>
    internal static void VerifyBossDisplayClocks(BossDisplayDocuments stock, BossDisplayDocuments edits)
    {
        // Phantoon's tentacles: four eight-update poses that loop ($A7:CCD7).
        const int duration = 8;
        const int frames = 150;
        var steps = PhantoonAnimations.Programs.Steps(PhantoonPartProgram.Tentacles);
        int poseCount = steps.Count - 1;
        BossDisplayFixture baseline = NewTentacleFixture(stock.Build());
        BossDisplayFixture changed = NewTentacleFixture(edits.Build());
        int transitions = 0;
        for (int frame = 0; frame < frames; frame++)
        {
            baseline.Step(); changed.Step(); baseline.Draw(); changed.Draw();
            AssertBossDisplayMechanics(baseline, changed, "boss instruction frame " + frame);
            int index = frame / duration % poseCount;
            AssertEqual(PhantoonPartFrameSpritemapsConstants.Spritemap(AssertIs<AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>.ShowStep>(steps[index], "pose step").Frame), changed.Actor.SpritemapPointer, "exact physical sequence ignores visual remapping");
            AssertEqual((ushort)(duration - frame % duration), changed.Actor.InstructionTimer.RawValue, "exact native hold/goto clock");
            bool fresh = frame % duration == 0;
            AssertEqual(fresh, changed.Actor.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame), "exact new-frame schedule");
            if (fresh) transitions++;
        }
        Console.WriteLine($"PASS boss clocks: {frames} production animation steps/{transitions} exact native frame admissions; actor state, RAM and sound/music requests unchanged.");
    }

    /// <summary>Phantoon's tentacles in their own slot beside the body, starting their program on a one-update timer.</summary>
    private static BossDisplayFixture NewTentacleFixture(InstalledEnemyArtwork artwork)
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
        state.PartProgram(fixture.Actor) = AnimationProgramCursor<PhantoonPartProgram>.Start(PhantoonPartProgram.Tentacles);
        fixture.Actor.InstructionTimer = WrappingCountdown.Start(1);
        return fixture;
    }
}
