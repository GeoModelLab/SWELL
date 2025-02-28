using UNIMI.optimizer;
using source.data;
using runner;
using runner.data;
using MathNet.Numerics.Distributions;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

#region Console opening message

Console.WriteLine(".-..  ..--.  .");
Console.WriteLine("`-.|/\\||- |  |");
Console.WriteLine("`-''  ''--'--'--");
#endregion

#region read the configuration file (SWELLConfig.config)
// Check if the config file path is passed as an argument
string configFilePath = string.Empty;

if (args.Length > 0)
{
    // If an argument is provided, use it as the path to the config file
    configFilePath = args[0];
}
else
{
    // Fallback to default directory if not passed
    // Use the current directory or modify this to a predefined path
    configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SWELLconfig.json");
}

// Check if the configuration file exists
if (!File.Exists(configFilePath))
{
    Console.WriteLine($"Error: The configuration file was not found at {configFilePath}");
    return;  // Exit if the file is not found
}

// Read the JSON configuration file
string jsonString = File.ReadAllText(configFilePath);
var config = JsonSerializer.Deserialize<root>(jsonString);

//Console.WriteLine("Configuration loaded from: " + configFilePath);

//switch between calibration and validation
bool isCalibration = bool.Parse(config.settings.calibration.ToString());
string runningMode = "";
if (isCalibration) runningMode = "calibration"; else runningMode = "validation";

//set start pixel and number of pixels (for calibration)
int startPixel = int.Parse(config.settings.startPixel.ToString());
int numberPixels = int.Parse(config.settings.numberPixels.ToString());
//Console.WriteLine("START FROM PIXEL: {0} TO PIXEL: {1}", startPixel, startPixel + numberPixels);

int simplexes = int.Parse(config.settings.simplexes.ToString());
int iterations = int.Parse(config.settings.iterations.ToString());

int validationReplicates = int.Parse(config.settings.validationReplicates.ToString());
string parametersDistribution = config.settings.parametersDistributions.ToString();

//set species
string species = config.settings.species.ToString();

if (isCalibration)
{
    Console.WriteLine("species: {0} | simplexes: {1} | iterations: {2}", species, simplexes, iterations);
}
else
{
    Console.WriteLine("species: {0} | replicates: {1} | distribution: {2}", species, validationReplicates, parametersDistribution);
}


//set weather directory
var weatherDir = config.settings.weatherDirectory;

//set parameters file
string parametersDataFile = config.settings.parametersDataFile.ToString();
string referenceDataFile = config.settings.referenceDataFile.ToString();
string parametersValidationFile = config.settings.parametersValidationFile.ToString();

//set start and end year
int startYear = int.Parse(config.settings.startYear.ToString());
int endYear = int.Parse(config.settings.endYear.ToString());

string outputsCalibrationDir = config.settings.outputCalibrationDir.ToString();
string outputsValidationDir = config.settings.outputValidationDir.ToString();
string outputParametersDir = config.settings.outputParametersDir.ToString();

string vegetationIndex = config.settings.vegetationIndex.ToString();

#endregion

#region read reference NDVI data
//message to console
Console.WriteLine("");

//instance of reference reader class
referenceReader referenceReader = new referenceReader();

//read simulated pixels
Random random = new Random();
Dictionary<string, ID> allPixels = referenceReader.readReferenceData(referenceDataFile);
#endregion

#region get all weather files
//optimizer class
optimizer optimizer = new optimizer();
optimizer.startYear = startYear;
optimizer.endYear = endYear;
optimizer.outputsCalibrationDir = outputsCalibrationDir;
optimizer.outputsValidationDir = outputsValidationDir;
optimizer.outputParametersDir = outputParametersDir;
optimizer.vegetationIndex = vegetationIndex;
//read weather files
var weatherFiles = new DirectoryInfo(weatherDir.ToString()).GetFiles();

// Convert FileInfo[] to List<string>
optimizer.allWeatherDataFiles = weatherFiles.Select(file => Path.GetFileName(file.FullName)).ToList();
optimizer.weatherDir = weatherDir.ToString();            
#endregion

#region read SWELL parameter files

//list of already calibrated files
var calibratedFilesInfo = new DirectoryInfo(outputsCalibrationDir).GetFiles();

// Convert FileInfo[] to List<string>
List<string> calibratedFiles = calibratedFilesInfo.Select(file => Path.GetFileNameWithoutExtension(file.FullName)).ToList();

//read parameter file with limits
paramReader paramReader = new paramReader();
optimizer.species_nameParam = paramReader.read(parametersDataFile, species);

//data structure to store calibrated parameters
Dictionary<string, float> paramCalibValue = new Dictionary<string, float>();
#endregion

// Define an array of bright colors
ConsoleColor[] colors = new ConsoleColor[]
{
    ConsoleColor.Cyan,
    ConsoleColor.Yellow,
    ConsoleColor.Green,
    ConsoleColor.Magenta,
    ConsoleColor.Blue
};

Random rnd = new Random();
Console.ForegroundColor = colors[rnd.Next(colors.Length)]; // Pick a random color

#region switch between calibration and validation
if (isCalibration)
{
    #region SWELL calibration

    #region define optimizer settings
    //optimizer instance
    MultiStartSimplex msx = new MultiStartSimplex();

    msx.NofSimplexes = simplexes;// 19; //5;
    msx.Ftol = 0.000001;
    msx.Itmax = iterations;// 999;
    #endregion


    #region loop over pixels

    for (int pixel = startPixel; pixel < startPixel + numberPixels; pixel++)
    {
        if (pixel< allPixels.Count )
        {
            //get pixel ID
            string pixelID = allPixels.ElementAt(pixel).Key;

            //check if pixel is already calibrated
            if (!calibratedFiles.Contains(pixelID))
            {
               
                //set pixel to calibrate
                optimizer.idPixel = allPixels.Where(x => x.Key == pixelID).ToDictionary(p => p.Key, p => p.Value); ;

                #region define parameters settings for calibration
                //count parameters under calibration
                int paramCalibrated = 0;
                //loop over parameters
                foreach (var name in optimizer.species_nameParam[species].Keys)
                {
                    //add parameter to calibration if calibration field is not empty (x)
                    if (optimizer.species_nameParam[species][name].calibration != "") { paramCalibrated++; }

                }
                //set number of dimension in the matrix
                double[,] Limits = new double[paramCalibrated, 2];

                //parameters out of calibration
                Dictionary<string, float> param_outCalibration = new Dictionary<string, float>();
                //populate limits 
                //count parameters under calibration
                int i = 0;
                foreach (var name in optimizer.species_nameParam[species].Keys)
                {
                    if (optimizer.species_nameParam[species][name].calibration != "")
                    {
                        Limits[i, 1] = optimizer.species_nameParam[species][name].maximum;
                        Limits[i, 0] = optimizer.species_nameParam[species][name].minimum;
                        i++;
                    }
                    else
                    {
                        param_outCalibration.Add(name, optimizer.species_nameParam[species][name].value);
                    }
                }
                double[,] results = new double[1, 1];
                #endregion

                //set optimizer calibration properties
                optimizer.species = species;
                optimizer.isCalibration = isCalibration;
                optimizer.param_outCalibration = param_outCalibration;
                optimizer.pixelNumber = allPixels.Count;
                optimizer.currentPixelNumber = pixel+1;
                optimizer.consoleColor = colors[rnd.Next(colors.Length)];
                //run optimizer
                msx.Multistart(optimizer, paramCalibrated, Limits, out results);

                //get calibrated parameters
                paramCalibValue = new Dictionary<string, float>();
                int count = 0;

                #region write calibrated parameters
                string header = "pixelID, ecoRegion, param, value";
                List<string> writeParam = new List<string>();
                writeParam.Add(header);
                foreach (var param in optimizer.species_nameParam[species].Keys)
                {
                    if (optimizer.species_nameParam[species][param].calibration != "")
                    {
                        //write a line for each parameter
                        string line = "";
                        line += pixelID + ",";
                        line += allPixels[pixelID].ecoName + ",";
                        line += param + ",";
                        line += Math.Round(results[0, count],3);
                        writeParam.Add(line);
                        paramCalibValue.Add(param, (float)Math.Round(results[0, count],3));
                        count++;
                    }
                }

                //write calibrated parameters to file
                System.IO.File.WriteAllLines(outputParametersDir + "//"+ pixelID + "_parameters.csv", writeParam);
                #endregion

                //empty dictionary of dates and outputs objects
                var dateOutputs = new Dictionary<DateTime, output>();
                //execute model with calibrated parameters
                optimizer.oneShot(paramCalibValue, out dateOutputs, optimizer.parset);
            }
        }
    }
    #endregion

    #endregion
}
else
{
    Dictionary<string, ID> allPixelsToValidate = referenceReader.readReferenceData(referenceDataFile);

    List<string> availableGroups = new List<string>();
    StreamReader sr = new StreamReader(parametersValidationFile);
    sr.ReadLine();
    while(!sr.EndOfStream)
    {
        string group = sr.ReadLine().Split(',')[0];
        if (!availableGroups.Contains(group))
        {
            availableGroups.Add(group);
        }
    }
    sr.Close();
    int barWidth = 30;

    #region loop over pixels 
    int currentPixelNumber = 0;

    List<ID> pixelsNotValidated = new List<ID>();

    foreach (var pixel in allPixelsToValidate.Keys)
    {

        //check if the pixels has been already validated
        if (!availableGroups.Contains(allPixelsToValidate[pixel].ecoName))
        {
            allPixelsToValidate[pixel].id = pixel;
            pixelsNotValidated.Add(allPixelsToValidate[pixel]);
        }
        else
        {
            //get calibrated parameters
            paramCalibValue = new Dictionary<string, float>();

            //get mean and standard deviation for each parameter
            Dictionary<string, List<float>> param_meanSD = new Dictionary<string, List<float>>();

            #region assign default parameters (out of calibration subset)
            //parameters out of calibration
            Dictionary<string, float> param_outCalibration = new Dictionary<string, float>();

            //populate parameters out of calibration
            foreach (var name in optimizer.species_nameParam[species].Keys)
            {
                if (optimizer.species_nameParam[species][name].calibration == "")
                {
                    //add parameter to calibration if calibration field is not empty (x)
                    param_outCalibration.Add(name, optimizer.species_nameParam[species][name].value);
                }
            }

            optimizer.param_outCalibration = param_outCalibration;
            #endregion

            //set optimizer properties
            optimizer.species = species;
            optimizer.isCalibration = isCalibration;

            //set pixel to calibrate
            Dictionary<string, ID> keyValuePairs = new Dictionary<string, ID>();

            //add pixel to dictionary
            keyValuePairs.Add(pixel, allPixelsToValidate[pixel]);
            optimizer.idPixel = keyValuePairs;

            //structure to store outputs from each parset
            var parsetOutputs = new Dictionary<int, Dictionary<DateTime, output>>();

            // Calculate the progress percentage
            double progress = Math.Round((double)currentPixelNumber / allPixelsToValidate.Count, 3);
            int progressBlocks = (int)(progress * barWidth) + 1;
            string progressBar = new string('█', progressBlocks).PadRight(barWidth, ' ');

            Console.ForegroundColor = colors[rnd.Next(colors.Length)];
            #region loop over calibrated parsets for each pixel
            for (int parset = 0; parset < validationReplicates; parset++)
            { 

                //reinitialize calibrated parameters
                paramCalibValue = new Dictionary<string, float>();

                //read calibrated parameters
                sr = new StreamReader(parametersValidationFile);
                sr.ReadLine();

                //read calibrated parameters and sample from distribution
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split(',', '"');
                    if (allPixelsToValidate[pixel].ecoName == line[0])
                    {
                        string sd = line[4];
                        if (line[4] == "NA")
                        {
                            sd = (float.Parse(line[4]) * 0.05F).ToString();
                        }

                        float randomSample = 0;
                        if (parametersDistribution == "uniform")
                        {
                            //uniform distribution from median +- one standard deviation
                            ContinuousUniform uniformDistribution = new ContinuousUniform(float.Parse(line[3]) - Math.Abs(double.Parse(sd)),
                                float.Parse(line[3]) + Math.Abs(double.Parse(sd)));
                            //sample one parset
                            randomSample = (float)uniformDistribution.Sample();
                        }
                        else if (parametersDistribution == "normal")
                        {
                            Normal normal = new Normal(float.Parse(line[9]), double.Parse(sd));
                            //sample one parset
                            randomSample = (float)normal.Sample();
                        }
                        paramCalibValue.Add(line[1] + "_" + line[2], randomSample);
                    }

                }
                sr.Close();

                optimizer.country = allPixelsToValidate[pixel].ecoName;
                optimizer.parset = parset;

                //empty list of dates and SWELL outputs
                var dateOutputs = new Dictionary<DateTime, output>();

                //run SWELL in validation (from optimizer class)
                optimizer.oneShot(paramCalibValue, out dateOutputs, parset);

                parsetOutputs.Add(parset, dateOutputs);

  

                // Messaging to console
                Console.Write("\rvalidation: pixel = {0:F2}; group = {1}; replicate = {2} {3} |", pixel,
                    allPixelsToValidate[pixel].ecoName, parset+1, progressBar);
            }
            #endregion

            // Flatten the nested dictionary and group by DateTime
            var flattenedAndGrouped = parsetOutputs.SelectMany(kv => kv.Value).GroupBy(kv => kv.Key);


            // Dictionary to hold the results
            Dictionary<DateTime, output> datePixelMeans = new Dictionary<DateTime, output>();

            // Dictionary to store all NDVI simulations for each date
            var ndviSimulations = new Dictionary<DateTime, Dictionary<string, float>>();

            // Loop over dates and outputs
            foreach (var group in flattenedAndGrouped)
            {
                // Define pixel mean output instance
                output pixelOutput = new output();

                // Get the date
                DateTime date = group.Key;

                // Compute median for each output except NDVI
                pixelOutput.phenoCode = Median(group.Select(kv => kv.Value.phenoCode)); // Assuming phenoCode is averaged
                pixelOutput.weather.date = date;
                pixelOutput.weather.airTemperatureMaximum = Median(group.Select(kv => kv.Value.weather.airTemperatureMaximum));
                pixelOutput.weather.airTemperatureMinimum = Median(group.Select(kv => kv.Value.weather.airTemperatureMinimum));
                pixelOutput.weather.radData.dayLength = Median(group.Select(kv => kv.Value.weather.radData.dayLength));
                pixelOutput.dormancyInduction.photoThermalDormancyInductionRate =
                    Median(group.Select(kv => kv.Value.dormancyInduction.photoThermalDormancyInductionRate));
                pixelOutput.dormancyInduction.photoThermalDormancyInductionState =
                    Median(group.Select(kv => kv.Value.dormancyInduction.photoThermalDormancyInductionState));
                pixelOutput.dormancyInductionPercentage = Median(group.Select(kv => kv.Value.dormancyInductionPercentage));
                pixelOutput.endodormancy.endodormancyRate = Median(group.Select(kv => kv.Value.endodormancy.endodormancyRate));
                pixelOutput.endodormancy.endodormancyState = Median(group.Select(kv => kv.Value.endodormancy.endodormancyState));
                pixelOutput.endodormancyPercentage = Median(group.Select(kv => kv.Value.endodormancyPercentage));
                pixelOutput.ecodormancy.ecodormancyRate = Median(group.Select(kv => kv.Value.ecodormancy.ecodormancyRate));
                pixelOutput.ecodormancy.ecodormancyState = Median(group.Select(kv => kv.Value.ecodormancy.ecodormancyState));
                pixelOutput.ecodormancyPercentage = Median(group.Select(kv => kv.Value.ecodormancyPercentage));
                pixelOutput.growth.growthRate = Median(group.Select(kv => kv.Value.growth.growthRate));
                pixelOutput.growth.growthState = Median(group.Select(kv => kv.Value.growth.growthState));
                pixelOutput.growthPercentage = Median(group.Select(kv => kv.Value.growthPercentage));
                pixelOutput.greenDown.greenDownRate = Median(group.Select(kv => kv.Value.greenDown.greenDownRate));
                pixelOutput.greenDown.greenDownState = Median(group.Select(kv => kv.Value.greenDown.greenDownState));
                pixelOutput.greenDownPercentage = Median(group.Select(kv => kv.Value.greenDownPercentage));
                pixelOutput.decline.declineRate = Median(group.Select(kv => kv.Value.decline.declineRate));
                pixelOutput.decline.declineState = Median(group.Select(kv => kv.Value.decline.declineState));
                pixelOutput.declinePercentage = Median(group.Select(kv => kv.Value.declinePercentage));
                pixelOutput.viReference = Median(group.Select(kv => kv.Value.viReference));
                pixelOutput.vi = Median(group.Select(kv => kv.Value.vi));

                // Handle NDVI separately: Store all simulations
                if (!ndviSimulations.ContainsKey(date))
                {
                    ndviSimulations.Add(date, new Dictionary<string, float>());
                    ndviSimulations[date].Add("10th", CalculatePercentile(group.Select(kv => kv.Value.vi), 10));
                    ndviSimulations[date].Add("25th", CalculatePercentile(group.Select(kv => kv.Value.vi), 25));
                    ndviSimulations[date].Add("40th", CalculatePercentile(group.Select(kv => kv.Value.vi), 40));
                    ndviSimulations[date].Add("60th", CalculatePercentile(group.Select(kv => kv.Value.vi), 60));
                    ndviSimulations[date].Add("75th", CalculatePercentile(group.Select(kv => kv.Value.vi), 75));
                    ndviSimulations[date].Add("90th", CalculatePercentile(group.Select(kv => kv.Value.vi), 90));
                }

                // Add the pixel output to the results dictionary
                datePixelMeans.Add(date, pixelOutput);
            }


            // group by DateTime
            var groupedByDate = parsetOutputs.GroupBy(kv => kv.Value);

            //write outputs in validation
            optimizer.writeOutputsValidation(pixel, datePixelMeans, ndviSimulations);
            //}
        }
        #endregion

        #endregion

        //increase the number of pixel
        currentPixelNumber++;
    }

    if (pixelsNotValidated.Count > 0)
    {
        for (int i = 0; i < pixelsNotValidated.Count; i++)
        {
            Console.WriteLine("\npixel {0} not run because group {1} is missing in the parameter structure",
                pixelsNotValidated[i].id.ToString(), pixelsNotValidated[i].ecoName.ToString());
        }
    }
}





#region compute statistics from the multiple validation runs
static float CalculatePercentile(IEnumerable<float> values, int percentile)
        {
            // Ensure the values are sorted
            var sortedValues = values.OrderBy(v => v).ToList();

            // Calculate the index of the percentile
            int index = (int)Math.Ceiling((percentile / 100.0) * sortedValues.Count) - 1;

            // Return the value at the calculated index
            return sortedValues[index];
        }

        // Median function
        static float Median(IEnumerable<float> values)
{
    List<float> sortedValues = values.OrderBy(x => x).ToList();
    int count = sortedValues.Count;

    if (count % 2 == 0)
    {
        // Even number of elements, average the middle two
        return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0F;
    }
    else
    {
        // Odd number of elements, return the middle one
        return sortedValues[count / 2];
    }
}
        #endregion
    

#region settings from json

public class root
{
    public settings? settings { get; set; }

}

//contains the parameters in the json configuration file
public class settings
{
    public object startYear { get; set; }
    public object endYear { get; set; }
    public object calibration { get; set; }
    public object simplexes { get; set; }
    public object iterations { get; set; }
    public object species { get; set; }
    public object georeferencingFile { get; set; }
    public object referenceDataFile { get; set; }
    public object parametersDataFile { get; set; }
    public object weatherDirectory { get; set; }
    public object startPixel { get; set; }
    public object numberPixels { get; set; }
    public object outputParametersDir { get; set; }
    public object validationReplicates { get; set; }
    public object parametersDistributions { get; set; }
    public object outputCalibrationDir { get; set; }
    public object outputValidationDir { get; set; }
    public object vegetationIndex { get; set; }
    public object parametersValidationFile { get; set; }
}

#endregion

