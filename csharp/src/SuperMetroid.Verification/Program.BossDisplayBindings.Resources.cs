using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyBossDisplayResources(BossDisplayDocuments stock, BossDisplayDocuments edits)
    {
        foreach ((ushort definition, EnemyExtendedFrameDefinition[] family) in BossDisplayDocuments.Families())
        {
            EnemyExtendedFrameDefinition frame = family.First(frame => edits.Writes(frame.Bank, edits.Selected(frame).Pointer).Length != 0);
            var missing = new BossDisplayFixture(definition, edits.Build(omitBg2Bank: frame.Bank));
            missing.SetFrame(frame.Pointer, fresh: true);
            byte[] unchanged = missing.Vram.Bytes.ToArray();
            AssertThrows<InvalidDataException>(() => missing.Draw(), "selected BG2 resource is mandatory");
            AssertTrue(unchanged.AsSpan().SequenceEqual(missing.Vram.Bytes), "missing selected BG2 cannot mutate VRAM");
            var noBindings = new BossDisplayFixture(definition, edits.Build(extended: false));
            noBindings.SetFrame(frame.Pointer, fresh: true);
            AssertThrows<InvalidDataException>(() => noBindings.Draw(), "BG2 cannot bypass mandatory shared composition bindings");
            AssertTrue(unchanged.AsSpan().SequenceEqual(noBindings.Vram.Bytes), "missing composition cannot write BG2 then fail");
        }
        var json = new EnemyIdentityFixture();
        var bad = new Dictionary<string, EnemyExtendedVisualComponent[]>(stock.Oam.Frames, StringComparer.Ordinal);
        string ordinary = EnemyExtendedFrameDefinitions.Frames.ToArray().First(frame => !EnemyExtendedFrameDefinitions.IsBg2Only(frame)).Name;
        bad[ordinary] = [];
        AssertThrows<InvalidDataException>(() => EnemyExtendedFrameCatalog.Load(json.Json(stock.Oam with { Frames = bad })),
            "empty components are permitted only on declared BG2-only physical roots");
        var cross = new Dictionary<string, string>(edits.Oam.DisplayFrames!, StringComparer.Ordinal);
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
            Version = EnemyExtendedFrameDefinitions.PreBg2BossBindingsVersion,
            Frames = old.ToDictionary(frame => frame.Name, frame => edits.Oam.Frames[frame.Name]),
            DisplayFrames = old.ToDictionary(frame => frame.Name, frame => edits.Oam.DisplayFrames![frame.Name]),
        };
        // Older Draygon bindings cannot point at the new BG2 keys. Use an authored
        // old OAM target just as a real schema-26 file does.
        string oldDraygon = old.First(frame => frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal)).Name;
        foreach (string name in legacy.DisplayFrames!.Keys.ToArray())
            if (!legacy.Frames.ContainsKey(legacy.DisplayFrames[name])) legacy.DisplayFrames[name] = oldDraygon;
        EnemyExtendedFrameCatalog baseline = stock.Build().ExtendedFrames!;
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
    private static void VerifyBossDisplayClocks(BossDisplayDocuments stock, BossDisplayDocuments edits)
    {
        var programs = new[]
        {
            (Definition: RoomEnemySystem.CrocomireDefinition, Program: CrocomireInstructionProgramDefinitions.StepForwardAfterDelay, Duration: 180, Count: 1),
            (Definition: RoomEnemySystem.PhantoonTentaclesDefinition, Program: PhantoonInstructionProgramDefinitions.InitialTentacles, Duration: 8, Count: 4),
            (Definition: DraygonEnemyDefinitionPointers.Arms, Program: DraygonInstructionProgramDefinitions.ArmsFacingLeftIdle, Duration: 5, Count: 6),
        };
        int transitions = 0;
        foreach (var program in programs)
        {
            var baseline = new BossDisplayFixture(program.Definition, stock.Build());
            var changed = new BossDisplayFixture(program.Definition, edits.Build());
            baseline.SetFrame(0, false); changed.SetFrame(0, false);
            baseline.SetProgram(program.Program); changed.SetProgram(program.Program);
            for (int frame = 0; frame < 150; frame++)
            {
                baseline.Step(frame); changed.Step(frame); baseline.Draw(); changed.Draw();
                AssertBossDisplayMechanics(baseline, changed, "boss instruction frame " + frame);
                int index = frame / program.Duration % program.Count;
                AssertTrue(CompiledEnemyVisualSelectors.TryGet(baseline.Actor.Definition.Bank,
                    checked((ushort)(program.Program + index * 4 + 2)), out ushort expected), "declared program selector is compiled");
                AssertEqual(expected, changed.Actor.SpritemapPointer, "exact physical sequence ignores visual remapping");
                AssertEqual((ushort)(program.Duration - frame % program.Duration), changed.Actor.InstructionTimer, "exact native hold/goto clock");
                bool fresh = frame % program.Duration == 0;
                AssertEqual(fresh, changed.Actor.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame), "exact new-frame schedule");
                if (fresh) transitions++;
            }
        }
        Console.WriteLine($"PASS boss clocks: 450 production instruction steps/{transitions} exact native frame admissions; actor state, RAM and sound/music requests unchanged.");
    }
}
