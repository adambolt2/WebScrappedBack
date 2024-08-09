using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrappedBack.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTablewithbools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IndeedSubscribed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LinkedInSubscribed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TotalJobsSubscribed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndeedSubscribed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LinkedInSubscribed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalJobsSubscribed",
                table: "Users");
        }
    }
}
