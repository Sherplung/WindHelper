local utils = require("utils")

local pinwheel = {}

local behaviorTypes = {
    AnyAngle = "AnyAngle",
    Cardinals = "Cardinals"
}

pinwheel.name = "WindHelper/Pinwheel"
pinwheel.depth = 0

function pinwheel.texture(room, entity)
    if entity.uses == 0 then
        return "Sherplung/WindHelper/Pinwheel/Gray/PinwheelGray1"
    elseif entity.behaviorType == behaviorTypes["AnyAngle"] then
        return "Sherplung/WindHelper/Pinwheel/Blue/PinwheelBlue1"
    elseif entity.behaviorType == behaviorTypes["Cardinals"] then
        return "Sherplung/WindHelper/Pinwheel/RedWhite/PinwheelRedWhite1"
    else 
        return "Sherplung/WindHelper/Pinwheel/Blue/PinwheelBlue1"
    end
end


pinwheel.fieldInformation = {
    behaviorType = {
        options = behaviorTypes,
        editable = false
    }
}

pinwheel.fieldOrder = {
    "x",
    "y",
    "wind_strength",
    "wind_duration",
    "behaviorType",
    "uses"
}

pinwheel.placements = {
  {
    name = "weak",
    data = {
      wind_strength = 400.0,
      wind_duration = 1.0,
      uses = -1,
      behaviorType = "AnyAngle"
    }
  },
  {
    name = "cardinal",
    data = {
      wind_strength = 400.0,
      wind_duration = 1.0,
      uses = -1,
      behaviorType = "Cardinals"
    }
  }
}

function pinwheel.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return pinwheel