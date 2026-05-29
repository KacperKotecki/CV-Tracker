using CvTracker.Api.Services;
using FluentAssertions;
using Xunit;

namespace CvTracker.Api.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SalaryParser.ParseSalary"/>.
/// Each test exercises a distinct formatting variant or business rule.
/// </summary>
public class SalaryParserTests
{
    // ── Happy-path formatting variants ────────────────────────────────────────

    [Fact]
    public void ParseSalary_SingleValueWithSpaceSeparator_ReturnsSameMinMax()
    {
        // "10 000 PLN" — space thousand separator
        var (min, max) = SalaryParser.ParseSalary("Wynagrodzenie: 10 000 PLN brutto miesięcznie.");

        min.Should().Be(10_000m);
        max.Should().Be(10_000m);
    }

    [Fact]
    public void ParseSalary_RangeAsciiDash_ReturnsBothEnds()
    {
        // "8000-12000 PLN" — no separator, ASCII dash
        var (min, max) = SalaryParser.ParseSalary("8000-12000 PLN brutto.");

        min.Should().Be(8_000m);
        max.Should().Be(12_000m);
    }

    [Fact]
    public void ParseSalary_RangeEnDash_ReturnsBothEnds()
    {
        // "8 000 – 12 000 zł" — space separator, Unicode en-dash
        var (min, max) = SalaryParser.ParseSalary("Zarobki: 8 000 \u2013 12 000 z\u0142 brutto.");

        min.Should().Be(8_000m);
        max.Should().Be(12_000m);
    }

    [Fact]
    public void ParseSalary_ShortFormKWithCurrency_ReturnsScaledValue()
    {
        // "10k PLN" — k suffix + currency keyword
        var (min, max) = SalaryParser.ParseSalary("Offer: 10k PLN brutto.");

        min.Should().Be(10_000m);
        max.Should().Be(10_000m);
    }

    [Fact]
    public void ParseSalary_ShortFormRangeKNoCurrency_ReturnsBothEnds()
    {
        // "8k–12k" — k suffix, en-dash, no PLN/zł
        var (min, max) = SalaryParser.ParseSalary("Salary: 8k\u201312k");

        min.Should().Be(8_000m);
        max.Should().Be(12_000m);
    }

    [Fact]
    public void ParseSalary_DotThousandSeparator_ReturnsParsedValue()
    {
        // "10.000 zł" — dot as thousand separator (exactly 3 digits follow)
        var (min, max) = SalaryParser.ParseSalary("Wynagrodzenie 10.000 z\u0142 brutto.");

        min.Should().Be(10_000m);
        max.Should().Be(10_000m);
    }

    [Fact]
    public void ParseSalary_CommaThousandSeparator_ReturnsParsedValue()
    {
        // "10,000 PLN" — comma as thousand separator
        var (min, max) = SalaryParser.ParseSalary("10,000 PLN brutto.");

        min.Should().Be(10_000m);
        max.Should().Be(10_000m);
    }

    // ── Netto / B2B multiplier rules ─────────────────────────────────────────

    [Fact]
    public void ParseSalary_NettoWithoutB2B_NoMultiplier()
    {
        // "netto" present but no "B2B" — multiplier must NOT be applied.
        var (min, max) = SalaryParser.ParseSalary("Wynagrodzenie 10 000 PLN netto miesięcznie.");

        min.Should().Be(10_000m);
        max.Should().Be(10_000m);
    }

    [Fact]
    public void ParseSalary_NettoAndB2B_AppliesMultiplier()
    {
        // "netto" + "B2B" → multiply by 1.23, rounded to 0 decimal places.
        var (min, max) = SalaryParser.ParseSalary("10 000 PLN netto B2B.");

        min.Should().Be(Math.Round(10_000m * 1.23m, 0));
        max.Should().Be(Math.Round(10_000m * 1.23m, 0));
    }

    [Fact]
    public void ParseSalary_RangeNettoAndB2B_AppliesMultiplierToBothEnds()
    {
        // Range "8000–12000 PLN netto B2B" → both ends multiplied.
        var (min, max) = SalaryParser.ParseSalary("8000-12000 PLN netto B2B.");

        min.Should().Be(Math.Round(8_000m * 1.23m, 0));
        max.Should().Be(Math.Round(12_000m * 1.23m, 0));
    }

    [Fact]
    public void ParseSalary_NoSalaryKeywordsInText_ReturnsBothNull()
    {
        // No numbers near PLN/zł and no k-suffix pattern.
        var (min, max) = SalaryParser.ParseSalary("Praca zdalna, nowoczesne biuro, benefity.");

        min.Should().BeNull();
        max.Should().BeNull();
    }

    [Fact]
    public void ParseSalary_NumbersWithoutCurrencyKeyword_ReturnsBothNull()
    {
        // Numbers present but no PLN/zł and no k suffix → must not match.
        var (min, max) = SalaryParser.ParseSalary("Wymagania: 5 lat doświadczenia i 3 projekty.");

        min.Should().BeNull();
        max.Should().BeNull();
    }

    [Fact]
    public void ParseSalary_BruttoLabelWithB2B_NoMultiplier()
    {
        // "brutto" present overrides "netto" — multiplier must NOT be applied
        // even if "B2B" also appears in text.
        var (min, max) = SalaryParser.ParseSalary("10 000 PLN brutto B2B.");

        min.Should().Be(10_000m);
        max.Should().Be(10_000m);
    }
}
