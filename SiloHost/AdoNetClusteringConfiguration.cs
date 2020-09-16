namespace SiloHost
{
    public class AdoNetClusteringConfiguration
    {
        public const string AdoNetClustering = "AdoNetClustering";

        public string ClusterId { get; set; }
        public string ServiceId { get; set; }
        public string Invariant { get; set; }
        public string ConnectionString { get; set; }
    }
}