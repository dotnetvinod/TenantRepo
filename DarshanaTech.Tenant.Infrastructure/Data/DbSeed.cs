using Microsoft.EntityFrameworkCore;
using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Infrastructure.Data;

public static class DbSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Plans.AnyAsync()) return;

        await context.Database.ExecuteSqlRawAsync(@"
            SET IDENTITY_INSERT [Plans] ON;
            INSERT INTO [Plans] ([PlanId], [PlanName], [MaxStudents], [MaxTeachers], [Price], [FeaturesJson])
            VALUES 
                (1, 'Basic', 100, 10, 49.99, '[""StudentManagement"",""Attendance""]'),
                (2, 'Premium', 500, 50, 149.99, '[""StudentManagement"",""Attendance"",""Fees"",""Exams""]'),
                (3, 'Enterprise', 5000, 500, 499.99, '[""StudentManagement"",""Attendance"",""Fees"",""Exams"",""Transport""]');
            SET IDENTITY_INSERT [Plans] OFF;
        ");
    }
}
