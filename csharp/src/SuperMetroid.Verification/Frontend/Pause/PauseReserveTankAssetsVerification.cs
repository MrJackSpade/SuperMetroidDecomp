using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveFramesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks reserve tank visuals across supply, flicker and restore.</summary>
internal static class PauseReserveTankAssetsVerification
{
    /// <summary>Compares native menu OAM and pixels with installed reserve art.</summary>
    internal static void VerifyPauseReserveTankAssets(ISnesAddressSpace bus, AreaMapPresentation catalog)
    {
        var guard = NewReserveTankAssetReadGuard(bus);
        Suite(nameof(VerifyPauseReserveAnchors), () => VerifyPauseReserveAnchors(bus));
        int comparisons = 0;
        Suite(nameof(VerifyPauseReserveNativeFrames), () => VerifyPauseReserveNativeFrames(bus, InstalledFixtures.ImportOf(catalog).PauseReserveTanks));
        foreach (ushort capacity in new ushort[] { 0, 100, 200, 300, 400 })
        {
            var nativeSamus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { MaxReserveEnergy = capacity, ReserveTankMode = ReserveTankMode.Manual };
            var installedSamus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { MaxReserveEnergy = capacity, ReserveTankMode = ReserveTankMode.Manual };
            var native = Create(bus, nativeSamus, catalog);
            var installed = Create(guard, installedSamus, catalog);
            foreach (ushort supply in ReserveSupplySamples(capacity))
            foreach (byte phase in new byte[] { 0, 4 })
            {
                nativeSamus.ReserveEnergy = installedSamus.ReserveEnergy = supply;
                native.Step(0, 0, frameCycle: phase); installed.Step(0, 0, frameCycle: phase);
                var expected = native.CaptureRenderSnapshot(); var actual = installed.CaptureRenderSnapshot();
                AssertTrue(expected.SpriteParts().SequenceEqual(actual.SpriteParts()), "every legal reserve supply/flicker state matches native ordered OAM with reads forbidden");
                AssertTrue(ScenePixels.Same(expected, actual), "every legal reserve supply/flicker state matches native pixels");
                if (capacity == 100 && supply == 1 && phase == 4)
                {
                    // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
                    var restored = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(installed); restored.BindFixtureArtwork(catalog);
                    AssertTrue(actual.SpriteParts().SequenceEqual(restored.CaptureRenderSnapshot().SpriteParts()), "restored nonzero fill flicker phase retains exact OAM");
                }
                comparisons++;
            }
        }
        Console.WriteLine($"Reserve presentation: 180 native composition/capacity cases and {comparisons} fill-step boundary supply/flicker menu frames pass.");
        // $82:B2AA draws full tanks from supply / 100, then one partial tank from the remainder's
        // 14-unit fill step, flickering when the remainder is between steps. Each tank's empty,
        // step-start, step-start+1 and step-end supplies cover every distinct drawing case.
        static IEnumerable<ushort> ReserveSupplySamples(ushort capacity)
        {
            var samples = new SortedSet<ushort>();
            for (int tank = 0; tank * PauseReserveEnergyDefinitions.EnergyPerTank <= capacity; tank++)
            {
                int baseSupply = tank * PauseReserveEnergyDefinitions.EnergyPerTank;
                samples.Add((ushort)baseSupply);
                for (int step = 0; step * PauseReserveEnergyDefinitions.EnergyPerFillStep < PauseReserveEnergyDefinitions.EnergyPerTank; step++)
                {
                    int start = step * PauseReserveEnergyDefinitions.EnergyPerFillStep;
                    int end = Math.Min(start + PauseReserveEnergyDefinitions.EnergyPerFillStep, PauseReserveEnergyDefinitions.EnergyPerTank) - 1;
                    foreach (int remainder in new[] { start, start + 1, end })
                        if (remainder is > 0 and < PauseReserveEnergyDefinitions.EnergyPerTank && baseSupply + remainder <= capacity)
                            samples.Add((ushort)(baseSupply + remainder));
                }
            }
            return samples;
        }
        static PauseMenuState Create(ISnesAddressSpace source, SamusState state, AreaMapPresentation content)
        {
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = state, System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = content, SpriteSheets = MenuSheetFixtures.For(content) });
            EnterPauseEquipment(pause);
            return pause;
        }
    }
    /// <summary>Blocks native reserve anchor and spritemap reads after installed artwork is bound.</summary>
    private static CartridgeReadGuard NewReserveTankAssetReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Add(int address, int count) { for (int i = 0; i < count; i++) blocked.Add(address + i); }
        Add(0x82c1d6, 14); Add(0x82b3d9, 32);
        foreach (ushort id in new ushort[] { 0x1b, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27 })
        {
            Add(0x82c569 + id * 2, 2);
            int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c569 + id * 2);
            Add(pointer, 2 + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), pointer) * 5);
        }
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read reserve visual ROM at {address:X6}.");
    }
}
