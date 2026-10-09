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

    /// <summary>Advances Samus's rainbow palette phase when the Baby crosses its activation height.</summary>
    /// <param name="samus">Player state whose drained-color animation is enabled or advanced.</param>
    /// <param name="samusRainbowActivated">Set to <see langword="true"/> when this step starts the rainbow effect.</param>
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

    /// <summary>Damps horizontal velocity and increases downward velocity using the death-flight update.</summary>
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

    /// <summary>Advances the palette fade after reaching its height threshold and emits a transfer when needed.</summary>
    /// <param name="paletteTransfer">Receives the next CGRAM transfer request, if this step advances a fade color.</param>
    /// <returns><see langword="true"/> when all fade steps are complete.</returns>
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
            SourceAddress: unchecked((uint)BabyMetroidCutsceneColorRomData.FadeSource(nextIndex)),
            DestinationColorIndex: BabyMetroidCutsceneColorRomData.DestinationByteIndex,
            ColorCount: BabyMetroidCutsceneColorRomData.FadeColorCount);
        return false;
    }

    /// <summary>Ticks the death-explosion cadence and returns the next scatter request when it expires.</summary>
    /// <returns>The next room explosion request, or <see langword="null"/> while its timer remains active.</returns>
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
            XPosition: unchecked((ushort)(XPosition + x)),
            YPosition: unchecked((ushort)(YPosition + y)),
            ProjectileParameter: 3,
            SoundEffect: 0x0013);
    }

    /// <summary>Installs an instruction-list pointer and initializes its timer and loop counter.</summary>
    /// <param name="pointer">Address of the next Baby Metroid instruction list.</param>
    private void SetInstructionList(ushort pointer)
    {
        InstructionList = pointer;
        InstructionTimer = 1;
        InstructionLoopCounter = 0;
    }

    /// <summary>Moves speed and angle toward their targets, then recalculates signed velocity components.</summary>
    /// <param name="bus">SNES address space used by the cartridge trigonometry tables.</param>
    /// <param name="angleDelta">Signed angular step applied before clamping at the target angle.</param>
    /// <param name="targetAngle">Angle toward which the Baby's heading advances.</param>
    /// <param name="targetSpeed">Speed approached in fixed increments.</param>
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
        XVelocity = CalculateVelocityComponent(Speed, angleByte);
        YVelocity = CalculateVelocityComponent(Speed, unchecked((byte)(angleByte + 0x40)));
    }

    /// <summary>Computes one signed velocity component from speed and a byte-indexed sine sample.</summary>
    /// <param name="speed">Unsigned movement speed supplied to the native multiply.</param>
    /// <param name="sineIndex">Angle-table byte used to select the signed sine value.</param>
    /// <returns>The signed 8.8 component encoded in a 16-bit word.</returns>
    private static ushort CalculateVelocityComponent(
        ushort speed,
        byte sineIndex)
    {
        // Bank `$86:C27A` multiplies unsigned speed by the absolute signed-table value,
        // returns product bits 8..23, then reapplies the original sign.
        return EnemyTrigonometryTables.MultiplySignedSine(speed, sineIndex);
    }

    /// <summary>Integrates both 8.8 velocity words into the Baby's whole and subpixel positions.</summary>
    private void MoveAccordingToVelocity()
    {
        XPosition = AddNativeEightEightVelocity(XPosition, XSubposition, XVelocity, out ushort xSubposition);
        XSubposition = xSubposition;
        YPosition = AddNativeEightEightVelocity(YPosition, YSubposition, YVelocity, out ushort ySubposition);
        YSubposition = ySubposition;
    }

    /// <summary>Adds a signed 8.8 velocity to whole-pixel position while preserving native fractional carry.</summary>
    /// <param name="wholePosition">Current whole-pixel coordinate.</param>
    /// <param name="subposition">Current fractional coordinate word.</param>
    /// <param name="velocity">Signed 8.8 velocity word.</param>
    /// <param name="newSubposition">Receives the updated fractional coordinate.</param>
    /// <returns>The updated whole-pixel coordinate with 16-bit wraparound.</returns>
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

    /// <summary>Tests overlap between the Baby's hitbox and an axis-aligned rectangle.</summary>
    /// <param name="centerX">Horizontal center of the tested rectangle.</param>
    /// <param name="centerY">Vertical center of the tested rectangle.</param>
    /// <param name="rectangleXRadius">Horizontal half-size of the rectangle.</param>
    /// <param name="rectangleYRadius">Vertical half-size of the rectangle.</param>
    /// <returns><see langword="true"/> when the two hitboxes overlap on both axes.</returns>
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

    /// <summary>Applies divided horizontal and vertical acceleration toward a target point.</summary>
    /// <param name="targetX">Target room-space horizontal coordinate.</param>
    /// <param name="targetY">Target room-space vertical coordinate.</param>
    /// <param name="accelerationDivisor">Byte-sized divisor controlling the gradual acceleration steps.</param>
    /// <param name="wrongWayOffScreenXSpeed">Horizontal speed used when moving the wrong way off screen.</param>
    /// <param name="layer1X">Horizontal camera origin used by the off-screen rule.</param>
    /// <param name="layer1Y">Vertical camera origin used by the off-screen rule.</param>
    private void GraduallyAccelerateTowardsPoint(
        ushort targetX,
        ushort targetY,
        ushort accelerationDivisor,
        ushort wrongWayOffScreenXSpeed,
        ushort layer1X,
        ushort layer1Y)
    {
        byte divisor = checked((byte)accelerationDivisor);
        XVelocity = BabyMetroidGradualAcceleration.AccelerateHorizontally(
            XPosition, targetX, XVelocity, divisor, wrongWayOffScreenXSpeed,
            () => IsVaguelyOffScreen(layer1X, layer1Y));
        YVelocity = BabyMetroidGradualAcceleration.AccelerateVertically(
            YPosition, targetY, YVelocity, divisor);
    }

    /// <summary>Applies the Baby's generous native bounds check against the current room camera.</summary>
    /// <param name="layer1X">Horizontal origin of layer one.</param>
    /// <param name="layer1Y">Vertical origin of layer one.</param>
    /// <returns><see langword="true"/> when the Baby lies outside the expanded gameplay viewport.</returns>
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

    /// <summary>Accelerates each axis toward a point and reports when both predicted positions reach it.</summary>
    /// <param name="targetX">Target room-space horizontal coordinate.</param>
    /// <param name="targetY">Target room-space vertical coordinate.</param>
    /// <param name="acceleration">Signed velocity increment applied independently on each axis.</param>
    /// <returns><see langword="true"/> once both axes have reached or crossed their targets.</returns>
    private bool AccelerateTowardsPoint(ushort targetX, ushort targetY, ushort acceleration)
    {
        // `$F5A6` counts axes whose next whole-pixel prediction reaches/crosses target,
        // then shifts the count twice. Carry is set only for count two.
        bool reachedX = AccelerateTowardsXPosition(targetX, acceleration);
        bool reachedY = AccelerateTowardsYPosition(targetY, acceleration);
        return reachedX && reachedY;
    }

    /// <summary>Adjusts vertical velocity toward a target and clamps it to zero at the crossing.</summary>
    /// <param name="targetY">Target room-space vertical coordinate.</param>
    /// <param name="acceleration">Signed velocity increment applied toward the target.</param>
    /// <returns><see langword="true"/> when the next whole-pixel movement reaches or crosses the target.</returns>
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

    /// <summary>Adjusts horizontal velocity toward a target and clamps it to zero at the crossing.</summary>
    /// <param name="targetX">Target room-space horizontal coordinate.</param>
    /// <param name="acceleration">Signed velocity increment applied toward the target.</param>
    /// <returns><see langword="true"/> when the next whole-pixel movement reaches or crosses the target.</returns>
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

    /// <summary>Creates the empty point marker used to identify a cutscene update boundary.</summary>
    private static BabyMetroidCutscenePoint Capture() => new();
}
