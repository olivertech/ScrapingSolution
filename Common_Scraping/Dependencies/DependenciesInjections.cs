namespace Common_Scraping.Dependencies
{
    public static class Dependencies
    {
        public static ServiceProvider Injections()
        {
            return new ServiceCollection()
                 .AddSingleton<IServiceAlura, ServiceAlura>()
                 .AddSingleton<IMongoClient, MongoClient>()
                 .BuildServiceProvider();
        }
    }
}
