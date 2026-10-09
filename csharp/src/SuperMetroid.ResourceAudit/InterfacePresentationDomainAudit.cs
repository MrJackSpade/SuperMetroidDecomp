using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Checks the finite Core implementation set and sparse cinematic frame identities, not arbitrary runtime points-to state.</summary>
internal static class InterfacePresentationDomainAudit
{
    /// <summary>Determines whether an interface contract has exactly its reviewed implementation set.</summary>
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

    /// <summary>Enumerates named types recursively beneath a namespace or containing type.</summary>
    private static IEnumerable<INamedTypeSymbol> Types(INamespaceOrTypeSymbol parent)
    {
        foreach (ISymbol member in parent.GetMembers())
        {
            if (member is INamedTypeSymbol type) yield return type;
            if (member is INamespaceOrTypeSymbol nested)
                foreach (INamedTypeSymbol child in Types(nested)) yield return child;
        }
    }

    /// <summary>Returns an error when a known cinematic frame belongs to no reviewed implementation.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(IIntroCinematicSpritePresentation).FullName ||
            operation.TargetMethod.Name != "Draw") return null;
        if (operation.Arguments.Single(argument => argument.Parameter?.Name == "pointer").Value.ConstantValue
            is not { HasValue: true, Value: not null } value) return null;
        int pointer = Convert.ToInt32(value.Value);
        bool owned = IntroDiscoveryActorSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            IntroEggEffectSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            IntroRinkaSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            IntroScientistSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            CeresFlightSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            CeresDestructionSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            EndingCloudSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            EndingCompletionTextSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            EndingExplosionSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            EndingLogoSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer) ||
            EndingRewardSpriteDefinitions.Frames.ToArray().Any(frame => frame.Pointer == pointer);
        return owned ? null : $"Known cinematic frame ${pointer:X4} belongs to no reviewed interface implementation.";
    }
}
