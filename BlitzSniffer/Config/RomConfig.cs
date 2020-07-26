namespace BlitzSniffer.Config
{
    public class RomConfig
    {
        public string ProdKeys
        {
            get;
            set;
        }

        public string TitleKeys
        {
            get;
            set;
        }

        public string BaseNca
        {
            get;
            set;
        }

        public string UpdateNca
        {
            get;
            set;
        }

        public RomConfig()
        {
            ProdKeys = "prod.keys";
            TitleKeys = "title.keys";
            BaseNca = "base.nca";
            UpdateNca = "update.nca";
        }

    }

}
