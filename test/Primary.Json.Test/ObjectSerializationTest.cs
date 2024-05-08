using Xunit;

namespace Primary.Json.Test;
public class ObjectSerializationTest
{
    [Fact]
    public void Serialize_Object_Without_Null_Values()
    {
        //Arrange
        var category = new Category
        {
            Id = 1,
            Name = "C",
            
        };
        var product = new Product
        {
            Id = 1,
            Name = "S",
            Description = "H",
            ImagesUrl = "F",
            CategoryId = category.Id,
            Category = category
        };

        //Act
        var jData = JSON.Stringify(product);

        //Assert
        var expect = @"{""Id"":1,""Name"":""S"",""Description"":""H"",""ImagesUrl"":""F"",""CategoryId"":1,""Category"":{""Id"":1,""Name"":""C""}}";
        Assert.NotNull(jData);
        Assert.Contains(expect, jData);
    }
    [Fact]
    public void Serialize_Object_When_Write_Null_Values()
    {
        //Arrange
        var category = new Category
        {
            Id = 1,
            Name = "C"
        };
        var product = new Product
        {
            Id = 1,
            Name = "S",
            Description = "H",
            ImagesUrl = "F",
            CategoryId = category.Id,
            Category = category
        };

        //Act
        var jData = JSON.Stringify(product, true);

        //Assert
        var expect = @"{""Id"":1,""Name"":""S"",""Description"":""H"",""ImagesUrl"":""F"",""CategoryId"":1,""Category"":{""Id"":1,""Name"":""C"",""Description"":null}}";
        Assert.NotNull(jData);
        Assert.Contains(expect, jData);
    }
    private class Category
    {
        public int Id { get; init; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImagesUrl { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
