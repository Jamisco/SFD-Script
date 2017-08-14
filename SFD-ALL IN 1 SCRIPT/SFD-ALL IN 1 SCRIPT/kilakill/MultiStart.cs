using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDFramework
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
        enum Gamemode { undefined = -1, DeathMatch, Duels, Vanilla };
        Gamemode gamemode = Gamemode.undefined;
        IObjectTrigger button;
        IDialogue lastDialogue;
        IPlayer host;
        public void OnStartup()
        {
            Game.SetCurrentCameraMode(CameraMode.Dynamic);
            if (Game.Data != string.Empty)
            {
                switch (Game.Data.Split(';')[0])
                {
                    case "DeathMatch":
                        new DeathMatchPlugin(5000);
                        break;
                    case "Duels":
                        new DuelsPlugin(3000);
                        break;
                    case "Vanilla":
                        new VanillaPlugin();
                        break;
                }
            }
            else
            {
                host = Game.GetPlayers()[0];
                lastDialogue = Game.CreateDialogue("Select game mode:", host, "", 0);
                button = Game.CreateObject("Button00", host.GetWorldPosition() + new Vector2(-10, 10)) as IObjectTrigger;
                button.CustomID = "Change";
                button.SetScriptMethod("PressButton");
                (Game.CreateObject("Button00", host.GetWorldPosition() + new Vector2(10, 10)) as IObjectButtonTrigger).SetScriptMethod("PressButton");
            }
        }
        public void PressButton(TriggerArgs args)
        {
            if (((IObject)args.Caller).CustomID == "Change") {
                if ((int)gamemode == 2) gamemode -= 3;
                lastDialogue.Close();
                lastDialogue = Game.CreateDialogue((++gamemode).ToString(), host, "", 0);
            } else {
                Game.Data = gamemode + ";";
                switch (gamemode)
                {
                    case Gamemode.DeathMatch:
                        new DeathMatchPlugin(5000);
                        break;
                    case Gamemode.Duels:
                        new DuelsPlugin(3000);
                        break;
                    case Gamemode.Vanilla:
                        new VanillaPlugin();
                        break;
                    default: return;
                }
                lastDialogue.Close();
                button.Remove();
                ((IObject)args.Caller).Remove();
            }
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
                Vector2 revPos = Vector2.Zero;
                if (respawns.Length > 0) revPos = respawns[rnd.Next(respawns.Length)].GetWorldPosition();
                IPlayer revivedPlayer = Game.CreatePlayer(revPos);
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
        public class DuelsPlugin
        {
            uint TIME_TO_NEXT_VERSUS;
            Events.PlayerDeathCallback m_deathEvent = null;
            IObjectText textObject = Game.CreateObject("Text") as IObjectText;
            List<IUser> users = new List<IUser>(8);
            IPlayer[] duelers = new IPlayer[2];
            Random rnd = new Random();

            public DuelsPlugin(uint TimeToNextVersus = 5000)
            {
                TIME_TO_NEXT_VERSUS = TimeToNextVersus;
                Game.SetMapType(MapType.Custom);
                Game.SetAllowedCameraModes(CameraMode.Static & CameraMode.Dynamic);
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

            public void StartDuel(float time = 0)
            {
                if (users.Count > 1)
                {
                    if (duelers[0] != null) duelers[0].Remove();
                    if (duelers[1] != null) duelers[1].Remove();
                    Game.SetCurrentCameraMode(CameraMode.Static);
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
                    Events.UpdateCallback.Start(elapsed => { textObject.SetText(""); Game.SetCurrentCameraMode(CameraMode.Dynamic); }, 5000, 1);
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
                        Game.SetCurrentCameraMode(CameraMode.Static);
                        users.Remove(player.GetUser());
                        textObject.SetTextColor(Color.Red);
                        if (!duelers[0].IsDead)
                            textObject.SetText(players[0].Name + " won!");
                        else if (!duelers[1].IsDead)
                            textObject.SetText(players[1].Name + " won!");
                    }
                    Events.UpdateCallback.Start(StartDuel, TIME_TO_NEXT_VERSUS, 1);
                }
            }
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
                Game.SetAllowedCameraModes(CameraMode.Static & CameraMode.Dynamic);
                textObject.SetTextScale(2);
                textObject.SetTextColor(Color.Blue);
                textObject.SetTextAlignment(TextAlignment.Middle);
                textObject.SetWorldPosition(Vector2.Zero + new Vector2(0, Game.GetCameraMaxArea().Top - 100));
                if (Game.Data == Gamemode.Vanilla + ";")
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
                    data = Game.Data.Split(';')[1].Split(':');
                    string text = string.Empty;
                    if (data[0] != "0") text += "Blue Team wons " + data[0] + " times\n";
                    if (data[1] != "0") text += "Red Team wons " + data[1] + " times\n";
                    if (data[2] != "0") text += "Green Team wons " + data[2] + " times\n";
                    if (data[3] != "0") text += "Yellow Team wons " + data[3] + " times";
                    textObject.SetText(text);
                    if (text != string.Empty)
                        Game.SetCurrentCameraMode(CameraMode.Static);
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
                }
                else if (textObject != null) { textObject.Remove(); Game.SetCurrentCameraMode(CameraMode.Dynamic); }

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
                        Game.Data = Gamemode.Vanilla + ";" + string.Join(":", data);
                        m_updateEvent.Stop();
                        m_updateEvent = null;
                    }
                }
            }
        }
        #endregion
    }
}