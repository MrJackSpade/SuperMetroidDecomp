using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Inventories the production resolver/draw dispatcher directly. No manually
/// duplicated provider list can hide a newly added or omitted resolver branch.
/// Method-token guards fail closed when a record layout or typed route changes.
/// </summary>
internal sealed class PlmProgramSource
{
    private readonly PlmProgramAuditReport report;
    private readonly Dictionary<string, Type> types;
    private readonly MethodDeclarationSyntax[] methods;
    private readonly List<MethodInfo> drawProviders = [];
    private readonly HashSet<ushort> directDraws = [];
    private readonly Dictionary<ushort, bool> draws = [];

    internal PlmProgramSource(string root, PlmProgramAuditReport report)
    {
        this.report = report;
        types = typeof(RoomPlmSystem).Assembly.GetTypes()
            .Where(type => type.Namespace == "SuperMetroid.Core.Rooms" || type.Namespace == "SuperMetroid.Core.Game")
            .GroupBy(type => type.Name).Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single());
        string core = Path.Combine(root, "csharp/src/SuperMetroid.Core");
        methods = Directory.EnumerateFiles(core, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
                !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .OrderBy(path => path, StringComparer.Ordinal)
            .SelectMany(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path)
                .GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(type => type.Identifier.Text is "RoomPlmSystem" or "RoomPlmProgramDefinitions" &&
                    type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ==
                        "SuperMetroid.Core.Rooms")
                .SelectMany(type => type.Members.OfType<MethodDeclarationSyntax>())).ToArray();
        MethodDeclarationSyntax draw = methods.Single(method => method.Identifier.Text == "DrawPlmInstruction" &&
            method.ParameterList.Parameters.Any(parameter => parameter.Identifier.Text == "headerPointer"));
        foreach (InvocationExpressionSyntax call in draw.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (call.Expression is not MemberAccessExpressionSyntax access ||
                call.ArgumentList.Arguments.Count != 2 ||
                call.ArgumentList.Arguments[0].Expression.ToString() != "drawPointer" ||
                access.Name.Identifier.Text is not ("TryGet" or "TryGetDraw")) continue;
            drawProviders.Add(ResolveMethod(access.Expression.ToString(), access.Name.Identifier.Text));
        }
        foreach (BinaryExpressionSyntax comparison in draw.DescendantNodes().OfType<BinaryExpressionSyntax>())
        {
            if (comparison.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression) &&
                comparison.Left.ToString() == "drawPointer" &&
                comparison.Right is MemberAccessExpressionSyntax constant)
                directDraws.Add((ushort)types[constant.Expression.ToString()]
                    .GetField(constant.Name.Identifier.Text, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!
                    .GetRawConstantValue()!);
        }
        if (drawProviders.Count == 0 || directDraws.Count == 0)
            throw new InvalidDataException("The production PLM draw dispatcher could not be inventoried.");
    }

    internal Dictionary<ushort, string> InventoryWords()
    {
        MethodDeclarationSyntax resolver = methods.Single(method => method.Identifier.Text == "TryReadWord");
        var result = new Dictionary<ushort, string>();
        foreach (InvocationExpressionSyntax call in resolver.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (call.Expression is not MemberAccessExpressionSyntax access)
                throw new InvalidDataException("An unclassified PLM definition resolver call was added: " + call);
            MethodInfo provider = ResolveMethod(access.Expression.ToString(), access.Name.Identifier.Text);
            var reader = provider.CreateDelegate<PlmProgramAudit.WordReader>();
            // This is a finite inventory of pure compiled definitions, not a CPU
            // read or a frame/parameter probe. Only the exact registered providers
            // are consulted; the interpreter's fixture/WRAM fallbacks never run.
            for (int address = RoomPlmMemoryLayout.CompiledProgramStart; address <= ushort.MaxValue; address++)
                if (reader((ushort)address, out _)) result.TryAdd((ushort)address, provider.DeclaringType!.Name);
        }
        return result;
    }

    internal IEnumerable<(ushort Address, string Owner)> AssignedConstantRoots()
    {
        foreach (MethodDeclarationSyntax method in methods)
        {
            if (method.Identifier.Text.Contains("Verification", StringComparison.Ordinal) ||
                method.SyntaxTree.FilePath.EndsWith(".Verification.cs", StringComparison.Ordinal)) continue;
            foreach (AssignmentExpressionSyntax assignment in method.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            {
                if (assignment.Left is not MemberAccessExpressionSyntax property ||
                    property.Name.Identifier.Text is not ("InstructionPointer" or "LinkInstruction")) continue;
                foreach (ExpressionSyntax expression in RootExpressions(assignment.Right))
                {
                    object? value = expression switch
                    {
                        LiteralExpressionSyntax literal => literal.Token.Value,
                        MemberAccessExpressionSyntax constant when types.TryGetValue(constant.Expression.ToString(), out Type? type) =>
                            type.GetField(constant.Name.Identifier.Text,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) is { IsLiteral: true } field
                                ? field.GetRawConstantValue() : null,
                        _ => null,
                    };
                    if (value is ushort or int && Convert.ToInt32(value) is >= RoomPlmMemoryLayout.CompiledProgramStart and <= ushort.MaxValue)
                        yield return ((ushort)Convert.ToInt32(value), method.Identifier.Text + ": " + expression);
                }
            }
        }
    }

    private static IEnumerable<ExpressionSyntax> RootExpressions(ExpressionSyntax expression)
    {
        // Conditions can compare a header ID; that is not an instruction root.
        // Follow values, not every constant mentioned inside the assignment.
        if (expression is ConditionalExpressionSyntax conditional)
        {
            foreach (var value in RootExpressions(conditional.WhenTrue)) yield return value;
            foreach (var value in RootExpressions(conditional.WhenFalse)) yield return value;
        }
        else yield return expression;
    }

    internal bool OwnsDraw(ushort pointer)
    {
        if (draws.TryGetValue(pointer, out bool owned)) return owned;
        owned = directDraws.Contains(pointer) || drawProviders.Any(provider =>
            (bool)provider.Invoke(null, [pointer, null])!);
        draws.Add(pointer, owned);
        return owned;
    }

    internal void VerifyInterpreter()
    {
        foreach (MethodDeclarationSyntax method in methods.Where(RequiresGuard))
        {
            string key = method.Identifier.Text + "/" + method.ParameterList.Parameters.Count;
            string hash = TokenHash(method);
            if (!PlmInterpreterSourceContract.Methods.TryGetValue(key, out string? expected) || expected != hash)
                report.Findings.Add(new("unresolved-source", key, 0,
                    "Record/route source changed; review the audit contract. Token SHA256=" + hash));
        }
        foreach (string key in PlmInterpreterSourceContract.Methods.Keys)
            if (!methods.Any(method => method.Identifier.Text + "/" + method.ParameterList.Parameters.Count == key))
                report.Findings.Add(new("unresolved-source", key, 0, "Guarded production method was removed."));
    }

    internal static string TokenHash(MethodDeclarationSyntax method) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(" ", method.DescendantTokens().Select(token => token.Text)))));

    private static bool RequiresGuard(MethodDeclarationSyntax method) =>
        method.Identifier.Text is "ExecuteInstructionStream" or "ReadProgramWord" or "ReadProgramByte" or
        "DrawPlmInstruction" or "TryIdentifyPermanentCollectible" or
        "TryRunRoomPopulationSetup" or "SetupScrollSlot" or "TryStepScrollPlm" or
        "SetupCollectibleSlot" or "TryStepCollectible" or "SetupStation" or "TryStepStation" or
        "TryStepWreckedShipTreadmill" or "SpawnEyeDoorProjectile" or "ConvertEyeToBlueDoor" ||
        method.Identifier.Text.StartsWith("TryExecute", StringComparison.Ordinal) ||
        method.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(call =>
            call.Expression.ToString() is "ReadProgramWord" or "ReadProgramByte");

    private MethodInfo ResolveMethod(string type, string name) => types[type].GetMethods(
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        .Single(method => method.Name == name && method.GetParameters() is
            [{ ParameterType: var input }, { IsOut: true }] && input == typeof(ushort));
}
