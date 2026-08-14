namespace Celeste.Mod.WindHelper.Extras;

public static class Utils
{
    [UsedImplicitly]
    internal static float Mod(float x, float m) => (x % m + m) % m;

    [UsedImplicitly]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    internal static bool nearlyEqual(this float a, float b, float epsilon = 128f * float.Epsilon, float abs_th = float.MinValue)
    {
        if (a == b) return true;

        float diff = Math.Abs(a - b);
        float norm = Math.Min(Math.Abs(a + b), float.MaxValue);
        return diff < Math.Max(abs_th, epsilon * norm);
    }

    [UsedImplicitly]
    internal static Vector2 ClampMagnitude(this Vector2 v, float max)
    {
        if (v.Length() == 0) return Vector2.Zero;
        return v.SafeNormalize() * Math.Min(v.Length(), max);
    }

    [UsedImplicitly]
    internal static int GameplayBufferWidth => GameplayBuffers.Gameplay?.Width ?? 320;

    [UsedImplicitly]
    internal static int GameplayBufferHeight => GameplayBuffers.Gameplay?.Height ?? 180;

    [UsedImplicitly]
    internal static Vector2 CorrectDashPrecision(Vector2 dir)
    {
        if (dir.X != 0.0 && Math.Abs(dir.X) < 1.0 / 1000.0)
        {
            dir.X = 0.0f;
            dir.Y = Math.Sign(dir.Y);
        }
        else if (dir.Y != 0.0 && Math.Abs(dir.Y) < 1.0 / 1000.0)
        {
            dir.Y = 0.0f;
            dir.X = Math.Sign(dir.X);
        }
        return dir;
    }

    [UsedImplicitly]
    internal static Vector2 GetAimVectorReal()
    {
        if (Input.MoveX == 0 && Input.MoveY == 0)
        {
            return Vector2.Zero;
        }
        return Input.GetAimVector();
    }

    [UsedImplicitly]
    internal static bool ExtendedWindControllerExists(Scene scene, out ExtendedWindController WindController)
    {
        WindController = scene.Tracker.GetEntity<ExtendedWindController>();
        return WindController is not null;
    }

    [UsedImplicitly]
    internal static void AddExtendedWindControllerIfNone(Scene scene, out ExtendedWindController WindController)
    {
        if (!LoadExtendedWindController.WindHelperIsNeeded)
        {
            WindController = null;
            return;
        }

        if (ExtendedWindControllerExists(scene, out ExtendedWindController windController))
        {
            WindController = windController;
            return;
        }

        WindController = new ExtendedWindController();
        scene.Add(WindController);
    }

    [UsedImplicitly]
    internal static bool CrystallineWindControllerExists(Scene scene) => scene.Tracker.GetEntity<CustomWindController>() is not null;

    [UsedImplicitly]
    internal static void LogError(string message)
    {
        Logger.Log(LogLevel.Error, "Wind Helper", message);
    }

    [UsedImplicitly]
    internal static void LogInfo(string message)
    {
        Logger.Log(LogLevel.Info, "Wind Helper", message);
    }
}