using System.Runtime.InteropServices;
using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// One side-by-side walk of two published graphs (#1532, #2602): the pairs of values still to compare, and the two field
/// recorders an object's generated members write its fields into, reused for every object the walk compares.
/// </summary>
internal sealed class GraphWalk
{
    /// <summary>The pairs of values still to compare.</summary>
    private readonly Stack<(object? Left, object? Right)> pending = new();

    /// <summary>The fields of the left graph's object being compared.</summary>
    internal FieldRecorder Left { get; } = new();

    /// <summary>The fields of the right graph's object being compared.</summary>
    internal FieldRecorder Right { get; } = new();

    /// <summary>Queues two values to compare.</summary>
    internal void Push(object? left, object? right) => pending.Push((left, right));

    /// <summary>The next pair of values to compare, or false when none is left.</summary>
    internal bool TryPop(out object? left, out object? right)
    {
        if (pending.TryPop(out (object? Left, object? Right) pair))
        {
            (left, right) = pair;
            return true;
        }
        left = right = null;
        return false;
    }

    /// <summary>
    /// Compares two objects through their generated field writer: equal scalars and enum members bit for bit, the same
    /// number of other values, which are queued in pairs.
    /// </summary>
    internal bool SameFields<T>(Action<IStateGraphWriter, T> write, T left, T right)
    {
        Left.Clear();
        Right.Clear();
        write(Left, left);
        write(Right, right);
        if (!Left.Scalars.SequenceEqual(Right.Scalars) || Left.Values.Count != Right.Values.Count)
            return false;
        for (int index = 0; index < Left.Values.Count; index++)
            Push(Left.Values[index], Right.Values[index]);
        return true;
    }

    /// <summary>
    /// Records the fields an object's generated members write: every scalar and enum member as its bytes, in order, and
    /// every other value as itself.
    /// </summary>
    internal sealed class FieldRecorder : IStateGraphWriter
    {
        /// <summary>The scalar bytes written so far.</summary>
        private byte[] scalars = new byte[256];

        /// <summary>How many scalar bytes are written.</summary>
        private int length;

        /// <summary>The scalar and enum bytes the object wrote, in order.</summary>
        internal ReadOnlySpan<byte> Scalars => scalars.AsSpan(0, length);

        /// <summary>The other values the object wrote, in order.</summary>
        internal List<object?> Values { get; } = [];

        /// <summary>Forgets the previous object's fields.</summary>
        internal void Clear()
        {
            length = 0;
            Values.Clear();
        }

        /// <inheritdoc/>
        public void Fields(int count)
        {
            // Both objects are of one type, whose generated members write the same fields in the same order.
        }

        /// <inheritdoc/>
        public void Field(StateGraphField field)
        {
            // Both objects are of one type, whose generated members write the same fields in the same order.
        }

        /// <inheritdoc/>
        public void Write(object? value) => Values.Add(value);

        /// <inheritdoc/>
        public void WriteScalar<T>(T value) where T : unmanaged => Append(MemoryMarshal.AsBytes(new ReadOnlySpan<T>(in value)));

        /// <inheritdoc/>
        public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum => Append(MemoryMarshal.AsBytes(new ReadOnlySpan<TEnum>(in value)));

        /// <summary>Appends a scalar's bytes.</summary>
        private void Append(ReadOnlySpan<byte> bytes)
        {
            if (length + bytes.Length > scalars.Length)
                Array.Resize(ref scalars, Math.Max(scalars.Length * 2, length + bytes.Length));
            bytes.CopyTo(scalars.AsSpan(length));
            length += bytes.Length;
        }
    }
}
