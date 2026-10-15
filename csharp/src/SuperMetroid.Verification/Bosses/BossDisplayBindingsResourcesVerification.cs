using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsDocumentsVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsFixtureVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsVerification;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Verification.Bosses;

/// <summary>Checks boss BG2 resource requirements, display bindings, and legacy document inheritance.</summary>
internal static class BossDisplayBindingsResourcesVerification
{
    /// <summary>Rejects missing resources and cross-boss bindings before VRAM writes, then checks schema-26 merge behavior.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyBossDisplayResources(BossDisplayDocuments stock, BossDisplayDocuments edits)
    {
        foreach ((EnemyDefinitionId definition, EnemyExtendedFrameDefinition[] family) in BossDisplayDocuments.Families())
        {
            EnemyExtendedFrameDefinition frame = family.First(frame => edits.Writes(frame.Bank, edits.Selected(frame).Pointer).Length != 0);
            var missing = new BossDisplayFixture(definition, edits.Build(omitBg2Bank: frame.Bank));
            missing.SetFrame(frame.Pointer, fresh: true);
            byte[] unchanged = missing.Vram.Bytes.ToArray();
            AssertThrows<InvalidDataException>(() => missing.Draw(), "selected BG2 resource is mandatory");
            AssertTrue(unchanged.SequenceEqual(missing.Vram.Bytes), "missing selected BG2 cannot mutate VRAM");
            var noBindings = new BossDisplayFixture(definition, edits.Build(extended: false));
            noBindings.SetFrame(frame.Pointer, fresh: true);
            AssertThrows<InvalidDataException>(() => noBindings.Draw(), "BG2 cannot bypass mandatory shared composition bindings");
            AssertTrue(unchanged.SequenceEqual(noBindings.Vram.Bytes), "missing composition cannot write BG2 then fail");
        }
        var json = new EnemyIdentityFixture();
        var bad = new Dictionary<string, EnemyExtendedVisualComponent[]>(stock.Oam.Frames, StringComparer.Ordinal);
        string ordinary = EnemyExtendedFrameDefinitions.Frames.First(frame => !EnemyExtendedFrameDefinitions.IsBg2Only(frame)).Name;
        bad[ordinary] = [];
        AssertThrows<InvalidDataException>(() => EnemyExtendedFrameCatalog.Load(json.Json(stock.Oam with { Frames = bad })),
            "empty components are permitted only on declared BG2-only physical roots");
        var cross = new Dictionary<string, string>((edits.Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires edits.Oam.DisplayFrames.")), StringComparer.Ordinal);
        string phantoon = cross.Keys.First(name => name.StartsWith("phantoon_bg2_", StringComparison.Ordinal));
        string draygon = cross.Keys.First(name => name.StartsWith("draygon_bg2_", StringComparison.Ordinal));
        cross[phantoon] = draygon;
        AssertThrows<InvalidDataException>(() => EnemyExtendedFrameCatalog.Load(json.Json(edits.Oam with { DisplayFrames = cross })),
            "BG2 bindings cannot select another boss's visuals");
        // Version 26 retains every prior user edit/binding and inherits the 56 new
        // stock roots. It must not discard or silently synthesize the new bindings.
        EnemyExtendedFrameDefinition[] old = EnemyExtendedFrameDefinitions.Frames.ToArray()[..EnemyExtendedFrameDefinitions.PreBg2BossBindingsFrameCount];
        var legacy = edits.Oam with
        {
            Version = (int)EnemyExtendedFrameSchema.PreBg2BossBindings,
            Frames = old.ToDictionary(frame => frame.Name, frame => edits.Oam.Frames[frame.Name]),
            DisplayFrames = old.ToDictionary(frame => frame.Name, frame => edits.Oam.DisplayFrames[frame.Name]),
        };
        // Older Draygon bindings cannot point at the new BG2 keys. Use an authored
        // old OAM target just as a real schema-26 file does.
        string oldDraygon = old.First(frame => frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal)).Name;
        foreach (string name in legacy.DisplayFrames.Keys.ToArray())
            if (!legacy.Frames.ContainsKey(legacy.DisplayFrames[name])) legacy.DisplayFrames[name] = oldDraygon;
        EnemyExtendedFrameCatalog baseline = (stock.Build().ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.Build().ExtendedFrames."));
        EnemyExtendedFrameCatalog merged = EnemyExtendedFrameCatalog.Load(json.Json(legacy), baseline);
        AssertEqual(merged.ContentIdentity, EnemyExtendedFrameCatalog.Load(json.Json(legacy), baseline).ContentIdentity,
            "schema-26 merge persists identically across reload");
        // Later schema versions also add ordinary OAM roots; only BG2-only roots must stay empty.
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames[EnemyExtendedFrameDefinitions.PreBg2BossBindingsFrameCount..])
        {
            AssertTrue(merged.TryGetDisplay(frame.Bank, frame.Pointer, out var inherited), "legacy file inherits each newly declared root");
            if (EnemyExtendedFrameDefinitions.IsBg2Only(frame))
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
            AssertEqual(steps[index].Frame.Spritemap, changed.Actor.SpritemapPointer, "exact physical sequence ignores visual remapping");
            AssertEqual((ushort)(duration - frame % duration), changed.Actor.InstructionTimer.RawValue, "exact native hold/goto clock");
            bool fresh = frame % duration == 0;
            AssertEqual(fresh, changed.Actor.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame), "exact new-frame schedule");
            if (fresh) transitions++;
        }
        Console.WriteLine($"PASS boss clocks: {frames} production animation steps/{transitions} exact native frame admissions; actor state, RAM and sound/music requests unchanged.");
    }

    /// <summary>Phantoon's tentacles in their own slot beside the body, starting their program on a one-update timer.</summary>
    private static BossDisplayFixture NewTentacleFixture(EnemyTileArtworkCatalog artwork)
    {
        var fixture = new BossDisplayFixture(EnemyDefinitionId.PhantoonTentacles, artwork, slot: 2);
        RoomEnemySlot body = fixture.Enemies.SlotTable.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.PhantoonBody;
        var state = new PhantoonEnemyState(body)
        {
            Eye = fixture.Enemies.SlotTable.Slots[1],
            Tentacles = fixture.Actor,
            Mouth = fixture.Enemies.SlotTable.Slots[3],
        };
        fixture.Enemies.PhantoonBehavior.Phantoon = state;
        fixture.SetFrame(0, false);
        state.PartProgram(fixture.Actor) = AnimationProgramCursor<PhantoonPartProgram>.Start(PhantoonPartProgram.Tentacles);
        fixture.Actor.InstructionTimer = NativeCountdown.Start(1);
        return fixture;
    }
}
