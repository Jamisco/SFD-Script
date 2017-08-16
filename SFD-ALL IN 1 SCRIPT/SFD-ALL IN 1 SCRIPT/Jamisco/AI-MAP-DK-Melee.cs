using SFDGameScriptInterface;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    AllObjects.Add(obj);
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
                if (obj.GetWorldPosition().Y > floorLvl_1 && obj.GetWorldPosition().Y < floorLvl_2)
                {
                    return FloorLevel.one;
                }
                else if (obj.GetWorldPosition().Y > floorLvl_2 && obj.GetWorldPosition().Y < floorLvl_3)
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
                if (plyr.GetWorldPosition().Y > floorLvl_1 && plyr.GetWorldPosition().Y < floorLvl_2)
                {
                    return FloorLevel.one;
                }
                else if (plyr.GetWorldPosition().Y > floorLvl_2 && plyr.GetWorldPosition().Y < floorLvl_3)
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

            public void MoveTo(Vector2 location, Vector2 tolerance)
            {
                List<Vector2> objs_inbot_flrlvl = new List<Vector2>();
                foreach (IObject obj in Objs_In_Path)
                {
                    if (FloorLvl(obj) == FloorLvl(bot))
                    {
                        objs_inbot_flrlvl.Add(obj.GetWorldPosition());
                    }
                }

                if (objs_inbot_flrlvl.Count > 0)
                {
                    for (int i = 0; i <= objs_inbot_flrlvl.Count; i++)
                    {
                        for (int j = i + 1; j <= objs_inbot_flrlvl.Count - 1; j++)
                        {
                            float distance1 = GetDistance(bot.GetWorldPosition(), objs_inbot_flrlvl[i]);
                            float distance2 = GetDistance(bot.GetWorldPosition(), objs_inbot_flrlvl[j]);
                            if (distance1 > distance2)
                            {
                                Vector2 tempvec = objs_inbot_flrlvl[i];
                                objs_inbot_flrlvl[i] = objs_inbot_flrlvl[j];
                                objs_inbot_flrlvl[j] = tempvec;
                            }
                        }
                    }

                    float botX = Math.Abs(bot.GetWorldPosition().X);
                    float objX = Math.Abs(objs_inbot_flrlvl.First().X);
                    float dstnce2Obj;
                    bool canjump;

                    if (FloorLvl(bot) != FloorLvl(target.m_enemy))
                    {
                        canjump = true;
                    }
                    else if (bot.GetWorldPosition().X >= objs_inbot_flrlvl.First().X &&
                        target.m_enemy.GetWorldPosition().X >= objs_inbot_flrlvl.First().X)
                    {
                        canjump = false;
                    }
                    else if (bot.GetWorldPosition().X <= objs_inbot_flrlvl.First().X &&
                        target.m_enemy.GetWorldPosition().X <= objs_inbot_flrlvl.First().X)
                    {
                        canjump = false;
                    }
                    else
                    {
                        canjump = true;
                    }

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

                    if (dstnce2Obj <= 20 && canjump)
                    {
                        if (bot.IsLedgeGrabbing)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.DropPlatform));
                        }
                        else if (target.m_enemy.IsTakingCover)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(roll, bot.FacingDirection * -1));
                        }
                        else
                        {
                            Game.CreateDialogue("jumpoverobj", bot, "", 200);
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(move2Pos, location + tolerance));
                            bot.AddCommand(new PlayerCommand(sprint));
                            bot.AddCommand(new PlayerCommand(jump));
                        }
                    }
                    else
                    {
                        if (bot.IsLedgeGrabbing)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.DropPlatform));
                        }
                        else if (target.m_enemy.IsTakingCover)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(move2Pos, new Vector2(5, 0) * bot.FacingDirection));
                            bot.AddCommand(new PlayerCommand(sprint));
                        }
                        else
                        {
                            bot.ClearCommandQueue();
                            Game.CreateDialogue("obj far away", bot, "", 200);
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StopClimb));
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.FaceAt, location));
                            bot.AddCommand(new PlayerCommand(move2Pos, location + tolerance));
                            bot.AddCommand(new PlayerCommand(sprint));
                        }
                    }
                }
                else
                {
                    if (bot.IsLedgeGrabbing)
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(PlayerCommandType.DropPlatform));
                    }
                    else if (target.m_enemy.IsTakingCover)
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(roll, bot.FacingDirection * -1));
                    }
                    else
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(PlayerCommandType.StopClimb));
                        bot.AddCommand(new PlayerCommand(PlayerCommandType.FaceAt, location));
                        bot.AddCommand(new PlayerCommand(move2Pos, location + tolerance));
                        bot.AddCommand(new PlayerCommand(sprint));
                    }
                }
            }

            public void GoToFloor(FloorLevel floorLvl2Go2)
            {
                List<Vector2> ledge_inbot_flrlvl = new List<Vector2>();
                List<Vector2> ladder_inbot_flrlvl = new List<Vector2>();
                float botX;
                float objX;
                float dstnce2Obj;

                if (FloorLvl(bot) != floorLvl2Go2)
                {
                    foreach (IObject obj in ledges)
                    {
                        if (FloorLvl(obj) == FloorLvl(bot))
                        {
                            ledge_inbot_flrlvl.Add(obj.GetWorldPosition());
                        }
                    }

                    for (int i = 0; i <= ledge_inbot_flrlvl.Count; i++)
                    {
                        for (int j = i + 1; j <= ledge_inbot_flrlvl.Count - 1; j++)
                        {
                            float distance1 = GetDistance(bot.GetWorldPosition(), ledge_inbot_flrlvl[i]);
                            float distance2 = GetDistance(bot.GetWorldPosition(), ledge_inbot_flrlvl[j]);
                            if (distance1 > distance2)
                            {
                                Vector2 tempvec = ledge_inbot_flrlvl[i];
                                ledge_inbot_flrlvl[i] = ledge_inbot_flrlvl[j];
                                ledge_inbot_flrlvl[j] = tempvec;
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

                    for (int i = 0; i <= ladder_inbot_flrlvl.Count; i++)
                    {
                        for (int j = i + 1; j <= ladder_inbot_flrlvl.Count - 1; j++)
                        {
                            float distance1 = GetDistance(bot.GetWorldPosition(), ladder_inbot_flrlvl[i]);
                            float distance2 = GetDistance(bot.GetWorldPosition(), ladder_inbot_flrlvl[j]);
                            if (distance1 > distance2)
                            {
                                Vector2 tempvec = ladder_inbot_flrlvl[i];
                                ladder_inbot_flrlvl[i] = ladder_inbot_flrlvl[j];
                                ladder_inbot_flrlvl[j] = tempvec;
                            }
                        }
                    }

                    if (floorLvl2Go2 > FloorLvl(bot))
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

                            if (dstnce2Obj < 20 && dstnce2Obj > 10 || bot.IsLedgeGrabbing && dstnce2Obj < 10)
                            {
                                if (!bot.IsLedgeGrabbing)
                                {
                                    Game.CreateDialogue("jump2ledge", bot, "", 1000);
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(move2Pos, ledge_inbot_flrlvl.First()));
                                    bot.AddCommand(new PlayerCommand(sprint));
                                    bot.AddCommand(new PlayerCommand(jump));
                                }

                                if (bot.IsLedgeGrabbing)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(move2Pos, target.m_enemy.GetWorldPosition()));
                                    bot.AddCommand(new PlayerCommand(jump));
                                    bot.AddCommand(new PlayerCommand(sprint));
                                }
                            }
                            else
                            {
                                Game.CreateDialogue("positioning", bot, "", 1000);
                                MoveTo(ledge_inbot_flrlvl.First(), new Vector2(20, 0) *
                                    bot.FacingDirection);
                            }
                        }
                        else
                        {
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

                            if (dstnce2Obj < 5)
                            {
                                Game.CreateDialogue("jump 2 ladder", bot, "", 500);
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(move2Pos, ladder_inbot_flrlvl.First()));
                                bot.AddCommand(new PlayerCommand(sprint));
                                bot.AddCommand(new PlayerCommand(jump));
                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StartClimbUp));
                            }
                            else
                            {
                                MoveTo(ladder_inbot_flrlvl.First(), new Vector2(0, 0));
                            }
                        }
                    }
                    else
                    {
                        List<Vector2> objs_inbot_flrlvl = new List<Vector2>();

                        foreach (IObject obj in Objs_In_Path)
                        {
                            if (FloorLvl(obj) == FloorLvl(bot))
                            {
                                objs_inbot_flrlvl.Add(obj.GetWorldPosition());
                            }
                        }

                        if (objs_inbot_flrlvl.Count > 0)
                        {
                            for (int i = 0; i <= objs_inbot_flrlvl.Count; i++)
                            {
                                for (int j = i + 1; j <= objs_inbot_flrlvl.Count - 1; j++)
                                {
                                    float distance1 = GetDistance(bot.GetWorldPosition(), objs_inbot_flrlvl[i]);
                                    float distance2 = GetDistance(bot.GetWorldPosition(), objs_inbot_flrlvl[j]);
                                    if (distance1 > distance2)
                                    {
                                        Vector2 tempvec = objs_inbot_flrlvl[i];
                                        objs_inbot_flrlvl[i] = objs_inbot_flrlvl[j];
                                        objs_inbot_flrlvl[j] = tempvec;
                                    }
                                }
                            }

                            botX = Math.Abs(bot.GetWorldPosition().X);
                            objX = Math.Abs(objs_inbot_flrlvl.First().X);

                            if (botX >= objX)
                            {
                                dstnce2Obj = botX - objX;
                            }
                            else
                            {
                                dstnce2Obj = objX - botX;
                            }

                            if (dstnce2Obj > 6)
                            {
                                bot.ClearCommandQueue();
                                Game.CreateDialogue("move to drop", bot, "", 200);
                                bot.AddCommand(new PlayerCommand(PlayerCommandType.DropPlatform));
                            }
                            else
                            {
                                MoveTo(objs_inbot_flrlvl.First(), new Vector2(15, 0) * bot.FacingDirection);
                            }
                        }
                    }
                }
            }

            public void StayWithTartget(bool isClimbing, bool close2Obj)
            {
                List<Vector2> ledge_inbot_flrlvl = new List<Vector2>();
                List<Vector2> ladder_inbot_flrlvl = new List<Vector2>();
                foreach (IObject obj in ledges)
                {
                    if (FloorLvl(obj) == FloorLvl(bot))
                    {
                        ledge_inbot_flrlvl.Add(obj.GetWorldPosition());
                    }
                }

                for (int i = 0; i <= ledge_inbot_flrlvl.Count; i++)
                {
                    for (int j = i + 1; j <= ledge_inbot_flrlvl.Count - 1; j++)
                    {
                        float distance1 = GetDistance(bot.GetWorldPosition(), ledge_inbot_flrlvl[i]);
                        float distance2 = GetDistance(bot.GetWorldPosition(), ledge_inbot_flrlvl[j]);
                        if (distance1 > distance2)
                        {
                            Vector2 tempvec = ledge_inbot_flrlvl[i];
                            ledge_inbot_flrlvl[i] = ledge_inbot_flrlvl[j];
                            ledge_inbot_flrlvl[j] = tempvec;
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

                for (int i = 0; i <= ladder_inbot_flrlvl.Count; i++)
                {
                    for (int j = i + 1; j <= ladder_inbot_flrlvl.Count - 1; j++)
                    {
                        float distance1 = GetDistance(bot.GetWorldPosition(), ladder_inbot_flrlvl[i]);
                        float distance2 = GetDistance(bot.GetWorldPosition(), ladder_inbot_flrlvl[j]);
                        if (distance1 > distance2)
                        {
                            Vector2 tempvec = ladder_inbot_flrlvl[i];
                            ladder_inbot_flrlvl[i] = ladder_inbot_flrlvl[j];
                            ladder_inbot_flrlvl[j] = tempvec;
                        }
                    }
                }

                if (isClimbing == true)
                {
                    Game.ShowPopupMessage(GetDistance(bot.GetWorldPosition(),target.m_enemy.GetWorldPosition()).ToString());
                    if (GetDistance(bot.GetWorldPosition(), target.m_enemy.GetWorldPosition()) <= 25)
                    {
                        Game.CreateDialogue("ic_jump attack", bot, "", 200);
                        int i = rnd.Next(1, 2);
                        if (i == 1)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(move2Pos, target.m_enemy.UniqueID));
                            bot.AddCommand(new PlayerCommand(sprint));
                            bot.AddCommand(new PlayerCommand(jump));
                            bot.AddCommand(new PlayerCommand(kick));
                        }
                        else
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(move2Pos, target.m_enemy.UniqueID));
                            bot.AddCommand(new PlayerCommand(sprint));
                            bot.AddCommand(new PlayerCommand(jump));
                            bot.AddCommand(new PlayerCommand(punch));
                        }
                    }
                    else if (GetDistance(bot.GetWorldPosition(), target.m_enemy.GetWorldPosition()) > 25)
                    {
                        Game.CreateDialogue("ic_goto", bot, "", 200);
                        MoveTo(target.m_enemy.GetWorldPosition(), Vector2.Zero);
                        float botX = Math.Abs(bot.GetWorldPosition().X);
                        float objX = Math.Abs(ladder_inbot_flrlvl.First().X);
                        float dstnce2Obj;
 
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
                        if (dstnce2Obj <= 6 && bot.GetWorldPosition().Y < target.m_enemy.GetWorldPosition().Y)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StartClimbUp));
                        }
                        else if (dstnce2Obj <= 6 && bot.GetWorldPosition().Y > target.m_enemy.GetWorldPosition().Y)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StartClimbDown));
                        }
                    }
                }
                else if (close2Obj == true)
                {
                    float botX = Math.Abs(bot.GetWorldPosition().X);
                    float objX = Math.Abs(ledge_inbot_flrlvl.First().X);
                    float dstnce2Obj;

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
                    Game.ShowPopupMessage(dstnce2Obj.ToString());

                    if (dstnce2Obj < 20 && dstnce2Obj > 10 || bot.IsLedgeGrabbing && dstnce2Obj < 10)
                    {
                        if (!bot.IsLedgeGrabbing)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(move2Pos, ledge_inbot_flrlvl.First()));
                            bot.AddCommand(new PlayerCommand(sprint));
                            bot.AddCommand(new PlayerCommand(jump));
                        }

                        if (bot.IsLedgeGrabbing)
                        {
                            bot.ClearCommandQueue();
                            bot.AddCommand(new PlayerCommand(move2Pos, target.m_enemy.GetWorldPosition()));
                            bot.AddCommand(new PlayerCommand(jump));
                            bot.AddCommand(new PlayerCommand(sprint));
                        }
                    }
                    else
                    {
                        MoveTo(ledge_inbot_flrlvl.First(), new Vector2(20, 0) *
                            bot.FacingDirection);
                    }
                }
            }

            public void GoToEnemyAndAttack()
            {
                //Game.CreateDialogue("ingotoenemy", bot); // DEBUG
                //Game.ShowPopupMessage(target.GetDistance(bot.GetWorldPosition()).ToString());
                bool close2Obj = false;
                List<Area> Areas = new List<Area>();
                List<IObject[]> objArea = new List<IObject[]>();

                foreach (IObject obj in ledges)
                {
                    if (FloorLvl(obj) == FloorLvl(bot))
                    {
                        Areas.Add(new Area(obj.GetWorldPosition() - new Vector2(10, 10),
                            target.m_enemy.GetWorldPosition() + new Vector2(10, 10)));
                    }
                }

                foreach (Area area in Areas)
                {
                    objArea.Add(Game.GetObjectsByArea(area));
                }

                foreach (IObject[] objs in objArea)
                {
                    if (objs.Contains(target.m_enemy))
                    {
                        close2Obj = true;
                    }
                }

                #region direction
                float x = (target.m_enemy.GetWorldPosition() - bot.GetWorldPosition()).X;
                PlayerCommandFaceDirection direction = (PlayerCommandFaceDirection)(x / Math.Abs(x));
                #endregion
                #region logic that kinda has to be here
                if (bot.CurrentMeleeWeapon.WeaponItem != WeaponItem.NONE && bot.CurrentWeaponDrawn != WeaponItemType.Melee)
                {
                    bot.ClearCommandQueue();
                    bot.AddCommand(new PlayerCommand(drawMelee));
                }
                #endregion
                if (target.GetDistance(bot.GetWorldPosition()) > 15 || FloorLvl(target.m_enemy) != FloorLvl(bot))
                {
                    if (FloorLvl(target.m_enemy) == FloorLvl(bot))
                    {
                        if (target.m_enemy.IsClimbing)
                        {
                            StayWithTartget(true, false);
                        }
                        else if (close2Obj == true)
                        {
                            StayWithTartget(false, true);
                        }
                        else
                        {
                            MoveTo(target.m_enemy.GetWorldPosition(), new Vector2(5, 0) * bot.FacingDirection);
                        }
                    }
                    else if (FloorLvl(bot) > FloorLvl(target.m_enemy))
                    {
                        GoToFloor(FloorLvl(target.m_enemy));
                    }
                    else if (FloorLvl(bot) < FloorLvl(target.m_enemy))
                    {
                        GoToFloor(FloorLvl(target.m_enemy));
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
                    if (winner == bot)
                    {
                        Game.CreateDialogue("Not Bad For A Mere Mortal", bot, "", 5000);
                        Game.SetGameOver(" THE BOT WON THE MATCH, BETTER LUCK NEXT TIME");
                    }
                    else
                    {
                        Game.SetGameOver(winner.Name + " WON THE MATCH, BETTER LUCK NEXT TIME");
                    }
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

                if (FindNearEnemy() && !bot.IsDead && bot != null)
                {
                    GoToEnemyAndAttack();
                }
                else if (!FindNearEnemy())
                {
                    GameOver();
                }
            }
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
