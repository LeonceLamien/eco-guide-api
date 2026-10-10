namespace EcoGuide.Data.Constants;

/// <summary>
/// Constants for databases & collections names.
/// </summary>
public static class Databases
{
    public class OpenFoodFact
    {
        public const string Name = "off";

        public class Collections
        {
            public const string Products = "products";
        }
    }

    public class EcoGuide
    {
        public const string Name = "ecoguide";

        public class Collections
        {
            public const string Users = "users";
        }
    }
}