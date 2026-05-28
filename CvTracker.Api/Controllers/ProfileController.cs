using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedAvatarExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedResumeExtensions = [".pdf", ".doc", ".docx"];

    private static UserProfileDto MapToDto(UserProfile profile, List<UserTechnology> technologies)
    {
        return new UserProfileDto
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Location = profile.Location,
            LinkedInUrl = profile.LinkedInUrl,
            GitHubUrl = profile.GitHubUrl,
            WebsiteUrl = profile.WebsiteUrl,
            AvatarUrl = profile.AvatarFileName != null
                ? $"/uploads/avatars/{profile.AvatarFileName}"
                : null,
            ResumeFileName = profile.ResumeFileName,
            ResumeUrl = profile.ResumeFileName != null
                ? $"/uploads/resumes/{profile.ResumeFileName}"
                : null,
            Skills = technologies.Select(t => new UserTechnologyDto
            {
                Id = t.Id,
                TechnologyId = t.TechnologyId,
                TechnologyName = t.Technology.Name,
                Category = t.Technology.Category,
                Proficiency = t.Proficiency,
            }).ToList(),
        };
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> Get()
    {
        var profile = await db.UserProfiles.FindAsync(1);
        var technologies = await db.UserTechnologies.Include(ut => ut.Technology).ToListAsync();

        if (profile == null)
        {
            return Ok(new UserProfileDto
            {
                Id = 0,
                Skills = technologies.Select(t => new UserTechnologyDto
                {
                    Id = t.Id,
                    TechnologyId = t.TechnologyId,
                    TechnologyName = t.Technology.Name,
                    Category = t.Technology.Category,
                    Proficiency = t.Proficiency,
                }).ToList(),
            });
        }

        return Ok(MapToDto(profile, technologies));
    }

    [HttpPut]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        [FromBody] UpdateUserProfileRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var profile = await db.UserProfiles.FindAsync(1);
        if (profile == null)
        {
            profile = new UserProfile { Id = 1 };
            db.UserProfiles.Add(profile);
        }

        profile.FirstName = req.FirstName;
        profile.LastName = req.LastName;
        profile.Location = req.Location;
        profile.LinkedInUrl = req.LinkedInUrl;
        profile.GitHubUrl = req.GitHubUrl;
        profile.WebsiteUrl = req.WebsiteUrl;

        await db.SaveChangesAsync();

        var technologies = await db.UserTechnologies.Include(ut => ut.Technology).ToListAsync();
        return Ok(MapToDto(profile, technologies));
    }

    [HttpPost("avatar")]
    public async Task<ActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedAvatarExtensions.Contains(ext))
            return BadRequest($"Unsupported file type. Allowed: {string.Join(", ", AllowedAvatarExtensions)}");

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"avatar-1{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        var profile = await db.UserProfiles.FindAsync(1);
        if (profile == null)
        {
            profile = new UserProfile { Id = 1 };
            db.UserProfiles.Add(profile);
        }

        profile.AvatarFileName = fileName;
        await db.SaveChangesAsync();

        return Ok(new { avatarUrl = $"/uploads/avatars/{fileName}" });
    }

    [HttpPost("resume")]
    public async Task<ActionResult> UploadResume(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedResumeExtensions.Contains(ext))
            return BadRequest($"Unsupported file type. Allowed: {string.Join(", ", AllowedResumeExtensions)}");

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "resumes");
        Directory.CreateDirectory(uploadsDir);

        var safeFileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadsDir, safeFileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        var profile = await db.UserProfiles.FindAsync(1);
        if (profile == null)
        {
            profile = new UserProfile { Id = 1 };
            db.UserProfiles.Add(profile);
        }

        profile.ResumeFileName = safeFileName;
        await db.SaveChangesAsync();

        return Ok(new { resumeFileName = safeFileName, resumeUrl = $"/uploads/resumes/{safeFileName}" });
    }

    [HttpPut("skills")]
    public async Task<ActionResult<List<UserTechnologyDto>>> UpdateSkills(
        [FromBody] UpdateUserTechnologiesRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await db.UserTechnologies.ToListAsync();
        db.UserTechnologies.RemoveRange(existing);

        db.UserTechnologies.AddRange(req.Technologies.Select(t => new UserTechnology
        {
            TechnologyId = t.TechnologyId,
            Proficiency = t.Proficiency,
        }));

        await db.SaveChangesAsync();

        var saved = await db.UserTechnologies.Include(ut => ut.Technology).ToListAsync();

        return Ok(saved.Select(t => new UserTechnologyDto
        {
            Id = t.Id,
            TechnologyId = t.TechnologyId,
            TechnologyName = t.Technology.Name,
            Category = t.Technology.Category,
            Proficiency = t.Proficiency,
        }).ToList());
    }
}
