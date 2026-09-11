using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Original-CPU Yard main comparison: earthquake eligibility and airborne velocity preservation.</summary>
internal static class YardQuakeNativeAudit
{
    public static int Run(string rom, string csv)
    {
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(csv))) != YardQuakeReference.TraceSha256)
            throw new InvalidDataException("Unrecognized native Yard quake trace.");
        string[] rows = File.ReadAllLines(csv);
        if (rows.Length != 289 || rows[0] != "behavior,control,facing,frame,state,function,x,xsub,y,ysub,vx,vxsub,vy,vysub,list,hidden")
            throw new InvalidDataException("Incomplete native Yard quake trace.");
        int row = 1;
        for (ushort behavior = 0; behavior < 6; behavior++)
        for (int control = 0; control < 3; control++)
        for (ushort facing = 0; facing < 2; facing++)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var population = new PopulationSelectionAddressSpace(bus,
                [new RoomEnemyPopulationRecord(RoomEnemySystem.YardDefinition, 128, 128, 0,
                    (ushort)EnemyProperties.ProcessOffScreen, 0, 0, 0)]);
            var enemies = new RoomEnemySystem();
            enemies.Load(population, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, new SnesVram(), new SnesCgram(), () => 1);
            var level = new RoomLevelData(16, 32, new ushort[512], new byte[512], new ushort[512], []);
            var samus = new SamusState { XPosition = 200, YPosition = 400, Pose = SamusPoseIds.FacingRightNormalPose, Health = 999 };
            var actor = enemies.Slots[0];
            var state = enemies.YardStates[0] ?? throw new InvalidDataException("Missing Yard state.");
            actor.XRadius = actor.YRadius = 8;
            actor.CurrentInstruction = 0;
            state.Behavior = behavior;
            state.AirborneFacingDirection = facing;
            state.HidingInstructionList = (ushort)YardMovementFunction.InstructionPending;
            state.MovementFunction = behavior < 3 ? YardMovementFunction.InstructionPending : YardMovementFunction.Airborne;
            state.AirborneXVelocity = state.AirborneYVelocity = 1;
            state.AirborneXSubvelocity = 0x4000;
            state.AirborneYSubvelocity = 0x8000;
            for (int frame = 0; frame < 8; frame++)
            {
                enemies.EarthquakeTimer = frame != 0 ? (ushort)0 : (ushort)(SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration - (control == 1 ? 1 : 0));
                enemies.EarthquakeType = control == 2 ? CrawlerQuakeAuditData.HorizontalQuakeControl : SamusProjectileRomData.NonBeam.SuperMissileEarthquakeType;
                if (frame == 0)
                {
                    // A0's enemy dispatcher and earthquake consumer both stop
                    // during frozen game time. On resume the pending event must
                    // still produce the same original-CPU trace below.
                    var before = Snapshot();
                    for (int frozenFrame = 0; frozenFrame < 5; frozenFrame++)
                    {
                        enemies.StepFrame(0, 0, true, samus, level: level);
                        if (Snapshot() != before)
                            throw new InvalidDataException("Frozen game time advanced Yard motion or consumed its earthquake.");
                    }
                }
                enemies.StepFrame(0, 0, false, samus, level: level);
                string actual = $"{behavior},{control},{facing},{frame},{state.Behavior:X4},{(ushort)state.MovementFunction:X4}," +
                    $"{actor.XPosition:X4},{actor.XSubposition:X4},{actor.YPosition:X4},{actor.YSubposition:X4}," +
                    $"{state.AirborneXVelocity:X4},{state.AirborneXSubvelocity:X4},{state.AirborneYVelocity:X4},{state.AirborneYSubvelocity:X4},{actor.CurrentInstruction:X4},{state.HidingInstructionList:X4}";
                if (actual != rows[row]) throw new InvalidDataException($"Yard CPU mismatch row {row}: expected {rows[row]}, actual {actual}.");
                row++;
            }
            string Snapshot() => $"{state.Behavior},{state.MovementFunction},{actor.XPosition},{actor.XSubposition},{actor.YPosition},{actor.YSubposition}," +
                $"{state.AirborneXVelocity},{state.AirborneXSubvelocity},{state.AirborneYVelocity},{state.AirborneYSubvelocity},{actor.CurrentInstruction},{state.HidingInstructionList},{enemies.EarthquakeTimer},{enemies.EarthquakeType}";
        }
        Console.WriteLine("Yard quake original CPU: 36 setups / 288 frames match eligibility, ignored airborne retriggers, selected lists and exact trajectories; 180 frozen-time frames preserve the pending response.");
        return 0;
    }
}

/// <summary>Reference generated by the headless original-CPU Yard probe.</summary>
internal static class YardQuakeReference
{
    /// <summary>SHA256 of the accepted 288-frame Yard main CSV.</summary>
    public const string TraceSha256 = "FD56C7E6CF84C58FC6913DF14D7D7A6700817AAAE04111657F0FBEC143271581";
}
