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

        #region DeathMatch
        Events.PlayerDeathCallback m_playerDeathEvent = null;
        Queue<IUser> queve = new Queue<IUser>();
        Random rnd = new Random();
        public void OnStartup()
        {
            m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
            Game.SetMapType(MapType.Custom);
            Game.DeathSequenceEnabled = false;
        }
        public void OnPlayerDeath(IPlayer player)
        {
            if (queve.Contains(player.GetUser()) || player.GetUser() == null) return;
            queve.Enqueue(player.GetUser());
            CreateTimer(3000);
        }
        public void RespawnPlayer(TriggerArgs args)
        {
            IUser plr = queve.Dequeue();
            IObject[] spawners = Game.GetObjectsByName("SpawnPlayer");
            Vector2 pos = Vector2.Zero;
            if (spawners.Length > 0) pos = spawners[rnd.Next(spawners.Length)].GetWorldPosition();
            IPlayer player = Game.CreatePlayer(pos);
            player.SetProfile(plr.GetProfile());
            player.SetUser(plr);
            ((IObject)args.Caller).Remove();
        }
        private void CreateTimer(int interval)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(1);
            timerTrigger.SetScriptMethod("RespawnPlayer");
            timerTrigger.Trigger();
        }
        #endregion
    }
}