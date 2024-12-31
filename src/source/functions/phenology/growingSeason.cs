using source.data;
using System;

namespace source.functions
{
    //this class contains the method to simulate the growth, greendown and decline processes
    public class growingSeason
    {
        //compute growth phenophase
        public void growthRate(input input, parameters parameters, output output, output outputT1)
        {
            //check if the growth phenophase is not completed and ecodormancy is completed
            if (!outputT1.isGrowthCompleted && outputT1.isEcodormancyCompleted) 
            {
                //check if the growth state is below the critical threshold
                if (output.growth.growthState < parameters.parGrowth.thermalThreshold)
                {
                    //compute growth rate
                    outputT1.growth.growthRate =
                            utils.forcingUnitFunction(input, parameters.parGrowth.minimumTemperature,
                            parameters.parGrowth.optimumTemperature, parameters.parGrowth.maximumTemperature);
                }
                else
                {
                    outputT1.growth.growthRate = 0;
                }

                //update the growth state
                outputT1.growth.growthState =  output.growth.growthState + outputT1.growth.growthRate;

                //update phenological code
                if (outputT1.growth.growthState > 0 && outputT1.ecodormancyPercentage==100)
                {
                    outputT1.ecodormancy.ecodormancyRate = 0;
                    outputT1.endodormancy.endodormancyRate = 0;
                    outputT1.endodormancy.endodormancyState = 0;
                    outputT1.endodormancyPercentage = 0;
                    outputT1.phenoCode = 3;                    
                }

                //if growth state is above the threshold, set it to the critical threshold
                if (outputT1.growth.growthState > parameters.parGrowth.thermalThreshold &&
                    !outputT1.isGrowthCompleted)
                {
                    outputT1.growth.growthState = parameters.parGrowth.thermalThreshold;
                    outputT1.dormancyInduction.photoThermalDormancyInductionState = 0;
                    outputT1.isGrowthCompleted = true;
                }

                //compute the completion percentage of the growth state
                outputT1.growthPercentage = outputT1.growth.growthState /
                parameters.parGrowth.thermalThreshold *100;
            }
            else //otherwise growth percentage is kept to the previous value
            {
                outputT1.growthPercentage = output.growthPercentage;
            }
        }

        //compute greendown phenophase
        public void greendownRate(input input, parameters parameters,
          output output, output outputT1)
        {
            //check if the growth phenophase is  completed and greendown is not completed
            if (outputT1.growthPercentage == 100 && !outputT1.isGreendownCompleted)
            {
                //compute thermal unit (call to an external function in the utils static class)
                outputT1.greenDown.greenDownRate =
                        utils.forcingUnitFunction(input, parameters.parGrowth.minimumTemperature,
                        parameters.parGrowth.optimumTemperature, parameters.parGrowth.maximumTemperature);

                //update greendown state variable
                outputT1.greenDown.greenDownState = output.greenDown.greenDownState +
                    outputT1.greenDown.greenDownRate;

                //update greendown percentage
                outputT1.greenDownPercentage = outputT1.greenDown.greenDownState /
                    parameters.parGreendown.thermalThreshold * 100;

                //limit the greendown percentage to 100
                if (outputT1.greenDownPercentage >= 100)
                {
                    outputT1.greenDownPercentage = 100;
                    outputT1.isGreendownCompleted = true;
                    outputT1.isDormancyInduced = false;
                    outputT1.greenDown.greenDownRate = 0;
                }

                //update phenological code
                if (!outputT1.isGreendownCompleted)
                {
                    outputT1.phenoCode = 4;
                }
            }
        }

        //compute decline phenophase
        public void declineRate(input input, parameters parameters,
           output output,  output outputT1)
        {
            //check if the greendown phase is completed and the decline phase is not completed
            if (outputT1.greenDownPercentage == 100 && !outputT1.isDeclineCompleted)
            {
                //compute thermal unit
                float thermalUnit =
                        utils.forcingUnitFunction(input, parameters.parGrowth.minimumTemperature,
                        parameters.parGrowth.optimumTemperature, parameters.parGrowth.maximumTemperature);

                //compute rad data
                input.radData = utils.astronomy(input);
                //call photoperiod function
                float photoFunction = utils.photoperiodFunctionInduction(input, parameters, outputT1);
                float tempFunction = utils.temperatureFunctionInduction(input, parameters, outputT1);
                float induPhotoThermal = photoFunction * tempFunction;

                //compute the percentage completion of the decline phase before updating, to compute the weighted average
                float declinePercentageYesterday = output.decline.declineState /
                    parameters.parSenescence.photoThermalThreshold;

                //compute the weighted average of the decline rate
                outputT1.decline.declineRate = thermalUnit * (1 - declinePercentageYesterday) +
                     induPhotoThermal * declinePercentageYesterday;

                //state variable
                outputT1.decline.declineState = output.decline.declineState +
                    outputT1.decline.declineRate;

                //update decline percentage
                outputT1.declinePercentage = outputT1.decline.declineState /
                    parameters.parSenescence.photoThermalThreshold * 100;

                //limit the decline percentage to 100
                if (outputT1.declinePercentage >= 100)
                {
                    outputT1.declinePercentage = 100;
                    outputT1.isDeclineCompleted = true;
                    outputT1.isDormancyInduced = false;
                    outputT1.greenDown.greenDownRate = 0;
                    outputT1.decline.declineRate = 0;
                }

                //update the phenological code
                if (!outputT1.isDeclineCompleted)
                {
                    outputT1.phenoCode = 5;
                }
            }
        }      
    }
}