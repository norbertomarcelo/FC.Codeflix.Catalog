using FluentAssertions;
using FC.Codeflix.Catalog.Domain.Exceptions;
using DomainEntity = FC.Codeflix.Catalog.Domain.Entity;

namespace FC.Codeflix.Catalog.UnitTests.Domain.Entity.Category;

[Collection(nameof(CategoryTestFixture))]
public class CategoryTest
{
    private readonly CategoryTestFixture _categoryFixture;

    public CategoryTest(CategoryTestFixture categoryFixture)
    {
        _categoryFixture = categoryFixture;
    }

    public static IEnumerable<object[]> GetNamesWithLessThan3Characters(int numberOfTests = 6)
    {
        var fixture = new CategoryTestFixture();
        for (int i = 0; i < numberOfTests; i++)
        {
            var isOdd = i % 2 == 1;
            yield return new object[] {
                fixture.GetValidCategoryName()[..(isOdd ? 1 : 2)]
            };
        }
    }

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "Category - Aggregates")]
    public void Instantiate()
    {
        var validData = _categoryFixture.GetValidCategory();
        var datetimeBefore = DateTime.Now;
        var category = new DomainEntity.Category(validData.Name, validData.Description);
        var datetimeAfter = DateTime.Now;

        category.Should().NotBeNull();
        category.Name.Should().Be(validData.Name);
        category.Description.Should().Be(validData.Description);
        category.Id.Should().NotBe(default(Guid));
        category.CreatedAt.Should().NotBe(default(DateTime));
        (category.CreatedAt >= datetimeBefore).Should().BeTrue();
        (category.CreatedAt <= datetimeAfter).Should().BeTrue();
        (category.IsActive).Should().BeTrue();
    }

    [Theory(DisplayName = nameof(InstantiateWithIsActive))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData(true)]
    [InlineData(false)]
    public void InstantiateWithIsActive(bool isActive)
    {
        var validData = _categoryFixture.GetValidCategory();
        var datetimeBefore = DateTime.Now;
        var category = new DomainEntity.Category(validData.Name, validData.Description, isActive);
        var datetimeAfter = DateTime.Now;

        category.Should().NotBeNull();
        category.Name.Should().Be(validData.Name);
        category.Description.Should().Be(validData.Description);
        category.Id.Should().NotBeEmpty();
        category.CreatedAt.Should().NotBeSameDateAs(default(DateTime));
        (category.CreatedAt >= datetimeBefore).Should().BeTrue();
        (category.CreatedAt <= datetimeAfter).Should().BeTrue();
        (category.IsActive).Should().Be(isActive);
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenNameIsEmpty))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void InstantiateErrorWhenNameIsEmpty(string? name)
    {
        var validDescription = _categoryFixture.GetValidCategoryDescription();

        Action action = () =>
        {
            new DomainEntity.Category(name, validDescription);
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null.");
    }

    [Fact(DisplayName = nameof(InstantiateErrorWhenDescriptionIsNull))]
    [Trait("Domain", "Category - Aggregates")]
    public void InstantiateErrorWhenDescriptionIsNull()
    {
        var validName = _categoryFixture.GetValidCategoryName();

        Action action = () =>
        {
            new DomainEntity.Category(validName, null);
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Description should not be empty or null.");
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenNameLessThan3Characters))]
    [Trait("Domain", "Category - Aggregates")]
    [MemberData(nameof(GetNamesWithLessThan3Characters), parameters: 10)]
    public void InstantiateErrorWhenNameLessThan3Characters(string? invalidName)
    {
        var validDescription = _categoryFixture.GetValidCategoryDescription();

        Action action = () =>
        {
            new DomainEntity.Category(invalidName, validDescription);
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be at least 3 characters long.");
    }

    [Fact(DisplayName = nameof(InstantiateErrorWhenNameGreaterThan255Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void InstantiateErrorWhenNameGreaterThan255Characters()
    {
        var invalidName = _categoryFixture.GetInvalidCategoryName();

        Action action = () =>
        {
            new DomainEntity.Category(invalidName, "Category description.");
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be less or equal 255 characters long.");
    }

    [Fact(DisplayName = nameof(InstantiateErrorWhenNameGreaterThan10_000Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void InstantiateErrorWhenNameGreaterThan10_000Characters()
    {
        var validName = _categoryFixture.GetValidCategoryName();
        var invalidDescription = _categoryFixture.GetInvalidCategoryDescription();

        Action action = () =>
        {
            new DomainEntity.Category(validName, invalidDescription);
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Description should be less or equal 10.000 characters long.");
    }

    [Fact(DisplayName = nameof(Activate))]
    [Trait("Domain", "Category - Aggregates")]
    public void Activate()
    {
        var validData = _categoryFixture.GetValidCategory();
        var category = new DomainEntity.Category(validData.Name, validData.Description, false);

        category.Activate();

        category.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(Deactivate))]
    [Trait("Domain", "Category - Aggregates")]
    public void Deactivate()
    {
        var validData = _categoryFixture.GetValidCategory();
        var category = new DomainEntity.Category(validData.Name, validData.Description, true);

        category.Deactivate();

        category.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "Category - Aggregates")]
    public void Update()
    {
        var category = _categoryFixture.GetValidCategory();
        var newValues = _categoryFixture.GetValidCategory();

        category.Update(newValues.Name, newValues.Description);

        category.Name.Should().Be(newValues.Name);
        category.Description.Should().Be(newValues.Description);
    }

    [Fact(DisplayName = nameof(UpdateOnlyName))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateOnlyName()
    {
        var category = _categoryFixture.GetValidCategory();
        var newName = _categoryFixture.GetValidCategoryName();
        var currentDescription = category.Description;

        category.Update(newName);

        category.Name.Should().Be(newName);
        category.Description.Should().Be(currentDescription);
    }

    [Theory(DisplayName = nameof(UpdateErrorWhenNameIsEmpty))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void UpdateErrorWhenNameIsEmpty(string? name)
    {
        var category = _categoryFixture.GetValidCategory();

        Action action = () =>
        {
            category.Update(name!);
        };

        action.Should().Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null.");
    }

    [Theory(DisplayName = nameof(UpdateErrorWhenNameLessThan3Characters))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("a")]
    [InlineData("Ca")]
    public void UpdateErrorWhenNameLessThan3Characters(string? invalidName)
    {
        var category = _categoryFixture.GetValidCategory();

        Action action = () =>
        {
            category.Update(invalidName!);
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be at least 3 characters long.");
    }

    [Fact(DisplayName = nameof(UpdateErrorWhenNameGreaterThan255Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateErrorWhenNameGreaterThan255Characters()
    {
        var category = _categoryFixture.GetValidCategory();
        var invalidName = _categoryFixture.GetInvalidCategoryName();

        Action action = () =>
        {
            category.Update(invalidName!);
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be less or equal 255 characters long.");
    }

    [Fact(DisplayName = nameof(UpdateErrorWhenNameGreaterThan10_000Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateErrorWhenNameGreaterThan10_000Characters()
    {
        var category = _categoryFixture.GetValidCategory();
        var newName = _categoryFixture.GetValidCategoryName();
        var invalidDescription = _categoryFixture.GetInvalidCategoryDescription();

        Action action = () =>
        {
            category.Update(newName, invalidDescription);
        };

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Description should be less or equal 10.000 characters long.");
    }
}
