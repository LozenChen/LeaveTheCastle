namespace Celeste.Mod.LeaveTheCastle.Module;

public class LeaveTheCastleModule : EverestModule {

    public static LeaveTheCastleModule Instance;

    public static LeaveTheCastleModSettings Settings => LeaveTheCastleModSettings.Instance;
    public LeaveTheCastleModule() {
        Instance = this;
    }

    public override Type SettingsType => typeof(LeaveTheCastleModSettings);
    public override void Load() {
        Loader.Load();
    }

    public override void Unload() {
        Loader.Unload();
    }

    public override void Initialize() {
        Loader.Initialize();
    }

    public override void LoadContent(bool firstLoad) {
        // do nothing
    }

    public override void LoadSettings() {
        base.LoadSettings();
    }

    public override void OnInputInitialize() {
        base.OnInputInitialize();
    }
}