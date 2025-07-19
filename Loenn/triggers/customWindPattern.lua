local customWindPattern = {}

customWindPattern.name = "WindHelper/CustomWindPatternTrigger"
customWindPattern.fieldInformation = {
	instructions = {
		fieldType = "list",
		default = "0.0,0.0,0.0",
		elementDefault = "0.0,0.0,0.0",
		elementSeparator = ":",
		minimumElements = 1,
		elementOptions = {
			fieldType = "list",
			minimumElements = 3,
			maximumElements = 3,
			elementDefault = 0.0,
			elementSeparator = ",",
			elementOptions = {
				fieldType = "number"
			}
		}
	}
}

customWindPattern.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
	"instructions",
    "onlyOnce"
}

customWindPattern.placements = {
	name = "default",
	data = {
		instructions = "0.0,0.0,0.0",
		onlyOnce = true
	}
}

return customWindPattern