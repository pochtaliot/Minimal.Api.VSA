namespace Minimal.Api.Shared.Contracts;

public class ErrorResponse
{
    public ErrorResponse(IEnumerable<string> errors) => Errors = errors;
    public ErrorResponse(string error) => Errors = [error];
    public IEnumerable<string> Errors { get; set; }
}
