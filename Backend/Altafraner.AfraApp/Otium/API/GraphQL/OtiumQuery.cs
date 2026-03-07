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
    public static IQueryable<OtiumKategorie> GetOtiumKategorien([Service] AfraAppContext dbContext)
        => dbContext.OtiaKategorien;

    /// <summary>
    ///     Returns all Otium definitions.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<OtiumDefinition> GetOtia([Service] AfraAppContext dbContext)
        => dbContext.Otia;

    /// <summary>
    ///     Returns all Otium appointments (Termine).
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<OtiumTermin> GetOtiumTermine([Service] AfraAppContext dbContext)
        => dbContext.OtiaTermine;

    /// <summary>
    ///     Returns all Otium recurrence rules (Wiederholungen).
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<OtiumWiederholung> GetOtiumWiederholungen([Service] AfraAppContext dbContext)
        => dbContext.OtiaWiederholungen;
}
