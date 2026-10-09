using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Accounts for production-owned, zero-part frames, not missing editable artwork.</summary>
internal static class CompiledEnemyDisplayAudit
{
    /// <summary>Installs reviewed compiled empty-frame display identities into the audit index.</summary>
    internal static void Install(string root, ResourceIndex exports, AuditReport report)
    {
        const string path = "csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.cs";
        SyntaxTreeGuard(root, path);
        foreach (byte bank in CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks)
        {
            ushort pointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
            if (!CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(bank, pointer))
                throw new InvalidDataException("The compiled empty-frame bank list disagrees with its production predicate.");
            string key = ResourceIndex.Address(bank, pointer);
            // This satisfies the *renderer* contract only. A direct TryGet on the
            // installed OAM catalog still requires an actual catalog entry.
            exports.Add(ResourceDomains.EnemyDisplay, key);
            report.CompiledDefinitions.Add(new(ResourceDomains.EnemyDisplay, key));
        }
    }

    /// <summary>Verifies the production renderer still contains the reviewed empty-frame no-op branch.</summary>
    private static void SyntaxTreeGuard(string root, string path)
    {
        // Do not turn a reviewed renderer convention into an unchecked allowlist.
        // If its no-op ownership changes, require a fresh audit adapter instead.
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, path)));
        MethodDeclarationSyntax method = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Single(item => item.Identifier.ValueText == "DrawEnemySpritemap");
        if (!HasNoOpBranch(method))
            throw new InvalidDataException("The compiled empty-OAM renderer contract changed; review the resource audit adapter.");
    }

    /// <summary>Determines whether a renderer method retains the reviewed empty-frame return branch.</summary>
    internal static bool HasNoOpBranch(MethodDeclarationSyntax method) =>
        method.DescendantNodes().OfType<IfStatementSyntax>().Any(branch =>
            branch.Else is null && branch.Statement is ReturnStatementSyntax { Expression: null } &&
            branch.Condition is InvocationExpressionSyntax invocation &&
            invocation.Expression is MemberAccessExpressionSyntax access &&
            access.Expression.ToString() == nameof(CommonEnemyEmptyExtendedFrameDefinitions) &&
            access.Name.Identifier.ValueText == nameof(CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap) &&
            invocation.ArgumentList.Arguments.Select(argument => argument.Expression.ToString())
                .SequenceEqual(new[] { "bank", "pointer" }));
}
