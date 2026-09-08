namespace SuperMetroid.Core.Game;

/// <summary>Mother Brain's encounter-specific projectile callbacks from bank <c>$A9</c>.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MotherBrainBodySamusHitboxes = 0xb427;
    private const ushort MotherBrainBrainSamusHitboxes = 0xb439;
    private const ushort MotherBrainNeckSamusHitboxes = 0xb44b;

    /// <summary>
    /// Ports <c>$A9:B3B6-$B454</c>. Mother Brain owns a bit-selected set of asymmetric
    /// rectangles whose side extents are interpreted relative to Samus, rather than an
    /// ordinary enemy radius. Phase one enables only the brain list; later phases also
    /// admit the body and three independently positioned neck segments.
    /// </summary>
    private bool ResolveMotherBrainSamusCollision(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        ushort enabled = state.HitboxesEnabled;
        if ((enabled & 1) != 0 && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainBodySamusHitboxes,
                state.Body.XPosition,
                state.Body.YPosition))
        {
            return true;
        }

        enabled >>= 1;
        RoomEnemySlot head = state.Head!;
        if ((enabled & 1) != 0 && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainBrainSamusHitboxes,
                head.XPosition,
                head.YPosition))
        {
            return true;
        }

        enabled >>= 1;
        if ((enabled & 1) != 0)
        {
            // Native tests only middle joints one, two, and three. Joint zero is buried in
            // the body and joint four is already covered by the separate brain rectangles.
            MotherBrainNeckPoint[] collisionSegments =
                [state.NeckSegment1, state.NeckSegment2, state.NeckSegment3];
            foreach (MotherBrainNeckPoint segment in collisionSegments)
            {
                if (ResolveMotherBrainSamusCollisionPart(
                        state,
                        samus,
                        MotherBrainNeckSamusHitboxes,
                        segment.X,
                        segment.Y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool ResolveMotherBrainSamusCollisionPart(
        MotherBrainEnemyState state,
        SamusState samus,
        ushort hitboxListPointer,
        ushort originX,
        ushort originY)
    {
        int list = 0xa90000 | hitboxListPointer;
        int count = ReadWord(_bus!, list);
        int record = list + 2;
        for (int index = 0; index < count; index++, record += 8)
        {
            bool belowOrigin = unchecked((short)(samus.YPosition - originY)) >= 0;
            int yDistance = Math.Abs(unchecked((short)(samus.YPosition - originY)));
            short yExtent = unchecked((short)ReadWord(
                _bus!,
                record + (belowOrigin ? 6 : 2)));
            int yOverlap = samus.Kinematics.YRadius + Math.Abs((int)yExtent) - yDistance;
            if (yOverlap < 0)
                continue;

            bool rightOfOrigin = unchecked((short)(samus.XPosition - originX)) >= 0;
            int xDistance = Math.Abs(unchecked((short)(samus.XPosition - originX)));
            short xExtent = unchecked((short)ReadWord(
                _bus!,
                record + (rightOfOrigin ? 4 : 0)));
            int xOverlap = samus.Kinematics.XRadius + Math.Abs((int)xExtent) - xDistance;
            if (xOverlap < 0)
                continue;

            // Native clamps shallow contact UP to four pixels. The resulting words are
            // consumed by Samus movement later in the frame, so overwrite rather than add.
            samus.Kinematics.ExtraXDisplacement = unchecked((ushort)Math.Max(xOverlap, 4));
            samus.Kinematics.ExtraYDisplacement = 4;
            samus.Kinematics.ExtraXSubdisplacement = 0;
            samus.Kinematics.ExtraYSubdisplacement = 0;
            samus.InvincibilityTimer = 96;
            samus.KnockbackTimer = 5;
            samus.KnockbackXDirection = 1;
            if (unchecked((short)(samus.YPosition - 192)) < 0)
                samus.Kinematics.YDirection = 2;

            // Only contact to the right of body X+24 calls MotherBrain_HurtSamus. Contact
            // elsewhere still installs the displacement and timers above without damage.
            if (unchecked((short)(state.Body.XPosition + 24 - samus.XPosition)) < 0)
                DamageSamusFromMotherBrainBody(state.Body, samus);
            return true;
        }
        return false;
    }

    private static void DamageSamusFromMotherBrainBody(RoomEnemySlot body, SamusState samus)
    {
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(body.Definition.Damage >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(body.Definition.Damage >> 1))
                : body.Definition.Damage;
        samus.Health = samus.Health <= damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - damage));
        samus.InvincibilityTimer = 96;
        samus.KnockbackTimer = 5;
        samus.KnockbackXDirection = samus.XPosition >= body.XPosition ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Ports the first-form branch of head shot AI <c>$A9:B507</c>. The glass encounter
    /// accepts only missile families; beams pass through without being converted into an
    /// impact, while every accepted missile increments the room PLM argument before common
    /// vulnerability damage is applied without the ordinary enemy-deletion tail.
    /// </summary>
    private bool ResolveMotherBrainHeadShot(
        Hardware.ISnesAddressSpace bus,
        RoomEnemySlot head,
        SamusProjectileSlot projectile,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        SamusProjectileTypeWord projectileType,
        ushort projectileDamage)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain head shot AI ran without its multipart encounter state.");
        if (!ReferenceEquals(state.Head, head))
        {
            throw new InvalidOperationException(
                "Mother Brain head shot AI ran for a slot not linked to the loaded body.");
        }
        SamusProjectileFamily family = projectileType.Family;
        if (state.Form != 0)
        {
            return ResolveMotherBrainLaterFormHeadShot(
                bus,
                state,
                head,
                projectile,
                projectiles,
                sharedProjectiles,
                family,
                projectileType,
                projectileDamage);
        }

        if (family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile))
            return true;

        if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
            return false;

        (_incrementMotherBrainGlassRoomArgument ?? throw new InvalidOperationException(
            "Mother Brain head damage has no loaded glass-PLM argument writer."))();

        // `$B52D-$B539` deliberately alternates odd/even flash parity when a second hit
        // arrives during the existing flash. The common no-death damage helper then owns
        // health, hurt-AI, and the final ROM-authored hurt duration.
        head.FlashTimer = head.FlashTimer != 0 && (head.FlashTimer & 1) != 0
            ? (ushort)14
            : (ushort)13;

        byte vulnerability = ReadProjectileVulnerability(bus, head, projectileType);
        if (vulnerability == 0xff)
        {
            head.FrozenTimer = 400;
            head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0004));
            head.InvincibilityTimer = 10;
            return true;
        }

        int damage = (projectileDamage >> 1) * (vulnerability & 0x7f);
        if (damage == 0)
        {
            CreateEnemyProjectileDudShot(projectile);
            return true;
        }

        ushort hurtTime = head.HurtAiTime == 0 ? (ushort)4 : head.HurtAiTime;
        head.FlashTimer = unchecked((ushort)(hurtTime + 8));
        head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0002));
        head.Health = damage >= head.Health
            ? (ushort)0
            : unchecked((ushort)(head.Health - damage));
        return true;
    }

    /// <summary>
    /// Ports <c>$A9:B54E-B5C4</c> plus the phase-two/three common no-death damage tail.
    /// Form one (the fake-death interval) still runs the recoil bookkeeping but converts the
    /// projectile into a dud. Forms two and later apply ordinary vulnerability damage while
    /// deliberately leaving the multipart boss records alive at zero health for body AI.
    /// </summary>
    private bool ResolveMotherBrainLaterFormHeadShot(
        Hardware.ISnesAddressSpace bus,
        MotherBrainEnemyState state,
        RoomEnemySlot head,
        SamusProjectileSlot projectile,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        SamusProjectileFamily family,
        SamusProjectileTypeWord projectileType,
        ushort projectileDamage)
    {
        // DetermineMotherBrainShotReactionType maps beams to two, missiles/supers to one,
        // and every other projectile family to zero. Form four's beam branch is the Hyper
        // Beam recoil routine at `$A9:B5A9`; by then the long rainbow/Baby/phase-three state
        // machine owns the same native walk counter and neck-function words. Route the hit
        // through that owner instead of maintaining a second, subtly divergent recoil copy.
        ushort reactionType = family switch
        {
            SamusProjectileFamily.Beam => 2,
            SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile => 1,
            _ => 0,
        };
        if (state.Form == 4 && reactionType == 2)
        {
            MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
                throw new InvalidOperationException(
                    "Mother Brain received a form-four beam without its phase-three state owner.");
            sequence.ApplyPhase2Or3ShotReaction(MotherBrainProjectileType.Beam);

            // These are not host-only mirrors. `$B5BA/$B5BD` clears the body function timer
            // on recoil underflow and `$B5C0` always publishes the post-subtraction walk
            // counter immediately, before common projectile damage runs. The sequence keeps
            // the authoritative phase-three copies; expose the same writes on the encounter
            // state so debugger watches retain their native WRAM meaning between enemy turns.
            state.FunctionTimer = sequence.FunctionTimer;
            state.WalkCounter = sequence.Phase3WalkCounter;
        }
        else if (reactionType == 1)
        {
            state.WalkCounter = 0;
        }
        else
        {
            // `$B57C-$B589` subtracts $0100 and clamps a negative signed result to zero.
            // This is an animation-distance accumulator, not health or a frame countdown.
            state.WalkCounter = state.WalkCounter < 0x0100
                ? (ushort)0
                : unchecked((ushort)(state.WalkCounter - 0x0100));
        }

        if (state.Form == 1)
        {
            CreateEnemyProjectileDudShot(projectile);
            return true;
        }

        if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
            return false;

        // The native callback tail-calls common no-death shot damage. In particular,
        // charged beams use the separate charged vulnerability byte after the ordinary
        // beam freeze check; Hyper would otherwise inherit the immune plasma entry.
        NormalShotVulnerability vulnerability = ReadNormalShotVulnerability(bus, head, projectileType);
        if (vulnerability.FreezeImmediately)
        {
            head.FrozenTimer = 400;
            head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0004));
            head.InvincibilityTimer = 10;
            return true;
        }

        int damage = (projectileDamage >> 1) * vulnerability.Multiplier;
        if (damage == 0)
        {
            CreateEnemyProjectileDudShot(projectile);
            return true;
        }

        ushort hurtTime = head.HurtAiTime == 0 ? (ushort)4 : head.HurtAiTime;
        head.FlashTimer = unchecked((ushort)(hurtTime + 8));
        head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0002));
        head.Health = damage >= head.Health
            ? (ushort)0
            : unchecked((ushort)(head.Health - damage));
        return true;
    }

    /// <summary>
    /// Ports family <c>$0500</c> through Mother Brain brain callback <c>$A9:B507</c>.
    /// The first form's eight-entry projectile-family table rejects bombs outright. Later
    /// forms always run <c>$B562</c>: family five selects reaction zero, subtracting
    /// <c>$0100</c> from the walk/recoil accumulator with a signed-zero clamp. Fake-death
    /// form one then creates only a dud, while forms two and later enter common no-death
    /// damage. The bank-$A0 multibox caller already owns the bomb collision mark.
    /// </summary>
    private void ResolveMotherBrainHeadNormalBomb(
        RoomEnemySlot head,
        SamusBombProjectileSlot bomb)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain head normal-bomb AI ran without its multipart encounter state.");
        if (!ReferenceEquals(state.Head, head))
        {
            throw new InvalidOperationException(
                "Mother Brain head normal-bomb AI ran for a slot not linked to the loaded body.");
        }

        if (state.Form == 0)
            return;

        state.WalkCounter = state.WalkCounter < 0x0100
            ? (ushort)0
            : unchecked((ushort)(state.WalkCounter - 0x0100));
        if (state.Form == 1)
            return;

        ApplyCommonNormalBombDamage(head, bomb, runGenericDeath: false);
    }
}
