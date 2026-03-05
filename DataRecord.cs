using XRL.World.Parts.Mutation;
using DODTS;
using XRL.World.Parts.Skill;
using System;
using System.Linq;
using System.Collections.Generic;

namespace XRL.World.Parts
{


    //plan: we will requirepart and mutation by string name and .Name
    // we will recreate them from the blueprint then require everything onto the new object
    //dismember limbs that shant be there etc
    //look into DeepCopy to see what we need to copy
    //but first TEST SERIALIZING with only the mutations list for now
    // or partslist thats easy
    //IDEA was to make the arrays not public so that they dont throw deserialize errors
    //we prob wont create an instance from a blueprint, well create an instance of their blueprint to match things like physics and  render and displayname stuff maybe idk. lots of parts to add lowkey.

    // final note: in scan, have it show object level specifically. additionally will need to see mutations w/ levels and skills and cybernetics etc for our copier. 
    // thank god there is a mutation lsit i can past that to one of my loggers. will need otherstuff like bodyparts too - anything listed in Copy. new wish "CheckCopy"

    //NEW ARRAY IDEA:
    //should we tag relations as well? opinions of the player? yes a list of Opinions to the player at least would be valid

    //OTHER ARRAY IDEA:
    //SKILLS string

    //best idea: look into DeepCopy and base it off that kinda...
    [Serializable]

    public class GameObjectDataRecord : IPart
    {
        public string DisplayName;
        public string Blueprint;
        public string ID;
        public int Level = 0;
        public int BaseHP = 0;
        public bool HadMutations = false;
        public bool IsAlive = false;
        public bool IsOrganic = false;
        public Dictionary<string, string> Properties = new();
        public Dictionary<string, int> IntProperties = new();
        public Dictionary<string, int> StatLevels = new();
        public Dictionary<string, int> MutationsWithLevels = new();
        public List<string> IParts = new();
        public List<string> Effects = new();
        public List<string> Skills = new();
        Dictionary<string, object> Collections => new()
        {
            [nameof(MutationsWithLevels)] = MutationsWithLevels,
            [nameof(IParts)] = IParts,
            [nameof(Properties)] = Properties,
            [nameof(IntProperties)] = IntProperties,
            [nameof(StatLevels)] = StatLevels,
            [nameof(Effects)] = Effects,
            [nameof(Skills)] = Skills,
        };

        public Dictionary<string, object> SimpleFields => new()
        {
            [nameof(Blueprint)] = Blueprint,
            [nameof(DisplayName)] = DisplayName,
            [nameof(ID)] = ID,
            [nameof(HadMutations)] = HadMutations,
            [nameof(Level)] = Level,
            [nameof(BaseHP)] = BaseHP,
            [nameof(IsAlive)] = IsAlive,
            [nameof(IsOrganic)] = IsOrganic
        };

        public GameObjectDataRecord()
        {

        }

        public GameObjectDataRecord(GameObject Object)
        {
            Record(Object);
        }
        static void CleanConsole()
        {
            for (int i = 0; i < 25; i++)
                MetricsManager.LogInfo("\n");
        }
        public void ReadData()
        {
            CleanConsole();
            MetricsManager.LogInfo($"\nREADING INFO ON {DisplayName}, {Blueprint}, {ID} START");
            Read(SimpleFields);
            MetricsManager.LogInfo("\n COLLECTIONS STARTED");
            ReadArrays(Collections);
            MetricsManager.LogInfo($"\nREADING INFO ON {DisplayName}, {Blueprint}, {ID} END");
        }
        void ReadArrays(Dictionary<string, object> arrays)
        {
            arrays.ForEach(x =>
            {
                MetricsManager.LogInfo($"\n {x.Key} START");
                CastArray(x.Value);
                MetricsManager.LogInfo($"{x.Key} END");
            });
        }
        void Record(GameObject Object)
        {
            RecordMutations(Object);
            RecordIParts(Object.PartsList);
            RecordSkills(Object);
            RecordStats(Object);
            RecordFX(Object);
            RecordStringProperties(Object);
            RecordIntProperties(Object);
            Blueprint = Object.Blueprint;
            Level = Object.Level;
            DisplayName = Object.DisplayName;
            BaseHP = Object.baseHitpoints;
            IsAlive = Object.IsAlive;
            IsOrganic = Object.IsOrganic;
            ID = Object.ID;
        }
        void RecordIntProperties(GameObject Object)
        {
            IntProperties = new(Object.IntProperty);
        }

        void RecordStringProperties(GameObject Object)
        {
            Properties = new(Object.Property);
        }

        void RecordFX(GameObject Object)
        {
            Effects = GetTypeNames(Object.Effects);
        }

        void RecordSkills(GameObject Object)
        {
            var skills = Object.GetPart<Skills>();
            if (skills != null)
                Skills = GetTypeNames(skills.SkillList);
        }

        void RecordMutations(GameObject Object)
        {
            Mutations m = Object.GetPart<Mutations>();
            if (m != null && m.MutationList.Count > 0)
            {
                m.MutationList.ForEach(x => MutationsWithLevels[x.Name] = x.Level);
                HadMutations = true;

            }
        }

        void RecordStats(GameObject Object)
        {
            if (Object.Statistics != null)
            {
                Object.Statistics.ForEach(x => StatLevels[x.Key] = x.Value.Value);
            }
        }

        void RecordIParts(PartRack parts)
        {
            parts.Where(x => x is not (BaseMutation or BaseSkill)).ForEach(x => IParts.Add(x.GetType().Name));
        }

        static List<string> GetTypeNames<T>(IList<T> list)
        {
            List<string> set = new(list.Count);
            list.ForEach(x => set.Add(x.GetType().Name));
            return set;
        }

        static void Read<T>(Dictionary<string, T> array)
        {
            array.ForEach(x => MetricsManager.LogInfo($"{x.Key}. {x.Value}"));
        }
        static void Read(List<string> array)
        {
            array.ForEach(x => MetricsManager.LogInfo(x));
        }

        static void CastArray(object obj)
        {
            switch (obj)
            {
                case Dictionary<string, int> stringIntArray:
                    Read(stringIntArray);
                    break;
                case Dictionary<string, string> stringStringArray:
                    Read(stringStringArray);
                    break;
                case List<string> stringArray:
                    Read(stringArray);
                    break;
            }
        }
    }
}