local addWindComponents = {}

local behaviorTypes = {
    WhileInside = "WhileInside",
    AddPerma = "AddPerma",
    AddDuration = "AddDuration"
}

addWindComponents.name = "WindHelper/AddWindComponentsTrigger"
addWindComponents.fieldInformation = {
    behaviorType = {
        options = behaviorTypes,
        editable = false
    }
}

addWindComponents.placements = {
    name = "default",
    data = {
        behaviorType = "WhileInside",
        windX = 400.0,
        windY = 0.0,
        duration = 5.0,
        onlyOnce = false
    }
}

return addWindComponents
