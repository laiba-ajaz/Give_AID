namespace Give_AID.Services;

// Admin ki upload ki hui images wwwroot/uploads/<folder>/ mein save hoti hain,
// DB mein sirf unka path ("/uploads/causes/xyz.jpg") jata hai.
public class ImageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxBytes = 2 * 1024 * 1024; // 2 MB

    private readonly IWebHostEnvironment _env;

    public ImageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>Validates and saves the file. Returns its public path, or throws InvalidOperationException with a user-friendly message.</summary>
    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Only JPG, PNG, WEBP or GIF images are allowed.");
        if (file.Length == 0)
            throw new InvalidOperationException("The selected image is empty.");
        if (file.Length > MaxBytes)
            throw new InvalidOperationException("Image must be smaller than 2 MB.");

        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        await using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{folder}/{fileName}";
    }

    /// <summary>Deletes a previously uploaded image. Ignores empty paths and anything outside /uploads/.</summary>
    public void Delete(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/uploads/")) return;

        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
        var fullPath = Path.GetFullPath(Path.Combine(_env.WebRootPath, path.TrimStart('/')));
        if (!fullPath.StartsWith(uploadsRoot)) return;

        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}
