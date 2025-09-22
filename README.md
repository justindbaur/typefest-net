# TypeFest.Net

Ever wish C# had some of the [utility types](https://www.typescriptlang.org/docs/handbook/utility-types.html) that TypeScript has? This library is the one for you. 

## Key Features

### Pick

Just like you've come to love from TypeScript, this attribute allows you to pick properties off of another type to create another one. If the type of one of the properties in the source type changes so will they in the target type. 

Example:

```c#
namespace Test;

public class Todo
{
    public string Title { get; set; }
    public int Description { get; set; }
    public bool Completed { get; set; }
}

[Pick<Todo>("Title", "Completed")]
public partial class TodoPreview;
```

Under the hood, a type with the `Title` and `Completed` just like they are on `Todo`.

### Omit

```c#
namespace Test;

public class Todo
{
    public string Title { get; set; }
    public int Description { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; }
}

[Omit<Todo>("Description")]
public partial class TodoPreview;
```

Just like [`Pick`](#pick), this will create you a type but with all the defined properties removed.

## Pitfalls

Because this library makes use of [C# source generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) that means that the properties added to the type aren't accessible to _other_ C# source generators. This is mainly a problem if you want to use these types with [JSON source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation) or [configuration source generator](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-generator). The properties won't be visible to them and they will not work they way you want. The best fix for this is some sort of framework to ensure some source generators work before others. But no such framework exists today, if you have ideas for one, please create an issue with it and link back to [this issue](https://github.com/dotnet/roslyn/issues/57239) and feel free to use this library as an example.

