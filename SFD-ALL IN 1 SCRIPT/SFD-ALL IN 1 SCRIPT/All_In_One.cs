using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDGameScriptInterface;

namespace SFD_ALL_IN_1_SCRIPT 
{
    class All_In_One : GameScriptInterface
    {
        public All_In_One() : base(null) { }
        #region All_In_One
        Events.PlayerDeathCallback m_playerDeathEvent = null;
        static Events.UpdateCallback m_updateEvent = null;
        static List<IObject> spawnPoints = new List<IObject>();
        static Queue<IUser> Death_Q = new Queue<IUser>();
        static Queue<IUser> Wait_Q = new Queue<IUser>();
        static IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
        static Random rnd = new Random();

        public void OnStartup()
        {
            m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
            Game.SetMapType(MapType.Custom);
            Game.DeathSequenceEnabled = false;
        }

        public void OnPlayerDeath(IPlayer player)
        {
            new Revive_Player(player.GetUser());
        }

        public static IObject Spawn_Points()
        {
            foreach (IObject spawn in Game.GetObjects("Spawn_Point")) //will be edited for team based gameplay
            {
                spawnPoints.Add(spawn);
            }
            return spawnPoints[rnd.Next(spawnPoints.Count)];
        }

        public void RespawnPlayer(TriggerArgs args)
        {
            //Revive_Player.Respawner();
            ((IObject)args.Caller).Remove();
        }

        public class Revive_Player
        {
            private static int wait_Timer = 1;
            public Revive_Player(IUser user)
            {
                foreach (IUser usr in Wait_Q)
                {
                    if (user == usr)
                    {
                        wait_Timer++;
                    }
                }
                Death_Q.Enqueue(user);
                //ReviveTimer(3000 * wait_Timer);
                m_updateEvent = Events.UpdateCallback.Start(Respawner, 6000 * (uint)wait_Timer);
            }

            public static void Respawner(float elapsed)
            {
                if (Death_Q.Count > 0)
                {
                    IUser plr = Death_Q.Dequeue();
                    Vector2 pos = Vector2.Zero;
                    if (spawnPoints.Count > 0)
                    {
                        pos = Spawn_Points().GetWorldPosition();
                    }
                    IPlayer player = Game.CreatePlayer(pos);
                    player.SetProfile(plr.GetProfile());
                    player.SetUser(plr);
                    Wait_Q.Enqueue(player.GetUser());
                }
            }

            private void WaitList(float elapsed)
            {
                if (Wait_Q.Count > 0)
                {
                    Wait_Q.Dequeue();
                    if (wait_Timer > 1)
                    {
                         wait_Timer--;
                    }
                }
            }
        }



        #endregion
    }
}
