using source.data;
using source.functions;

namespace source.functions
{
    //this static class contains the utility functions used by the SWELL model
    public static class utils
    {
        #region additional weather inputs
        //hourly temperatures for chilling (24 values in a list) (Campbell, 1985)
        public static List<float> hourlyTemperature(input input)
        {
            //empty list
            List<float> hourlyTemperatures = new List<float>();

            //average temperature
            double Tavg = (input.airTemperatureMaximum + input.airTemperatureMinimum) / 2;
            //daily range
            double DT = input.airTemperatureMaximum - input.airTemperatureMinimum;
            for (int h = 0; h < 24; h++)
            {
                //14 is set as the hour with maximum temperature
                hourlyTemperatures.Add((float)(Tavg + DT / 2 * Math.Cos(0.2618F * (h - 14))));
            }
            //return the list of hourly temperatures
            return hourlyTemperatures;
        }

        #region astronomy
        public static radData astronomy(input input)
        {
            float solarConstant = 4.921F;
            float DtoR = (float)Math.PI / 180;
            float dd;
            float ss;
            float cc;
            float ws;
            float dayHours = 0;

            dd = 1 + 0.0334F * (float)Math.Cos(0.01721 * input.date.DayOfYear - 0.0552);
            float SolarDeclination = 0.4093F * (float)Math.Sin((6.284 / 365) * (284 + input.date.DayOfYear));
            float SolarDeclinationMinimum = 0.4093F * (float)Math.Sin((6.284 / 365) * (284 + 356));//winter solstice
            ss = (float)Math.Sin(SolarDeclination) * (float)Math.Sin(input.latitude * DtoR);
            cc = (float)Math.Cos(SolarDeclination) * (float)Math.Cos(input.latitude * DtoR);
            ws = (float)Math.Acos(-Math.Tan(SolarDeclination) * (float)Math.Tan(input.latitude * DtoR));
            float wsMinimum = (float)Math.Acos(-Math.Tan(SolarDeclinationMinimum) * (float)Math.Tan(input.latitude * DtoR));

            //if -65 < Latitude and Latitude < 65 dayLength and ExtraterrestrialRadiation are
            //approximated using the algorithm in the hourly loop
            //if (rd.Latitude <65 || rd.Latitude>-65)
            if (input.latitude < 65 && input.latitude > -65)
            {
                input.radData.dayLength = 0.13333F / DtoR * ws;
                input.radData.extraterrestrialRadiation = solarConstant * dd * 24 / (float)Math.PI
                    * (ws * ss + cc * (float)Math.Sin(ws));
            }
            else
            {
                input.radData.dayLength = dayHours;
            }
            input.radData.hourSunrise = 12 - input.radData.dayLength / 2;
            input.radData.hourSunset = 12 + input.radData.dayLength / 2;

            return input.radData;
        }

        public static float dayLength(input input)
        {
            float DtoR = (float)Math.PI / 180;
            float dd;
            float ss;
            float cc;
            float ws;
            float dayHours = 0;

            dd = 1 + 0.0334F * (float)Math.Cos(0.01721 * input.date.DayOfYear - 0.0552);
            float SolarDeclination = 0.4093F * (float)Math.Sin((6.284 / 365) * (284 + input.date.DayOfYear));
            float SolarDeclinationYesterday = 0.4093F * (float)Math.Sin((6.284 / 365) * (284 + input.date.AddDays(-1).DayOfYear));
            ss = (float)Math.Sin(SolarDeclination) * (float)Math.Sin(input.latitude * DtoR);
            cc = (float)Math.Cos(SolarDeclination) * (float)Math.Cos(input.latitude * DtoR);
            ws = (float)Math.Acos(-Math.Tan(SolarDeclination) * (float)Math.Tan(input.latitude * DtoR));
            float wsYesterday = (float)Math.Acos(-Math.Tan(SolarDeclinationYesterday) * (float)Math.Tan(input.latitude * DtoR));

            //if -65 < Latitude and Latitude < 65 dayLength and ExtraterrestrialRadiation are
            //approximated using the algorithm in the hourly loop
            //if (rd.Latitude <65 || rd.Latitude>-65)
            if (input.latitude < 65 && input.latitude > -65)
            {
                dayHours = 0.13333F / DtoR * ws;
            }
            else
            {
                dayHours = 0;
            }

            return dayHours;
        }


        #endregion

        #endregion

        #region SWELL phenophase specific functions

        #region growth, greendown, decline thermal units
        //this method computes the forcing thermal unit (Yan & Hunt, 1999)
        public static float forcingUnitFunction(input input, float tmin, float topt, float tmax)
        {
            //local output variable
            float forcingRate = 0;

            //average air temperature
            float averageAirTemperature = (input.airTemperatureMaximum +
                input.airTemperatureMinimum) / 2;

            //if average temperature is below minimum or above maximum
            if (averageAirTemperature < tmin || averageAirTemperature > tmax)
            {
                forcingRate = 0;
            }
            else
            {
                //intermediate computations
                float firstTerm = (tmax - averageAirTemperature) / (tmax - topt);
                float secondTerm = (averageAirTemperature - tmin) / (topt - tmin);
                float Exponential = (topt - tmin) / (tmax - topt);

                //compute forcing rate
                forcingRate = (float)(firstTerm * Math.Pow(secondTerm, Exponential));
            }
            //assign to output variable
            return forcingRate;
        }
        #endregion

        #region dormancy induction 
        //photoperiod function
        public static float photoperiodFunctionInduction(input input,
           parameters parameters, output outputT1)
        {
            //local variable to store the output
            float photoperiodFunction = 0;

            //day length is non limiting PT
            if (input.radData.dayLength < parameters.parDormancyInduction.notLimitingPhotoperiod)
            {
                photoperiodFunction = 1;
            }
            else if (input.radData.dayLength > parameters.parDormancyInduction.limitingPhotoperiod)
            {
                photoperiodFunction = 0;
            }
            else
            {
                float midpoint = (parameters.parDormancyInduction.limitingPhotoperiod + parameters.parDormancyInduction.notLimitingPhotoperiod) * 0.5F;
                float width = parameters.parDormancyInduction.limitingPhotoperiod - parameters.parDormancyInduction.notLimitingPhotoperiod;

                //compute function
                photoperiodFunction = 1 / (1 + (float)Math.Exp(10 / width *
                    ((input.radData.dayLength - midpoint))));

            }
            //return the photoperiod function
            return photoperiodFunction;
        }

        //temperature function
        public static float temperatureFunctionInduction(input input,
           parameters parameters, output outputT1)
        {
            //average temperature
            float tAverage = (float)(input.airTemperatureMaximum + input.airTemperatureMinimum) * 0.5F;

            //local variable to store the output
            float temperatureFunction = 0;

            if (tAverage <= parameters.parDormancyInduction.notLimitingTemperature)
            {
                temperatureFunction = 1;
            }
            else if (tAverage >= parameters.parDormancyInduction.limitingTemperature)
            {
                temperatureFunction = 0;
            }
            else
            {
                float midpoint = (parameters.parDormancyInduction.limitingTemperature + parameters.parDormancyInduction.notLimitingTemperature) * .5F;
                float width = (parameters.parDormancyInduction.limitingTemperature - parameters.parDormancyInduction.notLimitingTemperature);
                //compute function
                temperatureFunction = 1 / (1 + (float)Math.Exp(10 / width * (tAverage - midpoint)));

            }
            //return the output
            return temperatureFunction;
        }
        #endregion

        #region endodormancy
        public static float endodormancyRate(input input, parameters parameters, //internal list to store hourly temperatures
            List<float> hourlyTemperatures, out List<float> chillingUnitsList)
        {

            chillingUnitsList = new List<float>();
            //internal variable to store chilling units
            float chillingUnits = 0;

            #region chilling units accumulation
            foreach (var temperature in hourlyTemperatures)
            {
                //when hourly temperature is below the limiting lower temperature or above the limiting upper temperature
                if (temperature < parameters.parEndodormancy.limitingLowerTemperature ||
                    temperature > parameters.parEndodormancy.limitingUpperTemperature)
                {
                    //no chilling units are accumulated 
                    chillingUnits = 0; //not needed, just to be clear
                }
                //when hourly temperature is between the limiting lower temperature
                //and the non limiting lower temperature
                else if (temperature >= parameters.parEndodormancy.limitingLowerTemperature &&
                    temperature < parameters.parEndodormancy.notLimitingLowerTemperature)
                {
                    //compute lag and slope
                    double midpoint = (parameters.parEndodormancy.limitingLowerTemperature +
                        parameters.parEndodormancy.notLimitingLowerTemperature) / 2;
                    double width = Math.Abs(parameters.parEndodormancy.limitingLowerTemperature -
                        parameters.parEndodormancy.notLimitingLowerTemperature);

                    //update chilling units
                    chillingUnits = 1 / (1 + (float)Math.Exp(10 / -width * ((temperature - midpoint))));
                }
                //when hourly temperature is between the non limiting lower temperature and the 
                //non limiting upper temperature
                else if (temperature >= parameters.parEndodormancy.notLimitingLowerTemperature &&
                    temperature <= parameters.parEndodormancy.notLimitingUpperTemperature)
                {
                    chillingUnits = 1;
                }
                //when hourly temperature is between the non limiting upper temperature and the
                //limiting upper temperature
                else
                {
                    double midpoint = (parameters.parEndodormancy.limitingUpperTemperature +
                       parameters.parEndodormancy.notLimitingUpperTemperature) / 2;
                    double width = Math.Abs(parameters.parEndodormancy.limitingUpperTemperature -
                        parameters.parEndodormancy.notLimitingUpperTemperature);

                    chillingUnits = 1 / (1 + (float)Math.Exp(10 / width * ((temperature - midpoint))));
                }

                chillingUnitsList.Add(chillingUnits);
            }
            #endregion

            //return the output
            return chillingUnitsList.Sum() / 24;
        }
        #endregion

        #region ecodormancy
        public static float ecodormancyRate(input input, float asymptote, parameters parameters)
        {
            //local variable to store the output
            float ecodormancyRate = 0;

           
            //the slope of the photothermal function depends on day length 
            float ratioPhotoperiod = input.radData.dayLength / parameters.parEcodormancy.notLimitingPhotoperiod;
            if (ratioPhotoperiod > 1)
            {
                ratioPhotoperiod = 1;
            }

            //modify asymptote depending on day length and endodormancy completion
            float asymptoteModifier = ratioPhotoperiod * asymptote;
            float newAsymptote = asymptote + (1 - asymptote) * asymptoteModifier;

            //lag depends on maximum temperature and day length
            float midpoint = parameters.parEcodormancy.notLimitingTemperature * 0.5F +
                (1 - ratioPhotoperiod) * parameters.parEcodormancy.notLimitingTemperature;
            float tavg = (input.airTemperatureMaximum + input.airTemperatureMinimum) * 0.5F;
            float width = parameters.parEcodormancy.notLimitingTemperature * ratioPhotoperiod;

            //compute ecodormancy rate
            ecodormancyRate = newAsymptote /
              (1 + (float)Math.Exp(-10 / width * ((tavg - midpoint)))); ;
           
            //return the output
            return ecodormancyRate;
        }
        #endregion

        #endregion
    }

    #region vvvv execution interface
    public class vvvvInterface
    {
        //initialize the SWELL phenology classes with functions
        dormancySeason dormancy = new dormancySeason();
        growingSeason growing = new growingSeason();
        NDVIdynamics NDVIdynamics = new NDVIdynamics();
        //initialize the outputT1
        output outputT0 = new output();
        output outputT1 = new output();

        //this method contains the logic for the SWELL execution in vvvv
        public output vvvvExecution(input input, parameters parameters)
        {

            //pass values from the previous day
            outputT0 = outputT1;
            outputT1 = new output();

            //call the functions
            //dormancy season
            dormancy.induction(input, parameters, outputT0, outputT1);
            dormancy.endodormancy(input, parameters, outputT0, outputT1);
            dormancy.ecodormancy(input, parameters, outputT0, outputT1);
            //growing season
            growing.growthRate(input, parameters, outputT0, outputT1);
            growing.greendownRate(input, parameters, outputT0, outputT1);
            growing.declineRate(input, parameters, outputT0, outputT1);
            //NDVI dynamics
            NDVIdynamics.ndviNormalized(input, parameters, outputT0, outputT1);

            return outputT1;
        }



    }
    #endregion

}