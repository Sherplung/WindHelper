local customizeWindPropertiesTrigger = {}

local easingTypes = {
    EaseSlowToZero = "EaseSlowToZero",
    NoEasing = "NoEasing",
    EaseFastAlways = "EaseFastAlways",
    EaseFastUpEaseSlowDown = "EaseFastUpEaseSlowDown",
    EaseFastStartEaseSlowEnd = "EaseFastStartEaseSlowEnd"
}

customizeWindPropertiesTrigger.name = "WindHelper/CustomizeWindPropertiesTrigger"
customizeWindPropertiesTrigger.fieldInformation = {
    easingType = {
        options = easingTypes,
        editable = false
    }
}

customizeWindPropertiesTrigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
	"maxWindSpeed",
    "easingType",
    "additiveWindAmbience",
	"oneUse"
}

customizeWindPropertiesTrigger.placements = {
    name = "default",
    data = {
		maxWindSpeed = "2000",
        easingType = "EaseSlowToZero",
        additiveWindAmbience = false,
		oneUse = false
    }
}

return customizeWindPropertiesTrigger