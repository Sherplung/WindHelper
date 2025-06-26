local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")

local floatingBlock = {}

floatingBlock.name = "WindHelper/FloatingBlock"
floatingBlock.depth = -9000
function floatingBlock.placements()
    return {
        name = "Windblown Block",
        data = {
            tiletype = fakeTilesHelper.getPlacementMaterial("m"),
            width = 8,
            height = 8,
            mass = 1.0
        }
    }
end

floatingBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

-- Filter by floating blocks sharing the same tiletype
local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.tiletype == target.tiletype
    end
end

function floatingBlock.sprite(room, entity)
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

return floatingBlock