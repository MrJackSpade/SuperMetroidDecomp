using SuperMetroid.Core.Frontend.Intro;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Checks the finite Core implementation set and sparse cinematic frame identities, not arbitrary runtime points-to state.</summary>
internal static class InterfacePresentationDomainAudit
{
    /// <summary>Compares actual interface implementers to the reviewed set and requires all partial declarations to stay in approved source files.</summary>
    internal static bool HasReviewedImplementations(Compilation compilation, ClosedPresentationContract contract)
    {
        string[]? expected = InterfacePresentationContractDefinitions.Implementations(contract.Type);
        if (expected is null) return true;
        INamedTypeSymbol[] types = Types(compilation.Assembly.GlobalNamespace).ToArray();
        string[] actual = types.Where(type => type.AllInterfaces.Any(item => item.ToDisplayString() == contract.Type))
            .Select(type => type.ToDisplayString()).Order(StringComparer.Ordinal).ToArray();
        if (!actual.SequenceEqual(expected.Order(StringComparer.Ordinal))) return false;
        // Include definitions and helpers, not only receiver types: another partial
        // declaration could gain access to a reviewed private metadata array.
        return types.Where(type => type.DeclaringSyntaxReferences.Any(declaration =>
                contract.Sources.Any(source => source.Path == declaration.SyntaxTree.FilePath)))
            .All(type => type.DeclaringSyntaxReferences.All(declaration =>
                contract.Sources.Any(source => source.Path == declaration.SyntaxTree.FilePath)));
    }

    /// <summary>Recursively visits source type symbols, including types nested under other types.</summary>
    private static IEnumerable<INamedTypeSymbol> Types(INamespaceOrTypeSymbol parent)
    {
        foreach (ISymbol member in parent.GetMembers())
        {
            if (member is INamedTypeSymbol type) yield return type;
            if (member is INamespaceOrTypeSymbol nested)
                foreach (INamedTypeSymbol child in Types(nested)) yield return child;
        }
    }

    /// <summary>Flags a constant cinematic pose that no reviewed sprite presenter owns.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(IIntroCinematicSpritePresentation).FullName ||
            operation.TargetMethod.Name != "Draw") return null;
        // A pose named in the source is an enum constant: its member or an undefined value cast to the pose.
        if (operation.Arguments.Single(argument => argument.Parameter?.Name == "pose").Value.ConstantValue is not { HasValue: true, Value: not null } value) return null;
        var pose = (CinematicSpritePose)Convert.ToInt32(value.Value, System.Globalization.CultureInfo.InvariantCulture);
        bool owned = IntroDiscoveryActorSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            IntroEggEffectSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            IntroRinkaSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            IntroScientistSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            IntroCaretSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            IntroNextPageArrowSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            CeresFlightSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            CeresDestructionSpriteDefinitions.Poses.Any(candidate => candidate == pose) ||
            EndingCloudSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            EndingCompletionTextSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            EndingExplosionSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            EndingLogoSpriteDefinitions.Frames.Any(frame => frame.Pose == pose) ||
            EndingRewardSpriteDefinitions.Frames.Any(frame => frame.Pose == pose);
        return owned ? null : $"Known cinematic pose {pose} belongs to no reviewed interface implementation.";
    }
}
