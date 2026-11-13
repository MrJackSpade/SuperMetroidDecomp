using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Waver;

/// <summary>Checks compiled Waver animation selectors in both handoff and initialization paths.</summary>
internal static class WaverAnimationDefinitionsVerification
{
    /// <summary>Compares four native pointers and confirms timer reset without rereading their table.</summary>
    internal static void VerifyWaverAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            WaverProgram selector = WaverNativeWords.Animations.Decode(index, "Waver animation index");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa386db + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(WaverNativeListAddresses.Entries, selector),
                $"Waver animation selector {selector}");
        }

        for (int index = 0; index < 4; index++)
        {
            WaverProgram selector = WaverNativeWords.Animations.Decode(index, "Waver animation index");
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                FrameHold = WrappingCountdown.Start(0x2222),
                Timer = WrappingCountdown.Start(0x3333),
            };
            var state = new WaverEnemyState
            {
                InstalledAnimation = selector == WaverProgram.SteadyFacingLeft
                    ? WaverProgram.SteadyFacingRight
                    : WaverProgram.SteadyFacingLeft,
                RequestedAnimation = selector,
            };
            WaverBehavior.SetWaverProgram(slot, state);
            AssertEqual(selector, state.InstalledAnimation,
                $"production Waver current selector {selector}");
            AssertEqual(new AnimationPlayback<WaverProgram>(WaverAnimations.StartingWith(selector), AnimationPlace.StartOfAnimation),
                state.Playback,
                $"production Waver animation {selector}");
            AssertEqual(1, slot.FrameHold.RawValue,
                $"production Waver instruction timer {selector}");
            AssertEqual(0, slot.Timer.RawValue,
                $"production Waver loop timer {selector}");
        }

        for (ushort parameter = 0; parameter < 2; parameter++)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Waver, slot.Definition);
            slot.SetSpawnParameter1(EnemyDefinitionId.Waver, parameter);
            WaverBehavior.InitializeWaver(slot);
            WaverProgram expected = WaverNativeWords.Animations.Decode(parameter, "Waver spawn facing");
            WaverEnemyState waver = enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0].");
            AssertEqual(expected, waver.RequestedAnimation,
                $"production Waver initial requested selector {parameter}");
            AssertEqual(expected, waver.InstalledAnimation,
                $"production Waver initial current selector {parameter}");
            AssertEqual(new AnimationPlayback<WaverProgram>(WaverAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation),
                waver.Playback,
                $"production Waver initial animation {parameter}");
        }

        AssertEqual(false, WaverNativeWords.Animations.TryDecode(4, out _), "Waver animation index past table");
        VerifyWaverAnimationRequests();
        Console.WriteLine(
            "Waver animation definitions: four native selectors, all four real handoffs, and both production initial facing paths and the named animation requests pass.");
    }

    /// <summary>
    /// The requests main AI makes keep the cartridge's bit arithmetic on its animation index: a wall toggles the facing
    /// bit and clears the spin bit, a spin sets the spin bit, and a finished spin clears it.
    /// </summary>
    private static void VerifyWaverAnimationRequests()
    {
        for (int index = 0; index < 4; index++)
        {
            WaverProgram animation = WaverNativeWords.Animations.Decode(index, "Waver animation index");
            AssertEqual(WaverNativeWords.Animations.Decode((index ^ 1) & 1, "Waver animation index"),
                WaverPrograms.TurnedAround(animation), $"Waver {animation} turned around by a wall");
            AssertEqual(WaverNativeWords.Animations.Decode(index | 2, "Waver animation index"),
                WaverPrograms.Spinning(animation), $"Waver {animation} spinning");
            AssertEqual(WaverNativeWords.Animations.Decode(index & 1, "Waver animation index"),
                WaverPrograms.SteadyWithSameFacing(animation), $"Waver {animation} after a spin");
        }
    }
}
