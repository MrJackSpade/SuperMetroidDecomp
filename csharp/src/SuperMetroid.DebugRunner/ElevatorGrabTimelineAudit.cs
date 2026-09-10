using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Runtime previous-frame contact handoff against native input/actor/movement timelines.</summary>
internal static class ElevatorGrabTimelineAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "45C13B8344FAF161FA34AC3FC8E185235758118EA726EA5A1F6648A02CE3B466")
            throw new InvalidDataException("Use the accepted elevator timeline v3 capture.");
        var rows=File.ReadLines(capture).Skip(1).Select(line=>line.Split(',')).ToArray();
        if (rows.Length != 110312 || rows.Any(row => row.Length != 16))
            throw new InvalidDataException("Incomplete elevator timeline matrix.");
        int cases=0, mismatches=0, compared=0;
        foreach(var group in rows.GroupBy(row=>string.Join(',',row[..6])))
        {
            var seed=group.First();
            int up=int.Parse(seed[0]), parity=int.Parse(seed[1]), left=int.Parse(seed[2]);
            int scenario=int.Parse(seed[3]), start=int.Parse(seed[4]), delay=int.Parse(seed[5]);
            if (((((up * 2 + parity) * 2 + left) * 3 + scenario) * 32 + start - 120) * 7 + delay - 7 != cases)
                throw new InvalidDataException("Reordered elevator timeline cases.");
            var retail=SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime=FlatFloorMovementFixture.Create(retail,false);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GreenBrinstarMainShaft,0,144);
            runtime.Plms.Reset();
            if(parity!=0) runtime.RunNmi(0,true);
            var level=runtime.LevelData!;
            for(int y=0;y<level.HeightInBlocks;y++) for(int x=0;x<level.WidthInBlocks;x++)
            {
                int index=y*level.WidthInBlocks+x;
                level.SetForegroundEntry(index,y==16?(ushort)0x8000:(ushort)0);
                level.SetBehavior(index,0);
            }
            level.SetForegroundEntry(16*level.WidthInBlocks+8,0x9000);
            level.SetBehavior(16*level.WidthInBlocks+8,9);
            var samus=runtime.Samus!;
            samus.EquippedItems=(ushort)SamusEquipmentFlags.MorphBall; samus.EquippedBeams=0;
            samus.Health=samus.MaxHealth=99; samus.InputLocked=false;
            samus.XPosition=(ushort)start; samus.YPosition=(ushort)(scenario==1?219:235);
            samus.Kinematics.XSubposition=samus.Kinematics.YSubposition=0;
            samus.Pose=left!=0?SamusPoseIds.FacingLeftNormalPose:SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(retail); samus.InitializeAnimation(retail); samus.SetAnimationFrameFromSpecialHandler(0,1);
            samus.PoseHistory.PreviousPose=samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement=(ushort)(left!=0?4:8);
            samus.PoseHistory.LastDifferentPose=samus.PoseHistory.LastDifferentDirectionAndMovement=0;
            runtime.Controller1.Latch(0);
            var bus=new PopulationSelectionAddressSpace(retail,[new RoomEnemyPopulationRecord(0xd73f,136,256,0,0,0,(ushort)up,0)]);
            runtime.Enemies.Load(bus,PopulationSelectionAddressSpace.PopulationPointer,PopulationSelectionAddressSpace.TilesetPointer,
                new SnesVram(),new SnesCgram(),()=>0,samus:samus);
            int frame=0; bool reported=false;
            foreach(var row in group)
            {
                if(int.Parse(row[6])!=frame) throw new InvalidDataException("Reordered elevator timeline.");
                ushort input = 0x10;
                if (scenario == 2 && frame < 8) input |= (ushort)(left != 0 ? 0x200 : 0x100);
                if (frame >= delay && (frame - delay) % 4 == 0) input |= (ushort)(up != 0 ? 0x800 : 0x400);
                if (input != ushort.Parse(row[7], NumberStyles.HexNumber))
                    throw new InvalidDataException("Changed elevator input timeline.");
                frame++;
                try { runtime.StepFrame(input); }
                catch(Exception error) { throw new InvalidOperationException($"Elevator timeline {group.Key}, frame {frame-1}",error); }
                ushort contact=(ushort)(runtime.Enemies.ElevatorFlags | (level.ElevatorDoorContactPending?1:0));
                string actual=$"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X4},{(ushort)samus.ReadMovementType(retail):X4},"+
                    $"{samus.HorizontalSpeed.BaseFixed:X8},{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{runtime.ElevatorStatus:X4},{contact:X4}";
                if(actual!=string.Join(',',row[8..]))
                {
                    mismatches++;
                    if(!reported && cases<40) Console.WriteLine($"GRABTIME {group.Key} frame={frame-1}: {actual} != {string.Join(',',row[8..])}");
                    reported=true;
                }
                compared++;
            }
            if (frame != 24 && runtime.ElevatorStatus == 0)
                throw new InvalidDataException("Elevator timeline ended before activation or its full control window.");
            cases++;
        }
        if (cases != 5376) throw new InvalidDataException("Incomplete elevator timeline cases.");
        Console.WriteLine($"Elevator timelines: {cases} cases, {compared} frames, {mismatches} mismatches.");
        return mismatches==0?0:1;
    }
}
