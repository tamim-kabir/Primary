using System.Text.Json;
using Xunit;

namespace Primary.Json.Test;

public class ObjectDeserializeTest
{
    [Fact]
    public void Deserialize_Object_With_Null_Value()
    {
        //Arrange
        var jdata = @"{""Id"":1,""Name"":""S"",""Description"":""H"",""CategoryId"":1,""Category"":{""Id"":1,""Name"":""C""}}";

        //Act
        var dObject = JSON.Parse<Product>(jdata);

        //Assert
        Assert.NotNull(dObject);
    }

    [Fact]
    public void Should_Throw_Exception_When_Required_Value_Not_Provided()
    {
        //Arrange
        var jdata = @"{""Id"":1,""Description"":""H"",""CategoryId"":1,""Category"":{""Id"":1,""Name"":""C""}}";
        
        //Act
        Product? Action() => JSON.Parse<Product>(jdata);

        //Assert
        Assert.Throws<JsonException>(Action);
    }


    private class Category
    {
        public int Id { get; init; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }

    private class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? ImagesUrl { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}