using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Phantoon rain placement, eye direction, and shot reaction tables against the ROM.</summary>
internal static class PhantoonPatternsVerification
{
    /// <summary>Compares native pattern words and production RNG-driven rain and shot handoffs without table reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon patterns")]
    internal static void VerifyCompiledPhantoonPatterns(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 8; i++)
        {
            var placement = PhantoonPatternDefinitions.RainPlacement(i);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdad + i * 8)), placement.Cursor, "Native rain figure-eight cursor");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdaf + i * 8)), placement.X, "Native rain body X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb1 + i * 8)), placement.Y, "Native rain body Y");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb3 + i * 8)), "Native unused rain record word");
            AssertEqual(rom.ReadCartridgeByte(0xa7cfc2 + i), PhantoonPatternDefinitions.FirstRainColumns[i], "Native first rain column");
            AssertEqual(rom.ReadCartridgeByte(0xa7cda5 + i), PhantoonPatternDefinitions.ShotEyeMarkers[i], "Native shot eye marker");
        }
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (int high = 0; high < 256; high++)
        for (int pattern = 0; pattern < 8; pattern++)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(NewPhantoonPatternReadGuard(rom));
            ushort random = SnesLittleEndian.Word(unchecked((byte)(0xf8 | pattern)), unchecked((byte)high));
            int calls = 0;
            enemies.VerificationServices().NextRandom = ((Func<ushort>)(() => { calls++; return random; }));
            var body = enemies.SlotTable.Slots[0];
            var eye = enemies.SlotTable.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye };
            body.VariableE = 1;
            eye.VariableC = 123;
            new Action<RoomEnemySlot, PhantoonEnemyState>(enemies.PhantoonBehavior.RunPhantoonHiddenFlameRain)(body, state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdad + pattern * 8)), body.VariableA, "Real rain cursor handoff");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdaf + pattern * 8)), body.Position.X.Whole, "Real rain body placement X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb1 + pattern * 8)), body.Position.Y.Whole, "Real rain body placement Y");
            AssertEqual((ushort)0, eye.VariableC, "Real rain direction reset");
            AssertEqual((ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain, body.VariableF, "Real rain phase handoff");
            AssertEqual(1, calls, "Rain consumes one RNG word");
            var flames = enemies.Projectiles.EnemyProjectiles.Where(p => p.IsActive).OrderBy(p => p.Velocity.X.RawValue).ToArray();
            AssertEqual(8, flames.Length, "Actual rain population");
            for (int i = 0; i < 8; i++)
            {
                int column = (rom.ReadCartridgeByte(0xa7cfc2 + pattern) + i) % 9;
                AssertEqual((ushort)rom.ReadCartridgeByte(0x8698f7 + column), flames[i].Position.X.Whole, "Actual rain column order with wrap");
                AssertEqual((ushort)40, flames[i].Position.Y.Whole, "Actual rain ceiling Y");
                AssertEqual((ushort)((i + 1) * 8), flames[i].Velocity.X.RawValue, "Actual staggered rain delay");
            }
        }

        (short X, short Y, ushort Direction)[] eyeTargets =
        [
            (0, -100, 0), (100, -100, 1), (100, 0, 2), (100, 100, 3),
            (0, 100, 4), (-100, 100, 6), (-100, 0, 7), (-100, -100, 8),
        ];
        for (ushort direction = 0; direction < 9; direction++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7d40d + direction * 2)),
                PhantoonPatternDefinitions.EyeInstruction(direction),
                $"native Phantoon eye direction {direction}");
        }
        var eyeEnemies = new RoomEnemySystem();
        eyeEnemies.BindFixtureBus(NewPhantoonPatternReadGuard(rom));
        Action<RoomEnemySlot, RoomEnemySlot, SamusState> pointEye = PhantoonBehavior.PointPhantoonEyeAtSamus;
        RoomEnemySlot eyeBody = eyeEnemies.SlotTable.Slots[0];
        RoomEnemySlot trackingEye = eyeEnemies.SlotTable.Slots[1];
        eyeBody.Position = eyeBody.Position.WithWhole(new(0x4000, 0x4000));
        foreach ((short x, short y, ushort direction) in eyeTargets)
        {
            pointEye(eyeBody, trackingEye, new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(eyeBody.Position.X.Whole + x)), unchecked((ushort)(eyeBody.Position.Y.Whole + y)))),
            });
            AssertEqual(PhantoonPatternDefinitions.EyeInstruction(direction),
                trackingEye.CurrentInstruction,
                $"production Phantoon eye direction {direction}");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonPatternDefinitions.EyeInstruction(9),
            "Phantoon eye direction beyond authored table");
        var shotEnemies = new RoomEnemySystem();
        var shotBody = shotEnemies.SlotTable.Slots[0];
        var shotState = new PhantoonEnemyState(shotBody) { Eye = shotEnemies.SlotTable.Slots[1], Tentacles = shotEnemies.SlotTable.Slots[2], Mouth = shotEnemies.SlotTable.Slots[3] };
        Action<RoomEnemySlot, PhantoonEnemyState, ushort, ushort> shot = shotEnemies.PhantoonBehavior.ResolvePhantoonShotReaction;
        ushort shotRandom = 0;
        int shotCalls = 0;
        shotEnemies.VerificationServices().NextRandom = ((Func<ushort>)(() => { shotCalls++; return shotRandom; }));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            shotRandom = (ushort)raw;
            shotBody.Health = 1000;
            shotBody.AiHandlerBits = 2;
            shotBody.VariableF = (ushort)PhantoonAiFunction.EyeTracksSamus;
            shotBody.VariableE = 60;
            shotState.Tentacles.VariableA = shotState.Tentacles.VariableB = 0;
            shot(shotBody, shotState, 0x100, 1);
            AssertEqual((ushort)rom.ReadCartridgeByte(0xa7cda5 + (raw & 7)), shotState.Eye.VariableB, "Real shot exact eye marker without bus");
            AssertEqual((ushort)(raw & 7), shotState.Mouth.Parameter2, "Real shot retains selected pattern");
            AssertEqual((ushort)16, shotBody.VariableE, "Real shot window shortening preserved");
            AssertEqual(raw + 1, shotCalls, "Shot consumes exactly one RNG call");
        }
        Console.WriteLine("Phantoon patterns: 41 native record/eye words, 16 bytes, all eight real eye octants, 2048 rain handoffs and 65536 bus-free shot reactions match.");
    }

    /// <summary>Blocks migrated Phantoon pattern ranges and unintended writes during behavior checks.</summary>
    internal static CartridgeReadGuard NewPhantoonPatternReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa7cda5 and < 0xa7cded or
                >= 0xa7cfc2 and < 0xa7cfca or
                >= 0xa7d40d and < 0xa7d41f,
                address => $"Migrated Phantoon pattern ROM read at {address:X6}.")
            .RejectWrites(_ => "Unexpected rain initialization bus write.");
}
