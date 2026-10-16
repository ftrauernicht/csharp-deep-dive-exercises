#!/usr/bin/env bash
# Checks the three bilingual conventions of this repository (see CONTRIBUTING.md):
#   1. exercises/en/ and exercises/de/ hold the same chapters, recognised by
#      their numeric prefix. The file names themselves are translated
#      (00-overview.md <-> 00-ueberblick.md), so the prefixes are what can be
#      compared.
#   2. Both folders hold the same number of files, which is what catches a
#      missing unnumbered page such as the learning material.
#   3. Prose outside the exercises follows NAME.md (English) plus NAME.de.md
#      (German). CLAUDE.md is exempt: it addresses a tool, not a reader.
set -euo pipefail
shopt -s nullglob

fail=0

en_dir="exercises/en"
de_dir="exercises/de"

for dir in "$en_dir" "$de_dir"; do
  if [[ ! -d "$dir" ]]; then
    echo "::error::${dir} is missing"
    fail=1
  fi
done

if [[ "$fail" -eq 0 ]]; then
  en_prefixes=$(for f in "$en_dir"/*.md; do basename "$f"; done | grep -oE '^[0-9]+' | sort -u || true)
  de_prefixes=$(for f in "$de_dir"/*.md; do basename "$f"; done | grep -oE '^[0-9]+' | sort -u || true)

  if [[ "$en_prefixes" != "$de_prefixes" ]]; then
    echo "::error::exercises/: en/ and de/ chapter numbers differ"
    echo "  en: $(echo "$en_prefixes" | tr '\n' ' ')"
    echo "  de: $(echo "$de_prefixes" | tr '\n' ' ')"
    fail=1
  fi

  en_count=$(ls -1 "$en_dir"/*.md | wc -l)
  de_count=$(ls -1 "$de_dir"/*.md | wc -l)

  if [[ "$en_count" -ne "$de_count" ]]; then
    echo "::error::exercises/: en/ has ${en_count} files, de/ has ${de_count}"
    fail=1
  fi
fi

# Paired prose outside the exercise folders: the repository root and the
# sample solution, which carries the LINQ overview from exercise 5.3.
for dir in . solution; do
  for f in "$dir"/*.md; do
    base=$(basename "$f")
    [[ "$base" == *.de.md ]] && continue
    [[ "$base" == "CLAUDE.md" ]] && continue
    de_file="${f%.md}.de.md"
    if [[ ! -f "$de_file" ]]; then
      echo "::error::${f} has no German counterpart (${de_file})"
      fail=1
    fi
  done

  for f in "$dir"/*.de.md; do
    en_file="${f%.de.md}.md"
    if [[ ! -f "$en_file" ]]; then
      echo "::error::${f} has no English counterpart (${en_file})"
      fail=1
    fi
  done
done

if [[ "$fail" -eq 0 ]]; then
  echo "Bilingual sync check passed."
fi

exit "$fail"
