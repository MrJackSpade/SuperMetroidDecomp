using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>How each beam, missile and effect sprite is composed: its animation phase or pose and the parts that phase draws.</summary>
public static class ProjectileSpritePartDefinitions
{
    /// <summary>Native order of the eight Wave-beam displacement groups.</summary>
    private enum WaveDirection
    {
        /// <summary>Upward axial offset.</summary>
        Up,
        /// <summary>Downward axial offset.</summary>
        Down,
        /// <summary>Up-right diagonal offset.</summary>
        UpRight,
        /// <summary>Down-left diagonal offset.</summary>
        DownLeft,
        /// <summary>Rightward axial offset.</summary>
        Right,
        /// <summary>Leftward axial offset.</summary>
        Left,
        /// <summary>Up-left diagonal offset.</summary>
        UpLeft,
        /// <summary>Down-right diagonal offset.</summary>
        DownRight,
    }
    /// <summary>$93:A252: Power-beam poses select OBJ tile $30..32 with priority2; those artwork choices remain required, while palette selects the installed CGRAM row.</summary>
    private const int PowerTile = 0x30;

    /// <summary>$93:A252: Power-beam poses select OBJ tile $30..32 with priority2; those artwork choices remain required, while palette selects the installed CGRAM row.</summary>
    internal const int PowerPriority = 2;

    /// <summary>$90:ACDE-ACE8 loads the active beam colors at CGRAM224. OBJ attributes select a sixteen-color row in CGRAM's upper half, so its palette field derives from that destination.</summary>
    internal const int PowerPalette = (SamusProjectileRomData.Palettes.BeamDestinationIndex - CgramPalettes.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;

    /// <summary>$82:E13E-E148 restores9A:81A0 at SpriteP5/CGRAM208. Fixed projectile artwork selects that installed sixteen-color row; its color payload remains required.</summary>
    internal const int FixedProjectilePalette = (GameplayBasePaletteFormat.EnemyProjectileInitialColor - CgramPalettes.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;

    /// <summary>Recognizes one-cell Power or Ice composition records and their cycle phase.</summary>
    internal static bool TrySingleBeamPhase(ushort pointer, out int phase) =>
        TryPhase(pointer, ProjectileSpriteDefinitions.PowerStart, 1, 8, out phase) || TryPhase(pointer, ProjectileSpriteDefinitions.IceStart, 1, 4, out phase);

    /// <summary>Validates alignment and range within a fixed-size composition group.</summary>
    private static bool TryPhase(ushort pointer, ushort start, int parts, int count, out int phase)
    {
        int relative = pointer - start;
        phase = relative / ProjectileSpriteDefinitions.RecordBytes(parts);
        return relative >= 0 && relative % ProjectileSpriteDefinitions.RecordBytes(parts) == 0 && phase < count;
    }

    /// <summary>Recognizes charged Power and Ice quad compositions while reserving Power's independently selected first record.</summary>
    internal static bool TryChargedBeamPhase(ushort pointer, out int phase, out bool ice)
    {
        ice = false;
        if (TryPhase(pointer, ProjectileSpriteDefinitions.ChargedPowerStart, 4, 16, out phase))
            return phase != 0; // EC3E's independently selected ordering remains required.
        ice = true;
        return TryPhase(pointer, ProjectileSpriteDefinitions.ChargedIceStart, 4, 4, out phase);
    }

    /// <summary>$93:ADD5/ADF2/AE0F/AE2C: axial SuperMissile poses, separated by one two-part axial and one three-part diagonal record.</summary>
    internal static bool TryAxialSuperMissilePose(ushort pointer, out int pose)
    {
        int offset = pointer - ProjectileSpriteDefinitions.SuperMissileStart;
        int stride = ProjectileSpriteDefinitions.RecordBytes(2) + ProjectileSpriteDefinitions.RecordBytes(3);
        pose = offset / stride;
        return offset >= 0 && offset < 4 * stride && offset % stride == 0;
    }

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissileHorizontalGlyph = 0x65;

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissileVerticalGlyph = 0x69;

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissilePriority = 2;

    /// <summary>$93:A24D..A27E: eight centered one-tile Power poses (also the first four Ice poses at $93:EDF6) traverse a triangular three-glyph cycle and rotate its horizontal/vertical reflection phases.</summary>
    internal static readonly IReadOnlyList<IReadOnlyList<CompiledSpritePart>> SingleBeamPoses =
        [.. Enumerable.Range(0, 8).Select(phase => (IReadOnlyList<CompiledSpritePart>)[SingleBeamPart(phase)])];

    /// <summary>Selects the cyclic glyph and reflection flags for a pose's one centered beam cell.</summary>
    private static CompiledSpritePart SingleBeamPart(int phase)
    {
        int tile = PowerTile + 2 - Math.Abs(2 - (phase & 3));
        var flips = (phase is >= 3 and <= 5 ? SnesTileFlipFlags.Horizontal : 0) |
            (phase >= 4 ? SnesTileFlipFlags.Vertical : 0);
        return new(SnesSpritemapXWord.Create(-8 / 2, false), SnesSpritemapYByte.Create(-8 / 2),
            SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
    }

    /// <summary>$93:EC43/EDA3: charged Power/Ice use independently selected OBJ glyphs $33/$34, sharing palette6/priority2 with the single-tile beam.</summary>
    internal const int ChargedBeamTile = 0x33;

    /// <summary>Recognizes one of the centered or displaced single-cell Wave composition records.</summary>
    internal static bool TryWavePhase(ushort pointer, out int phase) => TryPhase(pointer, ProjectileSpriteDefinitions.WaveStart, 1, 33, out phase);

    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    internal const int WaveDiagonalNumerator = 3;

    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    internal const int WaveDiagonalDenominator = 4;

    /// <summary>$93:AE65..AF4B: one centered pose then four stages in each of eight signed directional displacement groups.</summary>
    internal static readonly IReadOnlyList<IReadOnlyList<CompiledSpritePart>> WavePoses =
        [.. Enumerable.Range(0, 33).Select(phase => (IReadOnlyList<CompiledSpritePart>)[WavePart(phase)])];

    /// <summary>Places a pose's one Wave glyph at the required axial or diagonal travel distance.</summary>
    private static CompiledSpritePart WavePart(int phase)
    {
        int x = -8 / 2, y = -8 / 2, tile = PowerTile;
        if (phase != 0)
        {
            int step = (phase - 1) % 4;
            WaveDirection direction = ((phase - 1) / 4) switch
            {
                0 => WaveDirection.Up,
                1 => WaveDirection.Down,
                2 => WaveDirection.UpRight,
                3 => WaveDirection.DownLeft,
                4 => WaveDirection.Right,
                5 => WaveDirection.Left,
                6 => WaveDirection.UpLeft,
                7 => WaveDirection.DownRight,
                _ => throw new ArgumentOutOfRangeException(nameof(phase)),
            };
            (int dx, int dy) = direction switch
            {
                WaveDirection.Up => (0, -1),
                WaveDirection.Down => (0, 1),
                WaveDirection.UpRight => (1, -1),
                WaveDirection.DownLeft => (-1, 1),
                WaveDirection.Right => (1, 0),
                WaveDirection.Left => (-1, 0),
                WaveDirection.UpLeft => (-1, -1),
                WaveDirection.DownRight => (1, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(phase)),
            };
            int distance = SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(step);
            if (dx != 0 && dy != 0) distance = distance * WaveDiagonalNumerator / WaveDiagonalDenominator;
            x += dx * distance;
            y += dy * distance;
            tile += (step + 1) / 2;
        }
        return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(y),
            SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, 0), false);
    }

    /// <summary>Recognizes charged Wave or IceWave vertical paired-lobe composition phases.</summary>
    internal static bool TryVerticalChargedWavePhase(ushort pointer, out int phase) =>
        TryVerticalChargedWavePhase(pointer, ProjectileSpriteDefinitions.ChargedWaveStart, out phase) ||
        TryVerticalChargedWavePhase(pointer, ProjectileSpriteDefinitions.ChargedIceWaveStart, out phase);

    /// <summary>Validates a vertical charged-Wave pointer against its two cores and four displaced pairs.</summary>
    private static bool TryVerticalChargedWavePhase(ushort pointer, ushort start, out int phase)
    {
        if (TryPhase(pointer, start, 4, 2, out phase)) return true;
        if (!TryPhase(pointer, (ushort)(start + 2 * ProjectileSpriteDefinitions.RecordBytes(4)), 8, 8, out phase)) return false;
        phase += 2;
        return true;
    }

    /// <summary>Native93:AD6D/AD8A/ADA7/ADC4 andADE1/ADFE/AE1B/AE38: diagonal records follow each two-part axial record.</summary>
    internal static bool TryDiagonalMissilePose(ushort pointer, out int pose, out bool super)
    {
        super = false;
        if (TryDiagonalMissilePose(pointer, ProjectileSpriteDefinitions.MissileStart, out pose)) return true;
        super = true;
        return TryDiagonalMissilePose(pointer, ProjectileSpriteDefinitions.SuperMissileStart, out pose);
    }

    /// <summary>Recognizes a three-part diagonal missile record interleaved with axial poses.</summary>
    private static bool TryDiagonalMissilePose(ushort pointer, ushort start, out int pose)
    {
        int offset = pointer - start - ProjectileSpriteDefinitions.RecordBytes(2);
        int stride = ProjectileSpriteDefinitions.RecordBytes(2) + ProjectileSpriteDefinitions.RecordBytes(3);
        pose = offset / stride;
        return offset >= 0 && offset < 4 * stride && offset % stride == 0;
    }

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int MissileDiagonalX = -8;

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int MissileDiagonalY = -11;

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int SuperMissileDiagonalX = -6;

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int SuperMissileDiagonalY = -10;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int MissileDiagonalGlyph = 0x56;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int SuperMissileDiagonalGlyph = 0x66;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int MissileFirstCorner = 2;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int SuperMissileFirstCorner = 0;

    /// <summary>$93:AD45-AD60: four centered single-cell Bomb poses use consecutive glyphs from4C. Glyph origin/priority remain REQUIRED artwork inputs; palette selects the installed fixed-projectile row.</summary>
    internal const int BombGlyphStart = 0x4c;

    /// <summary>$93:AD45-AD60: four centered single-cell Bomb poses use consecutive glyphs from4C. Glyph origin/priority remain REQUIRED artwork inputs; palette selects the installed fixed-projectile row.</summary>
    internal const int BombPalette = FixedProjectilePalette;

    /// <summary>$93:AD45-AD60: four centered single-cell Bomb poses use consecutive glyphs from4C. Glyph origin/priority remain REQUIRED artwork inputs; palette selects the installed fixed-projectile row.</summary>
    internal const int EffectPriority = 3;

    /// <summary>$93:ABC1-AC18: four mirrored quadrant beam-explosion stages use consecutive glyphs60-63 and the installed beam-color row. Glyph selection/priority/pixels remain REQUIRED.</summary>
    internal const int BeamExplosionQuadGlyphStart = 0x60;

    /// <summary>$93:ABC1-AC18: four mirrored quadrant beam-explosion stages use consecutive glyphs60-63 and the installed beam-color row. Glyph selection/priority/pixels remain REQUIRED.</summary>
    internal const int BeamExplosionPalette = PowerPalette;

    /// <summary>Recognizes the one-cell Bomb cycle or four-quadrant beam-explosion stages.</summary>
    internal static bool TrySimpleEffectPose(ushort pointer, out int phase, out bool quad)
    {
        quad = false;
        if (TryPhase(pointer, ProjectileSpriteDefinitions.BombStart, 1, 4, out phase)) return true;
        quad = true;
        return TryPhase(pointer, ProjectileSpriteDefinitions.BeamExplosionPointer(2), 4, 4, out phase);
    }

    /// <summary>Recognizes a selected horizontal paired-lobe charged-Wave frame.</summary>
    internal static bool TryHorizontalChargedWavePhase(ushort pointer, out int phase) =>
        TryPhase(pointer, ProjectileSpriteDefinitions.ChargedWavePointer(ProjectileSpriteDefinitions.ChargedWaveStart, 26), 8, 8, out phase) ||
        TryPhase(pointer, ProjectileSpriteDefinitions.ChargedWavePointer(ProjectileSpriteDefinitions.ChargedIceWaveStart, 26), 8, 8, out phase);

    /// <summary>Recognizes one of eight ordinary Spazer seed records before its spread frames.</summary>
    internal static bool TrySpazerSeedPose(ushort pointer, out int pose)
    {
        int offset = pointer - ProjectileSpriteDefinitions.SpazerStart;
        int diagonalGroupBytes = ProjectileSpriteDefinitions.SpreadGroupBytes(4);
        if (offset >= 0 && offset < 4 * diagonalGroupBytes && offset % diagonalGroupBytes == 0)
        {
            pose = offset / diagonalGroupBytes;
            return true;
        }
        offset -= 4 * diagonalGroupBytes;
        int axialGroupBytes = ProjectileSpriteDefinitions.SpreadGroupBytes(2);
        pose = 4 + offset / axialGroupBytes;
        return offset >= 0 && offset < 4 * axialGroupBytes && offset % axialGroupBytes == 0;
    }

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerDiagonalGlyph = 0x32;

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerVerticalGlyph = 0x33;

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerHorizontalGlyph = 0x30;

    /// <summary>Recognizes three-lane diagonal Spazer records and their native spread phase.</summary>
    internal static bool TrySpazerDiagonalSpread(ushort pointer, out int pose, out int phase)
    {
        int offset = pointer - ProjectileSpriteDefinitions.SpazerStart;
        int groupBytes = ProjectileSpriteDefinitions.SpreadGroupBytes(4);
        pose = offset / groupBytes;
        int withinGroup = offset % groupBytes - ProjectileSpriteDefinitions.RecordBytes(4);
        phase = withinGroup / ProjectileSpriteDefinitions.RecordBytes(12);
        return offset >= 0 && offset < 4 * groupBytes && withinGroup >= 0 &&
            withinGroup < 4 * ProjectileSpriteDefinitions.RecordBytes(12) && withinGroup % ProjectileSpriteDefinitions.RecordBytes(12) == 0;
    }

    /// <summary>$93:D64A/D6F6/D7A2/D84E: axial spread records follow their two-cell seeds. D84E's distinct flip/order policy remains required.</summary>
    internal static bool TrySpazerAxialSpread(ushort pointer, out int pose, out int phase)
    {
        int offset = pointer - ProjectileSpriteDefinitions.SpazerStart - 4 * ProjectileSpriteDefinitions.SpreadGroupBytes(4);
        int groupBytes = ProjectileSpriteDefinitions.SpreadGroupBytes(2);
        pose = 4 + offset / groupBytes;
        int withinGroup = offset % groupBytes - ProjectileSpriteDefinitions.RecordBytes(2);
        phase = withinGroup / ProjectileSpriteDefinitions.RecordBytes(6);
        return offset >= 0 && offset < 4 * groupBytes && withinGroup >= 0 &&
            withinGroup < 5 * ProjectileSpriteDefinitions.RecordBytes(6) && withinGroup % ProjectileSpriteDefinitions.RecordBytes(6) == 0 &&
            !(pose == 7 && phase == 0);
    }

    /// <summary>$93:D8EE..DA39: horizontal charged Spazer seed and five spread records, ending before the vertical family.</summary>
    internal static bool TryHorizontalChargedSpazer(ushort pointer, out int phase)
    {
        if (pointer == ProjectileSpriteDefinitions.ChargedSpazerStart) { phase = 0; return true; }
        if (!TryPhase(pointer, (ushort)(ProjectileSpriteDefinitions.ChargedSpazerStart + ProjectileSpriteDefinitions.RecordBytes(4)), 12, 5, out phase)) return false;
        phase++;
        return true;
    }

    /// <summary>$93:D8EE: the chosen four-cell length and tile34 artwork remain REQUIRED inputs; cell adjacency and centering calculate.</summary>
    internal const int ChargedSpazerHorizontalCells = 4;

    /// <summary>$93:D8EE: the chosen four-cell length and tile34 artwork remain REQUIRED inputs; cell adjacency and centering calculate.</summary>
    internal const int ChargedSpazerHorizontalGlyph = 0x34;

    /// <summary>$93:DA8E..DB85: later vertical charged Spazer spreads follow the vertical seed and its first spread. Initial DA3A/DA50 ordering remains required.</summary>
    internal static bool TryVerticalChargedSpazerSpread(ushort pointer, out int phase) =>
        TryPhase(pointer, (ushort)(ProjectileSpriteDefinitions.ChargedSpazerStart + ProjectileSpriteDefinitions.SpreadGroupBytes(4) + ProjectileSpriteDefinitions.RecordBytes(4) + ProjectileSpriteDefinitions.RecordBytes(12)), 12, 4, out phase);

    /// <summary>$93:DA8E: the selected four-cell column and glyph37 remain REQUIRED artwork inputs.</summary>
    internal const int ChargedSpazerVerticalCells = 4;

    /// <summary>$93:DA8E: the selected four-cell column and glyph37 remain REQUIRED artwork inputs.</summary>
    internal const int ChargedSpazerVerticalGlyph = 0x37;

    /// <summary>$93:EE12..F085: each startup orientation group begins with a one-cell and two-cell axial strip, before its diagonal records.</summary>
    internal static bool TrySpazerAxialStartup(ushort pointer, out int group, out int length)
    {
        int groupBytes = 7 * sizeof(ushort) + 5 * (4 * 5 / 2 + 2 * (3 * 4 / 2));
        int offset = pointer - ProjectileSpriteDefinitions.SpazerStartupStart;
        group = offset / groupBytes;
        int within = offset % groupBytes;
        length = within == 0 ? 1 : 2;
        return offset >= 0 && group < 6 && (within == 0 || within == ProjectileSpriteDefinitions.RecordBytes(1));
    }

    /// <summary>$93:EE4C/EE58,EEC8/EED4,EF44/EF50: ordinary diagonal startup strips select one or two tile pairs after four physical axial records.</summary>
    internal static bool TrySpazerDiagonalStartup(ushort pointer, out int group, out int pairs)
    {
        int groupBytes = 7 * sizeof(ushort) + 5 * (4 * 5 / 2 + 2 * (3 * 4 / 2));
        int offset = pointer - ProjectileSpriteDefinitions.SpazerStartupStart;
        group = offset / groupBytes;
        int within = offset % groupBytes - (4 * sizeof(ushort) + 5 * 4 * 5 / 2);
        pairs = within == 0 ? 1 : 2;
        return offset >= 0 && group < 3 && (within == 0 || within == ProjectileSpriteDefinitions.RecordBytes(2));
    }

    /// <summary>Recognizes one of the charged Plasma startup cores across axial and diagonal orientations.</summary>
    internal static bool TryPlasmaStartupCore(ushort pointer, out int group)
    {
        for (group = 0; group < 8; group++)
            if (pointer == ProjectileSpriteDefinitions.PlasmaStartupPointer(group * 4)) return true;
        return false;
    }

    /// <summary>$93:F03C/F048/F0B8/F0C4: alternate diagonal Spazer startup pair and expanded two-endcap poses.</summary>
    internal static bool TryAlternateDiagonalStartup(ushort pointer, out bool reflected, out int phase)
    {
        for (int variant = 0; variant < 2; variant++)
        {
            SpazerStartupShape shape = variant == 0 ? SpazerStartupShape.FallingAlternate : SpazerStartupShape.RisingAlternate;
            for (phase = 0; phase < 2; phase++)
                if (pointer == ProjectileSpriteDefinitions.SpazerStartupPointer((int)shape * 2 + phase))
                {
                    reflected = shape == SpazerStartupShape.RisingAlternate;
                    return true;
                }
        }
        reflected = false;
        phase = 0;
        return false;
    }

    /// <summary>$93:F048: chosen inner/endcap glyphs35/36 remain REQUIRED artwork inputs.</summary>
    internal const int SpazerDiagonalInnerGlyph = 0x35;

    /// <summary>$93:F048: chosen inner/endcap glyphs35/36 remain REQUIRED artwork inputs.</summary>
    internal const int SpazerDiagonalEndcapGlyph = 0x36;

    /// <summary>$93:BC0A..BCC7: horizontal PlasmaWave Short core and four two-lobe spreads.</summary>
    internal static bool TryHorizontalPlasmaWaveShort(ushort pointer, out int phase)
    {
        if (pointer == ProjectileSpriteDefinitions.PlasmaWaveHorizontalShort) { phase = 0; return true; }
        if (!TryPhase(pointer, (ushort)(ProjectileSpriteDefinitions.PlasmaWaveHorizontalShort + ProjectileSpriteDefinitions.RecordBytes(4)), 8, 4, out phase)) return false;
        phase++;
        return true;
    }

    /// <summary>$93:BC0A: the chosen four-cell short Plasma footprint remains REQUIRED artwork input.</summary>
    internal const int PlasmaWaveShortCells = 4;
}
