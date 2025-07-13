local controllableWind = {}

local behaviorTypes = {
    WhileInside = "WhileInside",
    Add = "Add",
    Remove = "Remove",
    Duration = "Duration"
}

controllableWind.name = "WindHelper/ControllableWindTrigger"
controllableWind.fieldInformation = {
    behaviorType = {
        options = behaviorTypes,
        editable = false
    }
}

controllableWind.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "windStrength",
    "duration",
    "behaviorType",
    "onlyOnce"
}

controllableWind.placements = {
    name = "default",
    data = {
        behaviorType = "WhileInside",
        windStrength = 800.0,
        duration = 5.0,
        onlyOnce = false
    }
}

return controllableWind

