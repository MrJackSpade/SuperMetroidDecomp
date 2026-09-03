namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's collision reactions and hurt palette program from
/// <c>$A5:954D-$9735</c>. Geometry selection remains in the shared extended-hitbox walker;
/// this file begins only after an authored callback has selected the vulnerable eye or the
/// body has received touch/power-bomb damage.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort DraygonTouchAi = EnemyAiCodePointers.BankA5.DraygonTouch;
    private const ushort DraygonShotAi = EnemyAiCodePointers.BankA5.DraygonShot;
    private const ushort DraygonPowerBombAi = EnemyAiCodePointers.BankA5.DraygonPowerBomb;
    private const ushort DraygonDudHitboxShotAi = EnemyAiCodePointers.BankA0.DudShot;
    private const ushort DraygonNoOpHitboxTouchAi = EnemyAiCodePointers.BankA0.NoOp;

    private const int DraygonBgPalette = 0xa5a277;
    private const int DraygonSpritePalette = 0xa5a1f7;
    private const int DraygonWhiteFlashPalette = 0xa5a297;
    private const int DraygonHealthPaletteTable = 0xa596af;
    private const int DraygonHealthThresholdTable = 0xa596ef;
    private const int DraygonBgPaletteDestination = 80;
    private const int DraygonSpritePaletteDestination = 240;
    private const int DraygonHealthColorDestination = 89;

    /// <summary>
    /// Ports hurt AI <c>$A5:954D</c>. The body is rendered partly through BG2 palette five
    /// and partly through sprite palette seven, so a valid flash must update both surfaces.
    /// </summary>
    private void ApplyDraygonHurt(
        RoomEnemySlot body,
        DraygonEnemyState state,
        SamusState? samus)
    {
        bool whiteFrame = (body.FlashTimer & 2) != 0;
        _cgram!.LoadFromBus(
            _bus!,
            whiteFrame ? DraygonWhiteFlashPalette : DraygonBgPalette,
            colorCount: 16,
            destinationIndex: DraygonBgPaletteDestination);
        if (!whiteFrame)
            CopyDraygonHealthColors(state);

        _cgram.LoadFromBus(
            _bus!,
            whiteFrame ? DraygonWhiteFlashPalette : DraygonSpritePalette,
            colorCount: 16,
            destinationIndex: DraygonSpritePaletteDestination);

        // Grapple-connected flag bit zero is the retail electrocution channel. It is
        // sampled only while hurt AI owns the body and subtracts exactly 256 HP on every
        // eighth body frame before running the same fatal/nonfatal reaction as a shot.
        if (samus is null || !IsDraygonGrappleConnected(samus) || (body.FrameCounter & 7) != 0)
            return;

        body.Health = body.Health <= 0x0100
            ? (ushort)0
            : unchecked((ushort)(body.Health - 0x0100));
        ResolveDraygonReaction(body, samus);
    }

    /// <summary>
    /// The vulnerable eye callback increases future swoop curvature before common shot AI.
    /// A signed comparison prevents the stored value from ever advancing beyond <c>$98</c>.
    /// </summary>
    private static void AdvanceDraygonShotAcceleration(DraygonEnemyState state)
    {
        ushort candidate = unchecked((ushort)(state.SwoopYAcceleration + 8));
        if (unchecked((short)(candidate - 0x00a0)) < 0)
            state.SwoopYAcceleration = candidate;
    }

    /// <summary>Ports the common post-touch/shot/power-bomb reaction at <c>$A5:960D</c>.</summary>
    private void ResolveDraygonReaction(RoomEnemySlot body, SamusState? samus)
    {
        DraygonEnemyState state = RequireCompleteDraygonState(body);
        if (body.Health != 0)
        {
            UpdateDraygonHealthPalette(state);
            return;
        }

        BeginDraygonDeath(state, samus);
    }

    private void BeginDraygonDeath(DraygonEnemyState state, SamusState? samus)
    {
        if (state.Function is
            DraygonAiFunction.Dying or DraygonAiFunction.DyingSink or DraygonAiFunction.DyingFinish)
        {
            return;
        }

        RoomEnemySlot body = state.Body;
        samus?.Grapple.Phase = GrapplePhase.Dropped;
        InstallDraygonInstruction(
            body,
            state.FacingRight
                ? DraygonInstructionLists.Ilist_9C5A
                : DraygonInstructionLists.Ilist_9867);
        InstallDraygonInstruction(
            state.Eye!,
            state.FacingRight
                ? DraygonInstructionLists.Ilist_9D1C
                : DraygonInstructionLists.Ilist_997A);
        state.Eye!.VariableA = 0x804b;
        state.Function = DraygonAiFunction.Dying;

        // Bank $90's owner release is meaningful only if Draygon currently owns Samus. The
        // native caller then clears the complete grapple-flags word, including the one-shot
        // owner-release signal published by the translated bank-$90 state object.
        if (samus is not null)
        {
            if (samus.DraygonGrabbed.IsActive)
            {
                samus.DraygonGrabbed.Release(_bus!, samus);
                samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
            }
            samus.Grapple.Phase = GrapplePhase.Dropped;
        }

        short quarterDeltaX = unchecked((short)(0x0040 - (body.XPosition >> 2)));
        short quarterDeltaY = unchecked((short)(0x0078 - (body.YPosition >> 2)));
        byte cartridgeAngle = CalculateCartridgeAngle(quarterDeltaX, quarterDeltaY);
        byte movementAngle = unchecked((byte)(0x40 - cartridgeAngle));
        state.DeathMovementAngle = movementAngle;

        uint xMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            1,
            angleOffset: 0x40));
        uint yMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            1,
            angleOffset: 0x80));
        state.UnusedDeathXSpeed = unchecked((ushort)(xMagnitude >> 16));
        state.UnusedDeathXSubspeed = unchecked((ushort)xMagnitude);
        state.UnusedDeathYSpeed = unchecked((ushort)(yMagnitude >> 16));
        state.UnusedDeathYSubspeed = unchecked((ushort)yMagnitude);
    }

    private void UpdateDraygonHealthPalette(DraygonEnemyState state)
    {
        ushort tableByteIndex = 0;
        while (true)
        {
            ushort threshold = ReadWord(
                _bus!,
                DraygonHealthThresholdTable + tableByteIndex);
            if (unchecked((short)(state.Body.Health - threshold)) >= 0)
                break;
            tableByteIndex = unchecked((ushort)(tableByteIndex + 2));
        }

        if (tableByteIndex == state.HealthPaletteTableByteIndex)
            return;
        state.HealthPaletteTableByteIndex = tableByteIndex;
        CopyDraygonHealthColors(state);
    }

    private void CopyDraygonHealthColors(DraygonEnemyState state)
    {
        int source = DraygonHealthPaletteTable + state.HealthPaletteTableByteIndex * 4;
        for (int color = 0; color < 4; color++)
        {
            _cgram!.SetColor(
                DraygonHealthColorDestination + color,
                ReadWord(_bus!, source + color * 2));
        }
    }
}
