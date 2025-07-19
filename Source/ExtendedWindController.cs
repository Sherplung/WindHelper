using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Celeste;
using IL.Celeste.Mod.Registry.DecalRegistryHandlers;
using Monocle;
using MonoMod;
using System.Collections;
using Celeste.Mod.WindHelper;
using IL.MonoMod;

namespace Celeste.Mod.WindHelper.Entities;

public class ExtendedWindController : WindController
{

    private Vector2 additiveWind;

    private Vector2 incrementerAdditive;

    private Vector2 incrementerPattern;

    private int controllableWindCount;

    private float controllableWindStrength;

    private Vector2 controllableWind;

    private Vector2 additivePermaWind;

    private Vector2 customPatternWind;

    private Vector2 heldDirection;

    private Vector2 totalAddedWind;

    private Coroutine windCoroutine;

    private Coroutine customPatternCoroutine;

    private bool fastEasing;

    public ExtendedWindController(Patterns pattern)
        : base(pattern)
    {
        additiveWind = Vector2.Zero;
        controllableWindCount = 0;
        controllableWindStrength = 0;
        additivePermaWind = Vector2.Zero;
    }

    private void AdditiveSetAmbienceStrength(bool strong)
    {
        int num = 0;
        if ((targetSpeed + totalAddedWind).X != 0f)
        {
            num = Math.Sign((targetSpeed + totalAddedWind).X);
        }
        else if ((targetSpeed + totalAddedWind).Y != 0f)
        {
            num = Math.Sign((targetSpeed + totalAddedWind).Y);
        }
        Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", num);
        Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "strong_wind", strong ? 1 : 0);
    }

    private IEnumerator TimedWind(Vector2 wind, float duration)
    {
        fastEasing = true;
        additiveWind += wind;
        yield return duration;
        fastEasing = false;
        additiveWind -= wind;
        if (additiveWind.LengthSquared() < 1)
        {
            additiveWind = Vector2.Zero;
        }
    }

    public void AddPermaWind(Vector2 wind)
    {
        additivePermaWind += wind;
        if (additivePermaWind.LengthSquared() < 1)
        {
            additivePermaWind = Vector2.Zero;
        }
    }

    public void AddWind(Vector2 wind, float duration)
    {
        Add(windCoroutine = new Coroutine(TimedWind(wind, duration)));
    }

    public void ChangeControllableWind(float strength, bool add = true)
    {
        if (add)
        {
            controllableWindCount++;
            controllableWindStrength += strength;
        }
        else
        {
            controllableWindCount--;
            controllableWindStrength -= strength;
        }
    }

    public void AddCustomWindPattern(string code)
    {
        string[] commands = code.Split(':');
        float[,] values = new float[commands.Length, 3];
        for (int i = 0; i < commands.Length; i++)
        {
            string[] indivCmd = commands[i].Split(",");
            values[i,0] = float.Parse(indivCmd[0].Trim(',').Trim(':'));
            values[i,1] = float.Parse(indivCmd[1].Trim(',').Trim(':'));
            values[i,2] = float.Parse(indivCmd[2].Trim(',').Trim(':'));
        }
        if (customPatternCoroutine != null)
        {
            Remove(customPatternCoroutine);
            customPatternCoroutine = null;
        }
        Add(customPatternCoroutine = new Coroutine(CustomWindPattern(values)));
    }

    private IEnumerator CustomWindPattern(float[,] values)
    {
        while (true)
        {
            for (int i = 0; i < values.GetLength(0); i++)
            {
                customPatternWind.X = values[i, 0];
                customPatternWind.Y = values[i, 1];
                yield return values[i, 2];
            }
        }
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    public void base_Update()
    {
    }

    public override void Update()
    {
        base_Update();
        if (pattern == Patterns.LeftGemsOnly)
        {
            bool flag = false;
            foreach (StrawberrySeed entity in base.Scene.Tracker.GetEntities<StrawberrySeed>())
            {
                if (entity.Collected)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                targetSpeed.X = -400f;
                SetAmbienceStrength(strong: false);
            }
            else
            {
                targetSpeed.X = 0f;
                SetAmbienceStrength(strong: false);
            }
        }
        // handling controllable wind
        heldDirection.X = Input.MoveX;
        heldDirection.Y = Input.MoveY;
        heldDirection = heldDirection.SafeNormalize(ifZero : Vector2.Zero);
        if (controllableWindCount > 0)
        {
            controllableWind = heldDirection * controllableWindStrength;
        }
        else
        {
            controllableWind = Vector2.Zero;
            controllableWindStrength = 0f;
            controllableWindCount = 0;
        }
        // additive wind easing type selector
        totalAddedWind = controllableWind + additiveWind + additivePermaWind + customPatternWind;
        switch (WindHelperModule.Settings.AdditiveWindEasing)
        {
            case WindHelperModuleSettings.EasingTypes.EaseSlowToZero:
                if (totalAddedWind != Vector2.Zero || fastEasing) 
                { 
                    incrementerAdditive = totalAddedWind; 
                }
                else { incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 2000f * Engine.DeltaTime); }
                break;
            case WindHelperModuleSettings.EasingTypes.NoEasing:
                incrementerAdditive = totalAddedWind;
                break;
            case WindHelperModuleSettings.EasingTypes.EaseFastAlways:
                incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 10000f * Engine.DeltaTime);
                break;
            case WindHelperModuleSettings.EasingTypes.EaseFastUpEaseSlowDown:
                if (incrementerAdditive.LengthSquared() <= totalAddedWind.LengthSquared()) 
                { 
                    incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 10000f * Engine.DeltaTime); 
                }
                else { incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 2000f * Engine.DeltaTime); }
                break;
            case WindHelperModuleSettings.EasingTypes.EaseFastStartEaseSlowEnd:
                if (fastEasing)
                {
                    incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 10000f * Engine.DeltaTime);
                }
                else { incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 2000f * Engine.DeltaTime); }
                break;
        }
        incrementerPattern = Calc.Approach(incrementerPattern, targetSpeed, 1000f * Engine.DeltaTime);

        // handle ambience
        if (WindHelperModule.Settings.AdditiveWindAmbience == true)
        {
            // if the total target wind has a magnitude greater than or equal to 800 (LengthSquared is faster apparently)
            if ((totalAddedWind + targetSpeed).LengthSquared() >= (640000f - 1000f))
            {
                AdditiveSetAmbienceStrength(strong: true);
            }
            else if ((totalAddedWind + targetSpeed).LengthSquared() >= 0f)
            {
                AdditiveSetAmbienceStrength(strong: false);
            }
        }

        // actually move stuff
        level.Wind = incrementerPattern + incrementerAdditive;
        if (level.Wind.Equals(Vector2.Zero) || level.Transitioning)
        {
            return;
        }
        foreach (WindMover component in base.Scene.Tracker.GetComponents<WindMover>())
        {
            component.Move(level.Wind * 0.1f * Engine.DeltaTime);
        }
    }
}
