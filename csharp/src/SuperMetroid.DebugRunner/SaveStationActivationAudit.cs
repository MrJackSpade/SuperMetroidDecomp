using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Reproduces #308 through the resident save PLM's production collision setup.</summary>
internal static class SaveStationActivationAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int checks = 0;
        foreach (byte pose in new[] { SamusPoseIds.FacingRightNormalPose, SamusPoseIds.FacingLeftNormalPose })
        {
            foreach (int offset in Enumerable.Range(4, 28).Concat(Enumerable.Range(-8, 12)))
            {
                // Fresh population for every case prevents a previous trigger/lockout from
                // accidentally making the next rejected-position assertion pass.
                var runtime = new SuperMetroidRuntime(bus);
                runtime.InitializeHud(HudSnapshot.CeresDebug);
                runtime.RunNmi(0, true);
                runtime.InitializeStartingCeresRoom();
                runtime.InitializeCeresStartSamus();
                runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CrateriaSaveStation, 0, 0);
                var station = runtime.Plms.Stations.Single(value => value.Kind == StationKind.Save);
                var samus = runtime.Samus!;
                int left = station.BlockIndex % runtime.LevelData!.WidthInBlocks * 16;
                samus.Pose = pose;
                samus.RefreshCollisionRadii(bus);
                samus.XPosition = checked((ushort)(left + offset));
                if (offset == 4)
                {
                    // The reported foot-edge overlap must reproduce through the actual
                    // downward grounding scan, not just an artificial trigger call.
                    samus.YPosition = checked((ushort)(station.BlockIndex / runtime.LevelData.WidthInBlocks * 16 - samus.Kinematics.YRadius));
                    SamusBlockCollision.MoveVertical(bus, runtime.LevelData, samus.Kinematics,
                        1 << 16, scanLeftToRight: true, plms: runtime.Plms);
                    if (runtime.Plms.Stations.Single(value => value.Kind == StationKind.Save).Triggered)
                        throw new InvalidDataException("Foot-edge floor probe incorrectly activated the save station at offset +4.");
                }
                if (!runtime.Plms.TryNotifyStationCollision(station.BlockIndex,
                    (byte)StationAccessBehavior.SaveFloor, pose, horizontal: false, movingPositive: true,
                    roomWidthInBlocks: runtime.LevelData.WidthInBlocks))
                    throw new InvalidDataException("Save station lost its collision owner.");
                // Independent literal bounds from $84:B590: ((SamusX - 8) >> 4) == PLM block X.
                bool expected = offset >= 8 && offset <= 23;
                bool actual = runtime.Plms.Stations.Single(value => value.Kind == StationKind.Save).Triggered;
                if (actual != expected)
                    throw new InvalidDataException($"Save activation offset={offset}, pose=${pose:X2}: expected={expected}, actual={actual}.");
                checks++;
            }
        }
        Console.WriteLine($"PASS: {checks} save activation boundary checks in both standing directions.");
        return 0;
    }
}
