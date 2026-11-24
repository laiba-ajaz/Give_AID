namespace Give_AID.Data;

// App start hone par About Us ke 7 pages DB mein na hon to bana deta hai.
// Sirf pehli dafa chalta hai; uske baad admin jo bhi change kare wahi rehta hai.
public static class AboutSeeder
{
    public static void EnsurePages(AppDBContext db)
    {
        var existing = db.AboutPages.Select(p => p.Slug).ToList();
        int order = 0;

        foreach (var section in AboutSections.All)
        {
            order++;
            if (existing.Contains(section.Slug)) continue;

            var page = new AboutPage
            {
                Slug = section.Slug,
                Title = section.Title,
                SortOrder = order
            };
            FillDefaults(page);
            db.AboutPages.Add(page);
        }

        db.SaveChanges();
    }

    private static void FillDefaults(AboutPage page)
    {
        switch (page.Slug)
        {
            case AboutSections.ReadAboutUs:
                page.Heading = "Bridging generosity with grassroots resilience.";
                page.Content =
                    "Give-AID is a non-governmental organisation that brings many NGOs together on one platform. " +
                    "These NGOs run welfare activities and raise funds to help people in need.\n" +
                    "Give-AID was set up with the support of several leading companies. Their contribution covers " +
                    "our running costs, so the money you donate goes to the cause you choose.";
                page.Items.Add(Item("Give-AID founded", "2019", "Started with five partner NGOs and a small team of volunteers."));
                page.Items.Add(Item("First corporate partners", "2021", "Leading companies joined to fund our operating costs."));
                page.Items.Add(Item("Online donations launched", "2024", "Donors can now pick a cause and give securely through the website."));
                break;

            case AboutSections.WhatWeDo:
                page.Heading = "Connecting donors with NGOs that work on the ground.";
                page.Content =
                    "Our member NGOs run school drives, health camps, food distribution and care programmes for " +
                    "children, disabled and elderly people.\n" +
                    "Donors choose the cause they care about and pay securely online. We verify every NGO before " +
                    "it is listed and share updates on the programmes your money supports.";
                page.Items.Add(Item("Education", "Books, uniforms and school fees", "We help children from low-income families stay in school."));
                page.Items.Add(Item("Health care", "Free medical camps", "Doctors and volunteers hold regular check-up camps in underserved areas."));
                page.Items.Add(Item("Special care", "Children and disabled persons", "Support for specially challenged children and people with disabilities."));
                break;

            case AboutSections.OurMission:
                page.Heading = "Help should reach the people who need it – quickly and honestly.";
                page.Content =
                    "Our mission is to make giving simple and trustworthy. Every donation made through Give-AID goes " +
                    "to a verified cause, and every NGO on our platform is accountable for how it is spent.";
                page.Items.Add(Item("Transparency", null, "Donors can see which cause and programme their money supports."));
                page.Items.Add(Item("Compassion", null, "We treat every person we serve with dignity and respect."));
                page.Items.Add(Item("Accountability", null, "Partner NGOs report back on every programme they run."));
                break;

            case AboutSections.OurTeam:
                page.Heading = "The people behind Give-AID.";
                page.Content = "A small team of staff and volunteers who manage partnerships, verify NGOs and coordinate programmes in the field.";
                break;

            case AboutSections.Careers:
                page.Heading = "Build a career that makes a difference.";
                page.Content =
                    "We are looking for people who want to use their skills for social good – in programme " +
                    "coordination, fundraising, design and field work.\n" +
                    "If there is no suitable opening right now, you can still send us your CV through the Help Centre.";
                break;

            case AboutSections.Achievements:
                page.Heading = "What we have achieved together.";
                page.Content = "None of this would be possible without our donors, partner companies and NGO volunteers.";
                page.Items.Add(Item("Registered donors", "12,000+", "People who give through Give-AID."));
                page.Items.Add(Item("Partner NGOs", "85", "Verified NGOs listed on our platform."));
                page.Items.Add(Item("Medical camps", "120", "Free health camps held in rural areas."));
                break;

            case AboutSections.Supporters:
                page.Heading = "Thank you to everyone who stands with us.";
                page.Content = "Our work is funded by individual donors, volunteers and companies who believe in what we do.";
                break;
        }
    }

    private static AboutItem Item(string title, string? subtitle, string description) => new()
    {
        Title = title,
        Subtitle = subtitle,
        Description = description
    };
}
