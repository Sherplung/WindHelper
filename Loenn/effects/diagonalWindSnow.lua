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
    }
}
diagonalWindSnow.defaultData = {
    color = "ffffff",
    density = 240,
    thinningFactor = 0.0
}

return diagonalWindSnow