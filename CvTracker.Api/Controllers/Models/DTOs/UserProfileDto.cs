public class UserProfileDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ResumeFileName { get; set; }
    public string? ResumeUrl { get; set; }
    public List<UserTechnologyDto> Skills { get; set; } = [];
}
