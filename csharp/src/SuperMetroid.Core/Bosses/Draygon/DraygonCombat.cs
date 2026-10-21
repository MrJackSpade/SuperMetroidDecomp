using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's hitboxes, touch and hurt, and what Samus's weapons do to it.</summary>
[RoomLifetime]
internal sealed class DraygonCombat
{
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Updates the body and eye palette bands used by Draygon's presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Evaluates Samus and projectile contacts against Draygon's linked parts.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
    private readonly DraygonParts _parts;
    /// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
    private readonly DraygonGrab _grab;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonCombat(EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyCollision collision, DraygonParts parts, DraygonGrab grab, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _projectiles = projectiles;
        _presentation = presentation;
        _collision = collision;
        _parts = parts;
        _grab = grab;
    }

    /// <summary>Recognizes the four enemy definitions that form one Draygon encounter.</summary>
    internal static bool IsDraygonDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.DraygonBody or EnemyDefinitionId.DraygonEye or
        EnemyDefinitionId.DraygonTail or EnemyDefinitionId.DraygonArms;

    /// <summary>Returns the encounter state owned by <paramref name="body"/>; each part property requires its linked slot when read.</summary>
    internal DraygonEnemyState RequireCompleteDraygonState(RoomEnemySlot body)
    {
        DraygonEnemyState state = _parts.Draygon ??
            throw new InvalidDataException("Draygon body has no encounter state.");
        if (!ReferenceEquals(state.Body, body))
            throw new InvalidDataException("Draygon's encounter state belongs to a different body slot.");
        return state;
    }

    /// <summary>Routes body bomb hits through Draygon's special shot reaction.</summary>
    internal void RegisterDraygonBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.DraygonShot)
        {
            RunsGenericDeath = false,
            BeforeCommon = c => AdvanceDraygonShotAcceleration(RequireCompleteDraygonState(c.Enemy)),
            AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
        });
    }

    /// <summary>Routes power-bomb damage for Draygon's body through its encounter reaction.</summary>
    internal void RegisterDraygonPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DraygonPowerBomb)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (enemy, samus) => ResolveDraygonReaction(enemy, samus),
        });
    }

    /// <summary>Registers the body and linked-part contact behavior against Samus.</summary>
    internal void RegisterDraygonTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.DraygonTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Draygon requires his authored extended touch hitboxes.");
                if (context.HitboxTouchAi == EnemyInteractionCallback.NoOp)
                    return; // The vulnerable eye rectangle is harmless to Samus.
                if (context.HitboxTouchAi != EnemyInteractionCallback.DraygonTouch)
                {
                    throw new InvalidDataException(
                        $"Draygon hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                ResolveDraygonReaction(context.Slot, context.Samus);
            });
    }

    /// <summary>
    /// Ports hurt AI <c>$A5:954D</c>. The body is rendered partly through BG2 palette five
    /// and partly through sprite palette seven, so a valid flash must update both surfaces.
    /// </summary>
    internal void ApplyDraygonHurt(
        RoomEnemySlot body,
        DraygonEnemyState state,
        SamusState? samus)
    {
        bool whiteFrame = (body.FlashTimer.RawValue & 2) != 0;
        (_bossArtwork.Artwork?.DraygonColors ?? throw new InvalidOperationException(
            "Draygon damage requires installed color artwork."))
            .ApplyHurt(_presentation.Cgram, whiteFrame, RequireHealthPaletteBand(state));

        // Grapple-connected flag bit zero is the retail electrocution channel. It is
        // sampled only while hurt AI owns the body and subtracts exactly 256 HP on every
        // eighth body frame before running the same fatal/nonfatal reaction as a shot.
        if (samus is null || !DraygonGrab.IsDraygonGrappleConnected(samus) || (body.FrameCounter & 7) != 0)
            return;

        body.Health = body.Health <= 256
            ? 0
            : body.Health - 256;
        ResolveDraygonReaction(body, samus);
    }

    /// <summary>
    /// The vulnerable eye callback increases future swoop curvature before common shot AI.
    /// A signed comparison prevents the stored value from ever advancing beyond <c>$98</c>.
    /// </summary>
    internal static void AdvanceDraygonShotAcceleration(DraygonEnemyState state)
    {
        EightEightVector<RoomPixels> candidate = state.SwoopAcceleration + DraygonBehavior.DraygonShotSwoopAccelerationStep;
        if (unchecked((short)(candidate.Y.RawValue - DraygonBehavior.DraygonSwoopAccelerationLimit)) < 0)
            state.SwoopAcceleration = candidate;
    }

    /// <summary>Ports the common post-touch/shot/power-bomb reaction at <c>$A5:960D</c>.</summary>
    internal void ResolveDraygonReaction(RoomEnemySlot body, SamusState? samus)
    {
        DraygonEnemyState state = RequireCompleteDraygonState(body);
        if (body.Health != 0)
        {
            UpdateDraygonHealthPalette(state);
            return;
        }

        BeginDraygonDeath(state, samus);
    }

    /// <remarks>
    /// `DraygonReaction_Common`'s dead branch ($A5:9618) has no "already dying" test. Every
    /// touch, shot or power-bomb reaction on a zero-health body runs it again: the dying
    /// arm and eye lists restart, the drift function is reinstalled and Samus is released.
    /// </remarks>
    private void BeginDraygonDeath(DraygonEnemyState state, SamusState? samus)
    {
        RoomEnemySlot body = state.Body;
        samus?.Grapple.Phase = GrapplePhase.Dropped;
        _parts.StartDraygonProgram(
            body,
            state.FacingRight
                ? DraygonProgram.ArmsFacingRightDying
                : DraygonProgram.ArmsFacingLeftDying);
        _parts.StartDraygonProgram(
            state.Eye,
            state.FacingRight
                ? DraygonProgram.EyeFacingRightDying
                : DraygonProgram.EyeFacingLeftDying);
        (DraygonParts.RequirePart(state.Eye)).RequireSpeciesState<DraygonEyeState>().Function = DraygonEyeFunction.Inert;
        state.Function = DraygonAiFunction.Dying;

        // The native death callback releases Samus unconditionally, even during an
        // unrelated movement handler. In particular, this can interrupt a shinespark.
        // The caller then clears the grapple word, including the owner-release signal.
        if (samus is not null)
        {
            _grab.ReleaseSamusDuringEnemyMain(samus);
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
            samus.Grapple.Phase = GrapplePhase.Dropped;
        }
    }

    /// <summary>Changes Draygon's health color band only when the health threshold changes.</summary>
    internal void UpdateDraygonHealthPalette(DraygonEnemyState state)
    {
        int band = DraygonHealthPaletteDefinitions.BandForHealth(state.Body.Health);

        if (band == state.HealthPaletteBand)
            return;
        state.HealthPaletteBand = band;
        CopyDraygonHealthColors(state);
    }

    /// <summary>The installed health band; native code reads $7E:781C only after initialization installs one.</summary>
    private static int RequireHealthPaletteBand(DraygonEnemyState state) => state.HealthPaletteBand
        ?? throw new InvalidOperationException("Draygon has no installed health palette band.");

    /// <summary>Copies the selected installed health palette band into live CGRAM.</summary>
    internal void CopyDraygonHealthColors(DraygonEnemyState state)
    {
        (_bossArtwork.Artwork?.DraygonColors ?? throw new InvalidDataException(
            "Draygon health colors require installed artwork."))
            .ApplyHealthBand(_presentation.Cgram, RequireHealthPaletteBand(state));
    }

    /// <summary>Registers Draygon's shot response, including acceleration and special death exemption.</summary>
    internal void RegisterDraygonShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DraygonShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes)
                    throw new InvalidDataException("Draygon requires his authored extended shot hitboxes.");
                if (c.HitboxShotAi == EnemyInteractionCallback.DudShot)
                {
                    // Body, claws, and shell use shared `$A0:8046`; only the first
                    // eye rectangle in maps $1A/$2D reaches Draygon's damage callback.
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                    return true;
                }
                if (c.HitboxShotAi != EnemyInteractionCallback.DraygonShot)
                {
                    throw new InvalidDataException(
                        $"Draygon hitbox shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                return false;
            },
            AfterImpact = c => AdvanceDraygonShotAcceleration(RequireCompleteDraygonState(c.Enemy)),
            ExemptFromGenericDeath = true,
            AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
        });
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindDraygonHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(DraygonCombat.IsDraygonDefinition(enemy.EnemyDefinitionPointer) &&
            enemy.Definition.Bank == DraygonBg2FrameDefinitions.Bank))
            return null;

        // Both OAM and BG2 art are replaceable presentation. Their native
        // component offsets and hitbox pointers remain fixed gameplay data.
        foreach (DraygonCollisionComponent component in
                 DraygonCollisionDefinitions.ComponentsAt(
                     probe.Frames.Spritemap(enemy.Frame)))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (DraygonCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? DraygonHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : DraygonHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    }
}
