"""Install the reviewed read-only observation hook in a pinned private Snes9x clone."""

def run():
    import argparse
    import subprocess
    from pathlib import Path
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    args = parser.parse_args()
    sha = subprocess.check_output(["git", "-C", str(args.source), "rev-parse", "HEAD"], text=True).strip()
    if sha != "913b75d07c6e8d54e966e2c4a79d7c55428007df":
        raise ValueError("Use the pinned Snes9x 1.60 source revision")
    path = args.source / "cpuexec.cpp"
    text = path.read_text()
    if "RidleyObserveBoundary" not in text:
        declaration = "static inline void S9xReschedule (void);"
        instruction = "\t\tuint8\t\t\t\tOp;"
        if text.count(declaration) != 1 or text.count(instruction) != 1:
            raise ValueError("Unexpected CPU execution source layout")
        text = text.replace(declaration, declaration + "\nextern void RidleyObserveBoundary(unsigned pc);")
        hook = """        if ((Registers.PB == 0x82 && (Registers.PCw == 0x8948 || Registers.PCw == 0x897a)) ||
            (Registers.PB == 0x80 && (Registers.PCw == 0x9496 || Registers.PCw == 0x9459 || Registers.PCw == 0x8338)))
            RidleyObserveBoundary(Registers.PBPC);
"""
        text = text.replace(instruction, hook + instruction)
        path.write_text(text)
    if "RidleyObserveInstruction" not in text:
        # Optional execution trace: a single flag test per instruction when inactive.
        declaration = "extern void RidleyObserveBoundary(unsigned pc);"
        boundary = "        if ((Registers.PB == 0x82 && (Registers.PCw == 0x8948 || Registers.PCw == 0x897a)) ||"
        if text.count(declaration) != 1 or text.count(boundary) != 1:
            raise ValueError("Unexpected instrumented CPU execution source layout")
        text = text.replace(declaration, declaration +
            "\nextern bool RidleyTraceActive;\nextern void RidleyObserveInstruction(unsigned pc);")
        text = text.replace(boundary,
            "        if (RidleyTraceActive)\n            RidleyObserveInstruction(Registers.PBPC);\n" + boundary)
        path.write_text(text)
    # Current MSVC requires associative-container comparators to be const.
    changes = [
        ("conffile.cpp", "section_then_key_less::operator()(const ConfigEntry &a, const ConfigEntry &b) {", "section_then_key_less::operator()(const ConfigEntry &a, const ConfigEntry &b) const {"),
        ("conffile.h", "bool operator()(const ConfigEntry &a, const ConfigEntry &b);", "bool operator()(const ConfigEntry &a, const ConfigEntry &b) const;"),
        ("conffile.h", "bool operator()(const ConfigEntry &a, const ConfigEntry &b){", "bool operator()(const ConfigEntry &a, const ConfigEntry &b) const{"),
    ]
    for filename, original, replacement in changes:
        path = args.source / filename
        text = path.read_text()
        if original in text:
            if text.count(original) != 1:
                raise ValueError("Ambiguous compiler compatibility patch")
            path.write_text(text.replace(original, replacement))
        elif replacement not in text:
            raise ValueError("Unexpected configuration comparator source")
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
