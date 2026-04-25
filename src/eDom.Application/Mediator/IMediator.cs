namespace eDom.Application.Mediator;

public interface IMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
