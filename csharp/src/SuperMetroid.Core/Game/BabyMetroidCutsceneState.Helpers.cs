using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Baby Metroid damage, palette, death, acceleration, collision, and coordinate helpers.
/// </summary>
public sealed partial class BabyMetroidCutsceneState
{
    /// <summary>Applies the Baby's health/flash mutation from the blue-ring collision handler at $86:C381–C3A8: a living Baby gets a $0010 flash timer and health subtraction saturated at zero.</summary>
    /// <param name="damage">Unsigned health units to subtract, defaulting to the native $0050 (80); even zero damage refreshes the flash timer when health is nonzero.</param>
    /// <returns>An empty compatibility result; the effects are written directly to <see cref="Health"/> and <see cref="OnionRingHitFlashTimer"/>.</returns>
    /// <remarks>Zero health leaves all state unchanged. The caller must establish overlap and owns the Baby cry request, physical enemy-slot synchronization, and ring explosion/deletion.</remarks>
    public BabyMetroidOnionRingHitResult ApplyMotherBrainOnionRingHit(ushort damage = 0x0050)
    {
        ushort healthBefore = Health;
        if (healthBefore == 0)
            return new BabyMetroidOnionRingHitResult();

        OnionRingHitFlashTimer = 0x0010;
        Health = healthBefore < damage
            ? (ushort)0
            : unchecked((ushort)(healthBefore - damage));
        return new BabyMetroidOnionRingHitResult();
    }

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
                if (unchecked((short)(Position.Y.Whole + 0x0010 - samus.Position.Y.Whole)) < 0)
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
        ushort xVelocity = Velocity.X.RawValue;
        bool negative = (xVelocity & 0x8000) != 0;
        ushort magnitude = negative ? unchecked((ushort)-xVelocity) : xVelocity;
        magnitude = magnitude >= 0x0020 ? unchecked((ushort)(magnitude - 0x0020)) : (ushort)0;
        Velocity = new(
            new SnesSignedEightEight(negative ? unchecked((ushort)-magnitude) : magnitude),
            new SnesSignedEightEight(unchecked((ushort)(Velocity.Y.RawValue + 2))));
    }

    private bool StepFadeToBlack(ref BabyMetroidPaletteTransferRequest? paletteTransfer)
    {
        // No timer changes occur until the actor's centre reaches Y `$80`.
        if (unchecked((short)(Position.Y.Whole - 0x0080)) < 0)
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
            SourceAddress: unchecked((uint)BabyMetroidCutsceneColorRomData.FadeSource(nextIndex)),
            DestinationColorIndex: BabyMetroidCutsceneColorRomData.DestinationByteIndex,
            ColorCount: BabyMetroidCutsceneColorRomData.FadeColorCount);
        return false;
    }

    private BabyMetroidDeathExplosionRequest? StepDeathExplosion()
    {
        DeathExplosionTimer = unchecked((ushort)(DeathExplosionTimer - 1));
        if ((DeathExplosionTimer & 0x8000) == 0)
            return null;

        DeathExplosionTimer = 4;
        DeathExplosionPatternIndex++;
        if (DeathExplosionPatternIndex >= DeathExplosionScatterDefinitions.Count)
            DeathExplosionPatternIndex = 0;
        // `$A9:CDFC` holds the scatter shared with Ridley's death. The handler increments its
        // index before looking up a pair, so a freshly cleared counter begins at entry one.
        (short x, short y) = DeathExplosionScatterDefinitions.Offset(DeathExplosionPatternIndex);
        return new BabyMetroidDeathExplosionRequest(
            XPosition: unchecked((ushort)(Position.X.Whole + x)),
            YPosition: unchecked((ushort)(Position.Y.Whole + y)),
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
        Velocity = new(
            new SnesSignedEightEight(CalculateVelocityComponent(Speed, angleByte)),
            new SnesSignedEightEight(CalculateVelocityComponent(Speed, unchecked((byte)(angleByte + 0x40)))));
    }

    private static ushort CalculateVelocityComponent(
        ushort speed,
        byte sineIndex)
    {
        // Bank `$86:C27A` multiplies unsigned speed by the absolute signed-table value,
        // returns product bits 8..23, then reapplies the original sign.
        return EnemyTrigonometryTables.MultiplySignedSine(speed, sineIndex);
    }

    private void MoveAccordingToVelocity()
    {
        // SEP #$20 adds velocity's low byte to subposition byte +1. REP #$20 then sign-
        // extends velocity's high byte, retaining the 8-bit ADC carry for whole pixels. That
        // is exactly a 16.16 add of the 8.8 velocity shifted left eight bits: the low
        // subposition byte receives zero and the carry out of the fraction enters the whole.
        Position = Position.Advance(Velocity.ToSixteenSixteen());
    }

    /// <summary>
    /// <c>Enemy_vs_Samus_CollisionHandling</c> ($A0:A07A-$A0FE) for the Baby's radius hitbox.
    /// A non-zero contact-damage index clears Samus's invincibility and proceeds; otherwise an
    /// invincible Samus is not touched. Each axis then requires
    /// <c>|delta| - SamusRadius &lt; enemy radius</c> (a delta inside Samus's own radius
    /// always passes), which is strict, unlike <c>$A9:EF06</c>.
    /// </summary>
    private bool TouchesSamus(SamusState samus)
    {
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
            samus.InvincibilityTimer = 0;
        else if (samus.InvincibilityTimer != 0)
            return false;
        return AxisTouches(samus.Position.X.Whole, Position.X.Whole, samus.Kinematics.Radius.X, XHitboxRadius) &&
            AxisTouches(samus.Position.Y.Whole, Position.Y.Whole, samus.Kinematics.Radius.Y, YHitboxRadius);

        static bool AxisTouches(ushort samusPosition, ushort enemyPosition, ushort samusRadius, ushort enemyRadius)
        {
            int distance = Math.Abs(unchecked((short)(samusPosition - enemyPosition)));
            return distance < samusRadius || distance - samusRadius < enemyRadius;
        }
    }

    private bool CollidesWithRectangle(
        ushort centerX,
        ushort centerY,
        ushort rectangleXRadius,
        ushort rectangleYRadius)
    {
        // `$A9:EF06` adds both radii and one, then rejects `abs(delta) >= sum+1`.
        // In the normal room-coordinate domain that is equivalently `abs(delta) <= sum`.
        int xDistance = Math.Abs(unchecked((short)(centerX - Position.X.Whole)));
        if (xDistance > rectangleXRadius + XHitboxRadius)
            return false;
        int yDistance = Math.Abs(unchecked((short)(centerY - Position.Y.Whole)));
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
        byte divisor = checked((byte)accelerationDivisor);
        Velocity = Velocity.WithX(new SnesSignedEightEight(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            Position.X.Whole, targetX, Velocity.X.RawValue, divisor, wrongWayOffScreenXSpeed,
            () => IsVaguelyOffScreen(layer1X, layer1Y))));
        Velocity = Velocity.WithY(new SnesSignedEightEight(BabyMetroidGradualAcceleration.AccelerateVertically(
            Position.Y.Whole, targetY, Velocity.Y.RawValue, divisor)));
    }

    private bool IsVaguelyOffScreen(ushort layer1X, ushort layer1Y)
    {
        // This is the signed-branch sequence at `$A9:F57A`; its generous rectangle extends
        // 16 pixels left/right and 96 pixels vertically beyond the ordinary 256x224 view.
        if (unchecked((short)Position.Y.Whole) < 0)
            return true;
        short relativeY = unchecked((short)(Position.Y.Whole + 0x0060 - layer1Y));
        if (relativeY is < 0 or >= 0x01a0)
            return true;
        if (unchecked((short)Position.X.Whole) < 0)
            return true;
        short relativeX = unchecked((short)(Position.X.Whole + 0x0010 - layer1X));
        return relativeX is < 0 or >= 0x0120;
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
        short difference = unchecked((short)(Position.Y.Whole - targetY));
        if (difference == 0)
            return true;

        if (difference < 0)
        {
            int velocity = Math.Min(Velocity.Y.SignedRawValue + acceleration, 0x0500);
            Velocity = Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.Y.Whole + unchecked((sbyte)(Velocity.Y.RawValue >> 8))));
            if (unchecked((short)(prediction - targetY)) < 0)
                return false;
        }
        else
        {
            int velocity = Math.Max(Velocity.Y.SignedRawValue - acceleration, -0x0500);
            Velocity = Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.Y.Whole + unchecked((sbyte)(Velocity.Y.RawValue >> 8))));
            short predictedDifference = unchecked((short)(prediction - targetY));
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithY(default);
        return true;
    }

    private bool AccelerateTowardsXPosition(ushort targetX, ushort acceleration)
    {
        short difference = unchecked((short)(Position.X.Whole - targetX));
        if (difference < 0)
        {
            int velocity = Math.Min(Velocity.X.SignedRawValue + acceleration, 0x0500);
            Velocity = Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.X.Whole + unchecked((sbyte)(Velocity.X.RawValue >> 8))));
            if (unchecked((short)(prediction - targetX)) < 0)
                return false;
        }
        else
        {
            // Native deliberately sends exact equality through the left branch; unlike Y,
            // there is no BEQ before BPL at `$A9:F619-$F61B`.
            int velocity = Math.Max(Velocity.X.SignedRawValue - acceleration, -0x0500);
            Velocity = Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.X.Whole + unchecked((sbyte)(Velocity.X.RawValue >> 8))));
            short predictedDifference = unchecked((short)(prediction - targetX));
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithX(default);
        return true;
    }

    private static BabyMetroidCutscenePoint Capture() => new();
}
