# Value Objects

This folder contains immutable "value objects" that represent domain concepts without identity in the Mango Bay Cargo system. They're not really value objects the way Martin Fowler [uses the term](https://martinfowler.com/bliki/ValueObject.html). These are more like [branded types](https://egghead.io/blog/using-branded-types-in-typescript) that are popular in the javascript world: it's a primitive reserved for a specific use. 

## Key Advantages

-   **Self-Validating**: Encapsulates validation logic at creation time, eliminating repetitive validation code like `if(rating < 0 || rating > 5) throw new InvalidArgumentException();`
-   **Self-Documenting**: Makes method signatures clearer - `ProcessRating(Rating rating)` vs `ProcessRating(float rating)` - improving code readability
-   **Fewer Bugs**: Compiler catches type mismatches, preventing errors like passing a username where an email is expected

## Security Benefits

Correct behavior reduces exploitable vulnerabilities that stem from defects. By eliminating entire classes of bugs through type safety and encapsulated validation, value objects contribute to more secure applications.

## Value Objects

-   **[Vogen Library](https://stevedunn.github.io/Vogen)**: Uses compile-time source generation for efficient value object creation
-   **Validation at Creation**: All value objects validate input during construction
-   **Type Safety**: Strong typing prevents [primitive](https://refactoring.guru/smells/primitive-obsession) [obsession](https://wiki.c2.com/?
    PrimitiveObsession) and improves code clarity
-   **Minimal Runtime Cost**: Source generator produces efficient code with no runtime dependencies radiating outward
-   **Immutability**: Value objects are immutable, preventing accidental state changes that could lead to security issues

## Examples

-   `Rating` - Represents a star rating (1-5) with built-in validation
-   Future value objects may include `Money`, `EmailAddress`, `PhoneNumber`, etc.

## Usage

Value objects replace primitive types where domain rules and validation are important, making the code more expressive and preventing invalid state.

