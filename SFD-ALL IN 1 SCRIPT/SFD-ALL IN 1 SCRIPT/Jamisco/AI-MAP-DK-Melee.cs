using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDGameScriptInterface;

namespace SFDConsoleApplication1
{
    class AI_MAP_DK_Melee : GameScriptInterface
    {
        public AI_MAP_DK_Melee() : base(null) { }
        #region AI_MAP_DK_Melee
        static Random rnd = new Random();

        public void OnStartup()
        {
            The_Gunner.CreateBots("The_Gunner");
        }

        public abstract class Map_Details
        {
            protected static List<IObject> ledges = new List<IObject>(Game.GetObjectsByCustomID("ledge"));
            protected static List<IObject> ladders = new List<IObject>(Game.GetObjectsByCustomID("ladder"));
            protected static List<Vector2> cliff = new List<Vector2> { };
            protected static IObject[] XBarrel = Game.GetObjectsByName("BarrelExplosive");
            protected static List<IObject> AllObjects = new List<IObject>();
            protected static List<IObject> Objs_In_Path = new List<IObject>();
            public enum FloorLevel { one, two, three }
            #region floor levels
            private readonly int floorLvl_1 = -31;
            private readonly int floorLvl_2 = 15;
            private readonly int floorLvl_3 = 72;
            #endregion

            public Map_Details()
            {
                foreach (IObject obj in ledges)
                {
                    AllObjects.Add(obj);
                }

                foreach (IObject obj in ladders)
                {
                    AllObjects.Add(obj);
                }

                foreach (IObject obj in Game.GetObjectsByCustomID("cliff"))
                {
                    cliff.Add(obj.GetWorldPosition());
                }

                foreach (IObject obj in Game.GetObjectsByCustomID("Objs_In_Path"))
                {
                    Objs_In_Path.Add(obj);
                }
            }

            public float GetDistance(Vector2 BotPos, IObject obj)
            {
                float Distance = Vector2.Distance(new Vector2(BotPos.X, BotPos.Y),
                    new Vector2(obj.GetWorldPosition().X, obj.GetWorldPosition().Y));
                return Distance;
            }

            public float GetDistance(Vector2 BotPos, Vector2 obj_location)
            {
                float Distance = Vector2.Distance(new Vector2(BotPos.X, BotPos.Y),
                    new Vector2(obj_location.X, obj_location.Y));
                return Distance;
            }

            public FloorLevel FloorLvl(IObject obj)
            {
                if (obj.GetWorldPosition().Y < floorLvl_1)
                {
                    return FloorLevel.one;
                }
                else if (obj.GetWorldPosition().Y >= floorLvl_2 && obj.GetWorldPosition().Y < floorLvl_3)
                {
                    return FloorLevel.two;
                }
                else
                {
                    return FloorLevel.three;
                }
            }

            public FloorLevel FloorLvl(IPlayer plyr)
            {
                if (plyr.GetWorldPosition().Y < floorLvl_2)
                {
                    return FloorLevel.one;
                }
                else if (plyr.GetWorldPosition().Y >= floorLvl_2 && plyr.GetWorldPosition().Y < floorLvl_3)
                {
                    return FloorLevel.two;
                }
                else
                {
                    return FloorLevel.three;
                }
            }

        }

        public class The_Gunner : Map_Details
        {
            public IPlayer bot;
            public List<IObject> spawnLocation = new List<IObject>(Game.GetObjectsByName("SpawnPlayer"));
            List<Enemy> enemies = new List<Enemy>();
            public Enemy target = null;
            public bool NearTarget;
            Vector2 safeDstnce = new Vector2(5, 0);
            Events.UpdateCallback attck_updateEvent = null;
            //Events.UpdateCallback def_updateEvent = null;
            #region PlayerCommandTypes Declarations/ Initializations
            public PlayerCommandType sprint = PlayerCommandType.Sprint;
            public PlayerCommandType walk = PlayerCommandType.Walk;
            public PlayerCommandType punch = PlayerCommandType.AttackOnce;//rlly?
            public PlayerCommandType block = PlayerCommandType.Block;
            public PlayerCommandType move2Pos = PlayerCommandType.StartMoveToPosition;
            public PlayerCommandType arrvedDes = PlayerCommandType.WaitDestinationReached;
            public PlayerCommandType aim = PlayerCommandType.StartAimAtPrecise;
            public PlayerCommandType fire = PlayerCommandType.StartAttackRepeat;
            public PlayerCommandType ceaseFire = PlayerCommandType.StopAttackRepeat;
            public PlayerCommandType drawRifle = PlayerCommandType.DrawRifle;
            public PlayerCommandType drawPistol = PlayerCommandType.DrawHandgun;
            public PlayerCommandType drawMelee = PlayerCommandType.DrawMelee;
            public PlayerCommandType sheath = PlayerCommandType.Sheath;
            public PlayerCommandType grab = PlayerCommandType.Grab;
            public PlayerCommandType roll = PlayerCommandType.Roll;
            public PlayerCommandType dive = PlayerCommandType.Dive;
            public PlayerCommandType kick = PlayerCommandType.Kick;
            public PlayerCommandType jump = PlayerCommandType.Jump;
            public PlayerCommandType reload = PlayerCommandType.Reload;
            #endregion

            public static void CreateBots(string ID)
            {
                foreach (IObjectPlayerSpawnTrigger The_Gunner in Game.GetObjectsByCustomID(ID))
                {
                    new The_Gunner(The_Gunner.CreatePlayer(The_Gunner.GetWorldPosition()));
                    //spawns are random location
                }
            }

            public The_Gunner(IPlayer cpu)
            {
                bot = cpu;
                //  bot.SetWorldPosition(spawnLocation[rnd.Next(spawnLocation.Count)].GetWorldPosition());
                PlayerTeam GunnerTeam = bot.GetTeam();  // if bot team is set to none, error pops up
                if (GunnerTeam == PlayerTeam.Independent)    // change was made to account for that error here
                {
                    foreach (IPlayer plyr in Game.GetPlayers())
                    {
                        if (plyr != bot)
                        {
                            enemies.Add(new Enemy(plyr));
                        }
                    }
                }
                else
                {
                    foreach (IPlayer plyr in Game.GetPlayers())
                    {
                        if (plyr.GetTeam() != GunnerTeam)
                        {
                            enemies.Add(new Enemy(plyr));
                        }
                    }
                }

                attck_updateEvent = Events.UpdateCallback.Start(attck_OnUpdate, 200);
                //attck_updateEvent = Events.UpdateCallback.Start(Fire_OnUpdate, 200);
            } //constructor

            //public FloorLevel Get_FloorLvl
            //{
            //    get
            //    {
            //        if (bot.GetWorldPosition().Y < floorLvl_1)
            //        {
            //            return FloorLevel.one;
            //        }
            //        else if (bot.GetWorldPosition().Y >= floorLvl_2 && bot.GetWorldPosition().Y < floorLvl_3)
            //        {
            //            return FloorLevel.two;
            //        }
            //        else
            //        {
            //            return FloorLevel.three;
            //        }
            //    }
            //}

            public bool ObjInPath()
            {
                bool inPath = false;
                foreach (IObject obj in Objs_In_Path)
                {
                    if (GetDistance(bot.GetWorldPosition(), obj) <= 25)
                    {
                        inPath = true;
                    }
                }

                if (inPath == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public void Get_Enemies()
            {
                PlayerTeam botTeam = bot.GetTeam();
                enemies.RemoveAll(x => x.m_enemy.IsDead);
                enemies.RemoveAll(x => x.m_enemy == null);
                foreach (IPlayer plyr in Game.GetPlayers())
                {
                    if (bot.UniqueID == plyr.UniqueID) continue;
                    if (enemies.Find(x => x.m_enemy.UniqueID == plyr.UniqueID) != null) // if the player 
                    {
                        continue;
                    }
                    else
                    {
                        if (botTeam == PlayerTeam.Independent || plyr.GetTeam() == PlayerTeam.Independent || botTeam != plyr.GetTeam())
                        {
                            enemies.Add(new Enemy(plyr));
                        }
                    }
                }
            }

            public bool FindNearEnemy()
            {
                Get_Enemies();
                Vector2 GunnerPos = bot.GetWorldPosition();

                if (enemies.Count > 0)
                {
                    foreach (Enemy enemy in enemies)
                    {
                        if (enemy.m_enemy.IsDead || enemy == null)
                        {
                            continue;
                        }
                        if (target == null)
                        {
                            target = enemy;
                        }
                        else if (enemy.GetDistance(GunnerPos) < target.GetDistance(GunnerPos))
                        {
                            target = enemy;
                        }
                    }
                    if (target != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            public void GotoFloor(FloorLevel enemy_floorLvl)
            {
                List<Vector2> ledge_inbot_flrlvl = new List<Vector2>();
                List<Vector2> ladder_inbot_flrlvl = new List<Vector2>();
                float botX;
                float objX;
                float dstnce2Obj;

                if (FloorLvl(bot) != enemy_floorLvl)
                {
                    foreach (IObject obj in ledges)
                    {
                        if (FloorLvl(obj) == FloorLvl(bot))
                        {
                            ledge_inbot_flrlvl.Add(obj.GetWorldPosition());
                        }
                    }

                    if (ledge_inbot_flrlvl.Count > 0)
                    {
                        for (int i = 0; i <= ledge_inbot_flrlvl.Count; i++)
                        {
                            for (int j = i + 1; j <= ledge_inbot_flrlvl.Count - 1; j++)
                            {
                                float distance1 = GetDistance(bot.GetWorldPosition(), ledge_inbot_flrlvl[i]);
                                float distance2 = GetDistance(bot.GetWorldPosition(), ledge_inbot_flrlvl[j]);
                                if (distance2 > distance1)
                                {
                                    Vector2 tempvec = ledge_inbot_flrlvl[i];
                                    ledge_inbot_flrlvl[i] = ledge_inbot_flrlvl[j];
                                    ledge_inbot_flrlvl[j] = tempvec;
                                }
                            }
                        }
                    }

                    foreach (IObject obj in ladders)
                    {
                        if (FloorLvl(obj) == FloorLvl(bot))
                        {
                            ladder_inbot_flrlvl.Add(obj.GetWorldPosition());
                        }
                    }

                    if (ladder_inbot_flrlvl.Count > 0)
                    {
                        for (int i = 0; i <= ladder_inbot_flrlvl.Count; i++)
                        {
                            for (int j = i + 1; j <= ladder_inbot_flrlvl.Count - 1; j++)
                            {
                                float distance1 = GetDistance(bot.GetWorldPosition(), ladder_inbot_flrlvl[i]);
                                float distance2 = GetDistance(bot.GetWorldPosition(), ladder_inbot_flrlvl[j]);
                                if (distance2 > distance1)
                                {
                                    Vector2 tempvec = ladder_inbot_flrlvl[i];
                                    ladder_inbot_flrlvl[i] = ladder_inbot_flrlvl[j];
                                    ladder_inbot_flrlvl[j] = tempvec;
                                }
                            }
                        }
                    }

                    if (enemy_floorLvl > FloorLvl(bot))
                    {
                        if (GetDistance(bot.GetWorldPosition(), ledge_inbot_flrlvl.First()) <=
                            GetDistance(bot.GetWorldPosition(), ladder_inbot_flrlvl.First()))
                        {
                            botX = Math.Abs(bot.GetWorldPosition().X);
                            objX = Math.Abs(ledge_inbot_flrlvl.First().X);

                            if (botX > objX)
                            {
                                dstnce2Obj = botX - objX;
                            }
                            else if (objX > botX)
                            {
                                dstnce2Obj = objX - botX;
                            }
                            else
                            {
                                dstnce2Obj = botX - objX;
                            }

                            if (dstnce2Obj < 20 && dstnce2Obj > 5 || bot.IsLedgeGrabbing && dstnce2Obj < 5)
                            {
                                Game.CreateDialogue("JUmpppp", bot, "", 1000);
                                if (dstnce2Obj < 20 && !bot.IsLedgeGrabbing)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(move2Pos, ledge_inbot_flrlvl.First()));
                                    bot.AddCommand(new PlayerCommand(sprint));
                                    bot.AddCommand(new PlayerCommand(jump, ledge_inbot_flrlvl.First()));
                                }

                                if (bot.IsLedgeGrabbing)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(move2Pos, target.m_enemy.GetWorldPosition()));
                                    bot.AddCommand(new PlayerCommand(jump, target.m_enemy.GetWorldPosition()));
                                    bot.AddCommand(new PlayerCommand(sprint));
                                }
                            }
                            else
                            {
                                Game.CreateDialogue("position", bot, "", 1000);
                                bot.ClearCommandQueue();
                                if (bot.FacingDirection == 1)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(move2Pos, ledge_inbot_flrlvl.First() - new Vector2(15, 0)));
                                    bot.AddCommand(new PlayerCommand(sprint));
                                    bot.AddCommand(new PlayerCommand(PlayerCommandType.WaitDestinationReached));
                                }
                                else
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(move2Pos, ledge_inbot_flrlvl.First() + new Vector2(15, 0)));
                                    bot.AddCommand(new PlayerCommand(sprint));
                                    bot.AddCommand(new PlayerCommand(PlayerCommandType.WaitDestinationReached));
                                }
                            }
                        }
                        else
                        {
                            if (ladder_inbot_flrlvl.Count > 0)
                            {
                                Game.ShowPopupMessage("more");
                            }
                            else
                            {
                                Game.ShowPopupMessage("Zero");
                            }

                            botX = Math.Abs(bot.GetWorldPosition().X);
                            objX = Math.Abs(ladder_inbot_flrlvl.First().X);
                            if (botX >= objX)
                            {
                                dstnce2Obj = botX - objX;
                            }
                            else 
                            {
                                dstnce2Obj = objX - botX;
                            }

                            if (dstnce2Obj < 20)
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(move2Pos, ladder_inbot_flrlvl.First()));
                                bot.AddCommand(new PlayerCommand(sprint));
                                bot.AddCommand(new PlayerCommand(jump, ladder_inbot_flrlvl.First()));
                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StartClimbUp));
                            }
                            else
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(move2Pos, ladder_inbot_flrlvl.First()));
                                bot.AddCommand(new PlayerCommand(sprint));
                            }
                        }
                    }
                }
            }

            public void GoToEnemyAndAttack()
            {
                //Game.CreateDialogue("ingotoenemy", bot); // DEBUG
                //Game.ShowPopupMessage(target.GetDistance(bot.GetWorldPosition()).ToString());
                #region direction
                float x = (target.m_enemy.GetWorldPosition() - bot.GetWorldPosition()).X;
                PlayerCommandFaceDirection direction = (PlayerCommandFaceDirection)(x / Math.Abs(x));
                #endregion
                #region logic that kinda has to be here
                if (bot.CurrentWeaponDrawn == WeaponItemType.Rifle
                    || bot.CurrentWeaponDrawn == WeaponItemType.Handgun)
                {
                    bot.ClearCommandQueue();
                    bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                    bot.AddCommand(new PlayerCommand(ceaseFire));
                    bot.AddCommand(new PlayerCommand(sheath));
                }

                if (bot.CurrentMeleeWeapon.WeaponItem != WeaponItem.NONE && bot.CurrentWeaponDrawn != WeaponItemType.Melee)
                {
                    bot.ClearCommandQueue();
                    bot.AddCommand(new PlayerCommand(drawMelee));
                }


                if (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring)
                {
                    if (target.GetDistance(bot.GetWorldPosition()) > 15 && target.GetDistance(bot.GetWorldPosition()) < 45)
                    {
                        int num = rnd.Next(1, 3);
                        if (num == 1)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(roll, target.m_enemy.GetWorldPosition()));
                        }
                        else
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(dive, target.m_enemy.GetWorldPosition()));
                        }
                    }
                }
                #endregion

                if (target.GetDistance(bot.GetWorldPosition()) > 15 || FloorLvl(target.m_enemy) != FloorLvl(bot))
                {
                    if (!ObjInPath() && FloorLvl(target.m_enemy) == FloorLvl(bot))
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(move2Pos, target.m_enemy.GetWorldPosition() -
                        safeDstnce * (int)direction, direction));
                        bot.AddCommand(new PlayerCommand(sprint));
                    }
                    else if (ObjInPath() && FloorLvl(target.m_enemy) == FloorLvl(bot))
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(jump, target.m_enemy.GetWorldPosition()));
                    }
                    else if (FloorLvl(bot) < FloorLvl(target.m_enemy))
                    {
                        bot.ClearCommandQueue();
                        Game.CreateDialogue("drop", bot, "", 500);
                        bot.AddCommand(new PlayerCommand(PlayerCommandType.DropPlatform));
                    }
                    else if (FloorLvl(bot) > FloorLvl(target.m_enemy))
                    {
                        Game.CreateDialogue("floor", bot, "", 500);
                        GotoFloor(FloorLvl(target.m_enemy));
                    }
                    NearTarget = false;
                }
                else if (target.GetDistance(bot.GetWorldPosition()) < 15 && FloorLvl(target.m_enemy) == FloorLvl(bot))
                {
                    NearTarget = true;
                    if (target.m_enemy.IsMeleeAttacking)
                    {
                        // Game.ShowPopupMessage("isMA");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(block));
                        bot.AddCommand(new PlayerCommand(punch, direction));
                    }
                    else if (target.m_enemy.IsJumpKicking)
                    {
                        // Game.ShowPopupMessage("isjumpkicking");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(block, bot.FacingDirection));
                    }
                    else if (target.m_enemy.IsJumpAttacking)
                    {
                        // Game.ShowPopupMessage("isjumpattacking");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(block));
                    }
                    else if (target.m_enemy.IsDiving)
                    {
                        // Game.ShowPopupMessage("isDiving");
                    }
                    else if (target.m_enemy.IsInMidAir)
                    {
                        // Game.ShowPopupMessage("inAir");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(grab, target.m_enemy.UniqueID, 100));
                    }
                    else if (target.m_enemy.IsCrouching)
                    {
                        // Game.ShowPopupMessage("isCrouching");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(kick));
                    }
                    else if (target.m_enemy.IsGrabbing)
                    {
                        // Game.ShowPopupMessage("isGrabbing");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(roll, bot.FacingDirection / -1));
                    }
                    else if (target.m_enemy.IsKicking)
                    {
                        //  Game.ShowPopupMessage("isKicking");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(block));
                        bot.AddCommand(new PlayerCommand(punch, direction));
                    }
                    else if (target.m_enemy.IsManualAiming)
                    {
                        // Game.ShowPopupMessage("isAiming");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(block));
                        bot.AddCommand(new PlayerCommand(punch, direction));
                    }
                    else if (target.m_enemy.IsThrowing)
                    {
                        //Game.ShowPopupMessage("isThrowing");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(block));
                        bot.AddCommand(new PlayerCommand(punch, direction));
                    }
                    else if (target.m_enemy.IsDrawingWeapon)
                    {
                        // Game.ShowPopupMessage("isDrawingWeapon");
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(punch, direction));
                    }
                    else if (target.m_enemy.IsBlocking)
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(grab));
                    }
                    else if (target.m_enemy.IsLedgeGrabbing)
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(kick, direction));
                    }
                    else if (!target.m_enemy.IsDead || target != null)
                    {
                        //   Game.ShowPopupMessage("else");
                        bot.ClearCommandQueue();
                        if (new Random().Next(100) >= 20)
                        {
                            bot.AddCommand(new PlayerCommand(punch, direction));
                        }
                        else
                        {
                            if (new Random().Next(21) >= 10)
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(jump, direction));
                                bot.AddCommand(new PlayerCommand(punch, direction));
                            }
                            else
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(jump, direction));
                                bot.AddCommand(new PlayerCommand(kick, direction));
                            }
                        }
                    }

                    if (bot.IsHoldingPlayerInGrab)
                    {
                        bot.ClearCommandQueue();
                        if (new Random().Next(100) <= 70)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(punch));
                        }
                        else
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.Throw, -bot.FacingDirection, 200));
                            // 66% chance of non change FacingDirection
                        }
                    }
                }
            }

            private void GameOver()
            {
                IPlayer winner = null;

                if (Game.GetPlayers().Length != 0)
                {
                    foreach (IPlayer plyr in Game.GetPlayers())
                    {
                        winner = plyr;
                    }
                    if (winner == this.bot)
                    {
                        Game.CreateDialogue("Not Bad For A Mere Mortal", bot, "", 5000);
                        Game.SetGameOver(" THE BOT WON THE MATCH, BETTER LUCK NEXT TIME");
                    }
                    Game.SetGameOver(winner.Name + " WON THE MATCH, BETTER LUCK NEXT TIME");
                }
                else
                {
                    Game.SetGameOver("NO WINNER, BETTER LUCK NEXT TIME");
                }
                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAll));
            }

            private void attck_OnUpdate(float elapsed)
            {
                if (target != null && target.m_enemy.IsDead)
                {
                    Game.CreateDialogue("Target Eliminated!", bot);
                    enemies.Remove(target);
                    target = null;
                }

                if (FindNearEnemy() && !bot.IsDead)
                {
                    if (target.In_GFZ(bot.GetWorldPosition(), bot))
                    {
                        if (bot.CurrentWeaponDrawn == WeaponItemType.Rifle
                            || bot.CurrentWeaponDrawn == WeaponItemType.Handgun)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(sheath));
                        }
                        else if (bot.CurrentMeleeWeapon.WeaponItem != WeaponItem.NONE)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(drawMelee));
                        }
                        if (bot.CurrentWeaponDrawn != WeaponItemType.Rifle ||
                            bot.CurrentWeaponDrawn != WeaponItemType.Handgun)
                        {
                            GoToEnemyAndAttack();
                        }
                    }
                    else if (!target.In_GFZ(bot.GetWorldPosition(), bot))
                    {
                        if (bot.CurrentPrimaryWeapon.WeaponItem != WeaponItem.NONE)
                        {
                            if (bot.CurrentWeaponDrawn != WeaponItemType.Rifle && bot.CurrentPrimaryWeapon.TotalAmmo > 0)
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(drawRifle));
                            }
                            if (bot.CurrentPrimaryWeapon.TotalAmmo == 0)
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                                bot.AddCommand(new PlayerCommand(ceaseFire));
                                bot.AddCommand(new PlayerCommand(sheath));
                                if (bot.CurrentWeaponDrawn == WeaponItemType.NONE
                                    && bot.CurrentSecondaryWeapon.TotalAmmo == 0)
                                {
                                    GoToEnemyAndAttack();
                                }
                                else if (bot.CurrentSecondaryWeapon.TotalAmmo > 0)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(drawPistol));
                                }
                            }
                        }
                        else if (bot.CurrentSecondaryWeapon.WeaponItem != WeaponItem.NONE)
                        {
                            if (bot.CurrentWeaponDrawn != WeaponItemType.Handgun &&
                                bot.CurrentSecondaryWeapon.TotalAmmo > 0)
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(drawPistol));
                            }
                            if (bot.CurrentSecondaryWeapon.TotalAmmo == 0)
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                                bot.AddCommand(new PlayerCommand(ceaseFire));
                                bot.AddCommand(new PlayerCommand(sheath));
                                if (bot.CurrentWeaponDrawn == WeaponItemType.NONE
                                    && bot.CurrentPrimaryWeapon.TotalAmmo == 0)
                                {
                                    GoToEnemyAndAttack();
                                }
                                else if (bot.CurrentPrimaryWeapon.TotalAmmo > 0)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(drawPistol));
                                }
                            }
                        }
                        //else if ((target.m_enemy.IsTakingCover && !target.m_enemy.IsManualAiming) && bot.CurrentWeaponDrawn != WeaponItemType.NONE)
                        //{
                        //    bot.ClearCommandQueue();
                        //    bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                        //    bot.AddCommand(new PlayerCommand(ceaseFire));
                        //    bot.AddCommand(new PlayerCommand(PlayerCommandType.StartMoveToPosition, target.m_enemy.GetWorldPosition()));
                        //    bot.AddCommand(new PlayerCommand(walk));

                        //}
                        else
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(ceaseFire));
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                            bot.AddCommand(new PlayerCommand(sheath));
                            if (bot.CurrentWeaponDrawn == WeaponItemType.NONE)
                            {
                                GoToEnemyAndAttack();
                            }
                        }
                    }
                }
                else
                {
                    GameOver();
                }
            }

            //private void Fire_OnUpdate(float elapsed)
            //{
            //    int num2 = rnd.Next(1, 4);
            //    int num = rnd.Next(1, 3);

            //    if (enemies.Count != 0 && !bot.IsDead)
            //    {
            //        foreach (Enemy enemy in enemies)
            //        {
            //            if (enemy != target)
            //            {
            //                IPlayer plyr = enemy.m_enemy;

            //                if (plyr.IsManualAiming || plyr.IsHipFiring)
            //                {
            //                    if (num == 1)
            //                    {
            //                        bot.ClearCommandQueue();
            //                        bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
            //                        bot.AddCommand(new PlayerCommand(ceaseFire));
            //                        bot.AddCommand(new PlayerCommand(roll, rnd.Next(-1, 2), 200));
            //                    }
            //                    else if (num == 2)
            //                    {
            //                        bot.ClearCommandQueue();
            //                        bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
            //                        bot.AddCommand(new PlayerCommand(ceaseFire));
            //                        bot.AddCommand(new PlayerCommand(dive, rnd.Next(-1, 2), 200));
            //                    }
            //                }
            //            }
            //        } // checks if other players are aiming at the bot
            //    }
            //    else
            //    {
            //        GameOver();
            //    }

            //    if (target != null && enemies.Count > 0 && !bot.IsDead)
            //    {
            //        if (!target.In_GFZ(bot.GetWorldPosition(), bot))
            //        {

            //            if (bot.CurrentWeaponDrawn == WeaponItemType.Rifle && !target.m_enemy.IsRolling &&
            //                !target.m_enemy.IsDiving && !target.m_enemy.IsTakingCover || target.m_enemy.IsTakingCover &&
            //                (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring) &&
            //                bot.CurrentWeaponDrawn == WeaponItemType.Rifle)
            //            {
            //                if (bot.CurrentPrimaryWeapon.CurrentAmmo > 0)
            //                {
            //                    if (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring)
            //                    {
            //                        if (num2 == 1 || num2 == 2)
            //                        {
            //                            bot.ClearCommandQueue();
            //                            bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
            //                            bot.AddCommand(new PlayerCommand(fire));
            //                        }
            //                        else
            //                        {
            //                            if (num == 1)
            //                            {
            //                                bot.ClearCommandQueue();
            //                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim, 0, 100));
            //                                bot.AddCommand(new PlayerCommand(ceaseFire));
            //                                if (!bot.IsDiving || !bot.IsRolling)
            //                                {
            //                                    bot.AddCommand(new PlayerCommand(roll, rnd.Next(-1, 2), 500));

            //                                    // Game.CreateDialogue("roll", bot, "", 500); //DEBUG
            //                                }
            //                            }
            //                            else if (num == 2)
            //                            {

            //                                bot.ClearCommandQueue();
            //                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
            //                                bot.AddCommand(new PlayerCommand(ceaseFire));
            //                                if (!bot.IsDiving || !bot.IsRolling)
            //                                {
            //                                    bot.AddCommand(new PlayerCommand(dive, rnd.Next(-1, 2), 1000));
            //                                    //Game.CreateDialogue("dive", bot, "", 1000); //DEBUG
            //                                }
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        bot.ClearCommandQueue();
            //                        bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
            //                        bot.AddCommand(new PlayerCommand(fire));
            //                    }
            //                }
            //            }
            //            else
            //            if (bot.CurrentWeaponDrawn == WeaponItemType.Handgun && !target.m_enemy.IsRolling &&
            //               !target.m_enemy.IsDiving && !target.m_enemy.IsTakingCover || target.m_enemy.IsTakingCover &&
            //               (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring) &&
            //                bot.CurrentWeaponDrawn == WeaponItemType.Handgun)
            //            {
            //                if (bot.CurrentSecondaryWeapon.CurrentAmmo > 0)
            //                {
            //                    if (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring)
            //                    {
            //                        if (num2 == 1 || num2 == 2)
            //                        {
            //                            bot.ClearCommandQueue();
            //                            //   Game.CreateDialogue("inFire", target.m_enemy, "", 500);
            //                            bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
            //                            bot.AddCommand(new PlayerCommand(fire));
            //                        }
            //                        else
            //                        {
            //                            if (num == 1)
            //                            {
            //                                bot.ClearCommandQueue();
            //                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim, 0, 100));
            //                                bot.AddCommand(new PlayerCommand(ceaseFire));
            //                                if (!bot.IsDiving || !bot.IsRolling)
            //                                {
            //                                    bot.AddCommand(new PlayerCommand(roll, rnd.Next(-1, 2), 500));

            //                                    //Game.CreateDialogue("roll", bot, "", 500); //DEBUG
            //                                }
            //                            }
            //                            else if (num == 2)
            //                            {
            //                                bot.ClearCommandQueue();
            //                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
            //                                bot.AddCommand(new PlayerCommand(ceaseFire));
            //                                if (!bot.IsDiving || !bot.IsRolling)
            //                                {
            //                                    bot.AddCommand(new PlayerCommand(dive, rnd.Next(-1, 2), 1000));
            //                                    //  Game.CreateDialogue("dive", bot, "", 1000); //DEBUG
            //                                }
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        bot.ClearCommandQueue();
            //                        //  Game.CreateDialogue("inFire", target.m_enemy, "", 500);
            //                        bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
            //                        bot.AddCommand(new PlayerCommand(fire));
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if (bot.IsManualAiming)
            //                {
            //                    bot.ClearCommandQueue();
            //                    bot.AddCommand(new PlayerCommand(ceaseFire));
            //                }
            //                else
            //                {
            //                    bot.ClearCommandQueue();
            //                    bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
            //                    bot.AddCommand(new PlayerCommand(ceaseFire));
            //                }
            //            }
            //        }
            //        else
            //        {
            //            bot.ClearCommandQueue();
            //            bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
            //            bot.AddCommand(new PlayerCommand(ceaseFire));
            //            bot.AddCommand(new PlayerCommand(sheath));
            //        }
            //    }
            //    else if (bot.IsDead)
            //    {
            //        GameOver();
            //    }

            //}
        }

        public class Enemy : Map_Details
        {
            public IPlayer m_enemy;

            public Enemy(IPlayer enemy)
            {
                m_enemy = enemy;
            }

            public float GetDistance(Vector2 BotPosition)
            {
                float Distance = Vector2.Distance(new Vector2(BotPosition.X, BotPosition.Y),
                    new Vector2(m_enemy.GetWorldPosition().X, m_enemy.GetWorldPosition().Y));
                return Distance;
            }

            public bool In_GFZ(Vector2 BotPos, IPlayer bot) //GFZ means Gun Free Zone ... so this is the distance where bot should not use gun
            {
                if (FloorLvl(m_enemy) == FloorLvl(bot) && GetDistance(bot.GetWorldPosition()) < 60)
                {
                    return true;
                }
                else if (GetDistance(bot.GetWorldPosition()) <= 40)
                {
                    return false;
                }
                else
                {
                    return false;
                }


            }
        }
        #endregion

    }
}
