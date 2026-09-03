namespace SuperMetroid.Core.Game;

/// <summary>
/// Lower Norfair Ridley's zero-health lunge and death sequence ($A6:DFB7, $C538-$C600).
/// Generic enemy death is intentionally never used: the body remains a live owner until
/// the room acid, twelve breakup actors, item-drop request, music, and saved boss bit have
/// all reached their cartridge-authored points.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Last item-drop request published by Ridley's terminal routine.</summary>
    public bool RidleyDeathDropRequested { get; private set; }

    private static void StartNorfairRidleyDeathSequence(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (body.Health != 0 || unchecked((short)state.FightMode) < 0)
            return;

        state.FightMode = 0xffff;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Function = RidleyAiFunction.NorfairReleaseSamus;
    }

    /// <summary>
    /// $A6:C538 keeps the already-grabbed Samus attached while accelerating Ridley toward
    /// the four-pixel rectangle centered at (128,328). Reaching it immediately tail-calls
    /// the roar setup instead of spending a host-only transition frame.
    /// </summary>
    private void TickNorfairRidleyMoveToDeathSpot(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        MoveNorfairRidleyToward(body, state, 128, 328, divisorIndex: 16);
        if (IsWithinRidleyRectangle(body, 128, 328, 4, 4))
            BeginNorfairRidleyDeathRoar(body, state);
    }

    /// <summary>Ports $A6:C53E: select the death-roar list and arm its 32-frame move.</summary>
    private static void BeginNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        SetRidleyInstruction(body, 0xe6c8);
        state.Function = RidleyAiFunction.NorfairDeathExplosions;
        state.FunctionTimer = 32;
    }

    /// <summary>
    /// Ports $A6:C551. Ridley continues converging on the death point throughout the roar;
    /// expiry freezes velocity and starts lowering the room acid before entering the long
    /// small-explosion phase in the same dispatcher call.
    /// </summary>
    private void TickNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        MoveNorfairRidleyToward(body, state, 128, 328, divisorIndex: 16);
        if (!TickRidleyFunctionTimer(state))
            return;

        state.HorizontalVelocity = 0;
        state.VerticalVelocity = 0;
        state.FxTargetYPosition = 528;
        state.FxYSubVelocity = 64;
        state.FxTimer = 1;
        state.DeathExplosionTimer = 0;
        state.DeathExplosionCount = 0;
        state.Function = RidleyAiFunction.NorfairDeathFall;
        state.FunctionTimer = 160;
        TickNorfairRidleyDeathExplosions(body, state, samus: null);
    }

    /// <summary>
    /// Ports $A6:C588. A ROM-positioned small explosion appears every five calls while the
    /// acid lowers. When the 160-frame clock expires, Samus is released and Ridley's twelve
    /// independently moving body/tail breakup enemies are allocated from the real slot pool.
    /// </summary>
    private void TickNorfairRidleyDeathExplosions(
        RoomEnemySlot body,
        RidleyEnemyState state,
        SamusState? samus)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        if (!TickRidleyFunctionTimer(state))
            return;

        if (state.GrabState != 0)
            ReleaseNorfairRidleyGrab(state, samus);
        state.Function = RidleyAiFunction.NorfairDeathImpact;
        state.HorizontalVelocity = 0;
        state.VerticalVelocity = 0;
        SpawnNorfairRidleyBreakupActors(body, state);
    }

    /// <summary>
    /// $A6:C5A8 performs one final small-explosion tick, hides the intact body, and disables
    /// shared wing/tail movement so only the breakup actors remain visible.
    /// </summary>
    private void BeginNorfairRidleyBreakup(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        state.MovementAnimationEnabled = 0;
        body.Properties = body.Properties.With(EnemyProperties.Invisible);
        state.Function = RidleyAiFunction.NorfairDeathWait;
        state.FunctionTimer = 32;
    }

    /// <summary>$A6:C5C8's deliberate 32-frame pre-wait before the final 256 frames.</summary>
    private static void TickNorfairRidleyBreakupWait(RidleyEnemyState state)
    {
        if (!TickRidleyFunctionTimer(state))
            return;
        state.Function = RidleyAiFunction.NorfairDeathFinish;
        state.FunctionTimer = 256;
    }

    /// <summary>
    /// Ports $A6:C5DA. Boss persistence is published before the slot is deleted, exactly
    /// when the retail item drop and delayed music-track-three request occur.
    /// </summary>
    private void TickNorfairRidleyDeathFinish(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (!TickRidleyFunctionTimer(state))
            return;

        RequireSetAreaBossDefeated();
        state.BossDefeatPublished = true;

        // $A0:B8AC scatters sixteen pickups through Ridley's authored 128x64 arena
        // rectangle. The body is deleted immediately afterward, so the drop actors must
        // retain header $E17F themselves.
        SpawnEnemyDropScatter(
            NorfairRidleyDefinition,
            count: 16,
            xBase: 64,
            xMask: 0x007f,
            yBase: 320,
            yMask: 0x3f00);
        RidleyDeathDropRequested = true;
        state.MusicRequest = MusicCommand.SelectTrack(3);
        body.Properties = body.Properties.With(EnemyProperties.Deleted);
    }

    /// <summary>Ports $A6:C623 and its ten signed X/Y offset pairs at $A6:C66E.</summary>
    private void SpawnSmallExplosionNearNorfairRidley(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (state.DeathExplosionTimer != 0)
        {
            state.DeathExplosionTimer--;
            return;
        }

        state.DeathExplosionTimer = 4;
        state.DeathExplosionCount = unchecked((ushort)(state.DeathExplosionCount + 1));
        if (state.DeathExplosionCount >= 10)
            state.DeathExplosionCount = 0;

        int offsetAddress = 0xa6c66e + state.DeathExplosionCount * 4;
        ushort x = unchecked((ushort)(body.XPosition + unchecked((short)ReadWord(
            _bus!,
            offsetAddress))));
        ushort y = unchecked((ushort)(body.YPosition + unchecked((short)ReadWord(
            _bus!,
            offsetAddress + 2))));
        SpawnRidleyDust(x, y, variant: 3);
        state.LastDeathSoundEffect = 0x0024;
    }
}
