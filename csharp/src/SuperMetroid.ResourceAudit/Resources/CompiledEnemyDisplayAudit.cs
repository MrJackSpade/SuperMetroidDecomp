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
        SyntaxTreeGuard(root, "csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyFrameDisplays.cs",
            "csharp/src/SuperMetroid.Core/World/EnemyDrawList.cs");
        foreach (byte bank in CommonEnemyEmptyFrameDefinitionsTooling.SupportedBanks)
        {
            ushort pointer = CommonEnemyEmptyFrameToolingDefinitions.EmptySpritemap;
            if (!CommonEnemyEmptyFrameToolingDefinitions.HasEmptySpritemap(bank, pointer))
                throw new InvalidDataException("The compiled empty-frame bank list disagrees with its production predicate.");
            string key = ResourceIndex.Address(bank, pointer);
            exports.Add(ResourceDomains.EnemyDisplay, key);
            report.CompiledDefinitions.Add(new(ResourceDomains.EnemyDisplay, key));
        }
    }

    /// <summary>
    /// Requires the reviewed no-op branch, the empty frame resolved without a picture in the frame displays and the draw
    /// publishing only pictured frames, before treating compiled empty frames as renderer-owned resources.
    /// </summary>
    private static void SyntaxTreeGuard(string root, string displaysPath, string drawPath)
    {
        var displays = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, displaysPath)));
        var draw = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, drawPath)));
        if (!HasNoOpBranch(Method(displays, "TryGetDisplay"), Method(draw, "Advance"), Method(draw, "Draw")))
            throw new InvalidDataException("The compiled empty-OAM renderer contract changed; review the resource audit adapter.");
    }

    /// <summary>The one method named <paramref name="name"/> in <paramref name="tree"/>.</summary>
    internal static MethodDeclarationSyntax Method(SyntaxTree tree, string name) => tree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>().Single(item => item.Identifier.ValueText == name);

    /// <summary>
    /// Recognizes only the reviewed no-op (#1513, #1603): <paramref name="resolves"/> answers a frame
    /// <c>CommonEnemyEmptyFrames.DrawsNothing</c> names as displayed without a picture, <paramref name="prepares"/> marks
    /// each prepared draw pictured exactly as that resolution says, and <paramref name="draws"/> publishes the sheet
    /// picture only under a bare <c>if (draw.Pictured)</c>, so a changed renderer contract fails the audit.
    /// </summary>
    internal static bool HasNoOpBranch(MethodDeclarationSyntax resolves, MethodDeclarationSyntax prepares, MethodDeclarationSyntax draws) =>
        resolves.DescendantNodes().OfType<IfStatementSyntax>().Any(branch =>
            branch.Else is null &&
            branch.Condition is InvocationExpressionSyntax call &&
            call.Expression.ToString() == "CommonEnemyEmptyFrames.DrawsNothing" &&
            call.ArgumentList.Arguments.Count == 1 &&
            call.ArgumentList.Arguments[0].ToString() == "frame" &&
            branch.Statement.DescendantNodes().OfType<AssignmentExpressionSyntax>().Any(assignment =>
                assignment.Left.ToString() == "pictured" && assignment.Right.IsKind(SyntaxKind.FalseLiteralExpression))) &&
        prepares.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(call =>
            call.Expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "TryGetDisplay" } &&
            call.ArgumentList.Arguments.Count == 2 &&
            call.ArgumentList.Arguments[1].ToString() == "out bool pictured") &&
        prepares.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Any(creation =>
            creation.Type.ToString() == "PreparedEnemyDraw" &&
            creation.ArgumentList?.Arguments.LastOrDefault()?.ToString() == "pictured") &&
        draws.DescendantNodes().OfType<IfStatementSyntax>().Any(branch =>
            branch.Else is null && branch.Condition.ToString() == "draw.Pictured");
}
