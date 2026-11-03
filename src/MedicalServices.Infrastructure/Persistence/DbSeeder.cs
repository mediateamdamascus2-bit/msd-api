using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedicalServices.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var db = sp.GetRequiredService<AppDbContext>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var config = sp.GetRequiredService<IConfiguration>();
        var env = sp.GetRequiredService<IHostEnvironment>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.FacilityUser, AppRoles.BookingClerk, AppRoles.StationUser })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }

        if (!await db.Facilities.AnyAsync())
        {
            db.Facilities.AddRange(SeedFacilities());
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded facilities.");
        }
        else
        {
            var hasia = await db.Facilities.FirstOrDefaultAsync(f => f.Id == "hasia");
            if (hasia is not null && hasia.NameAr != "سجن حسيا")
            {
                hasia.NameAr = "سجن حسيا";
                await db.SaveChangesAsync();
            }
        }

        var seedDemos = config.GetValue<bool?>("Deployment:SeedDemoAccounts") ?? env.IsDevelopment();
        if (seedDemos)
        {
            await SeedDemoAccountsAsync(users, logger);
        }
        else
        {
            logger.LogInformation("Demo account seeding disabled (Deployment:SeedDemoAccounts=false).");
            await SeedBootstrapAdminAsync(users, logger, config);
        }
    }

    private static async Task SeedBootstrapAdminAsync(
        UserManager<ApplicationUser> users,
        ILogger logger,
        IConfiguration config)
    {
        var userName = config["Deployment:BootstrapAdmin:UserName"]?.Trim();
        var password = config["Deployment:BootstrapAdmin:Password"];
        var display = config["Deployment:BootstrapAdmin:DisplayName"]?.Trim();
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No bootstrap admin configured. Set Deployment:BootstrapAdmin:UserName and Password for first production boot, or create users manually.");
            return;
        }

        await EnsureUserAsync(users, logger,
            userName: userName,
            displayName: string.IsNullOrWhiteSpace(display) ? "مدير النظام" : display,
            password: password,
            role: AppRoles.Admin,
            facilityId: null,
            homePath: "/admin",
            resetPasswordIfExists: false);
    }

    private static async Task SeedDemoAccountsAsync(UserManager<ApplicationUser> users, ILogger logger)
    {
        await EnsureUserAsync(users, logger,
            userName: "admin",
            displayName: "مدير الخدمات الطبية",
            password: "1234",
            role: AppRoles.Admin,
            facilityId: null,
            homePath: "/admin");

        await EnsureUserAsync(users, logger,
            userName: "harasta",
            displayName: "مدير مشفى حرستا",
            password: "1234",
            role: AppRoles.FacilityUser,
            facilityId: "harasta",
            homePath: "/f/harasta");

        await EnsureUserAsync(users, logger,
            userName: "reception",
            displayName: "موظف استقبال حرستا",
            password: "1234",
            role: AppRoles.StationUser,
            facilityId: "harasta",
            homePath: "/stations/harasta/reception");

        await EnsureUserAsync(users, logger,
            userName: "unit-homs",
            displayName: "مستخدم نقطة طبية حمص",
            password: "1234",
            role: AppRoles.FacilityUser,
            facilityId: "unit-homs-2",
            homePath: "/f/unit-homs-2");

        await EnsureUserAsync(users, logger,
            userName: "hasia",
            displayName: "المسؤول الطبي لسجن حسيا",
            password: "1234",
            role: AppRoles.FacilityUser,
            facilityId: "hasia",
            homePath: "/f/hasia");

        foreach (var st in new (string User, string Display, string Home)[]
                 {
                     ("hasia-intake", "محطة استقبال النزيل — حسيا", "/stations/hasia/intake"),
                     ("hasia-triage", "محطة الفرز — حسيا", "/stations/hasia/triage"),
                     ("hasia-cases", "محطة الحالات — حسيا", "/stations/hasia/cases"),
                     ("hasia-ph-int", "محطة الصيدلية الداخلية — حسيا", "/stations/hasia/ph-internal"),
                     ("hasia-ph-ext", "محطة الصيدلية الخارجية — حسيا", "/stations/hasia/ph-external"),
                 })
        {
            await EnsureUserAsync(users, logger,
                userName: st.User,
                displayName: st.Display,
                password: "1234",
                role: AppRoles.StationUser,
                facilityId: "hasia",
                homePath: st.Home);
        }

        await EnsureUserAsync(users, logger,
            userName: "booking",
            displayName: "موظف حجز الإدارات",
            password: "1234",
            role: AppRoles.BookingClerk,
            facilityId: "harasta",
            homePath: "/stations/harasta/booking");

        await EnsureUserAsync(users, logger,
            userName: "clinic-live",
            displayName: "محطة إحصائيات العيادات",
            password: "1234",
            role: AppRoles.StationUser,
            facilityId: "harasta",
            homePath: "/stations/harasta/clinic-live");

        await EnsureUserAsync(users, logger,
            userName: "display",
            displayName: "شاشة الاستدعاء",
            password: "1234",
            role: AppRoles.StationUser,
            facilityId: "harasta",
            homePath: "/stations/harasta/display");

        foreach (var clinic in new (string User, string Display, string Clinic)[]
                 {
                     ("clinic-internal", "محطة عيادة الداخلية", "الداخلية"),
                     ("clinic-cardio", "محطة عيادة القلبية", "القلبية"),
                     ("clinic-peds", "محطة عيادة الأطفال", "الأطفال"),
                     ("clinic-neonatal", "محطة عيادة حديثي الولادة", "حديثي الولادة"),
                     ("clinic-obgyn", "محطة عيادة النسائية والتوليد", "النسائية والتوليد"),
                     ("clinic-derm", "محطة عيادة الجلدية", "الجلدية"),
                     ("clinic-gensurg", "محطة عيادة الجراحة العامة", "الجراحة العامة"),
                     ("clinic-ortho", "محطة عيادة الجراحة العظمية", "الجراحة العظمية"),
                     ("clinic-eye", "محطة عيادة العينية", "العينية"),
                     ("clinic-ent", "محطة عيادة الأذنية", "الأذنية"),
                     ("clinic-uro", "محطة عيادة البولية", "البولية"),
                     ("clinic-neuro", "محطة عيادة العصبية", "العصبية"),
                     ("clinic-dental", "محطة عيادة السنية", "السنية"),
                     ("clinic-kidney", "محطة عيادة الكلية وغسيل الكلى", "الكلية وغسيل الكلى"),
                     ("clinic-physio", "محطة عيادة العلاج الفيزيائي", "العلاج الفيزيائي"),
                     ("clinic-er", "محطة الإسعاف", "الإسعاف"),
                 })
        {
            await EnsureUserAsync(users, logger,
                userName: clinic.User,
                displayName: clinic.Display,
                password: "1234",
                role: AppRoles.StationUser,
                facilityId: "harasta",
                homePath: $"/stations/harasta/clinic/{clinic.Clinic}");
        }

        await EnsureUserAsync(users, logger,
            userName: "dept-lab",
            displayName: "محطة المخبر",
            password: "1234",
            role: AppRoles.StationUser,
            facilityId: "harasta",
            homePath: "/stations/harasta/dept/المخبر");

        await EnsureUserAsync(users, logger,
            userName: "dept-pharmacy",
            displayName: "محطة الصيدلية",
            password: "1234",
            role: AppRoles.StationUser,
            facilityId: "harasta",
            homePath: "/stations/harasta/dept/الصيدلية");
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> users,
        ILogger logger,
        string userName,
        string displayName,
        string password,
        string role,
        string? facilityId,
        string homePath,
        bool resetPasswordIfExists = false)
    {
        var existing = await users.FindByNameAsync(userName);
        if (existing is not null)
        {
            var dirty = false;
            if (!await users.IsInRoleAsync(existing, role))
            {
                await users.AddToRoleAsync(existing, role);
                logger.LogInformation("Added role {Role} to existing user {User}.", role, userName);
            }

            if (!string.Equals(existing.HomePath, homePath, StringComparison.Ordinal))
            {
                existing.HomePath = homePath;
                dirty = true;
            }

            if (!string.Equals(existing.FacilityId, facilityId, StringComparison.Ordinal))
            {
                existing.FacilityId = facilityId;
                dirty = true;
            }

            if (!string.Equals(existing.DisplayNameAr, displayName, StringComparison.Ordinal))
            {
                existing.DisplayNameAr = displayName;
                dirty = true;
            }

            if (dirty)
            {
                await users.UpdateAsync(existing);
                logger.LogInformation("Updated profile for {User} → {Path}.", userName, homePath);
            }

            if (resetPasswordIfExists)
            {
                var token = await users.GeneratePasswordResetTokenAsync(existing);
                await users.ResetPasswordAsync(existing, token, password);
                logger.LogInformation("Reset password for bootstrap user {User}.", userName);
            }

            return;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            DisplayNameAr = displayName,
            FacilityId = facilityId,
            HomePath = homePath,
            EmailConfirmed = true
        };

        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed user {User}: {Errors}",
                userName, string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await users.AddToRoleAsync(user, role);
        logger.LogInformation("Seeded user {User} with role {Role} → {Path}.", userName, role, homePath);
    }

    private static IEnumerable<Facility> SeedFacilities() =>
    [
        new Facility { Id = "harasta", NameAr = "مشفى حرستا", Type = FacilityType.Hospital, ProvinceAr = "ريف دمشق" },
        new Facility { Id = "mujtahed", NameAr = "مشفى المجتهد", Type = FacilityType.Hospital, ProvinceAr = "دمشق" },
        new Facility { Id = "hasia", NameAr = "سجن حسيا", Type = FacilityType.PrisonClinic, ProvinceAr = "حمص" },
        new Facility { Id = "sednaya", NameAr = "عيادة سجن صيدنايا", Type = FacilityType.PrisonClinic, ProvinceAr = "ريف دمشق" },
        new Facility { Id = "unit-homs-2", NameAr = "نقطة طبية 2 — حمص", Type = FacilityType.MedicalUnit, ProvinceAr = "حمص" },
        new Facility { Id = "unit-aleppo-1", NameAr = "وحدة طبية 1 — حلب", Type = FacilityType.MedicalUnit, ProvinceAr = "حلب" },
        new Facility { Id = "center-lattakia", NameAr = "مركز رعاية اللاذقية", Type = FacilityType.MedicalCenter, ProvinceAr = "اللاذقية" },
        new Facility { Id = "wh-central", NameAr = "المستودع المركزي", Type = FacilityType.Warehouse, ProvinceAr = "دمشق" },
        new Facility { Id = "wh-north", NameAr = "مستودع الشمال", Type = FacilityType.Warehouse, ProvinceAr = "حلب", IsActive = false }
    ];
}
