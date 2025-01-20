// See https://aka.ms/new-console-template for more information
using TypeFest.Net;

Console.WriteLine("Hello, World!");


public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

[Pick<Person>("Age", "Name")]
public partial class EditPerson;