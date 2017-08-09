using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDConsoleApplication1
{
    class DeathMatch : GameScriptInterface
    {
        public DeathMatch() : base(null) { }

        #region
        public void OnStartup()
        {
            new DeathMatchPlugin();
        }

        public class DeathMatchPlugin
        {
            const int TIME_TO_REVIVE = 5000;
            const MapType MAP_TYPE = MapType.Custom;

            Events.PlayerDeathCallback OnDeath = null;
            Events.UpdateCallback m_updateEvent = null;
            float m_totalElapsed = 0f;

            List<deadPlayer> deadPlayers = new List<deadPlayer>(8);
            Random rnd = new Random();
            public struct deadPlayer
            {
                public IUser user { get; set; }
                public PlayerTeam team { get; set; }
            }

            public DeathMatchPlugin()
            {
                Game.SetMapType(MAP_TYPE);
                Game.DeathSequenceEnabled = false;
                OnDeath = Events.PlayerDeathCallback.Start(OnPlayerDeath);
            }

            public void Revive(float elapsed)
            {
                deadPlayer player = deadPlayers[0]; deadPlayers.RemoveAt(0);
                if (player.user == null) return;
                IObject[] respawns = Game.GetObjectsByName("SpawnPlayer");
                IPlayer revivedPlayer = Game.CreatePlayer(respawns[rnd.Next(respawns.Length)].GetWorldPosition());
                revivedPlayer.SetUser(player.user);
                revivedPlayer.SetProfile(player.user.GetProfile());
                revivedPlayer.SetTeam(player.team);
            }

            public void OnPlayerDeath(IPlayer plyer)
            {
                if (plyer.GetUser() == null) return;
                if (deadPlayers.Find(x => x.user == plyer.GetUser()).user != null) return;
                deadPlayers.Add(new deadPlayer { user = plyer.GetUser(), team = plyer.GetTeam() });
                m_updateEvent = Events.UpdateCallback.Start(Revive, TIME_TO_REVIVE, 1);
            }
        }
        #endregion
    }
}