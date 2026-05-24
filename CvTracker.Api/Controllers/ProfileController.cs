using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CvTracker.Api.Services;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController(
    AppDbContext db,
    IWebHostEnvironment env,
    IJobOfferService jobOfferService) : ControllerBase
{
    private static readonly string[] AllowedAvatarExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedResumeExtensions = [".pdf", ".doc", ".docx"];

    /// <summary>Maps a UserProfile entity and its associated skills to a DTO.</summary>
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
            Skills = skills.Select(MapSkillToDto).ToList(),
        };
    }

    private static UserSkillDto MapSkillToDto(UserSkill s) => new()
    {
        Id         = s.Id,
        SkillId    = s.SkillId,
        Category   = s.Skill.Category ?? string.Empty,
        SkillName  = s.Skill.Name,
        Proficiency = s.Proficiency,
    };

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> Get()
    {
        var profile = await db.UserProfiles.FindAsync(1);
        var skills = await db.UserSkills
            .Where(s => s.UserId == 1)
            .Include(s => s.Skill)
            .ToListAsync();

        if (profile is null)
        {
            return Ok(new UserProfileDto
            {
                Id = 0,
                Skills = skills.Select(MapSkillToDto).ToList(),
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
        if (profile is null)
        {
            profile = new UserProfile { Id = 1 };
            db.UserProfiles.Add(profile);
        }

        profile.FirstName   = req.FirstName;
        profile.LastName    = req.LastName;
        profile.Location    = req.Location;
        profile.LinkedInUrl = req.LinkedInUrl;
        profile.GitHubUrl   = req.GitHubUrl;
        profile.WebsiteUrl  = req.WebsiteUrl;

        await db.SaveChangesAsync();

        var skills = await db.UserSkills
            .Where(s => s.UserId == 1)
            .Include(s => s.Skill)
            .ToListAsync();

        return Ok(MapToDto(profile, skills));
    }

    [HttpPost("avatar")]
    public async Task<ActionResult> UploadAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
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
        if (profile is null)
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
        if (file is null || file.Length == 0)
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
        if (profile is null)
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

        // Delete-reinsert runs inside a single UoW: all changes committed atomically.
        var existing = await db.UserSkills.Where(s => s.UserId == 1).ToListAsync();
        db.UserSkills.RemoveRange(existing);

        foreach (var s in req.Skills)
        {
            // Resolve (or create) the canonical Skill entity for each requested item.
            var skill = await FindOrCreateSkillAsync(s.SkillId, s.SkillName, s.Category);
            db.UserSkills.Add(new UserSkill
            {
                SkillId     = skill.Id,
                UserId      = 1,
                Proficiency = s.Proficiency,
            });
        }

        await db.SaveChangesAsync();

        // Recompute MatchScore for all offers now that user skills have changed.
        await jobOfferService.RecomputeAllMatchScoresAsync();

        var skills = await db.UserSkills
            .Where(s => s.UserId == 1)
            .Include(s => s.Skill)
            .ToListAsync();

        return Ok(skills.Select(MapSkillToDto).ToList());
    }

    /// <summary>
    /// Finds an existing <see cref="Skill"/> by ID or name (case-insensitive),
    /// or creates a new one if neither lookup succeeds.
    /// </summary>
    private async Task<Skill> FindOrCreateSkillAsync(int skillId, string name, string? category)
    {
        if (skillId > 0)
        {
            var byId = await db.Skills.FindAsync(skillId);
            if (byId is not null) return byId;
        }

        var trimmedName = name.Trim();
        var existing = await db.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == trimmedName.ToLower());

        if (existing is not null) return existing;

        var skill = new Skill { Name = trimmedName, Category = category };
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        return skill;
    }
}

