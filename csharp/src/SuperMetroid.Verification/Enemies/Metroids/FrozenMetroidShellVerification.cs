using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>Checks that freezing during an empty Metroid shell frame immediately selects visible frozen artwork.</summary>
internal static class FrozenMetroidShellVerification
{
    /// <summary>Reproduces the empty shell frame and compares all emitted frozen sprite parts while the animation timer remains held (#1207).</summary>
    internal static int VerifyFrozenMetroidShell()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom()) { BossArtwork = InstalledFixtures.BossArtwork };
        // The shell draws as a Metroid room loads it: at the Metroid's tile base and in its palette.
        enemies.DrawUnder(SpawnedObjectFixtures.SetLoading(EnemyDefinitionId.Metroid));
        int tileBase = enemies.GraphicsSet.TileBaseOf(EnemyDefinitionId.Metroid);
        var slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(120, 100));
        slot.Bind(EnemyDefinitionId.Metroid, slot.Definition);
        slot.Shading = PictureShading.Standard;
        slot.SetSpawnWords(EnemyDefinitionId.Metroid, words => words);
        enemies.MetroidBehavior.InitializeMetroid(slot);
        var state = enemies.MetroidStates[0];
        // The shell is read by its own draws, apart from the electrical overlay.
        var shell = (state ?? throw new InvalidOperationException("Verification requires state.")).OuterBodyB;
        // The shell segment's second beat, the native alternating empty shell frame.
        var shellBeats = RoomSpriteObjectAnimations.Segments.Beats(RoomSpriteObjectSegment.MetroidShell);
        // The shell plays its first beat out, which brings it onto the second with that beat's whole duration.
        var first = AssertIs<AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>.Frame>(shellBeats[0], "first shell beat");
        for (int update = 0; update < first.Duration; update++)
            shell.Step();
        var shown = AssertIs<AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>.Frame>(shellBeats[1], "second shell beat");
        Require(shell.Frame == shown.Shown && shell.Timer == shown.Duration, "The shell must reach its second record with that record's whole duration.");
        Require(shell.InstructionPointer == 0xc4ba, "The reproduction must run the native empty shell record.");
        OamBuffer actual = ShellOam();
        Require(shell.SpritemapPointer == 0xbda6 && actual.NextByteOffset == 0,
            "The reproduction must begin on the native empty shell frame.");
        ushort retainedTimer = shell.FrameHold;
        ushort expectedPointer = RoomSpriteObjectVisualDefinitionsConstants.Spritemap(first.Shown);
        Require((InstalledFixtures.EnemyTiles.Spritemaps ?? throw new InvalidOperationException("Verification requires the installed enemy compositions.")).TryGetDisplay(0xb4, expectedPointer, out var parts),
            "Installed frozen shell artwork must exist.");
        var expected = new OamBuffer();
        expected.BeginFrame();
        expected.AddEnemySpritemap(parts, new Position16<SpritePixels>(120, 100), 0x0c00,
            checked((ushort)tileBase));
        Require(expected.NextByteOffset > 0, "The frozen shell must contain visible sprite parts.");
        for (int frame = 0; frame < 3; frame++)
        {
            MetroidBehavior.RunMetroidFrozen(slot);
            enemies.SpriteObjects.StepRoomSpriteObjects();
            Require(shell.SpritemapPointer == expectedPointer,
                $"Frozen shell retained ${shell.SpritemapPointer:X4}, expected ${expectedPointer:X4}.");
            Require(shell.FrameHold == retainedTimer && shell.DisableFlags == 1,
                "Freezing must preserve the native animation timer while disabling advancement.");
            actual = ShellOam();
            Require(actual.NextByteOffset == expected.NextByteOffset &&
                actual.ImageEquals(expected),
                $"Frozen shell OAM must match every authored part and frozen palette attribute: expected {Convert.ToHexString(expected.LowTable[..expected.NextByteOffset])}, " +
                $"got {Convert.ToHexString(actual.LowTable[..Math.Max(actual.NextByteOffset, actual.LastFinalizedSpriteCount * 4)])}.");
        }
        Console.WriteLine("Frozen Metroid shell: empty-frame reproduction, visible frozen OAM and retained timer passed.");
        return 0;

        OamBuffer ShellOam()
        {
            var drawn = new SpriteFrameBuffer();
            drawn.BeginFrame();
            enemies.SpriteObjects.DrawRoomSpriteObjects(drawn, Position16<RoomPixels>.Zero, EnemyFormatDrawAccess.NoHitBurstSheets);
            drawn.FinalizeFrame();
            var source = new SpriteSource(SpriteSourceKind.RoomSpriteObject, enemies.SpriteObjects.DrawInstanceOf(shell));
            var oam = new OamBuffer();
            oam.BeginFrame();
            oam.AppendDrawn(frame =>
            {
                foreach (SpriteDraw draw in drawn.Draws)
                    if (draw.Source == source)
                        frame.Add(draw);
            });
            return oam;
        }

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidDataException(message);
        }
    }

    /// <summary>Runs the frozen Metroid shell OAM reproduction.</summary>
    [VerificationCommand("--frozen-metroid-shell")]
    internal static int FrozenMetroidShellCommand()
    {
        return VerifyFrozenMetroidShell();
    }
}
