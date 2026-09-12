using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyWrapShotWidths(string tracePath)
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
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            int width = widths[room], height = heights[room], target = targets[room];
            bool left = room == 2;
            var words = new ushort[width * height];
            var bts = new byte[words.Length];
            for (int row = 0; row < height; row++) words[row * width + (left ? 0 : width - 1)] = 0x8000;
            words[target] = 0xc40c; bts[target] = 0x41;
            for (int row = 1; row < 4; row++) { words[target + row * width] = 0xd40c; bts[target + row * width] = 0xff; }
            var level = CreateRoom(width, height, words, bts);
            var samus = new SamusState { XPosition = (ushort)(originsX[room] + offset), YPosition = (ushort)originsY[room],
                PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose,
                EquippedBeams = (ushort)(beam == 0 ? 0 : beam == 1 ? 1 : left ? 5 : 9) };
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            var plms = new RoomPlmSystem();
            int hit = -1;
            for (int frame = 0; frame < 40; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                bombs.StepFrame(bus, level, samus, input, input);
                shots.StepFrame(bus, level, samus, input, input, (ushort)(left ? 0 : width * 16 - 256),
                    (ushort)Math.Max(0, originsY[room] - 128), bombs, roomPlms: plms);
                var shot = shots.Slots[0];
                ushort targetWord = level.GetCollisionBlockByIndex(target).LevelWord;
                if (hit < 0 && targetWord == 0x840c) hit = frame;
                string actual = $"{room},{beam},{offset},{frame},{shot.XPosition:X4},{shot.XSubposition:X4},{shot.YPosition:X4},{shot.YSubposition:X4},{shot.Type:X4},{shot.InstructionPointer:X4},{shot.XRadius:X4},{shot.YRadius:X4},{targetWord:X4}";
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
}
