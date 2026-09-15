using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>Real chase admission and D-pad escape around controller-earned spark activation.</summary>
internal sealed class DraygonGrabBlueSuitProbe(ISnesAddressSpace bus, SamusState samus, bool left, int mode)
{
    private ushort _ownerX;
    private static int GrabFrame(int mode) => 150 + mode % 4;

    public static ushort InputAt(int frame, bool left, int mode)
    {
        ushort forward = left ? (ushort)0x200 : (ushort)0x100;
        if (frame < 140) return (ushort)(0x8000 | forward);
        if (frame == 140) return 0x410;
        if (frame < 150) return mode >= 4 ? (ushort)0 : (ushort)0x10;
        if (frame == 150) return 0x80;
        if (frame < 160) return 0x880;
        if (frame < 220) return (frame & 1) != 0 ? (ushort)0x100 : (ushort)0x200;
        if (frame >= 340 && frame < 350) return forward;
        if (frame >= 360 && frame < 370) return (ushort)(0x8000 | forward);
        return 0;
    }

    public void BeforeFrame(int frame)
    {
        if (frame > GrabFrame(mode) && frame < 220 && samus.DraygonGrabbed.IsActive)
            samus.DraygonGrabbed.ApplyOwnerPosition(samus, _ownerX, 400, !left);
    }

    public void Verify(int frame)
    {
        int releaseFrame = mode % 4 == 0 ? 218 : 219;
        if (frame == releaseFrame && (samus.DraygonGrabbed.IsActive ||
            samus.DraygonGrabbed.EscapeButtonCounter != SamusDraygonGrabbedState.EscapeButtonCounterTarget))
            throw new InvalidDataException("The native D-pad escape boundary was not reproduced.");
        bool retainsBlueSuit = mode >= 5;
        if (frame == 330 && samus.HorizontalSpeed.SpeedBoostCounter != (retainsBlueSuit ? 0x400 : 0))
            throw new InvalidDataException("Grab timing or retained-momentum exception changed persistent Blue Suit.");
        if (frame == 349 && samus.HorizontalSpeed.ContactDamageIndex != (retainsBlueSuit ? 1 : 0))
            throw new InvalidDataException("Walking after grab escape did not publish native boost contact damage.");
        if (frame == 361 && samus.HorizontalSpeed.SpeedBoostCounter != 1)
            throw new InvalidDataException("Dash did not replace retained boost with a fresh running counter.");
    }

    public void AfterFrame(int frame)
    {
        if (frame != GrabFrame(mode)) return;
        var room = CartridgeRoomHeader.Load(bus, RoomHeaderPointers.Draygon);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x4937);
        var enemies = new RoomEnemySystem();
        enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
            vram, cgram, random.NextRandom, random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber, level: assets.LevelData, samus: samus,
            isAreaBossDefeated: () => false, setAreaBossDefeated: () => { });
        var state = enemies.Draygon ?? throw new InvalidDataException("Missing Draygon.");
        state.Function = DraygonAiFunction.TryGrabSamus;
        state.FacingRight = !left;
        var body = state.Body;
        _ownerX = body.XPosition = unchecked((ushort)(samus.XPosition + (left ? 8 : -8)));
        body.YPosition = samus.YPosition;
        body.InstructionTimer = 100;
        // Model the attached-goop admission word only at the callback boundary;
        // all charge, velocity and spark state comes from the real preceding inputs.
        samus.XSpeedDivisor = 1;
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData,
            nmiFrameCounter8: unchecked((byte)(frame + 2)));
        if (!samus.DraygonGrabbed.IsActive || state.SuccessfulGrabs != 1)
            throw new InvalidDataException("Constructed claw window did not invoke the real grab callback.");
        samus.XSpeedDivisor = 0;
    }
}
