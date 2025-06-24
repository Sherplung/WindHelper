using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.WindHelper.Entities;

[Tracked(false)]
[CustomEntity("WindHelper/WindDustEdges")]
public class WindDustEdges : Entity
{
    public static int DustGraphicEstabledCounter;

    private bool hasDust;

    private float noiseEase;

    private Vector2 noiseFromPos;

    private Vector2 noiseToPos;

    private VirtualTexture DustNoiseFrom;

    private VirtualTexture DustNoiseTo;

    private Vector3[] customEdgeColor = 
    {
        Calc.HexToColor("00ffff").ToVector3(),
        Calc.HexToColor("11ff7f").ToVector3(),
        Calc.HexToColor("107dff").ToVector3()
    };

    
    public WindDustEdges()
    {
        AddTag((int)Tags.Global | (int)Tags.TransitionUpdate);
        base.Depth = -48;
        Add(new BeforeRenderHook(BeforeRender));
    }

    
    private void CreateTextures()
    {
        DustNoiseFrom = VirtualContent.CreateTexture("dust-noise-a", 128, 72, Color.White);
        DustNoiseTo = VirtualContent.CreateTexture("dust-noise-b", 128, 72, Color.White);
        Color[] array = new Color[DustNoiseFrom.Width * DustNoiseTo.Height];
        for (int i = 0; i < array.Length; i++)
        {
            float r1 = Calc.Random.NextFloat();
            array[i] = new Color(0f, r1, r1, 0f);
        }
        DustNoiseFrom.Texture_Safe.SetData(array);
        for (int j = 0; j < array.Length; j++)
        {
            float r2 = Calc.Random.NextFloat();
            array[j] = new Color(0f, r2, r2, 0f);
        }
        DustNoiseTo.Texture_Safe.SetData(array);
    }

    
    public override void Update()
    {
        noiseEase = Calc.Approach(noiseEase, 1f, Engine.DeltaTime);
        if (noiseEase == 1f)
        {
            VirtualTexture dustNoiseFrom = DustNoiseFrom;
            DustNoiseFrom = DustNoiseTo;
            DustNoiseTo = dustNoiseFrom;
            noiseFromPos = noiseToPos;
            noiseToPos = new Vector2(Calc.Random.NextFloat(), Calc.Random.NextFloat());
            noiseEase = 0f;
        }
        DustGraphicEstabledCounter = 0;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Dispose();
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Dispose();
    }

    public override void HandleGraphicsReset()
    {
        base.HandleGraphicsReset();
        Dispose();
    }

    
    private void Dispose()
    {
        if (DustNoiseFrom != null)
        {
            DustNoiseFrom.Dispose();
        }
        if (DustNoiseTo != null)
        {
            DustNoiseTo.Dispose();
        }
    }

    
    public void BeforeRender()
    {
        List<Component> components = base.Scene.Tracker.GetComponents<WindDustEdge>();
        hasDust = components.Count > 0;
        if (!hasDust)
        {
            return;
        }
        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, (base.Scene as Level).Camera.Matrix);
        foreach (Component item in components)
        {
            WindDustEdge dustEdge = item as WindDustEdge;
            if (dustEdge.Visible && dustEdge.Entity.Visible)
            {
                dustEdge.RenderDust();
            }
        }
        Draw.SpriteBatch.End();
        if (DustNoiseFrom == null || DustNoiseFrom.IsDisposed)
        {
            CreateTextures();
        }
        Vector2 vector = FlooredCamera();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.ResortDust);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Engine.Graphics.GraphicsDevice.Textures[1] = DustNoiseFrom.Texture_Safe;
        Engine.Graphics.GraphicsDevice.Textures[2] = DustNoiseTo.Texture_Safe;
        GFX.FxDust.Parameters["colors"].SetValue(customEdgeColor);
        GFX.FxDust.Parameters["noiseEase"].SetValue(noiseEase);
        GFX.FxDust.Parameters["noiseFromPos"].SetValue(noiseFromPos + new Vector2(vector.X / 320f, vector.Y / 180f));
        GFX.FxDust.Parameters["noiseToPos"].SetValue(noiseToPos + new Vector2(vector.X / 320f, vector.Y / 180f));
        GFX.FxDust.Parameters["pixel"].SetValue(new Vector2(0.003125f, 1f / 180f));
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GFX.FxDust, Matrix.Identity);
        Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.TempA, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
    }

    
    public override void Render()
    {
        if (hasDust)
        {
            Vector2 position = FlooredCamera();
            Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.ResortDust, position, Color.White);
        }
    }

    
    private Vector2 FlooredCamera()
    {
        Vector2 position = (base.Scene as Level).Camera.Position;
        position.X = (int)Math.Floor(position.X);
        position.Y = (int)Math.Floor(position.Y);
        return position;
    }
}
