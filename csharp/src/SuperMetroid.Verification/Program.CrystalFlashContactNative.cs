using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCrystalFlashContactNative(string rom, string path)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        AssertEqual("A9E43DA426DD832D9DFBBCF820A374CDA109A2E715764E5012B6AF9174CCB5F3",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))),
            "accepted native contact lifetime trace");
        using var trace = File.OpenText(path);
        AssertEqual("suit,left,offset,frame,phase,pose,anim,timer,y,health,missiles,supers,pbs,immunity,knockback",
            trace.ReadLine(), "native contact schema");
        int compared = 0;
        for (int suit = 0; suit < 3; suit++)
        for (int left = 0; left < 2; left++)
        for (int offset = 0; offset < 8; offset++)
        {
            var samus = new SamusState
            {
                Pose = left != 0 ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose,
                XPosition = 128, YPosition = 128, Health = 49, MaxHealth = 1499,
                Missiles = 10, SuperMissiles = 10, PowerBombs = 10,
                EquippedItems = (ushort)(suit == 1 ? SamusEquipmentFlags.VariaSuit :
                    suit == 2 ? SamusEquipmentFlags.GravitySuit : 0),
            };
            samus.RefreshCollisionRadii(bus);
            ushort input = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
            AssertTrue(samus.CrystalFlash.TryBegin(bus, samus, input), "contact lifetime activation");
            var level = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[8192]);
            int frame = 0;
            do
            {
                if (frame == 30)
                {
                    // Native deliberately invokes normal touch before movement.
                    // Managed enters it through the production collision dispatcher
                    // with an overlapping authored Ripper map; no direct HP write.
                    var enemies = new RoomEnemySystem();
                    enemies.Load(new CrystalFlashContactPopulation(bus, samus.XPosition, samus.YPosition),
                        CrystalFlashContactDefinitions.Pointer, CrystalFlashContactDefinitions.Pointer,
                        new SnesVram(), new SnesCgram(), () => 1);
                    var enemy = enemies.Slots[0];
                    // Build the production interactive index list without advancing AI
                    // or causing a contact before the intentionally admitted boundary.
                    enemies.StepFrame(0, 0, timeIsFrozen: true);
                    int address = CrystalFlashContactDefinitions.AiBank | (enemy.CurrentInstruction + 2);
                    enemy.SpritemapPointer = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                    ushort healthBeforeContact = samus.Health;
                    AssertTrue(enemies.ResolveOrdinarySamusContact(samus, 0, level), "controlled contact reaches native-style touch");
                    AssertEqual(healthBeforeContact - (suit == 0 ? 5 : suit == 1 ? 2 : 1), samus.Health,
                        "contact applies suit-reduced damage before refill");
                    AssertEqual(96, samus.InvincibilityTimer, "contact publishes immunity before movement clears it");
                    AssertEqual(5, samus.KnockbackTimer, "contact publishes pending knockback before movement clears it");
                }
                samus.CrystalFlash.Step(bus, samus, (ushort)(frame + offset));
                samus.AnimateNoFx(bus, input);
                if (samus.PendingTransitionalPose is not null)
                    samus.ApplyPendingVerifiedAnimationTransition(bus);
                string actual = $"{suit},{left},{offset},{frame},{(int)samus.CrystalFlash.Phase},{samus.Pose:X4},{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.YPosition:X4},{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4}";
                AssertEqual(trace.ReadLine(), actual, "native contact followed by movement/animation");
                compared++;
                if (++frame > 400) throw new InvalidDataException("Contact interrupted Crystal Flash completion.");
            } while (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive);
        }
        AssertTrue(trace.ReadLine() is null, "native contact trace entirely consumed");
        Console.WriteLine($"Crystal Flash contact: {compared} frames match across three suits, both facings and eight NMI phases.");
    }
}
