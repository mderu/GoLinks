using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoLinks.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Owner = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShortLink = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DestinationLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumUses = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoLinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoLinks_Id_ShortLink_Owner_NumUses",
                table: "GoLinks",
                columns: new[] { "Id", "ShortLink", "Owner", "NumUses" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoLinks");
        }
    }
}
