using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DestinyMatrix.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arcanas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Archetype = table.Column<string>(type: "TEXT", nullable: false),
                    Energy_Plus = table.Column<string>(type: "TEXT", nullable: false),
                    Energy_Minus = table.Column<string>(type: "TEXT", nullable: false),
                    ZonesJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arcanas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Arcanas");
        }
    }
}
