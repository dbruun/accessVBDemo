-- SQLite schema and seed data for ChompMan.
CREATE TABLE Players (
    PlayerId   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name       TEXT NOT NULL,
    CreatedOn  TEXT NOT NULL
);

CREATE TABLE HighScores (
    ScoreId       INTEGER PRIMARY KEY AUTOINCREMENT,
    PlayerId      INTEGER NOT NULL REFERENCES Players(PlayerId),
    Score         INTEGER NOT NULL,
    LevelReached  INTEGER NOT NULL,
    PlayedOn      TEXT NOT NULL
);

CREATE TABLE Levels (
    LevelId       INTEGER PRIMARY KEY AUTOINCREMENT,
    LevelNumber   INTEGER NOT NULL,
    MazeLayout    TEXT NOT NULL,
    GhostSpeed    INTEGER NOT NULL,
    PelletCount   INTEGER NOT NULL
);

CREATE TABLE Settings (
    [Key] TEXT NOT NULL PRIMARY KEY,
    Value TEXT NOT NULL
);

INSERT INTO Settings ([Key], Value) VALUES ('StartingLives', '3');
INSERT INTO Settings ([Key], Value) VALUES ('PlayerSpeedTicks', '4');
INSERT INTO Settings ([Key], Value) VALUES ('GhostSpeedTicks', '6');
INSERT INTO Settings ([Key], Value) VALUES ('FrightenedDuration', '180');
INSERT INTO Settings ([Key], Value) VALUES ('PlayerColour', 'Yellow');
INSERT INTO Settings ([Key], Value) VALUES ('WallColour', 'DarkBlue');
INSERT INTO Settings ([Key], Value) VALUES ('PelletColour', 'White');
INSERT INTO Settings ([Key], Value) VALUES ('PowerPelletColour', 'Orange');

INSERT INTO Players (Name, CreatedOn) VALUES ('ACE', CURRENT_TIMESTAMP);
INSERT INTO Players (Name, CreatedOn) VALUES ('DEMO', CURRENT_TIMESTAMP);
INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn) VALUES (1, 15200, 3, CURRENT_TIMESTAMP);
INSERT INTO HighScores (PlayerId, Score, LevelReached, PlayedOn) VALUES (2, 8400, 2, CURRENT_TIMESTAMP);
