using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDGameScriptInterface;

namespace SFDFramework
{
    class Vanilla : GameScriptInterface
    {
        public Vanilla() : base(null) { }

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
            new VanillaPlugin();
        }

        public class VanillaPlugin
        {
            uint TIME_TO_SHOW_TEXT = 10000;
            Events.UpdateCallback m_updateEvent = null;
            float m_totalElapsed = 0f;
            string[] data = new string[4] { "0", "0", "0", "0" };
            bool[] teams = new bool[4];
            IObjectText textObject = Game.CreateObject("Text") as IObjectText;

            public VanillaPlugin(uint TimeToShowText = 10000)
            {
                TIME_TO_SHOW_TEXT = TimeToShowText;
                Game.SetMapType(MapType.Versus);
                textObject.SetTextScale(2);
                textObject.SetTextColor(Color.Blue);
                textObject.SetTextAlignment(TextAlignment.Middle);
                textObject.SetWorldPosition(Vector2.Zero + new Vector2(0, Game.GetCameraMaxArea().Top-100));
                if (Game.Data == string.Empty)
                {
                    foreach (IPlayer player in Game.GetPlayers())
                        if (!player.IsDead)
                            if (player.GetTeam() == PlayerTeam.Team1) teams[0] = true;
                            else if (player.GetTeam() == PlayerTeam.Team2) teams[1] = true;
                            else if (player.GetTeam() == PlayerTeam.Team3) teams[2] = true;
                            else if (player.GetTeam() == PlayerTeam.Team4) teams[3] = true;
                }
                else
                {
                    data = Game.Data.Split(':');
                    string text = string.Empty;
                    if (data[0] != "0") text += "Blue Team wons " + data[0] + " times\n";
                    if (data[1] != "0") text += "Red Team wons " + data[1] + " times\n";
                    if (data[2] != "0") text += "Green Team wons " + data[2] + " times\n";
                    if (data[3] != "0") text += "Yellow Team wons " + data[3] + " times";
                    textObject.SetText(text);
                }
                m_updateEvent = Events.UpdateCallback.Start(OnUpdate, 1000);
            }

            public void OnUpdate(float elapsed)
            {
                m_totalElapsed += elapsed;
                if (m_totalElapsed < TIME_TO_SHOW_TEXT && textObject != null)
                {
                    if (textObject.GetTextColor().Equals(Color.Blue))
                        textObject.SetTextColor(Color.Red);
                    else if (textObject.GetTextColor().Equals(Color.Red))
                        textObject.SetTextColor(Color.Green);
                    else if (textObject.GetTextColor().Equals(Color.Green))
                        textObject.SetTextColor(Color.Yellow);
                    else if (textObject.GetTextColor().Equals(Color.Yellow))
                        textObject.SetTextColor(Color.Blue);
                } else if (textObject != null) textObject.Remove();

                if (Game.IsGameOver)
                {
                    PlayerTeam winnerTeam = PlayerTeam.Independent; // every time the game is over this would be reset
                    bool flag = true;                               // so your second if statement in the bottom is useless
                    foreach (IPlayer player in Game.GetPlayers())
                    {
                        if (player.IsDead)
                        {
                            continue;
                        }

                        if (winnerTeam == PlayerTeam.Independent && player.GetTeam() != winnerTeam)
                        {
                            winnerTeam = player.GetTeam();
                            continue;
                        }
                        if (player.GetTeam() != winnerTeam)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        if (winnerTeam == PlayerTeam.Team1) data[0] = (Convert.ToUInt32(data[0]) + 1).ToString();
                        else if (winnerTeam == PlayerTeam.Team2) data[1] = (Convert.ToUInt32(data[1]) + 1).ToString();
                        else if (winnerTeam == PlayerTeam.Team3) data[2] = (Convert.ToUInt32(data[2]) + 1).ToString();
                        else if (winnerTeam == PlayerTeam.Team4) data[3] = (Convert.ToUInt32(data[3]) + 1).ToString();
                        Game.Data = string.Join(":", data);
                        m_updateEvent.Stop();
                        m_updateEvent = null;
                    }
                }
            }
        }
        #endregion
    }
}
