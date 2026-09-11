using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCrystalFlashLifetime(string rom, string nativeCsv)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        AssertEqual("CA77210D138C654AEF79E44AAA897B5BE0F79244E2F72AB362D46303521BC043",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativeCsv))),
            "accepted original-CPU lifetime capture");
        string[] rows = File.ReadAllLines(nativeCsv);
        AssertEqual("left,offset,frame,phase,pose,anim,timer,y,health,missiles,supers,pbs,immunity,knockback", rows[0], "native lifetime schema");
        int row = 1;
        for (int left = 0; left < 2; left++)
        for (int offset = 0; offset < 8; offset++)
        {
            var samus = new SamusState
            {
                Pose = left != 0 ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose,
                XPosition = 128, YPosition = 128, Health = 49, MaxHealth = 1499,
                Missiles = 10, SuperMissiles = 10, PowerBombs = 10,
            };
            samus.RefreshCollisionRadii(bus);
            ushort input = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
            AssertTrue(samus.CrystalFlash.TryBegin(bus, samus, input), "lifetime starts from valid activation");
            int frame = 0;
            do
            {
                samus.InvincibilityTimer = 77;
                samus.KnockbackTimer = 5;
                samus.CrystalFlash.Step(bus, samus, (ushort)(frame + offset));
                samus.AnimateNoFx(bus, input);
                if (samus.PendingTransitionalPose is not null)
                    samus.ApplyPendingVerifiedAnimationTransition(bus);
                string actual = $"{left},{offset},{frame},{(int)samus.CrystalFlash.Phase},{samus.Pose:X4},{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.YPosition:X4},{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4}";
                AssertTrue(row < rows.Length, "native lifetime capture is not truncated");
                AssertEqual(rows[row++], actual, $"native lifetime left={left}, offset={offset}, frame={frame}");
                if (++frame > 400) throw new InvalidDataException("Crystal Flash did not finish.");
            } while (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive);
        }
        AssertEqual(rows.Length, row, "native lifetime has no unconsumed rows");
        Console.WriteLine($"Crystal Flash: {row - 1} original-CPU lifetime frames match across both facings and all eight NMI phases.");
    }

    /// <summary>Compare real Power Bomb cleanup admission with the original bank-$88 CPU probe.</summary>
    private static void VerifyCrystalFlashCleanup(string rom, string nativeCsv)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        AssertEqual("CCD507BE8423FEC78122CD95458577F21F58624510578184BF51EE25FF22A5F3",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativeCsv))),
            "accepted original-CPU Crystal Flash cleanup capture");
        string[] rows = File.ReadAllLines(nativeCsv);
        AssertEqual(37, rows.Length, "complete native Crystal Flash matrix");
        AssertEqual("case,left,pose,flag,immunity,knockback,health,missiles,supers,pbs", rows[0], "native cleanup trace schema");
        int row = 1;
        for (int test = 0; test < 18; test++)
        for (int left = 0; left < 2; left++)
        {
            var samus = new SamusState
            {
                Pose = left != 0 ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose,
                XPosition = 128, YPosition = 128, Health = 49, MaxHealth = 99,
                Missiles = 10, SuperMissiles = 10, PowerBombs = 10,
                InvincibilityTimer = 96, KnockbackTimer = 5,
            };
            samus.RefreshCollisionRadii(bus);
            ushort input = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
            switch (test)
            {
                case 1: samus.Health = 50; break;
                case 2: samus.Health = 51; break;
                case 3: samus.Missiles = 9; break;
                case 4: samus.SuperMissiles = 9; break;
                case 5: samus.PowerBombs = 9; break;
                case 6: samus.ReserveEnergy = 1; break;
                case 7: samus.Kinematics.YSpeed = 1; break;
                case 8: samus.Kinematics.YSubspeed = 1; break;
                case 9: samus.XPosition++; break;
                case 10: samus.YPosition++; break;
                case 11: samus.Kinematics.XSubposition = 0xffff; break;
                case 12: samus.Kinematics.YSubposition = 0xffff; break;
                case 13: input ^= (ushort)SnesButton.Down; break;
                case 14: input |= (ushort)SnesButton.A; break;
                case 15: samus.MaxReserveEnergy = 100; break;
                case 16: samus.MaxPowerBombs = 10; break;
                case 17: samus.Health = 0; break;
            }
            var level = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[0x2000]);
            var bombs = new SamusBombProjectileSystem();
            bombs.PowerBombExplosion.Arm();
            bombs.PowerBombExplosion.Spawn(128, 128);
            int frames = 0;
            do
            {
                bombs.StepFrame(bus, level, samus, input, 0, deferSamusOverlap: true);
                if (++frames > 1000) throw new InvalidDataException("Power Bomb never reached cleanup.");
            } while (bombs.PowerBombExplosion.Phase != PowerBombExplosionPhase.Inactive);
            string actual = $"{test},{left},{samus.Pose:X4},{bombs.PowerBombExplosion.Flag:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4},{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4}";
            AssertEqual(rows[row++], actual, $"original-CPU cleanup case {test}, left={left}");
        }
        Console.WriteLine("Crystal Flash: 36 original-CPU cleanup admission/resource/timer comparisons match.");
    }
}
