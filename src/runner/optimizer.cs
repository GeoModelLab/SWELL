using UNIMI.optimizer;
using source.data;
using source.functions;
using runner.data;
using System.Reflection;

namespace runner
{
    //this class perform the multi-start simplex optimization of SWELL parameters
    internal class optimizer : IOBJfunc
    {
        #region optimizer methods
        int _neval = 0;
        int _ncompute = 0;

        public Dictionary<int, string> _Phenology = new Dictionary<int, string>();
        
        // the number of times that this function is called        
        public int neval
        {
            get
            {
                return _neval;
            }

            set
            {
                _neval = value;
            }
        }

        
        // the number of times where the function is evaluated 
        // (when an evaluation is requested outside the parameters domain this counter is not incremented        
        public int ncompute
        {
            get
            {
                return _ncompute;
            }

            set
            {
                _ncompute = value;
            }
        }
        #endregion

        #region instances of SWELL data types and functions
        //SWELL data types
        output output = new output();
        output outputT1 = new output();
        //instance of the SWELL functions
        NDVIdynamics NDVIdynamics = new NDVIdynamics();
        dormancySeason dormancy = new dormancySeason();
        growingSeason growing = new growingSeason();
        #endregion

        #region instance of the weather reader class
        weatherReader weatherReader = new weatherReader();
        #endregion

        #region local variables to perform the optimization
        public Dictionary<string, pixel> idPixel = new Dictionary<string, pixel>();
        public Dictionary<string, Dictionary<string, parameter>> species_nameParam = new Dictionary<string, Dictionary<string, parameter>>();
        public Dictionary<string, float> param_outCalibration = new Dictionary<string, float>();
        public string caseStudy;
        public Dictionary<DateTime, output> date_outputs = new Dictionary<DateTime, output>();
        public string weatherDir;
        public List<string> allWeatherDataFiles;
        public string species;
        public string weatherDataFile;        
        public bool isCalibration;
        //for validation
        public string country;
        public int parset;
        public int startYear;
        public int endYear;
        public string outputsCalibrationDir;
        public string outputsValidationDir;
        public string outputParametersDir;
        public string vegetationIndex;
        #endregion

        //this method perform the multi-start simplex calibration
        public double ObjfuncVal(double[] Coefficient, double[,] limits)
        {
            #region Calibration methods
            for (int j = 0; j < Coefficient.Length; j++)
            {
                if (Coefficient[j] == 0)
                {
                    break;
                }
                if (Coefficient[j] <= limits[j, 0] | Coefficient[j] > limits[j, 1])
                {
                    return 1E+300;
                }
                
            }
            _neval++;
            _ncompute++;
            #endregion

            #region assign parameters under calibration
            source.data.parameters parameters = new parameters();
            parDormancyInduction parDormancy = new parDormancyInduction();
            PropertyInfo[] propsDormancy = parDormancy.GetType().GetProperties();//get all properties
            parEndodormancy parEndodormancy = new parEndodormancy();
            PropertyInfo[] propsEndodormancy = parEndodormancy.GetType().GetProperties();//get all properties
            parEcodormancy parEcodormancy = new parEcodormancy();
            PropertyInfo[] propsEcodormancy = parEcodormancy.GetType().GetProperties();//get all properties
            parGrowth parGrowth = new parGrowth();
            PropertyInfo[] propsGrowth = parGrowth.GetType().GetProperties();//get all properties
            parGreendown parGreendown = new parGreendown();
            PropertyInfo[] propsGreendown = parGreendown.GetType().GetProperties();//get all properties
            parSenescence parSenescence = new parSenescence();
            PropertyInfo[] propsDecline = parSenescence.GetType().GetProperties();//get all properties
            parVegetationIndex parVegetationIndex = new parVegetationIndex();
            PropertyInfo[] propsGrowingSeason = parVegetationIndex.GetType().GetProperties();//get all properties            

            int i = 0;
            //single species calibration
            foreach (var param in species_nameParam[species_nameParam.Keys.First()].Keys)
            {
                if (species_nameParam[species_nameParam.Keys.First()][param].calibration != "")
                {
                    //split class from param name
                    string[] paramClass = param.Split('_');
                    if (paramClass[0] == "parDormancy")
                    {
                        foreach (PropertyInfo prp in propsDormancy)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parDormancy, (float)(Coefficient[i])); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parEndodormancy")
                    {
                        foreach (PropertyInfo prp in propsEndodormancy)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parEndodormancy, (float)(Coefficient[i])); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parEcodormancy")
                    {
                        foreach (PropertyInfo prp in propsEcodormancy)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parEcodormancy, (float)(Coefficient[i])); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parGrowth")
                    {
                        foreach (PropertyInfo prp in propsGrowth)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parGrowth, (float)(Coefficient[i])); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parSenescence")
                    {
                        foreach (PropertyInfo prp in propsDecline)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parSenescence, (float)(Coefficient[i])); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parVegetationIndex")
                    {
                        foreach (PropertyInfo prp in propsGrowingSeason)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parVegetationIndex, (float)(Coefficient[i])); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parGreendown")
                    {
                        foreach (PropertyInfo prp in propsGreendown)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parGreendown, (float)(Coefficient[i])); //set the values for this parameter
                            }

                        }
                    }
                    i++;
                }
                else
                {
                    //split class from param name
                    string[] paramClass = param.Split('_');
                    if (paramClass[0] == "parDormancy")
                    {
                        foreach (PropertyInfo prp in propsDormancy)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parDormancy, param_outCalibration[param]); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parEndodormancy")
                    {
                        foreach (PropertyInfo prp in propsEndodormancy)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parEndodormancy, param_outCalibration[param]); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parEcodormancy")
                    {
                        foreach (PropertyInfo prp in propsEcodormancy)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parEcodormancy, param_outCalibration[param]); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parGrowth")
                    {
                        foreach (PropertyInfo prp in propsGrowth)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parGrowth, param_outCalibration[param]); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parSenescence")
                    {
                        foreach (PropertyInfo prp in propsDecline)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parSenescence, param_outCalibration[param]); //set the values for this parameter
                            }

                        }
                    }
                    else if (paramClass[0] == "parVegetationIndex")
                    {
                        foreach (PropertyInfo prp in propsGrowingSeason)
                        {
                            if (paramClass[1] == prp.Name)
                            {
                                prp.SetValue(parVegetationIndex, param_outCalibration[param]); //set the values for this parameter
                            }

                        }
                    }
                }
               
            }

          


            parameters.parDormancyInduction = parDormancy;
            parameters.parEndodormancy = parEndodormancy;
            parameters.parEcodormancy = parEcodormancy;
            parameters.parGrowth = parGrowth;
            parameters.parGreendown = parGreendown;
            parameters.parSenescence = parSenescence;
            parameters.parVegetationIndex = parVegetationIndex;
            
            #endregion

            //list of errors
            List<double> errors = new List<double>();

            double objFun = 0;
            List<float> simulated = new List<float>();
            List<float> measured = new List<float>();


            //loop over ids
            foreach (var id in idPixel.Keys)
            {
                //reinitialize variables for each site
                output = new output();
                outputT1 = new output();

                // Find the closest point
                string closestFile = FindClosestPoint(idPixel[id].latitude, idPixel[id].longitude, allWeatherDataFiles);

                //read weather data
                Dictionary<DateTime, input> weatherData = weatherReader.readWeather(weatherDir + "//" + closestFile);

               
                //loop over dates
                foreach (var day in weatherData.Keys)
                {
                   
                    //set the simulation period: TODO: adjust according to your needs!!!
                    if (day.Year >= startYear && day.Year <= endYear)
                    {
                        weatherData[day].vegetationIndex = vegetationIndex;
                        //call the SWELL model
                        modelCall(weatherData[day], parameters);

                        
                        if (idPixel[id].dateNDVInorm.ContainsKey(day) && day.Year >= startYear+1)
                        {
                            simulated.Add(outputT1.ndvi / 100);
                            measured.Add(idPixel[id].dateNDVInorm[day]);

                            //this is used to weight less the errors when the NDVI is very low
                            if (idPixel[id].dateNDVInorm[day] < (parVegetationIndex.minimumNDVI+(parVegetationIndex.minimumNDVI *.2F)))
                            {     
                            errors.Add(Math.Pow(idPixel[id].dateNDVInorm[day] - outputT1.ndvi / 100, 2)/6);
                            }
                            else
                            {
                            errors.Add(Math.Pow(idPixel[id].dateNDVInorm[day] - outputT1.ndvi / 100, 2));
                            }
                        }
                    }                   
                }
            }
            double pearsonR = Math.Round(ComputePearsonR(measured, simulated),3);
            double RMSE = Math.Round(Math.Sqrt(errors.Sum() / errors.Count), 3);

            //compute objective function
            objFun =(1-pearsonR) + RMSE;
            
            //write it in the console
            Console.WriteLine("pixel {0} group {1}: RMSE = {2}, Pearson r = {3}", idPixel.Keys.First(), 
                idPixel[idPixel.Keys.First()].ecoName, RMSE, pearsonR);

            //return the objective function
            return objFun;
        }

        public static double ComputePearsonR(List<float> listX, List<float> listY)
        {
            if (listX == null || listY == null || listX.Count != listY.Count || listX.Count == 0)
                throw new ArgumentException("Input lists must be non-null, of equal length, and not empty.");

            int n = listX.Count;

            double meanX = listX.Average();
            double meanY = listY.Average();

            double covariance = 0;
            double varianceX = 0;
            double varianceY = 0;

            for (int i = 0; i < n; i++)
            {
                double diffX = listX[i] - meanX;
                double diffY = listY[i] - meanY;

                covariance += diffX * diffY;
                varianceX += diffX * diffX;
                varianceY += diffY * diffY;
            }

            double denominator = Math.Sqrt(varianceX) * Math.Sqrt(varianceY);
            
            
            if (denominator == 0)
            {
                return -99;
            }
            



            return covariance / denominator;
        }


        //this method is called in the validation run
        public void oneShot(Dictionary<string, float> paramValue, out Dictionary<DateTime, output> date_outputs, int parset)
        {
            //reinitialize the date_outputs object
            date_outputs = new Dictionary<DateTime, output>();

            #region assign parameters
            source.data.parameters parameters = new parameters();
            parDormancyInduction parDormancy = new parDormancyInduction();
            PropertyInfo[] propsDormancy = parDormancy.GetType().GetProperties();//get all properties
            parEndodormancy parEndodormancy = new parEndodormancy();
            PropertyInfo[] propsEndodormancy = parEndodormancy.GetType().GetProperties();//get all properties
            parEcodormancy parEcodormancy = new parEcodormancy();
            PropertyInfo[] propsEcodormancy = parEcodormancy.GetType().GetProperties();//get all properties
            parGrowth parGrowth = new parGrowth();
            PropertyInfo[] propsGrowth = parGrowth.GetType().GetProperties();//get all properties
            parGreendown parGreendown = new parGreendown();
            PropertyInfo[] propsGreendown = parGreendown.GetType().GetProperties();//get all properties
            parSenescence parSenescence = new parSenescence();
            PropertyInfo[] propsDecline = parSenescence.GetType().GetProperties();//get all properties
            parVegetationIndex parVegetationIndex = new parVegetationIndex();
            PropertyInfo[] propsGrowingSeason = parVegetationIndex.GetType().GetProperties();//get all properties
            
            //assign calibrated parameters
            foreach (var param in paramValue.Keys)
            {
                //split class from param name
                string[] paramClass = param.Split('_');
                
                if (paramClass[0] == "parDormancy")
                {
                    foreach (PropertyInfo prp in propsDormancy)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parDormancy, (float)(paramValue[param])); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parEndodormancy")
                {
                    foreach (PropertyInfo prp in propsEndodormancy)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parEndodormancy, (float)(paramValue[param])); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parEcodormancy")
                {
                    foreach (PropertyInfo prp in propsEcodormancy)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parEcodormancy, (float)(paramValue[param])); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parGrowth")
                {
                    foreach (PropertyInfo prp in propsGrowth)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parGrowth, (float)(paramValue[param])); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parSenescence")
                {
                    foreach (PropertyInfo prp in propsDecline)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parSenescence, (float)(paramValue[param])); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parVegetationIndex")
                {
                    foreach (PropertyInfo prp in propsGrowingSeason)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parVegetationIndex, (float)(paramValue[param])); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parGreendown")
                {
                    foreach (PropertyInfo prp in propsGreendown)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parGreendown, (float)(paramValue[param])); //set the values for this parameter
                        }

                    }
                }

            }


            //assign parameters out of calibration
            foreach(var param in param_outCalibration.Keys)
            {
                //split class from param name
                string[] paramClass = param.Split('_');
                if (paramClass[0] == "parDormancy")
                {
                    foreach (PropertyInfo prp in propsDormancy)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parDormancy, param_outCalibration[param]); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parEndodormancy")
                {
                    foreach (PropertyInfo prp in propsEndodormancy)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parEndodormancy, param_outCalibration[param]); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parEcodormancy")
                {
                    foreach (PropertyInfo prp in propsEcodormancy)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parEcodormancy, param_outCalibration[param]); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parGrowth")
                {
                    foreach (PropertyInfo prp in propsGrowth)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parGrowth, param_outCalibration[param]); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parSenescence")
                {
                    foreach (PropertyInfo prp in propsDecline)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parSenescence, param_outCalibration[param]); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parVegetationIndex")
                {
                    foreach (PropertyInfo prp in propsGrowingSeason)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parVegetationIndex, param_outCalibration[param]); //set the values for this parameter
                        }

                    }
                }
                else if (paramClass[0] == "parGreendown")
                {
                    foreach (PropertyInfo prp in propsGreendown)
                    {
                        if (paramClass[1] == prp.Name)
                        {
                            prp.SetValue(parGreendown, param_outCalibration[param]); //set the values for this parameter
                        }

                    }
                }
            }


            parameters.parDormancyInduction = parDormancy;
            parameters.parEndodormancy = parEndodormancy;
            parameters.parEcodormancy = parEcodormancy;
            parameters.parGrowth = parGrowth;
            parameters.parGreendown = parGreendown;
            parameters.parSenescence = parSenescence;           
            parameters.parVegetationIndex = parVegetationIndex;
            #endregion

            //loop over pixels
            foreach (var id in idPixel.Keys)
            {
                //reinitialize variables for each site
                output = new output();
                outputT1 = new output();

                // Find the closest point
                string closestFile = FindClosestPoint(idPixel[id].latitude, idPixel[id].longitude, allWeatherDataFiles);

                //read weather
                Dictionary<DateTime, input> weatherData =
                   weatherReader.readWeather(weatherDir + "//" + closestFile);

                //reinitialize the date_outputs object
                date_outputs = new Dictionary<DateTime, output>();

                //loop over dates
                foreach (var day in weatherData.Keys)
                {
                    if (day.Year >= startYear && day.Year <= endYear)
                    {
                        //assing latitude
                        weatherData[day].latitude = idPixel[id].latitude;
                        weatherData[day].vegetationIndex = vegetationIndex;
                        //call the SWELL model
                        modelCall(weatherData[day], parameters);

                        //add weather data to output object
                        outputT1.weather.airTemperatureMinimum = weatherData[day].airTemperatureMinimum;
                        outputT1.weather.airTemperatureMaximum = weatherData[day].airTemperatureMaximum;
                        outputT1.weather.precipitation = weatherData[day].precipitation;
                        outputT1.weather.radData.dayLength = weatherData[day].radData.dayLength;
                        outputT1.weather.radData.extraterrestrialRadiation = weatherData[day].radData.extraterrestrialRadiation;

                        //add the NDVI data
                        if (idPixel[id].dateNDVInorm.ContainsKey(day))
                        {
                            outputT1.ndviReference = idPixel[id].dateNDVInorm[day];
                        }

                        //add the object to the output dictionary
                        date_outputs.Add(day, outputT1);
                    }
                }
                
                //write the outputs from the calibration run
                writeOutputsCalibration(id, date_outputs, isCalibration);
            }
        }

        #region write output files from calibration and validation
        //write outputs from the calibration run
        public void writeOutputsCalibration(string id, Dictionary<DateTime, output> date_outputs, bool isCalibration)
        {
            if (isCalibration)
            {
            #region write outputs
            //empty list to store outputs
            List<string> toWrite = new List<string>();

            #region full output file header
            //define the file header
            //string header = "pixel,date," +
            //"tmax,tmin,prec,dayLength,photoInduction,temperatureInduction," +
            //"dormancyInductionRate,dormancyInductionState,dormancyPercentage," +
            //"endodormancyRate,endodormancyState,endodormancyPercentage," +
            //"ecodormancyRate,ecodormancyState,ecodormancyPercentage," +
            //"growthRate,growthState,growthPercentage," +
            // "greendownRate,greendownState,greendownPercentage," +
            //"declineRate,declineState,declinePercentage," +
            //"NDVIswell_rate,NDVI_swell,reference,phenoCode";
            #endregion

            string header = "pixel,group,date,year,doy,tmax,tmin,dayLength,phenoPhase," +
            "dormancyInductionRate,dormancyInductionState,dormancyPercentage," +
            "endodormancyRate,endodormancyState,endodormancyPercentage," +
            "ecodormancyRate,ecodormancyState,ecodormancyPercentage," +
            "growthRate,growthState,growthPercentage," +
            "greendownRate,greendownState,greendownPercentage," +
            "declineRate,declineState,declinePercentage," +
            "SWELL_rate,SWELL,reference";
            //add the header to the list
            toWrite.Add(header);

            #region full output file
            //loop over days
            //foreach (var weather in date_outputs.Keys)
            //{
            //    //empty string to store outputs
            //    string line = "";
            //
            //    //populate this line
            //    line += id + ",";
            //    line += weather.ToString() + ",";
            //    line += date_outputs[weather].weather.airTemperatureMaximum + ",";
            //    line += date_outputs[weather].weather.airTemperatureMinimum + ",";
            //    line += date_outputs[weather].weather.precipitation + ",";
            //    line += date_outputs[weather].weather.radData.dayLength + ",";
            //    line += date_outputs[weather].dormancyInduction.photoperiodDormancyInductionRate + ",";
            //    line += date_outputs[weather].dormancyInduction.temperatureDormancyInductionRate + ",";
            //    line += date_outputs[weather].dormancyInduction.photoThermalDormancyInductionRate + ",";
            //    line += date_outputs[weather].dormancyInduction.photoThermalDormancyInductionState + ",";
            //    line += date_outputs[weather].dormancyInductionPercentage + ",";
            //    line += date_outputs[weather].endodormancy.endodormancyRate + ",";
            //    line += date_outputs[weather].endodormancy.endodormancyState + ",";
            //    line += date_outputs[weather].endodormancyPercentage + ",";
            //    line += date_outputs[weather].ecodormancy.ecodormancyRate + ",";
            //    line += date_outputs[weather].ecodormancy.ecodormancyState + ",";
            //    line += date_outputs[weather].ecodormancyPercentage + ",";
            //    line += date_outputs[weather].growth.growthRate + ",";
            //    line += date_outputs[weather].growth.growthState + ",";
            //    line += date_outputs[weather].growthPercentage + ",";
            //    line += date_outputs[weather].greenDown.greenDownRate + ",";
            //    line += date_outputs[weather].greenDown.greenDownState + ",";
            //    line += date_outputs[weather].greenDownPercentage + ",";
            //    line += date_outputs[weather].decline.declineRate + ",";
            //    line += date_outputs[weather].decline.declineState + ",";
            //    line += date_outputs[weather].declinePercentage + ",";
            //    line += date_outputs[weather].ndviRate + ",";
            //    line += date_outputs[weather].ndvi / 100 + ",";
            //    if (idPixel[id].dateNDVInorm.ContainsKey(weather))
            //    {
            //        line += idPixel[id].dateNDVInorm[weather] + ",";
            //    }
            //    else
            //    {
            //        line += ",";
            //    }
            //    line += date_outputs[weather].phenoCode;
            //
            //    //add the line to the list
            //    toWrite.Add(line);
            //}
            ////save the file
            //System.IO.File.WriteAllLines(outputsCalibrationDir + "//" + id + ".csv", toWrite);
            #endregion

            //TODO: FIX IT
            #region R file output
            foreach (var weather in date_outputs.Keys)
            {
                //empty string to store outputs
                string line = "";

                //populate this line
                line += id + ",";
                line += idPixel[id].ecoName + ",";
                line += weather.ToShortDateString() + ",";
                line += weather.Year + ",";
                line += weather.DayOfYear + ",";
                line += date_outputs[weather].weather.airTemperatureMaximum + ",";
                line += date_outputs[weather].weather.airTemperatureMinimum + ",";
                line += Math.Round(date_outputs[weather].weather.radData.dayLength, 3) + ",";
                #region phenocodes
                if (date_outputs[weather].phenoCode == 1)
                {
                    line += "Dormancy induction,";
                }
                else if (date_outputs[weather].phenoCode == 2)
                {
                    line += "Dormancy,";
                }
                else if (date_outputs[weather].phenoCode == 3)
                {
                    line += "Growth,";
                }
                else if (date_outputs[weather].phenoCode == 4)
                {
                    line += "Greendown,";
                }
                else if (date_outputs[weather].phenoCode == 5)
                {
                    line += "Senescence,";
                }
                #endregion
                line += Math.Round(date_outputs[weather].dormancyInduction.photoThermalDormancyInductionRate, 3) + ",";
                line += Math.Round(date_outputs[weather].dormancyInduction.photoThermalDormancyInductionState, 3) + ",";
                line += Math.Round(date_outputs[weather].dormancyInductionPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].endodormancy.endodormancyRate, 3) + ",";
                line += Math.Round(date_outputs[weather].endodormancy.endodormancyState, 3) + ",";
                line += Math.Round(date_outputs[weather].endodormancyPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].ecodormancy.ecodormancyRate, 3) + ",";
                line += Math.Round(date_outputs[weather].ecodormancy.ecodormancyState, 3) + ",";
                line += Math.Round(date_outputs[weather].ecodormancyPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].growth.growthRate, 3) + ",";
                line += Math.Round(date_outputs[weather].growth.growthState, 3) + ",";
                line += Math.Round(date_outputs[weather].growthPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].greenDown.greenDownRate, 3) + ",";
                line += Math.Round(date_outputs[weather].greenDown.greenDownState, 3) + ",";
                line += Math.Round(date_outputs[weather].greenDownPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].decline.declineRate, 3) + ",";
                line += Math.Round(date_outputs[weather].decline.declineState, 3) + ",";
                line += Math.Round(date_outputs[weather].declinePercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].ndviRate, 3) + ",";
                line += Math.Round(date_outputs[weather].ndvi / 100, 3) + ",";
                if (idPixel[id].dateNDVInorm.ContainsKey(weather))
                {
                    line += Math.Round(idPixel[id].dateNDVInorm[weather], 3);
                }
                else
                {
                    line += "";
                }

                //add the line to the list
                toWrite.Add(line);
            }
            if (isCalibration)
            {
                //save the file
                System.IO.File.WriteAllLines(outputsCalibrationDir + "//" + id + "_" + parset + ".csv", toWrite);
            }
            else
            {
                //save the file
                System.IO.File.WriteAllLines(outputsValidationDir + "//" + id + "_" + parset + ".csv", toWrite);
            }
            #endregion


            #endregion
            }
        }

        //write outputs from the validation run
        public void writeOutputsValidation(string id, Dictionary<DateTime, output> date_outputs, 
            Dictionary<DateTime, Dictionary<string, float>> ndviSimulations)
        {
            #region write outputs
            //empty list of strings to store outputs
            List<string> toWrite = new List<string>();

            // Define a base header
            string header = "pixel,group,date,year,doy,tmax,tmin,dayLength,phenoPhase," +
                "dormancyInductionRate,dormancyInductionState,dormancyPercentage," +
                "endodormancyRate,endodormancyState,endodormancyPercentage," +
                "ecodormancyRate,ecodormancyState,ecodormancyPercentage," +
                "growthRate,growthState,growthPercentage," +
                "greendownRate,greendownState,greendownPercentage," +
                "declineRate,declineState,declinePercentage," +
                "SWELL_rate,SWELL,reference,SWELL_10,SWELL_25,SWELL_40,SWELL_60,SWELL_75,SWELL_90";
          

            // Add the header to the list
            toWrite.Add(header);

            // Loop over days
            foreach (var weather in date_outputs.Keys)
            {
                // Empty first line
                string line = "";

                //populate this line
                line += id + ",";
                line += idPixel[id].ecoName + ",";
                line += weather.ToShortDateString() + ",";
                line += weather.Year + ",";
                line += weather.DayOfYear + ",";
                line += date_outputs[weather].weather.airTemperatureMaximum + ",";
                line += date_outputs[weather].weather.airTemperatureMinimum + ",";
                line += Math.Round(date_outputs[weather].weather.radData.dayLength, 3) + ",";
                #region phenocodes
                if (date_outputs[weather].phenoCode < 2)
                {
                    line += "Dormancy induction,";
                }
                else if (date_outputs[weather].phenoCode < 3)
                {
                    line += "Dormancy,";
                }
                else if (date_outputs[weather].phenoCode < 4)
                {
                    line += "Growth,";
                }
                else if (date_outputs[weather].phenoCode < 5)
                {
                    line += "Greendown,";
                }
                else 
                {
                    line += "Senescence,";
                }
                
                #endregion
                line += Math.Round(date_outputs[weather].dormancyInduction.photoThermalDormancyInductionRate, 3) + ",";
                line += Math.Round(date_outputs[weather].dormancyInduction.photoThermalDormancyInductionState, 3) + ",";
                line += Math.Round(date_outputs[weather].dormancyInductionPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].endodormancy.endodormancyRate, 3) + ",";
                line += Math.Round(date_outputs[weather].endodormancy.endodormancyState, 3) + ",";
                line += Math.Round(date_outputs[weather].endodormancyPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].ecodormancy.ecodormancyRate, 3) + ",";
                line += Math.Round(date_outputs[weather].ecodormancy.ecodormancyState, 3) + ",";
                line += Math.Round(date_outputs[weather].ecodormancyPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].growth.growthRate, 3) + ",";
                line += Math.Round(date_outputs[weather].growth.growthState, 3) + ",";
                line += Math.Round(date_outputs[weather].growthPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].greenDown.greenDownRate, 3) + ",";
                line += Math.Round(date_outputs[weather].greenDown.greenDownState, 3) + ",";
                line += Math.Round(date_outputs[weather].greenDownPercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].decline.declineRate, 3) + ",";
                line += Math.Round(date_outputs[weather].decline.declineState, 3) + ",";
                line += Math.Round(date_outputs[weather].declinePercentage, 3) + ",";
                line += Math.Round(date_outputs[weather].ndviRate, 3) + ",";
                line += Math.Round(date_outputs[weather].ndvi / 100, 3) + ",";
                if (idPixel[id].dateNDVInorm.ContainsKey(weather))
                {
                    line += Math.Round(idPixel[id].dateNDVInorm[weather], 3) + ",";
                }
                else
                {
                    line += ",";
                }

                // Add NDVI simulation values
                if (ndviSimulations.ContainsKey(weather))
                {
                    line += Math.Round(ndviSimulations[weather]["10th"] / 100, 3) + ",";
                    line += Math.Round(ndviSimulations[weather]["25th"] / 100, 3) + ",";
                    line += Math.Round(ndviSimulations[weather]["40th"] / 100, 3) + ",";
                    line += Math.Round(ndviSimulations[weather]["60th"] / 100, 3) + ",";
                    line += Math.Round(ndviSimulations[weather]["75th"] / 100, 3) + ",";
                    line += Math.Round(ndviSimulations[weather]["90th"] / 100, 3);
                }
               

                // Trim trailing comma (optional)
                line = line.TrimEnd(',');

                // Add the line to the list
                toWrite.Add(line);
            }

            // Create the directory if it doesn't exist
            string dirPath = outputsValidationDir  + "//";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            //save the file
            System.IO.File.WriteAllLines(dirPath + id  + ".csv", toWrite);
            #endregion
        }
        #endregion

        //call the SWELL functions
        public void modelCall(input weatherData, parameters parameters)
        {
            //pass values from the previous day
            output = outputT1;
            outputT1 = new output();

            //call the functions
            //dormancy season
            dormancy.induction(weatherData, parameters, output, outputT1);
            dormancy.endodormancy(weatherData, parameters, output, outputT1);
            dormancy.ecodormancy(weatherData, parameters, output, outputT1);
            //growing season
            growing.growthRate(weatherData, parameters, output, outputT1);
            growing.greendownRate(weatherData, parameters, output, outputT1);
            growing.declineRate(weatherData, parameters, output, outputT1);
            //NDVI dynamics
            NDVIdynamics.ndviNormalized(weatherData, parameters, output, outputT1);
        }

        #region associate the correct grid weather to the corresponding remote sensing pixel
        //find the nearest weather grid with respect to pixel latitude and longitude
        private string FindClosestPoint(double targetLatitude, double targetLongitude, List<string> fileNames)
        {
            double closestDistance = double.MaxValue;
            string closestFileName = null;

            foreach (string fileName in fileNames)
            {
                // Extract latitude and longitude from the file name
                string[] parts = fileName.Replace(".csv", "").Split('_');
                double latitude = double.Parse(parts[0]);
                double longitude = double.Parse(parts[1]);

                // Calculate distance using Haversine formula
                double distance = CalculateDistance(targetLatitude, targetLongitude, latitude, longitude);

                // Update closest point if the current distance is smaller
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFileName = fileName;
                }
            }

            return closestFileName;
        }

        //calculate distance between pixel and weather grid centroids
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Radius of the Earth in kilometers
            const double R = 6371;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        //conversion to radians
        static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        #endregion
    }
}
