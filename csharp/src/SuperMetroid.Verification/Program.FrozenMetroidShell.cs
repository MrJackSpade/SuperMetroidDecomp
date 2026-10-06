using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // #1207: freezing during the shell's intentionally empty frame must select
    // the frozen shell immediately, including its actual emitted sprite parts.
    private static int VerifyFrozenMetroidShell()
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        string root = Path.GetFullPath("out/workbook-investigation/crocomire-install");
        var installation = GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
        var enemies = new RoomEnemySystem { TileArtwork = installation.LoadEnemyTiles() };
        var slot = enemies.Slots[0];
        slot.XPosition = 120;
        slot.YPosition = 100;
        typeof(RoomEnemySystem).GetMethod("InitializeMetroid", instance)!.Invoke(enemies, [slot]);
        var state = enemies.MetroidStates[0]!;
        state.OuterBodyA.XPosition = 500; // Isolate the shell from the electrical overlay.
        var shell = state.OuterBodyB;
        shell.InstructionPointer = 0xc4ba; // Native alternating empty shell record.
        typeof(RoomEnemySystem).GetMethod("LoadRoomSpriteObjectFrame",
            BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [shell]);
        var draw = typeof(RoomEnemySystem).GetMethod("DrawRoomSpriteObjects", instance)!;
        var freeze = typeof(RoomEnemySystem).GetMethod("RunMetroidFrozen", instance)!;
        var step = typeof(RoomEnemySystem).GetMethod("StepRoomSpriteObjects", instance)!;
        var actual = new OamBuffer();
        actual.BeginFrame();
        draw.Invoke(enemies, [actual, (ushort)0, (ushort)0]);
        Require(shell.SpritemapPointer == 0xbda6 && actual.NextByteOffset == 0,
            "The reproduction must begin on the native empty shell frame.");
        ushort retainedTimer = shell.InstructionTimer;
        ushort expectedPointer = RoomSpriteObjectVisualDefinitions.FrameAt(0xc4b8);
        Require(enemies.TileArtwork!.Spritemaps!.TryGetDisplay(0xb4, expectedPointer, out var parts),
            "Installed frozen shell artwork must exist.");
        var expected = new OamBuffer();
        expected.BeginFrame();
        expected.AddEnemySpritemap(parts, 120, 100, 0x0c00, 0);
        Require(expected.NextByteOffset > 0, "The frozen shell must contain visible sprite parts.");
        for (int frame = 0; frame < 3; frame++)
        {
            freeze.Invoke(enemies, [slot]);
            step.Invoke(enemies, null);
            Require(shell.SpritemapPointer == expectedPointer,
                $"Frozen shell retained ${shell.SpritemapPointer:X4}, expected ${expectedPointer:X4}.");
            Require(shell.InstructionTimer == retainedTimer && shell.DisableFlags == 1,
                "Freezing must preserve the native animation timer while disabling advancement.");
            actual.BeginFrame();
            draw.Invoke(enemies, [actual, (ushort)0, (ushort)0]);
            Require(actual.NextByteOffset == expected.NextByteOffset &&
                actual.LowTable.SequenceEqual(expected.LowTable) &&
                actual.HighTable.SequenceEqual(expected.HighTable),
                "Frozen shell OAM must match every authored part and frozen palette attribute.");
        }
        Console.WriteLine("Frozen Metroid shell: empty-frame reproduction, visible frozen OAM and retained timer passed.");
        return 0;

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidDataException(message);
        }
    }
}
