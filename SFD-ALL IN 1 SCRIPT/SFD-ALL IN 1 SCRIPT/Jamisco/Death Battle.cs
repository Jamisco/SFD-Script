using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDGameScriptInterface;

namespace SFD_ALL_IN_1_SCRIPT.Jamisco
{
    class Death_Battle : GameScriptInterface
    {
        public Death_Battle() : base(null) { }

        #region DEATH BATTLE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

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

        public void OnStartup()
        {

            test();
        }

        public void test()
        {
            List<IPlayer> plyr = new List<IPlayer>();
            foreach (IPlayer plyrs in Game.GetPlayers())
            {
                plyr.Add(plyrs);
            }

            new DeathBattle(plyr[0], plyr[1]);
        }

        public class DeathBattle
        {

            Queue<IUser> Wait_Q = new Queue<IUser>();
            List<IPlayer> Contestants = new List<IPlayer>(2);
            List<Vector2> DuelPos = new List<Vector2> { new Vector2(-50, 0), new Vector2(50, 0) };
            IObjectText round_text_msg = (IObjectText)Game.CreateObject("text");
            Events.UpdateCallback update = null;

            public DeathBattle(IPlayer plyr1, IPlayer plyr2)
            {
                Game.ShowPopupMessage("hi");
                Contestants.Add(plyr1);
                Contestants.Add(plyr2);
                Ready_Contestants();
            }

            public void Ready_Contestants()
            {
                foreach (IPlayer plyr in Contestants)
                {
                    plyr.SetWorldPosition(DuelPos.First());
                    DuelPos.Remove(DuelPos.First());
                    plyr.SetInputEnabled(false);
                }
                round_text_msg.SetTextScale(3);
                round_text_msg.SetTextAlignment(TextAlignment.Middle);
                round_text_msg.SetTextColor(Color.Green);
                round_text_msg.SetWorldPosition(new Vector2(0, 50));
                round_text_msg.SetText("Fighters, Geeeeettt, Readyyyyy!!!!");
                update = Events.UpdateCallback.Start(Fight_Message, 5000, 1);
            }

            public void Fight_Message(float elapsed)
            {
                round_text_msg.SetTextScale(4);
                round_text_msg.SetTextAlignment(TextAlignment.Middle);
                round_text_msg.SetTextColor(Color.Yellow);
                round_text_msg.SetWorldPosition(new Vector2(0, 50));
                round_text_msg.SetText("May the Best Fighter Win");
                update = Events.UpdateCallback.Start(Fight, 5000, 1);
            }

            public void Fight(float elapsed)
            {
                round_text_msg.SetTextScale(5);
                round_text_msg.SetTextColor(Color.Red);
                round_text_msg.SetText("FIGHT");
                update = Events.UpdateCallback.Start(Begin_Fight, 500, 1);
            }
            public void Begin_Fight(float elapsed)
            {
                round_text_msg.Destroy();
                foreach(IPlayer plyr in Game.GetPlayers())
                {
                    plyr.SetInputEnabled(true);
                }
            }
        }



        #endregion

    }
}
