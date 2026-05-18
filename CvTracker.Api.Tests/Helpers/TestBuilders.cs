using CvTracker.Api.Models;

namespace CvTracker.Api.Tests.Helpers;

public static class TestBuilders
{
    public static Company BuildCompany(
        int id = 1,
        string name = "Acme Corp",
        string address = "123 Main St")
    {
        return new Company
        {
            Id = id,
            CompanyName = name,
            CompanyAddress = address
        };
    }

    public static JobOffer BuildJobOffer(
        int id = 1,
        int companyId = 1,
        string position = "Software Engineer",
        ApplicationStatus status = ApplicationStatus.Draft)
    {
        return new JobOffer
        {
            Id = id,
            Position = position,
            Salary = 10_000m,
            ContractType = ContractType.B2B,
            WorkLoad = WorkLoad.FullTime,
            WorkMode = WorkMode.Remote,
            CompanyId = companyId,
            Status = status
        };
    }

    public static JobOfferDto BuildJobOfferDto(
        int companyId = 1,
        string position = "Software Engineer",
        ApplicationStatus status = ApplicationStatus.Draft)
    {
        return new JobOfferDto
        {
            Position = position,
            Salary = 10_000m,
            ContractType = ContractType.B2B,
            WorkLoad = WorkLoad.FullTime,
            WorkMode = WorkMode.Remote,
            CompanyId = companyId,
            Status = status
        };
    }

    public static CreateCompanyDto BuildCreateCompanyDto(
        string name = "Acme Corp",
        string address = "123 Main St")
    {
        return new CreateCompanyDto
        {
            CompanyName = name,
            CompanyAddress = address
        };
    }
}
