using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDConsoleApplication1
{
    class CargoScript : GameScriptInterface
    {
        public CargoScript() : base(null) { }

        #region Cargo
        List<Cargo> cargos = new List<Cargo>();
        float CameraOutPos;
        public void AfterStartup()
        {
            CameraOutPos = Game.GetCameraMaxArea().Height;
            foreach (IObject cargo in Game.GetObjectsByName("CargoContainer01B"))
            {
                cargos.Add(new Cargo(cargo));
            }
            Game.ShowPopupMessage(Convert.ToString(cargos.Count));
        }

        public class Cargo
        {
            public IObject cargoO;
            public IObject cargoC;
            public IObjectWeldJoint weld;
            IObjectAlterCollisionTile alter;
            IObjectAreaTrigger area;
            public List<IPlayer> players = new List<IPlayer>(8);
            Events.UpdateCallback m_updateEvent = null;

            public Cargo(IObject cargo)
            {
                cargoO = cargo;
                Vector2 pos = cargoO.GetWorldPosition();
                cargoC = Game.CreateObject("CargoContainer01A", pos);
                alter = Game.CreateObject("AlterCollisionTile", pos) as IObjectAlterCollisionTile;
                alter.AddTargetObject(cargoO); alter.AddTargetObject(cargoC);
                alter.SetDisableCollisionTargetObjects(true);
                alter.SetBodyType(BodyType.Dynamic);
                area = Game.CreateObject("AreaTrigger", pos + new Vector2(-40, 16)) as IObjectAreaTrigger;
                area.SetSizeFactor(new Point(11, 5));
                area.SetOnEnterMethod("OnEnter");
                area.SetOnLeaveMethod("OnLeave");
                area.SetBodyType(BodyType.Dynamic);
                weld = Game.CreateObject("WeldJoint", cargoO.GetWorldPosition()) as IObjectWeldJoint;
                weld.AddTargetObject(cargoO); weld.AddTargetObject(cargoC);
                weld.AddTargetObject(area); weld.AddTargetObject(alter);
                m_updateEvent = Events.UpdateCallback.Start(OnUpdate, 1000);
            }
            public bool Check(IObjectAreaTrigger area)
            {
                if (this.area == area) return true;
                return false;
            }
            private void OnUpdate(float elapsed)
            {
                /*Vector2 pos = area.GetWorldPosition() + new Vector2(-4, 4);
                Point size = area.GetSize();
                Area ar = new Area(pos, pos + new Vector2(size.X, -size.Y));
                foreach (IObject obj in Game.GetObjectsByArea(ar))
                {
                    if (obj is IPlayer) return;
                }
                players.Clear();
                Game.ShowPopupMessage(players.Count.ToString());
                foreach (IPlayer player in players)
                    if (player.GetUser() == null) players.Remove(player);*/
                if (players.Count == 0)
                {
                    weld.RemoveTargetObject(cargoC);
                    cargoC.SetWorldPosition(cargoO.GetWorldPosition());
                    weld.AddTargetObject(cargoC);
                }
            }
        }
        public void OnEnter(TriggerArgs args)
        {
            IObjectAreaTrigger area = args.Caller as IObjectAreaTrigger;
            Cargo cargo = null;
            if ((cargo = GetCargo(area)) == null) return; //failsafe
            IPlayer plr = args.Sender as IPlayer;
            if (plr != null && !cargo.players.Contains(plr))
            {
                if (plr.IsDead && cargo.players.Count == 0) return;
                cargo.players.Add(plr);
                cargo.weld.RemoveTargetObject(cargo.cargoC);
                cargo.cargoC.SetWorldPosition(cargo.cargoO.GetWorldPosition() + new Vector2(0, CameraOutPos));
                cargo.weld.AddTargetObject(cargo.cargoC);
            }
        }
        public void OnLeave(TriggerArgs args)
        {
            IObjectAreaTrigger area = args.Caller as IObjectAreaTrigger;
            Cargo cargo = null;
            if ((cargo = GetCargo(area)) == null) return; //failsafe
            IPlayer plr = args.Sender as IPlayer;
            if (plr != null && cargo.players.Contains(plr))
            {
                cargo.players.Remove(plr);
                if (cargo.players.Count == 0)
                {
                    cargo.weld.RemoveTargetObject(cargo.cargoC);
                    cargo.cargoC.SetWorldPosition(cargo.cargoO.GetWorldPosition());
                    cargo.weld.AddTargetObject(cargo.cargoC);
                }
            }
        }
        private Cargo GetCargo(IObjectAreaTrigger area)
        {
            foreach (Cargo cargo in cargos)
            {
                if (cargo.Check(area))
                {
                    return cargo;
                }
            }
            return null;
        }
        #endregion
    }
}