using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyPlasmaEnemyPenetration()
    {
        var bus = new TestAddressSpace();
        foreach (bool charged in new[] { false, true })
        for (ushort combination = 0; combination < 8; combination++)
        {
            var shots = new SamusProjectileSystem();
            var slot = shots.Slots[0];
            ushort type = SamusProjectileTypeWord.CreateBeam(
                (ushort)(combination | (ushort)SamusBeamFlags.Plasma), charged);
            slot.Type = type;
            slot.Damage = 100;
            slot.Direction = (ushort)SamusProjectileDirection.Right;
            slot.InstructionPointer = 0x9000;
            slot.InstructionTimer = 7;
            var bombs = new SamusBombProjectileSystem();
            for (int hit = 0; hit < 2; hit++)
            {
                AssertTrue(shots.TryStartEnemyImpact(bus, bombs, 0), "penetrating enemy hit accepted");
                AssertEqual(type, slot.Type, "Plasma preserves live beam type instead of explosion");
                AssertEqual((ushort)100, slot.Damage, "Plasma preserves original damage");
                AssertEqual((ushort)SamusProjectileDirection.Right, slot.Direction, "Plasma does not acquire deletion marker");
                AssertEqual((ushort)0x9000, slot.InstructionPointer, "Plasma retains its animation");
                AssertEqual((ushort)7, slot.InstructionTimer, "Plasma retains animation timing");
            }
        }
    }
}
