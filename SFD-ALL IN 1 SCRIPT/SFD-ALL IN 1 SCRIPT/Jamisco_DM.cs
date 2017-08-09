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

        #region
        Events.PlayerDeathCallback OnDeath = null;
        static IPlayer[] initPlayers;
        IProfile prfle = new IProfile();
        static IPlayer plyer;
        Vector2 spawnPos = new Vector2();
        MapType map = MapType.Custom;

        public void OnStartUp()
        {
            initPlayers = new IPlayer[Game.GetPlayers().Length];

            OnDeath = Events.PlayerDeathCallback.Start(OnPlayerDeath);
            
        }

        public void Revive()
        {
            foreach (IPlayer deadPlyr in Game.GetPlayers())
            {
                if (deadPlyr == null)
                {
                    prfle = deadPlyr.GetProfile();
                }
            }

            foreach(IPlayer plyrs in Game.GetPlayers())
            {
                if (plyrs.GetTeam() == plyer.GetTeam())
                {
                    spawnPos = plyrs.GetWorldPosition() + new Vector2(5, 5);
                }
            }

            IPlayer plyr = Game.CreatePlayer(spawnPos);
            plyer.SetProfile(prfle);
            plyr.SetUser(plyr.GetUser());
            
        }

        public void OnPlayerDeath(IPlayer plyer)
        {
            ReviveTimer();
        }

        public void ReviveTimer()
        {
            IObjectTimerTrigger timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timer.SetIntervalTime(5000);
            timer.SetScriptMethod("Revive");
            timer.Trigger();
        }
        #endregion
    }
}
