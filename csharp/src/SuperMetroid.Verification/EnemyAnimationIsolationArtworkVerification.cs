using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static EnemyAnimationIsolationFixtureVerification;
using static EnemyContentIdentityFixtureVerification;
using static GoldenTorizoRightOrbFramesVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Builds distinct edited enemy visuals and checks their rendered OAM independently of animation state.</summary>
internal static class EnemyAnimationIsolationArtworkVerification
{
    /// <summary>Changes Boyon, Torizo, projectile, death, and pickup artwork while remapping selected display frames.</summary>
    internal static EnemyTileArtworkCatalog CreateEditedEnemyAnimationArtwork()
    {
        var fixture = new EnemyIdentityFixture();
        EnemySpritemapDocument ordinary = fixture.OamDocument();
        var boyon = EnemySpritemapDefinitions.Frames
            .Where(frame => frame.Name.StartsWith("boyon_", StringComparison.Ordinal)).ToArray();
        for (int index = 0; index < boyon.Length; index++)
        {
            // Different silhouettes, offsets, tile selections and sequence bindings.
            // If a binding is ignored, the selected tile is observably wrong even though
            // both alternative frames are valid and have the same number of parts.
            ordinary.Frames[boyon[index].Name] = EditedAnimationParts(index);
            (ordinary.DisplayFrames ?? throw new InvalidOperationException("Verification requires ordinary.DisplayFrames."))[boyon[index].Name] = boyon[(index + 1) % boyon.Length].Name;
        }

        EnemyExtendedFrameDocument extended = fixture.ExtendedDocument();
        var torizo = EnemyExtendedFrameDefinitions.Frames
            .Where(frame => frame.Bank == TorizoInstructionProgramDefinitions.Bank).ToArray();
        for (int index = 0; index < torizo.Length; index++)
            extended.Frames[torizo[index].Name] =
            [
                new EnemyExtendedVisualComponent { OffsetX = 12, OffsetY = -8, Parts = EditedAnimationParts(index % 16) },
                new EnemyExtendedVisualComponent { OffsetX = -16, OffsetY = 6, Parts = EditedAnimationParts((index + 1) % 16) },
            ];
        var orb = torizo.Where(frame => GoldenTorizoRightOrbFrames().Contains(frame.Pointer)).ToArray();
        AssertEqual(GoldenTorizoRightOrbFrames().Length, orb.Length, "all right-orb display frames are authored");
        for (int index = 0; index < orb.Length; index++)
            (extended.DisplayFrames ?? throw new InvalidOperationException("Verification requires extended.DisplayFrames."))[orb[index].Name] = orb[(index + 1) % orb.Length].Name;

        // Change every projectile/death/pickup visual too: its instruction clocks,
        // positions, damage and random drop decisions must remain unchanged.
        EnemyProjectileSpritemapDocument projectile = fixture.ProjectileDocument();
        foreach (string name in projectile.Frames.Keys.ToArray())
            projectile.Frames[name] = EditedAnimationParts(5);
        foreach (string name in (projectile.ProgramFrames ?? throw new InvalidOperationException("Verification requires projectile.ProgramFrames.")).Keys.ToArray())
            projectile.ProgramFrames[name] = EditedAnimationParts(7);
        return fixture.Build(EnemySpritemapCatalog.Load(fixture.Json(ordinary)),
            EnemyExtendedFrameCatalog.Load(fixture.Json(extended)),
            EnemyProjectileSpritemapCatalog.Load(fixture.Json(projectile)));
    }

    /// <summary>Creates two asymmetric OAM parts so ignored offsets, tile selection, and attributes are visible.</summary>
    private static SpriteVisualPart[] EditedAnimationParts(int tile) =>
    [
        new SpriteVisualPart
        {
            OffsetX = 21, OffsetY = -6, Size = 16, TileColumn = tile, TileRow = 2,
            Palette = 4, Priority = 2, FlipX = true, FlipY = true,
        },
        new SpriteVisualPart
        {
            OffsetX = -13, OffsetY = 9, Size = 8, TileColumn = tile, TileRow = 3,
            Palette = 5, Priority = 3, FlipX = false, FlipY = false,
        },
    ];

    /// <summary>Compares the edited Boyon or Torizo body prefix with independently assembled packed OAM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void AssertEditedAnimationDraw(EnemyAnimationFixture edited, OamBuffer drawn, int frame)
    {
        RoomEnemySlot actor = edited.Actor;
        if (actor.EnemyDefinitionPointer == 0 || actor.Properties.HasAny(EnemyProperties.Invisible | EnemyProperties.Deleted))
            return;
        var expected = new OamBuffer();
        expected.BeginFrame();
        if (edited.Torizo is null)
        {
            var definitions = EnemySpritemapDefinitions.Frames
                .Where(value => value.Name.StartsWith("boyon_", StringComparison.Ordinal)).ToArray();
            int nativeIndex = Array.FindIndex(definitions, value => value.Pointer == actor.SpritemapPointer);
            AssertTrue(nativeIndex >= 0, "draw assertion selects a declared Boyon frame");
            int selectedIndex = (nativeIndex + 1) % definitions.Length;
            expected.AddEnemySpritemap(EnemySpritemapCatalog.CompileParts(EditedAnimationParts(selectedIndex),
                definitions[selectedIndex].Name), new Position16<OamPixels>(actor.Position.X.Whole, actor.Position.Y.Whole), actor.PaletteIndex,
                actor.VramTilesIndex);
        }
        else
        {
            var definitions = EnemyExtendedFrameDefinitions.Frames
                .Where(value => value.Bank == TorizoInstructionProgramDefinitions.Bank).ToArray();
            var orb = definitions.Where(value => GoldenTorizoRightOrbFrames().Contains(value.Pointer)).ToArray();
            int orbIndex = Array.FindIndex(orb, value => value.Pointer == actor.SpritemapPointer);
            ushort selected = orbIndex >= 0 ? orb[(orbIndex + 1) % orb.Length].Pointer : actor.SpritemapPointer;
            int selectedIndex = Array.FindIndex(definitions, value => value.Pointer == selected);
            AssertTrue(selectedIndex >= 0, "draw assertion selects a declared Torizo frame");
            Add(12, -8, selectedIndex % 16);
            Add(-16, 6, (selectedIndex + 1) % 16);

            void Add(int x, int y, int tile) => expected.AddEnemySpritemap(
                EnemySpritemapCatalog.CompileParts(EditedAnimationParts(tile), definitions[selectedIndex].Name),
                new Position16<OamPixels>(unchecked((ushort)(actor.Position.X.Whole + x)), unchecked((ushort)(actor.Position.Y.Whole + y))),
                actor.PaletteIndex, actor.VramTilesIndex, clipVerticalWrap: true);
        }
        // Body is queued before projectiles; compare its exact packed OAM prefix.
        // This independent authored expectation fails if drawing uses the native
        // composition or ignores the requested display-sequence remap.
        AssertTrue(expected.NextByteOffset > 0 && drawn.NextByteOffset >= expected.NextByteOffset,
            $"replacement body is actually drawn at frame {frame}");
        AssertTrue(expected.LowTable[..expected.NextByteOffset].SequenceEqual(drawn.LowTable[..expected.NextByteOffset]),
            $"exact authored frame, component order, offsets, tile, flips and palette render at frame {frame}");
        int highBytes = (expected.NextByteOffset / 4 + 3) / 4;
        AssertTrue(expected.HighTable[..highBytes].SequenceEqual(drawn.HighTable[..highBytes]),
            $"exact authored size/X-high bits render at frame {frame}");
    }
}
