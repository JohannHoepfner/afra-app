using Altafraner.AfraApp.Profundum.Domain.Models;
using HotChocolate;
using HotChocolate.Data;

namespace Altafraner.AfraApp.Profundum.API.GraphQL;

/// <summary>
///     GraphQL query type for Profundum data.
/// </summary>
[QueryType]
public static class ProfundumQuery
{
    /// <summary>
    ///     Returns all Profundum definitions.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ProfundumDefinition> GetProfunda([Service] AfraAppContext dbContext)
        => dbContext.Profunda;

    /// <summary>
    ///     Returns all Profundum instances.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ProfundumInstanz> GetProfundumInstanzen([Service] AfraAppContext dbContext)
        => dbContext.ProfundaInstanzen;

    /// <summary>
    ///     Returns all Profundum categories.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ProfundumKategorie> GetProfundumKategorien([Service] AfraAppContext dbContext)
        => dbContext.ProfundaKategorien;

    /// <summary>
    ///     Returns all Profundum departments (Fachbereiche).
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ProfundumFachbereich> GetProfundumFachbereiche([Service] AfraAppContext dbContext)
        => dbContext.ProfundaFachbereiche;

    /// <summary>
    ///     Returns all Profundum slots.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ProfundumSlot> GetProfundumSlots([Service] AfraAppContext dbContext)
        => dbContext.ProfundaSlots;

    /// <summary>
    ///     Returns all Profundum enrollment timeframes (Einwahlzeiträume).
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ProfundumEinwahlZeitraum> GetProfundumEinwahlZeitraeume(
        [Service] AfraAppContext dbContext)
        => dbContext.ProfundumEinwahlZeitraeume;
}
