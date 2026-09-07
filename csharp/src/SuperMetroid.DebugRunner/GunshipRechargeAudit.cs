using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Exercises the ship's real refill function, including energy overflow into reserves.</summary>
internal static class GunshipRechargeAudit
{
    public static int Run(string romPath)
    {
        foreach (ushort initialMode in new ushort[] { 0, 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 0, 0);
            var samus = runtime.Samus!;
            samus.InputLocked = true;
            samus.Health = 96;
            samus.MaxHealth = 99;
            samus.ReserveEnergy = 0;
            samus.MaxReserveEnergy = 5;
            samus.ReserveTankMode = initialMode;
            samus.Missiles = 0;
            samus.MaxMissiles = 5;
            samus.SuperMissiles = 0;
            samus.MaxSuperMissiles = 3;
            samus.PowerBombs = 0;
            samus.MaxPowerBombs = 1;
            var top = runtime.Enemies.Slots[0];
            top.VariableF = GunshipCodePointers.BeginLiftoffOrRestoreSamus;
            // Isolate the enemy owner from movement, but execute its production dispatcher.
            for (int frame = 1; frame <= 4; frame++)
            {
                runtime.Enemies.StepFrame(1024, 1024, false, samus);
                int expectedHealth = Math.Min(99, 96 + frame * 2);
                int expectedReserve = Math.Max(0, 96 + frame * 2 - 99);
                if (samus.Health != expectedHealth || samus.ReserveEnergy != expectedReserve ||
                    samus.Missiles != Math.Min(5, frame * 2) ||
                    samus.SuperMissiles != Math.Min(3, frame * 2) || samus.PowerBombs != 1 ||
                    samus.ReserveTankMode != (expectedReserve > 0 && initialMode == 0 ? 1 : initialMode))
                    throw new InvalidDataException($"Ship refill frame {frame}, mode {initialMode}: energy {samus.Health}/{samus.ReserveEnergy}; expected {expectedHealth}/{expectedReserve}.");
                if (runtime.Enemies.GunshipSavePromptPending != (frame == 4))
                    throw new InvalidDataException("Ship did not wait for exactly the complete energy/ammo refill before requesting save.");
            }
        }
        Console.WriteLine("Gunship refill: exact per-frame health/reserve/ammo, auto-mode initialization and save readiness pass for all reserve modes.");
        return 0;
    }
}
