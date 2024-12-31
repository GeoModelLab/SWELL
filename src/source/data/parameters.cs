using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace source.data
{
    //parameters class contains the parameters of the SWELL model
    public class parameters
    {
        //instance of the parameter class for dormancy induction
        public parDormancyInduction parDormancyInduction = new parDormancyInduction();
        //instance of the parameter class for endodormancy phase
        public parEndodormancy parEndodormancy = new parEndodormancy();
        //instance of the parameter class for ecodormancy phase
        public parEcodormancy parEcodormancy = new parEcodormancy();
        //instance of the parameter class for growth phase
        public parGrowth parGrowth = new parGrowth();
        //instance of the parameter class for greendown phase
        public parGreendown parGreendown = new parGreendown();
        //instance of the parameter class for decline phase
        public parSenescence parSenescence = new parSenescence();
        //instance of the parameter class for growing season
        public parVegetationIndex parVegetationIndex = new parVegetationIndex();
    }

    public class parDormancyInduction
    {
        //day length below which dormancy could be induced
        public float limitingPhotoperiod{ get; set; } //hours

        //day length below which dormancy induction is not limited by photoperiod
        public float notLimitingPhotoperiod { get; set; } //hours
       
        //threhsold of photothermal time to trigger dormancy induction
        public float photoThermalThreshold{ get; set; } //photothermal units

        //temperature below which dormancy could be induced
        public float limitingTemperature { get; set; } //hours

        //temperature below which dormancy induction is not limited by temperature
        public float notLimitingTemperature { get; set; } //hours
       
    }

    public class parEndodormancy
    {
        //limiting temperature for chilling accumulation (lower threshold) 
        public float limitingLowerTemperature { get; set; } //°C

        //non limiting temperature for chilling accumulation (lower threshold)
        public float notLimitingLowerTemperature { get; set; } //°C

        //non limiting temperature for chilling accumulation (upper threshold)
        public float notLimitingUpperTemperature { get; set; }  //°C

        //limiting temperature for chilling accumulation (upper threshold)
        public float limitingUpperTemperature { get; set; }  //°C

        //critical threshold for endodormancy completion
        public float chillingThreshold { get; set; } //thermal units

    }

    public class parEcodormancy
    {
        
        //non limiting day length for dormancy release
        public float notLimitingPhotoperiod { get; set; } //hours

        //non limiting temperature for dormancy release
        public float notLimitingTemperature { get; set; }  //°C
       
        //critical threshold for ecodormancy completion
        public float photoThermalThreshold { get; set; } //photothermal units

    }

    public class parGrowth
    {
        //minimum temperature for forcing accumulation
        public float minimumTemperature { get; set; } //°C

        //optimum temperature for forcing accumulation
        public float optimumTemperature { get; set; }  //°C

        //maximum temperature for forcing accumulation
        public float maximumTemperature { get; set; } //°C
        //critical threshold for growth phase completion
        public float thermalThreshold { get; set; } //photothermal units

        
    }
   
    public class parSenescence
    {
        //day length below which dormancy could be induced
        public float limitingPhotoperiod { get; set; } //hours

        //day length below which dormancy induction is not limited by photoperiod
        public float notLimitingPhotoperiod { get; set; } //hours       

        //threshold of photothermal time to end the decline phase
        public float photoThermalThreshold { get; set; } //photothermal time

        //temperature below which dormancy could be induced
        public float limitingTemperature { get; set; } //°C

        //temperature below which dormancy induction is not limited by temperature
        public float notLimitingTemperature { get; set; } //°C
       
    }

    public class parGreendown
    {     
        //threhsold of thermal time during the greendown phase
        public float thermalThreshold { get; set; } //photothermal units
    }

    public class parVegetationIndex
    {      
        //maximum NDVI rate during growth phenophase
        public float nNDVIGrowth { get; set; } //NDVI, unitless
        //maximum NDVI rate during endodormancy phenophase 
        public float nNDVIEndodormancy { get; set; } //NDVI, unitless
        //maximum NDVI rate during senescence phenophase
        public float nNDVISenescence { get; set; } //NDVI, unitless
        //maximum NDVI rate during greendown phenophase
        public float nNDVIGreendown { get; set; } //NDVI, unitless
        //maximum NDVI rate during ecodormancy phenophase
        public float nNDVIEcodormancy { get; set; } //NDVI, unitless
        //temperature shift for understory, pixel specific
        public float pixelTemperatureShift { get; set; } //°C
        public float minimumNDVI { get; set; } //NDVI, unitless
        public float maximumNDVI { get; set; } //NDVI, unitless
    }
}

