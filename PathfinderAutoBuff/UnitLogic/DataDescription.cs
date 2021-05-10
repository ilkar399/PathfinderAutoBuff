using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;


namespace PathfinderAutoBuff.UnitLogic
{
    /*
     * Ability Data description and interface declaration 
     * for AbilityList and whenever it's needed
     */
    public interface DataDescriptionInterface
    {
        string AbilityID { get; }
        string AbilityName { get; }
        string AbilityDescription { get; }
        List<string> Casters { get; }
        [CanBeNull] string SourceItem { get; }
    }

    public class GenericDataDescription : DataDescriptionInterface
    {
        public string AbilityID { get; private set; }
        public string AbilityName { get; private set; }
        public string AbilityDescription { get; private set; }
        public List<string> Casters { get; private set; }
        public string SourceItem { get; private set; }

        public GenericDataDescription(
            string AbilityID,
            string AbilityName,
            string AbilityDescription,
            List<string> Casters,
            string SourceItem = "")
        {
            this.AbilityID = AbilityID;
            this.AbilityName = AbilityName;
            this.AbilityDescription = AbilityDescription;
            this.Casters = Casters;
            this.SourceItem = SourceItem;
        }
    }

}
