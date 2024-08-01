using Domain_Scraping.Entities.Alura;
using Domain_Scraping.Services.Alura;

namespace Services_Scraping.Services.Alura
{
    public class ServiceAlura : ServiceBase<AluraContent>, IServiceAlura
    {
        public ServiceAlura(IMongoClient? client) : base(client)
        { }
    }
}
