# TypeFest.Net

Ever wish C# had some of the [utility types](https://www.typescriptlang.org/docs/handbook/utility-types.html) that TypeScript has? This library is the one for you. 

## Pick

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

Under the hood, a type with the `Title` and `Completed` just like they are on `Todo`. It will also generate a method with the following signature: `public static TodoPreview From(Todo value)` for you so you can easily map from `Todo` to `TodoPreview`. 

## Omit

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

Just like [`Pick`](#pick), this will create you a type but with all the defined properties removed. It will also create you a `From` method for easily mapping between the two.