local bellows = {}

local orientations = {
    Floor = "Floor",
    WallLeft = "WallLeft",
    WallRight = "WallRight",
	Ceiling = "Ceiling"
}

bellows.name = "WindHelper/Bellows"
bellows.depth = -8500
bellows.justification = {0.5, 1.0}
bellows.texture = "Sherplung/WindHelper/Bellows/BellowsLoenn"

bellows.fieldInformation = {
    orientation = {
        options = orientations,
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
    },
    {
        name = "down",
        data = {
            orientation = "Ceiling",
            wind_strength = 400.0,
            wind_duration = 1.0,
            playerCanUse = true
        }
    }
}

function bellows.rotation(room, entity)
    if entity.orientation == "Floor" then
        return 0.0
    elseif entity.orientation == "WallLeft" then
        return math.pi / 2
    elseif entity.orientation == "WallRight" then
        return -math.pi / 2
	elseif entity.orientation == "Ceiling" then
		return math.pi
    end
end

function bellows.rotate(room, entity, direction)
    if direction < 0 then
	    if entity.orientation == "Floor" then
			entity.orientation = "WallRight"
		elseif entity.orientation == "WallRight" then
			entity.orientation = "Ceiling"
		elseif entity.orientation == "Ceiling" then
			entity.orientation = "WallLeft"
		elseif entity.orientation == "WallLeft" then
			entity.orientation = "Floor"
		end
		return direction < 0
	elseif direction > 0 then
	    if entity.orientation == "Floor" then
			entity.orientation = "WallLeft"
		elseif entity.orientation == "WallLeft" then
			entity.orientation = "Ceiling"
		elseif entity.orientation == "Ceiling" then
			entity.orientation = "WallRight"
		elseif entity.orientation == "WallRight" then
			entity.orientation = "Floor"
		end
		return direction > 0
	end
end

function bellows.flip(room, entity, horizontal, vertical)
	if horizontal then
		if entity.orientation == "Floor" then
			entity.orientation = "Ceiling"
		elseif entity.orientation == "Ceiling" then
			entity.orientation = "Floor"
		end
		return horizontal
	elseif vertical then
		if entity.orientation == "WallLeft" then
			entity.orientation = "WallRight"
		elseif entity.orientation == "WallRight" then
			entity.orientation = "WallLeft"
		end
		return vertical
	end
end

return bellows