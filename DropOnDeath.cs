
using XRL.World;
using XRL.World.Parts;
using DODTS;
using System.Linq;
using System;

namespace XRL.World.Parts
{

    [Serializable]
    public class DropDataOnDeath : IPart
    {
        public override bool WantEvent(int ID, int Cascade)
        {
            if (ID == DeathEvent.ID)
                return UI.Options.GetOptionBool("DODTS");
            return base.WantEvent(ID, Cascade);
        }

        public override bool HandleEvent(DeathEvent E)
        {
            if (E.Dying.CurrentCell != null)
            {
                GameObject obj = GameObject.Create("DataPacket");
                string tag = E.Dying.DisplayName + ", " + E.Dying.ID + ", " + E.Dying.Blueprint;
                obj.DisplayName = tag;
                obj.AddPart(new GameObjectDataRecord(E.Dying));
                E.Dying.CurrentCell.AddObject(obj);
            }
            return base.HandleEvent(E);
        }
    }
}


namespace XRL.Wish
{
    [HasWishCommand]
    public static class Wishes
    {

        [WishCommand("res")]

        public static void Res()
        {
            Cell cell = The.Player.PickDirection("res");
            if (cell != null)
            {
                var record = cell.Objects.Select(x => x.GetPart<GameObjectDataRecord>()).FirstOrDefault(x => x != null);
                if (record == null)
                {
                    IComponent<GameObject>.AddPlayerMessage("No object found with data record.");
                    return;
                }
                GameObject gameObj = record.Object;
                gameObj.MakeActive();
                cell.AddObject(gameObj);
                gameObj.RestorePristineHealth();
                IComponent<GameObject>.AddPlayerMessage($"{gameObj.t()} resurrected!");
            }
        }

        [WishCommand("readpack")]
        public static void ReadCopy()
        {
            Cell cell = The.Player.PickDirection("readpack");
            if (cell != null)
            {
                int records = 0;
                cell.Objects.ForEach(x => { if (x.TryGetPart(out GameObjectDataRecord record)) { records++; record.ReadData(); } });
                if (records > 0)
                    IComponent<GameObject>.AddPlayerMessage($"Read {records} records. See Player.log");
                else
                    IComponent<GameObject>.AddPlayerMessage("No objects found with GameObjectDataRecord part in cell.");
            }
        }

        [WishCommand("readany")]
        public static void read()
        {
            GameObject GO = The.Player;
            Cell cell = GO.PickDirection("readany");
            if (cell != null)
            {
                cell.Objects.ForEach(x => { GameObjectDataRecord record = new(x); record.ReadData(); });
                IComponent<GameObject>.AddPlayerMessage($"ReadComplete {cell.Objects.Count} objects. See Player.log");
            }
        }

        [WishCommand("read")]

        public static void read2()
        {
            GameObject GO = The.Player;
            Cell cell = GO.PickDirection("read");
            if (cell != null)
            {
                cell.Objects.Where(x => x.IsCombatObject()).ForEach(x => { GameObjectDataRecord record = new(x); record.ReadData(); });
                IComponent<GameObject>.AddPlayerMessage($"ReadComplete {cell.Objects.Count} objects. See Player.log");
            }
        }
    }
}