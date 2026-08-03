-- SQL Server / Azure SQL reference schema.
-- The EF Core InitialCreate migration is the authoritative schema.

-- Players
CREATE TABLE Players (
    PlayerId    INT IDENTITY(1, 1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL UNIQUE,
    CreatedOn   DATETIME2     NOT NULL
);

-- High Scores
CREATE TABLE HighScores (
    ScoreId      INT IDENTITY(1, 1) PRIMARY KEY,
    PlayerId     INTEGER        NOT NULL REFERENCES Players(PlayerId),
    Score        INTEGER        NOT NULL,
    LevelReached INTEGER        NOT NULL,
    PlayedOn     DATETIME2     NOT NULL
);

-- Levels
CREATE TABLE Levels (
    LevelId      INT IDENTITY(1, 1) PRIMARY KEY,
    LevelNumber  INT            NOT NULL UNIQUE,
    MazeLayout   NVARCHAR(MAX)  NOT NULL,
    GhostSpeed   INT            NOT NULL,
    PelletCount  INT            NOT NULL
);

-- Settings (key/value tunables)
CREATE TABLE Settings (
    [Key]  NVARCHAR(100) NOT NULL PRIMARY KEY,
    Value  NVARCHAR(255) NOT NULL
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
INSERT INTO Players (Name, CreatedOn) VALUES ('ACE',  SYSUTCDATETIME());
INSERT INTO Players (Name, CreatedOn) VALUES ('DEMO', SYSUTCDATETIME());

INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn)
VALUES (1, 15200, 3, SYSUTCDATETIME());
INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn)
VALUES (2,  8400, 2, SYSUTCDATETIME());
