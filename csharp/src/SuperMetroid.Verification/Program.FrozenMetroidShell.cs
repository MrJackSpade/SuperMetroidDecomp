using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class FrozenMetroidShellVerification
{
    // #1207: freezing during the shell's intentionally empty frame must select
    // the frozen shell immediately, including its actual emitted sprite parts.
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static int VerifyFrozenMetroidShell()
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        var slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(120, 100));
        enemies.MetroidBehavior.InitializeMetroid(slot);
        var state = enemies.MetroidStates[0]!;
        state.OuterBodyA.Position = state.OuterBodyA.Position.WithWholeX(500); // Isolate the shell from the electrical overlay.
        var shell = state.OuterBodyB;
        shell.InstructionPointer = 0xc4ba; // Native alternating empty shell record.
        RoomSpriteObjectPool.LoadRoomSpriteObjectFrame(shell);
        var actual = new OamBuffer();
        actual.BeginFrame();
        enemies.SpriteObjects.DrawRoomSpriteObjects(actual, Position16<RoomPixels>.Zero);
        Require(shell.SpritemapPointer == 0xbda6 && actual.NextByteOffset == 0,
            "The reproduction must begin on the native empty shell frame.");
        ushort retainedTimer = shell.InstructionTimer;
        ushort expectedPointer = RoomSpriteObjectVisualDefinitions.FrameAt(0xc4b8);
        Require(enemies.TileArtwork!.Spritemaps!.TryGetDisplay(0xb4, expectedPointer, out var parts),
            "Installed frozen shell artwork must exist.");
        var expected = new OamBuffer();
        expected.BeginFrame();
        expected.AddEnemySpritemap(parts, new Position16<OamPixels>(120, 100), 0x0c00, 0);
        Require(expected.NextByteOffset > 0, "The frozen shell must contain visible sprite parts.");
        for (int frame = 0; frame < 3; frame++)
        {
            enemies.MetroidBehavior.RunMetroidFrozen(slot);
            enemies.SpriteObjects.StepRoomSpriteObjects();
            Require(shell.SpritemapPointer == expectedPointer,
                $"Frozen shell retained ${shell.SpritemapPointer:X4}, expected ${expectedPointer:X4}.");
            Require(shell.InstructionTimer == retainedTimer && shell.DisableFlags == 1,
                "Freezing must preserve the native animation timer while disabling advancement.");
            actual.BeginFrame();
            enemies.SpriteObjects.DrawRoomSpriteObjects(actual, Position16<RoomPixels>.Zero);
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

    [VerificationCommand("--frozen-metroid-shell")]
    internal static int FrozenMetroidShellCommand()
    {
        return VerifyFrozenMetroidShell();
    }
}
