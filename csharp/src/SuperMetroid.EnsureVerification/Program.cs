using SuperMetroid.Core;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using static SuperMetroid.EnsureVerification.Contracts.MemberDocumentationAnalyzerTests;
using static SuperMetroid.EnsureVerification.CartridgeEncoding.CartridgeEncodingAnalyzerTests;
using static SuperMetroid.EnsureVerification.Hardware.CpuBusCodeFixTests;
using static SuperMetroid.EnsureVerification.Hardware.CpuBusDeclarationAnalyzerTests;
using static SuperMetroid.EnsureVerification.Contracts.EnsureCodeFixTests;
using static SuperMetroid.EnsureVerification.Contracts.OwnedReflectionAnalyzerTests;
using static SuperMetroid.EnsureVerification.PaletteFx.PaletteFxContractTests;
using static SuperMetroid.EnsureVerification.Contracts.ParameterContractsTests;
using static SuperMetroid.EnsureVerification.Contracts.ParameterListAnalyzerTests;
using static SuperMetroid.EnsureVerification.Contracts.NullForgivingAnalyzerTests;
using static SuperMetroid.EnsureVerification.Contracts.AdapterChainAnalyzerTests;
using static SuperMetroid.EnsureVerification.SourceLayout.OneTypePerFileAnalyzerTests;
using static SuperMetroid.EnsureVerification.SourceLayout.PartialTypeAnalyzerTests;
using static SuperMetroid.EnsureVerification.Hardware.WordReadAnalyzerTests;
using SuperMetroid.EnsureVerification.Contracts;
using SuperMetroid.EnsureVerification.Harness;
using SuperMetroid.EnsureVerification.Modules;

namespace SuperMetroid.EnsureVerification;

/// <summary>Runs focused Ensure API, analyzer, code-fix, and palette FX contract checks.</summary>
internal static class Program
{
    /// <summary>Installs the no-dialog process boundary and runs the requested focused verification.</summary>
    private static int Main(string[] args)
    {
        try
        {
            if (OperatingSystem.IsWindows())
                _ = NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);
            if (args is ["--palette-fx-contract"])
            {
                VerifyPaletteFxDependencyContract();
                return 0;
            }
            if (args is ["--member-documentation"])
            {
                VerifyMemberDocumentationAnalyzer();
                return 0;
            }
            // Every suite runs even after one fails, so a single run reports every failure in full.
            AnalyzerSuite[] suites =
            [
                new AnalyzerSuite(nameof(VerifyEnsure), VerifyEnsure),
                new AnalyzerSuite(nameof(VerifyAnalyzer), VerifyAnalyzer),
                new AnalyzerSuite(nameof(VerifyMemberDocumentationAnalyzer), VerifyMemberDocumentationAnalyzer),
                new AnalyzerSuite(nameof(VerifyPrimitiveDomainAnalyzer), VerifyPrimitiveDomainAnalyzer),
                new AnalyzerSuite(nameof(VerifySpatialClumpAnalyzer), VerifySpatialClumpAnalyzer),
                new AnalyzerSuite(nameof(VerifyCpuBusAccessAnalyzer), VerifyCpuBusAccessAnalyzer),
                new AnalyzerSuite(nameof(VerifyCpuBusAccessCodeFix), VerifyCpuBusAccessCodeFix),
                new AnalyzerSuite(nameof(VerifyCpuBusDeclarationAnalyzer), VerifyCpuBusDeclarationAnalyzer),
                new AnalyzerSuite(nameof(VerifyCartridgeEncodingAnalyzer), VerifyCartridgeEncodingAnalyzer),
                new AnalyzerSuite(nameof(VerifyPpuLayoutAccessAnalyzer), VerifyPpuLayoutAccessAnalyzer),
                new AnalyzerSuite(nameof(VerifyFrontendPpuMemoryAnalyzer), VerifyFrontendPpuMemoryAnalyzer),
                new AnalyzerSuite(nameof(VerifyPlmSlotAllocationAnalyzer), VerifyPlmSlotAllocationAnalyzer),
                new AnalyzerSuite(nameof(VerifyLooseCommentAnalyzer), VerifyLooseCommentAnalyzer),
                new AnalyzerSuite(nameof(VerifyPragmaWarningDisableAnalyzer), VerifyPragmaWarningDisableAnalyzer),
                new AnalyzerSuite(nameof(VerifyNumberedSequenceAnalyzer), VerifyNumberedSequenceAnalyzer),
                new AnalyzerSuite(nameof(VerifyMirroredMethodPairAnalyzer), VerifyMirroredMethodPairAnalyzer),
                new AnalyzerSuite(nameof(VerifyStructuralCloneAnalyzer), VerifyStructuralCloneAnalyzer),
                new AnalyzerSuite(nameof(VerifyIntegerCodedModeAnalyzer), VerifyIntegerCodedModeAnalyzer),
                new AnalyzerSuite(nameof(VerifyBoolFlagAnalyzer), VerifyBoolFlagAnalyzer),
                new AnalyzerSuite(nameof(VerifyForwardingContractAnalyzer), VerifyForwardingContractAnalyzer),
                new AnalyzerSuite(nameof(VerifyDuplicateConceptAnalyzer), VerifyDuplicateConceptAnalyzer),
                new AnalyzerSuite(nameof(VerifyTupleAnalyzer), VerifyTupleAnalyzer),
                new AnalyzerSuite(nameof(CoreVocabularyAnalyzerTests.VerifyCoreVocabularyAnalyzer), CoreVocabularyAnalyzerTests.VerifyCoreVocabularyAnalyzer),
                new AnalyzerSuite(nameof(CompatibilityServiceAnalyzerTests.VerifyCompatibilityServiceAnalyzer), CompatibilityServiceAnalyzerTests.VerifyCompatibilityServiceAnalyzer),
                new AnalyzerSuite(nameof(AddressIdentityAnalyzerTests.VerifyAddressIdentityAnalyzer), AddressIdentityAnalyzerTests.VerifyAddressIdentityAnalyzer),
                new AnalyzerSuite(nameof(FrameCompositionAnalyzerTests.VerifyFrameCompositionAnalyzer), FrameCompositionAnalyzerTests.VerifyFrameCompositionAnalyzer),
                new AnalyzerSuite(nameof(DisplayPacketAnalyzerTests.VerifyDisplayPacketAnalyzer), DisplayPacketAnalyzerTests.VerifyDisplayPacketAnalyzer),
                new AnalyzerSuite(nameof(PaletteLayoutAnalyzerTests.VerifyPaletteLayoutAnalyzer), PaletteLayoutAnalyzerTests.VerifyPaletteLayoutAnalyzer),
                new AnalyzerSuite(nameof(HostConcernAnalyzerTests.VerifyHostConcernAnalyzer), HostConcernAnalyzerTests.VerifyHostConcernAnalyzer),
                new AnalyzerSuite(nameof(VerifyKindTaggedRecordAnalyzer), VerifyKindTaggedRecordAnalyzer),
                new AnalyzerSuite(nameof(VerifyStringIdentityAnalyzer), VerifyStringIdentityAnalyzer),
                new AnalyzerSuite(nameof(VerifyDspRegisterAccessAnalyzer), VerifyDspRegisterAccessAnalyzer),
                new AnalyzerSuite(nameof(VerifySpcMemoryAccessAnalyzer), VerifySpcMemoryAccessAnalyzer),
                new AnalyzerSuite(nameof(VerifyParameterListAnalyzer), VerifyParameterListAnalyzer),
                new AnalyzerSuite(nameof(VerifyPartialTypeAnalyzer), VerifyPartialTypeAnalyzer),
                new AnalyzerSuite(nameof(VerifyNullForgivingAnalyzer), VerifyNullForgivingAnalyzer),
                new AnalyzerSuite(nameof(VerifyAdapterChainAnalyzer), VerifyAdapterChainAnalyzer),
                new AnalyzerSuite(nameof(VerifyOneTypePerFileAnalyzer), VerifyOneTypePerFileAnalyzer),
                new AnalyzerSuite(nameof(ModuleMapAnalyzerTests.VerifyModuleMapAnalyzers), ModuleMapAnalyzerTests.VerifyModuleMapAnalyzers),
                new AnalyzerSuite(nameof(SpeciesReachAnalyzerTests.VerifySpeciesReachAnalyzer), SpeciesReachAnalyzerTests.VerifySpeciesReachAnalyzer),
                new AnalyzerSuite(nameof(SpeciesIdentityAnalyzerTests.VerifySpeciesIdentityAnalyzer), SpeciesIdentityAnalyzerTests.VerifySpeciesIdentityAnalyzer),
                new AnalyzerSuite(nameof(EnemySlotWordAnalyzerTests.VerifyEnemySlotWordAnalyzer), EnemySlotWordAnalyzerTests.VerifyEnemySlotWordAnalyzer),
                new AnalyzerSuite(nameof(FixedPointVocabularyAnalyzerTests.VerifyFixedPointVocabularyAnalyzer), FixedPointVocabularyAnalyzerTests.VerifyFixedPointVocabularyAnalyzer),
                new AnalyzerSuite(nameof(UnderscoreMemberAnalyzerTests.VerifyUnderscoreMemberAnalyzer), UnderscoreMemberAnalyzerTests.VerifyUnderscoreMemberAnalyzer),
                new AnalyzerSuite(nameof(RoomLifetimeAnalyzerTests.VerifyRoomLifetimeAnalyzer), RoomLifetimeAnalyzerTests.VerifyRoomLifetimeAnalyzer),
                new AnalyzerSuite(nameof(RoomPointerAnalyzerTests.VerifyRoomPointerAnalyzer), RoomPointerAnalyzerTests.VerifyRoomPointerAnalyzer),
                new AnalyzerSuite(nameof(VerifyOwnedReflectionAnalyzer), VerifyOwnedReflectionAnalyzer),
                new AnalyzerSuite(nameof(VerifyParameterContracts), VerifyParameterContracts),
                new AnalyzerSuite(nameof(VerifyEnsureCallsAndCodeFix), VerifyEnsureCallsAndCodeFix),
                new AnalyzerSuite(nameof(VerifyWordReadAnalyzer), VerifyWordReadAnalyzer),
                new AnalyzerSuite(nameof(VerifyPaletteFxDependencyContract), VerifyPaletteFxDependencyContract),
            ];
            // `--suite <name>...` runs only the named suites, so a change to one rule checks only that rule.
            if (args is ["--suite", .. var names] && names.Length != 0)
            {
                string[] unknown = names.Where(name => !suites.Any(suite => suite.Name == name)).ToArray();
                if (unknown.Length != 0)
                    throw new ArgumentException($"Unknown verification suites: {string.Join(", ", unknown)}.", nameof(args));
                suites = suites.Where(suite => names.Contains(suite.Name)).ToArray();
            }
            else if (args.Length != 0)
            {
                throw new ArgumentException("Unknown verification arguments.", nameof(args));
            }
            var failed = new List<string>();
            foreach ((string name, Action run) in suites)
            {
                try
                {
                    run();
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine($"FAIL {name}: {exception}");
                    failed.Add(name);
                }
            }
            if (failed.Count > 0)
            {
                Console.Error.WriteLine($"{failed.Count} of {suites.Length} suite(s) failed: {string.Join(", ", failed)}");
                return 1;
            }
            Console.WriteLine("Ensure API, analyzer and palette-FX dependency checks passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>Checks Ensure return values, parameter names, bounds, and span length rejection.</summary>
    private static void VerifyEnsure()
    {
        string? text = "okay";
        Equal("okay", Ensure.NotNull(text));
        Throws<ArgumentNullException>(() => Ensure.NotNull((string?)null), "(string?)null");

        int count = 1;
        Equal(1, Ensure.AtLeastZero(count));
        Throws<ArgumentOutOfRangeException>(() => Ensure.AtLeastZero(count - 2), "count - 2");
        Equal(0, Ensure.BetweenInclusive(0, 64, 0));
        Equal(64, Ensure.BetweenInclusive(0, 64, 64));
        Throws<ArgumentOutOfRangeException>(() => Ensure.BetweenInclusive(0, 64, 65), "65");
        Throws<ArgumentException>(() => Ensure.BetweenInclusive(5, 4, 4), "minimum");

        ReadOnlySpan<int> span = [1, 2];
        Equal(2, Ensure.LengthEqual(span, 2).Length);
        try
        {
            Ensure.LengthEqual(span, 3);
            throw new InvalidOperationException("Expected span length rejection.");
        }
        catch (ArgumentException exception)
        {
            Equal("span", exception.ParamName);
        }
    }

    /// <summary>Fails the verification fixture when two values differ under default equality.</summary>
    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }

    /// <summary>Requires an exact argument exception type, parameter name, and optional message detail.</summary>
    private static void Throws<T>(Action action, string parameterName, string? messageFragment = null) where T : ArgumentException
    {
        try
        {
            action();
        }
        catch (T exception) when (exception.GetType() == typeof(T))
        {
            Equal(parameterName, exception.ParamName);
            if (messageFragment is not null &&
                !exception.Message.Contains(messageFragment, StringComparison.Ordinal))
                throw new InvalidOperationException($"Expected message to contain '{messageFragment}': {exception.Message}");
            return;
        }
        throw new InvalidOperationException($"Expected {typeof(T).Name} for {parameterName}.");
    }
}
