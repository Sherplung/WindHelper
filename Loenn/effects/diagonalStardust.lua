local diagonalStardust = {}

diagonalStardust.name = "WindHelper/DiagonalStardustFG"
diagonalStardust.type = "effect"
diagonalStardust.fieldInformation = {
	colors = {
		fieldType = "list",
		default = "4cccef,f243bd,42f1dd",
		elementDefault = "ffffff",
		minimumElements = 1,
		elementOptions = {
			fieldType = "color"
		}
	},
	density = {
		fieldType = "integer",
		default = 50,
		minimumValue = 1
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

diagonalStardust.defaultData = {
    colors = "4cccef,f243bd,42f1dd",
    density = 50,
	scroll = 1.0,
	alpha = 1.0,
	windXMultiplier = 1.0,
	windYMultiplier = 1.0
}

return diagonalStardust