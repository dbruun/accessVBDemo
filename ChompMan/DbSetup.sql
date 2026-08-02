-- ============================================================
-- ChompMan Database Setup Script
-- This script documents the schema; actual database creation
-- is performed at runtime by DatabaseInitializer.vb using
-- ADOX (COM) + DDL executed via OleDbCommand.
-- ============================================================

-- Players
CREATE TABLE Players (
    PlayerId    AUTOINCREMENT  PRIMARY KEY,
    Name        TEXT(100)      NOT NULL,
    CreatedOn   DATETIME       NOT NULL
);

-- High Scores
CREATE TABLE HighScores (
    ScoreId      AUTOINCREMENT  PRIMARY KEY,
    PlayerId     INTEGER        NOT NULL REFERENCES Players(PlayerId),
    Score        INTEGER        NOT NULL,
    LevelReached INTEGER        NOT NULL,
    PlayedOn     DATETIME       NOT NULL
);

-- Levels
CREATE TABLE Levels (
    LevelId      AUTOINCREMENT  PRIMARY KEY,
    LevelNumber  INTEGER        NOT NULL,
    MazeLayout   MEMO           NOT NULL,    -- long-text maze string
    GhostSpeed   INTEGER        NOT NULL,    -- ticks-per-move (lower = faster)
    PelletCount  INTEGER        NOT NULL
);

-- Settings (key/value tunables)
CREATE TABLE Settings (
    [Key]  TEXT(100)  NOT NULL PRIMARY KEY,
    Value  TEXT(255)  NOT NULL
);

-- ── Seed data ──────────────────────────────────────────────────────────────

INSERT INTO Settings ([Key], Value) VALUES ('StartingLives',    '3');
INSERT INTO Settings ([Key], Value) VALUES ('PlayerSpeedTicks', '4');
INSERT INTO Settings ([Key], Value) VALUES ('GhostSpeedTicks',  '6');
INSERT INTO Settings ([Key], Value) VALUES ('FrightenedDuration','180');
INSERT INTO Settings ([Key], Value) VALUES ('PlayerColour',     'Yellow');
INSERT INTO Settings ([Key], Value) VALUES ('WallColour',       'DarkBlue');
INSERT INTO Settings ([Key], Value) VALUES ('PelletColour',     'White');
INSERT INTO Settings ([Key], Value) VALUES ('PowerPelletColour','Orange');

-- Sample players & scores (seed data)
INSERT INTO Players (Name, CreatedOn) VALUES ('ACE',  NOW());
INSERT INTO Players (Name, CreatedOn) VALUES ('DEMO', NOW());

INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn)
VALUES (1, 15200, 3, NOW());
INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn)
VALUES (2,  8400, 2, NOW());
