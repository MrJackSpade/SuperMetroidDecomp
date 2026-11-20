using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Phantoon rain placement, eye direction, and shot reaction tables against the ROM.</summary>
internal static class PhantoonPatternsVerification
{
    /// <summary>Compares native pattern words and production RNG-driven rain and shot handoffs without table reads.</summary>
    internal static void VerifyCompiledPhantoonPatterns(CartridgeImportAddressSpace rom)
    {
        for (int i = 0; i < 8; i++)
        {
            PhantoonRainPattern placement = PhantoonRainPatternDefinitions.LaterRain.Choose(i);
            AssertEqual(rom.ReadCartridgeWord((0xa7cdad + i * 8)), placement.PathPlace, "Native rain figure-eight cursor");
            AssertEqual(rom.ReadCartridgeWord((0xa7cdaf + i * 8)), placement.BodyPosition.X, "Native rain body X");
            AssertEqual(rom.ReadCartridgeWord((0xa7cdb1 + i * 8)), placement.BodyPosition.Y, "Native rain body Y");
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa7cdb3 + i * 8)), "Native unused rain record word");
            AssertEqual(rom.ReadCartridgeByte(0xa7cfc2 + i), placement.FirstColumn.Place, "Native first rain column");
        }
        for (int high = 0; high < 256; high++)
        for (int pattern = 0; pattern < 8; pattern++)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            ushort random = LittleEndianBytes.Word(unchecked((byte)(0xf8 | pattern)), unchecked((byte)high));
            int calls = 0;
            enemies.VerificationServices().NextRandom = () => { calls++; return random; };
            var body = enemies.Places.InUpdateOrder[0];
            var eye = enemies.Places.InUpdateOrder[1];
            var state = new PhantoonEnemyState(body.BoundFor(EnemyDefinitionId.PhantoonBody), body, eye, enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3]);
            enemies.BeginPhantoonAsSetUp(state);
            state.PhaseWait.LastFor(1);
            state.FigureEight.Direction = PhantoonPathDirection.Backwards;
            enemies.PhantoonBehavior.Patterns.RunPhantoonHiddenFlameRain(body.HeldOccupant(), body, state);
            AssertEqual(rom.ReadCartridgeWord((0xa7cdad + pattern * 8)), state.FigureEight.PathPlace.Place, "Real rain cursor handoff");
            AssertEqual(rom.ReadCartridgeWord((0xa7cdaf + pattern * 8)), body.Position.X.Whole, "Real rain body placement X");
            AssertEqual(rom.ReadCartridgeWord((0xa7cdb1 + pattern * 8)), body.Position.Y.Whole, "Real rain body placement Y");
            AssertEqual(PhantoonPathDirection.Forwards, state.FigureEight.Direction, "Real rain direction reset");
            AssertEqual(PhantoonPhase.BecomeSolidAfterFlameRain, state.Phase, "Real rain phase handoff");
            AssertEqual(1, calls, "Rain consumes one RNG word");
            var flames = enemies.Projectiles.EnemyProjectiles.Where(p => p.IsActive).OrderBy(p => enemies.PhantoonBehavior.Combat.DestroyableFlame.Rains.Of(p).HangUpdates).ToArray();
            AssertEqual(8, flames.Length, "Actual rain population");
            for (int i = 0; i < 8; i++)
            {
                int column = (rom.ReadCartridgeByte(0xa7cfc2 + pattern) + i) % 9;
                AssertEqual(rom.ReadCartridgeByte(0x8698f7 + column), flames[i].Position.X.Whole, "Actual rain column order with wrap");
                AssertEqual((ushort)40, flames[i].Position.Y.Whole, "Actual rain ceiling Y");
                AssertEqual((i + 1) * 8, enemies.PhantoonBehavior.Combat.DestroyableFlame.Rains.Of(flames[i]).HangUpdates, "Actual staggered rain delay");
            }
        }

        EyeTarget[] eyeTargets = PhantoonEyeTargets.All;
        PhantoonEyeTargets.VerifyGazeProgramsAgainstCartridge(rom);
        var eyeEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemyPlace, PhantoonEnemyState, SamusState> pointEye = PhantoonPatterns.PointPhantoonEyeAtSamus;
        RoomEnemyPlace eyeBody = eyeEnemies.Places.InUpdateOrder[0];
        RoomEnemyPlace trackingEye = eyeEnemies.Places.InUpdateOrder[1];
        var eyeState = new PhantoonEnemyState(eyeBody.BoundFor(EnemyDefinitionId.PhantoonBody), eyeBody, trackingEye, eyeEnemies.Places.InUpdateOrder[2], eyeEnemies.Places.InUpdateOrder[3]);
        eyeBody.Position = eyeBody.Position.WithWhole(new(0x4000, 0x4000));
        foreach ((short x, short y, CompassDirection direction, _) in eyeTargets)
        {
            pointEye(eyeBody, eyeState, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(eyeBody.Position.X.Whole + x)), unchecked((ushort)(eyeBody.Position.Y.Whole + y)))),
            });
            AssertEqual(PhantoonEyeGaze.EyeAnimation(direction),
                eyeState.EyePart.PlayingProgram(),
                $"production Phantoon eye direction {direction}");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonEyeGaze.EyeAnimation((CompassDirection)Enum.GetValues<CompassDirection>().Length),
            "Phantoon eye direction beyond the eight directions");
        var shotEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        var shotBody = shotEnemies.Places.InUpdateOrder[0];
        var shotState = new PhantoonEnemyState(shotBody.BoundFor(EnemyDefinitionId.PhantoonBody), shotBody, shotEnemies.Places.InUpdateOrder[1], shotEnemies.Places.InUpdateOrder[2], shotEnemies.Places.InUpdateOrder[3]);
        Action<RoomEnemyPlace, PhantoonEnemyState, ushort, int> shot = (shotPlace, shotPhantoon, shotType, shotDamage) =>
            shotEnemies.PhantoonBehavior.Combat.ResolvePhantoonShotReaction(shotPlace, shotPhantoon, SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(shotType), shotDamage);
        ushort shotRandom = 0;
        int shotCalls = 0;
        shotEnemies.VerificationServices().NextRandom = () => { shotCalls++; return shotRandom; };
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            shotRandom = (ushort)raw;
            shotBody.Health = 1000;
            shotBody.AiStates = EnemyAiStates.Hurt;
            shotState.Phase = PhantoonPhase.EyeTracksSamus;
            shotState.PhaseWait.LastFor(60);
            shotState.WindowEnd = PhantoonWindowEnd.EyeCloses;
            shotState.Round.Damage = 0;
            shot(shotBody, shotState, 0x100, 1);
            AssertEqual(16, shotState.PhaseWait.UpdatesLeft, "Real shot window shortening preserved");
            AssertEqual(raw + 1, shotCalls, "Shot consumes exactly one RNG call");
        }
        Console.WriteLine("Phantoon patterns: 41 native record/eye words, 8 bytes, all eight real eye octants, 2048 rain handoffs and 65536 bus-free shot reactions match.");
    }

}
