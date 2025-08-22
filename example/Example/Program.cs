// See https://aka.ms/new-console-template for more information
using TypeFest.Net;

var person = new EditPerson
{
    Name = "John",
    Age = 42,
};

Console.WriteLine($"Name = {person.Name}, Age = {person.Age}");


public class Person
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
}

[Pick<Person>("Age", "Name")]
public partial class EditPerson;
