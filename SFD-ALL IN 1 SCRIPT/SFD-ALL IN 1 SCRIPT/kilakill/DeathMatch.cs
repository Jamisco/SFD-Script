using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDFramework.Plugins
{
    class MultiStart : GameScriptInterface
    {
        public MultiStart() : base(null) { }

        #region
        // SIGNATURE
        //            _____                            _____                            _____                    _____                  
        //           /\    \                          /\    \                          /\    \                  /\    \                 
        //          /::\____\                        /::\    \                        /::\____\                /::\    \                
        //         /:::/    /                        \:::\    \                      /:::/    /               /::::\    \               
        //        /:::/    /                          \:::\    \                    /:::/    /               /::::::\    \              
        //       /:::/    /                            \:::\    \                  /:::/    /               /:::/\:::\    \             
        //      /:::/____/                              \:::\    \                /:::/    /               /:::/__\:::\    \            
        //     /::::\    \                              /::::\    \              /:::/    /               /::::\   \:::\    \           
        //    /::::::\____\________            ____    /::::::\    \            /:::/    /               /::::::\   \:::\    \          
        //   /:::/\:::::::::::\    \          /\   \  /:::/\:::\    \          /:::/    /               /:::/\:::\   \:::\    \         
        //  /:::/  |:::::::::::\____\        /::\   \/:::/  \:::\____\        /:::/____/               /:::/  \:::\   \:::\____\        
        //  \::/   |::|~~~|~~~~~             \:::\  /:::/    \::/    /        \:::\    \               \::/    \:::\  /:::/    /        
        //   \/____|::|   |                   \:::\/:::/    / \/____/          \:::\    \               \/____/ \:::\/:::/    /         
        //         |::|   |                    \::::::/    /                    \:::\    \                       \::::::/    /          
        //         |::|   |                     \::::/____/                      \:::\    \                       \::::/    /           
        //         |::|   |                      \:::\    \                       \:::\    \                      /:::/    /            
        //         |::|   |                       \:::\    \                       \:::\    \                    /:::/    /             
        //         |::|   |                        \:::\    \                       \:::\    \                  /:::/    /              
        //         \::|   |                         \:::\____\                       \:::\____\                /:::/    /               
        //          \:|   |                          \::/    /                        \::/    /                \::/    /                
        //           \|___|                           \/____/                          \/____/                  \/____/                 
        //                                                                                                                              
        //            _____                            _____                            _____                    _____                  
        //           /\    \                          /\    \                          /\    \                  /\    \                 
        //          /::\____\                        /::\    \                        /::\____\                /::\____\                
        //         /:::/    /                        \:::\    \                      /:::/    /               /:::/    /                
        //        /:::/    /                          \:::\    \                    /:::/    /               /:::/    /                 
        //       /:::/    /                            \:::\    \                  /:::/    /               /:::/    /                  
        //      /:::/____/                              \:::\    \                /:::/    /               /:::/    /                   
        //     /::::\    \                              /::::\    \              /:::/    /               /:::/    /                    
        //    /::::::\____\________            ____    /::::::\    \            /:::/    /               /:::/    /                     
        //   /:::/\:::::::::::\    \          /\   \  /:::/\:::\    \          /:::/    /               /:::/    /                      
        //  /:::/  |:::::::::::\____\        /::\   \/:::/  \:::\____\        /:::/____/               /:::/____/                       
        //  \::/   |::|~~~|~~~~~             \:::\  /:::/    \::/    /        \:::\    \               \:::\    \                       
        //   \/____|::|   |                   \:::\/:::/    / \/____/          \:::\    \               \:::\    \                      
        //         |::|   |                    \::::::/    /                    \:::\    \               \:::\    \                     
        //         |::|   |                     \::::/____/                      \:::\    \               \:::\    \                    
        //         |::|   |                      \:::\    \                       \:::\    \               \:::\    \                   
        //         |::|   |                       \:::\    \                       \:::\    \               \:::\    \                  
        //         |::|   |                        \:::\    \                       \:::\    \               \:::\    \                 
        //         \::|   |                         \:::\____\                       \:::\____\               \:::\____\                
        //          \:|   |                          \::/    /                        \::/    /                \::/    /                
        //           \|___|                           \/____/                          \/____/                  \/____/                 
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 

        public void OnStartup()
        {
            new DeathMatchPlugin();
        }

        public class DeathMatchPlugin
        {
            uint TIME_TO_REVIVE;
            Events.PlayerDeathCallback m_playerDeathEvent = null;
            Events.UpdateCallback m_updateEvent = null;
            List<deadPlayer> deadPlayers = new List<deadPlayer>(8);
            Random rnd = new Random();
            public struct deadPlayer
            {
                public IUser user { get; set; }
                public PlayerTeam team { get; set; }
            }

            public DeathMatchPlugin(uint TimeToRevive = 5000)
            {
                TIME_TO_REVIVE = TimeToRevive;
                Game.SetMapType(MapType.Custom);
                Game.DeathSequenceEnabled = false;
                m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
                m_updateEvent = Events.UpdateCallback.Start(OnConnected, 1000);
            }

            public void OnConnected(float elapsed)
            {
                foreach (IUser user in Game.GetActiveUsers())
                {
                    if (user.GetPlayer() != null) continue;
                    if (deadPlayers.Find(x => x.user == user).user != null) continue;
                    deadPlayers.Add(new deadPlayer { user = user, team = PlayerTeam.Independent });
                    Events.UpdateCallback.Start(Revive, TIME_TO_REVIVE, 1);
                }
            }

            public void Revive(float elapsed)
            {
                deadPlayer player = deadPlayers[0];
                if (player.user.IsRemoved) return;
                if (player.user.GetPlayer() != null) player.user.GetPlayer().Remove();
                IObject[] respawns = Game.GetObjectsByName("SpawnPlayer");
                IPlayer revivedPlayer = Game.CreatePlayer(respawns[rnd.Next(respawns.Length)].GetWorldPosition());
                revivedPlayer.SetUser(player.user);
                revivedPlayer.SetProfile(player.user.GetProfile());
                revivedPlayer.SetTeam(player.team);
                deadPlayers.RemoveAt(0);
            }

            public void OnPlayerDeath(IPlayer player)
            {
                if (player.GetUser() == null || player.GetUser().IsRemoved) return;
                if (deadPlayers.Find(x => x.user == player.GetUser()).user != null) return;
                deadPlayers.Add(new deadPlayer { user = player.GetUser(), team = player.GetTeam() });
                Events.UpdateCallback.Start(Revive, TIME_TO_REVIVE, 1);
            }
        }
        #endregion
    }
}