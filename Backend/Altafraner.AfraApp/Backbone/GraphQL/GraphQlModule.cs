using Altafraner.AfraApp.Otium.API.GraphQL;
using Altafraner.AfraApp.Profundum.API.GraphQL;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Backbone.GraphQL;

/// <summary>
///     A module that sets up the GraphQL API using HotChocolate.
///     The GraphQL endpoint is accessible at <c>/graphql</c> and requires authentication.
/// </summary>
[DependsOn<DatabaseModule>]
public class GraphQlModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services
            .AddGraphQLServer()
            .AddTypes(typeof(OtiumQuery), typeof(ProfundumQuery))
            .AddProjections()
            .AddFiltering()
            .AddSorting();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapGraphQL()
            .RequireAuthorization();
    }
}
