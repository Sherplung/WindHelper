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
	}
}
diagonalStardust.defaultData = {
    colors = "4cccef,f243bd,42f1dd",
    density = 50
}

return diagonalStardust