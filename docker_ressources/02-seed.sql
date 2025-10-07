BEGIN;

-- Seed Types (normalized)
INSERT INTO "types" ("description") VALUES ('Grass')   ON CONFLICT ("description") DO NOTHING;
INSERT INTO "types" ("description") VALUES ('Poison')  ON CONFLICT ("description") DO NOTHING;
INSERT INTO "types" ("description") VALUES ('Fire')    ON CONFLICT ("description") DO NOTHING;
INSERT INTO "types" ("description") VALUES ('Water')   ON CONFLICT ("description") DO NOTHING;
INSERT INTO "types" ("description") VALUES ('Electric')ON CONFLICT ("description") DO NOTHING;
INSERT INTO "types" ("description") VALUES ('Psychic') ON CONFLICT ("description") DO NOTHING;

-- Seed Abilities (normalized)
INSERT INTO "abilities" ("description") VALUES ('Overgrow') ON CONFLICT ("description") DO NOTHING;
INSERT INTO "abilities" ("description") VALUES ('Blaze')     ON CONFLICT ("description") DO NOTHING;
INSERT INTO "abilities" ("description") VALUES ('Torrent')   ON CONFLICT ("description") DO NOTHING;
INSERT INTO "abilities" ("description") VALUES ('Static')    ON CONFLICT ("description") DO NOTHING;
INSERT INTO "abilities" ("description") VALUES ('Pressure')  ON CONFLICT ("description") DO NOTHING;

-- Seed Species/Media minimal if absent
INSERT INTO "species" ("region") VALUES ('Kanto') ON CONFLICT ("region") DO NOTHING;
INSERT INTO "media" ("note") VALUES ('placeholder') ON CONFLICT DO NOTHING;

-- Helper CTEs to get IDs
-- Ensure named Pokemon exist (aligned with schema)
DO $$
DECLARE m_id INT; s_id INT;
BEGIN
  SELECT id INTO m_id FROM media ORDER BY id LIMIT 1;
  SELECT id INTO s_id FROM species ORDER BY id LIMIT 1;
  IF m_id IS NULL THEN
    INSERT INTO media (note) VALUES ('placeholder');
    SELECT id INTO m_id FROM media ORDER BY id LIMIT 1;
  END IF;
  IF s_id IS NULL THEN
    INSERT INTO species (region) VALUES ('Kanto');
    SELECT id INTO s_id FROM species ORDER BY id LIMIT 1;
  END IF;
  INSERT INTO pokemon (name,is_legendary,species_id,image_id) VALUES
    ('Bulbasaur', FALSE, s_id, m_id),
    ('Charmander', FALSE, s_id, m_id),
    ('Squirtle', FALSE, s_id, m_id),
    ('Pikachu', FALSE, s_id, m_id),
    ('Mewtwo', TRUE, s_id, m_id)
  ON CONFLICT (name) DO NOTHING;
END $$;

-- Map Pokemon Types
DO $$
DECLARE
  p_id INT; t_id INT; sp_pdx INT; t_name TEXT;
BEGIN
  -- Bulbasaur: Grass, Poison
  SELECT id INTO p_id FROM pokemon WHERE name='Bulbasaur';
  IF p_id IS NOT NULL THEN
    SELECT id INTO t_id FROM types WHERE description='Grass';
    IF t_id IS NOT NULL THEN INSERT INTO "pokemon_types" ("pokemon_id","type_id") SELECT p_id,t_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_types" WHERE "pokemon_id"=p_id AND "type_id"=t_id); END IF;
    SELECT id INTO t_id FROM types WHERE description='Poison';
    IF t_id IS NOT NULL THEN INSERT INTO "pokemon_types" ("pokemon_id","type_id") SELECT p_id,t_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_types" WHERE "pokemon_id"=p_id AND "type_id"=t_id); END IF;
  END IF;
  -- Charmander: Fire
  SELECT id INTO p_id FROM pokemon WHERE name='Charmander';
  IF p_id IS NOT NULL THEN
    SELECT id INTO t_id FROM types WHERE description='Fire';
    IF t_id IS NOT NULL THEN INSERT INTO "pokemon_types" ("pokemon_id","type_id") SELECT p_id,t_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_types" WHERE "pokemon_id"=p_id AND "type_id"=t_id); END IF;
  END IF;
  -- Squirtle: Water
  SELECT id INTO p_id FROM pokemon WHERE name='Squirtle';
  IF p_id IS NOT NULL THEN
    SELECT id INTO t_id FROM types WHERE description='Water';
    IF t_id IS NOT NULL THEN INSERT INTO "pokemon_types" ("pokemon_id","type_id") SELECT p_id,t_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_types" WHERE "pokemon_id"=p_id AND "type_id"=t_id); END IF;
  END IF;
  -- Pikachu: Electric
  SELECT id INTO p_id FROM pokemon WHERE name='Pikachu';
  IF p_id IS NOT NULL THEN
    SELECT id INTO t_id FROM types WHERE description='Electric';
    IF t_id IS NOT NULL THEN INSERT INTO "pokemon_types" ("pokemon_id","type_id") SELECT p_id,t_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_types" WHERE "pokemon_id"=p_id AND "type_id"=t_id); END IF;
  END IF;
  -- Mewtwo: Psychic
  SELECT id INTO p_id FROM pokemon WHERE name='Mewtwo';
  IF p_id IS NOT NULL THEN
    SELECT id INTO t_id FROM types WHERE description='Psychic';
    IF t_id IS NOT NULL THEN INSERT INTO "pokemon_types" ("pokemon_id","type_id") SELECT p_id,t_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_types" WHERE "pokemon_id"=p_id AND "type_id"=t_id); END IF;
  END IF;
END $$;

-- Map Abilities
DO $$
DECLARE
  p_id INT; a_id INT;
BEGIN
  -- Bulbasaur: Overgrow
  SELECT id INTO p_id FROM pokemon WHERE name='Bulbasaur';
  SELECT id INTO a_id FROM abilities WHERE description='Overgrow';
  IF p_id IS NOT NULL AND a_id IS NOT NULL THEN
    INSERT INTO "pokemon_abilities" ("pokemon_id","ability_id")
    SELECT p_id,a_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_abilities" WHERE "pokemon_id"=p_id AND "ability_id"=a_id);
  END IF;
  -- Charmander: Blaze
  SELECT id INTO p_id FROM pokemon WHERE name='Charmander';
  SELECT id INTO a_id FROM abilities WHERE description='Blaze';
  IF p_id IS NOT NULL AND a_id IS NOT NULL THEN
    INSERT INTO "pokemon_abilities" ("pokemon_id","ability_id")
    SELECT p_id,a_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_abilities" WHERE "pokemon_id"=p_id AND "ability_id"=a_id);
  END IF;
  -- Squirtle: Torrent
  SELECT id INTO p_id FROM pokemon WHERE name='Squirtle';
  SELECT id INTO a_id FROM abilities WHERE description='Torrent';
  IF p_id IS NOT NULL AND a_id IS NOT NULL THEN
    INSERT INTO "pokemon_abilities" ("pokemon_id","ability_id")
    SELECT p_id,a_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_abilities" WHERE "pokemon_id"=p_id AND "ability_id"=a_id);
  END IF;
  -- Pikachu: Static
  SELECT id INTO p_id FROM pokemon WHERE name='Pikachu';
  SELECT id INTO a_id FROM abilities WHERE description='Static';
  IF p_id IS NOT NULL AND a_id IS NOT NULL THEN
    INSERT INTO "pokemon_abilities" ("pokemon_id","ability_id")
    SELECT p_id,a_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_abilities" WHERE "pokemon_id"=p_id AND "ability_id"=a_id);
  END IF;
  -- Mewtwo: Pressure
  SELECT id INTO p_id FROM pokemon WHERE name='Mewtwo';
  SELECT id INTO a_id FROM abilities WHERE description='Pressure';
  IF p_id IS NOT NULL AND a_id IS NOT NULL THEN
    INSERT INTO "pokemon_abilities" ("pokemon_id","ability_id")
    SELECT p_id,a_id WHERE NOT EXISTS (SELECT 1 FROM "pokemon_abilities" WHERE "pokemon_id"=p_id AND "ability_id"=a_id);
  END IF;
END $$;

-- Seed Stats (idempotent)
DO $$
DECLARE p_id INT; BEGIN
  SELECT id INTO p_id FROM pokemon WHERE name='Bulbasaur';
  IF p_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "stats" WHERE "pokemon_id"=p_id) THEN
    INSERT INTO "stats" ("pokemon_id","speed") VALUES (p_id,45);
  END IF;
  SELECT id INTO p_id FROM pokemon WHERE name='Charmander';
  IF p_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "stats" WHERE "pokemon_id"=p_id) THEN
    INSERT INTO "stats" ("pokemon_id","speed") VALUES (p_id,65);
  END IF;
  SELECT id INTO p_id FROM pokemon WHERE name='Squirtle';
  IF p_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "stats" WHERE "pokemon_id"=p_id) THEN
    INSERT INTO "stats" ("pokemon_id","speed") VALUES (p_id,43);
  END IF;
  SELECT id INTO p_id FROM pokemon WHERE name='Pikachu';
  IF p_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "stats" WHERE "pokemon_id"=p_id) THEN
    INSERT INTO "stats" ("pokemon_id","speed") VALUES (p_id,90);
  END IF;
  SELECT id INTO p_id FROM pokemon WHERE name='Mewtwo';
  IF p_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "stats" WHERE "pokemon_id"=p_id) THEN
    INSERT INTO "stats" ("pokemon_id","speed") VALUES (p_id,130);
  END IF;
END $$;

COMMIT;
