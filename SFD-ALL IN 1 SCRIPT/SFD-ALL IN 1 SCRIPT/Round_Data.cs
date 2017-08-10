using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDGameScriptInterface;

namespace SFD_ALL_IN_1_SCRIPT
{
    class Round_Data : GameScriptInterface
    {
        public Round_Data() : base(null) { }

        #region Round Data
        // SIGNATURE
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

        static string round_data;
        static  List<PlayerTeam> Team = new List<PlayerTeam>();
        static List<int> roundsWon = new List<int>();
        IObjectTimerTrigger timerTrigger;
        Events.PlayerDeathCallback m_playerDeathEvent = null;

        public void OnStartup()
        {
            Game.SetMapType(MapType.Custom);
            timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            IObjectText round_text_msg = (IObjectText)Game.CreateObject("text");
            m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
        }

        public void OnPlayerDeath(IPlayer PLYR)
        {
            GameOver();
        }

        public void GameOver()
        {
            IPlayer winner = null;      
            if (Game.GetPlayers().Length == 1)
            {
                foreach (IPlayer plyr in Game.GetPlayers())
                {
                    winner = plyr;
                    if (!Team.Contains(plyr.GetTeam()))
                    {
                        Team.Add(winner.GetTeam());
                        roundsWon.Add(0);
                    }
                    roundsWon.Insert(Team.IndexOf(winner.GetTeam()), roundsWon.ElementAt(Team.IndexOf(winner.GetTeam())));
                }
                Game.SetGameOver(winner.GetTeam().ToString() + " Won The Game" + " :" 
                            + roundsWon[Team.IndexOf(winner.GetTeam())].ToString() + " Times"); 
            }
        }

        #endregion
    }
}
