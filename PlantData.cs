using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTGreenHouse
{
    public class PlantData  : TableEntity
    {
        public string Temp { get; set; }
            // Set up Partition and Row Key information
            public PlantData(string id, string name, string temp) : base(id, name)
            {
                this.Temp = temp;
            }


            public PlantData() { }

            public string Short_Code { get; set; }

            public string Raw_URL { get; set; }

    }
}
