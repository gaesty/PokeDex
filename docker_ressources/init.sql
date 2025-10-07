-- CREATE DATABASE pokedex ENCODING = 'UTF8';


CREATE USER pokedex WITH ENCRYPTED PASSWORD 'pokedex';

GRANT CONNECT ON DATABASE pokedex TO pokedex;
GRANT USAGE ON SCHEMA public TO pokedex;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO pokedex;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO pokedex;

\connect pokedex

CREATE SCHEMA IF NOT EXISTS "pokedex";

GRANT ALL PRIVILEGES ON SCHEMA "pokedex" TO pokedex;
GRANT USAGE ON SCHEMA "pokedex" TO pokedex;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA "pokedex" TO pokedex;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA "pokedex" TO pokedex;

--set default schema
ALTER USER pokedex SET search_path = "pokedex";

SET search_path TO "pokedex";

CREATE OR REPLACE LANGUAGE plpgsql;