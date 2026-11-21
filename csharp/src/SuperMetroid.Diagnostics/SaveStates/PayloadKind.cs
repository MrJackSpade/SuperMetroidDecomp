namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>How the value a debugger state defines is stored after its type's name (#2602).</summary>
internal enum PayloadKind : byte
{
    /// <summary>A scalar through its fixed representation.</summary>
    Primitive,

    /// <summary>A string's text.</summary>
    String,

    /// <summary>An array's element storage as raw bytes after its length.</summary>
    PrimitiveArray,

    /// <summary>An array's elements, each framed, after its length.</summary>
    Array,

    /// <summary>A delegate's invocation list: each call's method name and target.</summary>
    Delegate,

    /// <summary>An object's levels of fields, each after its count, each field's value after its name.</summary>
    Fields,

    /// <summary>An enum member's name, or names joined for flags.</summary>
    EnumName,

    /// <summary>A hash-keyed collection's comparers and entries, which a restore adds to a new collection (#1681).</summary>
    HashEntries,

    /// <summary>An array of value-type elements as one packed run: its schema, its enums' member tables, its values (#1696).</summary>
    PackedArray,

    /// <summary>A type's name, for a value that is a type (#2060).</summary>
    TypeIdentity,

    /// <summary>A list, queue, stack or other sequence's elements, each framed.</summary>
    Sequence,

    /// <summary>A lazily loaded part's value.</summary>
    Lazy,
}
