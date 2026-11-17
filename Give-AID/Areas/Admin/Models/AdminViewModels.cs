using Give_AID.Models;

namespace Give_AID.Areas.Admin.Models;

public class DashboardViewModel
{
    public int TotalMembers { get; set; }
    public int NewMembersThisMonth { get; set; }
    public int ActiveCauses { get; set; }
    public int TotalCauses { get; set; }
    public int ActiveNgos { get; set; }
    public int TotalNgos { get; set; }
    public int UpcomingProgrammes { get; set; }
    public int TotalProgrammes { get; set; }

    public List<MemberRow> RecentMembers { get; set; } = new();
    public List<Programme> NextProgrammes { get; set; } = new();
}

// Members list ke liye sirf zaroori columns (password hash kabhi view tak nahi jata)
public class MemberRow
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Occupation { get; set; }
    public string? Organisation { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MembersListViewModel
{
    public List<MemberRow> Members { get; set; } = new();
    public string? Search { get; set; }
    public string? Role { get; set; }
    public List<string> Roles { get; set; } = new();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}
