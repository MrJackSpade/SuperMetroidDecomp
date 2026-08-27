using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Baby Metroid damage, palette, death, acceleration, collision, and coordinate helpers.
/// </summary>
public sealed partial class BabyMetroidCutsceneState
{
    public BabyMetroidOnionRingHitResult ApplyMotherBrainOnionRingHit(ushort damage = 0x0050)
    {
        ushort healthBefore = Health;
        if (healthBefore == 0)
            return new BabyMetroidOnionRingHitResult(false, healthBefore, healthBefore, OnionRingHitFlashTimer);

        OnionRingHitFlashTimer = 0x0010;
        Health = healthBefore < damage
            ? (ushort)0
            : unchecked((ushort)(healthBefore - damage));
        return new BabyMetroidOnionRingHitResult(true, healthBefore, Health, OnionRingHitFlashTimer);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private void StepSamusRainbowPaletteAnimation(
        SamusState samus,
        ref bool samusRainbowActivated)
    {
        switch (SamusRainbowPhase)
        {
            case BabyMetroidSamusRainbowPhase.Inactive:
                return;

            case BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow:
                // `$CD30` uses a signed CMP/BMI after adding sixteen to the Baby's Y.
                if (unchecked((short)(YPosition + 0x0010 - samus.YPosition)) < 0)
                    return;
                samus.Drained.EnableRainbow(samus);
                samusRainbowActivated = true;
                SamusRainbowPhase = BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown;
                return;

            case BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown:
            {
                uint sum = (uint)SamusRainbowPaletteAnimationCounter + 0x0300;
                SamusRainbowPaletteAnimationCounter = unchecked((ushort)sum);
                if (sum > ushort.MaxValue)
                    samus.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
                return;
            }

            default:
                throw new InvalidOperationException($"Unsupported Samus rainbow phase {SamusRainbowPhase}.");
        }
    }

    private void AccelerateDownwardsForDeath()
    {
        // `$CE40` subtracts `$20` from the magnitude, clamps at zero, reapplies the old
        // sign, and independently adds two to vertical 8.8 velocity with 16-bit wrap.
        bool negative = (XVelocity & 0x8000) != 0;
        ushort magnitude = negative ? unchecked((ushort)-XVelocity) : XVelocity;
        magnitude = magnitude >= 0x0020 ? unchecked((ushort)(magnitude - 0x0020)) : (ushort)0;
        XVelocity = negative ? unchecked((ushort)-magnitude) : magnitude;
        YVelocity = unchecked((ushort)(YVelocity + 2));
    }

    private bool StepFadeToBlack(ref BabyMetroidPaletteTransferRequest? paletteTransfer)
    {
        // No timer changes occur until the actor's centre reaches Y `$80`.
        if (unchecked((short)(YPosition - 0x0080)) < 0)
            return false;

        FadeToBlackPaletteTimer = unchecked((ushort)(FadeToBlackPaletteTimer - 1));
        if ((FadeToBlackPaletteTimer & 0x8000) == 0)
            return false;

        FadeToBlackPaletteTimer = 8;
        ushort nextIndex = unchecked((ushort)(FadeToBlackPaletteIndex + 1));
        if (nextIndex >= 7)
            return true;

        FadeToBlackPaletteIndex = nextIndex;
        paletteTransfer = new BabyMetroidPaletteTransferRequest(
            PaletteIndex: nextIndex,
            SourceAddress: unchecked((uint)(0xade8f0 + nextIndex * 0x1c)),
            DestinationColorIndex: 0x01e2,
            ColorCount: 0x000e);
        return false;
    }

    private BabyMetroidDeathExplosionRequest? StepDeathExplosion()
    {
        DeathExplosionTimer = unchecked((ushort)(DeathExplosionTimer - 1));
        if ((DeathExplosionTimer & 0x8000) == 0)
            return null;

        DeathExplosionTimer = 4;
        DeathExplosionPatternIndex++;
        if (DeathExplosionPatternIndex >= 10)
            DeathExplosionPatternIndex = 0;
        int index = DeathExplosionPatternIndex;
        return new BabyMetroidDeathExplosionRequest(
            PatternIndex: DeathExplosionPatternIndex,
            XPosition: unchecked((ushort)(XPosition + DeathExplosionXOffsets[index])),
            YPosition: unchecked((ushort)(YPosition + DeathExplosionYOffsets[index])),
            ProjectileParameter: 3,
            SoundEffect: 0x0013);
    }

    private void SetInstructionList(ushort pointer)
    {
        InstructionList = pointer;
        InstructionTimer = 1;
        InstructionLoopCounter = 0;
    }

    private void UpdateSpeedAndAngle(
        ISnesAddressSpace bus,
        ushort angleDelta,
        ushort targetAngle,
        ushort targetSpeed)
    {
        // `$CF31` changes speed by exactly `$20` and clamps rather than crossing the target.
        if (Speed != targetSpeed)
        {
            if (targetSpeed < Speed)
                Speed = Speed - 0x20 < targetSpeed ? targetSpeed : unchecked((ushort)(Speed - 0x20));
            else
                Speed = Speed + 0x20 >= targetSpeed ? targetSpeed : unchecked((ushort)(Speed + 0x20));
        }

        ushort candidateAngle = unchecked((ushort)(Angle + angleDelta));
        if (unchecked((short)angleDelta) < 0)
        {
            Angle = unchecked((short)(candidateAngle - targetAngle)) >= 0
                ? candidateAngle
                : targetAngle;
        }
        else
        {
            Angle = unchecked((short)(candidateAngle - targetAngle)) < 0
                ? candidateAngle
                : targetAngle;
        }

        byte angleByte = unchecked((byte)(Angle >> 8));
        XVelocity = CalculateVelocityComponent(bus, Speed, angleByte);
        YVelocity = CalculateVelocityComponent(bus, Speed, unchecked((byte)(angleByte + 0x40)));
    }

    private static ushort CalculateVelocityComponent(
        ISnesAddressSpace bus,
        ushort speed,
        byte sineIndex)
    {
        int address = SignedSineTable + sineIndex * 2;
        short sine = unchecked((short)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

        // Bank `$86:C27A` multiplies unsigned speed by the absolute signed-table value,
        // returns product bits 8..23, then reapplies the original sign.
        uint product = unchecked((uint)(speed * Math.Abs((int)sine)));
        ushort magnitude = unchecked((ushort)(product >> 8));
        return sine < 0 ? unchecked((ushort)-magnitude) : magnitude;
    }

    private void MoveAccordingToVelocity()
    {
        XPosition = AddNativeEightEightVelocity(XPosition, XSubposition, XVelocity, out ushort xSubposition);
        XSubposition = xSubposition;
        YPosition = AddNativeEightEightVelocity(YPosition, YSubposition, YVelocity, out ushort ySubposition);
        YSubposition = ySubposition;
    }

    private static ushort AddNativeEightEightVelocity(
        ushort wholePosition,
        ushort subposition,
        ushort velocity,
        out ushort newSubposition)
    {
        // SEP #$20 adds velocity's low byte to subposition byte +1. REP #$20 then sign-
        // extends velocity's high byte, retaining the 8-bit ADC carry for whole pixels.
        int fractionalSum = (subposition >> 8) + (velocity & 0x00ff);
        newSubposition = unchecked((ushort)(
            ((byte)fractionalSum << 8) | (subposition & 0x00ff)));
        int wholeDelta = unchecked((sbyte)(velocity >> 8)) + (fractionalSum > 0xff ? 1 : 0);
        return unchecked((ushort)(wholePosition + wholeDelta));
    }

    private bool CollidesWithRectangle(
        ushort centerX,
        ushort centerY,
        ushort rectangleXRadius,
        ushort rectangleYRadius)
    {
        // `$A9:EF06` adds both radii and one, then rejects `abs(delta) >= sum+1`.
        // In the normal room-coordinate domain that is equivalently `abs(delta) <= sum`.
        int xDistance = Math.Abs(unchecked((short)(centerX - XPosition)));
        if (xDistance > rectangleXRadius + XHitboxRadius)
            return false;
        int yDistance = Math.Abs(unchecked((short)(centerY - YPosition)));
        return yDistance <= rectangleYRadius + YHitboxRadius;
    }

    private void GraduallyAccelerateTowardsPoint(
        ushort targetX,
        ushort targetY,
        ushort accelerationDivisor,
        ushort wrongWayOffScreenXSpeed,
        ushort layer1X,
        ushort layer1Y)
    {
        GraduallyAccelerateHorizontally(
            targetX,
            accelerationDivisor,
            wrongWayOffScreenXSpeed,
            layer1X,
            layer1Y);

        short signedDistance = unchecked((short)(YPosition - targetY));
        if (signedDistance == 0)
            return;
        int acceleration = Math.Max(1, Math.Abs((int)signedDistance) / accelerationDivisor);
        int velocity = unchecked((short)YVelocity);
        if (signedDistance < 0)
        {
            velocity += velocity < 0 ? 8 + acceleration * 2 : acceleration;
            YVelocity = unchecked((ushort)Math.Min(velocity, 0x0500));
        }
        else
        {
            velocity -= velocity >= 0 ? 8 + acceleration * 2 : acceleration;
            YVelocity = unchecked((ushort)Math.Max(velocity, -0x0500));
        }
    }

    private void GraduallyAccelerateHorizontally(
        ushort targetX,
        ushort accelerationDivisor,
        ushort wrongWayOffScreenSpeed,
        ushort layer1X,
        ushort layer1Y)
    {
        short signedDistance = unchecked((short)(XPosition - targetX));
        if (signedDistance == 0)
            return;
        int acceleration = Math.Max(1, Math.Abs((int)signedDistance) / accelerationDivisor);
        int velocity = unchecked((short)XVelocity);
        bool offScreen = IsVaguelyOffScreen(layer1X, layer1Y);

        if (signedDistance < 0)
        {
            if (velocity < 0)
            {
                // The off-screen helper returns carry set. The following ADC therefore
                // adds `$0401`, an easily missed one-unit native asymmetry.
                if (offScreen)
                    velocity += wrongWayOffScreenSpeed + 1;
                velocity += 8 + acceleration * 2;
            }
            else
            {
                velocity += acceleration;
            }
            XVelocity = unchecked((ushort)Math.Min(velocity, 0x0800));
        }
        else
        {
            if (velocity >= 0)
            {
                if (offScreen)
                    velocity -= wrongWayOffScreenSpeed;
                velocity -= 8 + acceleration * 2;
            }
            else
            {
                velocity -= acceleration;
            }
            XVelocity = unchecked((ushort)Math.Max(velocity, -0x0800));
        }
    }

    private bool IsVaguelyOffScreen(ushort layer1X, ushort layer1Y)
    {
        // This is the signed-branch sequence at `$A9:F57A`; its generous rectangle extends
        // 16 pixels left/right and 96 pixels vertically beyond the ordinary 256x224 view.
        if (unchecked((short)YPosition) < 0)
            return true;
        short relativeY = unchecked((short)(YPosition + 0x0060 - layer1Y));
        if (relativeY < 0 || relativeY >= 0x01a0)
            return true;
        if (unchecked((short)XPosition) < 0)
            return true;
        short relativeX = unchecked((short)(XPosition + 0x0010 - layer1X));
        return relativeX < 0 || relativeX >= 0x0120;
    }

    private bool AccelerateTowardsPoint(ushort targetX, ushort targetY, ushort acceleration)
    {
        // `$F5A6` counts axes whose next whole-pixel prediction reaches/crosses target,
        // then shifts the count twice. Carry is set only for count two.
        bool reachedX = AccelerateTowardsXPosition(targetX, acceleration);
        bool reachedY = AccelerateTowardsYPosition(targetY, acceleration);
        return reachedX && reachedY;
    }

    private bool AccelerateTowardsYPosition(ushort targetY, ushort acceleration)
    {
        short difference = unchecked((short)(YPosition - targetY));
        if (difference == 0)
            return true;

        if (difference < 0)
        {
            int velocity = Math.Min(unchecked((short)YVelocity) + acceleration, 0x0500);
            YVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(YPosition + unchecked((sbyte)(YVelocity >> 8))));
            if (unchecked((short)(prediction - targetY)) < 0)
                return false;
        }
        else
        {
            int velocity = Math.Max(unchecked((short)YVelocity) - acceleration, -0x0500);
            YVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(YPosition + unchecked((sbyte)(YVelocity >> 8))));
            short predictedDifference = unchecked((short)(prediction - targetY));
            if (predictedDifference > 0)
                return false;
        }

        YVelocity = 0;
        return true;
    }

    private bool AccelerateTowardsXPosition(ushort targetX, ushort acceleration)
    {
        short difference = unchecked((short)(XPosition - targetX));
        if (difference < 0)
        {
            int velocity = Math.Min(unchecked((short)XVelocity) + acceleration, 0x0500);
            XVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(XPosition + unchecked((sbyte)(XVelocity >> 8))));
            if (unchecked((short)(prediction - targetX)) < 0)
                return false;
        }
        else
        {
            // Native deliberately sends exact equality through the left branch; unlike Y,
            // there is no BEQ before BPL at `$A9:F619-$F61B`.
            int velocity = Math.Max(unchecked((short)XVelocity) - acceleration, -0x0500);
            XVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(XPosition + unchecked((sbyte)(XVelocity >> 8))));
            short predictedDifference = unchecked((short)(prediction - targetX));
            if (predictedDifference > 0)
                return false;
        }

        XVelocity = 0;
        return true;
    }

    private BabyMetroidCutscenePoint Capture() => new(
        XPosition,
        XSubposition,
        YPosition,
        YSubposition);
}
