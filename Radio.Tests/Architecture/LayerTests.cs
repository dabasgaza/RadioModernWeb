// ============================================================
// LayerTests — طبقات المشروع
// ============================================================
// المسؤولية: تعريف طبقات المشروع.
// ============================================================
using System.Reflection;
using NetArchTest.Rules;

namespace Radio.Tests.Architecture;

/// <summary>
/// صنف طبقات المشروع.
/// </summary>
[Trait("Category", "Architecture")]
public class LayerTests
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Models.BaseEntity).Assembly;
    private static readonly Assembly DataAccessAssembly = typeof(DataAccess.Common.Result).Assembly;
    private static readonly Assembly WebAssembly = typeof(Radio.Web.Controllers.HomeController).Assembly;

    /// <summary>
    /// Domain_ Should Not Depend On_ بيانات Access.
    /// </summary>
    [Fact]
    public void Domain_ShouldNotDependOn_DataAccess()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("DataAccess")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.ToString());
    }

    /// <summary>
    /// Domain_ Should Not Depend On_ Radio Web.
    /// </summary>
    [Fact]
    public void Domain_ShouldNotDependOn_RadioWeb()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Radio.Web")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.ToString());
    }

    /// <summary>
    /// بيانات Access_ Should Not Depend On_ Radio Web.
    /// </summary>
    [Fact]
    public void DataAccess_ShouldNotDependOn_RadioWeb()
    {
        var result = Types.InAssembly(DataAccessAssembly)
            .ShouldNot()
            .HaveDependencyOn("Radio.Web")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.ToString());
    }

    /// <summary>
    /// بيانات Access_ Has Project Reference To_ Domain.
    /// </summary>
    [Fact]
    public void DataAccess_HasProjectReferenceTo_Domain()
    {
        var dataAccessAssemblyName = DataAccessAssembly.GetName().Name;
        var domainAssemblyName = DomainAssembly.GetName().Name;

        var referencedAssemblies = DataAccessAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        referencedAssemblies.Should().Contain(domainAssemblyName,
            "DataAccess project must reference Domain project");
    }

    /// <summary>
    /// Service Classes_ Should End With_ Service.
    /// </summary>
    [Fact]
    public void ServiceClasses_ShouldEndWith_Service()
    {
        var types = Types.InAssembly(DataAccessAssembly)
            .That()
            .ResideInNamespace("DataAccess.Services")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => !t.Name.Contains('<')) // exclude compiler-generated
            .ToList();

        types.Should().NotBeEmpty("there should be service classes in DataAccess.Services");

        // Known non-service types in Services namespace (DTOs, records, enums, hosted services)
        var knownNonService = new HashSet<string>
        {
            "DatabaseBackupScheduler", "ConflictInfo", "ConflictLevel",
            "SeqEventResponse", "MessageService",
            "AuditLogDto", "PagedAuditLogResult",
            "DiagnosticLogDto", "DiagnosticsSummaryDto"
        };

        var violations = types
            .Where(t => !t.Name.EndsWith("Service") && !knownNonService.Contains(t.Name))
            .ToList();

        violations.Should().BeEmpty($"all classes in DataAccess.Services should end with 'Service'. Violations: {string.Join(", ", violations.Select(v => v.Name))}");
    }

    /// <summary>
    /// Business Controllers_ Should Reference_ Service Interfaces.
    /// </summary>
    [Fact]
    public void BusinessControllers_ShouldReference_ServiceInterfaces()
    {
        var controllers = Types.InAssembly(WebAssembly)
            .That()
            .ResideInNamespace("Radio.Web.Controllers")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Controller")
            .GetTypes()
            .ToList();

        controllers.Should().NotBeEmpty("there should be controller classes");

        // AccountController uses SignInManager directly (ASP.NET Core Identity), not app services
        // DesignController is a static showcase page with no service dependency
        var businessControllers = controllers
            .Where(c => c.Name is not "AccountController" and not "DesignController"
                and not "SearchController" and not "CalendarController"
                and not "FileUploadController" and not "ProductionController"
                and not "SettingsController")
            .ToList();

        foreach (var controller in businessControllers)
        {
            var hasServiceRef = controller.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Any(p => p.ParameterType.Namespace?.StartsWith("DataAccess.Services") == true);

            hasServiceRef.Should().BeTrue(
                $"{controller.Name} should inject at least one service from DataAccess.Services");
        }
    }

    /// <summary>
    /// Interfaces_ Should Follow_ Naming Convention.
    /// </summary>
    [Fact]
    public void Interfaces_ShouldFollow_NamingConvention()
    {
        var types = Types.InAssembly(DataAccessAssembly)
            .That()
            .AreInterfaces()
            .And()
            .ResideInNamespace("DataAccess.Services")
            .GetTypes()
            .Where(t => !t.Name.Contains('<'))
            .ToList();

        types.Should().NotBeEmpty("there should be interfaces in DataAccess.Services");

        var violations = types.Where(t => !t.Name.StartsWith("I")).ToList();
        violations.Should().BeEmpty($"all interfaces in DataAccess.Services should start with 'I'. Violations: {string.Join(", ", violations.Select(v => v.Name))}");
    }

    /// <summary>
    /// الكل Services_ Should Implement_ Interface.
    /// </summary>
    [Fact]
    public void AllServices_ShouldImplement_Interface()
    {
        var serviceTypes = Types.InAssembly(DataAccessAssembly)
            .That()
            .ResideInNamespace("DataAccess.Services")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => t.Name.EndsWith("Service") && !t.Name.Contains('<'));

        var interfaceTypes = Types.InAssembly(DataAccessAssembly)
            .That()
            .ResideInNamespace("DataAccess.Services")
            .And()
            .AreInterfaces()
            .GetTypes()
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            var hasMatchingInterface = interfaceTypes.Any(i =>
                i.IsAssignableFrom(serviceType) &&
                i.Name != serviceType.Name);

            hasMatchingInterface.Should().BeTrue(
                $"{serviceType.Name} should implement a corresponding interface");
        }
    }

    /// <summary>
    /// Domain Models_ Should Not Have_ بيانات Access Dependency.
    /// </summary>
    [Fact]
    public void DomainModels_ShouldNotHave_DataAccessDependency()
    {
        var types = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("Domain.Models")
            .GetTypes()
            .ToList();

        types.Should().NotBeEmpty("there should be domain model classes");

        foreach (var type in types)
        {
            var referencedAssemblies = type.Assembly.GetReferencedAssemblies()
                .Select(a => a.Name)
                .ToList();

            referencedAssemblies.Should().NotContain("DataAccess",
                $"{type.Name} should not depend on DataAccess layer");
        }
    }
}
