local enums = require("consts.celeste_enums")

local bellows = {}

bellows.name = "WindHelper/Bellows"
bellows.depth = -8500
bellows.texture = "Sherplung/WindHelper/Bellows/BellowsLoenn"

bellows.fieldInformation = {
    orientation = {
        options = enums.spring_orientations,
        editable = false
    }
}

bellows.placements = {
    {
        name = "Bellows (Up)",
        data = {
            orientation = "Floor",
            wind_strength = 800.0,
            wind_duration = 1.0,
            playerCanUse = true
        }
    },
    {
        name = "Bellows (Right)",
        data = {
            orientation = "WallLeft",
            wind_strength = 800.0,
            wind_duration = 1.0,
            playerCanUse = true
        }
    },
    {
        name = "Bellows (Left)",
        data = {
            orientation = "WallRight",
            wind_strength = 800.0,
            wind_duration = 1.0,
            playerCanUse = true
        }
    }
}

function bellows.justification(room, entity)
    if entity.orientation == "Floor" then
        return {0.5, 1.0}
    else
        return {0.5, 1.0}
    end
end

function bellows.rotation(room, entity)
    if entity.orientation == "Floor" then
        return 0.0
    elseif entity.orientation == "WallLeft" then
        return math.pi / 2
    else
        return -math.pi / 2
    end
end

return bellows