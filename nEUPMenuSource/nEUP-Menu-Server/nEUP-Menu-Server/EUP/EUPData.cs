using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nEUP_Menu_Server.EUP
{
    public class EUPData
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<EUPProp> props { get; set; }
        public List<EUPComponent> components { get; set; }
        public string category { get; set; }
        public string gender { get; set; }

        public EUPData() { }

        public EUPData(int id, string name, List<EUPProp> props, List<EUPComponent> components, string category, string gender)
        {
            this.id = id;
            this.name = name;
            this.props = props;
            this.components = components;
            this.category = category;
            this.gender = gender;
        }
    }
}
