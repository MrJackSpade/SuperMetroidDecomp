using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyWrapShotTrace(string nativeTrace)
    {
        var native = File.ReadLines(nativeTrace).Skip(1).ToArray();
        AssertEqual(240, native.Length, "Complete wrap-shot native matrix");
        int mismatches = 0;
        for (int left = 0; left < 2; left++)
        for (int beam = 0; beam < 3; beam++)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var words = new ushort[64 * 128];
            for (int row = 0; row < 128; row++)
            {
                words[row * 64 + (left != 0 ? 63 : 0)] = 0x4000;
                words[row * 64 + (left != 0 ? 0 : 63)] = 0x8000;
            }
            var level = CreateRoom(64, 128, words, new byte[words.Length]);
            var samus = new SamusState
            {
                PoseId = left != 0 ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose,
                XPosition = (ushort)(left != 0 ? 32 : 992), YPosition = 128,
                EquippedBeams = (ushort)(beam == 0 ? 0 : beam == 1 ? 1 : 5),
            };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var plms = new RoomPlmSystem();
            int firstRemoteHit = -1;
            for (int frame = 0; frame < 40; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                shared.StepFrame(bus, level, samus, input, input);
                projectiles.StepFrame(bus, level, samus, input, input, (ushort)(left != 0 ? 0 : 768), 0, shared, roomPlms: plms);
                var shot = projectiles.Slots[0];
                string actual = $"{left},{beam},{frame},{shot.XPosition:X4},{shot.XSubposition:X4},{shot.YPosition:X4},{shot.YSubposition:X4},{shot.Type:X4},{shot.InstructionPointer:X4},";
                for (int row = 0; row < 128; row++)
                {
                    var block = level.GetCollisionBlock(left != 0 ? 63 : 0, row);
                    if (block.LevelWord != 0x4000) actual += $"{block.Index:X4}:{block.LevelWord:X4};";
                }
                int target = left != 0 ? 0x123f : 0x0280;
                if (firstRemoteHit < 0 && level.GetCollisionBlock(target % 64, target / 64).LevelWord == 0x0052)
                {
                    firstRemoteHit = frame;
                    AssertTrue(shot.IsActive, "Remote reaction occurs while the real projectile is retained");
                }
                string expected = native[(left * 3 + beam) * 40 + frame];
                if (actual != expected && mismatches++ < 8) Console.WriteLine($"Wrap mismatch:\n{actual}\n{expected}");
            }
            AssertEqual(beam == 0 ? -1 : left != 0 ? 3 : 4, firstRemoteHit, "Exact native remote-tile reaction and non-Wave negative");
        }
        AssertEqual(0, mismatches, "Original cartridge wrap-shot positions, lifetime and remote block writes");
        Console.WriteLine("PASS wrap-shot addressing: 240 original-cartridge frames, both edges and non-Wave negatives.");
    }
}
