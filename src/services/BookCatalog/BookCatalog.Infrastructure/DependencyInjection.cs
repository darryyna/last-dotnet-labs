using System.Reflection;
using BookCatalog.Application.Contracts.Repositories;
using BookCatalog.Infrastructure.Database;
using BookCatalog.Infrastructure.Options;
using BookCatalog.Infrastructure.Repositories;
using BookCatalog.Infrastructure.Repositories.CachedRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderAndInventory.Grpc;
using Shared.CQRS.PipelineBehaviours;

namespace BookCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddOptions<MongoOptions>()
            .BindConfiguration(MongoOptions.ConfigurationKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        //services.AddScoped<IMongoClient>(sp =>
        //{
         //   var options = sp.GetRequiredService<IOptions<MongoOptions>>();
         //   
        //    return new MongoClient(options.Value.ConnectionString);
        //});
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("redis");
        });

        services.AddScoped<BookCatalogDbContext>();

        services.AddScoped<BookRepository>();
        services.AddScoped<GenreRepository>();
        services.AddScoped<PublisherRepository>();
        services.AddScoped<ReviewRepository>();

        services.AddScoped<IBookRepository>(sp =>
        {
            var repo = sp.GetRequiredService<BookRepository>();
            var cache = sp.GetRequiredService<IDistributedCache>();
            return new CachedBookRepository(repo, cache);
        });

        services.AddScoped<IGenreRepository>(sp =>
        {
            var repo = sp.GetRequiredService<GenreRepository>();
            var cache = sp.GetRequiredService<IDistributedCache>();
            return new CachedGenreRepository(repo, cache);
        });

        services.AddScoped<IPublisherRepository>(sp =>
        {
            var repo = sp.GetRequiredService<PublisherRepository>();
            var cache = sp.GetRequiredService<IDistributedCache>();
            return new CachedPublisherRepository(repo, cache);
        });

        services.AddScoped<IReviewRepository>(sp =>
        {
            var repo = sp.GetRequiredService<ReviewRepository>();
            var cache = sp.GetRequiredService<IDistributedCache>();
            return new CachedReviewRepository(repo, cache);
        });
        
        services.AddGrpcClient<MemberGRPCService.MemberGRPCServiceClient>(options =>
        {
            options.Address = new Uri("https://localhost:15600");
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();

            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return handler;
        });
        services.AddMediatR(options =>
        {
            options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        return services;
    }
}