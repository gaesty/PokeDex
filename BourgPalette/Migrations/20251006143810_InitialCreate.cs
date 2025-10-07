using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BourgPalette.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "abilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpriteUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ArtworkUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    pokedex_number = table.Column<int>(type: "INTEGER", nullable: true),
                    name_fr = table.Column<string>(type: "TEXT", nullable: true),
                    name_en = table.Column<string>(type: "TEXT", nullable: true),
                    Generation = table.Column<int>(type: "INTEGER", nullable: true),
                    Region = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_species", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Owner = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pokemon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    species_id = table.Column<int>(type: "INTEGER", nullable: true),
                    form = table.Column<string>(type: "TEXT", nullable: true),
                    height = table.Column<double>(type: "REAL", nullable: true),
                    weight = table.Column<double>(type: "REAL", nullable: true),
                    base_experience = table.Column<int>(type: "INTEGER", nullable: true),
                    image_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pokemon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pokemon_media_image_id",
                        column: x => x.image_id,
                        principalTable: "media",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_pokemon_species_species_id",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "moves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    type_id = table.Column<int>(type: "INTEGER", nullable: true),
                    power = table.Column<int>(type: "INTEGER", nullable: true),
                    accuracy = table.Column<double>(type: "REAL", nullable: true),
                    pp = table.Column<int>(type: "INTEGER", nullable: true),
                    category = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_moves_types_type_id",
                        column: x => x.type_id,
                        principalTable: "types",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "evolutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    from_pokemon_id = table.Column<int>(type: "INTEGER", nullable: true),
                    to_pokemon_id = table.Column<int>(type: "INTEGER", nullable: true),
                    condition = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evolutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_evolutions_pokemon_from_pokemon_id",
                        column: x => x.from_pokemon_id,
                        principalTable: "pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evolutions_pokemon_to_pokemon_id",
                        column: x => x.to_pokemon_id,
                        principalTable: "pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pokemon_abilities",
                columns: table => new
                {
                    pokemon_id = table.Column<int>(type: "INTEGER", nullable: false),
                    ability_id = table.Column<int>(type: "INTEGER", nullable: false),
                    is_hidden = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pokemon_abilities", x => new { x.pokemon_id, x.ability_id });
                    table.ForeignKey(
                        name: "FK_pokemon_abilities_abilities_ability_id",
                        column: x => x.ability_id,
                        principalTable: "abilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pokemon_abilities_pokemon_pokemon_id",
                        column: x => x.pokemon_id,
                        principalTable: "pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pokemon_types",
                columns: table => new
                {
                    pokemon_id = table.Column<int>(type: "INTEGER", nullable: false),
                    type_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pokemon_types", x => new { x.pokemon_id, x.type_id });
                    table.ForeignKey(
                        name: "FK_pokemon_types_pokemon_pokemon_id",
                        column: x => x.pokemon_id,
                        principalTable: "pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pokemon_types_types_type_id",
                        column: x => x.type_id,
                        principalTable: "types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    pokemon_id = table.Column<int>(type: "INTEGER", nullable: true),
                    hp = table.Column<int>(type: "INTEGER", nullable: true),
                    attack = table.Column<int>(type: "INTEGER", nullable: true),
                    defense = table.Column<int>(type: "INTEGER", nullable: true),
                    special_attack = table.Column<int>(type: "INTEGER", nullable: true),
                    special_defense = table.Column<int>(type: "INTEGER", nullable: true),
                    speed = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stats_pokemon_pokemon_id",
                        column: x => x.pokemon_id,
                        principalTable: "pokemon",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "team_pokemons",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "INTEGER", nullable: false),
                    pokemon_id = table.Column<int>(type: "INTEGER", nullable: false),
                    slot = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_pokemons", x => new { x.team_id, x.pokemon_id });
                    table.ForeignKey(
                        name: "FK_team_pokemons_pokemon_pokemon_id",
                        column: x => x.pokemon_id,
                        principalTable: "pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_pokemons_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evolutions_from_pokemon_id",
                table: "evolutions",
                column: "from_pokemon_id");

            migrationBuilder.CreateIndex(
                name: "IX_evolutions_to_pokemon_id",
                table: "evolutions",
                column: "to_pokemon_id");

            migrationBuilder.CreateIndex(
                name: "IX_moves_type_id",
                table: "moves",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_pokemon_image_id",
                table: "pokemon",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "IX_pokemon_species_id",
                table: "pokemon",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "IX_pokemon_abilities_ability_id",
                table: "pokemon_abilities",
                column: "ability_id");

            migrationBuilder.CreateIndex(
                name: "IX_pokemon_types_type_id",
                table: "pokemon_types",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_stats_pokemon_id",
                table: "stats",
                column: "pokemon_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_pokemons_pokemon_id",
                table: "team_pokemons",
                column: "pokemon_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evolutions");

            migrationBuilder.DropTable(
                name: "moves");

            migrationBuilder.DropTable(
                name: "pokemon_abilities");

            migrationBuilder.DropTable(
                name: "pokemon_types");

            migrationBuilder.DropTable(
                name: "stats");

            migrationBuilder.DropTable(
                name: "team_pokemons");

            migrationBuilder.DropTable(
                name: "abilities");

            migrationBuilder.DropTable(
                name: "types");

            migrationBuilder.DropTable(
                name: "pokemon");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "species");
        }
    }
}
