using source.data;
using System.Collections.Generic;

namespace source.functions
{
    //this class contains the method to simulate the dormancy induction, endodormancy and ecodormancy processes
    public class dormancySeason
    {
        #region dormancy induction
       
        //compute dormancy induction
        public void induction(input input, parameters parameters, output output, output outputT1)
        {

            //check if dormancy induction started
            if (!outputT1.isDormancyInduced)
            {
                //estimate photoperiod 
                input.radData = utils.astronomy(input);

                #region photothermal units

                #region photothermal rate
                //call photoperiod function
                outputT1.dormancyInduction.photoperiodDormancyInductionRate=
                    utils.photoperiodFunctionInduction(input, parameters,outputT1);
                //call temperature function
                outputT1.dormancyInduction.temperatureDormancyInductionRate =
                    utils.temperatureFunctionInduction(input, parameters, outputT1);
                
                //compute dormancy induction rate
                outputT1.dormancyInduction.photoThermalDormancyInductionRate =
                    outputT1.dormancyInduction.photoperiodDormancyInductionRate *
                    outputT1.dormancyInduction.temperatureDormancyInductionRate;
                #endregion

                #region photothermal state and completion percentage
                //integrate the rate variable to compute the state variable
                outputT1.dormancyInduction.photoThermalDormancyInductionState = 
                    output.dormancyInduction.photoThermalDormancyInductionState +
                    outputT1.dormancyInduction.photoThermalDormancyInductionRate;

                //derive the percentage of phase completion
                outputT1.dormancyInductionPercentage = outputT1.dormancyInduction.photoThermalDormancyInductionState /
                    parameters.parDormancyInduction.photoThermalThreshold * 100;
               
                //check if dormancy induction is completed
                if (outputT1.dormancyInductionPercentage >= 100)
                {
                    //reset to 100% in case it exceeds (last day integration could be higher than threshold
                    outputT1.dormancyInductionPercentage = 100;
                    //boolean to state that dormancy is induced
                    outputT1.isDormancyInduced = true;
                    //reset to 0 the ecodormancy state
                    outputT1.ecodormancy.ecodormancyState = 0;

                   
                }

                #endregion

                #endregion

                #region update phenological code
                if (outputT1.dormancyInduction.photoThermalDormancyInductionState > 0)
                {
                    outputT1.phenoCode = 1;
                }
                #endregion
            }

        }

        #endregion

        #region endodormancy
        //compute endodormancy process
        public void endodormancy(input input, parameters parameters,
            output output,  output outputT1)
        {

            //check if dormancy is induced and ecodormancy is not completed
            if (outputT1.isDormancyInduced && !outputT1.isEcodormancyCompleted)
            {
                //initialize hourly temperature lists (call to the external function in utils static class)
                List<float> hourlyTemperatures = utils.hourlyTemperature(input);

                //internal variable to store chilling units
                float chillingUnits = utils.endodormancyRate(input, parameters, hourlyTemperatures, out List<float> chillingUnitsList);

                //compute daily chilling rate in a 0-1 scale
                outputT1.endodormancy.endodormancyRate = chillingUnits;

                //compute endodormancy progress
                outputT1.endodormancy.endodormancyState = output.endodormancy.endodormancyState+
                    outputT1.endodormancy.endodormancyRate;

                //compute endodormancy percentage
                outputT1.endodormancyPercentage = outputT1.endodormancy.endodormancyState /
                    parameters.parEndodormancy.chillingThreshold * 100;

                //if endodormancy is completed, set the variable to 100
                if (outputT1.endodormancyPercentage >= 100)
                {
                    outputT1.endodormancyPercentage = 100;                  
                }

            }

        }
        #endregion

        #region ecodormancy
        //compute ecodormancy process
        public void ecodormancy(input input, parameters parameters,
           output output, output outputT1)
        {
            //estimate photoperiod 
            input.radData = utils.astronomy(input);

            //check if dormancy is induced and ecodormancy is not completed
            if (outputT1.isDormancyInduced && !outputT1.isEcodormancyCompleted)
            {
                //the asymptote of photothermal units for ecodormancy depends on endodormancy percentage
                float asymptote = outputT1.endodormancyPercentage / 100;

                //compute ecodormancy rate (call to the external function in utils static class)
                outputT1.ecodormancy.ecodormancyRate = utils.ecodormancyRate(input, asymptote,parameters);

                //compute ecodormancy progress
                outputT1.ecodormancy.ecodormancyState = output.ecodormancy.ecodormancyState + outputT1.ecodormancy.ecodormancyRate;

                //ecodormancy completion percentage
                outputT1.ecodormancyPercentage = outputT1.ecodormancy.ecodormancyState /
                    parameters.parEcodormancy.photoThermalThreshold * 100;

                //if ecodormancy is completed, set the variable to 100 and set the boolean variable
                if (outputT1.ecodormancyPercentage >= 100)
                {
                    outputT1.ecodormancyPercentage = 100;
                    outputT1.isEcodormancyCompleted = true;
                }

                #region update phenological code
                if (outputT1.ecodormancy.ecodormancyState > 0)
                {
                    outputT1.phenoCode = 2;
                    outputT1.isGrowthCompleted = false;
                    outputT1.isDeclineCompleted = false;
                }
                #endregion
            }
            else
            {
                outputT1.ecodormancyPercentage = output.ecodormancyPercentage;               
            }
        }
        #endregion
    }
}
