using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Backdrops;



namespace Celeste.Mod.WindHelper.Stylegrounds;
[CustomBackdrop("WindHelper/DiagonalWindSnowFG")]
internal class DiagonalWindSnowFG : Backdrop
{
    public Vector2 CameraOffset = Vector2.Zero;

    public float Alpha = 1f;

    private Vector2[] positions;

    private SineWave[] sines;

    private Vector2 scale = Vector2.One;

    private float rotation;

    private float loopWidth = 640f;

    private float loopHeight = 360f;

    private float visibleFade = 1f;

    private float thinningFactor;

    public DiagonalWindSnowFG(BinaryPacker.Element data)
    {
        Color = Calc.HexToColor(data.Attr("color", defaultValue: "ffffff"));
        positions = new Vector2[data.AttrInt("density", defaultValue: 240)];
        thinningFactor = data.AttrFloat("thinningFactor", defaultValue: 0f);
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = Calc.Random.Range(new Vector2(0f, 0f), new Vector2(loopWidth, loopHeight));
        }
        sines = new SineWave[16];
        for (int j = 0; j < sines.Length; j++)
        {
            sines[j] = new SineWave(Calc.Random.Range(0.8f, 1.2f), 0f);
            sines[j].Randomize();
        }
    }
    public override void Update(Scene scene)
    {
        base.Update(scene);
        visibleFade = Calc.Approach(visibleFade, IsVisible(scene as Level) ? 1 : 0, Engine.DeltaTime * 2f);
        Level level = scene as Level;
        SineWave[] array = sines;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].Update();
        }
        if (level.Wind != Vector2.Zero)
        {
            float magnitude = level.Wind.Length();
            rotation = level.Wind.Angle();
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
            Vector2 zero = Vector2.Zero;
            if (level.Wind != Vector2.Zero)
            {
                zero = (new Vector2(level.Wind.X + value * 10f, level.Wind.Y + value * 10f));
            }
            else
            {
                zero = (new Vector2(level.Wind.X + value * 10f, level.Wind.Y + (3f + value) * 10f));
            }
            positions[j] += zero * Engine.DeltaTime;
        }
    }
    public override void Render(Scene scene)
    {
        if (Alpha <= 0f)
        {
            return;
        }
        Color color = Color * visibleFade * Alpha;
        int num = (int)((float)positions.Length * 0.6f);
        int num2 = 0;
        Vector2[] array = positions;
        for (int i = 0; i < array.Length; i++)
        {
            Vector2 position = array[i];
            position.Y -= (scene as Level).Camera.Y + CameraOffset.Y;
            position.Y %= loopHeight;
            if (position.Y < 0f)
            {
                position.Y += loopHeight;
            }
            position.X -= (scene as Level).Camera.X + CameraOffset.X;
            position.X %= loopWidth;
            if (position.X < 0f)
            {
                position.X += loopWidth;
            }
            if (num2 < num)
            {
                GFX.Game["particles/snow"].DrawCentered(position, color, scale, rotation);
            }
            num2++;
        }
    }
}
