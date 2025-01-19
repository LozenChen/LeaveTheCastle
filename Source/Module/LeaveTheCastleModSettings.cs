namespace Celeste.Mod.LeaveTheCastle.Module;

public class LeaveTheCastleModSettings : EverestModuleSettings {

    public static LeaveTheCastleModSettings Instance { get; private set; }

    public LeaveTheCastleModSettings() {
        Instance = this;
    }

    /*
    private static void Initialize() {
        if (horizontalSpeed != CastleCore.startInWallSpeed.ToString() || intoWallDepth != CastleCore.startInWallDepth.ToString()) {
            throw new Exception("LeaveTheCastleModSettings Wrong Description");
        }
    }
    */

    private const string horizontalSpeed = "5";

    private const string intoWallDepth = "4";

    [SettingSubText("Movement Direction in Wall:\nDucking          -> Go Up\nFacing Left    -> Go Right\nFacing Right -> Go Left")]
    public bool Enabled { get; set; } = true;

    [SettingSubText($"Off: You need these conditions to go into a wall: \n 1). Be dashing, \n 2). Have at least {horizontalSpeed} px/f horizontal speed, \n 3). If there's no wall you can go at least {intoWallDepth} px furthur. \nOn: NO conditions.")]
    public bool CrazyMode { get; set; } = false;

    public bool NoLevelBoundsWhenInWall { get; set; } = false;

}