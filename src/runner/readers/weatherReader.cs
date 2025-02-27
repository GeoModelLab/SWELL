using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using source.data;

namespace runner
{
    public class weatherReader
    {     
        public Dictionary<DateTime, input> readWeather(string fileName)
        {
            Dictionary<DateTime, input> date_input = new Dictionary<DateTime, input>();
            //StreamReader streamReader = new StreamReader(fileName);
            StreamReader streamReader = new StreamReader("C:\\Users\\simoneugomaria.brega\\Dropbox\\data\\e-obs\\data//42.9498605531828_-2.95013959721108.csv");

            float latitude = 0;
            ///get latitude
            //for (int i = 0; i < 13; i++)
            //{
            //    if(i==3)
            //    {
            //        string[] line = streamReader.ReadLine().Split(' ');
            //        latitude = float.Parse(line[3]);
            //    }
            //    streamReader.ReadLine();
            //}
            streamReader.ReadLine();

           
            while (!streamReader.EndOfStream)
            {
                string[] line = streamReader.ReadLine().Split(',');
                input input = new input();


                if (line[4] != "NA")
                {
                    #region read weather data
                    string rawDate = line[2].Trim('"');
                    DateTime date = Convert.ToDateTime(rawDate);
                    input.date = date;
                    //input.precipitation = (float)Convert.ToDouble(line[5]);
                    //if (input.precipitation < 0)
                    //{
                    //input.precipitation = 0;
                    // }
                    input.airTemperatureMaximum = (float)Convert.ToDouble(line[4]);
                    if (line[3] != "NA")
                    {
                        input.airTemperatureMinimum = (float)Convert.ToDouble(line[3]);
                    }
                    else
                    {
                        input.airTemperatureMinimum = input.airTemperatureMaximum - 10;
                    }
                    //TODO check
                    input.latitude = (float)Convert.ToDouble(line[0]);
                    date_input.Add(date, input);
                    #endregion
                }
            }
            streamReader.Close();

           
            return date_input;

        }
    }
}
