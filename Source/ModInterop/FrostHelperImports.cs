namespace Celeste.Mod.WindHelper.ModInterop;

[GenerateImports("FrostHelper")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class FrostHelperImports
{
    public static partial bool IsCeilingSpring(Spring spring);
    public static partial Vector2 GetSpringSpeedMultiplier(Spring spring);

    internal static bool SafeIsCeilingSpring(Spring spring) => IsImported && IsCeilingSpring(spring);
}