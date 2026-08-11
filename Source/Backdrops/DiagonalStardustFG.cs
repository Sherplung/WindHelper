namespace Celeste.Mod.WindHelper.Backdrops;

[CustomBackdrop("WindHelper/DiagonalStardustFG")]
[UsedImplicitly]
public class DiagonalStardustFG : Backdrop
{
    private struct Particle
    {
        public Vector2 Position;
        public float Percent;
        public float Duration;
        public Vector2 Direction;
        public Vector2 Angle;
        public float Speed;
        public float Spin;
        public int Color;
    }

    private readonly Particle[] particles;
    private static Color[] colors;

    private Vector2 feltWind;
    private Vector2 scale = Vector2.One;
    private float fade;

    public DiagonalStardustFG(BinaryPacker.Element data)
    {
        particles = new Particle[data.AttrInt("density", 50)];
        string[] colorStrings = data.Attr("colors", "4cccef,f243bd,42f1dd").Split(",");
        colors = new Color[colorStrings.Length];
        for (int j = 0; j < colorStrings.Length; j++)
        {
            colors[j] = Calc.HexToColor(colorStrings[j].Trim(','));
        }
        for (int i = 0; i < particles.Length; i++)
        {
            Reset(i, Calc.Random.NextFloat());
        }
    }

    private void Reset(int i, float p)
    {
        particles[i].Percent = p;
        particles[i].Position = new Vector2(Calc.Random.Range(0, Utils.GameplayBufferWidth), Calc.Random.Range(0, Utils.GameplayBufferHeight));
        particles[i].Speed = Calc.Random.Range(4, 14);
        particles[i].Spin = Calc.Random.Range(0.25f, MathF.PI * 6f);
        particles[i].Duration = Calc.Random.Range(1f, 4f);
        particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat(MathF.PI * 2f), 1f);
        particles[i].Color = Calc.Random.Next(colors.Length);
        particles[i].Angle = Vector2.UnitX;
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        if (scene is not Level level) return;
        Utils.AddExtendedWindControllerIfNone(scene, out ExtendedWindController windController);

        feltWind = level.Wind;
        if (WindHelperModule.CrystallineHelperLoaded)
        {
            feltWind = Utils.CrystallineWindControllerExists(scene) ? level.Wind + windController.GetAdditiveWind() : level.Wind;
        }

        scale.X = Math.Max(1f, Math.Abs(feltWind.Length()) / 100f);
        scale.Y = 1f;
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].Percent >= 1f)
            {
                Reset(i, 0f);
            }
            particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
            particles[i].Position += (particles[i].Direction * particles[i].Speed + feltWind) * Engine.DeltaTime;
            particles[i].Direction.Rotate(particles[i].Spin * Engine.DeltaTime);
            particles[i].Angle = feltWind.SafeNormalize(Vector2.UnitX);
        }
        fade = Calc.Approach(fade, Visible ? 1f : 0f, Engine.DeltaTime);
    }

    public override void Render(Scene scene)
    {
        if (scene is not Level level || fade <= 0f) return;

        Camera camera = level.Camera;
        for (int i = 0; i < particles.Length; i++)
        {
            Vector2 position = new Vector2
            {
                X = Utils.Mod(particles[i].Position.X - camera.X, Utils.GameplayBufferWidth),
                Y = Utils.Mod(particles[i].Position.Y - camera.Y, Utils.GameplayBufferHeight)
            };
            float percent = particles[i].Percent;
            float num = !(percent < 0.7f) ? Calc.ClampedMap(percent, 0.7f, 1f, 1f, 0f) : Calc.ClampedMap(percent, 0f, 0.3f);
            num *= FadeAlphaMultiplier;
            //Draw.Rect(position, scale.X, scale.Y, colors[particles[i].Color] * (fade * num));
            Draw.LineAngle(position, (-particles[i].Angle).Angle(), scale.X, colors[particles[i].Color] * (fade * num));
        }
    }
}