using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseProfessionalUpgradePhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_StaffRoles_StaffRoleId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialMediaPublishingLogs_Users_PublishedByUserId",
                table: "SocialMediaPublishingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsitePublishingLogs_Episodes_EpisodeId",
                table: "WebsitePublishingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsitePublishingLogs_Users_PublishedByUserId",
                table: "WebsitePublishingLogs");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePublishingLogs_PublishedByUserId",
                table: "WebsitePublishingLogs",
                newName: "IX_WebsitePublishingLog_PublishedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePublishingLogs_PublishedAt",
                table: "WebsitePublishingLogs",
                newName: "IX_WebsitePublishingLog_PublishedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePublishingLogs_EpisodeId",
                table: "WebsitePublishingLogs",
                newName: "IX_WebsitePublishingLog_EpisodeId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLogs_PublishedByUserId",
                table: "SocialMediaPublishingLogs",
                newName: "IX_SocialMediaPublishingLog_PublishedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLogs_PublishedAt",
                table: "SocialMediaPublishingLogs",
                newName: "IX_SocialMediaPublishingLog_PublishedAt");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLogs_EpisodeGuestId",
                table: "SocialMediaPublishingLogs",
                newName: "IX_SocialMediaPublishingLog_EpisodeGuestId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLogPlatforms_SocialMediaPublishingLogId",
                table: "SocialMediaPublishingLogPlatforms",
                newName: "IX_LogPlatform_LogId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLogPlatforms_SocialMediaPlatformId",
                table: "SocialMediaPublishingLogPlatforms",
                newName: "IX_LogPlatform_PlatformId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WebsitePublishingLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "WebsitePublishingLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishedAt",
                table: "WebsitePublishingLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WebsitePublishingLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "WebsitePublishingLogs",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WebsitePublishingLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "StaffRoles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "StaffRoles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "StaffRoles",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StaffRoles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialMediaPublishingLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishedAt",
                table: "SocialMediaPublishingLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "SocialMediaPublishingLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SocialMediaPublishingLogs",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SocialMediaPublishingLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ClipTitle",
                table: "SocialMediaPublishingLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "SocialMediaPublishingLogPlatforms",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialMediaPublishingLogPlatforms",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SocialMediaPublishingLogPlatforms",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SocialMediaPublishingLogPlatforms",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialMediaPlatforms",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SocialMediaPlatforms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SocialMediaPlatforms",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "SocialMediaPlatforms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SocialMediaPlatforms",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "BaseUrl",
                table: "SocialMediaPlatforms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ExecutionLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EpisodeGuests",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EpisodeEmployees",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EpisodeCorrespondents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Employees",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Employees",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "DatabaseBackupLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                columns: new[] { "CreatedByUserId", "UpdatedByUserId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                columns: new[] { "CreatedByUserId", "UpdatedByUserId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                columns: new[] { "CreatedByUserId", "UpdatedByUserId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                columns: new[] { "CreatedByUserId", "UpdatedByUserId" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedByUserId",
                table: "Roles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UpdatedByUserId",
                table: "Roles",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_StaffRoles_StaffRoleId",
                table: "Employees",
                column: "StaffRoleId",
                principalTable: "StaffRoles",
                principalColumn: "StaffRoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_CreatedByUserId",
                table: "Roles",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_UpdatedByUserId",
                table: "Roles",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialMediaPublishingLogs_Users_PublishedByUserId",
                table: "SocialMediaPublishingLogs",
                column: "PublishedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WebsitePublishingLogs_Episodes_EpisodeId",
                table: "WebsitePublishingLogs",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "EpisodeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WebsitePublishingLogs_Users_PublishedByUserId",
                table: "WebsitePublishingLogs",
                column: "PublishedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_StaffRoles_StaffRoleId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_CreatedByUserId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_UpdatedByUserId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialMediaPublishingLogs_Users_PublishedByUserId",
                table: "SocialMediaPublishingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsitePublishingLogs_Episodes_EpisodeId",
                table: "WebsitePublishingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsitePublishingLogs_Users_PublishedByUserId",
                table: "WebsitePublishingLogs");

            migrationBuilder.DropIndex(
                name: "IX_Roles_CreatedByUserId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_UpdatedByUserId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Roles");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePublishingLog_PublishedByUserId",
                table: "WebsitePublishingLogs",
                newName: "IX_WebsitePublishingLogs_PublishedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePublishingLog_PublishedAt",
                table: "WebsitePublishingLogs",
                newName: "IX_WebsitePublishingLogs_PublishedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePublishingLog_EpisodeId",
                table: "WebsitePublishingLogs",
                newName: "IX_WebsitePublishingLogs_EpisodeId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLog_PublishedByUserId",
                table: "SocialMediaPublishingLogs",
                newName: "IX_SocialMediaPublishingLogs_PublishedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLog_PublishedAt",
                table: "SocialMediaPublishingLogs",
                newName: "IX_SocialMediaPublishingLogs_PublishedAt");

            migrationBuilder.RenameIndex(
                name: "IX_SocialMediaPublishingLog_EpisodeGuestId",
                table: "SocialMediaPublishingLogs",
                newName: "IX_SocialMediaPublishingLogs_EpisodeGuestId");

            migrationBuilder.RenameIndex(
                name: "IX_LogPlatform_PlatformId",
                table: "SocialMediaPublishingLogPlatforms",
                newName: "IX_SocialMediaPublishingLogPlatforms_SocialMediaPlatformId");

            migrationBuilder.RenameIndex(
                name: "IX_LogPlatform_LogId",
                table: "SocialMediaPublishingLogPlatforms",
                newName: "IX_SocialMediaPublishingLogPlatforms_SocialMediaPublishingLogId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WebsitePublishingLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "WebsitePublishingLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishedAt",
                table: "WebsitePublishingLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WebsitePublishingLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "WebsitePublishingLogs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WebsitePublishingLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "StaffRoles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "StaffRoles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "StaffRoles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StaffRoles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialMediaPublishingLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishedAt",
                table: "SocialMediaPublishingLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "SocialMediaPublishingLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SocialMediaPublishingLogs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SocialMediaPublishingLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "ClipTitle",
                table: "SocialMediaPublishingLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "SocialMediaPublishingLogPlatforms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialMediaPublishingLogPlatforms",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SocialMediaPublishingLogPlatforms",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SocialMediaPublishingLogPlatforms",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialMediaPlatforms",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SocialMediaPlatforms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SocialMediaPlatforms",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "SocialMediaPlatforms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SocialMediaPlatforms",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "BaseUrl",
                table: "SocialMediaPlatforms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ExecutionLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EpisodeGuests",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EpisodeEmployees",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EpisodeCorrespondents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "DatabaseBackupLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_StaffRoles_StaffRoleId",
                table: "Employees",
                column: "StaffRoleId",
                principalTable: "StaffRoles",
                principalColumn: "StaffRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SocialMediaPublishingLogs_Users_PublishedByUserId",
                table: "SocialMediaPublishingLogs",
                column: "PublishedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebsitePublishingLogs_Episodes_EpisodeId",
                table: "WebsitePublishingLogs",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "EpisodeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebsitePublishingLogs_Users_PublishedByUserId",
                table: "WebsitePublishingLogs",
                column: "PublishedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
