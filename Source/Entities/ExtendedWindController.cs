namespace Celeste.Mod.WindHelper.Entities;

[Tracked]
[UsedImplicitly]
public class ExtendedWindController : WindController
{
    private Vector2 additiveWind;
    private Vector2 incrementerAdditive;

    private Vector2 controllableWind;
    private int controllableWindCount;
    private float controllableWindStrength;
    private Vector2 heldDirection;

    private Vector2 incrementerPattern;
    private Vector2 customPatternWind;
    private Coroutine customPatternCoroutine;

    private Vector2 additivePermaWind;
    private Vector2 totalAddedWind;

    private bool fastEasing;

    public ExtendedWindController(Patterns pattern) : base(pattern)
    {
        additiveWind = Vector2.Zero;
        incrementerAdditive = Vector2.Zero;
        incrementerPattern = Vector2.Zero;
        controllableWindCount = 0;
        controllableWindStrength = 0;
        additivePermaWind = Vector2.Zero;
        customPatternWind = Vector2.Zero;
        totalAddedWind = Vector2.Zero;

        TransitionListener listener;
        Add(listener = new TransitionListener());
        listener.OnOutBegin = () =>
        {
            Components.RemoveAll<Coroutine>();
            additiveWind = Vector2.Zero;
            incrementerAdditive = Vector2.Zero;
            incrementerPattern = Vector2.Zero;
            controllableWindCount = 0;
            controllableWindStrength = 0;
            additivePermaWind = Vector2.Zero;
            customPatternWind = Vector2.Zero;
            totalAddedWind = Vector2.Zero;
        };
    }

    public ExtendedWindController() : this(Patterns.None)
    {
    }

    public Vector2 GetAdditiveWind() => incrementerAdditive;

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
        Add(new Coroutine(TimedWind(wind, duration)));
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
            values[i, 0] = float.Parse(indivCmd[0].Trim(',').Trim(':'));
            values[i, 1] = float.Parse(indivCmd[1].Trim(',').Trim(':'));
            values[i, 2] = float.Parse(indivCmd[2].Trim(',').Trim(':'));
        }
        if (customPatternCoroutine != null)
        {
            Remove(customPatternCoroutine);
            customPatternCoroutine = null;
        }
        Add(customPatternCoroutine = new Coroutine(CustomWindPattern(values)));
    }

    [SuppressMessage("ReSharper", "IteratorNeverReturns")]
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
    private void base_Update()
    {
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    public override void Update()
    {
        base_Update();
        if (pattern == Patterns.LeftGemsOnly)
        {
            bool flag = Scene.Tracker.GetEntities<StrawberrySeed>().Cast<StrawberrySeed>().Any(entity => entity.Collected);
            targetSpeed.X = flag ? -400f : 0f;
            SetAmbienceStrength(false);
        }

        // handling controllable wind
        heldDirection = Utils.CorrectDashPrecision(Input.GetAimVector().SafeNormalize(Vector2.Zero));
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
        totalAddedWind = (controllableWind + additiveWind + additivePermaWind + customPatternWind).ClampMagnitude(WindHelperModule.Session.MaxWindSpeed);
        switch (WindHelperModule.Session.AdditiveWindEasing)
        {
            case WindHelperModuleSession.EasingTypes.EaseSlowToZero:
                if (totalAddedWind != Vector2.Zero || fastEasing)
                {
                    incrementerAdditive = totalAddedWind;
                }
                else
                {
                    incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 2000f * Engine.DeltaTime);
                }
                break;
            case WindHelperModuleSession.EasingTypes.NoEasing:
                incrementerAdditive = totalAddedWind;
                break;
            case WindHelperModuleSession.EasingTypes.EaseFastAlways:
                incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 10000f * Engine.DeltaTime);
                break;
            case WindHelperModuleSession.EasingTypes.EaseFastUpEaseSlowDown:
                if (incrementerAdditive.LengthSquared() <= totalAddedWind.LengthSquared())
                {
                    incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 10000f * Engine.DeltaTime);
                }
                else
                {
                    incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 2000f * Engine.DeltaTime);
                }
                break;
            case WindHelperModuleSession.EasingTypes.EaseFastStartEaseSlowEnd:
                if (fastEasing)
                {
                    incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 10000f * Engine.DeltaTime);
                }
                else
                {
                    incrementerAdditive = Calc.Approach(incrementerAdditive, totalAddedWind, 2000f * Engine.DeltaTime);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        incrementerPattern = Calc.Approach(incrementerPattern, targetSpeed, 1000f * Engine.DeltaTime);

        // handle ambience
        if (WindHelperModule.Session.AdditiveWindAmbience)
        {
            // if the total target wind has a magnitude greater than or equal to 800 (LengthSquared is faster apparently)
            if ((totalAddedWind + targetSpeed).LengthSquared() >= 640000f - 1000f)
            {
                AdditiveSetAmbienceStrength(true);
            }
            else if ((totalAddedWind + targetSpeed).LengthSquared() >= 0f)
            {
                AdditiveSetAmbienceStrength(false);
            }
        }

        // actually move stuff
        level.Wind = incrementerPattern + incrementerAdditive;
        if (level.Wind.Equals(Vector2.Zero) || level.Transitioning)
        {
            return;
        }

        //crystalline helper overlap protection
        if (WindHelperModule.CrystallineHelperLoaded && Utils.CrystallineWindControllerExists(Scene))
        {
            foreach (WindMover component in Scene.Tracker.GetComponents<WindMover>().Cast<WindMover>())
            {
                component.Move(incrementerAdditive * 0.1f * Engine.DeltaTime);
            }
            return;
        }
        foreach (WindMover component in Scene.Tracker.GetComponents<WindMover>().Cast<WindMover>())
        {
            component.Move(level.Wind * 0.1f * Engine.DeltaTime);
        }
    }
}