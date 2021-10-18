using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using PathfinderAutoBuff.Utility;

namespace PathfinderAutoBuff.QueueOperations
{
    //Queue metadata
    [Serializable]
    public class QueueMetadata
    {
        //Spellbook priorities
#if (WOTR)
        public bool MetadataMythicSpellbookPriority;
#endif
        public bool MetadataInverseCasterLevelPriority;
        //TODO spellbook priorities
        /*
         * Stop casting wthen the highest CL spellbook is used up
         * public bool StopCastingOnLowerSpellbook
        */
        //Spellslot priorities
        public bool MetadataIgnoreMetamagic;
        public bool MetadataLowestSlotFirst;
        //Metamagic priorities
        public List<Metamagic> MetamagicPriority;

        public string QueueName;

        //Constructor
        public QueueMetadata()
        {
            //Spellbook priorities
#if (WOTR)
            this.MetadataMythicSpellbookPriority = SettingsWrapper.MetadataMythicSpellbookPriority;
#endif
            this.MetadataInverseCasterLevelPriority = SettingsWrapper.MetadataInverseCasterLevelPriority;
            //Spellslot priorities
            this.MetadataIgnoreMetamagic = SettingsWrapper.MetadataIgnoreMetamagic;
            this.MetadataLowestSlotFirst = SettingsWrapper.MetadataLowestSlotFirst;
            //Metamagic priorities
            this.MetamagicPriority = new List<Metamagic>();

            this.QueueName = null;
        }

    //Loading metadata from file
    public QueueMetadata(string queueName): this()
        {
            string filePath = (Path.Combine(SettingsWrapper.ModPath, "scripts"));
            filePath += $"/{queueName}.metadata";
            this.QueueName = queueName;
            QueueMetadata fileData = null;
            if (!File.Exists(filePath))
            {
                this.MetamagicPriority = new List<Metamagic>(SettingsWrapper.MetamagicPriority);
                return;
            }
            try
            {
                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    fileData = (QueueMetadata)serializer.Deserialize(file, typeof(QueueMetadata));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deserializing the queue {queueName}");
                Logger.Log(ex.StackTrace);
                this.MetamagicPriority = new List<Metamagic>(SettingsWrapper.MetamagicPriority);
                return;
            }
            if (fileData != null)
            {
#if (WOTR)
                this.MetadataMythicSpellbookPriority = fileData.MetadataMythicSpellbookPriority;
#endif
                this.MetadataInverseCasterLevelPriority = fileData.MetadataInverseCasterLevelPriority;
                this.MetadataLowestSlotFirst = fileData.MetadataLowestSlotFirst;
                this.MetadataIgnoreMetamagic = fileData.MetadataIgnoreMetamagic;
                this.MetamagicPriority = fileData.MetamagicPriority;
            }
        }

        //Saving metadata
        public bool Save()
        {
            string savepath;
            Logger.Debug($"Saving {this.QueueName} Metadata");
            savepath = Path.Combine(SettingsWrapper.ModPath, "scripts") + $"/{this.QueueName}.metadata";
            try
            {
                if (!Directory.Exists(Path.Combine(SettingsWrapper.ModPath, "scripts")))
                {
                    Directory.CreateDirectory(Path.Combine(SettingsWrapper.ModPath, "scripts"));
                }
                var JsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };
//                DefaultJsonSettings.Initialize();
                using (StreamWriter file = File.CreateText(savepath))
                {
                    JsonSerializer serializer = JsonSerializer.Create(JsonSettings);
                    serializer.Serialize(file, this);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Error saving the queue {this.QueueName}");
                Logger.Log(e.StackTrace);
                return false;
            }
            return false;
        }
    }
}
