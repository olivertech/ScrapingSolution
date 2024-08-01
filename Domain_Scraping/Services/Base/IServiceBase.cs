namespace Domain_Scraping.Services.Base
{
    public interface IServiceBase<T>
        where T : BaseEntity
    {
        Task<long> GetCountAsync();
        IEnumerable<T> GetAllAsync();
        Task<T> GetByIdAsync(string id);
        Task AddAsync(T entity);
        void UpdateAsync(T entity);
        Task DeleteAsync(string id);
        Task<DeleteResult> DeleteAllAsync();
    }
}
