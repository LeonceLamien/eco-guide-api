using Bogus;
using EcoGuide.Data.Constants;
using EcoGuide.Data.Documents.Users;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EcoGuide.Data.Seed;

public class DataSeeder
{
    private IMongoDatabase Database { get; }

    private readonly List<string> ProductIds =
    [
        "0000000000949", "0000000001281", "0000000001663", "0000000001885", "0000000003827",
        "0000000004510", "0000000004794", "0000000005005", "0000000005272", "0000000005470",
        "0000000005661", "0000000007160", "0000000020114", "0000000024600", "0000000039529",
        "0000000274722", "0000000274739", "0000000290616", "0000000338219", "0000000394710"
    ];

    public DataSeeder(IMongoClient client)
    {
        Database = client.GetDatabase(Databases.EcoGuide.Name);
    }

    public void InitializeEcoGuideDb()
    {
        var collection = Database.GetCollection<UserDocument>(Databases.EcoGuide.Collections.Users);

        // Check if the collection already contains data
        if (collection.CountDocuments(FilterDefinition<UserDocument>.Empty) > 0)
        {
            return;
        }

        var fakerUser = new Faker<UserDocument>()
            .RuleFor(u => u._id, f => ObjectId.GenerateNewId(DateTime.Now).ToString())
            .RuleFor(u => u.email, f => f.Internet.Email());

        var searchFaker = new Faker<ProductSearchDocument>()
            .RuleFor(u => u.Id, f => ObjectId.GenerateNewId(DateTime.Now).ToString())
            .RuleFor(s => s.Terms, f => f.Commerce.ProductName())
            .RuleFor(s => s.Timestamp, f => f.Date.Past());

        var scanFaker = new Faker<ProductScanDocument>()
            .RuleFor(u => u.Id, f => ObjectId.GenerateNewId(DateTime.Now).ToString())
            .RuleFor(s => s.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(s => s.Timestamp, f => f.Date.Past());

        var consumptionFaker = new Faker<ProductConsumptionDocument>()
            .RuleFor(u => u.Id, f => ObjectId.GenerateNewId(DateTime.Now).ToString())
            .RuleFor(s => s.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(s => s.Timestamp, f => f.Date.Past());

        var fakeUsers = fakerUser.Generate(10);

        foreach (var user in fakeUsers)
        {
            user.Searches = searchFaker.Generate(10);
            user.Scans = scanFaker.Generate(10);
            user.Consumptions = consumptionFaker.Generate(10);
        }

        collection.InsertMany(fakeUsers);
    }
}