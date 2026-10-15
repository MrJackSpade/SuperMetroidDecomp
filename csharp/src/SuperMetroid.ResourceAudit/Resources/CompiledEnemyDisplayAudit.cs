using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Enemies.Drawing;
using Microsoft.CodeAnalysis;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>Accounts for production-owned, zero-part frames, not missing editable artwork.</summary>
internal static class CompiledEnemyDisplayAudit
{
    /// <summary>Credits the renderer-owned empty OAM frames in the resource audit after checking the renderer still recognizes them; direct OAM catalog lookups still need installed entries.</summary>
    internal static void Install(string root, ResourceIndex exports, AuditReport report)
    {
        const string path = "csharp/src/SuperMetroid.Core/Enemies/EnemyPresentation.cs";
        SyntaxTreeGuard(root, path);
        foreach (byte bank in CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks)
        {
            ushort pointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
            if (!CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(bank, pointer))
                throw new InvalidDataException("The compiled empty-frame bank list disagrees with its production predicate.");
            string key = ResourceIndex.Address(bank, pointer);
            exports.Add(ResourceDomains.EnemyDisplay, key);
            report.CompiledDefinitions.Add(new(ResourceDomains.EnemyDisplay, key));
        }
    }

    /// <summary>Requires the reviewed no-op branch in DrawEnemySpritemap before treating compiled empty frames as renderer-owned resources.</summary>
    private static void SyntaxTreeGuard(string root, string path)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, path)));
        MethodDeclarationSyntax method = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Single(item => item.Identifier.ValueText == "DrawEnemySpritemap");
        if (!HasNoOpBranch(method))
            throw new InvalidDataException("The compiled empty-OAM renderer contract changed; review the resource audit adapter.");
    }

    /// <summary>Recognizes only the bare return guarded by HasEmptySpritemap(draw.Bank, draw.Pointer), so a changed renderer contract fails the audit.</summary>
    internal static bool HasNoOpBranch(MethodDeclarationSyntax method) =>
        method.DescendantNodes().OfType<IfStatementSyntax>().Any(branch =>
            branch.Else is null && branch.Statement is ReturnStatementSyntax { Expression: null } &&
            branch.Condition is InvocationExpressionSyntax invocation &&
            invocation.Expression is MemberAccessExpressionSyntax access &&
            access.Expression.ToString() == nameof(CommonEnemyEmptyExtendedFrameDefinitions) &&
            access.Name.Identifier.ValueText == nameof(CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap) &&
            invocation.ArgumentList.Arguments.Select(argument => argument.Expression.ToString())
                .SequenceEqual(new[] { "draw.Bank", "draw.Pointer" }));
}
