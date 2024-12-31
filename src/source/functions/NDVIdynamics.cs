using source.data;
using System;

namespace source.functions
{
    //this class contains the method to simulate the growth, greendown and decline processes
    public class NDVIdynamics
    {
        public void ndviNormalized(input input, parameters parameters, output output, output outputT1)
        {
            outputT1.ndviAtGrowth = output.ndviAtGrowth;
            outputT1.ndviAtSenescence = output.ndviAtSenescence;
            //internal variable 
            float rateNDVInormalized = 0;
            if (outputT1.phenoCode == 2)
            {
                //compute day length previous day
                input inputPreviousDay = new input();
                inputPreviousDay.date = input.date.AddDays(-1);
                inputPreviousDay.latitude = input.latitude;

                radData dayLengthPreviousDay = utils.astronomy(inputPreviousDay);

                //compute growing degree days for the understory
                float tshift = parameters.parVegetationIndex.pixelTemperatureShift;
                float gddEco = utils.forcingUnitFunction(input, parameters.parGrowth.minimumTemperature - tshift,
                      parameters.parGrowth.optimumTemperature, parameters.parGrowth.maximumTemperature);

                //derive the rate of NDVI normalized for endodormancy
                float endodormancyContribution = 0;
                float aveTemp = (input.airTemperatureMaximum + input.airTemperatureMinimum) * 0.5F;
                float tratio = 0;

                if (aveTemp < (parameters.parGrowth.minimumTemperature - tshift))
                {
                    float tbelow0 = Math.Abs((parameters.parGrowth.minimumTemperature - tshift) - aveTemp);
                    tratio = -tbelow0 / 10;
                }
                else
                {
                    tratio = 0;
                }
                //compute endodormancy contribution
                endodormancyContribution = parameters.parVegetationIndex.nNDVIEndodormancy * tratio;

                //derive the rate of NDVI normalized for ecodormancy
                float ecodormancyNDVInormalized = parameters.parVegetationIndex.nNDVIEcodormancy;

                float ecodormancyContribution = gddEco * ecodormancyNDVInormalized;

                //derive the rate of NDVI normalized for dormancy
                rateNDVInormalized = ecodormancyContribution + endodormancyContribution;

                //vegetation is in winter phase
                if (dayLengthPreviousDay.dayLength > input.radData.dayLength && rateNDVInormalized > 0)
                {
                    rateNDVInormalized =  0F;
                }

            }
            //growth
            else if (outputT1.phenoCode == 3)
            {
                //derive the rate of NDVI normalized for growth
                float growthNDVInormalized = parameters.parVegetationIndex.nNDVIGrowth;
                //derive the contribution of growth to rate of NDVI
                rateNDVInormalized = growthNDVInormalized * 100 * outputT1.growth.growthRate;

                if (outputT1.ndviAtGrowth == 0)
                {
                    outputT1.ndviAtGrowth = output.ndvi / 100;
                    output.ndviAtGrowth = outputT1.ndviAtGrowth;
                }

                if (outputT1.ndviAtGrowth >= parameters.parVegetationIndex.maximumNDVI)
                {
                    outputT1.ndviAtGrowth = parameters.parVegetationIndex.maximumNDVI - 0.01F;
                }
                float EVItoMax = (output.ndvi / 100 - outputT1.ndviAtGrowth) / (parameters.parVegetationIndex.maximumNDVI - outputT1.ndviAtGrowth);
                if (EVItoMax > 1) EVItoMax = 1;
                rateNDVInormalized = growthNDVInormalized * (1 - outputT1.greenDownPercentage/100) * (1 - EVItoMax);

            }
            //greendown
            else if (outputT1.phenoCode == 4)
            {
                outputT1.ndviAtGrowth = 0;
                //derive the rate of NDVI normalized for greendown
                float greenDownNDVInormalized = parameters.parVegetationIndex.nNDVIGreendown;

                if (input.vegetationIndex == "EVI")
                {
                    float weight = 1 - (float)Math.Exp(-.25 * outputT1.greenDownPercentage);
                    //derive the contribution of greendown to rate of NDVI
                    rateNDVInormalized = -greenDownNDVInormalized *
                        (weight * outputT1.greenDown.greenDownRate);
                }
                else if (input.vegetationIndex == "NDVI")
                {
                    rateNDVInormalized = -greenDownNDVInormalized *
                       (outputT1.greenDownPercentage) / 100 *
                       outputT1.greenDown.greenDownRate;
                }
            }
            //decline
            else if (outputT1.phenoCode == 5 || outputT1.phenoCode == 1)
            {
                float weight = SymmetricBellFunction(outputT1.declinePercentage);
                //derive the contribution of decline to the rate of NDVI
                float declineNDVInormalized = -parameters.parVegetationIndex.nNDVIGreendown -
                    parameters.parVegetationIndex.nNDVISenescence * weight;


                //derive the contribution of degree days and photothermal units (decline) to rate of NDVI normalized
                rateNDVInormalized = declineNDVInormalized;
            }

            output.ndviRate = rateNDVInormalized;

            //update states
            outputT1.ndvi = output.ndvi + output.ndviRate;

            //NDVI thresholds between minimum and maximumNDVI
           if (outputT1.ndvi / 100 <parameters.parVegetationIndex.minimumNDVI)
           {
               outputT1.ndvi = parameters.parVegetationIndex.minimumNDVI*100;
           }

        }

        static float SymmetricBellFunction(float x)
        {
            float scaledX = (float)Math.Exp(-Math.Pow((x - 50), 2) / Math.Pow(10, 3));
            //float scaledX = 1F / (1F + (float)Math.Exp(0.1 * (x - 50F)));

            return scaledX;
        }       
    }
}