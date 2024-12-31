using System;

// data namespace. It contains all the classes that are used to pass data to the swell model
namespace source.data
{
    // output data class. It is used as argument in each computing method and return updated values.
    // Each phenophase is represented by a class containing its state and rate variables.
    public class output
    {
        #region phenophase classes
        // get or set the dormancy induction data.
        public dormancyInduction dormancyInduction = new dormancyInduction();

        // get or set the endodormancy data.
        public endodormancy endodormancy = new endodormancy();

        // get or set the ecodormancy data.
        public ecodormancy ecodormancy = new ecodormancy();

        // get or set the greendown data.
        public greenDown greenDown = new greenDown();

        // get or set the growth data.
        public growth growth = new growth();

        // get or set the decline data.
        public decline decline = new decline();

        #endregion
      
        #region global variables

        #region boolean variables referring to the phenophase state        
        // Get or set whether dormancy is induced.
        public bool isDormancyInduced { get; set; }
        // Get or set whether ecodormancy is completed.
        public bool isEcodormancyCompleted { get; set; }        
        // Get or set whether decline is completed.
        public bool isDeclineCompleted { get; set; }        
        // Get or set whether growth is completed.
        public bool isGrowthCompleted { get; set; }        
        // Get or set whether greendown is completed.        
        public bool isGreendownCompleted { get; set; }
        #endregion

        #region variables storing the percentage of the phenophase completion
        
        // Get or set the percentage of the dormancy induction completion.
        public float dormancyInductionPercentage { get; set; }        
        // Get or set the percentage of the endodormancy completion.
        public float endodormancyPercentage { get; set; }      
        // Get or set the percentage of the ecodormancy completion.
        public float ecodormancyPercentage { get; set; }        
        // Get or set the percentage of the growth completion.
        public float growthPercentage { get; set; }        
        // Get or set the percentage of the greendown completion.
        public float greenDownPercentage { get; set; }        
        // Get or set the percentage of the decline completion.
        public float declinePercentage { get; set; }
        #endregion

        #region variables related to NDVI dynamics        
        // Get or set the simulated NDVI.
        public float ndvi { get; set; }
        
        // Get or set the simulated daily rate of change of the NDVI.
        public float ndviRate { get; set; }
        
        // Get or set the reference NDVI used for model calibration/evaluation.
        public float ndviReference { get; set; }

        public float ndviAtGrowth { get; set; }
        public float ndviAtSenescence { get; set; }
        #endregion

        #region variables related to the phenological phase

        // Get or set the phenological code.
        // 1: dormancy induction, 2: endo/ecodormancy, 3: growth, 4: greendown, 5: decline
        public float phenoCode { get; set; }
        
        // Get or set the phenological phase as a string
        public string phenoString { get; set; }
        #endregion

        #endregion

        // used to save the input data for the creation of the output object
        public input weather = new input();       
    }

    // dormancy induction data class. It is used to store the components of the photoperiodic dormancy induction unit
    public class dormancyInduction
    {   
        // dormancy induction rate due to photoperiod (day -1)
        public float photoperiodDormancyInductionRate { get; set; }
        // dormancy induction rate due to temperature
        public float temperatureDormancyInductionRate { get; set; }
        // dormancy induction rate due to photoperiod and temperature
        public float photoThermalDormancyInductionRate { get; set; }
        // dormancy induction state due to photoperiod and temperature
        public float photoThermalDormancyInductionState { get; set; }
    }
    
    // endodormancy data class. It is used to store the endodormancy rate and state variable (chilling units).    
    public class endodormancy
    {
        // endodormancy rate (chilling units)
        public float endodormancyRate { get; set; }
        // endodormancy state (chilling units)
        public float endodormancyState { get; set; }       
    }

    // ecodormancy data class. It is used to store the ecodormancy rate and state variable.
    public class ecodormancy
    {
        // ecodormancy rate
        public float ecodormancyRate { get; set; }
        // ecodormancy state
        public float ecodormancyState { get; set; }
    }
    // growth data class. It is used to store the growth rate and state variable.
    public class growth
    {
        // growth rate
        public float growthRate { get; set; }
        // growth state
        public float growthState { get; set; }
    }

    // decline data class. It is used to store the decline rate and state variable.
    public class decline
    {
        //decline rate
        public float declineRate { get; set; }
        //decline state
        public float declineState { get; set; }
    }

    // greendown data class. It is used to store the greendown rate and state variable
    public class greenDown
    {
        //greendown rate
        public float greenDownRate { get; set; }
        //greendown state
        public float greenDownState { get; set; }
    }
}
