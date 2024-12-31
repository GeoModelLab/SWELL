using System;
using System.Collections.Generic;

namespace runner.data
{
    //this class define a pixel: the corresponding class, the minimum, maximum values, and the inclusion of the parameter in the calibration subset
    public class pixel
    {
        public string ecoName { get; set; }
        public string id { get; set; }
        public int cluster { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }      
        public Dictionary<DateTime, float> dateNDVInorm = new Dictionary<DateTime, float>();

    }
}
