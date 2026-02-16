local windBooster = {}

windBooster.name = "WindHelper/WindBooster"
windBooster.depth = -8500
windBooster.placements = {
    {
        name = "green",
        data = {
            red = false,
            windStrength = 1200.0,
            ch9_hub_booster = false
        }
    },
    {
        name = "red",
        data = {
            red = true,
            windStrength = 1200.0,
            ch9_hub_booster = false
        }
    }
}

function windBooster.texture(room, entity)
    local red = entity.red

    if red then
        return "objects/booster/boosterRed00"

    else
        return "objects/booster/booster00"
    end
end

return windBooster