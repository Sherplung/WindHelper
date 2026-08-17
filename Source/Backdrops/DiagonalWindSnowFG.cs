namespace Celeste.Mod.WindHelper.Backdrops;

[CustomBackdrop("WindHelper/DiagonalWindSnowFG")]
[UsedImplicitly]
internal class DiagonalWindSnowFG : Backdrop
{
    private readonly Vector2[] positions;
    private Vector2 scale = Vector2.One;
    private float rotation;
    private readonly float thinningFactor;

    private readonly SineWave[] sines;
    private Vector2 feltWind;

    private float visibleFade = 1f;
    private readonly Vector2 CameraOffset = Vector2.Zero;

    public DiagonalWindSnowFG(BinaryPacker.Element data)
    {
        Color = Calc.HexToColor(data.Attr("color", "ffffff"));
        positions = new Vector2[data.AttrInt("density", 240) / 2]; // I'm like pretty sure 4 is the correct divisor here due to the math involved, but feel free to change this (sherplung: 2 looks closer to me in practice)
        thinningFactor = data.AttrFloat("thinningFactor");
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = Calc.Random.Range(new Vector2(0f, 0f), new Vector2(Utils.GameplayBufferWidth, Utils.GameplayBufferHeight));
        }
        sines = new SineWave[16];
        for (int j = 0; j < sines.Length; j++)
        {
            sines[j] = new SineWave(Calc.Random.Range(0.8f, 1.2f), 0f);
            sines[j].Randomize();
        }
    }

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        orig(self, playerIntro, isFromLoader);
        foreach (Backdrop backdrop in self.Background.Backdrops)
        {
            if (backdrop is not DiagonalWindSnowFG diagonalWindSnow) continue;
            diagonalWindSnow.rotation = 0f;
            diagonalWindSnow.scale = Vector2.One;
        }
        foreach (Backdrop backdrop in self.Foreground.Backdrops)
        {
            if (backdrop is not DiagonalWindSnowFG diagonalWindSnow) continue;
            diagonalWindSnow.rotation = 0f;
            diagonalWindSnow.scale = Vector2.One;
        }
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        if (scene is not Level level) return;
        Utils.AddExtendedWindControllerIfNone(scene, out ExtendedWindController windController);

        visibleFade = Calc.Approach(visibleFade, IsVisible(level) ? 1 : 0, Engine.DeltaTime * 2f);

        feltWind = level.Wind;
        if (WindHelperModule.CrystallineHelperLoaded)
        {
            feltWind = Utils.CrystallineWindControllerExists(scene) ? level.Wind + windController.GetAdditiveWind() : level.Wind;
        }

        foreach (SineWave sine in sines)
        {
            sine.Update();
        }

        if (feltWind != Vector2.Zero)
        {
            float magnitude = feltWind.Length();
            rotation = feltWind.Angle();
            //scale.X = Math.Max(1f, Math.Abs(level.Wind.X) / 100f);
            scale.X = Math.Max(1f, magnitude / 100f);
            //scale.Y = 1f / Math.Max(1f, Math.Abs(level.Wind.Y) / 100f);
            scale.Y = 1f / Math.Max(1f, (float)Math.Log(magnitude) * thinningFactor);
        }
        else
        {
            rotation = Calc.Approach(rotation, 0f, Engine.DeltaTime * 8f);
            scale = Calc.Approach(scale, Vector2.One, Engine.DeltaTime * 20f);
        }
        for (int j = 0; j < positions.Length; j++)
        {
            float value = sines[j % sines.Length].Value;
            Vector2 zero = feltWind != Vector2.Zero ? new Vector2(feltWind.X + value * 10f, feltWind.Y + value * 10f) : new Vector2(feltWind.X + value * 10f, feltWind.Y + (2f) * 10f);
            positions[j] += zero * Engine.DeltaTime;
        }
    }

    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public override void Render(Scene scene)
    {
        if (scene is not Level level) return;

        Color color = Color * visibleFade;
        int num = (int)(positions.Length * 0.6f);
        int num2 = 0;
        Vector2[] array = positions;
        for (int i = 0; i < array.Length; i++)
        {
            Vector2 position = array[i];
            position.Y -= level.Camera.Y + CameraOffset.Y;
            position.Y %= Utils.GameplayBufferHeight;
            if (position.Y < 0f)
            {
                position.Y += Utils.GameplayBufferHeight;
            }
            position.X -= level.Camera.X + CameraOffset.X;
            position.X %= Utils.GameplayBufferWidth;
            if (position.X < 0f)
            {
                position.X += Utils.GameplayBufferWidth;
            }
            if (num2 < num)
            {
                GFX.Game["particles/snow"].DrawCentered(position, color, scale, rotation);
            }
            num2++;
        }
    }
}