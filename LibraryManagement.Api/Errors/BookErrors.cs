using ErrorOr;

namespace LibraryManagement.Api.Errors;

public static class BookErrors
{
    public static Error CategoryNotFound(int id) => Error.Validation(
        description: $"Category with id {id} does not exist."
    );

    public static Error AuthorNotFound() => Error.Validation(
        description: "One or more of the provided author ids is not valid."
    );
}
