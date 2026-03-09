using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarshanaTech.Tenant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Plans])
                BEGIN
                    SET IDENTITY_INSERT [Plans] ON;
                    INSERT INTO [Plans] ([PlanId], [PlanName], [MaxStudents], [MaxTeachers], [Price], [FeaturesJson])
                    VALUES 
                        (1, 'Basic', 100, 10, 49.99, '[""StudentManagement"",""Attendance""]'),
                        (2, 'Premium', 500, 50, 149.99, '[""StudentManagement"",""Attendance"",""Fees"",""Exams""]'),
                        (3, 'Enterprise', 5000, 500, 499.99, '[""StudentManagement"",""Attendance"",""Fees"",""Exams"",""Transport""]');
                    SET IDENTITY_INSERT [Plans] OFF;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Plans] WHERE [PlanId] IN (1, 2, 3)");
        }
    }
}
