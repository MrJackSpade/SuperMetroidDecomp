using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // Expected values come from native-chainsaw-fire-probe, not from another C# path.
    // This remains separately invokable for focused diagnostics and also runs in the
    // default suite now that the cartridge-backed firing/lifetime slice is implemented.
    private static void VerifyChainsawFiring()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(16, 16, new ushort[256], new byte[256],
            new ushort[256], new byte[8]);
        var samus = new SamusState
        {
            Pose = 1,
            XPosition = 128,
            YPosition = 128,
            EquippedBeams = 0x000d,
            SelectedHudItem = 0,
        };
        var shared = new SamusBombProjectileSystem();
        var projectiles = new SamusProjectileSystem();
        var result = projectiles.StepFrame(bus, level, samus,
            (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, shared);

        // A missing shot also leaves an empty slot, so checking deletion alone would
        // incorrectly pass the existing silent >=12 rejection in HandleBeamInput.
        AssertEqual((int?)0, result.FiredSlot, "native Chainsaw admits and allocates the shot");
        var spawn = projectiles.LastFiredProjectileSnapshot
            ?? throw new InvalidOperationException("Chainsaw firing has no producer snapshot.");
        AssertEqual(139, spawn.XPosition, "native Chainsaw muzzle X before its callback");
        AssertEqual(123, spawn.YPosition, "native Chainsaw muzzle Y before its callback");
        AssertEqual(-64, spawn.XVelocity, "native Chainsaw initial speed overread");
        AssertEqual(0, spawn.YVelocity, "native Chainsaw initial vertical speed");
        AssertEqual(0, projectiles.ProjectileCounter, "inactive Power Bomb deletes Chainsaw on first update");
        AssertEqual(0, projectiles.Slots[0].InstructionPointer, "deleted Chainsaw has no animation list");
        AssertEqual(0, projectiles.Slots[0].Damage, "deleted Chainsaw releases its damage sentinel");
        AssertEqual((byte)0x34, projectiles.ChainsawWindowRegisters.ReadByte(
            GameplayWindowRegisterAddresses.Window12Selection), "first callback stores inherited Y low byte");
        AssertEqual((byte)0x00, projectiles.ChainsawWindowRegisters.ReadByte(
            GameplayWindowRegisterAddresses.Window34Selection), "first callback stores inherited Y high byte");

        var activeShared = new SamusBombProjectileSystem();
        activeShared.PowerBombExplosion.Arm();
        var activeProjectiles = new SamusProjectileSystem();
        ushort[] nextLists = [0x902f, 0x9037, 0x903f, 0x9047, 0x904f, 0x9057, 0x905f, 0x9067];
        ushort[] spritemaps = [0xaf4c, 0xaf62, 0xaf78, 0xafa2, 0xafcc, 0xaff6, 0xb020, 0xb04a];
        ushort[] yRadii = [12, 12, 16, 16, 20, 20, 23, 23];
        ushort[] inheritedY = [0x0034, 0x9027, 0x902f, 0x9037, 0x903f, 0x9047, 0x904f, 0x9057];
        for (int update = 0; update < nextLists.Length; update++)
        {
            SamusProjectileFrameResult frame = activeProjectiles.StepFrame(
                bus,
                level,
                samus,
                update == 0 ? (ushort)SnesButton.X : (ushort)0,
                update == 0 ? (ushort)SnesButton.X : (ushort)0,
                0,
                0,
                activeShared);
            if (update == 0)
                AssertEqual((int?)0, frame.FiredSlot, "active-Power-Bomb Chainsaw allocation");
            SamusProjectileSlot slot = activeProjectiles.Slots[0];
            AssertEqual(nextLists[update], slot.InstructionPointer, $"active Chainsaw next list {update}");
            AssertEqual(spritemaps[update], slot.SpritemapPointer, $"active Chainsaw spritemap {update}");
            AssertEqual((ushort)8, slot.XRadius, $"active Chainsaw X radius {update}");
            AssertEqual(yRadii[update], slot.YRadius, $"active Chainsaw Y radius {update}");
            AssertEqual((byte)inheritedY[update], activeProjectiles.ChainsawWindowRegisters.ReadByte(
                GameplayWindowRegisterAddresses.Window12Selection), $"active Chainsaw inherited Y low {update}");
            AssertEqual((byte)(inheritedY[update] >> 8), activeProjectiles.ChainsawWindowRegisters.ReadByte(
                GameplayWindowRegisterAddresses.Window34Selection), $"active Chainsaw inherited Y high {update}");
        }

        Console.WriteLine("Chainsaw firing: native admission, muzzle, velocity, callback store and Power-Bomb-gated lifetime agree.");
    }
}
