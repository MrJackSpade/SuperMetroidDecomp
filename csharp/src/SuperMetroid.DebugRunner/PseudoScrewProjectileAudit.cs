using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Generic enemy-projectile contact parity, separate from enemy-touch consumption.</summary>
internal static class PseudoScrewProjectileAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "F273FEBAD2950CCC5BDE3CFC091F13B170F4D45EF7E9CAD15CBCBA22FBAE1F6B")
            throw new InvalidDataException("Use the accepted pseudo-projectile v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 288 || rows.Any(row => row.Length != 13))
            throw new InvalidDataException("Incomplete projectile-contact matrix.");
        var bus = new ProjectileFixture(SuperMetroidAddressSpace.LoadRetailRom(rom));
        int failures = 0, protectedCases = 0, hurtCases = 0;
        foreach (var row in rows)
        {
            ushort contact = ushort.Parse(row[0]);
            bool inv = row[1] == "1", persist = row[2] == "1", disabled = row[3] == "1";
            int offset = int.Parse(row[4]);
            ushort radius = ushort.Parse(row[5], NumberStyles.HexNumber);
            var samus = new SamusState { XPosition = 128, YPosition = 128, Health = 99, ProjectileFlareCounter = 120 };
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 12;
            samus.InvincibilityTimer = inv ? (ushort)9 : (ushort)0;
            samus.HorizontalSpeed.ContactDamageIndex = contact;
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
            var projectile = enemies.EnemyProjectiles[17];
            projectile.Kind = (RoomEnemyProjectileKind)0xf000;
            projectile.XPosition = (ushort)(128 + offset); projectile.YPosition = 128;
            projectile.XRadius = (byte)radius; projectile.YRadius = (ushort)(radius >> 8);
            projectile.Damage = 40; projectile.InvincibilityFrames = 96;
            projectile.CanDamageSamus = !disabled; projectile.PersistsOnSamusContact = persist;
            projectile.InstructionPointer = 0xf200; projectile.InstructionTimer = 7;
            enemies.ResolveEnemyProjectileSamusHits(samus);
            string actual = $"{samus.Health:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4},{samus.ProjectileFlareCounter:X4}," +
                $"{(ushort)projectile.Kind:X4},{projectile.InstructionPointer:X4},{projectile.InstructionTimer:X4}";
            if (actual != string.Join(',', row[6..]))
            {
                failures++;
                Console.WriteLine($"Projectile contact {string.Join(',', row[..6])}: {actual} != {string.Join(',', row[6..])}");
            }
            if (contact == 4)
            {
                protectedCases++;
                if (samus.Health != 99 || samus.ProjectileFlareCounter != 120 || !projectile.IsActive)
                    throw new InvalidDataException("Pseudo Screw must skip generic projectile contact without consuming charge.");
            }
            if (samus.Health < 99) hurtCases++;
        }
        if (protectedCases != 96 || hurtCases != 2)
            throw new InvalidDataException($"Changed projectile protection controls: {protectedCases}/{hurtCases}.");
        Console.WriteLine($"Pseudo Screw projectile: 288 cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private sealed class ProjectileFixture(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address switch
        {
            0xa1f000 or 0xa1f001 => 0xff,
            0x86f00a => 0,
            0x86f00b => 0xf1,
            _ => inner.ReadByte(address)
        };
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
