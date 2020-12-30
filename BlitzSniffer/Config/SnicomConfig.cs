namespace BlitzSniffer.Config
{
    public class SnicomConfig
    {
        public string IpAddress
        {
            get;
            set;
        }

        public SnicomConfig()
        {
            IpAddress = "0.0.0.0";
        }

    }
}
