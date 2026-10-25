using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace SuperMetroid.RenderVerification.Device;

/// <summary>
/// The Direct3D 11 renderer draws a frame from its display packet alone (#1408): the only frame input it takes is a
/// <see cref="DisplayPacket"/>, and nothing in the backend holds or accepts a console memory image, a register value or a
/// hardware layout. The installed images it draws reach it inside the packet.
/// </summary>
internal static class DisplayPacketInputTests
{
    /// <summary>
    /// The display model's values that still live in Core's Hardware module until #1422 moves them out: its colours and
    /// the layers a line shows and a blend's layers. Every other type there is a console memory image, register or layout the backend must not
    /// take.
    /// </summary>
    private static readonly HashSet<Type> ModelValues = [typeof(RgbColor), typeof(RgbaPixel), typeof(ScreenLayers), typeof(ColorBlend)];

    /// <summary>Checks the renderer's frame input and every member type of the backend assembly.</summary>
    internal static void Run()
    {
        Type renderer = typeof(D3D11FrameRenderer);
        foreach (MethodInfo method in renderer.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (parameter.ParameterType.Namespace?.StartsWith("SuperMetroid.Core", StringComparison.Ordinal) == true
                    && parameter.ParameterType != typeof(DisplayPacket))
                    throw new InvalidOperationException($"{renderer.Name}.{method.Name} takes a {parameter.ParameterType.Name}; a frame's only input is its display packet.");
            }
        }
        foreach (ConstructorInfo constructor in renderer.GetConstructors())
        {
            if (constructor.GetParameters() is not [{ ParameterType: var device }] || device != typeof(D3D11RenderDevice))
                throw new InvalidOperationException($"{renderer.Name} is constructed from more than its device.");
        }
        var offenders = new List<string>();
        foreach (Type type in renderer.Assembly.GetTypes())
        {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (FieldInfo field in type.GetFields(all))
                Check(field.FieldType, $"{type.Name}.{field.Name}");
            foreach (PropertyInfo property in type.GetProperties(all))
                Check(property.PropertyType, $"{type.Name}.{property.Name}");
            foreach (MethodBase method in type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all)))
            {
                if (method is MethodInfo { ReturnType: var returned })
                    Check(returned, $"{type.Name}.{method.Name} result");
                foreach (ParameterInfo parameter in method.GetParameters())
                    Check(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
            }
        }
        // Every type the backend's code names, static helpers and catalogs included.
        using (var file = File.OpenRead(renderer.Assembly.Location))
        using (var portable = new PEReader(file))
        {
            MetadataReader metadata = portable.GetMetadataReader();
            foreach (TypeReferenceHandle handle in metadata.TypeReferences)
            {
                TypeReference reference = metadata.GetTypeReference(handle);
                string space = metadata.GetString(reference.Namespace), name = metadata.GetString(reference.Name);
                bool modelValue = ModelValues.Any(type => type.Namespace == space && type.Name == name);
                if (space == "SuperMetroid.Core.Hardware" && !modelValue || name.EndsWith("RomData", StringComparison.Ordinal))
                    offenders.Add($"the backend's code names {space}.{name}");
            }
        }
        if (offenders.Count != 0)
            throw new InvalidOperationException("The Direct3D 11 backend takes hardware inputs:" + Environment.NewLine
                + string.Join(Environment.NewLine, offenders));
        Console.WriteLine($"{renderer.Name} draws from its display packet alone; no backend member holds or takes a hardware type.");

        void Check(Type type, string member)
        {
            Type shown = type.IsByRef || type.IsArray || type.IsPointer ? type.GetElementType()! : type;
            if (shown.IsGenericType)
            {
                foreach (Type argument in shown.GetGenericArguments())
                    Check(argument, member);
                return;
            }
            if (shown.Namespace == "SuperMetroid.Core.Hardware" && !ModelValues.Contains(shown))
                offenders.Add($"{member}: {shown.Name}");
        }
    }
}
