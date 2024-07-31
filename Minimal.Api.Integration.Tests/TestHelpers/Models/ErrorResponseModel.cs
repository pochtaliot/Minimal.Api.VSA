namespace Minimal.Api.Integration.Tests.TestHelpers.Models;
public class ErrorResponseModel
{
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
