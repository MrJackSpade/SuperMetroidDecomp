using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyGrappleGreenGateVisibility()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (var trigger in new[] { DownwardGateTriggerBehavior.GreenLeft, DownwardGateTriggerBehavior.GreenRight })
        {
            var (bus, level, streamer, plms, gateIndex) = CreateDownwardGateFixture(trigger);
            // Use the cartridge's actual gate sprite maps for the observable OAM check.
            SeedRetailGateSpriteMaps(bus, retail);
            SeedEmptyRoomEnemyPopulation(bus, 0x9600);
            var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
            enemies.Load(bus, 0x9600, 0, new SnesVram(), new SnesCgram(), () => 0, level: level, samus: new SamusState());
            enemies.ApplyDownwardGateProjectileRequest(plms.TakeDownwardGateProjectileRequests().Single(), level.Size.X);
            enemies.StepEnemyProjectiles(level, null);
            enemies.StepEnemyProjectiles(level, null);
            var oam = new OamBuffer();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            AssertTrue(oam.NextByteOffset > 0, "closed gate has visible OAM before grapple impact");
            byte[] expected = oam.LowTable[..oam.NextByteOffset].ToArray();
            int hitIndex = gateIndex + (trigger == DownwardGateTriggerBehavior.GreenLeft ? -1 : 1);
            var samus = new SamusState
            {
                Position = new(new((ushort)(hitIndex % level.Size.X * 16 + 8), 0), new((ushort)(hitIndex / level.Size.X * 16 + 8), 0)),
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
                    AssertEqual(GrapplePhase.CancelPending, result.Phase, "green gate rejects grapple attachment");
                }
                StepDownwardGatePlm(plms, bus, level, streamer);
                AssertEqual(0, plms.TakeDownwardGateProjectileRequests().Count, "rejected grapple does not wake or replace gate actor");
                enemies.StepEnemyProjectiles(level, null);
                enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
                AssertTrue(oam.LowTable[..oam.NextByteOffset].SequenceEqual(expected),
                    $"green gate OAM remains visible and identical on {trigger} frame {frame}");
                if (frame == 2)
                    AssertTrue(plms.SoundRequests.Any(r => r.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2, DownwardGatePlmRomData.RejectedShotSound)), "rejected grapple emits gate dud sound");
            }
        }
        Console.WriteLine("  Grapple green gates: both sides retain exact gate OAM before, during and after impact.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple green gate visibility")]
    private static byte GrappleGreenGateVisibilityRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Copies the retail bank-$8D sprite maps into the gate fixture for the observable OAM check.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture copying retail bank $8D sprite maps for green-gate visibility")]
    private static void SeedRetailGateSpriteMaps(TestAddressSpace bus, ISnesAddressSpace retail)
    {
        var maps = new byte[0x8000];
        for (int i = 0; i < maps.Length; i++) maps[i] = GrappleGreenGateVisibilityRomByte(retail, 0x8d8000 + i);
        bus.WriteBytes(0x8d8000, maps);
    }
}
