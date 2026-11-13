using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Checks the Power Bomb's radii and their speed (#1684) against the original explosion's sixteen-bit arithmetic: plain
/// lengths wherever retail growth cannot reach the word's end, and <see cref="PowerBombGrowthHold"/> where its shape
/// stages refuse a growth that carries.
/// </summary>
internal static class PowerBombRadiusVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>The radius word the original's stages start a radius at (<c>LDA #$0400</c>): four pixels.</summary>
    private const int OriginalStartingRadius = 0x0400;

    /// <summary>The flash's starting speed word (<c>$88:90DB</c>) and its slowing (<c>$88:90DD</c>).</summary>
    private const int OriginalFlashSpeed = 0x3000, OriginalFlashSlowing = 0x0080;

    /// <summary>The blast's starting speed word (<c>$88:8DE5</c>) and its acceleration (<c>$88:8DE7</c>).</summary>
    private const int OriginalBlastSpeed = 0x0000, OriginalBlastAcceleration = 0x0030;

    /// <summary>The radius words that end the flash's white stage, the blast's yellow stage and Crystal Flash's bubble.</summary>
    private const int OriginalWhiteFlashLimit = 0x9200, OriginalYellowBlastLimit = 0x8600, OriginalCrystalFlashLimit = 0x2000;

    /// <summary>The shapes the flash's yellow stage and the blast's white stage walk (the original's shape tables).</summary>
    private const int OriginalYellowShapes = 4, OriginalWhiteShapes = 17;

    /// <summary>
    /// A whole Power Bomb and a whole Crystal Flash, every update from spawn to cleanup: the production explosion's phase,
    /// flash radius, blast radius and speed equal the original stages' sixteen-bit words, which wrap on every addition
    /// and subtraction except the shape stages' growth, which is skipped with its speed change when it carries. No
    /// retail explosion's plain additions wrap, and its shape stages reach the hold.
    /// </summary>
    internal static void VerifyPowerBombExplosionFollowsTheOriginal()
    {
        var explosion = new SamusPowerBombExplosionState { PresentationColors = RetailPresentationFixture().PowerBombGlowColors };
        explosion.Arm();
        explosion.Spawn(new(128, 128));
        var original = new OriginalExplosion();
        int updates = 0;
        bool completed;
        do
        {
            completed = explosion.StepFrame();
            original.StepPowerBomb();
            Compare(explosion, original, $"Power Bomb update {++updates}");
        }
        while (!completed);
        AssertEqual(PowerBombExplosionPhase.Inactive, original.Phase, "the original Power Bomb ends on the same update");
        AssertTrue(original.HeldUpdates == 2 + 3, "the flash holds on two yellow shapes and the blast on three white shapes");

        explosion.BeginCrystalFlash(new(128, 128));
        original.BeginCrystalFlash();
        updates = 0;
        do
        {
            AssertTrue(!explosion.StepFrame(), "Crystal Flash's bubble does not clean up while it grows");
            original.StepCrystalFlash(explosion.Phase);
            Compare(explosion, original, $"Crystal Flash update {++updates}");
        }
        while (explosion.Phase == PowerBombExplosionPhase.CrystalFlashExplosion);
        Console.WriteLine("  Power Bomb radius: every update of a Power Bomb and a Crystal Flash grows as the original's words do.");
    }

    /// <summary>
    /// The hold against the original's carrying sum: every radius with speeds on each side of the carry and at both ends,
    /// and every speed with radii on each side of it, hold exactly when the sixteen-bit sum carries, and otherwise grow
    /// to that sum.
    /// </summary>
    internal static void VerifyPowerBombGrowthHold()
    {
        int[] edges = [0x0000, 0x0001, 0x00ff, 0x0100, 0x2e00, 0x7fff, 0x8000, 0xf000, 0xff00, 0xfffe, 0xffff];
        for (int word = 0; word < WordRange; word++)
        foreach (int edge in edges)
        {
            Check(word, edge);
            Check(edge, word);
            Check(word, WordRange - 1 - word);
            if (word > 0)
                Check(word, WordRange - word);
        }
        Console.WriteLine("  Power Bomb growth hold: every radius and speed holds exactly when the original's sum carries.");
    }

    /// <summary>Checks the growth of the radius word <paramref name="radius"/> by the speed word <paramref name="speed"/>.</summary>
    private static void Check(int radius, int speed)
    {
        PowerBombRadius typedRadius = Length(radius), typedSpeed = Length(speed);
        int sum = radius + speed;
        bool holds = PowerBombGrowthHold.Holds(typedRadius.WholePixelsGrownBy(typedSpeed));
        if (holds != sum > ushort.MaxValue)
            throw new InvalidOperationException($"A radius {typedRadius} growing by {typedSpeed} {(holds ? "holds" : "grows")}, but its sum is {sum:X5}.");
        if (!holds && typedRadius + typedSpeed != Length(sum))
            throw new InvalidOperationException($"A radius {typedRadius} growing by {typedSpeed} reaches {typedRadius + typedSpeed}, not {Length(sum)}.");
    }

    /// <summary>Compares the production explosion's phase, radii and speed with the original's words.</summary>
    private static void Compare(SamusPowerBombExplosionState explosion, OriginalExplosion original, string context)
    {
        AssertEqual(original.Phase, explosion.Phase, $"{context}: phase");
        AssertEqual(Length(original.FlashRadius), explosion.PreExplosionRadius, $"{context}: flash radius");
        AssertEqual(Length(original.BlastRadius), explosion.ExplosionRadius, $"{context}: blast radius");
        AssertEqual(Length(original.Speed), explosion.RadiusSpeed, $"{context}: radius speed");
    }

    /// <summary>The length a radius or speed word stands for: its high byte whole pixels and its low byte 256ths.</summary>
    private static PowerBombRadius Length(int word) => PowerBombRadius.FromPixels(word >> 8 & 0xff, word & 0xff);

    /// <summary>
    /// The original explosion's stages over its sixteen-bit radius and speed words (<c>$88:8B14</c>-<c>$88:8B4E</c>, the
    /// flash's white and yellow stages at <c>$88:90DF</c> and <c>$88:91A8</c>, the blast's yellow and white stages at
    /// <c>$88:8DE9</c> and <c>$88:8EB2</c>, and Crystal Flash's bubble at <c>$88:A552</c>).
    /// </summary>
    private sealed class OriginalExplosion
    {
        /// <summary>The stage the next update runs.</summary>
        internal PowerBombExplosionPhase Phase { get; private set; } = PowerBombExplosionPhase.PendingPreExplosionSetup;

        /// <summary>The flash's radius word.</summary>
        internal int FlashRadius { get; private set; }

        /// <summary>The blast's radius word.</summary>
        internal int BlastRadius { get; private set; }

        /// <summary>The speed word the flash and the blast share.</summary>
        internal int Speed { get; private set; }

        /// <summary>The updates whose growth carried and was skipped.</summary>
        internal int HeldUpdates { get; private set; }

        /// <summary>The shapes walked in the current shape stage.</summary>
        private int _shapes;

        /// <summary>The afterglow steps left.</summary>
        private int _afterglow;

        /// <summary>Runs one update of the Power Bomb's stages.</summary>
        internal void StepPowerBomb()
        {
            switch (Phase)
            {
                case PowerBombExplosionPhase.PendingPreExplosionSetup:
                    FlashRadius = OriginalStartingRadius;
                    Speed = OriginalFlashSpeed;
                    Phase = PowerBombExplosionPhase.PreExplosionWhite;
                    return;
                case PowerBombExplosionPhase.PreExplosionWhite:
                    FlashRadius = (FlashRadius + Speed) & 0xffff;
                    if (FlashRadius < OriginalWhiteFlashLimit)
                    {
                        Speed = (Speed - OriginalFlashSlowing) & 0xffff;
                        return;
                    }
                    _shapes = 0;
                    Phase = PowerBombExplosionPhase.PreExplosionYellow;
                    return;
                case PowerBombExplosionPhase.PreExplosionYellow:
                    FlashRadius = GrowUnlessCarrying(FlashRadius, -OriginalFlashSlowing);
                    if (++_shapes < OriginalYellowShapes)
                        return;
                    BlastRadius = OriginalStartingRadius;
                    Speed = OriginalBlastSpeed;
                    Phase = PowerBombExplosionPhase.ExplosionYellow;
                    return;
                case PowerBombExplosionPhase.ExplosionYellow:
                    BlastRadius = (BlastRadius + Speed) & 0xffff;
                    if (BlastRadius < OriginalYellowBlastLimit)
                    {
                        Speed = (Speed + OriginalBlastAcceleration) & 0xffff;
                        return;
                    }
                    _shapes = 0;
                    Phase = PowerBombExplosionPhase.ExplosionWhite;
                    return;
                case PowerBombExplosionPhase.ExplosionWhite:
                    BlastRadius = GrowUnlessCarrying(BlastRadius, OriginalBlastAcceleration);
                    if (++_shapes < OriginalWhiteShapes)
                        return;
                    _afterglow = 32;
                    Phase = PowerBombExplosionPhase.Afterglow;
                    return;
                case PowerBombExplosionPhase.Afterglow:
                    if (--_afterglow != 0)
                        return;
                    // Cleanup clears both radii and leaves the speed.
                    FlashRadius = 0;
                    BlastRadius = 0;
                    Phase = PowerBombExplosionPhase.Inactive;
                    return;
                case PowerBombExplosionPhase.Inactive:
                case PowerBombExplosionPhase.CrystalFlashExplosion:
                case PowerBombExplosionPhase.CrystalFlashAfterglow:
                default:
                    throw new InvalidOperationException($"The original Power Bomb has no stage {Phase}.");
            }
        }

        /// <summary>Starts Crystal Flash's bubble: both radii at four pixels and no speed.</summary>
        internal void BeginCrystalFlash()
        {
            FlashRadius = OriginalStartingRadius;
            BlastRadius = OriginalStartingRadius;
            Speed = 0;
            Phase = PowerBombExplosionPhase.CrystalFlashExplosion;
        }

        /// <summary>Runs one update of Crystal Flash's bubble; its afterglow changes no radius, so the phase is taken as run.</summary>
        internal void StepCrystalFlash(PowerBombExplosionPhase productionPhase)
        {
            if (Phase != PowerBombExplosionPhase.CrystalFlashExplosion)
                throw new InvalidOperationException($"The original Crystal Flash has no stage {Phase}.");
            BlastRadius = (BlastRadius + Speed) & 0xffff;
            if (BlastRadius < OriginalCrystalFlashLimit)
                Speed = (Speed + OriginalBlastAcceleration) & 0xffff;
            else
                Phase = PowerBombExplosionPhase.CrystalFlashAfterglow;
            if (productionPhase != Phase)
                throw new InvalidOperationException($"Crystal Flash's bubble reached {productionPhase}, the original {Phase}.");
        }

        /// <summary>A shape stage's growth: skipped, with its speed change, when the sum carries.</summary>
        private int GrowUnlessCarrying(int radius, int speedChange)
        {
            int sum = radius + Speed;
            if (sum > 0xffff)
            {
                HeldUpdates++;
                return radius;
            }
            Speed = (Speed + speedChange) & 0xffff;
            return sum;
        }
    }
}
