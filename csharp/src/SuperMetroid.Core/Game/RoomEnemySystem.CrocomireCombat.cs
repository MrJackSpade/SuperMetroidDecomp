namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's extended-hitbox touch, shot, and power-bomb reactions.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Dispatches $A4:B951/$B968/$BA05/$BAB4 after multibox collision.</summary>
    private void ResolveCrocomireHitboxShot(
        ushort projectileType,
        ushort projectileX,
        ushort projectileY,
        ushort callback)
    {
        CrocomireEnemyState state = _crocomire ??
            throw new InvalidOperationException("Crocomire hitbox has no body owner.");
        RoomEnemySlot body = state.Body;

        switch (callback)
        {
            case CrocomireNoOpHitboxShotAi:
            {
                // $B951 increments a temporary low-nibble value, then ORs it back into the
                // flags word instead of replacing the nibble. Preserve that odd instruction
                // sequence; changing it to an ordinary counter disagrees for several values.
                ushort value = unchecked((ushort)(state.FightFlags & 0x000f));
                if (unchecked((short)(value - 15)) < 0)
                    value++;
                state.FightFlags |= value;
                return;
            }

            case CrocomireDustHitboxShotAi:
            case CrocomireAlternateDustHitboxShotAi:
                SpawnCrocomireShotDust(projectileType, projectileX, projectileY);
                return;

            case CrocomireMouthShotAi:
                ResolveCrocomireMouthShot(
                    state,
                    body,
                    projectileType,
                    projectileX,
                    projectileY);
                return;

            case CrocomireHeaderTouchAi:
                // Some extended-map records deliberately point at the body header's RTL.
                return;

            default:
                throw new InvalidDataException(
                    $"Crocomire hitbox shot AI $A4:{callback:X4} is not translated.");
        }
    }

    /// <summary>Ports Crocomire's mouth reaction at $A4:BA05.</summary>
    private void ResolveCrocomireMouthShot(
        CrocomireEnemyState state,
        RoomEnemySlot body,
        ushort projectileType,
        ushort projectileX,
        ushort projectileY)
    {
        body.InvincibilityTimer = 0;

        // The off-screen branch bypasses projectile-family selection and still publishes
        // the pending-hit state. This wrapped signed comparison is the cartridge's exact
        // `body left edge - (camera + 256)` test.
        bool bodyLeftEdgeIsOnScreen = unchecked((short)(
            body.XPosition - body.XRadius - 256 - _crocomireCameraX)) < 0;
        ushort stepCount = 0;
        ushort family = unchecked((ushort)(projectileType & 0x0f00));
        if (bodyLeftEdgeIsOnScreen)
        {
            if (family == 0)
            {
                if ((projectileType & 0x0010) == 0)
                {
                    body.InstructionTimer = 8;
                    SpawnCrocomireShotDust(projectileType, projectileX, projectileY);
                    return;
                }
                stepCount = 2;
            }
            else if (family == (ushort)SamusProjectileFamily.Missile)
            {
                stepCount = 1;
            }
            else if (family == (ushort)SamusProjectileFamily.SuperMissile)
            {
                stepCount = 3;
            }
        }

        if (!bodyLeftEdgeIsOnScreen || stepCount != 0)
        {
            state.StepCounter = unchecked((ushort)(state.StepCounter + stepCount));
            ushort lowCount = unchecked((ushort)(state.FightFlags & 0x000f));
            if (unchecked((short)(lowCount - 15)) < 0)
                lowCount++;

            if ((state.FightFlags & 0x0800) == 0)
            {
                // Both regional constants at $A4:8692/$8694 are eight. Keep the branch
                // explicit because the source distinguishes projectile-attack state even
                // though this ROM revision stores identical delay values.
                body.InstructionTimer = unchecked((ushort)(
                    body.InstructionTimer +
                    (state.FightFunction == CrocomireFightFunction.ProjectileAttack ? 8 : 8)));
            }
            state.FightFlags = unchecked((ushort)(
                lowCount | (state.FightFlags & 0xb7f0) | 0x0800));
            state.ReactionTimer = 10;
        }

        body.FlashTimer = unchecked((ushort)(body.FlashTimer + 14));
        body.AiHandlerBits |= 0x0002;
    }

    private void SpawnCrocomireShotDust(
        ushort projectileType,
        ushort projectileX,
        ushort projectileY)
    {
        ushort animationIndex = (projectileType & 0x0200) == 0
            ? (ushort)6
            : (ushort)29;
        SpawnRoomGraphicsDustExplosion(
            projectileX,
            projectileY,
            animationIndex);
    }

    /// <summary>Ports $A4:B992 without applying normal power-bomb HP damage.</summary>
    private void ResolveCrocomirePowerBombReaction(RoomEnemySlot body)
    {
        CrocomireEnemyState state = RequireCrocomire(body);
        if (state.DeathSequenceIndex != 0)
            return;

        state.StepCounter = 3;
        if (state.FightFunction == CrocomireFightFunction.PowerBombCharge)
            return;

        state.FightFlags = unchecked((ushort)((state.FightFlags & 0x3ff0) | 0x8000));
        state.ReactionTimer = 10;
        body.FlashTimer = unchecked((ushort)(body.FlashTimer + 4));
        body.AiHandlerBits |= 0x0002;
        state.FightFunction = CrocomireFightFunction.PowerBombCharge;

        ushort reactionList = SelectCrocomirePowerBombInstructionList(body);
        InstallCrocomireInstructionList(body, reactionList);
    }

    /// <summary>
    /// Selects the fully-open/part-open/closed reaction list by scanning the ordinary
    /// spritemap pointers embedded in the current extended map, exactly as $A4:B9D8 does.
    /// </summary>
    private ushort SelectCrocomirePowerBombInstructionList(RoomEnemySlot body)
    {
        if ((body.SpritemapPointer & 0x8000) == 0)
            return 0xbdb6;

        int map = (body.Definition.Bank << 16) | body.SpritemapPointer;
        int count = ReadWord(_bus!, map);
        for (int component = 0; component < count; component++)
        {
            ushort ordinarySpritemap = ReadWord(
                _bus!,
                map + 6 + component * 8);
            if (ordinarySpritemap == 0xd600)
                return 0xbdae;
            if (ordinarySpritemap == 0xd51c)
                return 0xbdb2;
        }
        return 0xbdb6;
    }
}
