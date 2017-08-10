using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDConsoleApplication1
{
    class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        #region Script_2
        public void Btn_Press(TriggerArgs Btn)
        {
            //IObject playr = Game.GetSingleObjectByCustomID("Btn_1");

            //Vector2 playr_Pos = playr.GetWorldPosition();

            IPlayer caller = (IPlayer)Btn.Sender;

            IUser pusher = caller.GetUser();

            IPlayer playr = pusher.GetPlayer();

            Vector2 worldPos = playr.GetWorldPosition();

            int playr_dir = playr.FacingDirection;

            ProjectileItem Uzi = ProjectileItem.SNIPER;

            if (playr_dir == 1)
            {
                IObject rndPositive_Obj = Game.GetSingleObjectByCustomId("rndObj_pos");

                Vector2 positiveObj_pstn = rndPositive_Obj.GetWorldPosition();

                worldPos = worldPos + new Vector2(6, 4);

                for (int i = 1; i <= 10; i++)
                {
                    Game.SpawnProjectile(Uzi, worldPos, positiveObj_pstn);
                }

            }
            else if (playr_dir == -1)
            {
                IObject rndNegative_Obj = Game.GetSingleObjectByCustomId("rndObj_neg");

                Vector2 negativeObj_pstn = rndNegative_Obj.GetWorldPosition();

                worldPos = worldPos + new Vector2(-6, 4);

                for (int i = 1; i <= 10; i++)
                {
                    Game.SpawnProjectile(Uzi, worldPos, negativeObj_pstn);
                }

                // this code did not work as intended, but i think i fixed the accuracy problem, futher testing needed
                // additionally using the vector.normalize() method in the worldPos variable cause the code not to work as intended

            }
        }
        #endregion

    }
}