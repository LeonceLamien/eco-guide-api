using Mongo2Go;
using MongoDB.Driver;

namespace EcoGuide.Api.Integration.Tests;

public class MongoDbFixture : IDisposable
{
    public MongoDbRunner Runner { get; }
    public MongoClient Client { get; }

    public MongoDbFixture()
    {
        Runner = MongoDbRunner.Start();
        Client = new MongoClient(Runner.ConnectionString);
    }

    public void Dispose()
    {
        Runner.Dispose();
    }
}