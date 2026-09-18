using System.Reflection;
using System.Text.RegularExpressions;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShotCallbackDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var shotCallbacks = new HashSet<int>();
        var touchCallbacks = new HashSet<int>();
        int headers = 0, lists = 0, boxes = 0;
        foreach (string line in File.ReadLines("upstream-disassembly/src/bank_A0.asm"))
        {
            Match match = Regex.Match(line, @"^EnemyHeaders_\w+:\s*;([0-9A-F]{6});");
            if (!match.Success) continue;
            int address = Convert.ToInt32(match.Groups[1].Value, 16);
            int bank = rom.ReadByte(address + 12) << 16;
            touchCallbacks.Add(bank | Word(address + 48));
            shotCallbacks.Add(bank | Word(address + 50));
            headers++;
        }
        foreach (string file in Directory.EnumerateFiles("upstream-disassembly/src", "bank_*.asm"))
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
                int count = Word(address);
                AssertTrue(count < 100, "native hitbox inventory count sanity");
                lists++;
                for (int box = 0; box < count; box++)
                {
                    int hitbox = address + 2 + box * 12;
                    touchCallbacks.Add((address & 0xff0000) | Word(hitbox + 8));
                    shotCallbacks.Add((address & 0xff0000) | Word(hitbox + 10));
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
            .Where(address => (address & 65535) != 0 && rom.ReadByte(address) == 0x6b)
            .ToHashSet();
        var expectedTouchNoOps = touchCallbacks
            .Where(address => (address & 65535) != 0 && rom.ReadByte(address) == 0x6b)
            .ToHashSet();
        AssertEqual(12, expectedShotNoOps.Count, "native literal shot no-op count");
        AssertEqual(15, expectedTouchNoOps.Count, "native literal touch no-op count");
        var classifyShot = typeof(RoomEnemySystem).GetMethod("IsLiteralNoOpEnemyAi", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<byte, ushort, bool>>();
        foreach (int address in shotCallbacks)
            AssertEqual(expectedShotNoOps.Contains(address), classifyShot((byte)(address >> 16), (ushort)address), "real shot classifier native parity");
        for (int bank = 0; bank <= 255; bank++)
        for (int pointer = 0; pointer <= 65535; pointer++)
        {
            int address = (bank << 16) | pointer;
            AssertEqual(expectedShotNoOps.Contains(address),
                classifyShot((byte)bank, (ushort)pointer),
                "bank-qualified shot identity");
            AssertEqual(expectedTouchNoOps.Contains(address),
                EnemyTouchCallbackDefinitions.IsLiteralNoOp((byte)bank, (ushort)pointer),
                "bank-qualified touch identity");
        }
        var shortcut = typeof(RoomEnemySystem).GetMethod("IsCanonicalMultiboxNoOpAi", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<ushort, bool>>();
        AssertTrue(!shortcut(0x94b5) && shortcut(0x804b) && shortcut(0x804c), "Kraid private RTL is not a canonical multibox shortcut");
        VerifyLiteralNoOpCombatAdmission(rom, expectedTouchNoOps, expectedShotNoOps);
        Console.WriteLine(
            "Combat callback inventory: 163 headers, 221 lists, 309 hitboxes, 71 touch " +
            "and 80 shot callbacks match; all bank/pointer pairs and twenty-seven real admission " +
            "paths preserve fifteen touch/twelve shot RTL identities without executable reads.");
    }

    private static void VerifyLiteralNoOpCombatAdmission(
        SuperMetroidAddressSpace rom,
        HashSet<int> touchNoOps,
        HashSet<int> shotNoOps)
    {
        var forbidden = touchNoOps.Concat(shotNoOps).ToHashSet();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        foreach (int callback in touchNoOps)
        {
            var guard = new EnemyCallbackOpcodeReadGuard(rom, forbidden);
            (RoomEnemySystem enemies, RoomEnemySlot enemy) =
                CreateLiteralNoOpCombatSystem(guard);
            enemy.Definition = default(RoomEnemyDefinition) with
            {
                Bank = (byte)(callback >> 16),
                TouchAiPointer = unchecked((ushort)callback),
            };
            var samus = new SamusState
            {
                Pose = SamusPoseIds.FacingRightNormalPose,
                Health = 99,
                XPosition = enemy.XPosition,
                YPosition = enemy.YPosition,
            };

            AssertTrue(enemies.ResolveOrdinarySamusContact(samus, 0),
                $"literal touch no-op ${callback:X6} is admitted by production collision");
            AssertEqual((ushort)99, samus.Health,
                $"literal touch no-op ${callback:X6} does not damage Samus");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"literal touch no-op ${callback:X6} requires no executable read");
        }

        foreach (int callback in shotNoOps)
        {
            var guard = new EnemyCallbackOpcodeReadGuard(rom, forbidden);
            (RoomEnemySystem enemies, RoomEnemySlot enemy) =
                CreateLiteralNoOpCombatSystem(guard);
            enemy.Definition = default(RoomEnemyDefinition) with
            {
                Bank = (byte)(callback >> 16),
                ShotAiPointer = unchecked((ushort)callback),
            };
            var projectiles = new SamusProjectileSystem();
            SamusProjectileSlot projectile = projectiles.Slots[0];
            projectile.Type = 0x8000;
            projectile.Damage = 20;
            projectile.Direction = 2;
            projectile.XPosition = enemy.XPosition;
            projectile.YPosition = enemy.YPosition;
            projectile.XRadius = projectile.YRadius = 4;
            projectile.InstructionPointer = 0x9000;
            projectile.InstructionTimer = 1;

            AssertEqual(1, enemies.ResolveOrdinaryProjectileHits(
                    guard,
                    projectiles,
                    new SamusBombProjectileSystem()),
                $"literal shot no-op ${callback:X6} is admitted by production collision");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"literal shot no-op ${callback:X6} requires no executable read");
        }

        static (RoomEnemySystem System, RoomEnemySlot Enemy) CreateLiteralNoOpCombatSystem(
            ISnesAddressSpace bus)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            var interactive = (List<ushort>)typeof(RoomEnemySystem).GetField(
                "_interactiveEnemyIndexes",
                flags)!.GetValue(enemies)!;
            RoomEnemySlot enemy = enemies.Slots[0];
            interactive.Add(enemy.NativeIndex);
            enemy.EnemyDefinitionPointer = 0x1234;
            enemy.SpritemapPointer = 1;
            enemy.XPosition = enemy.YPosition = 0x0100;
            enemy.XRadius = enemy.YRadius = 8;
            enemy.Health = 100;
            return (enemies, enemy);
        }
    }

    private sealed class EnemyCallbackOpcodeReadGuard(
        ISnesAddressSpace source,
        HashSet<int> forbidden) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Combat admission probed executable callback ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
