local diagonalWindSnow = {}

diagonalWindSnow.name = "WindHelper/DiagonalWindSnowFG"
diagonalWindSnow.type = "effect"
diagonalWindSnow.fieldInformation = {
    color = {
        fieldType = "color",
        default = "ffffff",
        useAlpha = false,
        allowEmpty = false
    },
    density = {
        fieldType = "integer",
        default = 240,
        minimumValue = 1
    },
    thinningFactor = {
        fieldType = "number",
        default = 0.0,
        minimumValue = 0.0
    },
	scroll = {
		fieldType = "number",
		default = 1.0
	},
	alpha = {
	    fieldType = "number",
		default = 1.0,
		minimumValue = 0.0,
		maximumValue = 1.0
	},
	windXMultiplier = {
		fieldType = "number",
		default = 1.0
	},
	windYMultiplier = {
		fieldType = "number",
		default = 1.0
	}
}

diagonalWindSnow.defaultData = {
    color = "ffffff",
    density = 240,
    thinningFactor = 0.0,
	scroll = 1.0,
	alpha = 1.0,
	windXMultiplier = 1.0,
	windYMultiplier = 1.0
}

return diagonalWindSnow