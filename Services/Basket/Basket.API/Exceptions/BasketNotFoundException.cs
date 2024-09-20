namespace Basket.API.Exceptions;

public class BasketNotFoundException(string name) 
    : NotFoundException(name);