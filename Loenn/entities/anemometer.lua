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

anemometer.texture = "Sherplung/WindHelper/Anemometer/Anemometer89"
anemometer.name = "WindHelper/Anemometer"
anemometer.fieldInformation = {
    wind_direction = {
        options = windDirections,
        editable = false
    }
}

anemometer.placements = {
    name = "Anemometer",
    data = {
        wind_direction = "DashDirection",
        wind_strength = 400.0,
        wind_duration = 1.0,
        uses = -1
    }
}

return anemometer
