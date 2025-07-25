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

bellows.fieldOrder = {
    "x",
    "y",
    "wind_strength",
    "wind_duration",
    "orientation",
    "playerCanUse"
}

bellows.placements = {
    {
        name = "up",
        data = {
            orientation = "Floor",
            wind_strength = 400.0,
            wind_duration = 1.0,
            playerCanUse = true
        }
    },
    {
        name = "right",
        data = {
            orientation = "WallLeft",
            wind_strength = 400.0,
            wind_duration = 1.0,
            playerCanUse = true
        }
    },
    {
        name = "left",
        data = {
            orientation = "WallRight",
            wind_strength = 400.0,
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