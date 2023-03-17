namespace Sales.WEB.Repositories
{
    public interface IRepository
    {
        Task<HttpResponseWrapper<T>> Get<T>(string url);

        Task<HttpResponseWrapper<object>> Get(string url);

        Task<HttpResponseWrapper<object>> Post<T>(string url, T model);

        Task<HttpResponseWrapper<TResponse>> Post<T, TResponse>(string url, T model);

        Task<HttpResponseWrapper<object>> Delete(string url);

        Task<HttpResponseWrapper<object>> Put<T>(string url, T model);

        Task<HttpResponseWrapper<TResponse>> Put<T, TResponse>(string url, T model);
    }

}
