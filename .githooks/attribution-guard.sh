#!/bin/sh
# Shared check against AI assistant vendor attribution. Input is the author
# identity line, the committer identity line, then the commit message.
# Claude and Anthropic may not appear anywhere. Codex and OpenAI may not appear
# in an identity or an attribution trailer (Author:, Committer:, or any *-By:).

# Prints the offending lines of stdin and fails when any are found.
reject_blocked_names() {
    matches=$(awk '
        { line = tolower($0) }
        line ~ /claude|anthropic/ { print NR ": " $0; next }
        line ~ /codex|openai/ && (NR <= 2 || line ~ /^[ \t]*([a-z-]*-by|author|committer)[ \t]*:/) { print NR ": " $0 }
    ')
    if [ -n "$matches" ]; then
        printf '%s\n' "$matches" >&2
        return 1
    fi
    return 0
}
