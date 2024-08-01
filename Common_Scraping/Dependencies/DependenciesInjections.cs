namespace Common_Scraping.Dependencies
{
    public static class Dependencies
    {
        public static ServiceProvider Injections()
        {
            return new ServiceCollection()
                 .AddSingleton<IServiceCourse, ServiceCourse>()
                 .AddSingleton<IMongoClient, MongoClient>()
                 .BuildServiceProvider();
        }
    }
}
