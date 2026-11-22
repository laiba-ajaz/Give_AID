// Requirement doc ke mutabiq About Us ke 7 sub-links.
// Har section ke cards ka form alag hota hai (team member ka "Designation", job ka "Location" ...),
// is liye labels yahan rakhe hain taake admin view aur website dono same labels use karein.
public class AboutSectionInfo
{
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string Icon { get; init; } = "";

    public string ItemName { get; init; } = "";
    public string TitleLabel { get; init; } = "";
    public string? SubtitleLabel { get; init; }
    public string DescriptionLabel { get; init; } = "";
    public bool ItemHasImage { get; init; }
    public string? ImageLabel { get; init; }
}

public static class AboutSections
{
    public const string ReadAboutUs = "read-about-us";
    public const string WhatWeDo = "what-we-do";
    public const string OurMission = "our-mission";
    public const string OurTeam = "our-team";
    public const string Careers = "career-with-us";
    public const string Achievements = "our-achievements";
    public const string Supporters = "our-supporters";

    public static readonly List<AboutSectionInfo> All = new()
    {
        new() { Slug = ReadAboutUs, Title = "Read About Us", Icon = "menu_book",
                ItemName = "milestone", TitleLabel = "Milestone", SubtitleLabel = "Year",
                DescriptionLabel = "What happened" },

        new() { Slug = WhatWeDo, Title = "What We Do", Icon = "volunteer_activism",
                ItemName = "focus area", TitleLabel = "Focus area", SubtitleLabel = "Short tagline",
                DescriptionLabel = "Details", ItemHasImage = true, ImageLabel = "Photo" },

        new() { Slug = OurMission, Title = "Our Mission", Icon = "flag",
                ItemName = "value", TitleLabel = "Value", SubtitleLabel = null,
                DescriptionLabel = "What it means to us" },

        new() { Slug = OurTeam, Title = "Our Team", Icon = "groups",
                ItemName = "team member", TitleLabel = "Full name", SubtitleLabel = "Designation",
                DescriptionLabel = "Short bio", ItemHasImage = true, ImageLabel = "Photo" },

        new() { Slug = Careers, Title = "Career With Us", Icon = "work",
                ItemName = "job opening", TitleLabel = "Job title", SubtitleLabel = "Location / job type",
                DescriptionLabel = "Job details & how to apply" },

        new() { Slug = Achievements, Title = "Our Achievements", Icon = "emoji_events",
                ItemName = "achievement", TitleLabel = "Achievement", SubtitleLabel = "Figure or year (e.g. 12,000+)",
                DescriptionLabel = "Details", ItemHasImage = true, ImageLabel = "Photo" },

        new() { Slug = Supporters, Title = "Our Supporters", Icon = "favorite",
                ItemName = "supporter", TitleLabel = "Name", SubtitleLabel = "Supporter type (e.g. Monthly donor)",
                DescriptionLabel = "Message / quote", ItemHasImage = true, ImageLabel = "Photo or logo" },
    };

    public static AboutSectionInfo Find(string slug) =>
        All.FirstOrDefault(s => s.Slug == slug) ?? All[0];
}
