//#define PatchMoveV
using Celeste.Mod.LeaveTheCastle.Module;
using Celeste.Mod.LeaveTheCastle.Utils;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.LeaveTheCastle.Gameplay;

public static class Core {

    // https://tieba.baidu.com/p/5111510137
    // though what we do it is a bit different
    public static bool Enabled => LeaveTheCastleModSettings.Instance.Enabled;

    public static bool CrazyMode => LeaveTheCastleModSettings.Instance.CrazyMode;

    public static bool EnableSkipLevelBounds => LeaveTheCastleModSettings.Instance.NoLevelBoundsWhenInWall;


    public static void Load() {
        On.Celeste.Player.TransitionTo += OnTransitionTo;
        On.Celeste.Player.DashBegin += OnDashBegin;
        On.Celeste.Player.DashEnd += OnDashEnd;
        IL.Celeste.Level.EnforceBounds += HookEnforceBounds;
    }

    public static void Unload() {
        On.Celeste.Player.TransitionTo -= OnTransitionTo;
        On.Celeste.Player.DashBegin -= OnDashBegin;
        On.Celeste.Player.DashEnd -= OnDashEnd;
        IL.Celeste.Level.EnforceBounds -= HookEnforceBounds;
    }


    public static void Initialize() {
        using (DetourContext detourContext = new() { After = new List<string>() { "*" } }) {
            typeof(Player).GetMethod("orig_Update").IlHook(HookOrigUpdate);
        }
    }

    private static void HookEnforceBounds(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_1, ins => ins.OpCode == OpCodes.Ldloca_S, ins => ins.OpCode == OpCodes.Call, ins => ins.OpCode == OpCodes.Conv_R4, ins => ins.OpCode == OpCodes.Callvirt, ins => ins.OpCode == OpCodes.Ldarg_1, ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.OnBoundsH)) || ins.MatchCallOrCallvirt<Player>(nameof(Player.OnBoundsV)))) {
            cursor.Index += 7;
            Instruction target = cursor.Next;
            cursor.Index -= 7;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(SkipLevelBounds);
            cursor.Emit(OpCodes.Brtrue, target);
            cursor.Index++;
        }
        cursor.Goto(0);
        while (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_1, ins => ins.OpCode == OpCodes.Ldloca_S, ins => ins.OpCode == OpCodes.Call, ins => ins.OpCode == OpCodes.Ldc_I4_S, ins => ins.OpCode == OpCodes.Sub, ins => ins.OpCode == OpCodes.Conv_R4, ins => ins.OpCode == OpCodes.Callvirt, ins => ins.OpCode == OpCodes.Ldarg_1, ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.OnBoundsH)) || ins.MatchCallOrCallvirt<Player>(nameof(Player.OnBoundsV)))) {
            cursor.Index += 9;
            Instruction target = cursor.Next;
            cursor.Index -= 9;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(SkipLevelBounds);
            cursor.Emit(OpCodes.Brtrue, target);
            cursor.Index++;
        }
    }

    private static bool SkipLevelBounds(Player player) {
        return EnableSkipLevelBounds && player.CollideCheck<Solid>();
    }

    private static bool OnTransitionTo(On.Celeste.Player.orig_TransitionTo orig, Player player, Vector2 target, Vector2 direction) {
        if (Enabled && player.CollideCheck<Solid>()) {
            player.ZeroRemainderX();
            player.ZeroRemainderY();
            player.Position = target;
            player.Speed.X = (int)Math.Round(player.Speed.X);
            player.Speed.Y = (int)Math.Round(player.Speed.Y);
            return true;
        }
        else {
            return orig(player, target, direction);
        }
    }

    private static void OnDashBegin(On.Celeste.Player.orig_DashBegin orig, Player player) {
        canGoIntoWall = true;
        bool b = player.Ducking;
        orig(player);
        if (b && player.Ducking && player.CollideCheck<Solid>()) {
            player.Ducking = false;
        }
    }

    private static void OnDashEnd(On.Celeste.Player.orig_DashEnd orig, Player player) {
        orig(player);
        canGoIntoWall = false;
    }

    private static void HookOrigUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(ins => ins.MatchCall<Actor>(nameof(Actor.Update)))) {
            cursor.Index++;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(HandleInWallMovement);
        }
        if (cursor.TryGotoNext(ins => ins.MatchLdcI4(22), ins => ins.OpCode == OpCodes.Beq_S)) {
            cursor.Index += 2;
            cursor.MoveAfterLabels();
            if (cursor.Prev.Operand is ILLabel target) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(PatchedMoveH);
                cursor.Emit(OpCodes.Brfalse, target);
            }
        }
#if PatchMoveV
        if (cursor.TryGotoNext(ins => ins.MatchLdfld<Player>(nameof(Player.onCollideV)), ins => ins.OpCode == OpCodes.Ldnull, ins => ins.MatchCallOrCallvirt<Actor>(nameof(Player.MoveV)))) {
            cursor.Index += 2;
            cursor.Remove();
            cursor.EmitDelegate(PatchedMoveV);
        }
#endif
    }

    public const float startInWallSpeed = 5f;

    public const int startInWallDepth = 4;

    public const int inWallTransportSpeed = 4;

    public static bool canGoIntoWall = false;

    public static bool justInWall = false;
    private static bool PatchedMoveH(Player player) {
        float moveH = player.Speed.X * Engine.DeltaTime;
        justInWall = false;
        if (Enabled && (Math.Abs(moveH) > startInWallSpeed || CrazyMode) && !player.CollideCheck<Solid>()) {
            // go into the wall
            float xBefore = player.X;
            float counterXBefore = player.movementCounter.X;
            player.movementCounter.X += moveH;
            int num = (int)Math.Round(player.movementCounter.X);
            player.X += num;
            player.movementCounter.X -= num;
            IEnumerable<Entity> solids = Engine.Scene.Tracker.Entities[typeof(Solid)].Where(x => player.CollideCheck(x)).ToList(); // idk, it seems if i dont use ToList then there will be some issue in the following foreach sentence
            int inWallDepth = (startInWallDepth - 1) * Math.Sign(moveH);
            player.X -= inWallDepth;
            bool result = solids.IsNullOrEmpty() || CrazyMode;
            // if there's no collision (so you have really high speed), then you dont need to be in StDash
            if (canGoIntoWall && !CrazyMode) {
                foreach (Entity solid in solids) {
                    if (player.CollideCheck(solid)) {
                        result = true;
                        justInWall = true;
                        canGoIntoWall = false;
                        break;
                    }
                }
            }
            if (result) {
                player.X += inWallDepth;
                if (player.Inventory.DreamDash && player.DashAttacking && player.CollideFirst<DreamBlock>() is { } dreamblock) {
                    player.dreamBlock = dreamblock;
                    player.StateMachine.State = 9;
                    player.dashAttackTimer = 0f;
                    player.gliderBoostTimer = 0f;
                }
                return false;
            }
            else {
                player.X = xBefore;
                player.movementCounter.X = counterXBefore;
            }
        }
        return true;
    }

    private static bool PatchedMoveV(Player actor, float moveV, Collision onCollide = null, Solid pusher = null) {
        if (justInWall) {
            actor.NaiveMove(moveV * Vector2.UnitY);
            return false;
        }
        return actor.MoveV(moveV, onCollide, pusher);
    }

    private static void HandleInWallMovement(Player player) {
        if (Enabled && player.InControl && player.StateMachine.State != 9 && player.CollideFirst<Solid>() is { } solid) {
            int length = inWallTransportSpeed;
            Vector2 move = player.Ducking ? Vector2.UnitY : Vector2.UnitX * (int)player.Facing;
            while (length > 0) {
                player.Position -= move;
                if (!player.CollideCheck(solid) && !player.CollideCheck<Solid>()) {
                    break;
                }
                length--;
            }
        }
    }
}
