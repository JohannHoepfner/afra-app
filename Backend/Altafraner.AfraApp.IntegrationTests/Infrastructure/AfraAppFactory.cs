using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Altafraner.AfraApp.IntegrationTests.Infrastructure;

public class AfraAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("afra_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private string _certPath = null!;
    private string _keyPath = null!;

    // Well-known test user IDs
    public static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid TutorUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid StudentUserId = Guid.Parse("00000000-0000-0000-0000-000000000003");

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        GenerateTempCertificate();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        CleanupTempCerts();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["Certificates:DataProtectionCert"] = _certPath,
                ["Certificates:DataProtectionKey"] = _keyPath,
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    /// <summary>Creates an HttpClient authenticated as the given user.</summary>
    public HttpClient CreateClientAs(Guid userId, Rolle rolle, GlobalPermission[]? permissions = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, rolle.ToString());
        if (permissions != null)
            foreach (var perm in permissions)
                client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionHeader, perm.ToString());
        return client;
    }

    /// <summary>Creates an HttpClient authenticated as the admin user.</summary>
    public HttpClient CreateAdminClient() =>
        CreateClientAs(AdminUserId, Rolle.Tutor, [GlobalPermission.Admin, GlobalPermission.Otiumsverantwortlich]);

    /// <summary>Creates an HttpClient authenticated as the tutor user.</summary>
    public HttpClient CreateTutorClient() =>
        CreateClientAs(TutorUserId, Rolle.Tutor);

    /// <summary>Creates an HttpClient authenticated as the student user.</summary>
    public HttpClient CreateStudentClient() =>
        CreateClientAs(StudentUserId, Rolle.Oberstufe);

    /// <summary>Seeds the test database after the factory is started.</summary>
    public async Task SeedDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AfraAppContext>();

        // Idempotent: skip if already seeded
        if (await dbContext.Personen.AnyAsync())
            return;

        var admin = new Person
        {
            Id = AdminUserId,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            Rolle = Rolle.Tutor,
            GlobalPermissions = [GlobalPermission.Admin, GlobalPermission.Otiumsverantwortlich]
        };
        var tutor = new Person
        {
            Id = TutorUserId,
            FirstName = "Test",
            LastName = "Tutor",
            Email = "tutor@example.com",
            Rolle = Rolle.Tutor
        };
        var student = new Person
        {
            Id = StudentUserId,
            FirstName = "Test",
            LastName = "Student",
            Email = "student@example.com",
            Rolle = Rolle.Oberstufe
        };

        dbContext.Personen.AddRange(admin, tutor, student);

        var nextMonday = GetNextWeekday(DayOfWeek.Monday);
        var schultag = new Schultag
        {
            Datum = nextMonday,
            Wochentyp = Wochentyp.N
        };
        var block1 = new Block { SchemaId = '1', Schultag = schultag };
        var block2 = new Block { SchemaId = '2', Schultag = schultag };

        dbContext.Schultage.Add(schultag);
        dbContext.Blocks.AddRange(block1, block2);

        var kategorie = new OtiumKategorie
        {
            Bezeichnung = "Testbewegung",
            Icon = "pi pi-heart",
            CssColor = "var(--p-teal-500)"
        };
        dbContext.OtiaKategorien.Add(kategorie);

        var otium = new OtiumDefinition
        {
            Bezeichnung = "Test Otium",
            Beschreibung = "An otium for testing",
            Kategorie = kategorie,
            Verantwortliche = [tutor]
        };
        dbContext.Otia.Add(otium);

        var termin = new OtiumTermin
        {
            Otium = otium,
            Tutor = tutor,
            Block = block1,
            Ort = "101",
            IstAbgesagt = false
        };
        dbContext.OtiaTermine.Add(termin);

        await dbContext.SaveChangesAsync();
    }

    private static DateOnly GetNextWeekday(DayOfWeek day)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysUntil = ((int)day - (int)today.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;
        return today.AddDays(daysUntil);
    }

    private void GenerateTempCertificate()
    {
        using var rsa = RSA.Create(2048);
        var certRequest = new CertificateRequest(
            "CN=AfraAppTest",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var cert = certRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));

        _certPath = Path.Combine(Path.GetTempPath(), $"afra_test_{Guid.NewGuid():N}.pem");
        _keyPath = Path.Combine(Path.GetTempPath(), $"afra_test_{Guid.NewGuid():N}.key");

        File.WriteAllText(_certPath, cert.ExportCertificatePem());
        File.WriteAllText(_keyPath, rsa.ExportRsaPrivateKeyPem());
    }

    private void CleanupTempCerts()
    {
        if (_certPath != null && File.Exists(_certPath)) File.Delete(_certPath);
        if (_keyPath != null && File.Exists(_keyPath)) File.Delete(_keyPath);
    }
}
