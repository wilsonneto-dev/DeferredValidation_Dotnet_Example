
Console.WriteLine("Deferred validation example:");

// validating without a always valid domain
var p1 = new Person(0, 0, null);
var notification = new NotificationValidationHandler();
p1.Validate(notification);

if (notification.HasErrors())
    Console.WriteLine("Validation of p1 failed: {0}", string.Join(',', notification.GetErrors().Select(x => x.Message).ToList()));

/// still without an always valid domain
var p2 = new Person(0, 0, null);
var exceptionHandler = new ExceptionValidationHandler();
try
{
    p2.Validate(exceptionHandler);
}
catch (Exception ex)
{
    Console.WriteLine("Exception on the person 2 validation: {0}", ex.Message);    
}

Console.WriteLine("Deferred validation example with self contained validation");

// lets see now in an always valid domain, the validate is within the class
try
{
    var c = new Car(0, null);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

// ----------------------------------------------------------------------------------

// example of deffered validation without self validation
public class Person
{
    public int Id { get; private set; }
    public int Age { get; private set; }
    public string? Name { get; private set; }

    public Person(int id, int age, string? name)
    {
        Id = id;
        Age = age;
        Name = name;
    }

    public void Validate(ValidationHandler validationHandler) => (new PersonValidator(this, validationHandler)).Validate();
}

// example of deffered validation with self validation
public class Car
{
    public int Id { get; private set; }
    public string? Model { get; private set; }

    public Car(int id, string? model)
    {
        Id = id;
        Model = model;
        
        Validate();
    }

    private void Validate()
    {
        var notification = new NotificationValidationHandler();
        var personValidator = new CarValidator(this, notification);
        personValidator.Validate();
        if (!notification.HasErrors()) 
            return;
        
        var errors = string.Join(',', notification.GetErrors().Select(x => x.Message).ToList());
        throw new Exception($"Validation error: {errors}");
    }
}

public class CarValidator : Validator
{
    private readonly Car _car;
    
    public CarValidator(Car car, ValidationHandler handler) : base(handler) => _car = car;

    public override void Validate()
    {
        if (_car.Id <= 0)
            _handler.HandleError(new Error("Id must be greater than 0"));

        if (string.IsNullOrEmpty(_car.Model))
            _handler.HandleError(new Error("Model is required"));
    }
}

public class PersonValidator : Validator
{
    private readonly Person _person;

    public PersonValidator(Person person, ValidationHandler handler) 
        : base(handler) =>
        _person = person;

    public override void Validate()
    {
        if (_person.Id <= 0)
            _handler.HandleError(new Error("Id must be greater than 0"));
        
        if (string.IsNullOrEmpty(_person.Name))
            _handler.HandleError(new Error("Name is required"));

        if (_person.Age <= 0)
            _handler.HandleError(new Error("Age must be greater than 0"));
    }
}

public record Error(string Message);

public class ExceptionValidationHandler : ValidationHandler
{
    public override void HandleError(Error error) => throw new Exception(error.Message);
}

public class NotificationValidationHandler : ValidationHandler
{
    private readonly List<Error> _errors;

    public NotificationValidationHandler() => _errors = new();

    public IReadOnlyCollection<Error> GetErrors() => _errors.AsReadOnly();

    public bool HasErrors() => GetErrors().Count > 0;

    public override void HandleError(Error error) => _errors.Add(error);
}

public abstract class ValidationHandler
{
    public abstract void HandleError(Error error);
}

public abstract class Validator
{
    protected ValidationHandler _handler;

    protected Validator(ValidationHandler handler) => _handler = handler;

    public abstract void Validate();
}
