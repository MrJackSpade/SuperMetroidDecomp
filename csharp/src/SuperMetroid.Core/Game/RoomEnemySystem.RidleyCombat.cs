using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Lower Norfair Ridley's private seams around the shared bank-$A0 collision handlers.
/// Body rectangles still come from the current ROM extended spritemap; this file owns only
/// the state changes that bank $A6 performs before or after common damage.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private static void PrepareNorfairRidleyCombatFrame(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        UpdateNorfairRidleyIntangibility(body, state);
        if (state.PowerBombReactionLatched == 0)
            return;

        state.PowerBombReactionLatched--;
        if (state.FightMode > 0 && state.FightMode != 2 && state.GrabState == 0)
        {
            // $A6:BD2C interrupts the current action while a live power-bomb field is
            // present. Function $B84D resets both tail control words before the lunge.
            state.TailFunctionIndex = 1;
            state.FeetDistanceIndex = 1;
            state.Function = RidleyAiFunction.NorfairGrabApproach;
        }
    }

    /// <summary>
    /// Ports hurt AI $A6:B297. Function, movement, wing, and tail updates execute only on
    /// even actor frames; tail armor, flash/palette work, and the grabbed-Samus lag counter
    /// remain active on every hurt frame.
    /// </summary>
    private void RunNorfairRidleyHurt(
        RoomEnemySlot body,
        SamusState? samus,
        ushort controllerInput,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles)
    {
        RidleyEnemyState state = RequireNorfairRidley(body);
        if ((body.FrameCounter & 1) == 0)
        {
            PrepareNorfairRidleyCombatFrame(body, state);
            RunNorfairRidleyFunction(body, state, samus, controllerInput, level);
            if (state.MovementAnimationEnabled != 0)
            {
                IntegrateRidleyMovement(body, state);
                TickRidleyWingAnimation(state);
                TickRidleyTail(body, state, samus);
            }
        }

        if (samusProjectiles is not null && state.MovementAnimationEnabled != 0)
            ResolveRidleyTailProjectileHits(body, state, samusProjectiles);
        UpdateRidleyHurtFlashPalettes(body, state, body.FrameCounter);
        UpdateNorfairRidleyHealthPalette(body, state);

        state.HurtMovementClamp = Math.Min(
            (ushort)8,
            unchecked((ushort)(state.HurtMovementClamp + 1)));
        if (state.GrabState != 0 && state.HurtMovementClamp >= 8 && samus is not null)
            UpdateNorfairRidleyGrabbedSamus(body, state, samus);
    }

    /// <summary>
    /// Completes <c>EnemyShot_Ridley</c> at $A6:DF8A after common no-death-check damage.
    /// The real boss deliberately remains a live actor at zero health. His action selector
    /// then chooses only lunges until he catches Samus and begins the authored death scene.
    /// </summary>
    private void ResolveNorfairRidleyShotAfterCommon(RoomEnemySlot body)
    {
        RidleyEnemyState state = RequireNorfairRidley(body);

        // The collision pass runs after EnemyMain, while the native hurt palette is drawn
        // from the frame that just ran. Publish that same state immediately for standalone
        // consumers which inspect CGRAM before the next enemy dispatcher call.
        UpdateRidleyHurtFlashPalettes(
            body,
            state,
            unchecked((ushort)(body.FrameCounter - 1)));
        UpdateNorfairRidleyHealthPalette(body, state);
    }

    /// <summary>
    /// Completes <c>PowerBombReaction_Ridley</c> at $A6:DFB2. Common damage installs the
    /// 48-frame invincibility/flash state; the live power-bomb flag also interrupts a normal
    /// fight action with Ridley's lunge on the following enemy frame.
    /// </summary>
    private void ResolveNorfairRidleyPowerBombAfterCommon(RoomEnemySlot body)
    {
        RidleyEnemyState state = RequireNorfairRidley(body);
        state.PowerBombReactionLatched = 2;
        ResolveNorfairRidleyShotAfterCommon(body);
    }

    /// <summary>
    /// Ports the collision-relevant portion of <c>HandleRidleySamusInteractionBit</c> at
    /// $A6:BCB4. A grabbed or off-screen body is tangible; after release the authored short
    /// grace timer expires before the extended body rejoins bank-$A0 collision lists.
    /// </summary>
    private static void UpdateNorfairRidleyIntangibility(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        // Fight mode $FFFF is the death latch. Native never clears tangible again once that
        // sign bit is set, which prevents stray shots or contact from restarting the scene.
        if (unchecked((short)state.FightMode) < 0)
            return;

        if (state.GrabState != 0)
        {
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            return;
        }

        if (state.IntangibilityTimer != 0)
        {
            state.IntangibilityTimer--;
            if (state.IntangibilityTimer != 0)
                return;
        }

        if (state.FightMode != 0)
            body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>
    /// Ports $A6:E088. Only the final two solved tail joints are live in retail code: a
    /// radius-14 tip followed by a radius-10 inner segment. A hit is snapped to the joint,
    /// marked consumed, and replaced by the cartridge's dust/dud actor; Ridley's health is
    /// never consulted because the tail is armor rather than another body hitbox.
    /// </summary>
    private void ResolveRidleyTailProjectileHits(
        RoomEnemySlot body,
        RidleyEnemyState state,
        SamusProjectileSystem projectiles)
    {
        if (body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            state.TailSegments.Length != 7)
        {
            return;
        }

        if (TryBlockProjectileAtRidleyTailJoint(
                state.TailSegments[6],
                radius: 14,
                projectiles))
        {
            return;
        }

        _ = TryBlockProjectileAtRidleyTailJoint(
            state.TailSegments[5],
            radius: 10,
            projectiles);
    }

    private bool TryBlockProjectileAtRidleyTailJoint(
        RidleyTailSegment joint,
        ushort radius,
        SamusProjectileSystem projectiles)
    {
        // CheckForProjectileCollisionWithRectangle scans the five ordinary native slots in
        // ascending order. Its signed-type gate admits active beam/missile/super records and
        // rejects bomb, power-bomb, and already-converted impact families.
        foreach (SamusProjectileSlot projectile in projectiles.Slots)
        {
            int typeNibble = (projectile.Type >> 8) & 0x0f;
            if (!projectile.IsActive ||
                unchecked((short)projectile.Type) >= 0 ||
                typeNibble >= 3)
            {
                continue;
            }

            int xDistance = Math.Abs(unchecked((short)(projectile.XPosition - joint.XPosition)));
            int yDistance = Math.Abs(unchecked((short)(projectile.YPosition - joint.YPosition)));
            if (xDistance >= projectile.XRadius + radius ||
                yDistance >= projectile.YRadius + radius)
            {
                continue;
            }

            projectile.XPosition = joint.XPosition;
            projectile.YPosition = joint.YPosition;
            projectile.Direction = unchecked((ushort)(projectile.Direction | 0x0010));

            // Missile family one gets the tiny dud and library-one sound $3D. Beams and
            // supers use the larger smoke variant $0C, exactly as $A6:E126 selects it.
            ushort dustVariant = typeNibble == 1 ? (ushort)6 : (ushort)12;
            SpawnRidleyDust(joint.XPosition, joint.YPosition, dustVariant);
            if (typeNibble == 1)
                LastEnemyProjectileDudSoundEffect = 0x003d;
            return true;
        }

        return false;
    }

    /// <summary>Allocates enemy projectile $E509 with one ROM-authored dust variant.</summary>
    private void SpawnRidleyDust(ushort x, ushort y, ushort variant)
    {
        RoomEnemyProjectileSlot? dust = AllocateEnemyProjectile();
        if (dust is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            dust,
            RoomEnemyProjectileKind.MiscDustExplosion,
            graphicsIndex: 0);
        dust.XPosition = x;
        dust.YPosition = y;
        dust.InstructionPointer = ReadWord(
            _bus!,
            0x86e42c + Math.Min(variant, (ushort)0x001d) * 2);
        dust.InstructionTimer = 1;
    }
}
