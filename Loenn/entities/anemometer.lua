local anemometer = {}

local windDirections = {
    Up = "Up",
    UpRight = "UpRight",
    Right = "Right",
    DownRight = "DownRight",
    Down = "Down",
    DownLeft = "DownLeft",
    Left = "Left",
    UpLeft = "UpLeft",
    DashDirection = "DashDirection"
}

anemometer.texture = "Sherplung/WindHelper/Anemometer/AnemometerLoenn"
anemometer.name = "WindHelper/Anemometer"
anemometer.justification = {0.5, 0.96875}
anemometer.fieldInformation = {
    wind_direction = {
        options = windDirections,
        editable = false
    }
}

anemometer.placements = {
    name = "default",
    data = {
        wind_direction = "DashDirection",
        wind_strength = 800.0,
        wind_duration = 1.0,
        uses = -1
    }
}

return anemometer
