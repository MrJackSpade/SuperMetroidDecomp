using System.Reflection;

namespace SuperMetroid.Desktop;

/// <summary>Shared stable method identities for debugger delegates, independent of compiler token order.</summary>
internal static class DebuggerDelegateIdentity
{
    public static void Write(BinaryWriter writer, MethodInfo method)
    {
        writer.Write(method.Name);
        writer.Write(method.IsStatic);
        writer.Write(method.ReturnType.AssemblyQualifiedName!);
        Type[] arguments = method.IsGenericMethod ? method.GetGenericArguments() : [];
        writer.Write(arguments.Length);
        foreach (Type argument in arguments) writer.Write(argument.AssemblyQualifiedName!);
        ParameterInfo[] parameters = method.GetParameters();
        writer.Write(parameters.Length);
        foreach (ParameterInfo parameter in parameters) writer.Write(parameter.ParameterType.AssemblyQualifiedName!);
    }

    public static MethodInfo Read(BinaryReader reader, Type declaringType, Func<string, Type> resolveType)
    {
        string name = reader.ReadString();
        bool isStatic = reader.ReadBoolean();
        Type returnType = resolveType(reader.ReadString());
        int argumentCount = ReadCount(reader);
        var arguments = new Type[argumentCount];
        for (int i = 0; i < arguments.Length; i++) arguments[i] = resolveType(reader.ReadString());
        int parameterCount = ReadCount(reader);
        var parameters = new Type[parameterCount];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = resolveType(reader.ReadString());
        var matches = new List<MethodInfo>();
        foreach (MethodInfo definition in declaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (definition.Name != name || definition.IsStatic != isStatic ||
                definition.GetGenericArguments().Length != argumentCount) continue;
            MethodInfo method = definition;
            if (argumentCount != 0)
            {
                try { method = definition.MakeGenericMethod(arguments); }
                catch (ArgumentException) { continue; }
            }
            // The state loader already resolves allowed types across compatible builds.
            // Compare those identities, not their serialized assembly-version strings:
            // published 0.2.1 callbacks otherwise fail despite an unchanged signature.
            if (method.ReturnType == returnType &&
                method.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters))
                matches.Add(method);
        }
        return matches.Count == 1 ? matches[0] : throw new InvalidDataException(
            $"Debugger delegate {declaringType.FullName}.{name} has {matches.Count} compatible methods in this build.");
    }

    private static int ReadCount(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        return count >= 0 ? count : throw new InvalidDataException("Negative debugger method signature count.");
    }
}
