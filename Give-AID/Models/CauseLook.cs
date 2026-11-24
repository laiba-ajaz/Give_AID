// Website par cause/programme ka icon aur (agar admin ne image na di ho) default photo.
// Naam ke hisaab se andaza lagata hai, taake "Education", "Health" jaise causes sahi icon ke saath dikhen.
public static class CauseLook
{
    public static string Icon(string? causeName)
    {
        var n = (causeName ?? "").ToLowerInvariant();
        if (n.Contains("educat") || n.Contains("school") || n.Contains("literacy")) return "menu_book";
        if (n.Contains("health") || n.Contains("medic") || n.Contains("hospital")) return "medical_services";
        if (n.Contains("disab") || n.Contains("special")) return "accessible";
        if (n.Contains("child")) return "child_care";
        if (n.Contains("women") || n.Contains("woman")) return "female";
        if (n.Contains("elder") || n.Contains("senior")) return "elderly";
        if (n.Contains("youth")) return "psychology";
        if (n.Contains("food") || n.Contains("hunger") || n.Contains("nutrition")) return "restaurant";
        if (n.Contains("disaster") || n.Contains("flood") || n.Contains("relief")) return "emergency";
        return "volunteer_activism";
    }

    public static string DefaultImage(string? causeName)
    {
        var n = (causeName ?? "").ToLowerInvariant();
        if (n.Contains("educat") || n.Contains("school")) return "/images/home-9.jpg";
        if (n.Contains("health") || n.Contains("medic")) return "/images/programmes-24.jpg";
        if (n.Contains("disab") || n.Contains("special")) return "/images/home-10.jpg";
        if (n.Contains("child")) return "/images/home-8.jpg";
        if (n.Contains("women") || n.Contains("woman")) return "/images/home-11.jpg";
        if (n.Contains("elder") || n.Contains("senior")) return "/images/home-13.jpg";
        if (n.Contains("youth")) return "/images/home-12.jpg";
        return "/images/home-17.jpg";
    }
}
