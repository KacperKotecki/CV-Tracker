using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedAvatarExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedResumeExtensions = [".pdf", ".doc", ".docx"];

    private static UserProfileDto MapToDto(UserProfile profile, List<UserSkill> skills)
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
            Skills = skills.Select(s => new UserSkillDto
            {
                Id = s.Id,
                Category = s.Category,
                SkillName = s.SkillName,
                Proficiency = s.Proficiency,
            }).ToList(),
        };
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> Get()
    {
        var profile = await db.UserProfiles.FindAsync(1);
        var skills = await db.UserSkills.ToListAsync();

        if (profile == null)
        {
            return Ok(new UserProfileDto
            {
                Id = 0,
                Skills = skills.Select(s => new UserSkillDto
                {
                    Id = s.Id,
                    Category = s.Category,
                    SkillName = s.SkillName,
                    Proficiency = s.Proficiency,
                }).ToList(),
            });
        }

        return Ok(MapToDto(profile, skills));
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

        var skills = await db.UserSkills.ToListAsync();
        return Ok(MapToDto(profile, skills));
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
    public async Task<ActionResult<List<UserSkillDto>>> UpdateSkills(
        [FromBody] UpdateUserSkillsRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await db.UserSkills.ToListAsync();
        db.UserSkills.RemoveRange(existing);

        db.UserSkills.AddRange(req.Skills.Select(s => new UserSkill
        {
            Category = s.Category,
            SkillName = s.SkillName,
            Proficiency = s.Proficiency,
        }));

        await db.SaveChangesAsync();

        return Ok(db.UserSkills.Select(s => new UserSkillDto
        {
            Id = s.Id,
            Category = s.Category,
            SkillName = s.SkillName,
            Proficiency = s.Proficiency,
        }).ToList());
    }
}
