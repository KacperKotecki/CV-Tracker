using CvTracker.Api.Models;
using CvTracker.Api.Services.Scraping;
using FluentAssertions;
using Xunit;

namespace CvTracker.Api.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SalaryParser"/>.
/// Static class — no DI or EF Core needed.
/// </summary>
public class SalaryParserTests
{
    [Fact]
    public void Parse_PolishRangeWithContractType_ReturnsMinMaxAndUoP()
    {
        var (min, max, contractType) = SalaryParser.Parse("od 10 000 do 15 000 PLN brutto (UoP)");

        min.Should().Be(10_000m);
        max.Should().Be(15_000m);
        contractType.Should().Be(ContractType.UoP);
    }

    [Fact]
    public void Parse_DashRange_ReturnsMinMax()
    {
        var (min, max, contractType) = SalaryParser.Parse("10 000–15 000 PLN gross");

        min.Should().Be(10_000m);
        max.Should().Be(15_000m);
        // "gross" maps to UoP because the UoP regex includes \bbrutto\b|\bgross\b
        contractType.Should().Be(ContractType.UoP);
    }

    [Fact]
    public void Parse_SingleValueB2B_ReturnsBothSidesAndB2BContractType()
    {
        var (min, max, contractType) = SalaryParser.Parse("10 000 zł netto B2B");

        min.Should().Be(10_000m);
        max.Should().Be(10_000m);
        contractType.Should().Be(ContractType.B2B);
    }

    [Fact]
    public void Parse_NullInput_ReturnsAllNulls()
    {
        var (min, max, contractType) = SalaryParser.Parse(null);

        min.Should().BeNull();
        max.Should().BeNull();
        contractType.Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyString_ReturnsAllNulls()
    {
        var (min, max, contractType) = SalaryParser.Parse(string.Empty);

        min.Should().BeNull();
        max.Should().BeNull();
        contractType.Should().BeNull();
    }

    [Fact]
    public void Parse_NonBreakingSpaceThousandSeparator_IsHandled()
    {
        // U+00A0 non-breaking space as thousands separator — common on Polish job boards.
        var (min, max, _) = SalaryParser.Parse("10\u00A0000\u2013015\u00A0000 PLN");

        min.Should().Be(10_000m);
        max.Should().NotBeNull();
    }

    [Fact]
    public void Parse_NoNumbers_ReturnsNullMinMax()
    {
        var (min, max, contractType) = SalaryParser.Parse("Negotiable B2B");

        min.Should().BeNull();
        max.Should().BeNull();
        contractType.Should().Be(ContractType.B2B);
    }

    [Fact]
    public void Parse_NoContractKeyword_ReturnsNullContractType()
    {
        var (min, max, contractType) = SalaryParser.Parse("8 000–12 000 PLN");

        min.Should().Be(8_000m);
        max.Should().Be(12_000m);
        contractType.Should().BeNull();
    }
}
