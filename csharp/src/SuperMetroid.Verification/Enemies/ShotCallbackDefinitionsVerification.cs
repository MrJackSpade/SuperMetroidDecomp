using SuperMetroid.AssetExtraction.Enemies.Callbacks;
using SuperMetroid.Verification.Samus.Projectiles;
using System.Text.RegularExpressions;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombCallbackDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks bank-qualified enemy touch and shot no-op callback identities against pinned cartridge data.</summary>
internal static class ShotCallbackDefinitionsVerification
{
    /// <summary>Inventories callback pointers in enemy headers and hitboxes, then checks classification and combat admission.</summary>
    internal static void VerifyShotCallbackDefinitions(SuperMetroidAddressSpace rom)
    {
        var shotCallbacks = new HashSet<int>();
        var touchCallbacks = new HashSet<int>();
        int headers = 0, lists = 0, boxes = 0;
        string disassemblySource = FindCallbackDisassemblySource();
        foreach (string line in File.ReadLines(Path.Combine(disassemblySource, "bank_A0.asm")))
        {
            Match match = Regex.Match(line, @"^EnemyHeaders_\w+:\s*;([0-9A-F]{6});");
            if (!match.Success) continue;
            int address = Convert.ToInt32(match.Groups[1].Value, 16);
            int bank = rom.ReadCartridgeByte(address + 12) << 16;
            touchCallbacks.Add(bank | CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 48)));
            shotCallbacks.Add(bank | CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 50)));
            headers++;
        }
        foreach (string file in Directory.EnumerateFiles(disassemblySource, "bank_*.asm"))
        {
            bool pending = false;
            foreach (string line in File.ReadLines(file))
            {
                // Count-prefixed standard hitboxes only. Kraid's separately named
                // HitboxDefinitionTable and eight KraidMouth rectangles are raw
                // geometry, not count-prefixed callback-bearing lists.
                if (Regex.IsMatch(line, @"^(?:UNUSED_)?Hitbox_(?!KraidMouth_)\w+:")) pending = true;
                Match match = Regex.Match(line, @";([0-9A-F]{6});");
                if (!pending || !match.Success) continue;
                pending = false;
                int address = Convert.ToInt32(match.Groups[1].Value, 16);
                int count = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                AssertTrue(count < 100, "native hitbox inventory count sanity");
                lists++;
                for (int box = 0; box < count; box++)
                {
                    int hitbox = address + 2 + box * 12;
                    touchCallbacks.Add((SnesAddressMath.BankBase(address)) | CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + 8)));
                    shotCallbacks.Add((SnesAddressMath.BankBase(address)) | CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + 10)));
                    boxes++;
                }
            }
            AssertTrue(!pending, "native hitbox label has an addressed definition");
        }
        AssertEqual(163, headers, "pinned enemy header inventory");
        AssertEqual(221, lists, "pinned named hitbox list inventory including unused lists");
        AssertEqual(309, boxes, "pinned hitbox entries");
        AssertEqual(80, shotCallbacks.Count, "distinct header and hitbox shot callbacks");
        AssertEqual(71, touchCallbacks.Count, "distinct header and hitbox touch callbacks");
        var expectedShotNoOps = shotCallbacks
            .Where(address => (SnesAddressMath.OffsetInBank(address)) != 0 && rom.ReadCartridgeByte(address) == 0x6b)
            .ToHashSet();
        var expectedTouchNoOps = touchCallbacks
            .Where(address => (SnesAddressMath.OffsetInBank(address)) != 0 && rom.ReadCartridgeByte(address) == 0x6b)
            .ToHashSet();
        AssertEqual(12, expectedShotNoOps.Count, "native literal shot no-op count");
        AssertEqual(15, expectedTouchNoOps.Count, "native literal touch no-op count");
        var classifyShot = new Func<byte, ushort, bool>((bank, pointer) =>
            EnemyCollision.IsLiteralNoOpShotAi(
                default(RoomEnemyDefinition) with { NoOpCallbackBody = EnemyInteractionCallbacks.NoOpCallbackBody(bank) },
                EnemyInteractionCallbacks.TryDecode(bank, pointer)));
        foreach (int address in shotCallbacks)
            AssertEqual(expectedShotNoOps.Contains(address), classifyShot(SnesLittleEndianConstants.BankByte(address), (ushort)address), "real shot classifier native parity");
        for (int bank = 0; bank <= 255; bank++)
        for (int pointer = 0; pointer <= 65535; pointer++)
        {
            int address = ((int)new SnesAddress(checked((byte)bank), checked((ushort)pointer)));
            AssertEqual(expectedShotNoOps.Contains(address),
                classifyShot((byte)bank, (ushort)pointer),
                "bank-qualified shot identity");
            AssertEqual(expectedTouchNoOps.Contains(address),
                EnemyInteractionCallbacks.IsLiteralNoOpTouch((byte)bank, (ushort)pointer),
                "bank-qualified touch identity");
        }
        var shortcut = new Func<ushort, bool>(pointer =>
            EnemyCollision.IsCanonicalMultiboxNoOpAi(EnemyInteractionCallbacks.TryDecode(0xa7, pointer)));
        AssertTrue(!shortcut(0x94b5) && shortcut(0x804b) && shortcut(0x804c), "Kraid private RTL is not a canonical multibox shortcut");
        Suite(nameof(VerifyLiteralNoOpCombatAdmission), () => VerifyLiteralNoOpCombatAdmission(rom, expectedTouchNoOps, expectedShotNoOps));
        Console.WriteLine(
            "Combat callback inventory: 163 headers, 221 lists, 309 hitboxes, 71 touch " +
            "and 80 shot callbacks match; all bank/pointer pairs and twenty-seven real admission " +
            "paths preserve fifteen touch/twelve shot RTL identities without executable reads.");
    }

    /// <summary>Checks that real touch and projectile collision paths accept literal RTL callbacks without opcode reads.</summary>
    private static void VerifyLiteralNoOpCombatAdmission(
        SuperMetroidAddressSpace rom,
        HashSet<int> touchNoOps,
        HashSet<int> shotNoOps)
    {
        var forbidden = touchNoOps.Concat(shotNoOps).ToHashSet();

        foreach (int callback in touchNoOps)
        {
            var guard = NewEnemyCallbackOpcodeReadGuard(rom, forbidden);
            (RoomEnemySystem enemies, RoomEnemySlot enemy) =
                CreateLiteralNoOpCombatSystem();
            byte touchBank = SnesLittleEndianConstants.BankByte(callback);
            ushort touchWord = unchecked((ushort)callback);
            enemy.Bind(enemy.DefinitionId, default(RoomEnemyDefinition) with
            {
                Touch = EnemyInteractionCallbacks.TryDecode(touchBank, touchWord),
                TouchBody = EnemyInteractionCallbacks.Body(touchWord, EnemyInteractionCallbacks.IsLiteralNoOpTouch(touchBank, touchWord)),
            });
            var samus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose,
                Health = 99,
                Position = FixedPosition<RoomPixels>.FromWhole(enemy.Position.Whole),
            };

            AssertTrue(enemies.Collision.ResolveOrdinarySamusContact(samus, 0),
                $"literal touch no-op ${callback:X6} is admitted by production collision");
            AssertEqual(99, samus.Health,
                $"literal touch no-op ${callback:X6} does not damage Samus");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"literal touch no-op ${callback:X6} requires no executable read");
        }

        foreach (int callback in shotNoOps)
        {
            var guard = NewEnemyCallbackOpcodeReadGuard(rom, forbidden);
            (RoomEnemySystem enemies, RoomEnemySlot enemy) =
                CreateLiteralNoOpCombatSystem();
            byte shotBank = SnesLittleEndianConstants.BankByte(callback);
            ushort shotWord = unchecked((ushort)callback);
            enemy.Bind(enemy.DefinitionId, default(RoomEnemyDefinition) with
            {
                Shot = EnemyInteractionCallbacks.TryDecode(shotBank, shotWord),
                ShotBody = EnemyInteractionCallbacks.Body(shotWord, EnemyInteractionCallbacks.IsLiteralNoOpShot(shotBank, shotWord)),
                NoOpCallbackBody = EnemyInteractionCallbacks.NoOpCallbackBody(shotBank),
            });
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
            SamusProjectileSlot projectile = projectiles.Slots[0];
            projectile.Type = SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false);
            projectile.Damage = 20;
            projectile.Direction = SamusProjectileDirection.Right;
            projectile.Position = projectile.Position.WithWhole(enemy.Position.Whole);
            projectile.Radius = new(4, 4);
            projectile.Cursor = ProjectileFixtureCursors.Active;
            projectile.InstructionTimer = WrappingCountdown.Start(1);

            AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(
                    projectiles,
                    new SamusBombProjectileSystem()),
                $"literal shot no-op ${callback:X6} is admitted by production collision");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"literal shot no-op ${callback:X6} requires no executable read");
        }

        static CreateLiteralNoOpCombatSystemResult CreateLiteralNoOpCombatSystem()
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var interactive = (List<RoomEnemySlot>)enemies.ProcessingWindow.InteractiveEnemies;
            RoomEnemySlot enemy = enemies.SlotTable.Slots[0];
            interactive.Add(enemy);
            enemy.Bind((EnemyDefinitionId)0x1234, enemy.Definition);
            enemy.Frame = EnemyFrameId.SkulteraSwimLeft0;
            enemy.Position = enemy.Position.WithWhole(new(0x0100, 0x0100));
            enemy.Radius = new(8, 8);
            enemy.Health = 100;
            return new CreateLiteralNoOpCombatSystemResult(enemies, enemy);
        }
    }

    /// <summary>Forbids cartridge reads at callback addresses identified as literal RTL routines.</summary>
    private static CartridgeReadGuard NewEnemyCallbackOpcodeReadGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Combat admission probed executable callback ${address:X6}.");
}
