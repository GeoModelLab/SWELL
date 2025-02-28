using runner.data;

namespace runner
{
    //this class reads the NDVI reference data 
    internal class referenceReader
    {
        //this method reads the NDVI data from the .csv file
        internal Dictionary<string, ID> read(string file)
        {
            //create the dictionary
            Dictionary<string, ID> idPixel = new Dictionary<string, ID>();

            //open the stream
            StreamReader sr = new StreamReader(file);
            //read the first line
            sr.ReadLine();

            //loop over lines
            while (!sr.EndOfStream)
            {
                //read the line and split
                string[] line = sr.ReadLine().Split(',', '"');
                //assign the pixel
                string pixel = line[3];

                //if the dictionary does not contain the pixel, add it
                if (!idPixel.ContainsKey(pixel))
                {
                    idPixel.Add(pixel, new ID());
                    idPixel[pixel].id = line[3];
                    idPixel[pixel].ecoName = line[4];
                    idPixel[pixel].cluster = int.Parse(line[13]);
                    idPixel[pixel].latitude = float.Parse(line[33]);
                }
                int year = int.Parse(line[8]);
                //
                DateTime date = new DateTime(year, 1, 1).AddDays(int.Parse(line[9]));              
            }

            sr.Close();

            return idPixel;
        }

        public Dictionary<string, ID> readReferenceData(string file)
        {
            Dictionary<string, ID> idPixel = new Dictionary<string, ID>();
            //open a stream
            StreamReader sr = new StreamReader(file);
            sr.ReadLine();

            while (!sr.EndOfStream)
            {
                string[] line = sr.ReadLine().Split(',', '"');
                string pixel = line[0];

                if (line.Length == 7)
                {
                    if (!idPixel.ContainsKey(pixel))
                    {

                        idPixel.Add(pixel, new ID());
                        idPixel[pixel].longitude = float.Parse(line[4]);
                        idPixel[pixel].latitude = float.Parse(line[5]);
                        idPixel[pixel].ecoName=line[1];
                    }
                    int year = int.Parse(line[2]);

                    if (line[6] != "NA")
                    {
                        DateTime date = new DateTime(year, 1, 1).AddDays(int.Parse(line[3]));
                        if (!idPixel[pixel].dateVInorm.ContainsKey(date))
                        {                           
                            idPixel[pixel].dateVInorm.Add(date, float.Parse(line[6]));
                        }
                    }
                }
            }
            //close the file
            sr.Close();

            return idPixel;
        }
    }
}
