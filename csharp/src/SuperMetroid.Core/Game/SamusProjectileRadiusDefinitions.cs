namespace SuperMetroid.Core.Game;

/// <summary>Physical collision extents from bank-$93 timed projectile records, separate from spritemap artwork.</summary>
internal static class SamusProjectileRadiusDefinitions
{
    /// <summary>
    /// Bank-$93 address observed by the cartridge-safe left-facing Murder Beam. Its corrupt
    /// zero instruction pointer indexes the first two bank bytes as X/Y radii.
    /// </summary>
    internal const int MurderBeamRadiusAddress = 0x930004;

    /// <summary>The 805 timed instruction owners, calculated from native program layout.</summary>
    internal static IReadOnlyList<ushort> TimedRecordPointers { get; } = new TimedRecordSequence();

    /// <summary>
    /// Read-only, indexable view of the timed projectile instruction pointers in their
    /// native enumeration order. The view is calculated from instruction definitions.
    /// </summary>
    private sealed class TimedRecordSequence : IReadOnlyList<ushort>
    {
        /// <summary>Gets the number of timed instruction pointers in the calculated view.</summary>
        public int Count => SamusProjectileInstructionDefinitions.EnumerateTimedPointers().Count();

        /// <summary>Gets the timed instruction pointer at its zero-based enumeration position.</summary>
        /// <param name="index">Position in the ordered pointer view.</param>
        /// <value>The instruction pointer stored at <paramref name="index"/>.</value>
        public ushort this[int index]
        {
            get
            {
                if (index < 0) throw new IndexOutOfRangeException();
                foreach (ushort pointer in SamusProjectileInstructionDefinitions.EnumerateTimedPointers())
                    if (index-- == 0) return pointer;
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>Returns an enumerator over the calculated pointers in native definition order.</summary>
        public IEnumerator<ushort> GetEnumerator() => SamusProjectileInstructionDefinitions.EnumerateTimedPointers().GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>$93:86DB..A19C: exact physical hitbox policies. Native 93:8056/805F and8212/821B
    /// publish these dimensions directly; bank 94 block spans and enemy overlap consume them.
    /// Selected sizes/minima/growth caps/rounding express hit reach, independently of artwork.
    /// Direction symmetry, repeated phases and shared lobe geometry calculate without a sample table.</summary>
    internal static bool TryCalculatedPair(SamusProjectileInstructionDefinitions.TimedFrame frame, out ushort pair)
    {
        int axis = frame.Axis % 4;
        int phase = frame.Phase;
        bool diagonal = (axis & 1) != 0;
        int x, y;
        switch (frame.Family)
        {
            case SamusProjectileInstructionDefinitions.FrameFamily.Power:
                x = axis == 0 ? 4 : 8; y = 4; break;
            case SamusProjectileInstructionDefinitions.FrameFamily.Ice:
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedPower:
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedIce:
            case SamusProjectileInstructionDefinitions.FrameFamily.SuperMissile:
            case SamusProjectileInstructionDefinitions.FrameFamily.SuperMissileLink:
            case SamusProjectileInstructionDefinitions.FrameFamily.MissileExplosion:
                x = y = 8; break;
            case SamusProjectileInstructionDefinitions.FrameFamily.Missile:
            case SamusProjectileInstructionDefinitions.FrameFamily.PowerBomb:
            case SamusProjectileInstructionDefinitions.FrameFamily.FastPowerBomb:
            case SamusProjectileInstructionDefinitions.FrameFamily.Bomb:
            case SamusProjectileInstructionDefinitions.FrameFamily.FastBomb:
            case SamusProjectileInstructionDefinitions.FrameFamily.WaveSba:
                x = y = 4; break;
            case SamusProjectileInstructionDefinitions.FrameFamily.BeamExplosion:
            case SamusProjectileInstructionDefinitions.FrameFamily.UnusedExplosion:
                x = y = 0; break;
            case SamusProjectileInstructionDefinitions.FrameFamily.BombExplosion:
            case SamusProjectileInstructionDefinitions.FrameFamily.PlasmaSba:
            case SamusProjectileInstructionDefinitions.FrameFamily.SuperExplosion:
                x = y = Math.Min(16, 8 + 4 * phase); break;
            case SamusProjectileInstructionDefinitions.FrameFamily.UnusedEcho:
                x = 16; y = 32; break;
            case SamusProjectileInstructionDefinitions.FrameFamily.Echo:
                x = y = 32; break;
            case SamusProjectileInstructionDefinitions.FrameFamily.SpazerSba:
                x = 4 + 8 * phase; y = 8; break;
            case SamusProjectileInstructionDefinitions.FrameFamily.Wave:
                if (diagonal)
                    x = y = phase < 2 || phase > 14 ? 8 : 4 + 2 * (4 - Math.Abs(phase % 8 - 4));
                else
                {
                    int across = 12 + 4 * Math.Max(0, 2 - Math.Abs(phase % 8 - 4));
                    x = axis == 0 ? across : 4;
                    y = axis == 0 ? 4 : across;
                }
                break;
            case SamusProjectileInstructionDefinitions.FrameFamily.Spazer:
                if (diagonal) x = y = 8 + 4 * phase;
                else
                {
                    int across = phase < 2 ? 12 : 20;
                    x = axis == 0 ? across : 8;
                    y = axis == 0 ? 8 : across;
                }
                break;
            case SamusProjectileInstructionDefinitions.FrameFamily.SpazerWave when !diagonal:
                {
                    int outward = Math.Clamp(Math.Min(phase - 1, 9 - phase), 0, 4);
                    int across = Math.Max(12, ProjectileWaveEnvelopeDefinitions.SmallLobeOuterEdge(outward));
                    x = axis == 0 ? across : 8;
                    y = axis == 0 ? 8 : across;
                    break;
                }
            case SamusProjectileInstructionDefinitions.FrameFamily.PlasmaWave when !diagonal:
                {
                    int outward = phase == 0 ? 0 : Math.Min(phase - 1, 9 - phase);
                    int across = Math.Max(12, ProjectileWaveEnvelopeDefinitions.SmallLobeOuterEdge(outward));
                    int along = axis == 2 && phase == 0 ? 8 : 16;
                    x = axis == 0 ? across : along;
                    y = axis == 0 ? along : across;
                    break;
                }
            case SamusProjectileInstructionDefinitions.FrameFamily.SpazerWave when diagonal:
                x = y = Math.Clamp(4 * Math.Min(phase + 1, 11 - phase), 8, 16); break;
            case SamusProjectileInstructionDefinitions.FrameFamily.Plasma:
                if (diagonal) x = y = 8;
                else
                {
                    int along = axis == 0 || phase > 0 ? 16 : 8;
                    x = axis == 0 ? 8 : along;
                    y = axis == 0 ? along : 8;
                }
                break;
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedWave:
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedIceWave:
                {
                    int outward = Math.Min(phase / 2, 8 - phase / 2);
                    if (diagonal)
                        x = y = 8 + 4 * Math.Min(outward, 2) + Math.Max(0, outward - 2);
                    else
                    {
                        int across = outward == 0 ? 12 : 8 + ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(outward - 1);
                        // Selected middle-shoulder collision inset: full art extent 21 becomes 20.
                        if (outward == 2) across--;
                        x = axis == 0 ? across : 8;
                        y = axis == 0 ? 8 : across;
                    }
                    break;
                }
            case SamusProjectileInstructionDefinitions.FrameFamily.PlasmaWave when diagonal:
                {
                    int outward = phase == 0 ? 0 : Math.Min(phase - 1, 9 - phase);
                    x = y = outward == 0 ? 8 : ProjectileWaveEnvelopeDefinitions.SmallLobeOuterEdge(outward) / 4 * 4;
                    break;
                }
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedSpazerWave:
                {
                    int pairPhase = phase / 2;
                    if (pairPhase < 2)
                    {
                        x = diagonal ? 8 : axis == 0 ? 12 : 8;
                        y = diagonal ? 8 : axis == 0 ? 8 : 12;
                        break;
                    }
                    int spread = Math.Min(pairPhase - 2, 12 - pairPhase);
                    int edge = spread == 0 ? 4 : 4 + (spread == 1 ?
                        ProjectileWaveEnvelopeDefinitions.SpazerInitialAxialSpread :
                        ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(spread - 2));
                    if (diagonal)
                    {
                        // Selected broad collision envelope during the first three spread poses.
                        x = y = spread <= 2 ? 12 + 4 * Math.Max(0, spread - 1) : edge / 4 * 4;
                    }
                    else
                    {
                        int across = Math.Max(pairPhase < 4 ? 12 : 0, edge / 2 * 2);
                        x = axis == 0 ? across : 16;
                        y = axis == 0 ? 16 : across;
                    }
                    break;
                }
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedPlasmaWave:
                {
                    if (phase < 6)
                    {
                        int growth = phase < 4 ? 8 : 24;
                        if (diagonal) x = y = phase < 4 ? 8 : 12;
                        else { x = axis == 0 ? 12 : growth; y = axis == 0 ? growth : 12; }
                        break;
                    }
                    int outward = Math.Min((phase - 6) / 2, 8 - (phase - 6) / 2);
                    if (diagonal) x = y = ChargedPlasmaDiagonalEnvelope((PlasmaWaveSpreadPose)outward);
                    else
                    {
                        int across = Math.Max(12, ProjectileWaveEnvelopeDefinitions.SmallLobeOuterEdge(outward));
                        int along = axis == 0 ? 30 : 28;
                        x = axis == 0 ? across : along;
                        y = axis == 0 ? along : across;
                    }
                    break;
                }
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedSpazer:
                if (diagonal) x = y = 8 + 4 * Math.Max(0, phase / 2 - 1);
                else
                {
                    int across = phase < 8 ? 12 : 20;
                    int along = phase < 4 ? 8 : 16;
                    x = axis == 0 ? across : along;
                    y = axis == 0 ? along : across;
                }
                break;
            case SamusProjectileInstructionDefinitions.FrameFamily.ChargedPlasma:
                if (diagonal) x = y = 8 + 4 * (phase / 2);
                else
                {
                    int along = Math.Min(28, 8 * (phase / 2 + 1));
                    x = axis == 0 ? 8 : along;
                    y = axis == 0 ? along : 8;
                }
                break;
            default: pair = 0; return false;
        }
        pair = (ushort)(x | y << 8);
        return true;
    }
    /// <summary>$93:9CCF..9D47 and 9E37..9EAF: the five outward long-beam poses,
    /// paired with alternate artwork and then traversed back toward the center.</summary>
    private enum PlasmaWaveSpreadPose
    {
        /// <summary>The initial pose before the diagonal lobes separate.</summary>
        Centered,

        /// <summary>The first pose with the diagonal lobes split from the center.</summary>
        FirstSplit,

        /// <summary>The intermediate outward pose between the initial split and shoulder.</summary>
        Middle,

        /// <summary>The outward pose immediately before the lobes reach their maximum spread.</summary>
        OuterShoulder,

        /// <summary>The maximum outward pose in the spread sequence.</summary>
        FullySpread
    }

    /// <summary>$93:9CD3/9CE3/9CF3/9D03/9D13: selected 12/16/17/20/24 hit reach for the five
    /// semantic diagonal spread poses. The same source sprites have short-axis extents 20/26/29/31/32;
    /// no uniform art-bound or diagonal-projection operation yields these gameplay boxes.
    /// Retain precisely this collision design; phase pairing, reflection and axis sharing calculate.</summary>
    private static int ChargedPlasmaDiagonalEnvelope(PlasmaWaveSpreadPose pose) => pose switch
    {
        PlasmaWaveSpreadPose.Centered => 12,
        PlasmaWaveSpreadPose.FirstSplit => 16,
        PlasmaWaveSpreadPose.Middle => 17,
        PlasmaWaveSpreadPose.OuterShoulder => 20,
        PlasmaWaveSpreadPose.FullySpread => 24,
        _ => throw new IndexOutOfRangeException(),
    };
    /// <summary>
    /// Resolves a bank-$93 timed projectile frame's X or Y collision radius, including the
    /// separate zero-radius operands used by the cartridge-safe Murder Beam.
    /// </summary>
    /// <param name="address">Bank-$93 address of the X-radius or Y-radius byte.</param>
    /// <returns>The calculated collision radius for the addressed component.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a supported radius byte.</exception>
    internal static byte ReadByte(int address)
    {
        if (address is MurderBeamRadiusAddress or MurderBeamRadiusAddress + 1)
            return 0;
        if (SamusProjectileInstructionDefinitions.TryTimedFrame(unchecked(address - 4), out var frame) &&
            TryCalculatedPair(frame, out ushort calculated)) return (byte)calculated;
        if (SamusProjectileInstructionDefinitions.TryTimedFrame(unchecked(address - 5), out frame) &&
            TryCalculatedPair(frame, out calculated)) return (byte)(calculated >> 8);

        throw new InvalidDataException(
            $"Projectile radius byte ${address:X6} is outside the compiled timed-frame definitions.");
    }
}
