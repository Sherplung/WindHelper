local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")

local FloatingDashBlock = {}

FloatingDashBlock.name = "WindHelper/FloatingDashBlock"
FloatingDashBlock.depth = -9000

FloatingDashBlock.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "mass",
    "tiletype",
    "enableFlag",
    "disableFlag",
    "lockX",
    "lockY"
}

function FloatingDashBlock.placements()
    return {
        name = "floating_dash_block",
        data = {
            tiletype = fakeTilesHelper.getPlacementMaterial("m"),
            width = 8,
            height = 8,
            mass = 1.0,
            lockX = false,
            lockY = false,
            enableFlag = "",
            disableFlag = ""
        }
    }
end

FloatingDashBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

-- Filter by floating dash blocks sharing the same tiletype
local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.tiletype == target.tiletype
    end
end

function FloatingDashBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)
    local firstEntity = relevantBlocks[1] == entity

    if firstEntity then
        -- Can use simple render, nothing to merge together
        if #relevantBlocks == 1 then
            return fakeTilesHelper.getEntitySpriteFunction("tiletype", false)(room, entity)
        end

        return fakeTilesHelper.getCombinedEntitySpriteFunction(relevantBlocks, "tiletype")(room)
    end

    local entityInRoom = utils.contains(entity, relevantBlocks)

    -- Entity is from a placement preview
    if not entityInRoom then
        return fakeTilesHelper.getEntitySpriteFunction("tiletype", false)(room, entity)
    end
end

return FloatingDashBlock