using System.ComponentModel.DataAnnotations;

public class UpdateUserProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    [Url] public string? LinkedInUrl { get; set; }
    [Url] public string? GitHubUrl { get; set; }
    [Url] public string? WebsiteUrl { get; set; }
}
