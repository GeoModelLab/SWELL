using System.Collections.Generic;
using runner.data;
using System.IO;

namespace runner
{
    public class paramReader
    {
        public Dictionary<string, Dictionary<string, parameter>> read(string file, string species)
        {
            Dictionary<string, Dictionary<string, parameter>> species_nameParam = new Dictionary<string, Dictionary<string, parameter>>();

            if (!species_nameParam.ContainsKey(species))
            {
                species_nameParam.Add(species, new Dictionary<string, parameter>());

            }

            StreamReader sr = new StreamReader(file);
            sr.ReadLine();

            while(!sr.EndOfStream)
            {
                string[] line = sr.ReadLine().Split(',');

                
                species_nameParam[species].Add(line[1] + "_" + line[2], new parameter());
                parameter parameter = new parameter();
                parameter.value = float.Parse(line[5]);
                parameter.minimum = float.Parse(line[3]);
                parameter.maximum = float.Parse(line[4]);
                parameter.calibration = line[6];
                parameter.classParam = line[1];
                species_nameParam[species][line[1] + "_" + line[2]] = parameter;
                              
            }

            return species_nameParam;
        }
    }
}
