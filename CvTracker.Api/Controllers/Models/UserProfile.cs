using System.ComponentModel.DataAnnotations;

public class UserProfile
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    [Url] public string? LinkedInUrl { get; set; }
    [Url] public string? GitHubUrl { get; set; }
    [Url] public string? WebsiteUrl { get; set; }
    public string? AvatarFileName { get; set; }
    public string? ResumeFileName { get; set; }
}
