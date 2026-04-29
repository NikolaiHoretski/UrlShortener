namespace UrlShortener.Api.Services;

public interface IRedirectCountService {

    Task IncrementByIdAsync(Guid id, CancellationToken cancellationToken);
}
