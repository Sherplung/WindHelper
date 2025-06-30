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

controllableWind.placements = {
    name = "Controllable Wind",
    data = {
        behaviorType = "WhileInside",
        windStrength = 800.0,
        duration = 5.0,
        onlyOnce = false
    }
}

return controllableWind

