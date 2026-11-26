using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

public class WebApiController: ControllerBase
{
    protected ActionResult Problem(Error error)
    {
        var type = error.Type;
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        return Problem(
            detail: error.Description,
            statusCode: statusCode
        );
    }
}
