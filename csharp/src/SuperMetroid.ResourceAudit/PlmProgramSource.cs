using SuperMetroid.Core;
using System.Reflection;
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
    /// <summary>Audit findings receive unresolved source-contract changes.</summary>
    private readonly PlmProgramAuditReport report;
    /// <summary>Unique core type names used to resolve referenced compiled providers.</summary>
    private readonly Dictionary<string, Type> types;
    /// <summary>Production PLM methods parsed from the room and family owner types.</summary>
    private readonly MethodDeclarationSyntax[] methods;
    /// <summary>Calculated draw providers discovered from the production draw dispatcher.</summary>
    private readonly List<MethodInfo> drawProviders = [];
    /// <summary>Literal draw pointers tested directly by the production dispatcher.</summary>
    private readonly HashSet<ushort> directDraws = [];
    /// <summary>Memoized ownership results for inspected draw pointers.</summary>
    private readonly Dictionary<ushort, bool> draws = [];

    /// <summary>Parses production PLM methods and discovers their actual draw routes.</summary>
    [ReflectionAudit("static source audit: constants and draw providers named in parsed PLM source are resolved to their compiled values")]
    internal PlmProgramSource(string root, PlmProgramAuditReport report)
    {
        this.report = report;
        types = typeof(RoomPlmSystem).Assembly.GetTypes()
            .Where(type => type.Namespace is "SuperMetroid.Core.Rooms" or "SuperMetroid.Core.Game")
            .GroupBy(type => type.Name).Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single());
        string core = Path.Combine(root, "csharp/src/SuperMetroid.Core");
        methods = Directory.EnumerateFiles(core, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
                !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .OrderBy(path => path, StringComparer.Ordinal)
            .SelectMany(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path)
                .GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                // The PLM engine and its family owners (doors, collectibles, stations, scrolls, ...).
                .Where(type => (type.Identifier.Text is "RoomPlmSystem" or "RoomPlmProgramDefinitions" or "PlmPopulationLoader" or "PlmBlockDrawing" ||
                                type.Identifier.Text.EndsWith("Plms", StringComparison.Ordinal)) &&
                    type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ==
                        "SuperMetroid.Core.Rooms")
                .SelectMany(type => type.Members.OfType<MethodDeclarationSyntax>())).ToArray();
        MethodDeclarationSyntax draw = methods.Single(method => method.Identifier.Text == "DrawPlmInstruction" &&
            method.ParameterList.Parameters.Any(parameter => parameter.Identifier.Text == "headerPointer"));
        foreach (InvocationExpressionSyntax call in draw.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (call.Expression is not MemberAccessExpressionSyntax access ||
                call.ArgumentList.Arguments.Count < 2 ||
                call.ArgumentList.Arguments[0].Expression.ToString() != "drawPointer" ||
                call.ArgumentList.Arguments.Skip(1).Any(argument =>
                    !argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))) continue;
            // Calculated draw providers may return multiple descriptor fields.
            // Discover the production call and signature, not a list of method names.
            drawProviders.Add(types[access.Expression.ToString()].GetMethods(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Single(method =>
                    method.Name == access.Name.Identifier.Text && method.ReturnType == typeof(bool) &&
                    method.GetParameters().Length == call.ArgumentList.Arguments.Count &&
                    method.GetParameters()[0].ParameterType == typeof(ushort) &&
                    method.GetParameters().Skip(1).All(parameter => parameter.IsOut)));
        }
        foreach (BinaryExpressionSyntax comparison in draw.DescendantNodes().OfType<BinaryExpressionSyntax>())
        {
            if (comparison.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression) &&
                comparison.Left.ToString() == "drawPointer" &&
                comparison.Right is MemberAccessExpressionSyntax constant)
                directDraws.Add((ushort)((types[constant.Expression.ToString()]
                    .GetField(constant.Name.Identifier.Text, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException("Verification requires types[constant.Expression.ToString()] .GetField(constant.Name.Identifier.Text, BindingF...."))
                    .GetRawConstantValue() ?? throw new InvalidOperationException("Verification requires (types[constant.Expression.ToString()] .GetField(constant.Name.Identifier.Text, Binding....")));
        }
        if (drawProviders.Count == 0 || directDraws.Count == 0)
            throw new InvalidDataException("The production PLM draw dispatcher could not be inventoried.");
    }

    /// <summary>Enumerates compiled program words through each resolver provider referenced by production code.</summary>
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
            for (int address = RoomPlmMemoryLayoutTooling.CompiledProgramStart; address <= ushort.MaxValue; address++)
                if (reader((ushort)address, out _)) result.TryAdd((ushort)address, (provider.DeclaringType ?? throw new InvalidOperationException("Verification requires provider.DeclaringType.")).Name);
        }
        return result;
    }

    /// <summary>Finds constant instruction roots assigned by production PLM methods outside verification.</summary>
    [ReflectionAudit("static source audit: constants named in parsed PLM source are resolved to their compiled values")]
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
                    if (value is ushort or int && Convert.ToInt32(value) is >= RoomPlmMemoryLayoutTooling.CompiledProgramStart and <= ushort.MaxValue)
                        yield return ((ushort)Convert.ToInt32(value), method.Identifier.Text + ": " + expression);
                }
            }
        }
    }

    /// <summary>Follows conditional assignment values without mistaking comparison constants for roots.</summary>
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

    /// <summary>Checks whether a pointer belongs to a direct draw or any discovered calculated draw provider.</summary>
    internal bool OwnsDraw(ushort pointer)
    {
        if (draws.TryGetValue(pointer, out bool owned)) return owned;
        owned = directDraws.Contains(pointer) || drawProviders.Any(provider =>
        {
            object?[] arguments = new object?[provider.GetParameters().Length];
            arguments[0] = pointer;
            return (bool)(provider.Invoke(null, arguments) ?? throw new InvalidOperationException("Verification requires provider.Invoke(null, arguments)."));
        });
        draws.Add(pointer, owned);
        return owned;
    }

    /// <summary>Reports added or removed interpreter routes relative to the guarded source contract.</summary>
    internal void VerifyInterpreter()
    {
        foreach (MethodDeclarationSyntax method in methods.Where(RequiresGuard))
        {
            string key = method.Identifier.Text + "/" + method.ParameterList.Parameters.Count;
            if (!PlmInterpreterSourceContract.Methods.Contains(key))
                report.Findings.Add(new("unresolved-source", key, 0,
                    "Record/route method is not in the interpreter contract."));
        }
        foreach (string key in PlmInterpreterSourceContract.Methods)
            if (!methods.Any(method => method.Identifier.Text + "/" + method.ParameterList.Parameters.Count == key))
                report.Findings.Add(new("unresolved-source", key, 0, "Guarded production method was removed."));
    }


    /// <summary>Selects interpreter methods whose record or typed routes must stay in the source contract.</summary>
    private static bool RequiresGuard(MethodDeclarationSyntax method) =>
        method.Identifier.Text is "ExecuteInstructionStream" or "ReadProgramWord" or "ReadProgramByte" or
        "DrawPlmInstruction" or "TryIdentifyPermanentCollectible" or
        "TryRunRoomPopulationSetup" or "SetupScrollSlot" or "TryStepScrollPlm" or "FinishScrollMutation" or
        "SetupCollectibleSlot" or "TryStepCollectible" or "SetupStation" or "TryStepStation" or
        "TryStepWreckedShipTreadmill" or "SpawnEyeDoorProjectile" or "ConvertEyeToBlueDoor" ||
        method.Identifier.Text.StartsWith("TryExecute", StringComparison.Ordinal) ||
        method.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(call =>
            call.Expression.ToString() is "ReadProgramWord" or "ReadProgramByte");

    /// <summary>Finds a static word provider with one ushort input and one out parameter.</summary>
    [ReflectionAudit("static source audit: draw providers named in parsed PLM source are resolved to their compiled signatures")]
    private MethodInfo ResolveMethod(string type, string name) => types[type].GetMethods(
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        .Single(method => method.Name == name && method.GetParameters() is
            [{ ParameterType: var input }, { IsOut: true }] && input == typeof(ushort));
}
