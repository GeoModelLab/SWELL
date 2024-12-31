namespace runner.data
{
    //this class define a SWELL parameter: the corresponding class, the minimum, maximum values, and the inclusion of the parameter in the calibration subset
    public class parameter
    {
        public string classParam { get; set; }
        public float minimum  { get; set; }
        public float maximum { get; set; }
        public float value { get; set; }
        public string calibration { get; set; }
    }
}
