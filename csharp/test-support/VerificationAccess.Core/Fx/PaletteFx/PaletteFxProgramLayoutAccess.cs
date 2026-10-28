namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>A native timed record as the ROM oracles read it: its timer word and the colour words that follow.</summary>
/// <param name="InstructionPointer">The bank-$8D address of the record's timer word.</param>
/// <param name="ColorCount">The colour words that follow it.</param>
internal readonly record struct PaletteFxRecordView(ushort InstructionPointer, int ColorCount)
{
    /// <summary>The record's timer word.</summary>
    internal ushort Pointer => InstructionPointer;

    /// <summary>The first colour word.</summary>
    internal ushort FirstColorPointer => checked((ushort)(InstructionPointer + sizeof(ushort)));

    /// <summary>The terminal wait command after the colours.</summary>
    internal ushort WaitInstructionPointer => checked((ushort)(FirstColorPointer + ColorCount * sizeof(ushort)));
}

/// <summary>
/// Verification's native view of palette-FX program structure that the records do not carry: list entries, loop
/// commands and timer instructions, by the formulas of the native lists.
/// </summary>
internal static class PaletteFxProgramLayoutAccess
{
    /// <summary>The records of <paramref name="rows"/> as native views.</summary>
    internal static IReadOnlyList<PaletteFxRecordView> Records(PaletteFxColorRows rows) =>
        [.. Enumerable.Range(0, rows.FrameCount).Select(frame => new PaletteFxRecordView(
            PaletteFxColorLayoutAccess.RecordAddress(rows.Frame(frame)), rows.ColorsPerFrame))];

    /// <summary>Heat programs' native records.</summary>
    extension(PaletteFxHeatProgramDefinition self)
    {
        /// <summary>The sixteen phase records.</summary>
        internal IReadOnlyList<PaletteFxRecordView> Frames => Records(self.Rows);
    }

    /// <summary>Heat program selection by address.</summary>
    extension(PaletteFxHeatInstructionListDefinitions)
    {
        /// <summary>The bank-$8D record <paramref name="suit"/>'s program shows at <paramref name="phase"/>.</summary>
        internal static ushort Resolve(PaletteFxHeatSuit suit, ushort phase) =>
            PaletteFxColorLayoutAccess.RecordAddress(SuperMetroid.Core.Fx.Artwork.HeatPaletteColors.Rows(suit).Frame(phase));
    }

    /// <summary>Crateria lightning's native list structure.</summary>
    extension(CrateriaLightningPaletteFxColorDefinition self)
    {
        /// <summary>The program's timed records.</summary>
        internal IReadOnlyList<PaletteFxRecordView> Frames => Records(self.Rows);

        /// <summary>The list entry: <c>$8D:EB3B</c> for surface lightning, <c>$8D:EC6E</c> for dark lightning.</summary>
        internal ushort ProgramStart => self.Owner == CrateriaLightningPaletteOwner.SurfaceLightning ? (ushort)0xeb3b : (ushort)0xec6e;

        /// <summary>The first loop's timer instruction after the opening neutral record.</summary>
        internal ushort TimerTwoPointer => checked((ushort)(self.FirstFramePointer + 2 * (self.ColorsPerFrame + 2)));

        /// <summary>The final group's timer instruction after the neutral records.</summary>
        internal ushort TimerOnePointer
        {
            get
            {
                int frameBytes = 2 * (self.ColorsPerFrame + 2);
                int neutral = self.TimerTwoPointer + 3 + 7 * frameBytes + 4;
                return checked((ushort)(neutral + self.NeutralCount * frameBytes));
            }
        }
    }

    /// <summary>Norfair's native list structure.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>The list entry: <c>$8D:F08E</c> for the heat phase, then 264-byte palette programs from <c>$8D:F1D1</c>.</summary>
        internal ushort ProgramStart => self.Owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase
            ? (ushort)0xf08e
            : checked((ushort)(0xf1d1 + ((int)self.Owner - (int)NorfairEnvironmentalPaletteOwner.ForegroundPalette4) * 264));

        /// <summary>The first record's first command, which the loop returns to: the heat publication in the heat-phase program.</summary>
        internal ushort FirstRecordPointer => checked((ushort)(self.FirstFramePointer - self.HeatPublicationByteCount));

        /// <summary>The loop command after the sixteen records.</summary>
        internal ushort LoopInstructionPointer => checked((ushort)(self.FirstRecordPointer + NorfairEnvironmentalPaletteFxColorDefinitionsConstants.FrameCount * self.FrameByteCount));
    }

    /// <summary>The old Tourian accents' native list entries.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinition self)
    {
        /// <summary><c>$8D:FBC1</c> for the railings, <c>$8D:FC5F</c> for the panels.</summary>
        internal ushort ProgramStart => self.Owner == OldTourianEscapeAccentPaletteOwner.OrangeRailings ? (ushort)0xfbc1 : (ushort)0xfc5f;
    }

    /// <summary>PLANET ZEBES text's native list entries.</summary>
    extension(PlanetZebesTextPaletteFxColorDefinition self)
    {
        /// <summary><c>$8D:C90E</c> for the fade-in, <c>$8D:C964</c> for the fade-out.</summary>
        internal ushort ProgramStart => self.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn ? (ushort)0xc90e : (ushort)0xc964;
    }

    /// <summary>Maridia's native list structure: three programs in a row, each entry four bytes before its first record.</summary>
    extension(MaridiaEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>The list entry.</summary>
        internal ushort ProgramStart => checked((ushort)(self.FirstFramePointer - 2 * sizeof(ushort)));

        /// <summary>The loop command after the records.</summary>
        internal ushort LoopInstructionPointer => checked((ushort)(self.FirstFramePointer + self.FrameCount * self.FrameByteCount));
    }

    /// <summary>The title ambient programs' native list structure.</summary>
    extension(TitleScreenAmbientPaletteFxColorDefinition self)
    {
        /// <summary>The list entry, four bytes before the first record.</summary>
        internal ushort ProgramStart => checked((ushort)(self.FirstFramePointer - 4));

        /// <summary>The loop command after the records.</summary>
        internal ushort LoopInstructionPointer => checked((ushort)(self.FirstFramePointer + self.FrameCount * self.FrameByteCount));
    }

    /// <summary>The cinematic glows' native list entries.</summary>
    extension(CinematicGlowPaletteFxColorDefinition self)
    {
        /// <summary><c>$8D:C9BA</c> for the old Mother Brain lights, <c>$8D:CA4E</c> for the gunship.</summary>
        internal ushort ProgramStart => self.Owner == CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights ? (ushort)0xc9ba : (ushort)0xca4e;
    }
}

/// <summary>The Ceres cinematic light sets' native records.</summary>
internal static class CeresCinematicLightPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>Record and colour addresses of the gunship engine and navigation lights.</summary>
    extension(CeresCinematicLightPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the gunship engine's first record.</summary>
        internal static ushort GunshipEngineFirstFramePointer => GunshipEngineFramePointer(0);

        /// <summary>The bank-$8D address of gunship engine record <paramref name="frame"/>.</summary>
        internal static ushort GunshipEngineFramePointer(int frame) =>
            PaletteFxColorLayoutAccess.RecordAddress(CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineRows.Frame(frame));

        /// <summary>The bank-$8D address of gunship engine colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort GunshipEngineColorPointer(int frame, int color) =>
            PaletteFxColorLayoutAccess.ColorAddress(CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineRows.Color(frame, color));

        /// <summary>The bank-$8D address of the navigation lights' first record.</summary>
        internal static ushort NavigationLightsFirstFramePointer => NavigationLightsFramePointer(0);

        /// <summary>The bank-$8D address of navigation lights record <paramref name="frame"/>.</summary>
        internal static ushort NavigationLightsFramePointer(int frame) =>
            PaletteFxColorLayoutAccess.RecordAddress(CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsRows.Frame(frame));

        /// <summary>The bank-$8D address of navigation lights colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort NavigationLightsColorPointer(int frame, int color) =>
            PaletteFxColorLayoutAccess.ColorAddress(CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsRows.Color(frame, color));
    }
}

/// <summary>The Zebes explosion foreground's native list entry.</summary>
internal static class ZebesExplosionForegroundPaletteFxColorDefinitionsLayoutConstants
{
    /// <summary>The entry the definition selects.</summary>
    extension(ZebesExplosionForegroundPaletteFxColorDefinitions)
    {
        /// <summary><c>$8D:CB3C</c>, four bytes before the first record.</summary>
        internal static ushort ProgramStart => 0xcb3c;
    }
}
