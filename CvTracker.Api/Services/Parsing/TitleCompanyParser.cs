using System.Text.RegularExpressions;
using CvTracker.Api.Services;

namespace CvTracker.Api.Services.Parsing;

/// <summary>
/// Extracts the job <c>Position</c> (title) and <c>CompanyName</c> from raw offer text.
/// Internal — no interface needed; called only from <see cref="OfferTextParserService"/>.
/// </summary>
internal sealed class TitleCompanyParser
{
    private readonly ISkillNormalizationService _normalizationService;

    internal TitleCompanyParser(ISkillNormalizationService normalizationService)
    {
        _normalizationService = normalizationService;
    }

    // ── Position ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the job position/title using three strategies in order:
    /// <list type="number">
    ///   <item>Label match ("Stanowisko:" / "Position:")</item>
    ///   <item>Role keyword found in early lines</item>
    ///   <item>Positive-signal conditions: A = seniority+role combo,
    ///         B = tech alias+role in first 3 lines, C = standalone empty-neighbour line</item>
    /// </list>
    /// Returns <c>null</c> when none of the strategies fire.
    /// </summary>
    internal string? ExtractPosition(string text, string[] lines)
    {
        // Strategy 1: Label-based.
        var labelMatch = Regex.Match(text,
            @"(?i)(?:stanowisko|position)\s*[:\u2014]\s*([^\n]{1,100})");
        if (labelMatch.Success)
        {
            var v = labelMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        // Strategy 2: Role keyword in first 10 non-empty lines.
        var nonEmptyLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        foreach (var line in nonEmptyLines.Take(10))
        {
            var trimmed = line.Trim();
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length is >= 2 and <= 10 &&
                OfferParserKeywords.RoleKeywords.Any(kw =>
                    trimmed.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            {
                return trimmed;
            }
        }

        // Strategy 3: Positive-signal conditions.

        // Condition A: Title Case or ALL CAPS line with both a seniority marker and a role term.
        foreach (var line in nonEmptyLines.Take(10))
        {
            var trimmed = line.Trim();
            if (!IsTitleCaseOrAllCaps(trimmed)) continue;

            bool hasSeniority = OfferParserKeywords.SeniorityKeywords.Any(kw =>
                trimmed.Contains(kw, StringComparison.OrdinalIgnoreCase));
            bool hasRole = OfferParserKeywords.RoleTermKeywords.Any(kw =>
                trimmed.Contains(kw, StringComparison.OrdinalIgnoreCase));

            if (hasSeniority && hasRole)
                return trimmed;
        }

        // Condition B: Line in first 3 non-empty lines that contains a known technology alias
        //              and a role term.
        foreach (var line in nonEmptyLines.Take(3))
        {
            var trimmed = line.Trim();
            if (trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2) continue;

            bool hasTech = _normalizationService.FindAllInText(trimmed).Any();
            bool hasRole = OfferParserKeywords.RoleTermKeywords.Any(kw =>
                trimmed.Contains(kw, StringComparison.OrdinalIgnoreCase));

            if (hasTech && hasRole)
                return trimmed;
        }

        // Condition C: Standalone line in first 10 lines (index-based) that is both preceded
        //              and followed by an empty line (or the start/end of the text).
        for (int i = 0; i < Math.Min(lines.Length, 10); i++)
        {
            var trimmed = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length is < 2 or > 6) continue;

            // No punctuation except hyphens and slashes.
            if (trimmed.Any(c => char.IsPunctuation(c) && c != '-' && c != '/')) continue;

            bool precededByEmpty = i == 0 || string.IsNullOrWhiteSpace(lines[i - 1]);
            bool followedByEmpty = i == lines.Length - 1 || string.IsNullOrWhiteSpace(lines[i + 1]);

            if (precededByEmpty && followedByEmpty)
                return trimmed;
        }

        return null;
    }

    // ── Company name ──────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the company name using six heuristics in priority order:
    /// <list type="number">
    ///   <item>Label match (60-char cap)</item>
    ///   <item>Legal form suffix detection</item>
    ///   <item>English "at &lt;Name&gt;" pattern</item>
    ///   <item>Polish "w firmie &lt;Name&gt;" pattern</item>
    ///   <item>Polish "dla &lt;Name&gt;" pattern</item>
    ///   <item>Tightened title-case fallback (requires at least one positive signal)</item>
    /// </list>
    /// Every non-label candidate is validated by <see cref="IsValidCompanyCandidate"/>.
    /// </summary>
    internal string? ExtractCompanyName(string text, string[] lines)
    {
        // Heuristic 1: Label-based (60-char cap; exempt from other validation).
        var labelMatch = Regex.Match(text,
            @"(?i)(?:firma|company|pracodawca|employer|o\s+nas|about\s+us|kim\s+jeste[sś]my)\s*[:\u2013\u2014]\s*([^\n]{1,60})");
        if (labelMatch.Success)
        {
            var v = labelMatch.Groups[1].Value.Split('\n')[0].Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        // Heuristic 2: Legal form suffix detection (whole-text scan).
        var legalCandidate = TryExtractLegalFormCompany(text);
        if (legalCandidate != null && IsValidCompanyCandidate(legalCandidate))
            return legalCandidate;

        // Heuristics 3–5: inline patterns searched in the first 500 characters only.
        var head = text.Length > 500 ? text[..500] : text;

        // Heuristic 3: English "at <CompanyName>" — 1–3 capitalised words.
        var atMatch = Regex.Match(head,
            @"\bat\s+([A-ZŁÓŹĄĆĘŚŃ][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-\.]*(?:\s+[A-ZŁÓŹĄĆĘŚŃ][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-\.]*){0,2})(?=[\s,\.\n]|$)");
        if (atMatch.Success)
        {
            var v = atMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v) && IsValidCompanyCandidate(v)) return v;
        }

        // Heuristic 4: Polish "w firmie <Name>" — 1–3 words.
        var wFirmieMatch = Regex.Match(head,
            @"(?i)\bw\s+firmie\s+([A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*(?:\s+[A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*){0,2})(?=[\s,\.\n]|$)");
        if (wFirmieMatch.Success)
        {
            var v = wFirmieMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v) && char.IsUpper(v[0]) && IsValidCompanyCandidate(v))
                return v;
        }

        // Heuristic 5: Polish "dla <Name>" — e.g. "rekrutujemy dla TechCorp".
        var dlaMatch = Regex.Match(head,
            @"(?i)\bdla\s+([A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*(?:\s+[A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*){0,2})(?=[\s,\.\n]|$)");
        if (dlaMatch.Success)
        {
            var v = dlaMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v) && char.IsUpper(v[0]) &&
                !OfferParserKeywords.RoleKeywords.Any(kw =>
                    v.Contains(kw, StringComparison.OrdinalIgnoreCase)) &&
                IsValidCompanyCandidate(v))
                return v;
        }

        // Heuristic 6: Tightened title-case fallback in first 300 characters.
        //   Requires (a) title-case words only, (b) passes IsValidCompanyCandidate,
        //   and (c) at least one positive signal (standalone / empty neighbour / connector).
        var head300Lines = (text.Length > 300 ? text[..300] : text).Split('\n');
        for (int i = 0; i < head300Lines.Length; i++)
        {
            var trimmed = head300Lines[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length is < 2 or > 5) continue;
            if (!words.All(w => w.Length > 0 && char.IsUpper(w[0]))) continue;
            if (OfferParserKeywords.RoleKeywords.Any(kw =>
                    trimmed.Contains(kw, StringComparison.OrdinalIgnoreCase))) continue;
            if (!IsValidCompanyCandidate(trimmed)) continue;

            // Positive signal gate:
            // (i)  Standalone line — phrase occupies the entire trimmed line (always true here).
            // (ii) Empty-line neighbourhood — preceded or followed by a blank line.
            // (iii) Connector word between proper nouns.
            bool isStandaloneLine = true; // candidate IS the entire trimmed line
            bool emptyNeighbour = i == 0
                || string.IsNullOrWhiteSpace(head300Lines[i - 1])
                || i == head300Lines.Length - 1
                || string.IsNullOrWhiteSpace(head300Lines[i + 1]);
            bool hasConnector = OfferParserKeywords.ConnectorWords.Any(c =>
                words.Any(w => w.Equals(c, StringComparison.OrdinalIgnoreCase)));

            if (isStandaloneLine || emptyNeighbour || hasConnector)
                return trimmed;
        }

        return null;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Scans <paramref name="text"/> for registered legal form suffixes and extracts the
    /// preceding 1–5 words on the same line as the company name candidate.
    /// Returns <c>null</c> when no legal form is found.
    /// </summary>
    private static string? TryExtractLegalFormCompany(string text)
    {
        // LegalFormPatterns is already ordered longest-first.
        foreach (var pattern in OfferParserKeywords.LegalFormPatterns)
        {
            int searchFrom = 0;
            while (true)
            {
                int idx = text.IndexOf(pattern, searchFrom, StringComparison.Ordinal);
                if (idx < 0) break;

                int afterIdx = idx + pattern.Length;

                // Boundary guard: do not match the pattern embedded inside a larger word.
                bool trailingLetter = afterIdx < text.Length && char.IsLetter(text[afterIdx]);
                bool leadingLetter = idx > 0 && char.IsLetter(text[idx - 1]) && char.IsLetter(pattern[0]);
                if (trailingLetter || leadingLetter)
                {
                    searchFrom = idx + 1;
                    continue;
                }

                // Extract the company name from the words that precede the pattern on the same line.
                int lineStart = text.LastIndexOf('\n', idx) + 1;
                if (lineStart < 0) lineStart = 0;

                string beforePattern = text[lineStart..idx];
                var wordsBefore = beforePattern.Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (wordsBefore.Length == 0)
                {
                    searchFrom = idx + 1;
                    continue;
                }

                int take = Math.Min(5, wordsBefore.Length);
                var candidate = string.Join(" ", wordsBefore[^take..]) + " " + pattern;
                candidate = candidate.Trim();

                if (candidate.Length > 60)
                    candidate = candidate[..60].Trim();

                if (!string.IsNullOrWhiteSpace(candidate))
                    return candidate;

                searchFrom = idx + 1;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns <c>false</c> when <paramref name="candidate"/> looks like a sentence,
    /// location description, or work-mode description rather than a company name.
    /// Applied by every non-label heuristic.
    /// </summary>
    private static bool IsValidCompanyCandidate(string candidate)
    {
        if (candidate.Length > 60) return false;

        // First-person plural verbs indicate a sentence, not a company name.
        if (OfferParserKeywords.CompanyExclusionVerbs.Any(v =>
                candidate.Contains(v, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Comma followed by a known city → location pattern.
        foreach (var city in OfferParserKeywords.KnownCities)
        {
            if (Regex.IsMatch(candidate, @",\s*" + Regex.Escape(city), RegexOptions.IgnoreCase))
                return false;
        }

        // Work-mode keyword → description, not a company name.
        if (OfferParserKeywords.WorkModeExclusionKeywords.Any(kw =>
                candidate.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Dash or slash → likely a location range, salary, or work-mode note.
        if (candidate.IndexOfAny(['-', '\u2013', '\u2014', '/']) >= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Returns <c>true</c> when all (or all but one) words in <paramref name="line"/> start
    /// with an uppercase letter, or when every letter in the line is uppercase (ALL CAPS).
    /// </summary>
    private static bool IsTitleCaseOrAllCaps(string line)
    {
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return false;

        // ALL CAPS: no lower-case letters at all.
        if (words.All(w => w.All(c => !char.IsLetter(c) || char.IsUpper(c))))
            return true;

        // Title Case: at least (N-1) out of N words start with an uppercase letter.
        int upperCount = words.Count(w => w.Length > 0 && char.IsUpper(w[0]));
        return upperCount >= Math.Max(1, words.Length - 1);
    }
}
