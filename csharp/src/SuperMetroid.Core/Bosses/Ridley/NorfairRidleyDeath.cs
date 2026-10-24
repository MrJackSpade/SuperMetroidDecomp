using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Norfair Ridley's death: the roar, breakup, explosions and the drop.</summary>
[RoomLifetime]
internal sealed class NorfairRidleyDeath
{
    /// <summary>Enemy dispatcher used for Ridley's registered native callbacks.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Room services, level state, and native RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Handles common combat damage and enemy drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    private readonly EnemyRoomFx _roomFx;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyDeath(IEnemyDispatch dispatch, EnemyRoomBinding room, EnemyDeathsAndDrops deathsAndDrops, RidleyEncounter ridleyEncounter, RidleyProjectiles ridleyProjectiles, EnemyRoomFx roomFx)
    {
        _dispatch = dispatch;
        _room = room;
        _deathsAndDrops = deathsAndDrops;
        _ridleyEncounter = ridleyEncounter;
        _ridleyProjectiles = ridleyProjectiles;
        _roomFx = roomFx;
    }

    /// <summary>Releases Samus's script lock and grants movement-dependent intangibility outside death.</summary>
    internal static void ReleaseNorfairRidleyGrab(RidleyEnemyState state, SamusState? samus)
    {
        state.GrabState = RidleyGrabState.Released;
        samus?.Control.Unlock();
        state.TailWhipRequest = 1;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        // $A6:BC8F-BC93 leaves the timer untouched once death owns the fight.
        if (state.FightMode != RidleyFightMode.Dying)
        {
            SamusMovementType movement = samus is null ? SamusMovementType.Standing : SamusPoseMetadata.ReadMovementType(samus);
            state.IntangibilityTimer = WrappingCountdown.FromRaw(RidleySamusInteractionDefinitions.ReleaseIntangibilityFrames(movement));
        }
    }

    /// <summary>Ports the breakup explosion actor's initialization AI.</summary>
    internal void RunRidleyExplosionInitializationAi(RoomEnemySlot slot) =>
        InitializeNorfairRidleyExplosion(
            slot,
            _ridleyEncounter.Body,
            _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
                "A Ridley breakup actor has no shared Ridley owner."));

    /// <summary>Last item-drop request published by Ridley's terminal routine.</summary>
    public bool RidleyDeathDropRequested { get; private set; }

    /// <summary>
    /// Ports $A6:C551. Ridley continues converging on the death point throughout the roar;
    /// expiry freezes velocity and starts lowering the room acid before entering the long
    /// small-explosion phase in the same dispatcher call.
    /// </summary>
    internal void TickNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisorIndex, RidleyInertiaDefinitions.DeathReversalBoost);
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        state.Velocity = new EightEightVector<RoomPixels>(0, 0);
        RidleyLiquidMotion.Publish(_roomFx, state, RidleyLiquidRomData.DrainedHeight,
            RidleyLiquidRomData.DrainVelocity, RidleyLiquidRomData.DrainDelay);
        state.DeathExplosionTimer = WrappingCountdown.Start(0);
        state.DeathExplosionCount = 0;
        state.Function = RidleyAiFunction.NorfairDeathFall;
        state.FunctionTimer = WrappingCountdown.Start(160);
        TickNorfairRidleyDeathExplosions(body, state, samus: null);
    }

    /// <summary>
    /// Ports $A6:C588. A ROM-positioned small explosion appears every five calls while the
    /// acid lowers. When the 160-frame clock expires, Samus is released and Ridley's twelve
    /// independently moving body/tail breakup enemies are allocated from the real slot pool.
    /// </summary>
    internal void TickNorfairRidleyDeathExplosions(
        RoomEnemySlot body,
        RidleyEnemyState state,
        SamusState? samus)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        if (state.GrabState != RidleyGrabState.Released)
            ReleaseNorfairRidleyGrab(state, samus);
        state.Function = RidleyAiFunction.NorfairDeathImpact;
        state.Velocity = new EightEightVector<RoomPixels>(0, 0);
        SpawnNorfairRidleyBreakupActors(state);
    }

    /// <summary>
    /// $A6:C5A8 performs one final small-explosion tick, hides the intact body, and disables
    /// shared wing/tail movement so only the breakup actors remain visible.
    /// </summary>
    internal void BeginNorfairRidleyBreakup(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        state.MovementAnimationEnabled = false;
        body.Properties = body.Properties.With(EnemyProperties.Invisible);
        state.Function = RidleyAiFunction.NorfairDeathWait;
        state.FunctionTimer = WrappingCountdown.Start(32);
    }

    /// <summary>$A6:C5C8's deliberate 32-frame pre-wait before the final 256 frames.</summary>
    internal static void TickNorfairRidleyBreakupWait(RidleyEnemyState state)
    {
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;
        state.Function = RidleyAiFunction.NorfairDeathFinish;
        state.FunctionTimer = WrappingCountdown.Start(256);
    }

    /// <summary>
    /// Ports $A6:C5DA. Boss persistence is published before the slot is deleted, exactly
    /// when the retail item drop and delayed music-track-three request occur.
    /// </summary>
    internal void TickNorfairRidleyDeathFinish(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
        state.BossDefeatPublished = true;

        // $A0:B8AC scatters sixteen pickups through Ridley's authored 128x64 arena
        // rectangle. The body is deleted immediately afterward, so the drop actors must
        // retain header $E17F themselves.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Ridley,
            count: 16,
            xBase: 64,
            xMask: 127,
            yBase: 320,
            yMask: SnesLittleEndian.Word(0, 63));
        RidleyDeathDropRequested = true;
        state.MusicRequest = MusicRequest.SelectTrack(3);
        body.Properties = body.Properties.With(EnemyProperties.Deleted);
        state.Function = RidleyAiFunction.NorfairDeathComplete;
    }

    /// <summary>Ports $A6:C623 and its ten signed X/Y offset pairs at $A6:C66E.</summary>
    internal void SpawnSmallExplosionNearNorfairRidley(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (!state.DeathExplosionTimer.IsZero)
        {
            state.DeathExplosionTimer = state.DeathExplosionTimer.Decrement().Remaining;
            return;
        }

        state.DeathExplosionTimer = WrappingCountdown.Start(4);
        state.DeathExplosionCount++;
        if (state.DeathExplosionCount >= 10)
            state.DeathExplosionCount = 0;

        _ridleyProjectiles.SpawnRidleyDust(
            body.Position.Whole + RidleyExplosionDefinitions.DeathExplosionPlacement(state.DeathExplosionCount),
            variant: 3);
        state.LastDeathSoundEffect = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, RidleySoundDefinitions.DeathExplosion);
    }

    /// <summary>Allocates the body and tail pieces that continue moving after the intact body hides.</summary>
    internal void SpawnNorfairRidleyBreakupActors(RidleyEnemyState state)
    {
        if (state.DeathBreakupSpawned)
            return;
        state.DeathBreakupSpawned = true;

        RidleyExplosionDefinitions.SpawnInNativeOrder(SpawnPart);

        void SpawnPart(ushort parameter)
        {
            // The embedded records at $A6:C987-$CA46 all use the same zero position,
            // property $2C00, and one even parameter. Initialization replaces position and
            // instruction from the still-live shared Ridley joints below.
            RoomEnemyPopulationRecord population = new(
                EnemyDefinitionId.RidleyExplosion,
                Position: Position16<RoomPixels>.Zero,
                InitializationParameter: 0,
                Properties: new EnemyPropertyWord(
                    EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision),
                ExtraProperties: new EnemyExtraPropertyWord(EnemyExtraProperties.None),
                Parameter1: parameter,
                Parameter2: 0);
            // The explosion's initialization AI ($A6:C696) starts each part from the still-live shared Ridley joints.
            if (_dispatch.SpawnEnemy(population) is null)
            {
                throw new InvalidOperationException(
                    "Lower Norfair Ridley breakup exhausted the 32-slot enemy pool.");
            }
        }
    }

    /// <summary>Ports initialization AI $A6:C696 for one parameter $00..$16.</summary>
    internal void InitializeNorfairRidleyExplosion(
        RoomEnemySlot fragment,
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        RidleyExplosionPart part = ClosedNativeWords.Decode<RidleyExplosionPart>(
            checked((ushort)fragment.Spawn.Population.Parameter1), "Ridley explosion part");

        fragment.InstructionTimer = WrappingCountdown.Start(1);
        fragment.Timer = WrappingCountdown.Start(0);
        fragment.VramTilesIndex = EnemyTileBaseDefinitions.Fixed[EnemyDefinitionId.RidleyExplosion];
        fragment.PaletteIndex = EnemyPaletteBits.Palette7;
        int random = _room.Random.Generate();
        ushort horizontalMagnitude = unchecked((ushort)(random & RidleyExplosionDefinitions.FragmentSpeedMask));
        var fragmentState = new RidleyExplosionFragmentState
        {
            Lifetime = WrappingCountdown.FromRaw(RidleyExplosionDefinitions.GetPart(checked((ushort)fragment.Spawn.Population.Parameter1)).Lifetime),
            Velocity = new(new SnesSignedEightEight(unchecked((short)random) < 0
                ? unchecked((ushort)-horizontalMagnitude)
                : horizontalMagnitude), default),
        };
        fragment.SetSpeciesState(fragmentState, fragmentState.ClearWithRecord);

        if (part <= RidleyExplosionPart.TailTip)
        {
            int tailIndex = (int)part >> 1;
            RidleyTailSegment tail = state.TailSegments[tailIndex];
            fragment.Position = fragment.Position.WithWhole(tail.Position);
            int tailTipOrientation =
                unchecked((byte)(unchecked((byte)tail.Angle) +
                    unchecked((byte)state.TailSegments[5].Angle) + 8)) / 16;
            StartRidleyBreakupProgram(state, fragment, RidleyExplosionDefinitions.SelectTailFrame(
                part,
                tailTipOrientation));
            return;
        }

        RidleyExplosionBodyPartDefinition bodyPart =
            RidleyExplosionDefinitions.SelectBodyPart(
                part,
                facingRight: state.FacingDirection != RidleyFacing.Left);
        fragment.Position = fragment.Position.WithWhole(body.Position.Whole + bodyPart.Offset);
        StartRidleyBreakupProgram(state, fragment, bodyPart.Frame);
    }

    /// <summary>Starts a breakup fragment's typed program for <paramref name="frame"/>.</summary>
    private static void StartRidleyBreakupProgram(RidleyEnemyState state, RoomEnemySlot fragment, RidleyBreakupFrame frame) =>
        state.BreakupPrograms[fragment.SlotIndex] = AnimationProgramCursor<RidleyBreakupFrame>.Start(frame);

    /// <summary>Ports main AI $A6:C8D4: flicker, drag, gravity, movement, and lifetime.</summary>
    internal void RunNorfairRidleyExplosionMain(RoomEnemySlot fragment)
    {
        if ((fragment.FrameCounter & 1) != 0)
            fragment.Properties = fragment.Properties.With(EnemyProperties.Invisible);
        else
            fragment.Properties = fragment.Properties.Without(EnemyProperties.Invisible);
        fragment.FrameCounter &= 1;

        RidleyExplosionFragmentState fragmentState = fragment.SpeciesState<RidleyExplosionFragmentState>()
            ?? throw new InvalidOperationException($"Ridley breakup fragment in slot {fragment.SlotIndex} has no state.");
        int horizontal = fragmentState.Velocity.X.SignedRawValue;
        int magnitude = Math.Max(0, Math.Abs(horizontal) - 4);
        var velocity = new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(unchecked((ushort)(horizontal < 0 ? -magnitude : magnitude))),
            new SnesSignedEightEight(unchecked((ushort)(fragmentState.Velocity.Y.RawValue + 4))));
        fragmentState.Velocity = velocity;
        fragment.Position = fragment.Position.Advance(new(velocity.X.ToSixteenSixteenDelta(), velocity.Y.ToSixteenSixteenDelta()));

        CountdownStep lifetime = fragmentState.Lifetime.Decrement();
        fragmentState.Lifetime = lifetime.Remaining;
        if (lifetime.IsNonNegative)
            return;

        // $A6:C90B calls EnemyDeathAnimation(0): clear the slot immediately and
        // retain the fragment header/position in the shared explosion-to-pickup actor.
        _deathsAndDrops.StartGenericEnemyDeath(fragment, (ushort)EnemyDeathAnimation.SmallExplosion);
    }
}
