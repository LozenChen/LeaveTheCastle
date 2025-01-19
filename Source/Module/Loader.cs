using Celeste.Mod.LeaveTheCastle.Utils;

namespace Celeste.Mod.LeaveTheCastle.Module;

internal static class Loader {
    public static void Load() {
        Gameplay.Core.Load();
    }

    public static void Unload() {
        Gameplay.Core.Unload();
        HookHelper.Unload();
    }

    public static void Initialize() {
        Gameplay.Core.Initialize();
        LeaveTheCastleModule.Instance.SaveSettings();
    }
}