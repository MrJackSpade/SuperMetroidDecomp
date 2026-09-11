using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGrappleGreenGateVisibility()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (var trigger in new[] { DownwardGateTriggerBehavior.GreenLeft, DownwardGateTriggerBehavior.GreenRight })
        {
            var (bus, level, streamer, plms, gateIndex) = CreateDownwardGateFixture(trigger);
            // Use the cartridge's actual gate sprite maps for the observable OAM check.
            var maps = new byte[0x8000];
            for (int i = 0; i < maps.Length; i++) maps[i] = retail.ReadByte(0x8d8000 + i);
            bus.WriteBytes(0x8d8000, maps);
            bus.WriteBytes(0xa19600, [0xff, 0xff]);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, 0x9600, 0, new SnesVram(), new SnesCgram(), () => 0, level: level, samus: new SamusState());
            enemies.ApplyDownwardGateProjectileRequest(plms.TakeDownwardGateProjectileRequests().Single(), level.WidthInBlocks);
            enemies.StepEnemyProjectiles(level, null);
            enemies.StepEnemyProjectiles(level, null);
            var oam = new OamBuffer();
            enemies.DrawEnemyProjectiles(oam, 0, 0);
            AssertTrue(oam.NextByteOffset > 0, "closed gate has visible OAM before grapple impact");
            byte[] expected = oam.LowTable[..oam.NextByteOffset].ToArray();
            int hitIndex = gateIndex + (trigger == DownwardGateTriggerBehavior.GreenLeft ? -1 : 1);
            var samus = new SamusState
            {
                XPosition = (ushort)(hitIndex % level.WidthInBlocks * 16 + 8),
                YPosition = (ushort)(hitIndex / level.WidthInBlocks * 16 + 8),
            };
            for (int frame = 0; frame < 12; frame++)
            {
                // Runtime clears OAM before alpha. An exception on impact interrupts the
                // draw later in the frame; the opt-in reporter then exposes partial OAM.
                oam.BeginFrame();
                if (frame == 2)
                {
                    SeedStationaryGrappleCollisionProbe(bus, samus);
                    var result = SamusGrappleMovement.StepFiring(bus, level, samus, (ushort)SnesButton.X, plms);
                    AssertTrue(result.CancelQueued, "green gate rejects grapple attachment");
                }
                StepDownwardGatePlm(plms, bus, level, streamer);
                AssertEqual(0, plms.TakeDownwardGateProjectileRequests().Count, "rejected grapple does not wake or replace gate actor");
                enemies.StepEnemyProjectiles(level, null);
                enemies.DrawEnemyProjectiles(oam, 0, 0);
                AssertTrue(oam.LowTable[..oam.NextByteOffset].SequenceEqual(expected),
                    $"green gate OAM remains visible and identical on {trigger} frame {frame}");
                if (frame == 2)
                    AssertTrue(plms.SoundRequests.Any(r => r.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2, DownwardGatePlmRomData.RejectedShotSound)), "rejected grapple emits gate dud sound");
            }
        }
        Console.WriteLine("  Grapple green gates: both sides retain exact gate OAM before, during and after impact.");
    }
}
