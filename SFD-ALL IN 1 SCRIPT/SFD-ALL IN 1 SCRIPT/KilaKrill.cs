using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDConsoleApplication1
{
    class Krill
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>

        #region Script
        public void AfterStartup()
        {
            HelloWorld();
        }

        void HelloWorld()
        {
            Game.ShowPopupMessage("Hello World!");
        }

        public void OnStartup()
        {
            Game.RunCommand("/MSG  ALT+Block ");
            Shoot(200, 0, "Shoot", "");
        }

        public void Shoot(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.IsBlocking && ply.IsWalking)
                {
                    Vector2 pos = ply.GetWorldPosition();
                    int dir = ply.FacingDirection;
                    for (int i = 1; i >= 1; i--)
                    {
                        Game.SpawnProjectile(ProjectileItem.PISTOL, pos + new Vector2(6f * dir, 9f), new Vector2(100f * dir, i));
                    }
                }
            }
        }

        private void Shoot(int interval, int count, string method, string id)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
        }
        #endregion

    }
}