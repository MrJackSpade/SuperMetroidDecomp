using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyMaridiaPuyoPile()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CartridgeRoomHeader.Load(bus, 0xd27e);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var enemies = new RoomEnemySystem();
        var random = new Bank80SystemState();
        enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
            new SnesVram(), new SnesCgram(), random.NextRandom, random.SetRandomNumber);
        // These are the six literal retail population records, not a constructed pile.
        (ushort X, ushort Y, ushort Delay)[] population =
            [(87,156,0x0f00), (98,156,0x0f00), (105,156,0x0f00),
             (91,152,0x0e00), (101,152,0x0e00), (97,148,0x0d00)];
        AssertEqual(population.Length, enemies.EnemyCount, "reported room has six authored Puyos");
        for (int i = 0; i < population.Length; i++)
        {
            var actor = enemies.Slots[i];
            AssertEqual((ushort)0xcfbf, actor.EnemyDefinitionPointer, "pile actor is retail Puyo");
            AssertEqual(population[i].X, actor.XPosition, "native pile spawn X");
            AssertEqual(population[i].Y, actor.YPosition, "native pile spawn Y");
            AssertEqual(population[i].Delay, enemies.PuyoStates[i]!.HopCooldownTimer, "native full-word initial cooldown");
        }
        var firstHop = Enumerable.Repeat(-1, population.Length).ToArray();
        var samus = new SamusState { XPosition = 220, YPosition = 128, Health = 999, MaxHealth = 999 };
        for (int frame = 1; frame <= 3841; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            for (int i = 0; i < population.Length; i++)
            {
                var state = enemies.PuyoStates[i]!;
                if (firstHop[i] < 0 && state.Function == PuyoEnemyFunction.Airborne) firstHop[i] = frame;
                if (frame <= population[i].Delay)
                {
                    AssertEqual(PuyoEnemyFunction.Grounded, state.Function, "native cooldown holds pile member grounded");
                    AssertEqual(population[i].X, enemies.Slots[i].XPosition, "waiting pile X does not drift");
                    AssertEqual(population[i].Y, enemies.Slots[i].YPosition, "waiting pile Y does not collapse");
                }
            }
        }
        for (int i = 0; i < population.Length; i++)
            AssertEqual(population[i].Delay + 1, firstHop[i], "native DEC/BPL starts hop on signed timer underflow");
        Console.WriteLine($"Maridia $04/$10 authored Puyo pile: first hops at {string.Join(',', firstHop)} AI updates; all six spawn coordinates and waiting trajectories match retail data.");
    }
}
