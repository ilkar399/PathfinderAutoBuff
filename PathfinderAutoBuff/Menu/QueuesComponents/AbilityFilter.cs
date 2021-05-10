using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PathfinderAutoBuff.UnitLogic;


namespace PathfinderAutoBuff.Menu.QueuesComponents
{
    internal class AbilityFilter
    {
        /*
         * Abilty filter components. Made more or less universal 
         * for spells/abilities
         * 
         * Filter name, Filter function, Filtered Data
         * Caster and Intfilter could be made into the dynamic delegation
         * as it's supported by c#. But it seemed easier to make it as is
         * and just ignore the unneeded arguments when necessary.
         */
        public string Name { get; }
        public List<DataDescriptionInterface> FilteredData { get; private set; }
        [CanBeNull]
        public string Caster { get; private set; }
        [CanBeNull]
        public int IntFilter { get; private set; }
        public Func<string, int, List<DataDescriptionInterface>> Filter { get; private set; }

        //Constructor
        public AbilityFilter(string name, Func<string, int, List<DataDescriptionInterface>> filter)
        {
            this.Name = name;
            this.Filter = filter;
            this.FilteredData = new List<DataDescriptionInterface>();
        }

        //Constructor
        public AbilityFilter(string name, string caster, int intFilter, Func<string, int, List<DataDescriptionInterface>> filter)
        {
            this.Name = name;
            this.Filter = filter;
            this.Caster = caster;
            this.IntFilter = intFilter;
            this.FilteredData = new List<DataDescriptionInterface>();
        }

        public void Reset()
        {
            this.Caster = "";
            this.IntFilter = 0;
            this.FilteredData.Clear();
        }

        public void Update()
        {
            this.FilteredData = Filter(Caster, IntFilter);
        }

        public void Update(string caster, int IntFilter)
        {
            this.Caster = caster;
            this.IntFilter = IntFilter;
            this.FilteredData = Filter(caster, IntFilter);
        }

    }
}
