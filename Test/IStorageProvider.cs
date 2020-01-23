using System.Threading.Tasks;

namespace Test
{
    public interface IStorageProvider<in TRequest, TResponse>
    {
        Task<TResponse> SaveAsync(TRequest blob);
    }
}