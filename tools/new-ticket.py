"""Opens a GitHub ticket, and is the only way one is opened: it refuses a ticket that does not declare what kind of
change it asks for, and writes the declaration into the ticket's body.

The owner's rule (2026-11-26): every ticket says whether its change is exclusively presentation. One that is says
whether the change is idiomatic. One that is exclusively presentation and not idiomatic gives the justification for
its design pattern: why implementing it is more important than an idiomatic design, which for presentation alone is
unlikely ever to hold. It is justified when the owner says it is, and never without cause to believe there is an
actual difference a player sees or hears. A ticket that is not exclusively presentation, or that is idiomatic, needs
nothing further.

    python tools/new-ticket.py --title "..." --body-file body.md --milestone v1.0.0 --label low
        --presentation-only true|false [--idiomatic true|false]
        [--design-justification "..." --why-not-idiomatic "..."]

A change that is exclusively presentation and not idiomatic needs both of the last two: why it matters more than an
idiomatic design, and why the same effect cannot be produced idiomatically. Where it can be, that is the route taken.

The hook tools/require-ticket-declaration.py refuses `gh issue create` by any other route. Prints the new ticket's
address."""
import argparse
import subprocess
import sys
import traceback

#: The heading the declaration is written under at the end of the ticket's body.
HEADING = "## Design declaration"


def boolean(text: str) -> bool:
    """Reads a declaration's boolean: only the two words are taken."""
    if text not in ("true", "false"):
        raise argparse.ArgumentTypeError(f"must be true or false, not {text!r}")
    return text == "true"


def declaration(presentation_only: bool, idiomatic: bool | None, justification: str | None, why_not_idiomatic: str | None) -> str:
    """The declaration as it is written into the body, or an error saying what it lacks."""
    lines = [HEADING, "", f"- Presentation only: {str(presentation_only).lower()}"]
    if not presentation_only:
        return "\n".join(lines)
    if idiomatic is None:
        raise ValueError("A ticket that is exclusively presentation says whether its change is idiomatic: give --idiomatic true or false.")
    lines.append(f"- Idiomatic: {str(idiomatic).lower()}")
    if idiomatic:
        return "\n".join(lines)
    if justification is None or not justification.strip():
        raise ValueError(
            "A ticket that is exclusively presentation and not idiomatic gives the justification for its design pattern: "
            "give --design-justification with the reason in words.")
    if why_not_idiomatic is None or not why_not_idiomatic.strip():
        raise ValueError(
            "A ticket that is exclusively presentation and not idiomatic also says why the same effect cannot be produced "
            "idiomatically: give --why-not-idiomatic. If it can be, that is the route to take: make the ticket idiomatic.")
    lines.append(f"- Design justification: {justification.strip()}")
    lines.append(f"- Why it cannot be idiomatic: {why_not_idiomatic.strip()}")
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Opens a GitHub ticket with its design declaration.")
    parser.add_argument("--title", required=True)
    parser.add_argument("--body-file", required=True, help="The ticket's body, as a UTF-8 file.")
    parser.add_argument("--milestone", required=True)
    parser.add_argument("--label", action="append", default=[], help="A label; give it once for each.")
    parser.add_argument("--presentation-only", required=True, type=boolean, help="Whether the change is exclusively presentation.")
    parser.add_argument("--idiomatic", type=boolean, help="For a change that is exclusively presentation: whether it is idiomatic.")
    parser.add_argument(
        "--design-justification",
        help="For a presentation change that is not idiomatic: why implementing it is more important than an idiomatic design. "
             "It is justified when the owner says it is, and never without cause to believe a player sees or hears a difference: "
             "say what that difference is.")
    parser.add_argument(
        "--why-not-idiomatic",
        help="For a presentation change that is not idiomatic: why the same effect cannot be produced idiomatically. "
             "Where it can be, that is the route taken and the ticket is an idiomatic one.")
    arguments = parser.parse_args()

    declared = declaration(arguments.presentation_only, arguments.idiomatic, arguments.design_justification, arguments.why_not_idiomatic)
    with open(arguments.body_file, encoding="utf-8") as file:
        body = file.read().rstrip()
    if HEADING in body:
        raise ValueError(f"The body already has a \"{HEADING}\" section; the declaration is written by this tool alone.")
    command = ["gh", "issue", "create", "--milestone", arguments.milestone, "--title", arguments.title, "--body", f"{body}\n\n{declared}\n"]
    for label in arguments.label:
        command += ["--label", label]
    created = subprocess.run(command, check=True, capture_output=True, text=True, encoding="utf-8")
    print(created.stdout.strip().split("\n")[-1])
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except subprocess.CalledProcessError as error:
        sys.stderr.write(f"{error}\n{error.stderr}\n")
        sys.exit(1)
    except Exception:
        traceback.print_exc()
        sys.exit(1)
