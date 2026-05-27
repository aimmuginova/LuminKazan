using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminKazan.Migrations
{
    /// <inheritdoc />
    public partial class CreateScheduleBlocksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),

                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),

                    Reason = table.Column<string>(
                        type: "nvarchar(300)",
                        maxLength: 300,
                        nullable: true),

                    IsActive = table.Column<bool>(type: "bit", nullable: false),

                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleBlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleBlocks_StartAt_EndAt_IsActive",
                table: "ScheduleBlocks",
                columns: new[] { "StartAt", "EndAt", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleBlocks");
        }
    }
}
