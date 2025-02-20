using System;

// data namespace. It contains all the classes that are used to pass data to the swell model
namespace source.data
{

    // input data class. It is used as argument in each computing method
    public class input
    {
        public string vegetationIndex { get; set; }  //vegetation index (NDVI/EVI)
        public float airTemperatureMaximum { get; set; }  //air temperature maximum, °C
        public float airTemperatureMinimum { get; set; } //air temperature minimum, °C
        public float precipitation { get; set; }   //precipitation, mm
        public DateTime date { get; set; }     //date, DateTime object
        public float latitude { get; set; }     //latitude, decimal degrees

        public radData radData = new radData(); //radiation data, see below
    }

    // separate object containing radiation data    
    public class radData
    {
       public float dayLength { get; set; } //hours
    
    }
}
