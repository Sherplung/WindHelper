using Celeste;
using Celeste.Mod.Backdrops;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.WindHelper.Stylegrounds;
[CustomBackdrop("WindHelper/DiagonalStardustFG")]

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

    private static Color[] colors;

    private Particle[] particles;

    private float fade;

    private Vector2 scale = Vector2.One;

    public DiagonalStardustFG(BinaryPacker.Element data)
    {
        particles = new Particle[data.AttrInt("density", defaultValue: 50)];
        String[] colorStrings = data.Attr("colors", defaultValue: "4cccef,f243bd,42f1dd").Split(",");
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
        particles[i].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
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
        Level level = scene as Level;
        scale.X = Math.Max(1f, Math.Abs(level.Wind.Length()) / 100f);
        scale.Y = 1f;
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].Percent >= 1f)
            {
                Reset(i, 0f);
            }
            particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
            particles[i].Position += (particles[i].Direction * particles[i].Speed + level.Wind) * Engine.DeltaTime;
            particles[i].Direction.Rotate(particles[i].Spin * Engine.DeltaTime);
            particles[i].Angle = level.Wind.SafeNormalize(ifZero : Vector2.UnitX);
        }
        fade = Calc.Approach(fade, Visible ? 1f : 0f, Engine.DeltaTime);
    }

    public override void Render(Scene level)
    {
        if (!(fade <= 0f))
        {
            Camera camera = (level as Level).Camera;
            for (int i = 0; i < particles.Length; i++)
            {
                Vector2 position = new Vector2
                {
                    X = mod(particles[i].Position.X - camera.X, 320f),
                    Y = mod(particles[i].Position.Y - camera.Y, 180f)
                };
                float percent = particles[i].Percent;
                float num = 0f;
                num = ((!(percent < 0.7f)) ? Calc.ClampedMap(percent, 0.7f, 1f, 1f, 0f) : Calc.ClampedMap(percent, 0f, 0.3f));
                num *= FadeAlphaMultiplier;
                //Draw.Rect(position, scale.X, scale.Y, colors[particles[i].Color] * (fade * num));
                Draw.LineAngle(position, (-particles[i].Angle).Angle(), scale.X, colors[particles[i].Color] * (fade * num));
            }
        }
    }

    private float mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
