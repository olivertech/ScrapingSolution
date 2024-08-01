namespace Services_Scraping.Services
{
    public class ServiceCourse : ServiceBase<Course>, IServiceCourse
    {
        public ServiceCourse(IMongoClient? client) : base(client)
        {}
    }
}
