using Altafraner.AfraApp.Otium.Domain.Models;
using HotChocolate;
using HotChocolate.Data;

namespace Altafraner.AfraApp.Otium.API.GraphQL;

/// <summary>
///     GraphQL query type for Otium data.
/// </summary>
[QueryType]
public static class OtiumQuery
{
    /// <summary>
    ///     Returns all Otium categories.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<OtiumKategorie> GetKategorien([Service] AfraAppContext dbContext)
        => dbContext.OtiaKategorien;

    /// <summary>
    ///     Returns all Otium definitions.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<OtiumDefinition> GetOtia([Service] AfraAppContext dbContext)
        => dbContext.Otia;
}
