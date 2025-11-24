var builder = DistributedApplication.CreateBuilder(args);

var orderInventoryDb = builder.AddPostgres("order-inventory-db")
    .WithDataVolume()
    .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "trust")
    .AddDatabase("orderInventoryDb");

var bookCatalogDb = builder.AddMongoDB("book-catalog-db")
    .WithDataVolume()
    .AddDatabase("bookCatalogDb");

var cartWishlistDb = builder.AddPostgres("card-wishlist-db")
    .WithDataVolume()
    .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "trust")
    .AddDatabase("cartWishlistDb");

var orderInventoryApi = builder.AddProject<Projects.OrderAndInventory_API>("order-inventory-api")
    .WithReference(orderInventoryDb)
    .WithHttpEndpoint(port: 5001, name: "order-inventory-http")
    .WaitFor(orderInventoryDb);

var bookCatalogApi = builder.AddProject<Projects.BookCatalog_API>("book-catalog-api")
    .WithReference(bookCatalogDb)
    .WithHttpEndpoint(port: 5002, name: "book-catalog-http")
    .WaitFor(bookCatalogDb);


var cartWishlistApi = builder.AddProject<Projects.CartAndWishlist_API>("cart-wishlist-api")
    .WithReference(cartWishlistDb)
    .WithHttpEndpoint(port: 5003, name: "cart-wishlist-http")
    .WaitFor(cartWishlistDb);

var aggregator = builder.AddProject<Projects.Aggregator>("aggregator")
    .WithReference(orderInventoryApi)
    .WithReference(bookCatalogApi)
    .WithReference(cartWishlistApi)
    .WithHttpEndpoint(port: 5004, name: "aggregator-http");

builder.Build().Run();