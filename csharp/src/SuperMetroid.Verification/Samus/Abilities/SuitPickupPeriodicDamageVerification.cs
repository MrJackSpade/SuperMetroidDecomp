using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Confirms periodic damage is deferred during the suit pickup transformation (#1337).</summary>
internal static class SuitPickupPeriodicDamageVerification
{
    /// <summary>
    /// Samus command <c>$15</c> (<c>$90:F310</c>, via <c>$90:F2FC</c>) installs the empty beta
    /// <c>RTL_90E8CD</c> for the whole suit transformation, so <c>HandlePeriodicDamageToSamus</c>
    /// (<c>$90:E9CE</c>) does not run until the HDMA sequence restores the normal handlers.
    /// Periodic damage accumulated meanwhile stays in <c>$0A4E.$0A50</c> and is applied by
    /// the first ordinary beta afterwards, divided by the newly equipped suit (#1337).
    /// </summary>
    /// <remarks>
    /// The producer adds a quarter unit (the heat rate) between gameplay frames, the seam
    /// where post-beta producers such as the PLM instruction <c>$84:AC9D</c> accumulate.
    /// </remarks>
    internal static void VerifySuitPickupPeriodicDamage()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(400, 350));

        SamusState samus = runtime.Player.IntroducedSamus;
        samus.MaxHealth = 299;
        samus.Health = 299;
        samus.SubunitHealth = 0;
        // `$84:88F3` equips the suit before the message box and the pickup routine.
        samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
        samus.LiquidPhysics.PeriodicSubDamage = 0;
        samus.LiquidPhysics.PeriodicDamage = 0;
        runtime.SuitPickup.Begin(bus, samus, runtime.Room.LoadedCamera.PixelPosition, SamusSuitPickupKind.Varia);

        // Reference ledger: the same producer calls, consumed once by the $90:E9CE port
        // after the transformation.
        var reference = new SamusState
        {
            MaxHealth = samus.MaxHealth,
            Health = samus.Health,
            SubunitHealth = samus.SubunitHealth,
            EquippedItems = samus.EquippedItems,
        };
        int pickupFrames = 0;
        while (runtime.SuitPickup.IsActive)
        {
            AssertTrue(pickupFrames < 4096, "the suit transformation finishes");
            samus.LiquidPhysics.AccumulatePeriodicDamage(PaletteFxHeatData.SubdamagePerFrame, wholeDamage: 0);
            reference.LiquidPhysics.AccumulatePeriodicDamage(PaletteFxHeatData.SubdamagePerFrame, wholeDamage: 0);
            ushort healthBefore = samus.Health;
            ushort subunitBefore = samus.SubunitHealth;
            runtime.StepFrame(0);
            if (!runtime.SuitPickup.IsActive)
                break;
            pickupFrames++;
            AssertEqual(healthBefore, samus.Health,
                $"the empty suit beta skips $90:E9CE on pickup frame {pickupFrames}");
            AssertEqual(subunitBefore, samus.SubunitHealth,
                $"no sub-unit damage lands on pickup frame {pickupFrames}");
            AssertEqual(reference.LiquidPhysics.PeriodicSubDamage, samus.LiquidPhysics.PeriodicSubDamage,
                $"fractional periodic damage stays pending on pickup frame {pickupFrames}");
            AssertEqual(reference.LiquidPhysics.PeriodicDamage, samus.LiquidPhysics.PeriodicDamage,
                $"whole periodic damage stays pending on pickup frame {pickupFrames}");
        }

        AssertTrue(pickupFrames > 0, "the transformation spans gameplay frames");
        AssertTrue(reference.LiquidPhysics.PeriodicDamage > 0, "the pickup accumulated whole units of damage");
        // The frame whose HDMA pass restores the normal handlers runs ordinary beta, which
        // applies everything pending at once, halved for the now-equipped Varia Suit.
        reference.LiquidPhysics.ApplyPeriodicDamage(reference, timeIsFrozen: false);
        AssertTrue(reference.Health < samus.MaxHealth, "the deferred damage costs energy");
        AssertEqual(reference.Health, samus.Health,
            "the first ordinary beta applies all pickup-time damage, halved by Varia");
        AssertEqual(reference.SubunitHealth, samus.SubunitHealth,
            "the first ordinary beta applies the halved fractional damage");
        AssertEqual((ushort)0, samus.LiquidPhysics.PeriodicSubDamage, "$90:E9CE clears the fractional word");
        AssertEqual((ushort)0, samus.LiquidPhysics.PeriodicDamage, "$90:E9CE clears the whole word");
        Console.WriteLine($"  Suit pickup periodic damage: {pickupFrames} pickup frames keep energy; the deferred damage lands afterwards.");
    }

    /// <summary>Runs the suit pickup periodic damage suite.</summary>
    [VerificationCommand("--suit-pickup-periodic-damage")]
    internal static int SuitPickupPeriodicDamageCommand()
    {
        Suite(nameof(VerifySuitPickupPeriodicDamage), () => VerifySuitPickupPeriodicDamage());
        return 0;
    }
}
