local utils = require("utils")

local floatingSpinner = {}

floatingSpinner.name = "WindHelper/FloatingSpinner"
floatingSpinner.texture = "Sherplung/WindHelper/FloatingSpinner/fg_FloatingSpinner00"
floatingSpinner.depth = 0
floatingSpinner.placements = {
  {
    name = "default",
    data = {
      mass = 1.0
    }
  }
}

function floatingSpinner.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return floatingSpinner