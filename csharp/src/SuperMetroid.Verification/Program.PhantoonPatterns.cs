using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPhantoonPatterns(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int i = 0; i < 8; i++)
        {
            var placement = PhantoonPatternDefinitions.RainPlacement(i);
            AssertEqual(Word(0xa7cdad + i * 8), placement.Cursor, "Native rain figure-eight cursor");
            AssertEqual(Word(0xa7cdaf + i * 8), placement.X, "Native rain body X");
            AssertEqual(Word(0xa7cdb1 + i * 8), placement.Y, "Native rain body Y");
            AssertEqual((ushort)0, Word(0xa7cdb3 + i * 8), "Native unused rain record word");
            AssertEqual(rom.ReadByte(0xa7cfc2 + i), PhantoonPatternDefinitions.FirstRainColumns[i], "Native first rain column");
            AssertEqual(rom.ReadByte(0xa7cda5 + i), PhantoonPatternDefinitions.ShotEyeMarkers[i], "Native shot eye marker");
        }
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        var randomField = typeof(RoomEnemySystem).GetField("_nextRandom", flags)!;
        var rainMethod = typeof(RoomEnemySystem).GetMethod("RunPhantoonHiddenFlameRain", flags)!;
        for (int high = 0; high < 256; high++)
        for (int pattern = 0; pattern < 8; pattern++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, new PhantoonPatternReadGuard(rom));
            ushort random = (ushort)((high << 8) | 0xf8 | pattern);
            int calls = 0;
            randomField.SetValue(enemies, (Func<ushort>)(() => { calls++; return random; }));
            var body = enemies.Slots[0];
            var eye = enemies.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye };
            body.VariableE = 1;
            eye.VariableC = 123;
            rainMethod.CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState>>(enemies)(body, state);
            AssertEqual(Word(0xa7cdad + pattern * 8), body.VariableA, "Real rain cursor handoff");
            AssertEqual(Word(0xa7cdaf + pattern * 8), body.XPosition, "Real rain body placement X");
            AssertEqual(Word(0xa7cdb1 + pattern * 8), body.YPosition, "Real rain body placement Y");
            AssertEqual((ushort)0, eye.VariableC, "Real rain direction reset");
            AssertEqual((ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain, body.VariableF, "Real rain phase handoff");
            AssertEqual(1, calls, "Rain consumes one RNG word");
            var flames = enemies.EnemyProjectiles.Where(p => p.IsActive).OrderBy(p => p.XVelocity).ToArray();
            AssertEqual(8, flames.Length, "Actual rain population");
            for (int i = 0; i < 8; i++)
            {
                int column = (rom.ReadByte(0xa7cfc2 + pattern) + i) % 9;
                AssertEqual((ushort)rom.ReadByte(0x8698f7 + column), flames[i].XPosition, "Actual rain column order with wrap");
                AssertEqual((ushort)40, flames[i].YPosition, "Actual rain ceiling Y");
                AssertEqual((ushort)((i + 1) * 8), flames[i].XVelocity, "Actual staggered rain delay");
            }
        }
        var shotEnemies = new RoomEnemySystem();
        var shotBody = shotEnemies.Slots[0];
        var shotState = new PhantoonEnemyState(shotBody) { Eye = shotEnemies.Slots[1], Tentacles = shotEnemies.Slots[2], Mouth = shotEnemies.Slots[3] };
        var shot = typeof(RoomEnemySystem).GetMethod("ResolvePhantoonShotReaction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState, ushort, ushort>>(shotEnemies);
        ushort shotRandom = 0;
        int shotCalls = 0;
        randomField.SetValue(shotEnemies, (Func<ushort>)(() => { shotCalls++; return shotRandom; }));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            shotRandom = (ushort)raw;
            shotBody.Health = 1000;
            shotBody.AiHandlerBits = 2;
            shotBody.VariableF = (ushort)PhantoonAiFunction.EyeTracksSamus;
            shotBody.VariableE = 60;
            shotState.Tentacles.VariableA = shotState.Tentacles.VariableB = 0;
            shot(shotBody, shotState, 0x100, 1);
            AssertEqual((ushort)rom.ReadByte(0xa7cda5 + (raw & 7)), shotState.Eye.VariableB, "Real shot exact eye marker without bus");
            AssertEqual((ushort)(raw & 7), shotState.Mouth.Parameter2, "Real shot retains selected pattern");
            AssertEqual((ushort)16, shotBody.VariableE, "Real shot window shortening preserved");
            AssertEqual(raw + 1, shotCalls, "Shot consumes exactly one RNG call");
        }
        Console.WriteLine("Phantoon patterns: 32 native record words, 16 bytes, 2048 real eight-flame rain handoffs and 65536 bus-free shot reactions match.");
    }

    private sealed class PhantoonPatternReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0xa7cda5 and < 0xa7cded or >= 0xa7cfc2 and < 0xa7cfca)
                throw new InvalidOperationException($"Migrated Phantoon pattern ROM read at {address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected rain initialization bus write.");
    }
}
