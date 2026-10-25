using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What Ceres Ridley's body does to Samus and what Samus's weapons do to Ceres Ridley.</summary>
[RoomLifetime]
internal sealed class CeresRidleyHits
{
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves the door and Mode-7 wall enemy records beside Ridley.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Performs Samus and projectile contact against Ridley's extended body.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyHits(EnemyRoomBinding room, EnemySlotTable slotTable, EnemyPresentation presentation, EnemyCollision collision, RidleyEncounter ridleyEncounter, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _room = room;
        _slotTable = slotTable;
        _presentation = presentation;
        _collision = collision;
        _ridleyEncounter = ridleyEncounter;
    }

    /// <summary>
    /// Runs Ceres Ridley's bank-$A6 shot-overlap seam against the five ordinary Samus
    /// projectile slots. The native shot handler increments a dedicated counter and never
    /// subtracts enemy health; the projectile owner performs its normal explosion change.
    /// </summary>
    public int ResolveCeresRidleyProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);

        if (_ridleyEncounter.Ridley is null || _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
            return 0;

        RoomEnemySlot slot = _slotTable.Slots[0];
        if (slot.DefinitionId != EnemyDefinitionId.RidleyCeres ||
            slot.Properties.HasAny(
                EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return 0;
        }

        foreach (SamusProjectileSlot projectile in projectiles.Slots)
        {
            if (!projectile.HasEnemyCollisionPayload ||
                !ExtendedSpritemapOverlapsRectangle(
                    slot,
                    projectile.Position.Whole,
                    projectile.Radius))
            {
                continue;
            }
            if (!projectiles.EnemyImpacts.TryStartEnemyImpact(sharedProjectiles, projectile.SlotIndex))
                continue;

            ResolveCeresRidleyShotAfterCollision(slot);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Executes the non-area-two half of <c>EnemyShot_Ridley</c> at $A6:DF8A after bank
    /// $A0 has accepted and marked an overlapping projectile. Both the ordinary-shot and
    /// physical normal-bomb walkers dispatch this exact callback; keeping the reaction here
    /// prevents those two collision owners from acquiring subtly different cinematic rules.
    /// </summary>
    private void ResolveCeresRidleyShotAfterCollision(RoomEnemySlot slot)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        if (slot.DefinitionId != EnemyDefinitionId.RidleyCeres)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} entered Ceres Ridley's shot branch as " +
                $"definition {slot.DefinitionId}.");
        }

        // `$DF99-$DFA9` starts with Y=13, shifts the old nonzero timer once, and selects
        // 14 only when that shift produced carry. In host terms this is precisely old bit
        // zero; testing bit one (or simply toggling the new value) changes repeated-hit
        // timing and eventually moves the escape threshold relative to the hurt palette.
        slot.FlashTimer = WrappingCountdown.FromRaw(!slot.FlashTimer.IsZero && (slot.FlashTimer.RawValue & 1) != 0
            ? (ushort)14
            : (ushort)13);
        state.HitCounter++;

        // The cartridge draws the hurt palette from the enemy frame that just completed.
        // Collision runs afterward, so publish that same pre-increment frame immediately
        // for standalone renderers inspecting CGRAM before the next enemy dispatcher call.
        UpdateRidleyHurtFlashPalettes(
            slot,
            state,
            unchecked((ushort)(slot.FrameCounter - 1)));
        UpdateCeresRidleyHealthPalette(state);
    }

    /// <summary>
    /// Ports <c>RidleyHurtFlashHandling</c> at $A6:D4DA and the common extended-enemy
    /// palette override at $A0:9497. Ridley's composite is split between those two writers:
    /// the ordinary body uses the current execution counter, while the manual tail/wings
    /// deliberately test the counter after an increment.
    /// </summary>
    internal static void UpdateRidleyHurtFlashPalettes(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        int enemyMainExecutionCounter)
    {
        state.CommonDrawPaletteIndex = new SnesObjAttributeWord(!slot.FlashTimer.IsZero &&
            (enemyMainExecutionCounter & 2) != 0
                ? (ushort)0
                : checked((ushort)slot.PaletteIndex));

        state.SpritemapPaletteIndex = slot.FlashTimer.RawValue > 1 &&
            (unchecked((ushort)(enemyMainExecutionCounter + 1)) & 2) != 0
                ? CeresRidleyBehavior.HurtFlashSpritemapPalette
                : SnesObjPalettes.Palette(7);
    }

    /// <summary>
    /// Ports $A6:D4B5-$D4D7. Ceres uses shot count in place of health, including the
    /// original missing branch after the 90-shot comparison: 50-69 selects palette zero,
    /// and every value from 70 onward selects palette two.
    /// </summary>
    internal void UpdateCeresRidleyHealthPalette(RidleyEnemyState state)
    {
        if (state.FightMode == RidleyFightMode.NotFighting || state.HitCounter < 50)
            return;

        int paletteIndex = state.HitCounter < 70
            ? CeresRidleyPaletteRomData.HealthMidRow
            : CeresRidleyPaletteRomData.HealthLateRow;
        (_bossArtwork.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley damage requires installed health colors."))
            .ApplyHealth(_presentation.Colours, paletteIndex);
    }

    /// <summary>Routes exploding normal bombs to Ceres Ridley's shot counter after common bank-$A0 overlap.</summary>
    internal void RegisterCeresRidleyBombReactions(EnemySharedReactions table)
    {
        // `$A6:DF8A` branches on area index before it ever reaches common vulnerability damage. Ceres therefore
        // treats an exploding normal bomb exactly like an ordinary shot for its cinematic counter: the bank-$A0
        // walker has already marked the physical bomb, while Ridley alternates hurt timing and advances the 100-hit
        // escape condition.
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.RidleyCeres &&
                                  c.SelectedShotAi == EnemyInteractionCallback.RidleyShot,
            c => ResolveCeresRidleyShotAfterCollision(c.Enemy));
    }

    /// <summary>Tests Samus against Ridley's extended body and applies contact response after native invincibility gating.</summary>
    internal bool ResolveRidleyBodySamusContact(SamusState samus)
    {
        Ensure.NotNull(samus);
        _room.EnsureLoaded();
        // $A0:9A8B-$9A98 gates ordinary contact only on the contact-damage mode and
        // Samus's invincibility timer. It does not inspect the bank-$90 knockback-active
        // word. The extra host guard previously made a stale/ongoing aerial knockback
        // suppress every later Ridley overlap even after invincibility had expired.
        RoomEnemySlot slot = _slotTable.Slots[0];
        if (_ridleyEncounter.Ridley is null ||
            !RidleyInteractions.IsRidleyDefinition(slot.DefinitionId) ||
            slot.DefinitionId == EnemyDefinitionId.RidleyCeres && _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ||
            !samus.InvincibilityTimer.IsZero)
        {
            return false;
        }

        if (slot.Frame is null ||
            slot.Properties.HasAny(
                EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return false;
        }

        if (!_collision.TryFindExtendedHitboxCallback(
                slot,
                samus.Position.Whole,
                samus.Kinematics.Radius,
                callbackKind: HitboxCallbackKind.Touch,
                out EnemyInteractionCallback? touchAi))
        {
            return false;
        }

        if (touchAi != EnemyInteractionCallback.RidleyExtendedTouch)
        {
            throw new InvalidDataException(
                $"Ridley extended body selected touch AI {touchAi?.ToString() ?? "(unnamed)"}, expected " +
                $"{EnemyInteractionCallback.RidleyExtendedTouch} from map {slot.Frame}.");
        }

        // $A6:DF59 enters the common no-death-check handler. That distinction matters for
        // Screw Attack: a lethal body touch leaves Lower Norfair Ridley alive long enough
        // to select the forced zero-health lunge and grab Samus for the authored death.
        _collision.ResolveNormalEnemyTouch(
            slot,
            samus,
            deathAnimation: TouchDeathAnimation.Skip);
        return true;
    }

    /// <summary>
    /// Shared port of the rectangle walk used by $A0:9A5A (Samus contact) and $A0:9B7F
    /// (projectile contact). Every component offset and every rectangle comes from the
    /// active extended spritemap; the tiny 8x8 radius in Ridley's enemy header is not his
    /// body and must never be substituted for this data.
    /// </summary>
    internal static bool ExtendedSpritemapOverlapsRectangle(
        RoomEnemySlot slot,
        Position16<RoomPixels> subject,
        Extent16<RoomPixels> subjectRadius)
    {
        int subjectLeft = unchecked((ushort)(subject.X - subjectRadius.X));
        int subjectRight = unchecked((ushort)(subject.X + subjectRadius.X));
        int subjectTop = unchecked((ushort)(subject.Y - subjectRadius.Y));
        int subjectBottom = unchecked((ushort)(subject.Y + subjectRadius.Y));
        var components =
            RidleyCollisionDefinitions.ComponentsAt(slot.Frame);
        foreach (RidleyCollisionComponent component in components)
        {
            Position16<RoomPixels> componentOrigin = slot.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (EnemyHitboxRectangle hitbox in component.Hitboxes)
            {
                int left = unchecked((ushort)(componentX + hitbox.Left));
                int top = unchecked((ushort)(componentY + hitbox.Top));
                int right = unchecked((ushort)(componentX + hitbox.Right));
                int bottom = unchecked((ushort)(componentY + hitbox.Bottom));

                // $A0:9B14/$9B20/$9B2C/$9B38 use signed comparisons and treat touching
                // right/bottom edges as non-overlap. Ceres coordinates remain in the low
                // positive room range, so these integer comparisons are the same result.
                if (unchecked((short)(left - subjectRight)) <= 0 &&
                    unchecked((short)(right - subjectLeft)) > 0 &&
                    unchecked((short)(top - subjectBottom)) <= 0 &&
                    unchecked((short)(bottom - subjectTop)) > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
