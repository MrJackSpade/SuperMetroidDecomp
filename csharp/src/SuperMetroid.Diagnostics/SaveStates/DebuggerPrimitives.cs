using System.Collections.Frozen;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The scalars a debugger state writes through a fixed binary representation, and how each is written and read: every
/// framed primitive value and every primitive inside a packed element run (#1696) goes through these two operations.
/// </summary>
internal static class DebuggerPrimitives
{
    /// <summary>Every scalar type a state can write, each through its fixed representation.</summary>
    private static readonly FrozenSet<Type> Supported = new[]
    {
        typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
        typeof(ulong), typeof(char), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(TimeSpan),
        typeof(Guid),
    }.ToFrozenSet();

    /// <summary>Whether <paramref name="type"/> is a scalar a state can write.</summary>
    internal static bool IsSupported(Type type) => Supported.Contains(type);

    /// <summary>Writes <paramref name="value"/>, a boxed <paramref name="type"/>, through its fixed binary representation.</summary>
    /// <exception cref="NotSupportedException">The type is not a scalar a state can write.</exception>
    internal static void Write(BinaryWriter writer, Type type, object value)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean: writer.Write((bool)value); return;
            case TypeCode.Byte: writer.Write((byte)value); return;
            case TypeCode.SByte: writer.Write((sbyte)value); return;
            case TypeCode.Int16: writer.Write((short)value); return;
            case TypeCode.UInt16: writer.Write((ushort)value); return;
            case TypeCode.Int32: writer.Write((int)value); return;
            case TypeCode.UInt32: writer.Write((uint)value); return;
            case TypeCode.Int64: writer.Write((long)value); return;
            case TypeCode.UInt64: writer.Write((ulong)value); return;
            case TypeCode.Char: writer.Write((char)value); return;
            case TypeCode.Single: writer.Write((float)value); return;
            case TypeCode.Double: writer.Write((double)value); return;
            case TypeCode.Decimal: writer.Write((decimal)value); return;
            case TypeCode.DateTime: writer.Write(((DateTime)value).ToBinary()); return;
            case TypeCode.Empty or TypeCode.Object or TypeCode.DBNull or TypeCode.String:
                break;
            default:
                throw new NotSupportedException($"Debugger state primitive {type.FullName} has an unknown type code.");
        }
        if (type == typeof(TimeSpan)) writer.Write(((TimeSpan)value).Ticks);
        else if (type == typeof(Guid)) writer.Write(((Guid)value).ToByteArray());
        else throw new NotSupportedException($"Debugger state primitive {type.FullName} is unsupported.");
    }

    /// <summary>Reads a boxed <paramref name="type"/> written by <see cref="Write"/>.</summary>
    /// <exception cref="NotSupportedException">The type is not a scalar a state can write.</exception>
    internal static object Read(BinaryReader reader, Type type)
    {
        if (type == typeof(bool)) return reader.ReadBoolean();
        if (type == typeof(byte)) return reader.ReadByte();
        if (type == typeof(sbyte)) return reader.ReadSByte();
        if (type == typeof(short)) return reader.ReadInt16();
        if (type == typeof(ushort)) return reader.ReadUInt16();
        if (type == typeof(int)) return reader.ReadInt32();
        if (type == typeof(uint)) return reader.ReadUInt32();
        if (type == typeof(long)) return reader.ReadInt64();
        if (type == typeof(ulong)) return reader.ReadUInt64();
        if (type == typeof(char)) return reader.ReadChar();
        if (type == typeof(float)) return reader.ReadSingle();
        if (type == typeof(double)) return reader.ReadDouble();
        if (type == typeof(decimal)) return reader.ReadDecimal();
        if (type == typeof(DateTime)) return DateTime.FromBinary(reader.ReadInt64());
        if (type == typeof(TimeSpan)) return TimeSpan.FromTicks(reader.ReadInt64());
        if (type == typeof(Guid)) return new Guid(reader.ReadBytes(16));
        throw new NotSupportedException($"Debugger state primitive {type.FullName} is unsupported.");
    }
}
