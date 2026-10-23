using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's hitboxes, touch and hurt, and what Samus's weapons do to them.</summary>
[RoomLifetime]
internal sealed class TorizoCombat
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Exposes the two live CGRAM rows occupied by Torizo's body.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Applies shot, bomb, and touch damage through the shared collision rules.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, program start and the low-health drool it triggers.</summary>
    private readonly TorizoBody _body;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function word their branches test.</summary>
    private readonly TorizoEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoCombat(EnemyRoomBinding room, EnemyPresentation presentation, EnemyCollision collision, TorizoBody body, TorizoEncounterAccess access)
    {
        _room = room;
        _presentation = presentation;
        _collision = collision;
        _body = body;
        _access = access;
    }

    /// <summary>
    /// Dispatches an already-colliding family-<c>$0500</c> bomb through Torizo hitbox
    /// callbacks <c>$AA:C97C/$C9C2/$D667</c>. The bank-$A0 multibox walker has already set
    /// direction bit <c>$10</c>; this method owns only the species callback and its common
    /// no-death damage tail.
    /// </summary>
    private void ResolveTorizoNormalBomb(
        RoomEnemySlot torizo,
        SamusBombProjectileSlot bomb,
        EnemyInteractionCallback? selectedShotAi)
    {
        bool isBombTorizo = torizo.DefinitionId == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo;
        if (!isBombTorizo && !isGoldenTorizo)
            throw new ArgumentException("Normal-bomb Torizo dispatch requires a Torizo body.");

        TorizoEnemyState state = _access.RequireBombTorizoState(torizo);
        if (selectedShotAi == EnemyInteractionCallback.TorizoStandUpSitDownShot)
        {
            // `$C9C2` is a complete no-op in Bomb Torizo's area. Golden Torizo jumps to
            // `$D658`, which accepts common damage only outside the active flash and while
            // `toriz_var_04` is zero. This transitional callback never arms the later
            // counterattack bits or captures a projectile family.
            if (isBombTorizo || !torizo.FlashTimer.IsZero || state.AnimationLocked)
                return;

            _collision.ApplyCommonNormalBombDamage(torizo, bomb, runGenericDeath: false);
            if (torizo.Health == 0)
                BeginBombTorizoDeath(torizo, state);
            return;
        }

        bool validMainCallback = isBombTorizo
            ? selectedShotAi == EnemyInteractionCallback.BombTorizoShot
            : selectedShotAi is EnemyInteractionCallback.BombTorizoShot or EnemyInteractionCallback.GoldenTorizoShot;
        if (!validMainCallback)
        {
            throw new InvalidDataException(
                $"Torizo normal-bomb hitbox AI {selectedShotAi?.ToString() ?? "(unnamed)"} is not translated.");
        }

        // Both normal callbacks return during the hurt flash. Bomb Torizo also uses its
        // awakening guard as a hard rejection; Golden's nonzero guard deliberately falls
        // through to common damage instead.
        if (!torizo.FlashTimer.IsZero || isBombTorizo && state.AnimationLocked)
            return;

        if (isGoldenTorizo && !state.AnimationLocked)
        {
            // A bomb is neither Missile nor Super Missile, so `$D667` takes LABEL_11 and arms the
            // stun latch before common damage, whether or not the Super-catch latch is set.
            SetBehavior(torizo, TorizoBehaviorFlags.Stunned);
        }

        _collision.ApplyCommonNormalBombDamage(torizo, bomb, runGenericDeath: false);
        if (torizo.Health == 0)
            BeginBombTorizoDeath(torizo, state);
    }

    /// <summary>Restores the normal body rows on alternating Bomb Torizo hurt frames.</summary>
    private void LoadTorizoDeathPalette() =>
        _body.LoadTorizoBodyPalette(TorizoPaletteDefinitions.Normal[..16], TorizoPaletteDefinitions.Normal[16..]);

    /// <summary>Selects Golden Torizo's body and belly gradient row from health bits 11 through 14.</summary>
    private void LoadGoldenTorizoHealthPalette(int health)
    {
        // $84:8000 selects one of eight 16-color rows using bits 11..14 of health, clamping
        // the upper half to row seven. Deriving the row start algebraically preserves the
        // original descending copy while keeping the loop natural for debugger inspection.
        int selector = SnesLittleEndian.HighByte(checked((ushort)health)) & TorizoCombatDefinitions.HealthPaletteRowBits;
        if ((selector & TorizoCombatDefinitions.HealthPaletteUpperHalf) != 0)
            selector = 56;
        int sourceWord = selector * 2;
        var colors = _room.Artwork?.AuxiliaryColors ?? throw new InvalidDataException(
            "Golden Torizo health palettes require installed artwork.");
        for (int color = 0; color < 16; color++)
        {
            _presentation.BossColours().TorizoBody.Write(
                color,
                colors.Resolve(SuperMetroid.Core.Enemies.Artwork.EnemyAuxiliaryPalette.GoldenTorizoBody,
                    sourceWord / 16, color));
            _presentation.BossColours().TorizoBelly.Write(
                color,
                colors.Resolve(SuperMetroid.Core.Enemies.Artwork.EnemyAuxiliaryPalette.GoldenTorizoBelly,
                    sourceWord / 16, color));
        }
    }

    /// <summary>Ports Bomb Torizo's custom hurt callback $AA:C67E.</summary>
    internal void ApplyBombTorizoHurt(RoomEnemySlot torizo)
    {
        _body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
        if ((torizo.FlashTimer.RawValue & 1) == 0)
        {
            if (torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo)
                LoadGoldenTorizoHealthPalette(torizo.Health);
            else
                LoadTorizoDeathPalette();
        }
        else
        {
            for (int color = 0; color < 16; color++)
            {
                _presentation.BossColours().TorizoBody.Write(color, Bgr555.White);
                _presentation.BossColours().TorizoBelly.Write(color, Bgr555.White);
            }
        }
    }

    /// <summary>Enters the authored death list once and removes the dying statue from Samus collision.</summary>
    internal void BeginBombTorizoDeath(RoomEnemySlot torizo, TorizoEnemyState state)
    {
        if (state.DeathStarted)
            return;
        state.DeathStarted = true;
        state.Function = TorizoFunction.Idle;
        TorizoBody.StartTorizoBodyProgram(torizo, state, AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.DeathSequence0));
        SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp | TorizoBehaviorFlags.FaceBlownUp);
        // EnemyShot_Torizo_Normal `$AA:C9B7-$AA:C9BD` sets property $0400 when
        // entering the death stream. This removes the dying actor from Samus contact and
        // solid-enemy collision while its flashing/invisible instruction frames continue.
        torizo.Properties = torizo.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Latches <paramref name="flags"/>.</summary>
    internal void SetBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flags) => _access.RequireBombTorizoState(torizo).Behavior |= flags;

    /// <summary>Routes overlapping normal bombs through the selected Torizo hitbox callback.</summary>
    internal void RegisterBombTorizoBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            c => ResolveTorizoNormalBomb(c.Enemy, c.Bomb, c.SelectedShotAi));
    }

    /// <summary>Validates extended hitbox callbacks, handles catches, and defers death to Torizo's own sequence.</summary>
    internal void RegisterBombTorizoShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
                                                     EnemyCollision.ShotAi(enemy) == (enemy.DefinitionId == EnemyDefinitionId.BombTorizo
                                                         ? EnemyInteractionCallback.BombTorizoShot
                                                         : EnemyInteractionCallback.GoldenTorizoShot))
        {
            // Golden's definition installs $D667, but its ordinary extended body hitboxes call shared $C97C; that
            // wrapper dispatches to $D667 by area. Stand-up/sit-down maps deliberately use $C9C2 instead. That
            // callback is a no-op for area-zero Bomb Torizo and a guarded direct-damage path for Golden Torizo, so it
            // must remain distinguishable below.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes || c.HitboxShotAi is not (
                        EnemyInteractionCallback.BombTorizoShot or EnemyInteractionCallback.TorizoStandUpSitDownShot))
                {
                    throw new InvalidDataException(
                        $"Torizo hitbox shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                return false;
            },
            BeforeImpact = ResolveTorizoShotBeforeImpact,
            ExemptFromGenericDeath = true,
            AfterCommon = c =>
            {
                if (c.Enemy.Health == 0)
                    BeginBombTorizoDeath(c.Enemy, _access.RequireBombTorizoState(c.Enemy));
            },
        });
    }

    /// <summary>Registers extended body touch damage without invoking the generic death animation.</summary>
    internal void RegisterBombTorizoTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BombTorizoTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes || context.HitboxTouchAi != EnemyInteractionCallback.BombTorizoTouch)
                {
                    throw new InvalidDataException(
                        $"Torizo hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                if (context.Slot.Health == 0)
                    BeginBombTorizoDeath(context.Slot, _access.RequireBombTorizoState(context.Slot));
            });
    }

    /// <summary>
    /// Handles flash/awakening guards and Golden Torizo missile catches before shared
    /// shot damage; returns true when the hit was consumed by a special reaction.
    /// </summary>
    internal bool ResolveTorizoShotBeforeImpact(in EnemyShotContext c)
    {
        RoomEnemySlot enemy = c.Enemy;
        bool isBombTorizo = enemy.DefinitionId == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = !isBombTorizo;
        TorizoEnemyState torizoState = _access.RequireBombTorizoState(enemy);
        if (c.HitboxShotAi == EnemyInteractionCallback.TorizoStandUpSitDownShot)
        {
            // `$AA:C9C2` returns immediately in area zero. In nonzero areas it calls `$AA:D658`, which admits common
            // no-death shot damage only while both the flash timer and native `toriz_var_04` are zero. Crucially it
            // never enters `$AA:D667`, so missiles are not caught, Supers are not reflected, and parameter-two
            // counterattack bits are not armed through these transitional body rectangles.
            if (isBombTorizo || !enemy.FlashTimer.IsZero || torizoState.AnimationLocked)
            {
                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                return true;
            }
        }
        if (!enemy.FlashTimer.IsZero || isBombTorizo && torizoState.AnimationLocked)
        {
            // Both callbacks return during a damage flash. Bomb Torizo also rejects shots while its awakening guard
            // is set; Golden Torizo's nonzero guard instead routes through common damage below.
            c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
            return true;
        }

        // A caught Super bypasses family selection, but still reaches the native stun write before common damage.
        // Animation lock and the transitional hitbox callback bypass that write instead.
        if (c.HitboxShotAi == EnemyInteractionCallback.BombTorizoShot && isGoldenTorizo &&
            !torizoState.AnimationLocked &&
            _body.HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
            SetBehavior(enemy, TorizoBehaviorFlags.Stunned);

        if (c.HitboxShotAi == EnemyInteractionCallback.BombTorizoShot &&
            isGoldenTorizo && !torizoState.AnimationLocked &&
            !_body.HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
        {
            if (c.Family == SamusProjectileFamily.Missile)
            {
                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                c.Projectile.Direction = c.Projectile.PackedDirection.WithoutCollisionLifecycleState();
                torizoState.Function = TorizoFunction.Idle;
                enemy.InstructionTimer = WrappingCountdown.Start(1);
                _access.RequireBombTorizoState(enemy).BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(_access.FacesRight(enemy)
                    ? TorizoBodyProgram.GoldenDodgeTurningRight
                    : TorizoBodyProgram.GoldenDodgeTurningLeft);
                return true;
            }

            if (c.Family == SamusProjectileFamily.SuperMissile)
            {
                if (c.Samus is not null && _access.BombTorizoFunction12IsNonNegative(enemy, c.Samus))
                {
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    SetBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile);
                    torizoState.Function = TorizoFunction.Idle;
                    c.Projectile.Direction = c.Projectile.PackedDirection.WithCollisionLifecycleState();
                    enemy.InstructionTimer = WrappingCountdown.Start(1);
                    _access.RequireBombTorizoState(enemy).BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(_access.Posture(enemy).HasFlag(TorizoPostureFlags.OppositeLegForward)
                        ? _access.FacesRight(enemy)
                            ? TorizoBodyProgram.GoldenCaughtSuperFacingRightLeftLegFwd
                            : TorizoBodyProgram.GoldenCaughtSuperFacingLeftRightLegFwd
                        : _access.FacesRight(enemy)
                            ? TorizoBodyProgram.GoldenCaughtSuperFacingRightRightLegFwd
                            : TorizoBodyProgram.GoldenCaughtSuperFacingLeftLeftLegFwd);
                    return true;
                }
            }
            else
            {
                // Beams and other common-damage families arm the high-health counterattack branch before entering
                // normal shot AI.
                SetBehavior(enemy, TorizoBehaviorFlags.Stunned);
            }
        }
        return false;
    }

    /// <summary>
    /// Registers Torizo_Hurt for each variant; the common instruction interpreter still
    /// advances after the hurt callback, matching <c>$A0:8FF7</c>.
    /// </summary>
    internal void RegisterBombTorizoHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            (slot, _) => ApplyBombTorizoHurt(slot));

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindTorizoHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (enemy.DefinitionId is not (EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo))
            return null;

        foreach (GoldenTorizoCollisionComponent component in
                 TorizoCollisionDefinitions.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            IReadOnlyList<GoldenTorizoCollisionHitbox> rectangles = component.Hitboxes.Rectangles;
            for (int index = 0; index < rectangles.Count; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = rectangles[index];
                ushort left = unchecked((ushort)(componentX + hitbox.Left));
                ushort top = unchecked((ushort)(componentY + hitbox.Top));
                ushort right = unchecked((ushort)(componentX + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? TorizoHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : TorizoHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    }
}
