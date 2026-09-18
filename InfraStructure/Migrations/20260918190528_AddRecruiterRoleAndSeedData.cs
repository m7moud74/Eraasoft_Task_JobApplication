using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterRoleAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "11111111-1111-1111-1111-111111111111", "admin-role-stamp", "Admin", "ADMIN" },
                    { "22222222-2222-2222-2222-222222222222", "candidate-role-stamp", "Candidate", "CANDIDATE" },
                    { "33333333-3333-3333-3333-333333333333", "recruiter-role-stamp", "Recruiter", "RECRUITER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "CandidateId", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", 0, null, "admin-con-stamp", "admin@trackapplication.com", true, false, null, "ADMIN@TRACKAPPLICATION.COM", "ADMIN@TRACKAPPLICATION.COM", "AQAAAAIAAYagAAAAEP8UZUlR0DWLRbTRXSQVaoukedY3JGlSv7Inqz2M99Z83fpD2lZCKDdQ/hFpf3CA/g==", null, false, "admin-sec-stamp", false, "admin@trackapplication.com" },
                    { "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", 0, null, "recruiter-con-stamp", "recruiter@trackapplication.com", true, false, null, "RECRUITER@TRACKAPPLICATION.COM", "RECRUITER@TRACKAPPLICATION.COM", "AQAAAAIAAYagAAAAEGQ+uwT28xXzpK1jkjEdy9/6cQnw5EYi5eSrIvNbkL1bs6ZXFfvVSKIyaUbVtn89CA==", null, false, "recruiter-sec-stamp", false, "recruiter@trackapplication.com" }
                });

            migrationBuilder.InsertData(
                table: "Candidates",
                columns: new[] { "Id", "CvUrl", "Email", "Name" },
                values: new object[,]
                {
                    { 1, "https://example.com/cvs/ahmed.pdf", "ahmed@example.com", "Ahmed Ali" },
                    { 2, "https://example.com/cvs/sara.pdf", "sara@example.com", "Sara Mohamed" }
                });

            migrationBuilder.InsertData(
                table: "Jobs",
                columns: new[] { "Id", "CreatedByUserId", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Build scalable web APIs and microservices using ASP.NET Core", true, "Senior .NET Developer" },
                    { 2, "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Develop responsive single-page applications", true, "Frontend Angular Developer" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "11111111-1111-1111-1111-111111111111", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" },
                    { "33333333-3333-3333-3333-333333333333", "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "CandidateId", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "cccccccc-cccc-cccc-cccc-cccccccccccc", 0, 1, "candidate1-con-stamp", "ahmed@example.com", true, false, null, "AHMED@EXAMPLE.COM", "AHMED@EXAMPLE.COM", "AQAAAAIAAYagAAAAEEL0j8uzjSBk/PwElVXAvhwnqJD4KkEXThsjSqEdgQHEZkgVIJl6KRxV7QoK15t3mQ==", null, false, "candidate1-sec-stamp", false, "ahmed@example.com" },
                    { "dddddddd-dddd-dddd-dddd-dddddddddddd", 0, 2, "candidate2-con-stamp", "sara@example.com", true, false, null, "SARA@EXAMPLE.COM", "SARA@EXAMPLE.COM", "AQAAAAIAAYagAAAAELOdWxLfCLqfsNoxSh6YuOea+qy5gCtZWleyxSC/OpP09V3A6c6RA738LX5V4FnGsg==", null, false, "candidate2-sec-stamp", false, "sara@example.com" }
                });

            migrationBuilder.InsertData(
                table: "JobCandidateApplications",
                columns: new[] { "Id", "AppliedAt", "CancelledAt", "CandidateId", "JobApplicationStatus", "JobId", "StatusUpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 0, 1, new DateTime(2026, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 1, 2, new DateTime(2026, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 2, 1, new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2026, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 5, 2, new DateTime(2026, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "22222222-2222-2222-2222-222222222222", "cccccccc-cccc-cccc-cccc-cccccccccccc" },
                    { "22222222-2222-2222-2222-222222222222", "dddddddd-dddd-dddd-dddd-dddddddddddd" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "11111111-1111-1111-1111-111111111111", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "33333333-3333-3333-3333-333333333333", "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "22222222-2222-2222-2222-222222222222", "cccccccc-cccc-cccc-cccc-cccccccccccc" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "22222222-2222-2222-2222-222222222222", "dddddddd-dddd-dddd-dddd-dddddddddddd" });

            migrationBuilder.DeleteData(
                table: "JobCandidateApplications",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JobCandidateApplications",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JobCandidateApplications",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "JobCandidateApplications",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cccccccc-cccc-cccc-cccc-cccccccccccc");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "dddddddd-dddd-dddd-dddd-dddddddddddd");

            migrationBuilder.DeleteData(
                table: "Jobs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Jobs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Candidates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Candidates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Jobs");
        }
    }
}
