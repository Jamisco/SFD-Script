using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDGameScriptInterface;

namespace SFDConsoleApplication1
{
    class Jamisco_DM : GameScriptInterface
    {
        public Jamisco_DM() : base(null) { }

        #region All_In_One
//            _____                            _____                            _____                            _____                  
//           /\    \                          /\    \                          /\    \                          /\    \                 
//          /::\    \                        /::\    \                        /::\____\                        /::\    \                
//          \:::\    \                      /::::\    \                      /::::|   |                        \:::\    \               
//           \:::\    \                    /::::::\    \                    /:::::|   |                         \:::\    \              
//            \:::\    \                  /:::/\:::\    \                  /::::::|   |                          \:::\    \             
//             \:::\    \                /:::/__\:::\    \                /:::/|::|   |                           \:::\    \            
//             /::::\    \              /::::\   \:::\    \              /:::/ |::|   |                           /::::\    \           
//    _____   /::::::\    \            /::::::\   \:::\    \            /:::/  |::|___|______            ____    /::::::\    \          
//   /\    \ /:::/\:::\    \          /:::/\:::\   \:::\    \          /:::/   |::::::::\    \          /\   \  /:::/\:::\    \         
//  /::\    /:::/  \:::\____\        /:::/  \:::\   \:::\____\        /:::/    |:::::::::\____\        /::\   \/:::/  \:::\____\        
//  \:::\  /:::/    \::/    /        \::/    \:::\  /:::/    /        \::/    / ~~~~~/:::/    /        \:::\  /:::/    \::/    /        
//   \:::\/:::/    / \/____/          \/____/ \:::\/:::/    /          \/____/      /:::/    /          \:::\/:::/    / \/____/         
//    \::::::/    /                            \::::::/    /                       /:::/    /            \::::::/    /                  
//     \::::/    /                              \::::/    /                       /:::/    /              \::::/____/                   
//      \::/    /                               /:::/    /                       /:::/    /                \:::\    \                   
//       \/____/                               /:::/    /                       /:::/    /                  \:::\    \                  
//                                            /:::/    /                       /:::/    /                    \:::\    \                 
//                                           /:::/    /                       /:::/    /                      \:::\____\                
//                                           \::/    /                        \::/    /                        \::/    /                
//                                            \/____/                          \/____/                          \/____/                 
//                                                                                                                                      
//                            _____                            _____                           _______                                  
//                           /\    \                          /\    \                         /::\    \                                 
//                          /::\    \                        /::\    \                       /::::\    \                                
//                         /::::\    \                      /::::\    \                     /::::::\    \                               
//                        /::::::\    \                    /::::::\    \                   /::::::::\    \                              
//                       /:::/\:::\    \                  /:::/\:::\    \                 /:::/~~\:::\    \                             
//                      /:::/__\:::\    \                /:::/  \:::\    \               /:::/    \:::\    \                            
//                      \:::\   \:::\    \              /:::/    \:::\    \             /:::/    / \:::\    \                           
//                    ___\:::\   \:::\    \            /:::/    / \:::\    \           /:::/____/   \:::\____\                          
//                   /\   \:::\   \:::\    \          /:::/    /   \:::\    \         |:::|    |     |:::|    |                         
//                  /::\   \:::\   \:::\____\        /:::/____/     \:::\____\        |:::|____|     |:::|    |                         
//                  \:::\   \:::\   \::/    /        \:::\    \      \::/    /         \:::\    \   /:::/    /                          
//                   \:::\   \:::\   \/____/          \:::\    \      \/____/           \:::\    \ /:::/    /                           
//                    \:::\   \:::\    \               \:::\    \                        \:::\    /:::/    /                            
//                     \:::\   \:::\____\               \:::\    \                        \:::\__/:::/    /                             
//                      \:::\  /:::/    /                \:::\    \                        \::::::::/    /                              
//                       \:::\/:::/    /                  \:::\    \                        \::::::/    /                               
//                        \::::::/    /                    \:::\    \                        \::::/    /                                
//                         \::::/    /                      \:::\____\                        \::/____/                                 
//                          \::/    /                        \::/    /                         ~~                                       
//                           \/____/                          \/____/                                                                   
//                                                                                                                                                                                                                                                                           

        Events.PlayerDeathCallback m_playerDeathEvent = null;
        static Events.UpdateCallback waitList_update = null;
        static List<IObject> spawnPoints = new List<IObject>();
        static Queue<IUser> Death_Q = new Queue<IUser>();
        static Queue<IUser> Wait_Q = new Queue<IUser>();
        static IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
        static Random rnd = new Random();

        public void OnStartup()
        {
            waitList_update = Events.UpdateCallback.Start(DQ_WaitList, 5000);
            m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
            Game.SetMapType(MapType.Custom);
            Game.DeathSequenceEnabled = false;
        }

        public void OnPlayerDeath(IPlayer player)
        {
            if (player != null && player.GetUser() != null)
            {
                new Revive_Player(player.GetUser());
            }
            else if (player == null || player.GetUser() == null)
            {
                return;
            }
        }

        public static IObject Spawn_Points()
        {
            foreach (IObject spawn in Game.GetObjects("Spawn_Point")) //will be edited for team based gameplay
            {
                spawnPoints.Add(spawn);
            }
            return spawnPoints[rnd.Next(spawnPoints.Count)];
        }

        public void Spawner(TriggerArgs args)
        {
            Revive_Player.Respawner();
        }

        public void DQ_WaitList(float elapsed)
        {
            if (Wait_Q.Count > 0)
            {
                Wait_Q.Dequeue();
            }
        }

        public class Revive_Player
        {
            int wait_Timer = 1;
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
                CreateTimer(2000 * wait_Timer);
            }

            public static void Respawner()
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

            private void CreateTimer(int interval)
            {
                timerTrigger.SetIntervalTime(interval);
                timerTrigger.SetRepeatCount(1);
                timerTrigger.SetScriptMethod("Spawner");
                timerTrigger.Trigger();
            }
        }
        #endregion
    }
}
