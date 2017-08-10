using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDConsoleApplication1
{
    class AI : GameScriptInterface
    {
        public AI() : base(null) { }
        #region Script_3: AI_The_Gunner
        static Random rnd = new Random();

        public void OnStartup()
        {
            Game.SetMapType(MapType.Custom);
            The_Gunner.CreateBots("The_Gunner");
        }

        public class The_Gunner
        {
            public IPlayer bot;
            public static List<Vector2> spawnLocation = new List<Vector2>();
            List<Enemy> enemies = new List<Enemy>();
            public Enemy target = null;
            public bool NearTarget;
            Vector2 safeDstnce = new Vector2(5, 0);
            Events.UpdateCallback attck_updateEvent = null;
            Events.UpdateCallback def_updateEvent = null;
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
                foreach (IObject spawn in Game.GetObjectsByCustomID("Spawn_Point")) // create spawn points and name them                                                       
                {                                                                   // Spawn_Point
                    spawnLocation.Add(spawn.GetWorldPosition());
                }

                foreach (IObjectPlayerSpawnTrigger The_Gunner in Game.GetObjectsByCustomID(ID))
                {
                    new The_Gunner(The_Gunner.CreatePlayer(spawnLocation[rnd.Next(spawnLocation.Count)]));
                    //spawns are random location
                }
            }

            public The_Gunner(IPlayer cpu)
            {
                bot = cpu;
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
                def_updateEvent = Events.UpdateCallback.Start(Fire_OnUpdate, 500);
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

            public void GoToEnemyAndAttack()
            {
                //Game.CreateDialogue("ingotoenemy", bot); // DEBUG
                //Game.ShowPopupMessage(target.GetDistance(bot.GetWorldPosition()).ToString());
                #region direction
                float x = (target.m_enemy.GetWorldPosition() - bot.GetWorldPosition()).X;
                PlayerCommandFaceDirection direction = (PlayerCommandFaceDirection)(x / Math.Abs(x));

                #endregion
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

                if (target.GetDistance(bot.GetWorldPosition()) > 20)
                {
                    bot.ClearCommandQueue();
                    bot.AddCommand(new PlayerCommand(move2Pos, target.m_enemy.GetWorldPosition() -
                    safeDstnce * (int)direction, direction));
                    bot.AddCommand(new PlayerCommand(sprint));

                    if (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring)
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

                    if (target.GetDistance(bot.GetWorldPosition()) == 40)
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(dive, target.m_enemy.GetWorldPosition()));
                    }
                    NearTarget = false;
                }
                else
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

                if (Game.GetPlayers().Length == 1)
                {
                    foreach (IPlayer plyr in Game.GetPlayers())
                    {
                        winner = plyr;
                    }
                    if (winner == bot && !bot.IsDead)
                    {
                        Game.CreateDialogue("Not Bad For A Mere Mortal", bot, "", 5000);
                        Game.SetGameOver(" THE BOT WON THE MATCH, BETTER LUCK NEXT TIME");
                    }
                    Game.SetGameOver(winner.Name + " WON THE MATCH, BETTER LUCK NEXT TIME");
                }
                else if (Game.GetPlayers().Length == 0)
                {
                    Game.SetGameOver("NO WINNER, BETTER LUCK NEXT TIME");
                }
            }

            private void attck_OnUpdate(float elapsed)
            {
                if (target != null && target.m_enemy.IsDead)
                {
                    Game.CreateDialogue("Target Eliminated!", bot);
                    enemies.Remove(target);
                    target = null;
                }

                if (bot == null)
                {
                    bot.Dispose();
                }

                if (Game.GetPlayers().Length == 0)
                {
                    GameOver();
                }

                if (bot.IsBurning)
                {
                    bot.ClearCommandQueue();
                    bot.AddCommand(new PlayerCommand(roll));
                }

                if (FindNearEnemy() && !bot.IsDead && bot != null)
                {
                    if (target.In_GFZ(bot.GetWorldPosition()))
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
                    else if (!target.In_GFZ(bot.GetWorldPosition()) && bot != null)
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
                else if (Game.GetPlayers().Length <= 1)
                {
                    GameOver();
                }
            }

            private void Fire_OnUpdate(float elapsed)
            {
                int num2 = rnd.Next(1, 4);
                int num = rnd.Next(1, 3);

                if (bot == null)
                {
                    bot.Dispose();
                }

                if (enemies.Count != 0 && !bot.IsDead && bot != null)
                {
                    foreach (Enemy enemy in enemies)
                    {
                        if (enemy != target)
                        {
                            IPlayer plyr = enemy.m_enemy;

                            if (plyr.IsManualAiming || plyr.IsHipFiring)
                            {
                                if (num == 1)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                                    bot.AddCommand(new PlayerCommand(ceaseFire));
                                    bot.AddCommand(new PlayerCommand(roll, rnd.Next(-1, 2), 200));
                                }
                                else if (num == 2)
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                                    bot.AddCommand(new PlayerCommand(ceaseFire));
                                    bot.AddCommand(new PlayerCommand(dive, rnd.Next(-1, 2), 200));
                                }
                            }
                        }
                    } // checks if other players are aiming at the bot
                }
                else if (Game.GetPlayers().Length <= 1)
                {
                    GameOver();
                }

                if (target != null && enemies.Count > 0 && !bot.IsDead)
                {
                    if (!target.In_GFZ(bot.GetWorldPosition()))
                    {

                        if (bot.CurrentWeaponDrawn == WeaponItemType.Rifle && !target.m_enemy.IsRolling &&
                            !target.m_enemy.IsDiving && !target.m_enemy.IsTakingCover || target.m_enemy.IsTakingCover &&
                            (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring) &&
                            bot.CurrentWeaponDrawn == WeaponItemType.Rifle)
                        {
                            if (bot.CurrentPrimaryWeapon.CurrentAmmo > 0)
                            {
                                if (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring)
                                {
                                    if (num2 == 1 || num2 == 2)
                                    {
                                        bot.ClearCommandQueue();
                                        bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
                                        bot.AddCommand(new PlayerCommand(fire));
                                    }
                                    else
                                    {
                                        if (num == 1)
                                        {
                                            bot.ClearCommandQueue();
                                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim, 0, 100));
                                            bot.AddCommand(new PlayerCommand(ceaseFire));
                                            if (!bot.IsDiving || !bot.IsRolling)
                                            {
                                                bot.AddCommand(new PlayerCommand(roll, rnd.Next(-1, 2), 500));

                                                // Game.CreateDialogue("roll", bot, "", 500); //DEBUG
                                            }
                                        }
                                        else if (num == 2)
                                        {

                                            bot.ClearCommandQueue();
                                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                                            bot.AddCommand(new PlayerCommand(ceaseFire));
                                            if (!bot.IsDiving || !bot.IsRolling)
                                            {
                                                bot.AddCommand(new PlayerCommand(dive, rnd.Next(-1, 2), 1000));
                                                //Game.CreateDialogue("dive", bot, "", 1000); //DEBUG
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    bot.ClearCommandQueue();
                                    bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
                                    bot.AddCommand(new PlayerCommand(fire));
                                }
                            }
                        }
                        else
                        if (bot.CurrentWeaponDrawn == WeaponItemType.Handgun && !target.m_enemy.IsRolling &&
                           !target.m_enemy.IsDiving && !target.m_enemy.IsTakingCover || target.m_enemy.IsTakingCover &&
                           (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring) &&
                            bot.CurrentWeaponDrawn == WeaponItemType.Handgun)
                        {
                            if (bot.CurrentSecondaryWeapon.CurrentAmmo > 0)
                            {
                                if (target.m_enemy.IsManualAiming || target.m_enemy.IsHipFiring)
                                {
                                    if (num2 == 1 || num2 == 2)
                                    {
                                        bot.ClearCommandQueue();
                                        //   Game.CreateDialogue("inFire", target.m_enemy, "", 500);
                                        bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
                                        bot.AddCommand(new PlayerCommand(fire));
                                    }
                                    else
                                    {
                                        if (num == 1)
                                        {
                                            bot.ClearCommandQueue();
                                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim, 0, 100));
                                            bot.AddCommand(new PlayerCommand(ceaseFire));
                                            if (!bot.IsDiving || !bot.IsRolling)
                                            {
                                                bot.AddCommand(new PlayerCommand(roll, rnd.Next(-1, 2), 500));

                                                //Game.CreateDialogue("roll", bot, "", 500); //DEBUG
                                            }
                                        }
                                        else if (num == 2)
                                        {
                                            bot.ClearCommandQueue();
                                            bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                                            bot.AddCommand(new PlayerCommand(ceaseFire));
                                            if (!bot.IsDiving || !bot.IsRolling)
                                            {
                                                bot.AddCommand(new PlayerCommand(dive, rnd.Next(-1, 2), 1000));
                                                //  Game.CreateDialogue("dive", bot, "", 1000); //DEBUG
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    bot.ClearCommandQueue();
                                    //  Game.CreateDialogue("inFire", target.m_enemy, "", 500);
                                    bot.AddCommand(new PlayerCommand(aim, target.m_enemy.UniqueID));
                                    bot.AddCommand(new PlayerCommand(fire));
                                }
                            }
                        }
                        else
                        {
                            if (bot.IsManualAiming)
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(ceaseFire));
                            }
                            else
                            {
                                bot.ClearCommandQueue();
                                bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                                bot.AddCommand(new PlayerCommand(ceaseFire));
                            }
                        }
                    }
                    else
                    {
                        bot.ClearCommandQueue();
                        bot.AddCommand(new PlayerCommand(PlayerCommandType.StopAim));
                        bot.AddCommand(new PlayerCommand(ceaseFire));
                        bot.AddCommand(new PlayerCommand(sheath));
                    }
                }
                else if (bot.IsDead && Game.GetPlayers().Length <= 1)
                {
                    GameOver();
                }

            }
        }


        public class Enemy
        {
            public IPlayer m_enemy;

            public Enemy(IPlayer enemy)
            {
                m_enemy = enemy;
            }

            public float GetDistance(Vector2 BotPosition)
            {
                return (m_enemy.GetWorldPosition() - BotPosition).Length();
            }

            public bool In_GFZ(Vector2 BotPos) //GFZ means Gun Free Zone ... so this is the distance where bot should not use gun
            {
                float distance = GetDistance(BotPos);

                if (distance <= 80) // this is solely for shooting and not melee attaking.
                {
                    //Game.ShowPopupMessage("true");
                    return true;
                }
                else
                {
                    // Game.ShowPopupMessage("false");
                    return false;
                }
            }
        }
        #endregion //
    }
}
