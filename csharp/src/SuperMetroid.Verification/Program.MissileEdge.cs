using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyMissileImpactCameraEdge(string nativeTrace)
    {
        var native = File.ReadLines(nativeTrace).Skip(1).Select(line => line.Split(','))
            .ToDictionary(row => (int.Parse(row[0]), int.Parse(row[1])), row => row.Skip(2)
                .Select(value => Convert.ToUInt16(value, 16)).ToArray());
        int compared = 0;
        for (int edge = 0; edge < 2; edge++)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var blocks = new ushort[64 * 16];
            for (int row = 0; row < 16; row++) blocks[row * 64 + 32] = 0x8000;
            var room = CreateRoom(64, 16, blocks, new byte[blocks.Length]);
            var samus = new SamusState { Pose = 1, XPosition = 497, YPosition = 128,
                SelectedHudItem = 1, Missiles = 5, MaxMissiles = 5 };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 20; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                shared.StepFrame(bus, room, samus, input, input);
                var result = projectiles.StepFrame(bus, room, samus, input, input,
                    (ushort)(edge == 1 ? 192 : 384), 0, shared);
                if (frame == 0) AssertEqual((int?)0, result.FiredSlot, "Missile edge fixture uses actual fire dispatch");
                var shot = projectiles.Slots[0];
                ushort[] actual = [shot.XPosition, shot.XSubposition, shot.YPosition, shot.YSubposition,
                    unchecked((ushort)shot.XVelocity), unchecked((ushort)shot.YVelocity), shot.Type, shot.InstructionPointer];
                AssertTrue(actual.SequenceEqual(native[(edge, frame)]),
                    $"Missile edge={edge} frame={frame} native comparison: {string.Join(',', actual.Select(v => v.ToString("X4")))}");
                compared++;
                if (!shot.IsActive || shot.PackedType.Family == SamusProjectileFamily.MissileExplosion)
                {
                    AssertEqual(edge == 0, shot.IsActive, "Only the centered-camera impact retains its explosion");
                    break;
                }
            }
        }
        AssertEqual(native.Count, compared, "Every native missile impact and lifetime frame is checked");
        Console.WriteLine($"Missile impact camera edge: {compared} original-cartridge frames match, including retained and deleted explosions.");
    }
}
