namespace reviewservice.Services
{
    public interface IOrderServiceClient
    {
        Task<bool> ValidateOrderAndProduct(Guid userId, Guid orderId, Guid productId);
    }
}
//da bi napisali recenziju moramo prvo da porucimo proizvod