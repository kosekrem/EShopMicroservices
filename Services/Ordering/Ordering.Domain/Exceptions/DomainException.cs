namespace Ordering.Domain.Exceptions;

public class DomainException(string message) 
    : Exception($"Domain exception \"{message}\" throws from domain layer.");