using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDGameScriptInterface;

namespace SFD_ALL_IN_1_SCRIPT.Jamisco
{
    class Duels : GameScriptInterface
    {
        public Duels() : base(null) { }

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
            new DuelsPlugin();
        }

        public class DuelsPlugin
        {
            Events.PlayerDeathCallback m_deathEvent = null;
            //Events.UpdateCallback m_updateEvent = null;
            IObjectText textObject = Game.CreateObject("Text") as IObjectText;
            List<IUser> users = new List<IUser>(8);
            IPlayer[] duelers = new IPlayer[2];
            Random rnd = new Random();

            public DuelsPlugin()
            {
                Game.SetMapType(MapType.Custom);
                textObject.SetTextScale(2);
                textObject.SetTextColor(Color.Magenta);
                textObject.SetTextAlignment(TextAlignment.Middle);
                textObject.SetWorldPosition(Vector2.Zero + new Vector2(0, Game.GetCameraMaxArea().Top - 100));
                foreach (IUser user in Game.GetActiveUsers())
                    users.Add(user);
                foreach (IPlayer player in Game.GetPlayers())
                    player.Remove();
                StartDuel();
            }

            public void StartDuel()
            {
                if (users.Count > 1)
                {
                    IObject[] spawners = Game.GetObjectsByName("SpawnPlayer");
                    int one = rnd.Next(spawners.Length), two = rnd.Next(spawners.Length);
                    if (spawners.Length > 1)
                        while (one == two) two = rnd.Next(spawners.Length);
                    IPlayer newPlayer = Game.CreatePlayer(spawners[one].GetWorldPosition());
                    newPlayer.SetUser(users[0]); newPlayer.SetProfile(users[0].GetProfile());
                    duelers[0] = newPlayer;
                    newPlayer = Game.CreatePlayer(spawners[two].GetWorldPosition());
                    newPlayer.SetUser(users[1]); newPlayer.SetProfile(users[1].GetProfile());
                    duelers[1] = newPlayer;
                    textObject.SetTextColor(Color.Magenta);
                    textObject.SetText(users[0].GetProfile().Name + " vs " + users[1].GetProfile().Name);
                    Events.UpdateCallback.Start(elapsed => textObject.SetText(""), 5000, 1);
                    if (m_deathEvent == null)
                        m_deathEvent = Events.PlayerDeathCallback.Start(OnDeath);
                }
                else if (users.Count == 1)
                {
                    Game.SetGameOver(users[0].Name + " the best fighter!");
                }
            }

            public void OnDeath(IPlayer player)
            {
                if (player == duelers[0] || player == duelers[1])
                {
                    m_deathEvent.Stop();
                    m_deathEvent = null;
                    IUser user = player.GetUser();
                    IPlayer[] players = Game.GetPlayers();
                    if (players.Length == 2)
                    {
                        users.Remove(player.GetUser());
                        textObject.SetTextColor(Color.Red);
                        if (!duelers[0].IsDead)
                            textObject.SetText(players[0].Name + " won!");
                        else if (!duelers[1].IsDead)
                            textObject.SetText(players[1].Name + " won!");
                        duelers[0].Remove();
                        duelers[1].Remove();
                    }
                    StartDuel();
                }
            }
        }
        #endregion
    }
}
