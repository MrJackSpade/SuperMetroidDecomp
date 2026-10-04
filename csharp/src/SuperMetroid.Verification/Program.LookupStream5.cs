using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream5ActorLayouts(ISnesAddressSpace rom)
    {
        var flight = Enumerable.Range(0, 5).Select(index =>
        {
            var source = CeresFlightActorDefinitions.RearViewPlacementSource(index);
            return new CeresFlightActorPlacement { Id = source.Id,
                X = ReadVerificationWord(rom, 0x8b0000 | source.XAddress),
                Y = ReadVerificationWord(rom, 0x8b0000 | source.YAddress) };
        }).ToArray();
        Check(flight, values =>
        {
            using var encoded = new MemoryStream();
            CeresFlightActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresFlightActorLayout.Load(encoded);
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
        var reveal = Enumerable.Range(0, 6).Select(index =>
        {
            var source = CeresDestructionActorDefinitions.ZebesPlacementSource(index);
            return new CeresRevealActorPlacement { Id = source.Id,
                X = ReadVerificationWord(rom, 0x8b0000 | source.XAddress),
                Y = ReadVerificationWord(rom, 0x8b0000 | source.YAddress) };
        }).ToArray();
        Check(reveal, values =>
        {
            using var encoded = new MemoryStream();
            CeresRevealActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresRevealActorLayout.Load(encoded);
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
        var destruction = Enumerable.Range(0, 3).Select(index =>
        {
            var inherited = flight[index == 0 ? 0 : index == 1 ? 2 : 3];
            return new CeresDestructionActorPlacement { Id = CeresDestructionActorDefinitions.InitialPlacementId(index),
                X = index == 2 ? ReadVerificationWord(rom, 0x8bbfa6) : inherited.X, Y = inherited.Y };
        }).ToArray();
        Check(destruction, values =>
        {
            using var encoded = new MemoryStream();
            CeresDestructionActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresDestructionActorLayout.Load(encoded);
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
        Console.WriteLine("Ceres layouts: all14 original actor placements/28 coordinate operands, no stored stock rows, all28 independent coordinate edits, complete identity preservation and bounds pass.");

        static void Check<T>(T[] original,
            Func<T[], (object Layout, Func<int, T> Read, string Identity)> load,
            Func<T, (string Id, int X, int Y)> fields, Func<T, bool, T> edit)
        {
            var stock = load(original);
            var storage = stock.Layout.GetType().GetField("placements",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            AssertTrue(storage.GetValue(stock.Layout) is null, "Ceres stock placements select semantic actor dispatch");
            Verify(stock, original);
            for (int index = 0; index < original.Length; index++)
            foreach (bool horizontal in new[] { false, true })
            {
                T[] changed = original.ToArray();
                changed[index] = edit(changed[index], horizontal);
                var installed = load(changed);
                AssertTrue(storage.GetValue(installed.Layout) is not null, "Independent Ceres coordinate edit remains supplied");
                Verify(installed, changed);
            }
            AssertThrows<IndexOutOfRangeException>(() => stock.Read(-1), "Ceres layout lower bound");
            AssertThrows<IndexOutOfRangeException>(() => stock.Read(original.Length), "Ceres layout upper bound");

            void Verify((object Layout, Func<int, T> Read, string Identity) actual, T[] expected)
            {
                for (int index = 0; index < expected.Length; index++)
                    AssertEqual(fields(expected[index]), fields(actual.Read(index)), "Ceres complete selected placement");
                string identity = SelectedPresentationHash.Create(actual.Layout.GetType().Name, content =>
                {
                    content.Append("actors", expected.Length);
                    foreach (T placement in expected)
                    {
                        var value = fields(placement);
                        content.Append("id", System.Text.Encoding.UTF8.GetBytes(value.Id));
                        content.Append("x", value.X); content.Append("y", value.Y);
                    }
                });
                AssertEqual(identity, actual.Identity, "Ceres canonical selected content identity");
            }
        }
    }
    private static void VerifyLookupStream5Initialization(ISnesAddressSpace rom)
    {
        for (ushort variant = 0; variant < 7; variant++)
        {
            var actual = CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6f52c + 2 * variant), actual.InstructionList, "Ceres door original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f72b + 2 * variant), actual.MainFunction, "Ceres door original function dispatch");
        }
        for (ushort variant = 0; variant < 6; variant++)
        {
            var actual = CeresSteamDefinitions.Initialization((CeresSteamVariant)variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6eff5 + 2 * variant), actual.InstructionList, "Ceres steam original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f001 + 2 * variant), (ushort)actual.Function, "Ceres steam original function dispatch");
        }
        for (int index = 0; index < 9; index++)
        {
            var actual = MagdollitePhaseDefinitions.Phase(index);
            AssertEqual(ReadVerificationWord(rom, 0xa8af55 + 2 * index), actual.DistanceThreshold, "Magdollite original rise threshold");
            AssertEqual(ReadVerificationWord(rom, 0xa8af67 + 2 * index), actual.BodyInstructionList, "Magdollite original body program");
            AssertEqual(ReadVerificationWord(rom, 0xa8af79 + 2 * index), actual.OverlayYOffset, "Magdollite original overlay offset");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(7), "Ceres door upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(ushort.MaxValue), "Ceres door full-word rejection");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)6), "Ceres steam upper bound");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)ushort.MaxValue), "Ceres steam full-word rejection");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(-1), "Magdollite lower bound");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(9), "Magdollite upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[-1]; }, "Ceres DMA list lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[19]; }, "Ceres DMA list upper bound");
        Console.WriteLine("Stream 5 initialization: all 53 native selector/phase words and rejected domains pass.");
    }
    private static void VerifyLookupStream5PaletteEntries(ISnesAddressSpace rom)
    {
        CheckEntries(CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 3);
        CheckEntries(CinematicGlowPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 4);
        int lightWords = 0, glowWords = 0, redWords = 0, greyWords = 0;
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            ushort pointer = (ushort)address;
            if (CeresCinematicLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort light))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), light, "Ceres light original mechanics");
                lightWords++;
            }
            if (CinematicGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort glow))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), glow, "Cinematic glow original mechanics");
                glowWords++;
            }
            if (TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort red))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), red, "Tourian red-flash original mechanics");
                redWords++;
            }
            if (TourianStatueGreyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort grey))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), grey, "Tourian statue original mechanics");
                greyWords++;
            }
        }
        AssertEqual(44, lightWords, "Ceres light mechanics coverage");
        AssertEqual(64, glowWords, "Cinematic glow mechanics coverage");
        AssertEqual(50, redWords, "Tourian shared red mechanics coverage");
        AssertEqual(31, greyWords, "Tourian statue mechanics coverage");
        Console.WriteLine("Stream 5 palette entries: all eleven entry identities, original controls and collection bounds pass.");

        void CheckEntries<T>(IReadOnlyList<T> entries, Func<T, (ushort Definition, ushort Program)> project, int expectedCount)
        {
            AssertEqual(expectedCount, entries.Count, "Palette entry count");
            int index = 0;
            foreach (T entry in entries)
            {
                var identity = project(entry);
                AssertEqual(identity, project(entries[index]), "Palette entry enumeration order");
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | identity.Definition + 2), identity.Program, "Original palette definition list pointer");
                index++;
            }
            AssertEqual(expectedCount, index, "All palette entries enumerated");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[-1]; }, "Palette entries lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[expectedCount]; }, "Palette entries upper bound");
        }
    }}