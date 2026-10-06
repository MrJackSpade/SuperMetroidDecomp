"""Print the labeled routines a native instruction trace executed, per source frame.

Reads instructions.bin.gz from `ridley_capture --trace` (records of little-endian
source frame and 24-bit PC) and names every PC that starts a label in the pinned
disassembly. Consecutive repeats of one label are collapsed with a count.
"""


def load_labels(disassembly):
    """Map 24-bit addresses to labels using each label's first `;BBAAAA;` comment."""
    import re
    from pathlib import Path
    labels = {}
    address = re.compile(r";([0-9A-F]{6});")
    label = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
    local = re.compile(r"^\s+(\.[A-Za-z0-9_]+):")
    for path in sorted(Path(disassembly).glob("bank_*.asm")):
        pending = []
        current = None
        for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
            match = label.match(line)
            if match:
                current = match.group(1)
                pending.append(current)
            else:
                match = local.match(line)
                if match and current:
                    pending.append(current + match.group(1))
            # A label line may carry its own address comment (`.data: ;91ECB4;`).
            found = address.search(line)
            if found and pending:
                value = int(found.group(1), 16)
                for name in pending:
                    labels.setdefault(value, name)
                pending = []
    return labels


def run():
    import argparse
    import gzip
    import struct
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("trace", help="instructions.bin.gz from capture --trace")
    parser.add_argument("disassembly", help="directory containing bank_*.asm")
    parser.add_argument("--include-local", action="store_true", help="also print local .labels")
    args = parser.parse_args()
    labels = load_labels(args.disassembly)
    previous = None
    repeat = 0
    frame_shown = None
    with gzip.open(args.trace, "rb") as source:
        while True:
            record = source.read(8)
            if not record:
                break
            if len(record) != 8:
                raise ValueError("Truncated instruction trace record")
            frame, pc = struct.unpack("<II", record)
            name = labels.get(pc)
            if name is None or ("." in name and not args.include_local):
                continue
            if frame != frame_shown:
                if repeat > 1:
                    print(f"    x{repeat}")
                print(f"== source frame {frame}")
                frame_shown, previous, repeat = frame, None, 0
            if name == previous:
                repeat += 1
                continue
            if repeat > 1:
                print(f"    x{repeat}")
            print(f"  {pc:06X} {name}")
            previous, repeat = name, 1
    if repeat > 1:
        print(f"    x{repeat}")
    return 0


if __name__ == "__main__":
    import sys
    try:
        if sys.platform == "win32":
            import ctypes
            ctypes.windll.kernel32.SetErrorMode(0x0001 | 0x0002 | 0x8000)
        raise SystemExit(run())
    except Exception:
        import traceback
        traceback.print_exc(file=sys.stderr)
        raise SystemExit(1)
