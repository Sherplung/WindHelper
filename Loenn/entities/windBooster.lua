local windBooster = {}

windBooster.name = "WindHelper/WindBooster"
windBooster.depth = -8500
windBooster.placements = {
    {
        name = "green",
        data = {
            red = false,
            windStrength = 800.0,
            dashBased = true,
            windDuration = 1.0,
            ch9_hub_booster = false,
			oneUse = false
        }
    },
    {
        name = "red",
        data = {
            red = true,
            windStrength = 1200.0,
            dashBased = false,
            windDuration = 1.0,
            ch9_hub_booster = false,
			oneUse = false
        }
    }
}

function windBooster.texture(room, entity)
    local red = entity.red

    if red then
        return "Sherplung/WindHelper/WindBooster/WindBoosterRLoenn"

    else
        return "Sherplung/WindHelper/WindBooster/WindBoosterGLoenn"
    end
end

return windBooster