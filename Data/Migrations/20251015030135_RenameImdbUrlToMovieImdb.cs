using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fall2025_Project3_pslumpkins.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameImdbUrlToMovieImdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImdbUrl",
                table: "Movies",
                newName: "MovieImdb");

            migrationBuilder.AddColumn<string>(
                name: "ActorImdb",
                table: "Actors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorImdb",
                table: "Actors");

            migrationBuilder.RenameColumn(
                name: "MovieImdb",
                table: "Movies",
                newName: "ImdbUrl");
        }
    }
}
