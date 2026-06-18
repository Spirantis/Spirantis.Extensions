namespace Spirantis.Extensions.SystemTests;

/// <summary>Tests for the <c>IEnumerable&lt;Type&gt;.Derives</c> extension declared in <c>TypeExtensions</c>.</summary>
[Trait("Category", "Unit")]
public sealed class TypeExtensionsTests
{
    private abstract class Shape;

    private sealed class Circle : Shape;

    private class Rectangle : Shape;

    private sealed class Square : Rectangle;

    private sealed class User;

    private class Repository<T>;

    private class UserRepository : Repository<User>;

    private sealed class AuditedUserRepository : UserRepository;

    private static readonly Type[] ShapeTypes =
    [
        typeof(Shape),
        typeof(Circle),
        typeof(Rectangle),
        typeof(Square),
    ];

    private static readonly Type[] RepositoryTypes =
    [
        typeof(Repository<>),
        typeof(User),
        typeof(UserRepository),
        typeof(AuditedUserRepository),
    ];

    [Fact]
    public void Derives_FromNonGenericBase_ReturnsAllDescendants()
    {
        var result = ShapeTypes.Derives(typeof(Shape)).ToList();

        Assert.Equal(
            new HashSet<Type> { typeof(Circle), typeof(Rectangle), typeof(Square) },
            result.ToHashSet()
        );
        Assert.DoesNotContain(typeof(Shape), result);
    }

    [Fact]
    public void Derives_FromNonGenericBase_ReturnsDistinctTypes()
    {
        var result = ShapeTypes.Derives(typeof(Shape)).ToList();

        Assert.Equal(result.Count, result.Distinct().Count());
    }

    [Fact]
    public void Derives_FromGenericBase_WalksTheInheritanceChain()
    {
        var result = RepositoryTypes.Derives(typeof(Repository<>)).ToList();

        Assert.Equal(
            new HashSet<Type> { typeof(UserRepository), typeof(AuditedUserRepository) },
            result.ToHashSet()
        );
    }

    [Fact]
    public void Derives_WithNullBaseType_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ShapeTypes.Derives(null!).ToList());
    }
}
