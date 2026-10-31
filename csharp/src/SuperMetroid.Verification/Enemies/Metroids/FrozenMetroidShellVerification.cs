using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
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
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        // The shell draws as a Metroid room loads it: at the Metroid's tile base and in its palette.
        enemies.GraphicsSet.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, SpawnedObjectFixtures.SetLoading(EnemyDefinitionId.Metroid), new SpriteColourSets());
        SpriteGraphics indexes = enemies.GraphicsSet.DrawIndexes(EnemyDefinitionId.Metroid);
        var slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(120, 100));
        slot.Bind(EnemyDefinitionId.Metroid, slot.Definition);
        slot.TileIndexBase = indexes.TileBase;
        slot.Colouring = indexes.Colouring;
        slot.SetSpawnWords(EnemyDefinitionId.Metroid, words => words);
        enemies.MetroidBehavior.InitializeMetroid(slot);
        var state = enemies.MetroidStates[0];
        (state ?? throw new InvalidOperationException("Verification requires state.")).OuterBodyA.Position = state.OuterBodyA.Position.WithWholeX(500); // Isolate the shell from the electrical overlay.
        var shell = state.OuterBodyB;
        // The shell program's second record, the native alternating empty shell frame.
        var shellSteps = RoomSpriteObjectAnimations.Programs.Steps(RoomSpriteObjectProgram.MetroidShell);
        shell.Cursor = new(RoomSpriteObjectProgram.MetroidShell, 2);
        var shown = AssertIs<AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.ShowStep>(shellSteps[1], "second shell step");
        shell.Frame = shown.Frame;
        shell.Timer = WrappingCountdown.Start(shown.Duration);
        Require(shell.InstructionPointer == 0xc4ba, "The reproduction must run the native empty shell record.");
        var actual = new OamBuffer();
        actual.BeginFrame();
        enemies.SpriteObjects.DrawRoomSpriteObjectsAsOam(actual, Position16<RoomPixels>.Zero);
        Require(shell.SpritemapPointer == 0xbda6 && actual.NextByteOffset == 0,
            "The reproduction must begin on the native empty shell frame.");
        ushort retainedTimer = shell.InstructionTimer;
        ushort expectedPointer = RoomSpriteObjectVisualDefinitionsConstants.Spritemap(AssertIs<AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.ShowStep>(shellSteps[0], "first shell step").Frame);
        Require(((enemies.TileArtwork ?? throw new InvalidOperationException("Verification requires installed enemy artwork.")).Spritemaps ?? throw new InvalidOperationException("Verification requires enemies.TileArtwork.Spritemaps.")).TryGetDisplay(0xb4, expectedPointer, out var parts),
            "Installed frozen shell artwork must exist.");
        var expected = new OamBuffer();
        expected.BeginFrame();
        expected.AddEnemySpritemap(parts, new Position16<SpritePixels>(120, 100), 0x0c00,
            checked((ushort)indexes.TileBase));
        Require(expected.NextByteOffset > 0, "The frozen shell must contain visible sprite parts.");
        for (int frame = 0; frame < 3; frame++)
        {
            MetroidBehavior.RunMetroidFrozen(slot);
            enemies.SpriteObjects.StepRoomSpriteObjects();
            Require(shell.SpritemapPointer == expectedPointer,
                $"Frozen shell retained ${shell.SpritemapPointer:X4}, expected ${expectedPointer:X4}.");
            Require(shell.InstructionTimer == retainedTimer && shell.DisableFlags == 1,
                "Freezing must preserve the native animation timer while disabling advancement.");
            actual.BeginFrame();
            enemies.SpriteObjects.DrawRoomSpriteObjectsAsOam(actual, Position16<RoomPixels>.Zero);
            Require(actual.NextByteOffset == expected.NextByteOffset &&
                actual.ImageEquals(expected),
                "Frozen shell OAM must match every authored part and frozen palette attribute.");
        }
        Console.WriteLine("Frozen Metroid shell: empty-frame reproduction, visible frozen OAM and retained timer passed.");
        return 0;

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
