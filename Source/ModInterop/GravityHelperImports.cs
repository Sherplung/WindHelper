namespace Celeste.Mod.WindHelper.ModInterop;

[GenerateImports("GravityHelper")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class GravityHelperImports
{
    public static partial bool IsPlayerInverted();

    public static partial bool IsActorInverted(Actor actor);

    public static partial void InvertedSuperBounce(Player player, float fromY);

    internal static float InvertIfPlayerInverted(float f) => IsImported && IsPlayerInverted() ? -f : f;
    internal static float InvertIfActorInverted(Actor actor, float f) => IsImported && IsActorInverted(actor) ? -f : f;
}