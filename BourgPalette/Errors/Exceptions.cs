namespace BourgPalette.Errors;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class BadRequestAppException : Exception
{
    public BadRequestAppException(string message) : base(message) { }
}
