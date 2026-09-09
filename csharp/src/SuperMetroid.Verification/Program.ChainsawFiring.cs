using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // Intentionally isolated while #396 is incomplete. Expected values come from
    // native-chainsaw-fire-probe, not from the current C# projectile implementation.
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
        Console.WriteLine("Chainsaw firing: native admission, muzzle, velocity and first-update lifetime agree.");
    }
}
