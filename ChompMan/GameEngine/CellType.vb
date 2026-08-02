Namespace GameEngine

    ''' <summary>Types of cells that can appear in a maze grid.</summary>
    Public Enum CellType
        ''' <summary>Impassable wall.</summary>
        Wall
        ''' <summary>Regular pellet worth 10 points.</summary>
        Pellet
        ''' <summary>Power pellet worth 50 points; frightens ghosts.</summary>
        PowerPellet
        ''' <summary>Open corridor — no collectible.</summary>
        Empty
        ''' <summary>Player spawn cell (treated as Empty after parse).</summary>
        PlayerStart
        ''' <summary>Ghost spawn cell (treated as Empty after parse).</summary>
        GhostStart
        ''' <summary>Ghost-house door — ghosts can pass, player cannot.</summary>
        GhostHouseDoor
    End Enum

End Namespace
