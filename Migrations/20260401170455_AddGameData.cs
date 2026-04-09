using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeBuilder.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddGameData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Generation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Pokemon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Types = table.Column<string>(type: "jsonb", nullable: false),
                    StatHp = table.Column<int>(type: "integer", nullable: false),
                    StatAttack = table.Column<int>(type: "integer", nullable: false),
                    StatDefense = table.Column<int>(type: "integer", nullable: false),
                    StatSpAttack = table.Column<int>(type: "integer", nullable: false),
                    StatSpDefense = table.Column<int>(type: "integer", nullable: false),
                    StatSpeed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pokemon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameDex",
                columns: table => new
                {
                    GameKey = table.Column<string>(type: "character varying(50)", nullable: false),
                    PokemonId = table.Column<int>(type: "integer", nullable: false),
                    DexNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameDex", x => new { x.GameKey, x.PokemonId });
                    table.ForeignKey(
                        name: "FK_GameDex_Games_GameKey",
                        column: x => x.GameKey,
                        principalTable: "Games",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameDex_Pokemon_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePokemonInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameKey = table.Column<string>(type: "character varying(50)", nullable: false),
                    PokemonId = table.Column<int>(type: "integer", nullable: false),
                    ObtainMethods = table.Column<string>(type: "jsonb", nullable: false),
                    Locations = table.Column<string>(type: "jsonb", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePokemonInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePokemonInfo_Games_GameKey",
                        column: x => x.GameKey,
                        principalTable: "Games",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePokemonInfo_Pokemon_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Learnsets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameKey = table.Column<string>(type: "character varying(50)", nullable: false),
                    PokemonId = table.Column<int>(type: "integer", nullable: false),
                    MoveName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LearnMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Learnsets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Learnsets_Games_GameKey",
                        column: x => x.GameKey,
                        principalTable: "Games",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Learnsets_Pokemon_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameDex_PokemonId",
                table: "GameDex",
                column: "PokemonId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePokemonInfo_GameKey_PokemonId",
                table: "GamePokemonInfo",
                columns: new[] { "GameKey", "PokemonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GamePokemonInfo_PokemonId",
                table: "GamePokemonInfo",
                column: "PokemonId");

            migrationBuilder.CreateIndex(
                name: "IX_Learnsets_GameKey_PokemonId_LearnMethod",
                table: "Learnsets",
                columns: new[] { "GameKey", "PokemonId", "LearnMethod" });

            migrationBuilder.CreateIndex(
                name: "IX_Learnsets_PokemonId",
                table: "Learnsets",
                column: "PokemonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameDex");

            migrationBuilder.DropTable(
                name: "GamePokemonInfo");

            migrationBuilder.DropTable(
                name: "Learnsets");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Pokemon");
        }
    }
}
