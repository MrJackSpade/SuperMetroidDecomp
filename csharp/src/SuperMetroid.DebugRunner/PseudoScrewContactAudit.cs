using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Actual charged projectile owner passed through generic EnemyMain contact.</summary>
internal static class PseudoScrewContactAudit
{
    public static int Run(string rom)
    {
        int failures = 0;
        foreach (ushort contact in new ushort[] { 3, 4 })
        foreach (byte vulnerability in new byte[] { 0, 1, 2, 0x82 })
        {
            var cartridge = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var bus = new VulnerabilityOverlay(cartridge, vulnerability);
            var runtime = FlatFloorMovementFixture.Create(cartridge, water: false);
            foreach (var slot in runtime.Enemies.Slots) slot.Clear();
            foreach (var slot in runtime.Enemies.EnemyProjectiles) slot.Clear();
            runtime.Plms.Reset();
            var samus = runtime.Samus!;
            samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
            for (int frame = 0; frame < 125; frame++) runtime.StepFrame(0x40);
            if (runtime.Projectiles.FlareCounter < 60)
                throw new InvalidDataException("Fixture failed to build a real beam charge.");
            ushort originalCharge = runtime.Projectiles.FlareCounter;
            ushort previousCharge = runtime.Projectiles.PreviousBeamChargeCounter;
            var flareOam = new OamBuffer();
            runtime.Projectiles.HandleChargeFlareAndDraw(cartridge, flareOam, samus, 0, 0);
            if (flareOam.NextByteOffset == 0)
                throw new InvalidDataException("Charged fixture did not draw a visible flare.");
            for (int color = 0; color < 16; color++) runtime.Cgram.SetColor(192 + color, 0x1234);
            samus.HorizontalSpeed.ContactDamageIndex = contact;
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, 0xf000, 0, runtime.Vram, runtime.Cgram, () => 1);
            var enemy = enemies.Slots[0];
            enemy.EnemyDefinitionPointer = 0xf000;
            enemy.Definition = default(RoomEnemyDefinition) with
            {
                Bank = 0xa3, MainAiPointer = 0x804c,
                TouchAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyTouch,
                VulnerabilityPointer = 0xf000,
            };
            enemy.XPosition = samus.XPosition; enemy.YPosition = samus.YPosition;
            enemy.XRadius = enemy.YRadius = 8;
            enemy.Health = 5000; enemy.SpritemapPointer = 0x8000;
            enemies.StepFrame(0, 0, false, samus, level: runtime.LevelData,
                samusProjectiles: runtime.Projectiles, resolveSamusContactBeforeAi: true);
            int expectedHealth = 5000 - (contact == 3 ? 1000 : 100) * (vulnerability & 0x7f);
            ushort expectedCharge = contact == 4 ? (ushort)0 : originalCharge;
            flareOam.BeginFrame();
            runtime.Projectiles.HandleChargeFlareAndDraw(cartridge, flareOam, samus, 0, 0);
            if ((flareOam.NextByteOffset == 0) != (contact == 4))
                throw new InvalidDataException("Contact failed to preserve/erase the correct charge flare.");
            int palettePointerAddress = SamusProjectileRomData.Palettes.NormalSuitPointers;
            int palettePointer = cartridge.ReadByte(palettePointerAddress) | cartridge.ReadByte(palettePointerAddress + 1) << 8;
            for (int color = 0; color < 16; color++)
            {
                int address = SamusProjectileRomData.Banks.PaletteAndTrailData | (palettePointer + color * 2);
                int expectedColor = contact == 4 ? cartridge.ReadByte(address) | cartridge.ReadByte(address + 1) << 8 : 0x1234;
                if (runtime.Cgram.Colors[192 + color] != expectedColor)
                    throw new InvalidDataException("Contact suit palette differs from native command four.");
            }
            if (enemy.Health != expectedHealth || runtime.Projectiles.FlareCounter != expectedCharge ||
                samus.ProjectileFlareCounter != expectedCharge ||
                runtime.Projectiles.PreviousBeamChargeCounter != previousCharge ||
                (contact == 4 && runtime.Projectiles.SamusChargePaletteIndex != 0))
            {
                failures++;
                Console.WriteLine($"Pseudo touch {contact}/{vulnerability:X2}: health={enemy.Health}, charge={runtime.Projectiles.FlareCounter}, mirror={samus.ProjectileFlareCounter}; expected {expectedHealth}/{expectedCharge}.");
            }
        }
        Console.WriteLine($"Pseudo Screw contact: 8 cases, {failures} failures.");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>Only the synthetic vulnerability bytes differ from the retail address space.</summary>
    private sealed class VulnerabilityOverlay(ISnesAddressSpace inner, byte value) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is 0xa1f000 or 0xa1f001 ? (byte)0xff :
            address is >= 0xb4f000 and < 0xb4f020 ? value : inner.ReadByte(address);
        public void WriteByte(int address, byte data) => inner.WriteByte(address, data);
    }
}
