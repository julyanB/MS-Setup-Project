namespace CoreService.Domain.Common;

public interface IRequestBase<TStatus> : IAuditable
{
    TStatus? Status { get; set; }
}
