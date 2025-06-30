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

    private Vector2 heldDirection;

    private Vector2 totalAddedWind;

    private Coroutine windCoroutine;

    private bool fastEasing;

    public ExtendedWindController(Patterns pattern)
        : base(pattern)
    {
        additiveWind = Vector2.Zero;
        controllableWindCount = 0;
        controllableWindStrength = 0;
        additivePermaWind = Vector2.Zero;
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
        //handling controllable wind
        heldDirection.X = Input.MoveX;
        heldDirection.Y = Input.MoveY;
        if (heldDirection != Vector2.Zero) { heldDirection.SafeNormalize(); }
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
        //additive wind easing type selector
        totalAddedWind = controllableWind + additiveWind + additivePermaWind;
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
