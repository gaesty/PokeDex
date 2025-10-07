# Cours 

## Docker command

 - Create container with PostgreSQL image 
`docker run --name postgres-db -e POSTGRES_PASSWORD={PASSWORD} -e POSTGRES_USER={USER} -e POSTGRES_DB={DATABASE} -p 5432:5432 -v postgres-data:/var/lib/postgresql/data -d postgres`

 - Show all containers running
`docker ps`

 - Create the volume 
`docker volume create postgres-data`

 - Inspect the wolume
`docker volume inspect postgres-data`

 - Create a custom Image with dotnet and API program
` docker build -f .\BourgPalette\Dockerfile -t bourgpalette:latest .`

 - Create a container with the custom Image
` docker run --rm -p 8080:8080 -e "Swagger__Enabled=true" --name bourgpalette bourgpalette:latest`

 - Down Docker-Compose
` docker compose down -v`

  - Up and Build Docker-Compose
`docker compose up --build`


## Link

[Tutorial Docker PostgreSQL](https://www.datacamp.com/tutorial/postgresql-docker?dc_referrer=https%3A%2F%2Fwww.google.com%2F)

[Documentation Docker HealthCheck](https://docs.docker.com/reference/dockerfile/#healthcheck)

[Tutorial API DotNet](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-9.0&tabs=visual-studio-code)

[Documentation EF Core](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli)

## DotNet

 - Run the file Program.cs
`dotnet run`

 - Migrate database 
`dotnet-ef migrations add InitialCreate`

 - Update database
`dotnet-ef database update`

## Database 

![Diagram Database](Untitled.png)



<!-- ```
// Pokémon DB - schéma DBML généré d'après la diapositive

Table species {
  id int [pk, increment]            // Identifiant de l'espèce
  pokedex_number int
  name_fr varchar(100)
  name_en varchar(100)
  generation int
  region varchar(50)
}

Table media {
  id int [pk, increment]
  sprite_url text
  artwork_url text
  note text                         // champ libre pour préciser le type d'image si besoin
}

Table pokemon {
  id int [pk, increment]
  species_id int
  form varchar(50)                  // ex: Alola, forme shiny, etc.
  height double
  weight double
  base_experience int
  image_id int
}

Table types {
  id int [pk, increment]
  name varchar(50)
  description text
}

Table pokemon_types {
  pokemon_id int [pk]
  type_id int [pk]
}

Table abilities {
  id int [pk, increment]
  name varchar(100)
  description text
}

Table pokemon_abilities {
  pokemon_id int [pk]
  ability_id int [pk]
  is_hidden boolean                 // facultatif : talent caché
}

Table moves {
  id int [pk, increment]
  name varchar(150)
  description text
  type_id int
  power int                         // nullable si pas applicable
  accuracy double
  pp int
  category varchar(50)              // phys/spec/status (optionnel)
}

Table stats {
  id int [pk, increment]
  pokemon_id int
  hp int
  attack int
  defense int
  special_attack int
  special_defense int
  speed int
}

Table evolutions {
  id int [pk, increment]
  from_pokemon_id int
  to_pokemon_id int
  condition text                    // ex: "niveau 16", "Pierre d'éveil", "objet X"
}

Table teams {
  id int [pk, increment]
  name varchar(150)
  description text
  owner varchar(100)                // optionnel: user/owner
}

Table team_pokemons {
  team_id int [pk]
  pokemon_id int [pk]
  slot int                          // position dans l'équipe (optionnel)
}

/* Relations / Foreign keys */
Ref: pokemon.species_id > species.id
Ref: pokemon.image_id > media.id

Ref: pokemon_types.pokemon_id > pokemon.id
Ref: pokemon_types.type_id > types.id

Ref: pokemon_abilities.pokemon_id > pokemon.id
Ref: pokemon_abilities.ability_id > abilities.id

Ref: moves.type_id > types.id

Ref: stats.pokemon_id > pokemon.id

Ref: evolutions.from_pokemon_id > pokemon.id
Ref: evolutions.to_pokemon_id > pokemon.id

Ref: team_pokemons.team_id > teams.id
Ref: team_pokemons.pokemon_id > pokemon.id

``` -->