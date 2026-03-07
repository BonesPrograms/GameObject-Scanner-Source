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
        public Dictionary<string, object> Collections => new()
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

        public GameObjectDataRecord(GameObject gameObject)
        {
            Record(gameObject);
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
        void ReadArrays(Dictionary<string, object> collections)
        {
            collections.ForEach(x =>
            {
                MetricsManager.LogInfo($"\n {x.Key} START");
                CastCollection(x.Value);
                MetricsManager.LogInfo($"{x.Key} END");
            });
        }
        void Record(GameObject gameObj)
        {
            RecordMutations(gameObj);
            RecordIParts(gameObj.PartsList);
            RecordSkills(gameObj);
            RecordStats(gameObj);
            RecordFX(gameObj);
            RecordStringProperties(gameObj);
            RecordIntProperties(gameObj);
            Blueprint = gameObj.Blueprint;
            Level = gameObj.Level;
            DisplayName = gameObj.DisplayName;
            BaseHP = gameObj.baseHitpoints;
            IsAlive = gameObj.IsAlive;
            IsOrganic = gameObj.IsOrganic;
            ID = gameObj.ID;
        }
        void RecordIntProperties(GameObject gameObj)
        {
            IntProperties = new(gameObj.IntProperty);
        }

        void RecordStringProperties(GameObject GameObj)
        {
            Properties = new(GameObj.Property);
        }

        void RecordFX(GameObject gameObj)
        {
            Effects = GetTypeNames(gameObj.Effects);
        }

        void RecordSkills(GameObject gameObj)
        {
            var skills = gameObj.GetPart<Skills>();
            if (skills?.SkillList != null)
                Skills = GetTypeNames(skills.SkillList);
        }

        void RecordMutations(GameObject gameObj)
        {
            Mutations m = gameObj.GetPart<Mutations>();
            if (m?.MutationList?.Count > 0)
            {
                MutationsWithLevels = m.MutationList.ToDictionary(x => x.Name, x => x.Level);
                HadMutations = true;
            }
        }

        void RecordStats(GameObject gameObj)
        {
            if (gameObj.Statistics != null)
                StatLevels = gameObj.Statistics.ToDictionary(x => x.Key, x => x.Value.Value);
        }

        void RecordIParts(PartRack parts)
        {
            IParts = GetTypeNames(parts, x => x is not (BaseMutation or BaseSkill));
        }

        static List<string> GetTypeNames<T>(IEnumerable<T> collection, Func<T, bool> expr = null)
        {
            static string GetNameOfT(T t) => t.GetType().Name;
            return expr == null ? collection.Select(GetNameOfT).ToList() : collection.Where(expr).Select(GetNameOfT).ToList();
        }

        static void Read<T>(Dictionary<string, T> dic)
        {
            dic.ForEach(x => MetricsManager.LogInfo($"{x.Key}. {x.Value}"));
        }
        static void Read(List<string> list)
        {
            list.ForEach(x => MetricsManager.LogInfo(x));
        }

        static void CastCollection(object collection)
        {
            switch (collection)
            {
                case Dictionary<string, int> stringIntDictionary:
                    Read(stringIntDictionary);
                    break;
                case Dictionary<string, string> stringStringDictionary:
                    Read(stringStringDictionary);
                    break;
                case List<string> stringList:
                    Read(stringList);
                    break;
            }
        }
    }
}