using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Areas.Admin.Models;
using Give_AID.Data;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class UsersController : Controller
{
    private const int PageSize = 15;

    private readonly AppDBContext _context;

    public UsersController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index(string? q, string? role, int page = 1)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u =>
                u.FullName.Contains(term) ||
                u.Email.Contains(term) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)) ||
                (u.City != null && u.City.Contains(term)) ||
                (u.Organisation != null && u.Organisation.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role == role);
        }

        var total = query.Count();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var model = new MembersListViewModel
        {
            Search = q,
            Role = role,
            Page = page,
            PageSize = PageSize,
            TotalCount = total,
            TotalPages = totalPages,
            Roles = _context.Users
                .Where(u => u.Role != null)
                .Select(u => u.Role!)
                .Distinct()
                .OrderBy(r => r)
                .ToList(),
            Members = query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(u => new MemberRow
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    City = u.City,
                    Occupation = u.Occupation,
                    Organisation = u.Organisation,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToList()
        };

        return View(model);
    }
}
