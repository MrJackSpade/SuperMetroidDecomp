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
        const string path = "csharp/src/SuperMetroid.Core/World/EnemyDrawList.cs";
        SyntaxTreeGuard(root, path);
        foreach (byte bank in CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks)
        {
            ushort pointer = CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap;
            if (!CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap(bank, pointer))
                throw new InvalidDataException("The compiled empty-frame bank list disagrees with its production predicate.");
            string key = ResourceIndex.Address(bank, pointer);
            exports.Add(ResourceDomains.EnemyDisplay, key);
            report.CompiledDefinitions.Add(new(ResourceDomains.EnemyDisplay, key));
        }
    }

    /// <summary>Requires the reviewed no-op branch in DrawLayers before treating compiled empty frames as renderer-owned resources.</summary>
    private static void SyntaxTreeGuard(string root, string path)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, path)));
        if (!HasNoOpBranch(Method(tree, "AdvanceLayers"), Method(tree, "DrawLayers")))
            throw new InvalidDataException("The compiled empty-OAM renderer contract changed; review the resource audit adapter.");
    }

    /// <summary>The one method named <paramref name="name"/> in <paramref name="tree"/>.</summary>
    internal static MethodDeclarationSyntax Method(SyntaxTree tree, string name) => tree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>().Single(item => item.Identifier.ValueText == name);

    /// <summary>
    /// Recognizes only the reviewed no-op (#1513): <paramref name="prepares"/> marks each prepared draw pictured exactly when
    /// its frame is not one <c>CommonEnemyEmptyFrames.DrawsNothing</c> names, and <paramref name="draws"/> publishes the sheet
    /// picture only under a bare <c>if (draw.Pictured)</c>, so a changed renderer contract fails the audit.
    /// </summary>
    internal static bool HasNoOpBranch(MethodDeclarationSyntax prepares, MethodDeclarationSyntax draws) =>
        prepares.DescendantNodes().OfType<ArgumentSyntax>().Any(argument =>
            argument.NameColon?.Name.Identifier.ValueText == "Pictured" &&
            argument.Expression is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression } negation &&
            negation.Operand is InvocationExpressionSyntax call &&
            call.Expression.ToString() == "CommonEnemyEmptyFrames.DrawsNothing" &&
            call.ArgumentList.Arguments.Count == 1 &&
            call.ArgumentList.Arguments[0].ToString() == "frame") &&
        draws.DescendantNodes().OfType<IfStatementSyntax>().Any(branch =>
            branch.Else is null && branch.Condition.ToString() == "draw.Pictured");
}
