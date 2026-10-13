using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

/// <summary>Compares wrapped beam collision widths and trajectories against 1,080 native boundary records.</summary>
internal static class WrapShotWidthsVerification
{
    /// <summary>Checks shot positions, radii, lifetime, and remote door writes at adjacent one-pixel offsets.</summary>
    internal static void VerifyWrapShotWidths(string tracePath)
    {
        var native = File.ReadLines(tracePath).Skip(1).ToArray();
        AssertEqual(1080, native.Length, "Complete native width matrix");
        int[] widths = [144, 128, 64], heights = [80, 16, 192];
        int[] originsX = [2260, 2004, 38], originsY = [546, 34, 1606], targets = [0x1561, 0x301, 0x2981];
        int cursor = 0, mismatches = 0;
        for (int room = 0; room < 3; room++)
        for (int beam = 0; beam < 3; beam++)
        for (int offset = -1; offset <= 1; offset++)
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            int width = widths[room], height = heights[room], target = targets[room];
            bool left = room == 2;
            var words = new ushort[width * height];
            var bts = new byte[words.Length];
            for (int row = 0; row < height; row++) words[row * width + (left ? 0 : width - 1)] = 0x8000;
            words[target] = 0xc40c; bts[target] = 0x41;
            for (int row = 1; row < 4; row++) { words[target + row * width] = 0xd40c; bts[target + row * width] = 0xff; }
            var level = CreateRoom(new(checked((ushort)width), checked((ushort)height)), words, bts);
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new((ushort)(originsX[room] + offset), (ushort)originsY[room])),
                PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose,
                EquippedBeams = (ushort)(beam == 0 ? 0 : beam == 1 ? 1 : left ? 5 : 9) };
            var shots = CreateProjectileFixture();
            var bombs = CreateBombFixture();
            var plms = new RoomPlmSystem();
            int hit = -1;
            for (int frame = 0; frame < 40; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                bombs.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, input, input);
                shots.StepFrame(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new((ushort)(left ? 0 : width * 16 - 256), (ushort)Math.Max(0, originsY[room] - 128)), bombs);
                var shot = shots.Slots[0];
                ushort targetWord = level.GetCollisionBlockByIndex(target).LevelWord;
                if (hit < 0 && targetWord == 0x840c) hit = frame;
                string actual = $"{room},{beam},{offset},{frame},{shot.Position.X.Whole:X4},{shot.Position.X.Fraction:X4},{shot.Position.Y.Whole:X4},{shot.Position.Y.Fraction:X4},{shot.Type:X4},{shot.InstructionPointer:X4},{shot.Radius.X:X4},{shot.Radius.Y:X4},{targetWord:X4}";
                string expected = native[cursor++];
                if (actual != expected && mismatches++ < 8) Console.WriteLine($"Width mismatch:\n{actual}\n{expected}");
            }
            Console.WriteLine($"Width room={room} beam={beam} offset={offset}: hit={hit}");
            int expectedHit = room == 2 ? (beam == 0 ? -1 : 5) : (beam == 2 && offset <= 0 ? 9 : -1);
            AssertEqual(expectedHit, hit, "Native width and adjacent one-pixel activation boundary");
        }
        AssertEqual(0, mismatches, "Native beam-width trajectory, radii, lifetime and remote-door setup writes");
        Console.WriteLine("PASS 1080 original-cartridge width-boundary records.");
    }

    /// <summary>Runs the checked-in width-boundary trace comparison.</summary>
    [VerificationCommand("--wrap-shot-widths")]
    internal static int WrapShotWidthsCommand()
    {
        Suite(nameof(VerifyWrapShotWidths), () => VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv"));
        return 0;
    }
}
