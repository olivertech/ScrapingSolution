namespace Services_Scraping.Services
{
    public class ServiceAlura : ServiceBase<AluraContent>, IServiceAlura
    {
        public ServiceAlura(IMongoClient? client) : base(client)
        {}
    }
}
